#region Copyright (c) 2005 Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
#endregion

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Jayrock.Json.Conversion;
    using NUnit.Framework;

    #if !NET_1_0 && !NET_1_1

    using System.Collections.Generic;

    #endif

    #if !NET_1_0 && !NET_1_1 && !NET_2_0

    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.CSharp.RuntimeBinder;

    #endif

    #endregion

    [ TestFixture ]
    public class TestJsonObject
    {
        [ Test ]
        public void Names()
        {
            var o = new JsonObject();
            o.Put("one", 1);
            o.Put("two", 2);
            var names = o.Names;
            var e = names.GetEnumerator();
            e.MoveNext(); Assert.AreEqual("one", e.Current);
            e.MoveNext(); Assert.AreEqual("two", e.Current);
        }

        [ Test ]
        public void InitWithMembers()
        {
            var o = new JsonObject(new[]
            {
                new JsonMember("one", 1),
                new JsonMember("two", 2),
            });
            Assert.AreEqual(2, o.Count);
            Assert.AreEqual(1, o["one"]);
            Assert.AreEqual(2, o["two"]);
        }

        [ Test ]
        public void InitWithMembersAccumulates()
        {
            var o = new JsonObject(new[]
            {
                new JsonMember("one"  , 1),
                new JsonMember("two"  , 2),
                new JsonMember("three", 3),
                new JsonMember("two"  , 4),
            });
            Assert.AreEqual(3, o.Count);
            Assert.AreEqual(1, o["one"]);
            var two = o["two"] as IList;
            Assert.IsNotNull(two, "Member 'two' should be an ordered list of accumulated values.");
            Assert.AreEqual(2, two.Count, "Count of values under 'two'.");
            Assert.AreEqual(2, two[0]);
            Assert.AreEqual(4, two[1]);
            Assert.AreEqual(3, o["three"]);
        }

        [ Test ]
        public void CannotInitWithDefaultMember()
        {
            var e = Assert.Throws<ArgumentException>(() =>
                new JsonObject(new JsonMember[1]));
            Assert.AreEqual("members", e.ParamName);
        }

        [ Test ]
        public void Import()
        {
            var article = new JsonObject();

            article.Import(new JsonTextReader(new StringReader(@"
                /* Article */ {
                    Title : 'Introduction to JSON',
                    Rating : 2,
                    Abstract : null,
                    Author : {
                        Name : 'John Doe',
                        'E-Mail Address' : 'john.doe@example.com'
                    },
                    References : [
                        { Title : 'JSON RPC', Link : 'http://www.json-rpc.org/' }
                    ]
                }")));

            Assert.IsNotNull(article);
            Assert.AreEqual(5, article.Count);
            Assert.AreEqual("Introduction to JSON", article["Title"]);
            Assert.AreEqual(2, (int) (JsonNumber) article["Rating"]);
            Assert.AreEqual(null, article["Abstract"]);

            var author = (IDictionary) article["Author"];
            Assert.IsNotNull(author);
            Assert.AreEqual(2, author.Count);
            Assert.AreEqual("John Doe", author["Name"]);
            Assert.AreEqual("john.doe@example.com", author["E-Mail Address"]);

            var references = (JsonArray) article["References"];
            Assert.IsNotNull(references);
            Assert.AreEqual(1, references.Length);

            var reference = (IDictionary) references[0];
            Assert.IsNotNull(reference);
            Assert.AreEqual(2, reference.Count);
            Assert.AreEqual("JSON RPC", reference["Title"]);
            Assert.AreEqual("http://www.json-rpc.org/", reference["Link"]);
        }

        [ Test ]
        public void ContentsClearedBeforeImporting()
        {
            var o = new JsonObject();
            o.Put("foo", "bar");
            Assert.AreEqual(1, o.Count);
            o.Import(new JsonTextReader(new StringReader("{}")));
            Assert.AreEqual(0, o.Count);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotUseNullReaderWithImport()
        {
            IJsonImportable o = new JsonObject();
            o.Import(new ImportContext(), null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotUseNullContextWithImport()
        {
            IJsonImportable o = new JsonObject();
            o.Import(null, (new JsonRecorder()).CreatePlayer());
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotUseNullArgWithExport()
        {
            IJsonExportable o = new JsonObject();
            o.Export(null, null);
        }

        [ Test ]
        public void Export()
        {
            var o = new JsonObject();
            o.Put("Number", 123);
            o.Put("String", "Hello World");
            o.Put("Boolean", true);
            var writer = new JsonRecorder();
            o.Export(writer);
            var reader = writer.CreatePlayer();
            reader.ReadToken(JsonTokenClass.Object);
            var members = o.GetNamesArray().Cast<string>().ToArray();
            Assert.AreEqual(members[0], reader.ReadMember());
            Assert.AreEqual(o[members[0]], reader.ReadNumber().ToInt32());
            Assert.AreEqual(members[1], reader.ReadMember());
            Assert.AreEqual(o[members[1]], reader.ReadString());
            Assert.AreEqual(members[2], reader.ReadMember());
            Assert.AreEqual(o[members[2]], reader.ReadBoolean());
            Assert.AreEqual(JsonTokenClass.EndObject, reader.TokenClass);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotPutUsingNullName()
        {
            (new JsonObject()).Put(null, new object());
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotAddUsingNullName()
        {
            (new JsonObject()).Add(null, new object());
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddUsingMemberWithNullName()
        {
            (new JsonObject()).Add(default);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotRemoveByNullName()
        {
            (new JsonObject()).Remove(null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotTestContainmentForNullName()
        {
            (new JsonObject()).Contains(null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotListNamesToNullList()
        {
            (new JsonObject()).ListNames(null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotExportToNullWriter()
        {
            IJsonExportable obj = new JsonObject();
            obj.Export(new ExportContext(), null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotAccumulateUsingNullName()
        {
            (new JsonObject()).Accumulate(null, new object());
        }

        [ Test ]
        public void NoHasMemberWhenEmpty()
        {
            Assert.IsFalse((new JsonObject()).HasMembers);
        }

        [ Test ]
        public void HasMemberWhenNotEmpty()
        {
            var o = new JsonObject();
            Assert.IsFalse(o.HasMembers);
            o.Put("foo", "bar");
            Assert.IsTrue(o.HasMembers);
        }

        [ Test ]
        public void Add()
        {
            var o = new JsonObject { ["foo"] = "bar" };
            Assert.AreEqual("bar", o["foo"]);
            var names = o.Names;
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual(new[] { "foo" }, names);
        }

        [ Test ]
        [ ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddMemberNameDuplicates()
        {
            var _ = new JsonObject
            {
                { "foo", "bar" },
                { "foo", "baz" },
            };
        }

        [ Test ]
        public void Enumeration()
        {
            var o = new JsonObject
            {
                ["one"  ] = 1,
                ["two"  ] = 2,
                ["three"] = 3,
                ["four" ] = 4
            };

            var e = o.GetEnumerator();

            Assert.IsNotNull(e);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("one", e.Current.Name);
            Assert.AreEqual(1, e.Current.Value);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("two", e.Current.Name);
            Assert.AreEqual(2, e.Current.Value);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("three", e.Current.Name);
            Assert.AreEqual(3, e.Current.Value);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("four", e.Current.Name);
            Assert.AreEqual(4, e.Current.Value);

            Assert.IsFalse(e.MoveNext());
        }

        [ Test ]
        public void DynamicallyGetMember()
        {
            var obj = new JsonObject { { "foo", "bar" } };
            Assert.AreEqual("bar", ((dynamic) obj).foo);
        }

        [Test, Ignore("To be completed")]
        public void DynamicallyGetMemberIgnoringCase()
        {
            var obj = new JsonObject { { "foo", "bar" } };
            var dmo = ((IDynamicMetaObjectProvider)obj).GetMetaObject(Expression.Parameter(typeof(JsonObject)));
            var s0 = CallSite<Func<CallSite, DynamicMetaObject, object>>.Create(new FakeGetMemberBinder("FOO", true));
            Assert.AreEqual("bar", s0.Target(s0, dmo));
        }

        class FakeGetMemberBinder : GetMemberBinder
        {
            public FakeGetMemberBinder(string name, bool ignoreCase) :
                base(name, ignoreCase) {}

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        [ Test, ExpectedException(typeof(RuntimeBinderException)) ]
        public void CannotDynamicallyGetNonExistingMember()
        {
            var obj = new JsonObject();
            var unused = ((dynamic) obj).foo;
        }

        [ Test ]
        public void DynamicallySetMember()
        {
            var obj = new JsonObject();
            Assert.IsFalse(obj.HasMembers);
            ((dynamic)obj).foo = "bar";
            Assert.IsTrue(obj.HasMembers);
            Assert.AreEqual("foo", obj.GetNamesArray()[0]);
            Assert.AreEqual("bar", obj["foo"]);
        }

        [ Test ]
        public void DynamicallyGetMemberNames()
        {
            var obj = new JsonObject {{"x", 123}, {"y", 456}};
            var dmo = ((IDynamicMetaObjectProvider) obj).GetMetaObject(Expression.Parameter(typeof(object)));
            using (var e = dmo.GetDynamicMemberNames().GetEnumerator())
            {
                Assert.IsTrue(e.MoveNext()); Assert.AreEqual("x", e.Current);
                Assert.IsTrue(e.MoveNext()); Assert.AreEqual("y", e.Current);
                Assert.IsFalse(e.MoveNext());
            }
        }

        // TODO Add test for dynamically deleting member

        [ Test ]
        public void AddViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            Assert.AreEqual(0, obj.Count);
            obj.Add("one", 1);
            Assert.AreEqual(1, obj.Count);
            obj.Add("two", 2);
            Assert.AreEqual(2, obj.Count);
        }

        [ Test ]
        public void AddViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            Assert.AreEqual(0, obj.Count);
            obj.Add(new KeyValuePair<string, object>("one", 1));
            Assert.AreEqual(1, obj.Count);
            obj.Add(new KeyValuePair<string, object>("two", 2));
            Assert.AreEqual(2, obj.Count);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddDuplicateKeyViaGenericCollection()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("foo", "bar"));
            obj.Add(new KeyValuePair<string, object>("foo", "baz"));
        }

        [ Test ]
        public void KeyContainmentViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            Assert.IsFalse(obj.ContainsKey("foo"));
            obj.Add(new KeyValuePair<string, object>("foo", "bar"));
            Assert.IsTrue(obj.ContainsKey("foo"));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotTestKeyContainmentViaGenericDictionaryWithNullKey()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.ContainsKey(null);
        }

        [ Test ]
        public void KeysViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.Add("first", 123);
            Assert.AreEqual(new[] { "first" }, obj.Keys);
            obj.Add("second", 456);
            Assert.AreEqual(new[] { "first", "second" }, obj.Keys);
        }

        [ Test ]
        public void ValuesViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.Add("first", 123);
            Assert.AreEqual(new object[] { 123 }, obj.Values);
            obj.Add("second", 456);
            Assert.AreEqual(new object[] { 123, 456 }, obj.Values);
        }

        [ Test ]
        public void RemoveExistingViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.Add("first", 123);
            Assert.AreEqual(1, obj.Count);
            Assert.IsTrue(obj.Remove("first"));
            Assert.AreEqual(0, obj.Count);
        }

        [ Test ]
        public void RemoveNonExistingViaGenericDictionary()
        {
            IDictionary<string, object> obj = new JsonObject();
            obj.Add("first", 123);
            Assert.AreEqual(1, obj.Count);
            Assert.IsFalse(obj.Remove("second"));
            Assert.AreEqual(1, obj.Count);
        }

        [ Test ]
        public void RemoveExistingViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.AreEqual(1, obj.Count);
            Assert.IsTrue(obj.Remove(new KeyValuePair<string, object>("first", 123)));
            Assert.AreEqual(0, obj.Count);
        }

        [ Test ]
        public void RemoveNonExistingViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.AreEqual(1, obj.Count);
            Assert.IsFalse(obj.Remove(new KeyValuePair<string, object>("second", 456)));
            Assert.AreEqual(1, obj.Count);
        }

        [ Test ]
        public void RemoveExistingKeyNonMatchingValueViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.AreEqual(1, obj.Count);
            Assert.IsFalse(obj.Remove(new KeyValuePair<string, object>("first", 456)));
            Assert.AreEqual(1, obj.Count);
        }

        [ Test ]
        public void RemoveNonExistingKeyMatchingValueViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.AreEqual(1, obj.Count);
            Assert.IsFalse(obj.Remove(new KeyValuePair<string, object>("second", 123)));
            Assert.AreEqual(1, obj.Count);
        }

        [ Test ]
        public void ContainsExistingViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.IsTrue(obj.Contains(new KeyValuePair<string, object>("first", 123)));
        }

        [ Test ]
        public void ContainsNonExistingViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.IsFalse(obj.Contains(new KeyValuePair<string, object>("second", 456)));
        }

        [ Test ]
        public void ContainsExistingKeyNonMatchingValueViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.IsFalse(obj.Contains(new KeyValuePair<string, object>("first", 456)));
        }

        [ Test ]
        public void ContainsNonExistingKeyMatchingValueViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("first", 123));
            Assert.IsFalse(obj.Contains(new KeyValuePair<string, object>("second", 123)));
        }

        [ Test ]
        public void IsReadOnlyViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            Assert.IsFalse(obj.IsReadOnly);
        }

        [ Test ]
        public void CopyToViaGenericCollection()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            var first = new KeyValuePair<string, object>("first", 123);
            obj.Add(first);
            var second = new KeyValuePair<string, object>("second", 456);
            obj.Add(second);
            var pairs = new KeyValuePair<string, object>[2];
            obj.CopyTo(pairs, 0);
            Assert.AreEqual(first, pairs[0]);
            Assert.AreEqual(second, pairs[1]);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCopyToViaGenericCollectionWithNullArray()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.CopyTo(null, 0);
        }

        [ Test, ExpectedException(typeof(ArgumentOutOfRangeException)) ]
        public void CannotCopyToViaGenericCollectionWithNegativeArrayIndex()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.CopyTo(new KeyValuePair<string, object>[0], -1);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotCopyToViaGenericCollectionWithTooSmallArray()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("foo", "bar"));
            obj.CopyTo(new KeyValuePair<string, object>[1], 1);
        }

        [ Test ]
        public void KeyValuePairEnumeration()
        {
            ICollection<KeyValuePair<string, object>> obj = new JsonObject();

            var first = new KeyValuePair<string, object>("first", 123);
            obj.Add(first);

            var second = new KeyValuePair<string, object>("second", 456);
            obj.Add(second);

            using (var e = obj.GetEnumerator())
            {
                Assert.IsNotNull(e);
                Assert.IsTrue(e.MoveNext());
                Assert.AreEqual(first, e.Current);
                Assert.IsTrue(e.MoveNext());
                Assert.AreEqual(second, e.Current);
                Assert.IsFalse(e.MoveNext());
            }
        }

        [ Test ]
        public void JsonMemberEnumeratorUsesSameEnumeratorAsGetEnumerator()
        {
            var obj = new JsonObject();
            Assert.AreEqual(obj.GetEnumerator().GetType(), ((IEnumerable<JsonMember>) obj).GetEnumerator().GetType());
        }

        [ Test ]
        public void NonGenericEnumeratorUsesSameEnumeratorAsGetEnumerator()
        {
            var obj = new JsonObject();
            Assert.AreEqual(obj.GetEnumerator().GetType(), ((IEnumerable) obj).GetEnumerator().GetType());
        }

        [ Test ]
        public void InitWithJsonMemberSequence()
        {
            var obj = new JsonObject(
                from i in Enumerable.Range(0, 10)
                let ch = (char)('a' + i)
                select new JsonMember(ch.ToString(), i));

            Assert.AreEqual(10, obj.Count);
            Assert.AreEqual(0, obj["a"]);
            Assert.AreEqual(1, obj["b"]);
            Assert.AreEqual(2, obj["c"]);
            Assert.AreEqual(3, obj["d"]);
            Assert.AreEqual(4, obj["e"]);
            Assert.AreEqual(5, obj["f"]);
            Assert.AreEqual(6, obj["g"]);
            Assert.AreEqual(7, obj["h"]);
            Assert.AreEqual(8, obj["i"]);
            Assert.AreEqual(9, obj["j"]);
        }
    }
}

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

namespace Jayrock.Collections
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestKeyedCollection
    {
        [ Test ]
        public void AddValue()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            values.Add(value);
            Assert.AreEqual(1, values.Count);
            Assert.AreSame(value, values[value.Name]);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddWithNullKey()
        {
            var values = new NamedValueCollection();
            values.Add(new NamedValue(null, new object()));
        }

        [ Test ]
        public void Put()
        {
            var values = new NamedValueCollection();
            values.Put(new NamedValue("Foo", new object()));
            var value = new NamedValue("Foo", new object());
            values.Put(value);
            Assert.AreEqual(1, values.Count);
            Assert.AreSame(value, values[value.Name]);
        }

        [ Test ]
        public void ValueContainment()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            Assert.IsFalse(values.Contains(value.Name));
            values.Add(value);
            Assert.IsTrue(values.Contains(value.Name));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotUseNullKeyForContainmentTest()
        {
            var values = new NamedValueCollection();
            values.Contains(null);
        }

        [ Test ]
        public void RemoveValue()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            values.Add(value);
            Assert.AreEqual(1, values.Count);
            Assert.IsTrue(values.Remove(value.Name));
            Assert.IsFalse(values.Contains(value.Name));
            Assert.AreEqual(0, values.Count);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotRemoveNullKey()
        {
            var values = new NamedValueCollection();
            values.Remove(null);
        }

        [ Test ]
        public void RemoveByIndex()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            values.Add(value);
            Assert.IsTrue(values.Contains(value.Name));
            Assert.AreEqual(1, values.Count);
            values.RemoveAt(0);
            Assert.IsFalse(values.Contains(value.Name));
            Assert.AreEqual(0, values.Count);
        }

        [ Test ]
        public void ResetValueByIndex()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            values.Add(value);
            Assert.IsTrue(values.Contains(value.Name));
            Assert.AreEqual(1, values.Count);
            var newValue = new NamedValue("Bar", new object());
            ((IList) values)[0] = newValue;
            Assert.AreEqual(1, values.Count);
            Assert.IsFalse(values.Contains(value.Name));
            Assert.IsTrue(values.Contains(newValue.Name));
        }

        [ Test ]
        public void Clear()
        {
            var values = new NamedValueCollection();
            var value = new NamedValue("Foo", new object());
            values.Add(value);
            Assert.AreEqual(1, values.Count);
            Assert.IsTrue(values.Contains(value.Name));
            values.Clear();
            Assert.AreEqual(0, values.Count);
            Assert.IsFalse(values.Contains(value.Name));
        }

        [ Test ]
        public void GetKeys()
        {
            var values = new NamedValueCollection();
            var names = new[] { "one", "two", "three" };
            foreach (var name in names)
                values.Add(new NamedValue(name, new object()));
            Assert.AreEqual(names.Length, values.Count);
            var keys = values.NamesByIndex.ToArray();
            Assert.AreEqual(names, keys);
        }

        [ Test ]
        public void Enumeration()
        {
            var values = new NamedValueCollection();
            var value1 = new NamedValue("one", new object()); values.Add(value1);
            var value2 = new NamedValue("two", new object()); values.Add(value2);
            var value3 = new NamedValue("three", new object()); values.Add(value3);
            Assert.AreEqual(3, values.Count);
            IEnumerator e = values.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreSame(value1, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreSame(value2, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreSame(value3, e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [ Test ]
        public void RemoveNonExistingKey()
        {
            var values = new NamedValueCollection();
            Assert.IsFalse(values.Remove("something"));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotIndexByNullKey()
        {
            /* object unused = */ new NamedValueCollection()[null] /* [1] */ .ToString();

            //
            // [1] The ToString call is unnecessary but it is added here
            //     instead of taking the indexed value into an unused
            //     variable to avoid the CS0219 warning issue from Mono's
            //     C# compiler. See:
            //     http://bugzilla.novell.com/show_bug.cgi?id=316137
        }

        [ Serializable ]
        sealed class NamedValue
        {
            public string Name;
            public object Value;

            public NamedValue(string name, object value)
            {
                Name = name;
                Value = value;
            }
        }

        [ Serializable ]
        sealed class NamedValueCollection : KeyedCollection<string, NamedValue>
        {
            protected override string GetKeyForItem(NamedValue item) =>
                item.Name;

            public IEnumerable<string> NamesByIndex =>
                KeysByIndex;
        }
    }
}

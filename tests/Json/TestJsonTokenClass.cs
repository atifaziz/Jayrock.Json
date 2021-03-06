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
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonTokenClass
    {
        [ Test ]
        public void Names()
        {
            Assert.AreEqual("Null", JsonTokenClass.Null.Name);
            Assert.AreEqual("Array", JsonTokenClass.Array.Name);
            Assert.AreEqual("String", JsonTokenClass.String.Name);
            Assert.AreEqual("Boolean", JsonTokenClass.Boolean.Name);
            Assert.AreEqual("Number", JsonTokenClass.Number.Name);
            Assert.AreEqual("Object", JsonTokenClass.Object.Name);
        }

        [ Test ]
        public void ToStringReturnsName()
        {
            foreach (var tokenClass in JsonTokenClass.All)
                Assert.IsTrue(tokenClass.Name.Equals(tokenClass.ToString()));
        }

        [ Test ]
        public void HashCodeFromName()
        {
            foreach (var tokenClass in JsonTokenClass.All)
                Assert.IsTrue(tokenClass.GetHashCode().Equals(tokenClass.Name.GetHashCode()));
        }

        [ Test ]
        public void AllIsReadOnly()
        {
            Assert.IsTrue(((IList) JsonTokenClass.All).IsReadOnly);
        }

        [ Test ]
        public void AllIsFixedSize()
        {
            Assert.IsTrue(((IList) JsonTokenClass.All).IsFixedSize);
        }

        [ Test ]
        public void All()
        {
            var list = new ArrayList();

            foreach (var field in typeof(JsonTokenClass).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (typeof(JsonTokenClass).IsAssignableFrom(field.FieldType))
                    list.Add(field.GetValue(null));
            }

            Assert.AreEqual(list.Count, JsonTokenClass.All.Count);

            foreach (var tokenClass in JsonTokenClass.All)
                list.Remove(tokenClass);

            if (list.Count > 0)
                Assert.Fail("{0} not found in All collection.", list[0]);
        }

        [ Test ]
        public void NullToken()
        {
            Assert.AreEqual(JsonTokenClass.Null, JsonToken.Null().Class);
        }

        [ Test ]
        public void StringToken()
        {
            var token = JsonToken.String("hello");
            Assert.AreEqual(JsonTokenClass.String, token.Class);
            Assert.AreEqual("hello", token.Text);
        }

        [ Test ]
        public void StringTokenNeverNull()
        {
            Assert.IsNotNull(JsonToken.String(null).Text);
        }

        [ Test ]
        public void NumberToken()
        {
            var token = JsonToken.Number("123");
            Assert.AreEqual(JsonTokenClass.Number, token.Class);
            Assert.AreEqual("123", token.Text);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NumberTokenTextCannotBeNull()
        {
            JsonToken.Number(null);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void NumberTokenTextCannotBeZeroLength()
        {
            JsonToken.Number("");
        }

        [ Test ]
        public void BooleanToken()
        {
            var token = JsonToken.Boolean(true);
            Assert.AreEqual(JsonTokenClass.Boolean, token.Class);
            Assert.AreEqual("true", token.Text);

            token = JsonToken.Boolean(false);
            Assert.AreEqual(JsonTokenClass.Boolean, token.Class);
            Assert.AreEqual("false", token.Text);
        }

        [ Test ]
        public void ObjectToken()
        {
            Assert.AreEqual(JsonTokenClass.Object, JsonToken.Object().Class);
            Assert.AreEqual(JsonTokenClass.EndObject, JsonToken.EndObject().Class);
        }

        [ Test ]
        public void ArrayToken()
        {
            Assert.AreEqual(JsonTokenClass.Array, JsonToken.Array().Class);
            Assert.AreEqual(JsonTokenClass.EndArray, JsonToken.EndArray().Class);
        }

        [ Test ]
        public void BOFEOF()
        {
            Assert.AreEqual(JsonTokenClass.BOF, JsonToken.BOF().Class);
            Assert.AreEqual(JsonTokenClass.EOF, JsonToken.EOF().Class);
        }

        [ Test ]
        public void DeserializesToFactoryInstance()
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, JsonTokenClass.Null);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreSame(JsonTokenClass.Null, formatter.Deserialize(stream));
        }

        [ Test ]
        public void EqualityByReference()
        {
            foreach (var clazz in JsonTokenClass.All)
                Assert.IsTrue(clazz.Equals(clazz), clazz.ToString());
        }

        [ Test ]
        public void InequalityWithNull()
        {
            foreach (var clazz in JsonTokenClass.All)
                Assert.IsFalse(clazz.Equals(null), clazz.ToString());
        }

        [ Test ]
        public void InequalityWithIncompatibleType()
        {
            foreach (var clazz in JsonTokenClass.All)
                Assert.IsFalse(clazz.Equals(new object()), clazz.ToString());
        }
    }
}

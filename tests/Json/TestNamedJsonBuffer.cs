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
    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class TestNamedJsonBuffer
    {
        [ Test ]
        public void IsSerializable()
        {
            Assert.IsTrue(typeof(NamedJsonBuffer).IsSerializable);
        }

        [ Test ]
        public void EmptyHasNullName()
        {
            Assert.IsNull(NamedJsonBuffer.Empty.Name);
        }

        [Test]
        public void EmptyHasEmptyBuffer()
        {
            Assert.IsTrue(NamedJsonBuffer.Empty.IsEmpty);
        }

        [Test]
        public void EmptyIsEmpty()
        {
            Assert.IsTrue(NamedJsonBuffer.Empty.IsEmpty);
        }

        [Test]
        public void EmptyToString()
        {
            var str = NamedJsonBuffer.Empty.ToString();
            Assert.IsNotNull(str);
            Assert.AreEqual(0, str.Length);
        }

        [Test,ExpectedException(typeof(ArgumentNullException))]
        public void CannotBeInitializedWithNullName()
        {
            new NamedJsonBuffer(null, JsonBuffer.From("null"));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotBeInitializedWithEmptyBuffer()
        {
            new NamedJsonBuffer("foo", JsonBuffer.Empty);
        }

        [Test]
        public void Initialzation()
        {
            var nb = new NamedJsonBuffer("foo", JsonBuffer.From("bar"));
            Assert.AreEqual("foo", nb.Name);
            Assert.AreEqual("bar", nb.Buffer.GetString());
        }

        [Test]
        public void Equality()
        {
            var nb1 = new NamedJsonBuffer("foo", JsonBuffer.From("bar"));
            var nb2 = new NamedJsonBuffer("foo", JsonBuffer.From("bar"));
            Assert.AreEqual(nb1, nb2);
        }

        [Test]
        public void NameInequality()
        {
            var nb1 = new NamedJsonBuffer("foo", JsonBuffer.From("bar"));
            var nb2 = new NamedJsonBuffer("FOO", JsonBuffer.From("bar"));
            Assert.AreNotEqual(nb1, nb2);
        }

        [Test]
        public void BufferInequality()
        {
            var nb1 = new NamedJsonBuffer("foo", JsonBuffer.From("bar"));
            var nb2 = new NamedJsonBuffer("foo", JsonBuffer.From("123"));
            Assert.AreNotEqual(nb1, nb2);
        }

        [Test]
        public void AnonymousToString()
        {
            Assert.AreEqual("(anonymous): \"foo\"",
                new NamedJsonBuffer(string.Empty, JsonBuffer.From("foo")).ToString());
        }

        [Test]
        public void ToObject()
        {
            var obj = NamedJsonBuffer.ToObject(
                new NamedJsonBuffer("foo", JsonBuffer.From("bar")),
                new NamedJsonBuffer("qux", JsonBuffer.From("quux")));
            Assert.AreEqual(6, obj.Length);
            Assert.AreEqual(JsonToken.Object(), obj[0]);
            Assert.AreEqual(JsonToken.Member("foo"), obj[1]);
            Assert.AreEqual(JsonToken.String("bar"), obj[2]);
            Assert.AreEqual(JsonToken.Member("qux"), obj[3]);
            Assert.AreEqual(JsonToken.String("quux"), obj[4]);
            Assert.AreEqual(JsonToken.EndObject(), obj[5]);
        }
    }
}

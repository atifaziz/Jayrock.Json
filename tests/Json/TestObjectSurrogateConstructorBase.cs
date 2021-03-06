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
    using Conversion;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestObjectSurrogateConstructorBase
    {
        [ Test ]
        public void ImplementsIObjectSurrogateConstructor()
        {
            Assert.IsInstanceOf<IObjectSurrogateConstructor>(new Surrogate(new object()));
        }

        [Test]
        public void ImplementsINonObjectMemberImporter()
        {
            Assert.IsInstanceOf<INonObjectMemberImporter>(new Surrogate(new object()));
        }

        [Test]
        public void ImportNonObjectMember()
        {
            var surrogate = new Surrogate(new object());
            var context = new ImportContext();
            surrogate.Import(context, "foo", JsonText.CreateReader("bar"));
            var tail = surrogate.CreateObject(context).TailReader;
            tail.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("foo", tail.ReadMember());
            Assert.AreEqual("bar", tail.ReadString());
            tail.ReadToken(JsonTokenClass.EndObject);
        }

        [Test]
        public void CreateObject()
        {
            var obj = new object();
            var surrogate = new Surrogate(obj);
            var context = new ImportContext();
            var result = surrogate.CreateObject(context);
            Assert.AreSame(obj, result.Object);
            var tail = result.TailReader;
            tail.ReadToken(JsonTokenClass.Object);
            tail.ReadToken(JsonTokenClass.EndObject);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CannotImportNonObjectMemberWithNullContext()
        {
            var surrogate = new Surrogate(new object());
            surrogate.Import(null, "foo", StockJsonBuffers.EmptyObject.CreateReader());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CannotImportNonObjectMemberWithNullReader()
        {
            var surrogate = new Surrogate(new object());
            surrogate.Import(new ImportContext(), "foo", null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateObjectWithNullContext()
        {
            new Surrogate(new object()).CreateObject(null);
        }

        class Surrogate : ObjectSurrogateConstructorBase
        {
            public object _obj;

            public Surrogate(object obj)
            {
                _obj = obj;
            }

            public override object OnCreateObject(ImportContext context)
            {
                return _obj;
            }
        }
    }
}

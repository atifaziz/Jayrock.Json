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
    using System;
    using Conversion;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestJsonMember
    {
        [ Test ]
        public void Default()
        {
            var member = new JsonMember();
            Assert.IsNull(member.Name);
            Assert.IsNull(member.Value);
            Assert.IsEmpty(member.ToString());
        }

        [ Test ]
        public void CannotInitWithNullName()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                new JsonMember(null, null));
            Assert.AreEqual("name", e.ParamName);
        }

        [ Test ]
        public void NameWithNullValue()
        {
            var member = new JsonMember("foo", null);
            Assert.AreEqual("foo", member.Name);
            Assert.IsNull(member.Value);
            Assert.AreEqual("foo: ", member.ToString());
        }

        [ Test ]
        public void NameWithNonNullValue()
        {
            var member = new JsonMember("foo", "bar");
            Assert.AreEqual("foo", member.Name);
            Assert.AreEqual("bar", member.Value);
            Assert.AreEqual("foo: bar", member.ToString());
        }

        [ Test ]
        public void NameValueExported()
        {
            Assert.AreEqual(@"{""name"":""foo"",""value"":""bar""}",
                JsonConvert.ExportToString(new JsonMember("foo", "bar")));
        }
    }
}

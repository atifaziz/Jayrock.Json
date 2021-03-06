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

namespace Jayrock.Json.Conversion.Converters
{
    #region Imports

    using System.Collections.Specialized;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestNameValueCollectionExporter
    {
        [ Test ]
        public void Empty()
        {
            Assert.AreEqual("{}", Export(new NameValueCollection()));
        }

        [ Test ]
        public void OneNameValue()
        {
            var collection = new NameValueCollection { ["foo"] = "bar" };
            Assert.AreEqual("{\"foo\":\"bar\"}", Export(collection));
        }

        [ Test ]
        public void EmptyName()
        {
            var collection = new NameValueCollection { [string.Empty] = "bar" };
            Assert.AreEqual("{\"\":\"bar\"}", Export(collection));
        }

        [ Test ]
        public void EmptyValue()
        {
            var collection = new NameValueCollection { ["foo"] = string.Empty };
            Assert.AreEqual("{\"foo\":\"\"}", Export(collection));
        }

        [ Test ]
        public void NullValue()
        {
            var collection = new NameValueCollection { ["foo"] = null };
            Assert.AreEqual("{\"foo\":null}", Export(collection));
        }

        [ Test ]
        public void ValuesArray()
        {
            var collection = new NameValueCollection
            {
                { "foo", "bar1" },
                { "foo", "bar2" },
                { "foo", "bar3" }
            };
            Assert.AreEqual("{\"foo\":[\"bar1\",\"bar2\",\"bar3\"]}", Export(collection));
        }

        [ Test ]
        public void ManyEntries()
        {
            var collection = new NameValueCollection
            {
                ["foo1"] = "bar1",
                ["foo2"] = "bar2",
                ["foo3"] = "bar3"
            };
            Assert.AreEqual("{\"foo1\":\"bar1\",\"foo2\":\"bar2\",\"foo3\":\"bar3\"}", Export(collection));
        }

        static string Export(object o)
        {
            var writer = new JsonTextWriter();
            JsonConvert.Export(o, writer);
            return writer.ToString();
        }
    }
}

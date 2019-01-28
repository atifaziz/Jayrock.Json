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
    using System.IO;
    using Jayrock.Json.Conversion;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestNameValueCollectionImporter
    {
        [ Test ]
        public void ImportNull()
        {
            Assert.IsNull(UncheckedImport("null"));
        }

        [ Test ]
        public void ImportEmpty()
        {
            var collection = Import("{}");
            Assert.AreEqual(0, collection.Count);
        }

        [ Test ]
        public void ImportOneNameValue()
        {
            var collection = Import("{\"foo\":\"bar\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("bar", collection["foo"]);
        }

        [ Test ]
        public void ImportEmptyName()
        {
            var collection = Import("{\"\":\"bar\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("bar", collection[""]);
        }

        [ Test ]
        public void ImportEmptyValue()
        {
            var collection = Import("{\"foo\":\"\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("", collection["foo"]);
        }

        [ Test ]
        public void ImportNullValue()
        {
            var collection = Import("{\"foo\":null}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("foo", collection.Keys[0]);
            Assert.AreEqual(null, collection.Get(0));
        }

        [ Test ]
        public void ImportValuesArray()
        {
            var collection = Import("{\"foo\":[\"bar1\",\"bar2\",\"bar3\"]}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(new string[] { "bar1", "bar2", "bar3" }, collection.GetValues("foo"));
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportObjectValue()
        {
            Import("{\"foo\":{}}");
        }

        [ Test ]
        public void PositionAfterImport()
        {
            JsonReader reader = new JsonTextReader(new StringReader("[{},'end']"));
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreSame(JsonTokenClass.Object, reader.TokenClass);
            IImporter importer = new NameValueCollectionImporter();
            importer.Import(new ImportContext(), reader);
            Assert.AreEqual("end", reader.ReadString());
        }

        static NameValueCollection UncheckedImport(string s)
        {
            JsonReader reader = new JsonTextReader(new StringReader(s));
            IImporter importer = new NameValueCollectionImporter();
            var import = (NameValueCollection) importer.Import(new ImportContext(), reader);
            Assert.IsTrue(reader.EOF, "Reader must be at EOF.");
            return import;
        }

        static NameValueCollection Import(string s)
        {
            object o = UncheckedImport(s);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf<NameValueCollection>(o);
            return (NameValueCollection) o;
        }
    }
}

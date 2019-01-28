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

    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestTypeImporterBase
    {
        [ Test ]
        public void OutputTypeInitialization()
        {
            var importer = new TestImporter();
            Assert.AreSame(typeof(object), importer.OutputType);
        }

        [ Test ]
        public void NullHandling()
        {
            var reader = CreateReader("null");
            var importer = new TestImporter();
            Assert.IsNull(importer.Import(new ImportContext(), reader));
            Assert.IsTrue(reader.EOF);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportNumber()
        {
            Import("42");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportBoolean()
        {
            Import("true");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportString()
        {
            Import("'string'");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportObject()
        {
            Import("{}");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportArray()
        {
            Import("[]");
        }

        [ Test ]
        public void NumberCallsImportNumber()
        {
            var reader = CreateReader("42");
            var importer = new ImporterMock();
            const int result = 42;
            importer.Number = result;
            Assert.AreEqual(result, importer.Import(new ImportContext(), reader));
        }

        [ Test ]
        public void StringCallsImportString()
        {
            var reader = CreateReader("''");
            var importer = new ImporterMock();
            const string result = "hello";
            importer.String = result;
            Assert.AreEqual(result, importer.Import(new ImportContext(), reader));
        }

        [ Test ]
        public void BooleanCallsImportBoolean()
        {
            var reader = CreateReader("true");
            var importer = new ImporterMock();
            importer.Boolean = true;
            Assert.AreEqual(true, importer.Import(new ImportContext(), reader));
        }

        [ Test ]
        public void ArrayCallsImportArray()
        {
            var reader = CreateReader("[]");
            var importer = new ImporterMock();
            var result = new object();
            importer.Array = result;
            Assert.AreEqual(result, importer.Import(new ImportContext(), reader));
        }

        [ Test ]
        public void ObjectCallsImportObject()
        {
            var reader = CreateReader("{}");
            var importer = new ImporterMock();
            var result = new object();
            importer.Object = result;
            Assert.AreEqual(result, importer.Import(new ImportContext(), reader));
        }

        static void Import(string s)
        {
            (new TestImporter()).Import(new ImportContext(), CreateReader(s));
        }

        static JsonReader CreateReader(string s)
        {
            return new JsonTextReader(new StringReader(s));
        }

        class TestImporter : ImporterBase
        {
            public TestImporter() :
                base(typeof(object)) {}
        }

        class ImporterMock : TestImporter
        {
            public object Boolean;
            public object Number;
            public object String;
            public object Object;
            public object Array;

            protected override object ImportFromBoolean(ImportContext context, JsonReader reader)
            {
                Assert.IsNotNull(context);
                Assert.IsNotNull(reader);

                return Boolean;
            }

            protected override object ImportFromNumber(ImportContext context, JsonReader reader)
            {
                Assert.IsNotNull(context);
                Assert.IsNotNull(reader);

                return Number;
            }

            protected override object ImportFromString(ImportContext context, JsonReader reader)
            {
                Assert.IsNotNull(context);
                Assert.IsNotNull(reader);

                return String;
            }

            protected override object ImportFromArray(ImportContext context, JsonReader reader)
            {
                Assert.IsNotNull(context);
                Assert.IsNotNull(reader);

                return Array;
            }

            protected override object ImportFromObject(ImportContext context, JsonReader reader)
            {
                Assert.IsNotNull(context);
                Assert.IsNotNull(reader);

                return Object;
            }
        }
    }
}

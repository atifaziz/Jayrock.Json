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
    public class TestBooleanImporter
    {
        [ Test ]
        public void ImportNull()
        {
            Assert.IsNull(Import("null"));
        }

        [ Test ]
        public void ImportTrue()
        {
            AssertImport(true, "true");
        }

        [ Test ]
        public void ImportFalse()
        {
            AssertImport(false, "false");
        }

        [ Test ]
        public void ImportNonZeroNumber()
        {
            AssertImport(true, "123");
        }

        [ Test ]
        public void ImportZeroNumber()
        {
            AssertImport(false, "0");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportFractionalNumbers()
        {
            Import("0.5");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportString()
        {
            Import("'true'");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportArray()
        {
            Import("[]");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportObject()
        {
            Import("{}");
        }

        static void AssertImport(bool expected, string input)
        {
            var o = Import(input);
            Assert.IsInstanceOf<bool>(o);
            Assert.AreEqual(expected, o);
        }

        static object Import(string input)
        {
            var reader = new JsonTextReader(new StringReader(input));
            var importer = new BooleanImporter();
            var o = importer.Import(new ImportContext(), reader);
            Assert.IsTrue(reader.EOF, "Reader must be at EOF.");
            return o;
        }
    }
}

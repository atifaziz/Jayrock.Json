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
    using System;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestValueTupleImporter
    {
        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotInitializeWithNonTupleType()
        {
            new ValueTupleImporter(typeof(object));
        }

        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ImporterBase>(new ValueTupleImporter(typeof(ValueTuple<object>)));
        }

        [ Test ]
        public void InputTypeIsTuple()
        {
            Assert.AreSame(typeof(ValueTuple<object>), new ValueTupleImporter(typeof(ValueTuple<object>)).OutputType);
        }

        [ Test ]
        public void ImportTuple1FromNumber()
        {
            AssertImport(ValueTuple.Create(42), "42");
        }

        [ Test ]
        public void ImportTuple1FromString()
        {
            AssertImport(ValueTuple.Create("foo"), "foo");
        }

        [ Test ]
        public void ImportTuple1FromBoolean()
        {
            AssertImport(ValueTuple.Create(true), "true");
        }

        [ Test ]
        public void ImportTuple1FromNull()
        {
            var importer = new ValueTupleImporter(typeof(ValueTuple<int>));
            var result = importer.Import(JsonConvert.CreateImportContext(), JsonText.CreateReader("null"));
            Assert.IsNull(result);
        }

        [ Test ]
        public void ImportTuple1FromObject()
        {
            var tuple = JsonConvert.Import<ValueTuple<JsonObject>>("{x:123,y:456}");
            Assert.IsNotNull(tuple.Item1);
            dynamic obj = tuple.Item1;
            Assert.AreEqual(123, obj.x.ToInt32());
            Assert.AreEqual(456, obj.y.ToInt32());
        }

        [ Test ]
        public void ImportTuple1FromArray()
        {
            var expected = ValueTuple.Create(new[] { 123, 456, 789 });
            var actual = JsonConvert.Import<ValueTuple<int[]>>("[123, 456, 789]");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Item1, actual.Item1);
        }

        [ Test ]
        public void ImportTuple3FromArray()
        {
            AssertImport(ValueTuple.Create(42, "foo", true), "[42,foo,true]");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportTuple2FromNumber()
        {
            JsonConvert.Import<ValueTuple<int, int>>("42");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportTuple2FromString()
        {
            JsonConvert.Import<ValueTuple<string, string>>("foo");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportTuple2FromBoolean()
        {
            JsonConvert.Import<ValueTuple<bool, bool>>("true");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportTuple2FromObject()
        {
            JsonConvert.Import<ValueTuple<JsonObject, JsonObject>>("{}");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportWhenArrayHasTooFewElements()
        {
            JsonConvert.Import<ValueTuple<int, string, bool>>("[123,foo]");
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void SubClassCannotExportValueWithNullContext()
        {
            var tuple = ValueTuple.Create(42);
            var exporter = new ExportValueTestExporter(tuple.GetType()) { ForceNullContext = true };
            var context = JsonConvert.CreateExportContext();
            var writer = new JsonBufferWriter();
            exporter.Export(context, tuple, writer);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void SubClassCannotExportValueWithNullWriter()
        {
            var tuple = ValueTuple.Create(42);
            var exporter = new ExportValueTestExporter(tuple.GetType()) { ForceNullWriter = true };
            var context = JsonConvert.CreateExportContext();
            var writer = new JsonBufferWriter();
            exporter.Export(context, tuple, writer);
        }

        static void AssertImport(object expected, string input)
        {
            var importer = new ValueTupleImporter(expected.GetType());
            var reader = JsonText.CreateReader(input);
            var context = JsonConvert.CreateImportContext();
            var actual = importer.Import(context, reader);
            Assert.IsTrue(reader.EOF, "Reader must be at EOF.");
            Assert.AreEqual(expected, actual);
        }

        class ExportValueTestExporter : ValueTupleExporter
        {
            public bool ForceNullContext;
            public bool ForceNullWriter;

            public ExportValueTestExporter(Type inputType) :
                base(inputType) {}

            protected override void ExportValue(ExportContext context, object value, JsonWriter writer)
            {
                base.ExportValue(ForceNullContext ? null : context, value, ForceNullWriter ? null : writer);
            }
        }
    }
}

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

    using System;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestTupleExporter
    {
        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotInitializeWithNonTupleType()
        {
            new TupleExporter(typeof(object));
        }

        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ExporterBase>(new TupleExporter(typeof(Tuple<object>)));
        }

        [ Test ]
        public void InputTypeIsTuple()
        {
            Assert.AreSame(typeof(Tuple<object>), new TupleExporter(typeof(Tuple<object>)).InputType);
        }

        [ Test ]
        public void Export()
        {
            var reader = Export(Tuple.Create(123, "foo", true));
            Assert.IsTrue(reader.MoveToContent());
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual(123, reader.ReadNumber().ToInt32());
            Assert.AreEqual("foo", reader.ReadString());
            Assert.AreEqual(true, reader.ReadBoolean());
            reader.ReadToken(JsonTokenClass.EndArray);
            Assert.IsFalse(reader.Read());
        }

        static JsonReader Export(object value)
        {
            var writer = new JsonBufferWriter();
            JsonConvert.Export(value, writer);
            return writer.GetBuffer().CreateReader();
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void SubClassCannotExportValueWithNullContext()
        {
            var tuple = Tuple.Create(42);
            var exporter = new ExportValueTestExporter(tuple.GetType()) { ForceNullContext = true };
            var context = JsonConvert.CreateExportContext();
            var writer = new JsonBufferWriter();
            exporter.Export(context, tuple, writer);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void SubClassCannotExportValueWithNullWriter()
        {
            var tuple = Tuple.Create(42);
            var exporter = new ExportValueTestExporter(tuple.GetType()) { ForceNullWriter = true };
            var context = JsonConvert.CreateExportContext();
            var writer = new JsonBufferWriter();
            exporter.Export(context, tuple, writer);
        }

        class ExportValueTestExporter : TupleExporter
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

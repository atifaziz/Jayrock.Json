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
    public class TestNullableExporter
    {
        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ExporterBase>(new NullableExporter(typeof(int?)));
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotInitializeWithNonNullableType()
        {
            new NullableExporter(typeof(int));
        }

        [ Test ]
        public void InputTypeIsNullable()
        {
            Assert.AreSame(typeof(int?), (new NullableExporter(typeof(int?))).InputType);
        }

        [ Test ]
        public void ExportNull()
        {
            Export(null).ReadNull();
        }

        [ Test ]
        public void ExportNonNull()
        {
            Assert.AreEqual(42, Export(42).ReadNumber().ToInt32());
        }

        static JsonReader Export(int? value)
        {
            var writer = new JsonRecorder();
            var context = JsonConvert.CreateExportContext();
            new NullableExporter(typeof(int?)).Export(context, value, writer);
            return writer.CreatePlayer();
        }
    }
}

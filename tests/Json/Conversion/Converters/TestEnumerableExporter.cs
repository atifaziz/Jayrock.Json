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
    using System.Collections;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestEnumerableExporter
    {
        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ExporterBase>(new EnumerableExporter(typeof(Array)));
        }

        [ Test ]
        public void InputTypeInitialization()
        {
            var type = typeof(Array);
            var exporter = new EnumerableExporter(type);
            Assert.AreSame(type, exporter.InputType);
        }

        [ Test ]
        public void ExportEmpty()
        {
            var reader = Export(new object[] {});
            reader.ReadToken(JsonTokenClass.Array);
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void ExportFlatArray()
        {
            var reader = Export(new[] { 11, 22, 33 });
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual(11, reader.ReadNumber().ToInt32());
            Assert.AreEqual(22, reader.ReadNumber().ToInt32());
            Assert.AreEqual(33, reader.ReadNumber().ToInt32());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void ExportList()
        {
            var reader = Export(new ArrayList(new[] { 11, 22, 33 }));
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual(11, reader.ReadNumber().ToInt32());
            Assert.AreEqual(22, reader.ReadNumber().ToInt32());
            Assert.AreEqual(33, reader.ReadNumber().ToInt32());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void ExportNestedArrays()
        {
            var reader = Export(new ArrayList(new object[] { 11, 22, new object[] { 33, 44 }, 55 }));
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual(11, reader.ReadNumber().ToInt32());
            Assert.AreEqual(22, reader.ReadNumber().ToInt32());
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual(33, reader.ReadNumber().ToInt32());
            Assert.AreEqual(44, reader.ReadNumber().ToInt32());
            reader.ReadToken(JsonTokenClass.EndArray);
            Assert.AreEqual(55, reader.ReadNumber().ToInt32());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        static JsonReader Export(IEnumerable values)
        {
            var writer = new JsonRecorder();
            JsonConvert.Export(values, writer);
            return writer.CreatePlayer();
        }
    }
}

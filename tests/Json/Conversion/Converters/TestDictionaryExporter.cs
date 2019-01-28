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
    using System.Collections;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestDictionaryExporter
    {
        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ExporterBase>(new DictionaryExporter(typeof(Hashtable)));
        }

        [ Test ]
        public void InputTypeInitialization()
        {
            var type = typeof(Hashtable);
            var exporter = new DictionaryExporter(type);
            Assert.AreSame(type, exporter.InputType);
        }

        [ Test ]
        public void ExportEmpty()
        {
            var reader = Export(new Hashtable());
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual(JsonTokenClass.EndObject, reader.TokenClass);
        }

        [ Test ]
        public void ExportFlat()
        {
            var h = new Hashtable
            {
                ["FirstName" ] = "John",
                ["LastName"  ] = "Doe",
                ["MiddleName"] = null
            };

            var reader = Export(h);

            //
            // We need a complex assertions loop here because the order in
            // which members are written cannot be guaranteed to follow
            // the order of insertion.
            //

            reader.ReadToken(JsonTokenClass.Object);
            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                var member = reader.ReadMember();
                Assert.IsTrue(h.Contains(member));

                var expected = h[member];

                if (expected == null)
                    reader.ReadNull();
                else
                    Assert.AreEqual(expected, reader.ReadString());
            }
        }

        static JsonReader Export(IDictionary value)
        {
            var writer = new JsonRecorder();
            JsonConvert.Export(value, writer);
            return writer.CreatePlayer();
        }
    }
}

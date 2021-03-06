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
    public class TestDateTimeExporter
    {
        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOf<ExporterBase>(new DateTimeExporter());
        }

        [ Test ]
        public void InputTypeIsString()
        {
            Assert.AreSame(typeof(DateTime), (new DateTimeExporter()).InputType);
        }

        [ Test ]
        public void Export()
        {
            var time = new DateTime(1999, 12, 31, 23, 30, 59, 999);
            Assert.AreEqual("1999-12-31T23:30:59.9990000" + Tzd(time), Export(time));
        }

        static string Export(DateTime value)
        {
            var writer = new JsonRecorder();
            var exporter = new DateTimeExporter();
            exporter.Export(new ExportContext(), value, writer);
            return writer.CreatePlayer().ReadString();
        }

        static string Tzd(DateTime localTime)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(localTime);
            var offsetString = offset.ToString();
            return offset.Ticks < 0 ?
                   (offsetString.Substring(0, 6)) :
                   ("+" + offsetString.Substring(0, 5));
        }
    }
}

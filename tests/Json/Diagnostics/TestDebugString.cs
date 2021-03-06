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

namespace Jayrock.Diagnostics
{
    #region Imports

    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestDebugString
    {
        const string _sample = "The quick brown fox jumps over the lazy dog.";

        [ Test ]
        public void FormatNullYieldsEmptyString()
        {
            Assert.AreEqual(string.Empty, DebugString.Format(null));
        }

        [ Test ]
        public void ClippedWhenExceedsWidth()
        {
            Assert.AreEqual("The quick" + DebugString.Ellipsis, DebugString.Format(_sample, 10));
        }

        [ Test ]
        public void NotClippedWhenWithinWidth()
        {
            Assert.AreEqual(_sample, DebugString.Format(_sample, 100));
            Assert.AreEqual(_sample, DebugString.Format(_sample, _sample.Length));
        }

        [ Test ]
        public void ControlsCharactersSuppression()
        {
            int[] ranges = { 0, 0x1f, 0x7f, 0x7f, 0x80, 0x9f };

            var count = 0;
            for (var i = 0; i < ranges.Length; i += 2)
                count += (ranges[i + 1] - ranges[i]) + 1;

            var controls = new char[count];

            var running = 0;
            for (var i = 0; i < ranges.Length; i += 2)
                for (var j = ranges[i]; j <= ranges[i + 1]; j++)
                    controls[running++] = (char) j;

            Assert.AreEqual(new string(DebugString.ControlReplacement, count), DebugString.Format(new string(controls), count));
        }
    }
}

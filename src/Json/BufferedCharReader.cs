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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.IO;

    #endregion

    sealed class BufferedCharReader
    {
        readonly TextReader _reader;
        readonly int _bufferSize;
        char[] _buffer;
        int _index;
        int _end;
        bool _backed;
        char _backup;
        int _lastLinePosition;
        bool _sawLineFeed = true;

        public const char EOF = (char) 0;

        public BufferedCharReader(TextReader reader) :
            this(reader, 0) {}

        public BufferedCharReader(TextReader reader, int bufferSize)
        {
            Debug.Assert(reader != null);

            _reader = reader;
            _bufferSize = Math.Max(256, bufferSize);
        }

        public int CharCount    { get; private set; }
        public int LineNumber   { get; private set; }
        public int LinePosition { get; private set; }

        /// <summary>
        /// Back up one character. This provides a sort of lookahead capability,
        /// so that one can test for a digit or letter before attempting to,
        /// for example, parse the next number or identifier.
        /// </summary>
        /// <remarks>
        /// This implementation currently does not support backing up more
        /// than a single character (the last read).
        /// </remarks>

        public void Back()
        {
            Debug.Assert(!_backed);

            if (CharCount == 0)
                return;

            _backed = true;

            CharCount--;
            LinePosition--;

            if (LinePosition == 0)
            {
                LineNumber--;
                LinePosition = _lastLinePosition;
                _sawLineFeed = true;
            }
        }

        /// <summary>
        /// Determine if the source string still contains characters that Next()
        /// can consume.
        /// </summary>
        /// <returns>true if not yet at the end of the source.</returns>

        public bool More()
        {
            if (!_backed && _index == _end)
            {
                if (_buffer == null)
                    _buffer = new char[_bufferSize];

                _index = 0;
                _end = _reader.Read(_buffer, 0, _buffer.Length);

                if (_end == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get the next character in the source string.
        /// </summary>
        /// <returns>The next character, or 0 if past the end of the source string.</returns>

        public char Next()
        {
            char ch;

            if (_backed)
            {
                _backed = false;
                ch = _backup;
            }
            else
            {
                if (!More())
                    return EOF;

                ch = _buffer[_index++];
                _backup = ch;
            }

            return UpdateCounters(ch);
        }

        char UpdateCounters(char ch)
        {
            CharCount++;

            if (_sawLineFeed)
            {
                LineNumber++;
                _lastLinePosition = LinePosition;
                LinePosition = 1;
                _sawLineFeed = false;
            }
            else
            {
                LinePosition++;
            }

            _sawLineFeed = ch == '\n';
            return ch;
        }
    }
}

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

    using System.IO;

    #endregion

    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only means of
    /// emitting JSON data formatted as JSON text (RFC 4627).
    /// </summary>

    public class JsonTextWriter : JsonWriterBase
    {
        //
        // Pretty printing as per:
        // http://developer.mozilla.org/es4/proposals/json_encoding_and_decoding.html
        //
        // <quote>
        // ...linefeeds are inserted after each { and , and before } , and multiples
        // of 4 spaces are inserted to indicate the level of nesting, and one space
        // will be inserted after :. Otherwise, no whitespace is inserted between
        // the tokens.
        // </quote>
        //

        bool _newLine;
        int _indent;
        char[] _indentBuffer;

        public JsonTextWriter() :
            this(null) {}

        public JsonTextWriter(TextWriter writer)
        {
            InnerWriter = writer ?? new StringWriter();
        }

        public bool PrettyPrint { get; set; }

        protected TextWriter InnerWriter { get; }

        public override void Flush()
        {
            InnerWriter.Flush();
        }

        public override string ToString()
        {
            var stringWriter = InnerWriter as StringWriter;
            return stringWriter != null ?
                stringWriter.ToString() : base.ToString();
        }

        protected override void WriteStartObjectImpl()
        {
            OnWritingValue();
            WriteDelimiter('{');
            PrettySpace();
        }

        protected override void WriteEndObjectImpl()
        {
            if (Index > 0)
            {
                PrettyLine();
                _indent--;
            }

            WriteDelimiter('}');
        }

        protected override void WriteMemberImpl(string name)
        {
            if (Index > 0)
            {
                WriteDelimiter(',');
                PrettyLine();
            }
            else
            {
                PrettyLine();
                _indent++;
            }

            WriteStringImpl(name);
            WriteDelimiter(':');
            PrettySpace();
        }

        protected override void WriteStringImpl(string value)
        {
            WriteScalar(JsonString.Enquote(value));
        }

        protected override void WriteNumberImpl(string value)
        {
            WriteScalar(value);
        }

        protected override void WriteBooleanImpl(bool value)
        {
            WriteScalar(value ? JsonBoolean.TrueText : JsonBoolean.FalseText);
        }

        protected override void WriteNullImpl()
        {
            WriteScalar(JsonNull.Text);
        }

        protected override void WriteStartArrayImpl()
        {
            OnWritingValue();
            WriteDelimiter('[');
            PrettySpace();
        }

        protected override void WriteEndArrayImpl()
        {
            if (IsNonEmptyArray())
                PrettySpace();

            WriteDelimiter(']');
        }

        void WriteScalar(string text)
        {
            OnWritingValue();
            PrettyIndent();
            InnerWriter.Write(text);
        }

        bool IsNonEmptyArray()
        {
            return Bracket == JsonWriterBracket.Array && Index > 0;
        }

        //
        // Methods below are mostly related to pretty-printing of JSON text.
        //

        void OnWritingValue()
        {
            if (IsNonEmptyArray())
            {
                WriteDelimiter(',');
                PrettySpace();
            }
        }

        void WriteDelimiter(char ch)
        {
            PrettyIndent();
            InnerWriter.Write(ch);
        }

        void PrettySpace()
        {
            if (!PrettyPrint) return;
            WriteDelimiter(' ');
        }

        void PrettyLine()
        {
            if (!PrettyPrint) return;
            InnerWriter.WriteLine();
            _newLine = true;
        }

        void PrettyIndent()
        {
            if (!PrettyPrint)
                return;

            if (_newLine)
            {
                if (_indent > 0)
                {
                    var spaces = _indent * 4;

                    if (_indentBuffer == null || _indentBuffer.Length < spaces)
                        _indentBuffer = new string(' ', spaces * 4).ToCharArray();

                    InnerWriter.Write(_indentBuffer, 0, spaces);
                }

                _newLine = false;
            }
        }
    }
}

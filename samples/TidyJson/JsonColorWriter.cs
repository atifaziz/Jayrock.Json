#region Copyright (c) 2006 Atif Aziz. All rights reserved.
//
// The MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files
// (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject
// to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

namespace TidyJson
{
    #region Imports

    using System;
    using Jayrock.Json;

    #endregion

    sealed class JsonColorWriter : JsonWriter
    {
        public JsonColorWriter(JsonWriter inner) :
            this(inner, null) {}

        public JsonColorWriter(JsonWriter inner, JsonPalette palette)
        {
            this.InnerWriter = inner;
            this.Palette = palette ?? JsonPalette.Auto();
        }

        public JsonWriter InnerWriter { get; }

        public JsonPalette Palette { get; set; }

        public override int Index => InnerWriter.Index;

        public override JsonWriterBracket Bracket => InnerWriter.Bracket;

        public override void WriteStartObject()
        {
            Palette.Object.Apply();
            InnerWriter.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            Palette.Object.Apply();
            InnerWriter.WriteEndObject();
        }

        public override void WriteMember(string name)
        {
            Palette.Member.Apply();
            InnerWriter.WriteMember(name);
        }

        public override void WriteStartArray()
        {
            Palette.Array.Apply();
            InnerWriter.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            Palette.Array.Apply();
            InnerWriter.WriteEndArray();
        }

        public override void WriteString(string value)
        {
            Palette.String.Apply();
            InnerWriter.WriteString(value);
        }

        public override void WriteNumber(string value)
        {
            Palette.Number.Apply();
            InnerWriter.WriteNumber(value);
        }

        public override void WriteBoolean(bool value)
        {
            Palette.Boolean.Apply();
            InnerWriter.WriteBoolean(value);
        }

        public override void WriteNull()
        {
            Palette.Null.Apply();
            InnerWriter.WriteNull();
        }

        public override void Flush()
        {
            InnerWriter.Flush();
        }

        public override void Close()
        {
            InnerWriter.Close();
        }

        public override int Depth => InnerWriter.Depth;

        public override int MaxDepth
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}

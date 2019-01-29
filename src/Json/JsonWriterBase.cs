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
    using System.Collections.Generic;
    using System.Diagnostics;

    #endregion

    /// <summary>
    /// Base implementation of <see cref="JsonWriter"/> that can be used
    /// as a starting point for sub-classes of <see cref="JsonWriter"/>.
    /// </summary>

    public abstract class JsonWriterBase : JsonWriter
    {
        Stack<(JsonWriterBracket, int)> _stateStack;
        JsonWriterBracket _bracket;
        int _index;

        protected JsonWriterBase()
        {
            _bracket = JsonWriterBracket.Pending;
        }

        public sealed override int Depth => HasStates ? States.Count : 0;

        public override int MaxDepth { get; set; } = 30;

        public sealed override int Index => Depth == 0 ? -1 : _index;

        public sealed override JsonWriterBracket Bracket => _bracket;

        public sealed override void WriteStartObject()
        {
            EnteringBracket();
            WriteStartObjectImpl();
            EnterBracket(JsonWriterBracket.Object);
        }

        public sealed override void WriteEndObject()
        {
            if (_bracket != JsonWriterBracket.Object)
                throw new JsonException("JSON Object tail not expected at this time.");

            WriteEndObjectImpl();
            ExitBracket();
        }

        public sealed override void WriteMember(string name)
        {
            if (_bracket != JsonWriterBracket.Object)
                throw new JsonException("A JSON Object member is not valid inside a JSON Array.");

            WriteMemberImpl(name);
            _bracket = JsonWriterBracket.Member;
        }

        public sealed override void WriteStartArray()
        {
            EnteringBracket();
            WriteStartArrayImpl();
            EnterBracket(JsonWriterBracket.Array);
        }

        public sealed override void WriteEndArray()
        {
            if (_bracket != JsonWriterBracket.Array)
                throw new JsonException("JSON Array tail not expected at this time.");

            WriteEndArrayImpl();
            ExitBracket();
        }

        public sealed override void WriteString(char[] chars, int offset, int length)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            WriteStringOrChars(null, chars, offset, length);
        }

        public sealed override void WriteString(string value)
        {
            WriteStringOrChars(value, null, 0, 0);
        }

        void WriteStringOrChars(string value, char[] chars, int offset, int length)
        {
            if (Depth == 0)
            {
                WriteStartArray();
                if (chars != null)
                    WriteString(chars, offset, length);
                else
                    WriteString(value);
                WriteEndArray();
            }
            else
            {
                EnsureMemberOnObjectBracket();
                if (chars != null)
                    WriteStringImpl(chars, offset, length);
                else
                    WriteStringImpl(value);
                OnValueWritten();
            }
        }

        public sealed override void WriteNumber(string value)
        {
            if (Depth == 0)
            {
                WriteStartArray(); WriteNumber(value); WriteEndArray();
            }
            else
            {
                EnsureMemberOnObjectBracket();
                WriteNumberImpl(value);
                OnValueWritten();
            }
        }

        public sealed override void WriteBoolean(bool value)
        {
            if (Depth == 0)
            {
                WriteStartArray(); WriteBoolean(value); WriteEndArray();
            }
            else
            {
                EnsureMemberOnObjectBracket();
                WriteBooleanImpl(value);
                OnValueWritten();
            }
        }

        public sealed override void WriteNull()
        {
            if (Depth == 0)
            {
                WriteStartArray(); WriteNull(); WriteEndArray();
            }
            else
            {
                EnsureMemberOnObjectBracket();
                WriteNullImpl();
                OnValueWritten();
            }
        }

        //
        // Actual methods that need to be implemented by the subclass.
        // These methods do not need to check for the structural
        // integrity since this is checked by this base implementation.
        //

        protected abstract void WriteStartObjectImpl();
        protected abstract void WriteEndObjectImpl();
        protected abstract void WriteMemberImpl(string name);
        protected abstract void WriteStartArrayImpl();
        protected abstract void WriteEndArrayImpl();
        protected abstract void WriteStringImpl(string value);
        protected virtual  void WriteStringImpl(char[] buffers, int offset, int length) { WriteStringImpl(new string(buffers, offset, length)); }
        protected abstract void WriteNumberImpl(string value);
        protected abstract void WriteBooleanImpl(bool value);
        protected abstract void WriteNullImpl();

        bool HasStates => _stateStack?.Count > 0;
        Stack<(JsonWriterBracket, int)> States =>
            _stateStack ?? (_stateStack = new Stack<(JsonWriterBracket, int)>());

        void EnteringBracket()
        {
            EnsureNotEnded();

            if (_bracket != JsonWriterBracket.Pending)
                EnsureMemberOnObjectBracket();

            if (Depth + 1 > MaxDepth)
                throw new Exception("Maximum allowed depth has been exceeded.");
        }

        void EnterBracket(JsonWriterBracket newBracket)
        {
            Debug.Assert(newBracket == JsonWriterBracket.Array || newBracket == JsonWriterBracket.Object);

            States.Push((_bracket, _index));
            _bracket = newBracket;
            _index = 0;
        }

        void ExitBracket()
        {
            (_bracket, _index) = States.Pop();

            if (_bracket == JsonWriterBracket.Pending)
                _bracket = JsonWriterBracket.Closed;
            else
                OnValueWritten();
        }

        void OnValueWritten()
        {
            if (_bracket == JsonWriterBracket.Member)
                _bracket = JsonWriterBracket.Object;

            _index++;
        }

        void EnsureMemberOnObjectBracket()
        {
            if (_bracket == JsonWriterBracket.Object)
                throw new JsonException("A JSON member value inside a JSON object must be preceded by its member name.");
        }

        void EnsureNotEnded()
        {
            if (_bracket == JsonWriterBracket.Closed)
                throw new JsonException("JSON data has already been ended.");
        }
    }
}

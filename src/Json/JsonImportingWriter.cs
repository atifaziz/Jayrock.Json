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
    using System.Collections.Generic;

    public class JsonImportingWriter : JsonWriterBase
    {
        readonly Stack<object> _valueStack = new Stack<object>();
        readonly Stack<string> _memberStack = new Stack<string>();
        JsonObject _object;
        JsonArray _array;
        string _member;

        public object Value { get; private set; }
        public bool IsObject => _object != null;
        public bool IsArray => _array != null;

        void Push()
        {
            _valueStack.Push(Value);
            _memberStack.Push(_member);
            _array = null;
            _object = null;
            Value = null;
            _member = null;
        }

        void Pop()
        {
            var current = Value;
            var popped = _valueStack.Pop();
            _member = _memberStack.Pop();
            if (popped == null) // Final result?
                return;
            _object = popped as JsonObject;
            _array = _object == null ? (JsonArray) popped : null;
            Value = popped;
            WriteValue(current);
        }

        protected override void WriteStartObjectImpl()
        {
            Push();
            Value = _object = new JsonObject();
        }

        protected override void WriteEndObjectImpl()
        {
            Pop();
        }

        protected override void WriteMemberImpl(string name)
        {
            _member = name;
        }

        protected override void WriteStartArrayImpl()
        {
            Push();
            Value = _array = new JsonArray();
        }

        protected override void WriteEndArrayImpl()
        {
            Pop();
        }

        void WriteValue(object value)
        {
            if (IsObject)
            {
                _object[_member] = value;
                _member = null;
            }
            else
            {
                _array.Add(value);
            }
        }

        protected override void WriteStringImpl(string value)
        {
            WriteValue(value);
        }

        protected override void WriteNumberImpl(string value)
        {
            WriteValue(new JsonNumber(value));
        }

        protected override void WriteBooleanImpl(bool value)
        {
            WriteValue(value);
        }

        protected override void WriteNullImpl()
        {
            WriteValue(null);
        }
    }
}

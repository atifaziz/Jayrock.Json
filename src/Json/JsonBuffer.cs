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
    using System.Text;

    #endregion

    /// <summary>
    /// Represent buffered JSON token data.
    /// </summary>

    [ Serializable ]
    public struct JsonBuffer
    {
        public static readonly JsonBuffer Empty = new JsonBuffer();

        readonly JsonBufferStorage _storage;
        readonly int _start;
        readonly int _end;

        static readonly JsonBuffer _null;
        static readonly JsonBuffer _true;
        static readonly JsonBuffer _false;

        static JsonBuffer()
        {
            var buffer = new JsonBufferStorage(5) // [null,true,false]
                .Write(JsonToken.Array())
                    .Write(JsonToken.Null(), JsonToken.True(), JsonToken.False())
                .Write(JsonToken.EndArray())
                .ToBuffer();

            _null = buffer.SliceImpl(1, 2);
            _true = buffer.SliceImpl(2, 3);
            _false = buffer.SliceImpl(3, 4);
        }

        internal JsonBuffer(JsonBufferStorage storage, int start, int end)
        {
            Debug.Assert(storage != null);
            Debug.Assert(start >= 0);
            Debug.Assert(end >= start);

            _storage = storage;
            _start = start;
            _end = end;
        }

        public static JsonBuffer From(string json)
        {
            return From(JsonText.CreateReader(json));
        }

        public static JsonBuffer From(JsonToken token)
        {
            var clazz = token.Class;

            if (clazz == JsonTokenClass.Null)
                return _null;

            if (!clazz.IsScalar)
                throw new ArgumentException("Token must represent a JSON scalar value or null.", nameof(token));

            if (clazz == JsonTokenClass.Boolean)
                return token.Equals(JsonToken.True()) ? _true : _false;

            var storage = new JsonBufferStorage(1);
            storage.Write(token);
            return storage.ToBuffer();
        }

        public static JsonBuffer From(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var bufferReader = reader as JsonBufferReader;
            if (bufferReader != null)
                return bufferReader.BufferValue();

            if (!reader.MoveToContent())
                return Empty;

            if (reader.TokenClass == JsonTokenClass.Member)
                reader.Read();

            var structured = reader.TokenClass == JsonTokenClass.Array
                              || reader.TokenClass == JsonTokenClass.Object;

            var writer = new JsonBufferWriter();
            writer.MaxDepth = reader.MaxDepth;
            writer.WriteFromReader(reader);
            var buffer = writer.GetBuffer();

            if (!structured)
            {
                bufferReader = buffer.CreateReader();
                bufferReader.MoveToContent();
                bufferReader.Read();
                buffer = bufferReader.BufferValue();
            }

            return buffer;
        }

        /// <summary>
        /// Gets the token at the specified index.
        /// </summary>

        public JsonToken this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _storage[_start + index];
            }
        }

        /// <summary>
        /// Gets the number of tokens contained in the buffer.
        /// </summary>

        public int Length { get { return _end - _start; } }

        JsonToken FirstToken
        {
            get
            {
                Debug.Assert(!IsEmpty);
                return _storage[_start];
            }
        }

        /// <summary>
        /// Indicates whether the buffer is empty or not.
        /// </summary>

        public bool IsEmpty { get { return Length == 0; } }

        /// <summary>
        /// Indicates whether the buffer represents simply a JSON null or not.
        /// </summary>

        public bool IsNull
        {
            get { return Length == 1 && FirstToken.Class == JsonTokenClass.Null; }
        }

        /// <summary>
        /// Indicates whether the buffer represents a JSON scalar value
        /// (number, string or Boolean) or not.
        /// </summary>

        public bool IsScalar
        {
            get { return Length == 1 && FirstToken.Class.IsScalar; }
        }

        /// <summary>
        /// Indicates whether the buffer represents a JSON structured value,
        /// that is, an array or object or not.
        /// </summary>

        public bool IsStructured
        {
            get { return !IsEmpty && !IsNull && !IsScalar; }
        }

        /// <summary>
        /// Indicates whether the buffer represents a JSON object or not.
        /// </summary>

        public bool IsObject
        {
            get { return IsStructured && FirstToken.Class == JsonTokenClass.Object; }
        }

        /// <summary>
        /// Indicates whether the buffer represents a JSON array or not.
        /// </summary>

        public bool IsArray
        {
            get { return IsStructured && FirstToken.Class == JsonTokenClass.Array; }
        }

        /// <summary>
        /// Creates a <see cref="JsonBufferReader" /> object that can be
        /// used to read the content of the buffer.
        /// </summary>
        /// <remarks>
        /// If the buffer contains a JSON null or scalar value then the
        /// returned reader is already started and positioned on the value.
        /// </remarks>

        public JsonBufferReader CreateReader()
        {
            if (IsEmpty)
                throw new InvalidOperationException();
            var reader = new JsonBufferReader(this);
            if (!IsStructured)
                reader.ReadToken(JsonTokenClass.Array);
            return reader;
        }

        public bool GetBoolean()
        {
            return CreateReader().ReadBoolean();
        }

        public string GetString()
        {
            return CreateReader().ReadString();
        }

        public JsonNumber GetNumber()
        {
            return CreateReader().ReadNumber();
        }

        public int GetArrayLength()
        {
            return GetArray(null, 0, int.MaxValue);
        }

        public JsonBuffer[] GetArray()
        {
            var values = new JsonBuffer[GetArrayLength()];
            GetArray(values);
            return values;
        }

        public int GetArray(JsonBuffer[] values)
        {
            return GetArray(values, 0, values.Length);
        }

        public int GetArray(JsonBuffer[] values, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, null);

            var reader = CreateReader();

            if (!reader.MoveToContent())
                throw new JsonException("Unexpected EOF.");

            if (reader.TokenClass == JsonTokenClass.Null)
                return 0;

            reader.ReadToken(JsonTokenClass.Array);
            var readCount = 0;

            while (reader.TokenClass != JsonTokenClass.EndArray)
            {
                if (count-- == 0)
                    return ~readCount;

                if (values != null)
                    values[index++] = reader.BufferValue();
                else
                    reader.Skip();

                readCount++;
            }

            return readCount;
        }

        public int GetMemberCount()
        {
            return GetMembers(null);
        }

        public int GetMembers(NamedJsonBuffer[] members)
        {
            return GetMembers(members, 0, members != null ? members.Length : int.MaxValue);
        }

        public int GetMembers(NamedJsonBuffer[] members, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, null);

            var reader = CreateReader();

            if (!reader.MoveToContent())
                throw new JsonException("Unexpected EOF.");

            if (reader.TokenClass == JsonTokenClass.Null)
                return 0;

            reader.ReadToken(JsonTokenClass.Object);
            var readCount = 0;

            while (reader.TokenClass == JsonTokenClass.Member)
            {
                if (count-- == 0)
                    return ~readCount;

                if (members != null)
                    members[index++] = new NamedJsonBuffer(reader.Text, reader.BufferValue());
                else
                    reader.Skip();

                readCount++;
            }

            return readCount;
        }

        public NamedJsonBuffer[] GetMembersArray()
        {
            var members = new NamedJsonBuffer[GetMemberCount()];
            GetMembers(members);
            return members;
        }

        public override int GetHashCode()
        {
            var result = 0;
            for (var i = 0; i < Length; i++)
                result ^= this[i].GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is JsonBuffer && Equals((JsonBuffer) obj);
        }

        public bool Equals(JsonBuffer other)
        {
            if (Length != other.Length)
                return false;

            for (var i = 0; i < Length; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public IEnumerable<NamedJsonBuffer> GetMembers()
        {
            var reader = CreateReader();
            reader.ReadToken(JsonTokenClass.Object);
            while (reader.TokenClass == JsonTokenClass.Member)
                yield return new NamedJsonBuffer(reader.Text, reader.BufferValue());
        }

        /// <summary>
        /// Returns the content of the buffer formatted as JSON text.
        /// </summary>

        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;
            if (IsNull)
                return JsonNull.Text;
            if (FirstToken.Class == JsonTokenClass.String)
                return JsonString.Enquote(FirstToken.Text);
            if (IsScalar)
                return FirstToken.Text;
            var sb = new StringBuilder();
            JsonText.CreateWriter(sb).WriteFromReader(CreateReader());
            return sb.ToString();
        }

        public static bool operator==(JsonBuffer lhs, JsonBuffer rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(JsonBuffer lhs, JsonBuffer rhs)
        {
            return !lhs.Equals(rhs);
        }

        internal JsonBuffer Slice(int start, int end)
        {
            return IsStructured
                 ? SliceImpl(_start + start, _start + end)
                 : SliceImpl(start, end);
        }

        JsonBuffer SliceImpl(int start, int end)
        {
            Debug.Assert(start >= _start);
            Debug.Assert(end <= _end);

            return start == end ? Empty : new JsonBuffer(_storage, start, end);
        }
    }
}

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

    #endregion

    [ Serializable ]
    public struct NamedJsonBuffer
    {
        public static readonly NamedJsonBuffer Empty = new NamedJsonBuffer();

        private readonly string _name;
        private readonly JsonBuffer _buffer;

        public NamedJsonBuffer(string name, JsonBuffer buffer)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (buffer.IsEmpty)
                throw new ArgumentException(null, nameof(buffer));

            _name = Mask.NullString(name);
            _buffer = buffer;
        }

        public string Name { get { return _name; } }
        public JsonBuffer Buffer { get { return _buffer; } }
        public bool IsEmpty { get { return _name == null && _buffer.IsEmpty; } }

        public bool Equals(NamedJsonBuffer other)
        {
            return Name == other.Name && Buffer.Equals(other.Buffer);
        }

        public override bool Equals(object obj)
        {
            return obj is NamedJsonBuffer && Equals((NamedJsonBuffer) obj);
        }

        public override int GetHashCode()
        {
            return IsEmpty ? 0 : Name.GetHashCode() ^ Buffer.GetHashCode();
        }

        public override string ToString()
        {
            return IsEmpty ? string.Empty : Mask.EmptyString(Name, "(anonymous)") + ": " + Buffer;
        }

        public static JsonBuffer ToObject(params NamedJsonBuffer[] members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            if (members.Length == 0)
                return StockJsonBuffers.EmptyObject;

            var writer = new JsonBufferWriter();
            writer.WriteStartObject();
            foreach (var member in members)
            {
                writer.WriteMember(member.Name);
                writer.WriteFromReader(member.Buffer.CreateReader());
            }
            writer.WriteEndObject();
            return writer.GetBuffer();
        }
    }
}

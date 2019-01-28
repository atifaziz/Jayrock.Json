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
    using Jayrock.Diagnostics;

    #endregion

    [ Serializable ]
    public struct JsonToken : IEquatable<JsonToken>
    {
        JsonToken(JsonTokenClass clazz, string text = null)
        {
            Class = clazz;
            Text = text;
        }

        /// <summary>
        /// Gets the class/type/category of the token.
        /// </summary>

        public JsonTokenClass Class { get; }

        /// <summary>
        /// Gets the current token text, if applicable, otherwise null.
        /// </summary>

        public string Text { get; }

        public override string ToString()
        {
            return Text == null ? Class.Name : Class.Name + ":" + DebugString.Format(Text);
        }

        public override int GetHashCode()
        {
            return Class.GetHashCode() ^ (Text == null ? 0 : Text.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return obj is JsonToken token && Equals(token);
        }

        public bool Equals(JsonToken other)
        {
            return Class.Equals(other.Class) && (Text == null || Text.Equals(other.Text));
        }

        //
        // Static methods for building tokens of various classes...
        //

        public static JsonToken Null()
        {
            return new JsonToken(JsonTokenClass.Null, JsonNull.Text);
        }

        public static JsonToken Array()
        {
            return new JsonToken(JsonTokenClass.Array);
        }

        public static JsonToken EndArray()
        {
            return new JsonToken(JsonTokenClass.EndArray);
        }

        public static JsonToken Object()
        {
            return new JsonToken(JsonTokenClass.Object);
        }

        public static JsonToken EndObject()
        {
            return new JsonToken(JsonTokenClass.EndObject);
        }

        public static JsonToken BOF()
        {
            return new JsonToken(JsonTokenClass.BOF);
        }

        public static JsonToken EOF()
        {
            return new JsonToken(JsonTokenClass.EOF);
        }

        public static JsonToken String(string text)
        {
            return new JsonToken(JsonTokenClass.String, Mask.NullString(text));
        }

        public static JsonToken Boolean(bool value)
        {
            return new JsonToken(JsonTokenClass.Boolean, value ? "true" : "false");
        }

        public static JsonToken True()
        {
            return Boolean(true);
        }

        public static JsonToken False()
        {
            return Boolean(false);
        }

        public static JsonToken Number(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
                throw new ArgumentException("Number text cannot zero in length.", nameof(text));

            return new JsonToken(JsonTokenClass.Number, text);
        }

        public static JsonToken Member(string name)
        {
            return new JsonToken(JsonTokenClass.Member, name);
        }
    }
}

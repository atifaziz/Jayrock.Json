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
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    public class DictionaryImporter : ImportAwareImporter
    {
        public DictionaryImporter() :
            base(typeof(IDictionary)) {}

        protected override IJsonImportable CreateObject()
        {
            return new JsonObject();
        }
    }

    public class DictionaryImporter<TKey, TValue> : ImporterBase
    {
        public DictionaryImporter() :
            this(typeof(IDictionary<TKey, TValue>)) { }

        protected DictionaryImporter(Type outputType) :
            base(outputType) { }

        protected override object ImportFromObject(ImportContext context, JsonReader reader)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var dictionary = CreateDictionary();
            var isKeyOfString = IsKeyOfString;

            reader.ReadToken(JsonTokenClass.Object);

            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                var name = reader.ReadMember();
                TKey key;

                if (isKeyOfString)
                {
                    key = (TKey) (object) name;
                }
                else
                {
                    var buffer = JsonBuffer.From(JsonToken.String(name));
                    key = context.Import<TKey>(buffer.CreateReader());
                }

                dictionary.Add(key, context.Import<TValue>(reader));
            }

            return ReadReturning(reader, dictionary);
        }

        protected virtual IDictionary<TKey, TValue> CreateDictionary()
        {
            var comparer = IsKeyOfString
                ? (IEqualityComparer<TKey>) StringComparer.Ordinal
                : null;

            return new Dictionary<TKey, TValue>(comparer);
        }

        static bool IsKeyOfString
        {
            get { return Type.GetTypeCode(typeof(TKey)) == TypeCode.String; }
        }
    }

    sealed class DictionaryImporter<TDictionary, TKey, TValue> :
        DictionaryImporter<TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        public DictionaryImporter() :
            base(typeof(TDictionary)) {}

        protected override IDictionary<TKey, TValue> CreateDictionary()
        {
            return new TDictionary();
        }
    }
}

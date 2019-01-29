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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.IO;
    using System.Linq.Expressions;

    using Jayrock.Dynamic;
    using Jayrock.Json.Conversion;

    #endregion

    /// <summary>
    /// A collection of name-value member pairs making up a JSON object.
    /// </summary>

    // Public Domain 2002 JSON.org, ported to C# by Are Bjolseth (teleplan.no)
    // and re-adapted by Atif Aziz (www.raboof.com)</para>

    [ Serializable ]
    public class JsonObject :
        KeyedCollection<string, JsonMember>,
        IDictionary<string, object>,
        IDictionary,
        IJsonImportable, IJsonExportable,
        IDynamicMetaObjectProvider
    {
        [ NonSerialized ] ReadOnlyCollection<string> _keys;
        [ NonSerialized ] ReadOnlyCollection<object> _values;

        public JsonObject() {}

        /// <summary>
        /// Construct a JsonObject from a IDictionary
        /// </summary>

        public JsonObject(IDictionary members)
        {
            foreach (var entry in DictionaryHelper.GetEntries(members))
            {
                if (entry.Key == null)
                    throw new InvalidMemberException();

                Add(entry.Key.ToString(), entry.Value);
            }
        }

        public JsonObject(string[] keys, object[] values)
        {
            var keyCount = keys?.Length ?? 0;
            var valueCount = values?.Length ?? 0;
            var count = Math.Max(keyCount, valueCount);

            var key = string.Empty;

            for (var i = 0; i < count; i++)
            {
                if (i < keyCount)
                    key = Mask.NullString(keys[i]);

                Accumulate(key, i < valueCount ? values[i] : null);
            }
        }

        public JsonObject(IEnumerable<JsonMember> members)
        {
            if (members == null)
                return;

            foreach (var member in members)
                Accumulate(member.Name, member.Value);
        }

        ReadOnlyCollection<string> CachedKeys   => _keys   ?? (_keys   = GetMembers(m => m.Name ));
        ReadOnlyCollection<object> CachedValues => _values ?? (_values = GetMembers(m => m.Value));

        void OnUpdating()
        {
            _keys = null;
            _values = null;
        }

        protected override string GetKeyForItem(JsonMember item)
        {
            return item.Name;
        }

        public new virtual object this[string name]
        {
            get => TryGetMemberByName(name, out var member) ? member.Value : null;
            set => Put(name, value);
        }

        public virtual bool HasMembers => Count > 0;

        bool TryGetMemberByName(string name, out JsonMember member)
        {
            if (Dictionary != null)
                return Dictionary.TryGetValue(name, out member);

            foreach (var e in this)
            {
                if (Comparer.Equals(e.Name, name))
                {
                    member = e;
                    return true;
                }
            }

            member = default;
            return false;
        }

        /// <summary>
        /// Accumulate values under a key. It is similar to the Put method except
        /// that if there is already an object stored under the key then a
        /// JsonArray is stored under the key to hold all of the accumulated values.
        /// If there is already a JsonArray, then the new value is appended to it.
        /// In contrast, the Put method replaces the previous value.
        /// </summary>

        public virtual void Accumulate(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var current = this[name];

            if (current == null)
            {
                Put(name, value);
            }
            else
            {
                if (current is IList values)
                {
                    values.Add(value);
                }
                else
                {
                    values = new JsonArray { current, value };
                    Put(name, values);
                }
            }
        }

        /// <summary>
        /// Adds a key/value pair to the JsonObject. If the key is already
        /// present then an exception is thrown.
        /// </summary>

        public virtual void Add(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Add(new JsonMember(name, value));
        }

        /// <summary>
        /// Put a key/value pair in the JsonObject. If the value is null,
        /// then the key will be removed from the JsonObject if it is present.
        /// </summary>

        public virtual void Put(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (Dictionary == null || Dictionary.ContainsKey(name))
            {
                for (var i = 0; i < Count; i++)
                {
                    if (Comparer.Equals(GetKeyForItem(this[i]), name))
                    {
                        this[i] = new JsonMember(name, value);
                        return;
                    }
                }
            }

            Add(name, value);
        }

        public virtual ICollection<string> Names => CachedKeys;

        /// <summary>
        /// Produce a JsonArray containing the names of the elements of this
        /// JsonObject.
        /// </summary>

        public virtual JsonArray GetNamesArray()
        {
            return new JsonArray(Names);
        }

        public virtual void ListNames(IList<string> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            foreach (var name in Names)
                list.Add(name);
        }

        /// <summary>
        /// Overridden to return a JSON formatted object as a string.
        /// </summary>

        public override string ToString()
        {
            var writer = new StringWriter();
            Export(JsonText.CreateWriter(writer));
            return writer.ToString();
        }

        public void Export(JsonWriter writer)
        {
            Export(JsonConvert.CreateExportContext(), writer);
        }

        void IJsonExportable.Export(ExportContext context, JsonWriter writer)
        {
            Export(context, writer);
        }

        protected virtual void Export(ExportContext context, JsonWriter writer)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteStartObject();

            foreach (var member in this)
            {
                writer.WriteMember(member.Name);
                context.Export(member.Value, writer);
            }

            writer.WriteEndObject();
        }

        /// <remarks>
        /// This method is not exception-safe. If an error occurs while
        /// reading then the object may be partially imported.
        /// </remarks>

        public void Import(JsonReader reader)
        {
            Import(JsonConvert.CreateImportContext(), reader);
        }

        void IJsonImportable.Import(ImportContext context, JsonReader reader)
        {
            Import(context, reader);
        }

        protected virtual void Import(ImportContext context, JsonReader reader)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            // FIXME: Consider making this method exception-safe.
            // Right now this is a problem because of reliance on
            // DictionaryBase.

            Clear();

            reader.ReadToken(JsonTokenClass.Object);

            while (reader.TokenClass != JsonTokenClass.EndObject)
                Put(reader.ReadMember(), context.Import(reader));

            reader.Read();
        }

        protected void OnValidate(JsonMember member, string paramName)
        {
            if (member.Name == null)
                throw new ArgumentException(null, paramName);
        }

        protected override void InsertItem(int index, JsonMember item)
        {
            OnValidate(item, nameof(item));
            OnUpdating();
            base.InsertItem(index, item);
        }


        protected override void SetItem(int index, JsonMember item)
        {
            OnValidate(item, nameof(item));
            OnUpdating();
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            OnUpdating();
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            OnUpdating();
            base.ClearItems();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            if (!Contains(key))
            {
                value = null;
                return false;
            }

            value = this[key];
            return true;
        }

        ICollection<string> IDictionary<string, object>.Keys => Names;

        ReadOnlyCollection<T> GetMembers<T>(Func<JsonMember, T> selector)
        {
            var arr = new T[Count];
            var i = 0;
            foreach (var member in this)
                arr[i++] = selector(member);
            return Array.AsReadOnly(arr);
        }

        ICollection<object> IDictionary<string, object>.Values => CachedValues;

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return Contains(key);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            if (!Contains(key))
                return false;
            Remove(key);
            return true;
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            foreach (var member in this)
                yield return new KeyValuePair<string, object>(member.Name, member.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item);
        }

        bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>) this).TryGetValue(item.Key, out var value)
                && EqualityComparer<object>.Default.Equals(item.Value, value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, null);
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException(null, nameof(arrayIndex));

            var i = arrayIndex;
            foreach (var member in this)
                array[i++] = new KeyValuePair<string, object>(member.Name, member.Value);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            if (!Contains(item))
                return false;
            Remove(item.Key);
            return true;
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        void IDictionary.Add(object key, object value) =>
            Add(NameFromKeyObject(key), value);

        bool IDictionary.Contains(object key) =>
            Contains(NameFromKeyObject(key));

        void IDictionary.Remove(object key) =>
            Remove(NameFromKeyObject(key));

        string NameFromKeyObject(object key)
            => key == null ? throw new ArgumentNullException(nameof(key))
             : key is string name ? name
             : throw new ArgumentException(null, nameof(key));

        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;

        ICollection IDictionary.Keys => CachedKeys;
        ICollection IDictionary.Values => CachedValues;

        object IDictionary.this[object key]
        {
            get => this[NameFromKeyObject(key)];
            set => this[NameFromKeyObject(key)] = value;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() =>
            new DictionaryEnumerator(GetEnumerator());

        sealed class DictionaryEnumerator : IDictionaryEnumerator
        {
            readonly IEnumerator<JsonMember> _enumerator;

            public DictionaryEnumerator(IEnumerator<JsonMember> enumerator) =>
                _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));

            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
            public object Current => Entry;
            public DictionaryEntry Entry => new DictionaryEntry(_enumerator.Current.Name, _enumerator.Current.Value);
            public object Key => Entry.Key;
            public object Value => Entry.Value;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DynamicMetaObject<JsonObject>(parameter, this, Dor, /* dontFallbackFirst */ true);
        }

        static readonly DynamicObjectRuntime<JsonObject> Dor = new DynamicObjectRuntime<JsonObject>
        {
            TryGetMember = TryGetMember,
            TrySetMember = TrySetMember,
            TryInvokeMember = TryInvokeMember,
            TryDeleteMember = TryDeleteMember,
            GetDynamicMemberNames = o => o.Names
        };

        static Option<object> TryInvokeMember(JsonObject obj, InvokeMemberBinder arg2, object[] arg3)
        {
            return Option<object>.None; // TryGetMember(arg1, arg2)
        }

        static bool TryDeleteMember(JsonObject obj, DeleteMemberBinder binder)
        {
            if (!obj.Contains(binder.Name))
                return false;
            obj.Remove(binder.Name);
            return true;
        }

        static bool TrySetMember(JsonObject obj, SetMemberBinder binder, object value)
        {
            obj[binder.Name] = value;
            return true;
        }

        static Option<object> TryGetMember(JsonObject obj, GetMemberBinder binder)
        {
            if (!obj.HasMembers)
                return Option<object>.None;
            var name = binder.Name;
            var value = obj[name];
            if (value == null && !obj.Contains(name))
            {
                // TODO support case-insensitive bindings
                return Option<object>.None;
            }
            return Option.Value(value);
        }
    }
}

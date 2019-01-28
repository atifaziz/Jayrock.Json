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
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    using Jayrock.Dynamic;
    using Jayrock.Json.Conversion;

    #endregion

    /// <summary>
    /// An unordered collection of name/value pairs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Althought the collection should be considered unordered by the user,
    /// the implementation does internally try to remember the order in which
    /// the keys were added in order facilitate human-readability as in when
    /// an instance is rendered as text.</para>
    /// <para>
    /// Public Domain 2002 JSON.org, ported to C# by Are Bjolseth (teleplan.no)
    /// and re-adapted by Atif Aziz (www.raboof.com)</para>
    /// </remarks>

    [ Serializable ]
    public class JsonObject : DictionaryBase, IJsonImportable, IJsonExportable,
        IEnumerable<JsonMember>,
        IDictionary<string, object>,
        IDynamicMetaObjectProvider
    {
        ArrayList _nameIndexList;
        [ NonSerialized ] IList _readOnlyNameIndexList;

        [ NonSerialized ] string[] _keys;
        [ NonSerialized ] object[] _values;

        void OnUpdating()
        {
            _keys = null;
            _values = null;
        }

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

                InnerHashtable.Add(entry.Key.ToString(), entry.Value);
            }

            _nameIndexList = new ArrayList(members.Keys);
        }

        public JsonObject(string[] keys, object[] values)
        {
            var keyCount = keys == null ? 0 : keys.Length;
            var valueCount = values == null ? 0 : values.Length;
            var count = Math.Max(keyCount, valueCount);

            var key = string.Empty;

            for (var i = 0; i < count; i++)
            {
                if (i < keyCount)
                    key = Mask.NullString(keys[i]);

                Accumulate(key, i < valueCount ? values[i] : null);
            }
        }

        public virtual new JsonMemberEnumerator GetEnumerator()
        {
            return new JsonMemberEnumerator(this, NameIndexList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public JsonObject(IEnumerable<JsonMember> members)
        {
            if (members == null)
                return;

            foreach (var member in members)
                Accumulate(member.Name, member.Value);
        }

        IEnumerator<JsonMember> IEnumerable<JsonMember>.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Serializable]
        public sealed class JsonMemberEnumerator : IEnumerator<JsonMember>
        {
            JsonObject _obj;
            IEnumerator _enumerator;
            JsonMember _member;
            bool _memberInitialized;

            public JsonMemberEnumerator(JsonObject obj, IEnumerator enumerator)
            {
                Debug.Assert(obj != null);
                Debug.Assert(enumerator != null);

                _obj = obj;
                _enumerator = enumerator;
            }

            public bool MoveNext()
            {
                EnsureNotDisposed();
                ResetMember();
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                EnsureNotDisposed();
                ResetMember();
                _enumerator.Reset();
            }

            void ResetMember()
            {
                _member = new JsonMember();
                _memberInitialized = false;
            }

            public JsonMember Current
            {
                get
                {
                    EnsureNotDisposed();
                    if (!_memberInitialized)
                    {
                        var name = (string) _enumerator.Current;
                        _member = new JsonMember(name, _obj[name]);
                        _memberInitialized = true;
                    }
                    return _member;
                }
            }

            object IEnumerator.Current { get { return Current; } }

            void EnsureNotDisposed()
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }

            bool IsDisposed { get { return _enumerator == null; } }

            public void Dispose()
            {
                if (IsDisposed)
                    return;

                _obj = null;
                _enumerator = null;
                ResetMember();
            }
        }

        public virtual object this[string key]
        {
            get { return InnerHashtable[key]; }
            set { Put(key, value); }
        }

        public virtual bool HasMembers
        {
            get { return Count > 0; }
        }

        ArrayList NameIndexList
        {
            get
            {
                if (_nameIndexList == null)
                    _nameIndexList = new ArrayList();

                return _nameIndexList;
            }
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

            var current = InnerHashtable[name];

            if (current == null)
            {
                Put(name, value);
            }
            else
            {
                var values = current as IList;

                if (values != null)
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

            Dictionary.Add(name, value);
        }

        /// <summary>
        /// Put a key/value pair in the JsonObject. If the value is null,
        /// then the key will be removed from the JsonObject if it is present.
        /// </summary>

        public virtual void Put(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Dictionary[name] = value;
        }

        public virtual bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return Dictionary.Contains(name);
        }

        public virtual void Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Dictionary.Remove(name);
        }

        public virtual ICollection Names
        {
            get
            {
                if (_readOnlyNameIndexList == null)
                    _readOnlyNameIndexList = ArrayList.ReadOnly(NameIndexList);

                return _readOnlyNameIndexList;
            }
        }

        /// <summary>
        /// Produce a JsonArray containing the names of the elements of this
        /// JsonObject.
        /// </summary>

        public virtual JsonArray GetNamesArray()
        {
            var names = new JsonArray();
            ListNames(names);
            return names;
        }

        public virtual void ListNames(IList list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            foreach (string name in NameIndexList)
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

            foreach (string name in NameIndexList)
            {
                writer.WriteMember(name);
                context.Export(InnerHashtable[name], writer);
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

        protected override void OnValidate(object key, object value)
        {
            base.OnValidate(key, value);

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (!(key is string))
                throw new ArgumentException(string.Format("The key cannot be of the supplied type {0}. It must be typed System.String.", key.GetType().FullName), nameof(key));
        }

        protected override void OnInsert(object key, object value)
        {
            OnUpdating();

            //
            // NOTE: OnInsert leads one to believe that keys are ordered in the
            // base dictionary in that they can be inserted somewhere in the
            // middle. However, the base implementation only calls OnInsert
            // during the Add operation, so we known it is safe here to simply
            // add the new key at the end of the name list.
            //

            NameIndexList.Add(key);
        }

        protected override void OnSet(object key, object oldValue, object newValue)
        {
            OnUpdating();

            //
            // NOTE: OnSet is called when the base dictionary is modified via
            // the indexer. We need to trap this and detect when a new key is
            // being added via the indexer. If the old value is null for the
            // key, then there is a big chance it is a new key. But just to be
            // sure, we also check out key index if it does not already exist.
            // Finally, we just delegate to OnInsert. In effect, we're
            // converting OnSet to OnInsert where needed. Ideally, the base
            // implementation would have done this for.
            //

            if (oldValue == null && !NameIndexList.Contains(key))
                OnInsert(key, newValue);
        }

        protected override void OnRemove(object key, object value)
        {
            OnUpdating();
            NameIndexList.Remove(key);
        }

        protected override void OnClear()
        {
            OnUpdating();
            NameIndexList.Clear();
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

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return _keys ?? (_keys = GetMembers(m => m.Name)); }
        }

        T[] GetMembers<T>(Func<JsonMember, T> selector)
        {
            var arr = new T[Count];
            var i = 0;
            foreach (var member in this)
                arr[i++] = selector(member);
            return arr;
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return _values ?? (_values = GetMembers(m => m.Value)); }
        }

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
            object value;
            return ((IDictionary<string, object>) this).TryGetValue(item.Key, out value)
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

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return InnerHashtable.IsReadOnly; }
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DynamicMetaObject<JsonObject>(parameter, this, _runtime, /* dontFallbackFirst */ true);
        }

        readonly DynamicObjectRuntime<JsonObject> _runtime = new DynamicObjectRuntime<JsonObject>
        {
            TryGetMember = TryGetMember,
            TrySetMember = TrySetMember,
            TryInvokeMember = TryInvokeMember,
            TryDeleteMember = TryDeleteMember,
            GetDynamicMemberNames = o => o.Names.Cast<string>()
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

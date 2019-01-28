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

namespace Jayrock.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class KeyedCollection<TKey, TValue> :
        System.Collections.ObjectModel.KeyedCollection<TKey, TValue>
    {
        protected IEnumerable<TKey> KeysByIndex =>
            from item in Items
            select GetKeyForItem(item);

        public void Put(TValue value)
        {
            var key = GetKeyForItem(value);
            if (Dictionary is IDictionary<TKey, TValue> dict
                && !dict.ContainsKey(key))
            {
                Add(value);
                return;
            }

            for (var i = 0; i < Count; i++)
            {
                if (Comparer.Equals(GetKeyForItem(Items[i]), key))
                {
                    SetItem(i, value);
                    return;
                }
            }

            Add(value);
        }

        protected override void SetItem(int index, TValue item) =>
            base.SetItem(index, ValidateItem(item));

        protected override void InsertItem(int index, TValue item) =>
            base.InsertItem(index, ValidateItem(item));

        private TValue ValidateItem(TValue item)
            => GetKeyForItem(item) == null
             ? throw new ArgumentException(null, nameof(item))
             : item;

        public new TValue this[TKey key]
            => key == null
             ? throw new ArgumentNullException(nameof(key))
             : Dictionary is IDictionary<TKey, TValue> dict
               && dict.TryGetValue(key, out var value)
             ? value
             : Items.FirstOrDefault(item => Comparer.Equals(key, GetKeyForItem(item)));
    }
}

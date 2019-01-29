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
    using System;

    public static class CompatibilityExtensions
    {
        //
        // Methods that imitate the JavaScript array methods.
        //

        /// <summary>
        /// Appends new elements to an array.
        /// </summary>
        /// <returns>
        /// The new length of the array.
        /// </returns>
        /// <remarks>
        /// This method appends elements in the order in which they appear. If
        /// one of the arguments is an array, it is added as a single element.
        /// Use the <see cref="Concat"/> method to join the elements from two or
        /// more arrays.
        /// </remarks>

        public static int Push(JsonArray array, object value)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            array.Add(value);
            return array.Count;
        }

        /// <summary>
        /// Appends new elements to an array.
        /// </summary>
        /// <returns>
        /// The new length of the array.
        /// </returns>
        /// <remarks>
        /// This method appends elements in the order in which they appear. If
        /// one of the arguments is an array, it is added as a single element.
        /// Use the <see cref="Concat"/> method to join the elements from two or
        /// more arrays.
        /// </remarks>

        public static int Push(JsonArray array, params object[] values)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (values != null)
            {
                foreach (var value in values)
                    Push(array, value);
            }

            return array.Count;
        }

        /// <summary>
        /// Removes the last element from an array and returns it.
        /// </summary>
        /// <remarks>
        /// If the array is empty, null is returned.
        /// </remarks>

        public static object Pop(JsonArray array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (array.Count == 0)
                return null;

            var lastValue = array[array.Count - 1];
            array.RemoveAt(array.Count - 1);
            return lastValue;
        }

        /// <summary>
        /// Returns a new array consisting of a combination of two or more
        /// arrays.
        /// </summary>

        public static JsonArray Concat(JsonArray array, params object[] values)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            var newArray = new JsonArray(array);

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value is JsonArray arrayValue)
                    {
                        foreach (var arrayValueValue in arrayValue)
                            Push(newArray, arrayValueValue);
                    }
                    else
                    {
                        Push(newArray, value);
                    }
                }
            }

            return newArray;
        }

        /// <summary>
        /// Removes the first element from an array and returns it.
        /// </summary>

        public static object Shift(JsonArray array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (array.Count == 0)
                return null;

            var firstValue = array[0];
            array.RemoveAt(0);
            return firstValue;
        }

        /// <summary>
        /// Inserts the specified element at the beginning of the array.
        /// </summary>
        /// <remarks>
        /// The unshift method inserts elements into the start of an array, so
        /// they appear in the same order in which they appear in the argument
        /// list.
        /// </remarks>

        public static void Unshift(JsonArray array, object value)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            array.Insert(0, value);
        }

        /// <summary>
        /// Inserts the specified elements at the beginning of the array.
        /// </summary>
        /// <remarks>
        /// The unshift method inserts elements into the start of an array, so
        /// they appear in the same order in which they appear in the argument
        /// list.
        /// </remarks>

        public static void Unshift(JsonArray array, params object[] values)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (values != null)
            {
                foreach (var value in values)
                    Unshift(array, value);
            }
        }

    }
}

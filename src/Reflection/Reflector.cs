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

namespace Jayrock.Reflection
{
    using System;

    static class Reflector
    {
        /// <summary>
        /// Determines if type is a constructed type of <see cref="System.Nullable{T}"/>.
        /// </summary>
        ///

        // Source Mannex: http://mannex.googlecode.com
        // License: http://code.google.com/p/mannex/wiki/License

        public static bool IsConstructionOfNullable(Type type)
        {
            return IsConstructionOfGenericTypeDefinition(type, typeof(Nullable<>));
        }

        /// <summary>
        /// Determines if type is a constructed type of generic type definition.
        /// For example, this method can be used to test if <see cref="System.Nullable{T}"/>
        /// of <see cref="int" /> is indeed a construction of the generic type definition
        /// <see cref="System.Nullable{T}"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Either <paramref name="type"/> or <paramref name="genericTypeDefinition"/>
        /// is a null reference.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The type identified by <paramref name="genericTypeDefinition"/> is not
        /// a generic type definition.
        /// </exception>

        // Source Mannex: http://mannex.googlecode.com
        // License: http://code.google.com/p/mannex/wiki/License

        internal static bool IsConstructionOfGenericTypeDefinition(Type type, Type genericTypeDefinition)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException(string.Format("{0} is not a generic type definition.", genericTypeDefinition), nameof(genericTypeDefinition));

            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        /// <summary>
        /// Finds and returns the constructed type of an interface
        /// generic type definition if the type implements it. Otherwise
        /// returns a <c>null</c> reference.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Either <paramref name="type"/> or <paramref name="genericTypeDefinition"/>
        /// is a null reference.
        /// </exception>

        internal static Type FindConstructionOfGenericInterfaceDefinition(Type type, Type genericTypeDefinition)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));

            var interfaces = type.FindInterfaces(IsConstructionOfGenericInterfaceDefinition, genericTypeDefinition);
            return interfaces.Length == 0 ? null : interfaces[0];
        }

        static bool IsConstructionOfGenericInterfaceDefinition(Type type, object criteria)
        {
            return IsConstructionOfGenericTypeDefinition(type, (Type) criteria);
        }

        static readonly Type[] CommonTupleTypes =
        {
            // Tuple of 1 not expected to be common so excluded from here
            typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>)
        };

        /// <summary>
        /// Determines if a type is one of the generic <see cref="System.Tuple"/> family
        /// of types.
        /// </summary>

        public static bool IsTupleFamily(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType || type.IsGenericTypeDefinition)
                return false;

            //
            // Quick check against common generic type definitions
            //

            if (Array.IndexOf(CommonTupleTypes, type.GetGenericTypeDefinition()) >= 0)
                return true;

            //
            // Slower check for less common cases like tuple of 1 or
            // just way too many items.
            //

            var someTupleType = CommonTupleTypes[0];
            const char tick = '`';
            var i = type.FullName.IndexOf(tick);
            return type.Assembly == someTupleType.Assembly
                && i == someTupleType.FullName.IndexOf(tick)
                && 0 == string.CompareOrdinal(someTupleType.FullName, 0, type.FullName, 0, i);
        }

        static readonly Type[] CommonValueTupleTypes =
        {
            // Tuple of 1 not expected to be common so excluded from here
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>)
        };

        /// <summary>
        /// Determines if a type is one of the generic <see cref="System.Tuple"/> family
        /// of types.
        /// </summary>

        public static bool IsValueTupleFamily(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType || type.IsGenericTypeDefinition)
                return false;

            //
            // Quick check against common generic type definitions
            //

            if (Array.IndexOf(CommonValueTupleTypes, type.GetGenericTypeDefinition()) >= 0)
                return true;

            //
            // Slower check for less common cases like tuple of 1 or
            // just way too many items.
            //

            var someTupleType = CommonValueTupleTypes[0];
            const char tick = '`';
            var i = type.FullName.IndexOf(tick);
            return type.Assembly == someTupleType.Assembly
                && i == someTupleType.FullName.IndexOf(tick)
                && 0 == string.CompareOrdinal(someTupleType.FullName, 0, type.FullName, 0, i);
        }
    }
}

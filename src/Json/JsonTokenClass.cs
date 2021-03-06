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
    using System.Runtime.Serialization;

    #endregion

    [ Serializable ]
    public sealed class JsonTokenClass : IObjectReference
    {
        public static readonly JsonTokenClass Null = new JsonTokenClass("Null");
        public static readonly JsonTokenClass Boolean = new JsonTokenClass("Boolean", Superclass.Scalar);
        public static readonly JsonTokenClass Number = new JsonTokenClass("Number", Superclass.Scalar);
        public static readonly JsonTokenClass String = new JsonTokenClass("String", Superclass.Scalar);
        public static readonly JsonTokenClass Array = new JsonTokenClass("Array");
        public static readonly JsonTokenClass EndArray = new JsonTokenClass("EndArray", Superclass.Terminator);
        public static readonly JsonTokenClass Object = new JsonTokenClass("Object");
        public static readonly JsonTokenClass EndObject = new JsonTokenClass("EndObject", Superclass.Terminator);
        public static readonly JsonTokenClass Member = new JsonTokenClass("Member");
        public static readonly JsonTokenClass BOF = new JsonTokenClass("BOF", Superclass.Terminator);
        public static readonly JsonTokenClass EOF = new JsonTokenClass("EOF", Superclass.Terminator);

        public static readonly IReadOnlyCollection<JsonTokenClass>
            All = System.Array.AsReadOnly(new[]
            {
                BOF, EOF,
                Null, Boolean, Number, String,
                Array, EndArray,
                Object, EndObject, Member
            });

        [ NonSerialized ] readonly Superclass _superclass;

        enum Superclass
        {
            Unspecified,
            Scalar,
            Terminator
        }

        JsonTokenClass(string name, Superclass superclass = Superclass.Unspecified)
        {
            Debug.Assert(name != null);
            Debug.Assert(name.Length > 0);

            Name = name;
            _superclass = superclass;
        }

        public string Name { get; }

        internal bool IsTerminator => _superclass == Superclass.Terminator;

        internal bool IsScalar => _superclass == Superclass.Scalar;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        object IObjectReference.GetRealObject(StreamingContext context)
        {
            foreach (var clazz in All)
            {
                if (string.CompareOrdinal(clazz.Name, Name) == 0)
                    return clazz;
            }

            throw new SerializationException(string.Format("{0} is not a valid {1} instance.", Name, typeof(JsonTokenClass).FullName));
        }
    }
}

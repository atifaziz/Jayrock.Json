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

namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    #endregion

    public sealed class ObjectConstructor : IObjectConstructor
    {
        readonly Type _type;
        readonly ConstructorInfo[] _ctors;

        static readonly IComparer<ConstructorInfo>
            ArrayLengthComparer =
                Comparer<ConstructorInfo>.Create((a, b) =>
                    -1 * a.GetParameters().Length.CompareTo(b.GetParameters().Length));

        public ObjectConstructor(Type type) : this(type, null) {}

        public ObjectConstructor(Type type, ConstructorInfo[] ctors)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (ctors == null)
            {
                ctors = type.GetConstructors();
            }
            else
            {
                foreach (var ctor in ctors)
                {
                    if (ctor.DeclaringType != type)
                        throw new ArgumentException(null, nameof(ctors));
                }

                ctors = (ConstructorInfo[]) ctors.Clone();
            }

            if (type.IsClass && ctors.Length == 0)
            {
                //
                // Value types are excluded here because they always have
                // a default constructor available but one which does not
                // show up in reflection.
                //

                throw new ArgumentException(null, nameof(ctors));
            }

            _type = type;
            _ctors = ctors;
            Array.Sort(_ctors, ArrayLengthComparer);
        }

        public ObjectConstructionResult CreateObject(ImportContext context, JsonReader reader)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            return CreateObject(context, JsonBuffer.From(reader).GetMembersArray());
        }

        public ObjectConstructionResult CreateObject(ImportContext context, NamedJsonBuffer[] members)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (members == null) throw new ArgumentNullException(nameof(members));

            if (_ctors.Length > 0)
            {
                foreach (var ctor in _ctors)
                {
                    var result = TryCreateObject(context, ctor, members);
                    if (result != null)
                        return result;
                }
            }

            if (_type.IsValueType)
            {
                //
                // Value types always have a default constructor available
                // but one which does not show up in reflection. If no other
                // constructors matched then just use the default one.
                //

                var obj = Activator.CreateInstance(_type);
                JsonReader tail = NamedJsonBuffer.ToObject(members).CreateReader();
                return new ObjectConstructionResult(obj, tail);
            }

            throw new JsonException(string.Format("None constructor could be used to create {0} object from JSON.", _ctors[0].DeclaringType));
        }

        static ObjectConstructionResult TryCreateObject(ImportContext context, ConstructorInfo ctor, NamedJsonBuffer[] members)
        {
            Debug.Assert(context != null);
            Debug.Assert(ctor != null);
            Debug.Assert(members != null);

            var parameters = ctor.GetParameters();

            if (parameters.Length > members.Length)
                return null;

            var bindings = Bind(context, parameters, members);

            var argc = 0;
            object[] args = null;
            JsonBufferWriter tailw = null;

            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i] - 1;

                if (binding >= 0)
                {
                    if (args == null)
                        args = new object[parameters.Length];

                    var type = parameters[binding].ParameterType;
                    var arg = members[i].Buffer;
                    args[binding] = context.Import(type, arg.CreateReader());
                    argc++;
                }
                else
                {
                    if (tailw == null)
                    {
                        tailw = new JsonBufferWriter();
                        tailw.WriteStartObject();
                    }

                    var member = members[i];
                    tailw.WriteMember(member.Name);
                    tailw.WriteFromReader(member.Buffer.CreateReader());
                }
            }

            tailw?.WriteEndObject();

            if (argc != parameters.Length)
                return null;

            var obj = ctor.Invoke(args);

            var tail = tailw?.GetBuffer() ?? StockJsonBuffers.EmptyObject;

            return new ObjectConstructionResult(obj, tail.CreateReader());
        }

        /// <remarks>
        /// Bound indicies returned in the resulting array are one-based
        /// therefore zero means unbound.
        /// </remarks>
        static int[] Bind(ImportContext context, ParameterInfo[] parameters, NamedJsonBuffer[] members)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (members == null) throw new ArgumentNullException(nameof(members));

            var bindings = new int[members.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter == null)
                    throw new ArgumentException(null, nameof(parameters));

                var mi = FindMember(members, parameter.Name);

                if (mi >= 0)
                    bindings[mi] = i + 1;
            }

            return bindings;
        }

        static int FindMember(NamedJsonBuffer[] members, string name)
        {
            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];

                if (member.IsEmpty)
                    throw new ArgumentException(null, nameof(members));

                if (0 == CultureInfo.InvariantCulture.CompareInfo.Compare(name, member.Name, CompareOptions.IgnoreCase))
                    return i;
            }

            return -1;
        }
    }
}

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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Jayrock.Reflection;

    #endregion

    public abstract class TupleExporterBase : ExporterBase
    {
        readonly Action<ExportContext, object, JsonWriter> _exporter;

        protected internal TupleExporterBase(Type inputType, Action<ExportContext, object, JsonWriter> exporter)
            : base(inputType)
        {
            _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        }

        protected override void ExportValue(ExportContext context, object value, JsonWriter writer)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStartArray();
            _exporter(context, value, writer);
            writer.WriteEndArray();
        }
    }

    public class TupleExporter : TupleExporterBase
    {
        public TupleExporter(Type inputType)
            : base(Reflector.IsTupleFamily(inputType) ? inputType : throw new ArgumentException(null, nameof(inputType)),
                   TupleExporterCompiler.Compile(inputType, inputType.GetProperties())) {}
    }

    public class ValueTupleExporter : TupleExporterBase
    {
        public ValueTupleExporter(Type inputType)
            : base(Reflector.IsValueTupleFamily(inputType) ? inputType : throw new ArgumentException(null, nameof(inputType)),
                   TupleExporterCompiler.Compile(inputType, inputType.GetFields())) {}
    }

    static class TupleExporterCompiler
    {
        static readonly MethodInfo ExportMethod = ((MethodCallExpression) ((Expression<Action<ExportContext>>) (context => context.Export(null, null))).Body).Method;

        public static Action<ExportContext, object, JsonWriter> Compile<T>(Type tupleType, IEnumerable<T> members)
            where T : MemberInfo
        {
            Debug.Assert(tupleType != null);

            var context = Expression.Parameter(typeof(ExportContext), "context");
            var obj     = Expression.Parameter(typeof(object), "obj");
            var writer  = Expression.Parameter(typeof(JsonWriter), "writer");

            var tuple   = Expression.Variable(tupleType, "tuple");
            var body    = Expression.Block
                          (
                              new[] { tuple },
                              new[] { Expression.Assign(tuple, Expression.Convert(obj, tupleType)) } /* ...
                              ... */ .Concat(CreateItemExportCallExpressions())
                          );

            var lambda  = Expression.Lambda<Action<ExportContext, object, JsonWriter>>(body, context, obj, writer);
            return lambda.Compile();

            IEnumerable<Expression> CreateItemExportCallExpressions()
            {
                Debug.Assert(context != null);
                Debug.Assert(tuple != null);
                Debug.Assert(writer != null);

                //
                // Suppose type of tuple is Tuple<int, string, DateTime>, return
                // call expressions like this:
                //
                //  context.Export((object) tuple.Item1, writer);
                //  context.Export((object) tuple.Item2, writer);
                //  context.Export((object) tuple.Item3, writer);
                //

                return from property in members
                       select Expression.Call
                       (
                           context, ExportMethod,
                               Expression.Convert(Expression.MakeMemberAccess(tuple, property), typeof(object)),
                               writer
                       );
            }
        }
    }
}

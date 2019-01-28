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
    using System.Collections;
    using System.Diagnostics;
    using Jayrock.Json.Conversion.Converters;
    using Jayrock.Reflection;

    #endregion

    [ Serializable ]
    public class ExportContext
    {
        ExporterCollection _exporters;
        IDictionary _items;

        static ExporterCollection _stockExporters;

        public virtual void Export(object value, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (JsonNull.LogicallyEquals(value))
            {
                writer.WriteNull();
            }
            else
            {
                var exporter = FindExporter(value.GetType());

                if (exporter != null)
                    exporter.Export(this, value, writer);
                else
                    writer.WriteString(value.ToString());
            }
        }

        public virtual void Register(IExporter exporter)
        {
            if (exporter == null)
                throw new ArgumentNullException(nameof(exporter));

            Exporters.Put(exporter);
        }

        public virtual IExporter FindExporter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var exporter = Exporters[type];

            if (exporter != null)
                return exporter;

            exporter = StockExporters[type] ?? FindCompatibleExporter(type);

            if (exporter != null)
            {
                Register(exporter);
                return exporter;
            }

            return null;
        }

        public IDictionary Items => _items ?? (_items = new Hashtable());

        IExporter FindCompatibleExporter(Type type)
        {
            Debug.Assert(type != null);

            if (typeof(IJsonExportable).IsAssignableFrom(type))
                return new ExportAwareExporter(type);

            if (Reflector.IsConstructionOfNullable(type))
                return new NullableExporter(type);

            if (Reflector.IsTupleFamily(type))
                return new TupleExporter(type);

            if (type.IsClass && type != typeof(object))
            {
                var exporter = FindBaseExporter(type.BaseType, type);
                if (exporter != null)
                    return exporter;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
                return new DictionaryExporter(type);

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return new EnumerableExporter(type);

            if ((type.IsPublic || type.IsNestedPublic) &&
                !type.IsPrimitive && !type.IsEnum &&
                (type.IsValueType || type.GetConstructors().Length > 0))
            {
                return new ComponentExporter(type);
            }

            var anonymousClass = CustomTypeDescriptor.TryCreateForAnonymousClass(type);
            if (anonymousClass != null)
                return new ComponentExporter(type, anonymousClass);

            return new StringExporter(type);
        }

        IExporter FindBaseExporter(Type baseType, Type actualType)
        {
            Debug.Assert(baseType != null);
            Debug.Assert(actualType != null);

            if (baseType == typeof(object))
                return null;

            var exporter = Exporters[baseType];

            if (exporter == null)
            {
                exporter = StockExporters[baseType];

                if (exporter == null)
                    return FindBaseExporter(baseType.BaseType, actualType);
            }

            return (IExporter) Activator.CreateInstance(exporter.GetType(), new object[] { actualType });
        }

        ExporterCollection Exporters => _exporters ?? (_exporters = new ExporterCollection());

        static ExporterCollection StockExporters
        {
            get
            {
                if (_stockExporters == null)
                {
                    var exporters = new ExporterCollection
                    {
                        new ByteExporter(),
                        new Int16Exporter(),
                        new Int32Exporter(),
                        new Int64Exporter(),
                        new SingleExporter(),
                        new DoubleExporter(),
                        new DecimalExporter(),
                        new StringExporter(),
                        new BooleanExporter(),
                        new DateTimeExporter(),
                        new JsonNumberExporter(),
                        new JsonBufferExporter(),
                        new ByteArrayExporter(),
                        new DataRowViewExporter(),
                        new NameValueCollectionExporter(),
                        new DataSetExporter(),
                        new DataTableExporter(),
                        new DataRowExporter(),
                        new DbDataRecordExporter(),
                        new StringExporter(typeof(Uri)),
                        new StringExporter(typeof(Guid)),
                        new BigIntegerExporter(),
                        new ExpandoObjectExporter()
                    };

                    IList typeList = null; // TODO (IList)ConfigurationSettings.GetConfig("jayrock/json.conversion.exporters");

                    if (typeList != null && typeList.Count > 0)
                    {
                        foreach (Type type in typeList)
                            exporters.Put((IExporter) Activator.CreateInstance(type));
                    }

                    _stockExporters = exporters;
                }

                return _stockExporters;
            }
        }
    }
}

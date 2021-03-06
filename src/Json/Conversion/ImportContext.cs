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
    using System.Collections.Generic;
    using System.Diagnostics;
    using Jayrock.Json.Conversion.Converters;
    using Jayrock.Reflection;

    #endregion

    [ Serializable ]
    public class ImportContext
    {
        ImporterCollection _importers;
        IDictionary _items;

        static ImporterCollection _stockImporters;

        public virtual object Import(JsonReader reader)
        {
            return Import(AnyType.Value, reader);
        }

        public virtual object Import(Type type, JsonReader reader)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var importer = FindImporter(type);

            if (importer == null)
                throw new JsonException(string.Format("Don't know how to import {0} from JSON.", type.FullName));

            reader.MoveToContent();
            return importer.Import(this, reader);
        }

        public virtual T Import<T>(JsonReader reader)
        {
            return (T) Import(typeof(T), reader);
        }

        public virtual void Register(IImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException(nameof(importer));

            Importers.Put(importer);
        }

        public virtual IImporter FindImporter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var importer = Importers[type];

            if (importer != null)
                return importer;

            importer = StockImporters[type] ?? FindCompatibleImporter(type);

            if (importer != null)
            {
                Register(importer);
                return importer;
            }

            return null;
        }

        public IDictionary Items => _items ?? (_items = new Hashtable());

        static IImporter FindCompatibleImporter(Type type)
        {
            Debug.Assert(type != null);

            if (typeof(IJsonImportable).IsAssignableFrom(type))
                return new ImportAwareImporter(type);

            if (type.IsArray && type.GetArrayRank() == 1)
                return new ArrayImporter(type);

            if (type.IsEnum)
                return new EnumImporter(type);

            if (Reflector.IsConstructionOfNullable(type))
                return new NullableImporter(type);

            var isGenericList = Reflector.IsConstructionOfGenericTypeDefinition(type, typeof(IList<>));
            var isGenericCollection = !isGenericList && Reflector.IsConstructionOfGenericTypeDefinition(type, typeof(ICollection<>));
            var isSequence = !isGenericCollection && (type == typeof(IEnumerable) || Reflector.IsConstructionOfGenericTypeDefinition(type, typeof(IEnumerable<>)));

            if (isGenericList || isGenericCollection || isSequence)
            {
                var itemType = type.IsGenericType
                              ? type.GetGenericArguments()[0]
                              : typeof(object);
                var importerType = typeof(CollectionImporter<,>).MakeGenericType(type, itemType);
                return (IImporter) Activator.CreateInstance(importerType, new object[] { isSequence });
            }

            if (Reflector.IsConstructionOfGenericTypeDefinition(type, typeof(IDictionary<,>)))
                return (IImporter) Activator.CreateInstance(typeof(DictionaryImporter<,>).MakeGenericType(type.GetGenericArguments()));

            var genericDictionaryType = Reflector.FindConstructionOfGenericInterfaceDefinition(type, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var args2 = genericDictionaryType.GetGenericArguments();
                Debug.Assert(args2.Length == 2);
                var args3 = new Type[3];
                args3[0] = type;        // [ TDictionary, ... , ...    ]
                args2.CopyTo(args3, 1); // [ TDictionary, TKey, TValue ]
                return (IImporter)Activator.CreateInstance(typeof(DictionaryImporter<,,>).MakeGenericType(args3));
            }

            if (Reflector.IsConstructionOfGenericTypeDefinition(type, typeof(ISet<>)))
            {
                var typeArguments = type.GetGenericArguments();
                var hashSetType = typeof(HashSet<>).MakeGenericType(typeArguments);
                return (IImporter)Activator.CreateInstance(typeof(CollectionImporter<,,>).MakeGenericType(hashSetType, type, typeArguments[0]));
            }

            if (Reflector.IsValueTupleFamily(type))
                return new ValueTupleImporter(type);

            if (Reflector.IsTupleFamily(type))
                return new TupleImporter(type);

            if ((type.IsPublic || type.IsNestedPublic) &&
                !type.IsPrimitive &&
                (type.IsValueType || type.GetConstructors().Length > 0))
            {
                return new ComponentImporter(type, new ObjectConstructor(type));
            }

            var anonymousClass = CustomTypeDescriptor.TryCreateForAnonymousClass(type);
            if (anonymousClass != null)
                return new ComponentImporter(type, anonymousClass, new ObjectConstructor(type));

            return null;
        }

        ImporterCollection Importers => _importers ?? (_importers = new ImporterCollection());

        static ImporterCollection StockImporters
        {
            get
            {
                if (_stockImporters == null)
                {
                    var importers = new ImporterCollection
                    {
                        new ByteImporter(),
                        new Int16Importer(),
                        new Int32Importer(),
                        new Int64Importer(),
                        new SingleImporter(),
                        new DoubleImporter(),
                        new DecimalImporter(),
                        new StringImporter(),
                        new BooleanImporter(),
                        new DateTimeImporter(),
                        new GuidImporter(),
                        new UriImporter(),
                        new ByteArrayImporter(),
                        new AnyImporter(),
                        new DictionaryImporter(),
                        new ListImporter(),
                        new NameValueCollectionImporter(),
                        new JsonBufferImporter(),
                        new BigIntegerImporter(),
                        new ExpandoObjectImporter()
                    };

                    IList typeList = null; // TODO (IList) ConfigurationSettings.GetConfig("jayrock/json.conversion.importers");

                    if (typeList != null && typeList.Count > 0)
                    {
                        foreach (Type type in typeList)
                            importers.Put((IImporter) Activator.CreateInstance(type));
                    }

                    _stockImporters = importers;
                }

                return _stockImporters;
            }
        }
    }
}

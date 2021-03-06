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
    using System.ComponentModel;
    using System.Diagnostics;
    using CustomTypeDescriptor = Conversion.CustomTypeDescriptor;

    #endregion

    public sealed class ComponentImporter : ImporterBase
    {
        readonly PropertyDescriptorCollection _properties; // TODO: Review thread-safety of PropertyDescriptorCollection
        readonly IObjectMemberImporter[] _importers;
        readonly IObjectConstructor _constructor;

        public ComponentImporter(Type type) :
            this(type, null, null) {}

        public ComponentImporter(Type type, ICustomTypeDescriptor typeDescriptor) :
            this(type, typeDescriptor, null) {}

        public ComponentImporter(Type type, IObjectConstructor constructor) :
            this(type, null, constructor) {}

        public ComponentImporter(Type type, ICustomTypeDescriptor typeDescriptor, IObjectConstructor constructor) :
            base(type)
        {
            if (typeDescriptor == null)
                typeDescriptor = new CustomTypeDescriptor(type);

            var count = 0;
            var properties = typeDescriptor.GetProperties();
            var importers = new IObjectMemberImporter[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                var sp = properties[i] as IServiceProvider;

                var importer = (IObjectMemberImporter) sp?.GetService(typeof(IObjectMemberImporter));

                if (importer == null)
                    continue;

                importers[i] = importer;
                count++;
            }

            _properties = properties;

            if (count > 0)
                _importers = importers;

            _constructor = constructor;
        }

        protected override object ImportFromObject(ImportContext context, JsonReader reader)
        {
            Debug.Assert(context != null);
            Debug.Assert(reader != null);

            object obj;

            if (_constructor != null)
            {
                var result = _constructor.CreateObject(context, reader);
                obj = result.Object;
                reader = result.TailReader;
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.Read();
                obj = Activator.CreateInstance(OutputType);
            }

            var otherImporter = obj as INonObjectMemberImporter;

            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                var memberName = reader.ReadMember();

                var property = _properties.Find(memberName, true);

                //
                // Skip over the member value and continue with reading if
                // the property was not found or if the property found cannot
                // be set.
                //

                if (property == null || property.IsReadOnly)
                {
                    if (otherImporter == null || !otherImporter.Import(context, memberName, reader))
                        reader.Skip();
                    continue;
                }

                //
                // Check if the property defines a custom import scheme.
                // If yes, ask it to import the value into the property
                // and then continue with the next.
                //

                if (_importers != null)
                {
                    var index = _properties.IndexOf(property);

                    var importer = _importers[index];

                    if (importer != null)
                    {
                        importer.Import(context, reader, obj);
                        continue;
                    }
                }

                //
                // Import from reader based on the property type and
                // then set the value of the property.
                //

                property.SetValue(obj, context.Import(property.PropertyType, reader));
            }

            return ReadReturning(reader, obj);
        }
    }
}

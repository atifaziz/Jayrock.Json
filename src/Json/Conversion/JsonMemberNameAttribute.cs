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
    using System.ComponentModel;

    #endregion

    [ Serializable ]
    [ AttributeUsage(AttributeTargets.Property | AttributeTargets.Field) ]
    public class JsonMemberNameAttribute : Attribute, IPropertyDescriptorCustomization
    {
        string _name;

        public JsonMemberNameAttribute() {}

        public JsonMemberNameAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get => Mask.NullString(_name);
            set => _name = value;
        }

        void IPropertyDescriptorCustomization.Apply(PropertyDescriptor property)
        {
            ApplyCustomization(property);
        }

        protected virtual void ApplyCustomization(PropertyDescriptor property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (Name.Length == 0)
                return;

            var customization = (IPropertyCustomization)property;
            customization.SetName(Name);
        }
    }
}

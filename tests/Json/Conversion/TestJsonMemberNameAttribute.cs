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
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonMemberNameAttribute
    {
        [ Test ]
        public void IsSerializable()
        {
            Assert.IsTrue(typeof(JsonMemberNameAttribute).IsSerializable);
        }

        [ Test ]
        public void DefaultInitializationYieldsEmptyName()
        {
            var attribute = new JsonMemberNameAttribute();
            Assert.IsNotNull(attribute.Name);
            Assert.AreEqual(string.Empty, attribute.Name);
        }

        [ Test ]
        public void InitializingNullNameYieldsEmptyName()
        {
            var attribute = new JsonMemberNameAttribute(null);
            Assert.IsNotNull(attribute.Name);
            Assert.AreEqual(string.Empty, attribute.Name);
        }

        [ Test ]
        public void InitializeName()
        {
            var attribute = new JsonMemberNameAttribute("name");
            Assert.AreEqual("name", attribute.Name);
        }

        [ Test ]
        public void SetName()
        {
            var attribute = new JsonMemberNameAttribute();
            Assert.AreEqual(string.Empty, attribute.Name);
            attribute.Name = "foo";
            Assert.AreEqual("foo", attribute.Name);
        }

        [ Test ]
        public void PropertyDescriptorNameCustomization()
        {
            var property = CreateTestProperty("foo");
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute("bar");
            customization.Apply(property);
            Assert.AreEqual("bar", property.CustomizedName);
        }

        [ Test ]
        public void PropertyDescriptorNameCustomizationSkippedOnEmptyName()
        {
            var property = CreateTestProperty("foo");
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute();
            customization.Apply(property);
            Assert.IsNull(property.CustomizedName);
        }

        [ Test ]
        [ ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotApplyToNullPropertyDescriptor()
        {
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute();
            customization.Apply(null);
        }

        static TestPropertyDescriptor CreateTestProperty(string baseName)
        {
            var property = new TestPropertyDescriptor(baseName);
            Assert.AreEqual(baseName, property.Name);
            Assert.IsNull(property.CustomizedName);
            return property;
        }

        sealed class TestPropertyDescriptor : PropertyDescriptor, IPropertyCustomization
        {
            public string CustomizedName;

            public TestPropertyDescriptor(string name)
                : base(name, null) {}

            public void SetName(string name)
            {
                CustomizedName = name;
            }

            public void SetType(Type type) { throw new NotImplementedException(); }
            public IPropertyImpl OverrideImpl(IPropertyImpl impl) { throw new NotImplementedException(); }

            #region Unimplemented members of PropertyDescriptor

            public override bool CanResetValue(object component) { throw new NotImplementedException(); }
            public override object GetValue(object component) { throw new NotImplementedException(); }
            public override void ResetValue(object component) { throw new NotImplementedException(); }
            public override void SetValue(object component, object value) { throw new NotImplementedException(); }
            public override bool ShouldSerializeValue(object component) { throw new NotImplementedException(); }
            public override Type ComponentType => throw new NotImplementedException();
            public override bool IsReadOnly => throw new NotImplementedException();
            public override Type PropertyType => throw new NotImplementedException();

            #endregion
        }
    }
}

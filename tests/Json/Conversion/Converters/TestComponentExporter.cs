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
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using CustomTypeDescriptor = Conversion.CustomTypeDescriptor;

    #endregion

    [ TestFixture ]
    public class TestComponentExporter
    {
        [ Test ]
        public void EmptyObject()
        {
            Assert.AreEqual("[\"System.Object\"]", Format(new object()));
        }

        [ Test ]
        public void PublicProperties()
        {
            var car = new Car();
            car.Manufacturer = "BMW";
            car.Model = "350";
            car.Year = 2000;
            car.Color = "Silver";

            Test(new JsonObject(
                new[] { "manufacturer", "model", "year", "color" },
                new object[] { car.Manufacturer, car.Model, car.Year, car.Color }), car);
        }

        [ Test ]
        public void NullPropertiesSkipped()
        {
            var car = new Car();

            var reader = FormatForReading(car);
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("year", reader.ReadMember());
            Assert.AreEqual(0, (int) reader.ReadNumber());
            Assert.AreEqual(JsonTokenClass.EndObject, reader.TokenClass);
        }

        [ Test ]
        public void EmbeddedObjects()
        {
            var snow = new Person();
            snow.Id = 2;
            snow.FullName = "Snow White";

            var albert = new Person();
            albert.Id = 1;
            albert.FullName = "Albert White";

            var m = new Marriage();
            m.Husband = albert;
            m.Wife = snow;

            Test(new JsonObject(
                new[] { "husband", "wife" },
                new object[] {
                    /* Husband */ new JsonObject(
                        new[] { "id", "fullName" },
                        new object[] { albert.Id, albert.FullName }),
                    /* Wife */ new JsonObject(
                        new[] { "id", "fullName" },
                        new object[] { snow.Id, snow.FullName })
                }), m);
        }

        [ Test ]
        public void CustomPropertiesInternally()
        {
            var point = new Point(123, 456);
            var writer = new JsonRecorder();
            JsonConvert.Export(point, writer);
            var reader = writer.CreatePlayer();
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("x", reader.ReadMember());
            Assert.AreEqual(123, reader.ReadNumber().ToInt32());
            Assert.AreEqual("y", reader.ReadMember());
            Assert.AreEqual(456, reader.ReadNumber().ToInt32());
            Assert.AreSame(JsonTokenClass.EndObject, reader.TokenClass);
            Assert.IsFalse(reader.Read());
        }

        [ Test ]
        public void TypeSpecific()
        {
            var john = new Person();
            john.Id = 123;
            john.FullName = "John Doe";

            var beamer = new Car();
            beamer.Manufacturer = "BMW";
            beamer.Model = "350";
            beamer.Year = 2000;
            beamer.Color = "Silver";

            var johnCars = new OwnerCars();
            johnCars.Owner = john;
            johnCars.Cars.Add(beamer);

            var test = new JsonObject(
                new[] { "owner", "cars" },
                new object[] {
                    /* Owner */ new JsonObject(
                        new[] { "id", "fullName" },
                        new object[] { john.Id,  john.FullName }),
                    /* Cars */ new object[] {
                        new JsonObject(
                            new[] { "manufacturer", "model", "year", "color" },
                            new object[] { beamer.Manufacturer, beamer.Model, beamer.Year, beamer.Color })
                    }
                });

            Test(test, johnCars);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void ImmediateCircularReferenceDetection()
        {
            var context = new ExportContext();
            var exporter = new ComponentExporter(typeof(Thing));
            context.Register(exporter);
            var thing = new Thing();
            thing.Other = thing;
            exporter.Export(context, thing, new EmptyJsonWriter());
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void DeepCircularReferenceDetection()
        {
            var context = new ExportContext();
            var exporter = new ComponentExporter(typeof(Thing));
            context.Register(exporter);
            var thing = new Thing();
            thing.Other = new Thing();
            thing.Other.Other = new Thing();
            thing.Other.Other.Other = thing;
            exporter.Export(context, thing, new EmptyJsonWriter());
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CircularReferenceDetectionAcrossTypes()
        {
            var context = new ExportContext();
            var exporter = new ComponentExporter(typeof(Parent));
            context.Register(exporter);
            context.Register(new ComponentExporter(typeof(ParentChild)));
            var parent = new Parent();
            parent.Child = new ParentChild();
            parent.Child.Parent = parent;
            exporter.Export(context, parent, new EmptyJsonWriter());
        }

        [ Test ]
        public void MemberExportCustomization()
        {
            var calls = new ArrayList();

            var logicalType = new TestTypeDescriptor();
            var properties = logicalType.GetProperties();

            Hashtable services;

            var memexp1 = new TestObjectMemberExporter(calls);
            services = new Hashtable();
            services.Add(typeof(IObjectMemberExporter), memexp1);
            properties.Add(new TestPropertyDescriptor("prop1", services));

            var memexp2 = new TestObjectMemberExporter(calls);
            services = new Hashtable();
            services.Add(typeof(IObjectMemberExporter), memexp2);
            properties.Add(new TestPropertyDescriptor("prop2", services));

            var exporter = new ComponentExporter(typeof(Thing), logicalType);
            var context = new ExportContext();
            context.Register(exporter);

            var writer = new JsonRecorder();
            var thing = new Thing();
            context.Export(thing, writer);

            Assert.AreEqual(2, calls.Count);

            object[] args = { context, writer, thing };

            Assert.AreSame(memexp1, calls[0]);
            Assert.AreEqual(args, ((TestObjectMemberExporter) calls[0]).ExportArgs);

            Assert.AreSame(memexp2, calls[1]);
            Assert.AreEqual(args, ((TestObjectMemberExporter) calls[1]).ExportArgs);
        }

        sealed class TestObjectMemberExporter : IObjectMemberExporter
        {
            public object[] ExportArgs;

            readonly IList _sequence;

            public TestObjectMemberExporter(IList recorder)
            {
                _sequence = recorder;
            }

            void IObjectMemberExporter.Export(ExportContext context, JsonWriter writer, object source)
            {
                ExportArgs = new[] { context, writer, source };
                _sequence.Add(this);
            }
        }

        sealed class TestPropertyDescriptor : PropertyDescriptor, IServiceProvider
        {
            readonly IDictionary _services;

            public TestPropertyDescriptor(string name, IDictionary services) : base(name, null)
            {
                _services = services;
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                return _services[serviceType];
            }

            #region Unimplemented members of PropertyDescriptor

            public override bool CanResetValue(object component) { throw new NotImplementedException(); }
            public override object GetValue(object component) { throw new NotImplementedException(); }
            public override void ResetValue(object component) { throw new NotImplementedException(); }
            public override void SetValue(object component, object value) { throw new NotImplementedException(); }
            public override bool ShouldSerializeValue(object component) { throw new NotImplementedException(); }
            public override Type ComponentType { get { throw new NotImplementedException(); } }
            public override bool IsReadOnly { get { throw new NotImplementedException(); } }
            public override Type PropertyType { get { throw new NotImplementedException(); } }

            #endregion
        }

        sealed class TestTypeDescriptor : ICustomTypeDescriptor
        {
            readonly PropertyDescriptorCollection _properties = new PropertyDescriptorCollection(null);

            public PropertyDescriptorCollection GetProperties()
            {
                return _properties;
            }

            #region Unimplemented members of ICustomTypeDescriptor

            public AttributeCollection GetAttributes() { throw new NotImplementedException(); }
            public string GetClassName() { throw new NotImplementedException(); }
            public string GetComponentName() { throw new NotImplementedException(); }
            public TypeConverter GetConverter() { throw new NotImplementedException(); }
            public EventDescriptor GetDefaultEvent() { throw new NotImplementedException(); }
            public PropertyDescriptor GetDefaultProperty() { throw new NotImplementedException(); }
            public object GetEditor(Type editorBaseType) { throw new NotImplementedException(); }
            public EventDescriptorCollection GetEvents() { throw new NotImplementedException(); }
            public EventDescriptorCollection GetEvents(Attribute[] attributes) { throw new NotImplementedException(); }
            public PropertyDescriptorCollection GetProperties(Attribute[] attributes) { throw new NotImplementedException(); }
            public object GetPropertyOwner(PropertyDescriptor pd) { throw new NotImplementedException(); }

            #endregion
        }

        sealed class Thing
        {
            public Thing Other;
        }

        sealed class Parent
        {
            public ParentChild Child;
        }

        sealed class ParentChild
        {
            public Parent Parent;
        }

        static string Format(object o)
        {
            var writer = new JsonTextWriter();
            JsonConvert.Export(o, writer);
            return writer.ToString();
        }

        static JsonReader FormatForReading(object o)
        {
            return new JsonTextReader(new StringReader(Format(o)));
        }

        static void Test(JsonObject expected, object actual)
        {
            var reader = FormatForReading(actual);
            TestObject(expected, reader, "(root)");
            Assert.IsFalse(reader.Read(), "Expected EOF.");
        }

        static void TestObject(JsonObject expected, JsonReader reader, string path)
        {
            reader.MoveToContent();
            reader.ReadToken(JsonTokenClass.Object);

            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                var name = reader.ReadMember();
                var value = expected[name];
                expected.Remove(name);
                TestValue(value, reader, path + "/" + name);
            }

            Assert.AreEqual(0, expected.Count);
            reader.Read();
        }

        static void TestArray(Array expectations, JsonReader reader, string path)
        {
            reader.MoveToContent();
            reader.ReadToken(JsonTokenClass.Array);

            for (var i = 0; i < expectations.Length; i++)
                TestValue(expectations.GetValue(i), reader, path + "/" + i);

            reader.ReadToken(JsonTokenClass.EndArray);
        }

        static void TestValue(object expected, JsonReader reader, string path)
        {
            if (JsonNull.LogicallyEquals(expected))
            {
                Assert.AreEqual(JsonTokenClass.Null, reader.TokenClass, path);
            }
            else
            {
                var expectedType = Type.GetTypeCode(expected.GetType());

                if (expectedType == TypeCode.Object)
                {
                    if (expected.GetType().IsArray)
                        TestArray((Array) expected, reader, path);
                    else
                        TestObject((JsonObject) expected, reader, path);
                }
                else
                {
                    switch (expectedType)
                    {
                        case TypeCode.String : Assert.AreEqual(expected, reader.ReadString(), path); break;
                        case TypeCode.Int32  : Assert.AreEqual(expected, (int) reader.ReadNumber(), path); break;
                        default : Assert.Fail("Don't know how to handle {0} values.", expected.GetType()); break;
                    }
                }
            }
        }

        public sealed class Car
        {
            string _manufacturer;
            string _model;
            int _year;
            string _color;

            public string Manufacturer
            {
                get { return _manufacturer; }
                set { _manufacturer = value; }
            }

            public string Model
            {
                get { return _model; }
                set { _model = value; }
            }

            public int Year
            {
                get { return _year; }
                set { _year = value; }
            }

            public string Color
            {
                get { return _color; }
                set { _color = value; }
            }
        }

        public sealed class Person
        {
            int _id;
            string _fullName;

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string FullName
            {
                get { return _fullName; }
                set { _fullName = value; }
            }
        }

        public sealed class Marriage
        {
            public Person Husband;
            public Person Wife;
        }

        public sealed class OwnerCars
        {
            public Person Owner;
            public ArrayList Cars = new ArrayList();
        }

        public class Point : IJsonExportable
        {
            int _x;
            int _y;

            static readonly ICustomTypeDescriptor _componentType;

            static Point()
            {
                var type = typeof(Point);
                var x = type.GetProperty("X");
                var y = type.GetProperty("Y");
                _componentType = new CustomTypeDescriptor(type, new MemberInfo[] { x, y }, new[] { "x", "y" });
            }

            public Point(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public int X { get { return _x; } set { _x = value; } }
            public int Y { get { return _y; } set { _y = value; } }

            public void Export(ExportContext context, JsonWriter writer)
            {
                var exporter = new ComponentExporter(typeof(Point), _componentType);
                exporter.Export(new ExportContext(), this, writer);
            }
        }
    }
}

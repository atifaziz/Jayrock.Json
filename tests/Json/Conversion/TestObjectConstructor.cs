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

namespace Jayrock.Tests.Json.Conversion
{
    #region Imports

    using System;
    using System.Collections;
    using System.Reflection;
    using Jayrock.Json;
    using Jayrock.Json.Conversion;
    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class TestObjectConstructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CannotInitializeWithNullType()
        {
            new ObjectConstructor(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotInitializeWithConstructorsFromDifferentTypes()
        {
            var fc = typeof(FooBase).GetConstructors();
            Assert.AreEqual(1, fc.Length);
            var fdc = typeof(FooDerived).GetConstructors();
            Assert.AreEqual(1, fdc.Length);
            var ctors = new ConstructorInfo[fc.Length + fdc.Length];
            Array.Copy(fc, 0, ctors, 0, fc.Length);
            Array.Copy(fdc, 0, ctors, fc.Length, fdc.Length);
            new ObjectConstructor(typeof(FooDerived), ctors);
        }

        class FooBase {}
        class FooDerived : FooBase {}

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotInitializeWithNonConstructibleType()
        {
            new ObjectConstructor(typeof(PrivateThing));
        }

        class PrivateThing {
            PrivateThing() {} }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotInitializeWithZeroConstructors()
        {
            new ObjectConstructor(typeof(object), new ConstructorInfo[0]);
        }

        [Test]
        public void Construction()
        {
            var ctor = new ObjectConstructor(typeof(Point));
            var context = JsonConvert.CreateImportContext();
            const string json = "{ y: 456, x: 123 }";
            var result = ctor.CreateObject(context, JsonText.CreateReader(json));
            var point = (Point)result.Object;
            Assert.AreEqual(123, point.X);
            Assert.AreEqual(456, point.Y);
            Assert.AreEqual(0, JsonBuffer.From(result.TailReader).GetMemberCount());
        }

        [Test]
        public void ConstructionWithTail()
        {
            var ctor = new ObjectConstructor(typeof(Point));
            var context = JsonConvert.CreateImportContext();
            const string json = "{ y: 456, z: 789, x: 123 }";
            var result = ctor.CreateObject(context, JsonText.CreateReader(json));
            var point = (Point) result.Object;
            Assert.AreEqual(123, point.X);
            Assert.AreEqual(456, point.Y);
            var tail = JsonBuffer.From(result.TailReader).GetMembersArray();
            Assert.AreEqual(1, tail.Length);
            var z = tail[0];
            Assert.AreEqual("z", z.Name);
            Assert.AreEqual(789, z.Buffer.GetNumber().ToInt32());
        }

        [Test, ExpectedException(typeof(JsonException))]
        public void CannotCreateIfNoneConstructorsMatch()
        {
            var ctor = new ObjectConstructor(typeof(Point));
            var context = JsonConvert.CreateImportContext();
            ctor.CreateObject(context, JsonText.CreateReader("{ z: x: 123 }"));
        }

        class Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }
        }

        [Test]
        public void ConstructorSpecificity()
        {
            var thing = CreateThing("{ num: 42 }");
            Assert.AreEqual(42, thing.Number);
            Assert.AreEqual(GetThingConstructor(typeof(int)), thing.Constructor);

            thing = CreateThing("{ num: 42, flag: true }");
            Assert.AreEqual(42, thing.Number);
            Assert.IsTrue(thing.Flag);
            Assert.AreEqual(GetThingConstructor(typeof(int), typeof(bool)), thing.Constructor);

            thing = CreateThing("{ num: 42, flag: true, str: hello }");
            Assert.AreEqual(42, thing.Number);
            Assert.IsTrue(thing.Flag);
            Assert.AreEqual("hello", thing.String);
            Assert.AreEqual(GetThingConstructor(typeof(int), typeof(bool), typeof(string)), thing.Constructor);

            thing = CreateThing("{ num: 42, flag: true, str: hello, obj: { a: 123, b: 456 } }");
            Assert.AreEqual(42, thing.Number);
            Assert.IsTrue(thing.Flag);
            Assert.AreEqual("hello", thing.String);
            Assert.AreEqual(2, thing.Object.Count);
            Assert.AreEqual(123, Convert.ToInt32(thing.Object["a"]));
            Assert.AreEqual(456, Convert.ToInt32(thing.Object["b"]));
            Assert.AreEqual(GetThingConstructor(typeof(int), typeof(bool), typeof(string), typeof(IDictionary)), thing.Constructor);
        }

        static ConstructorInfo GetThingConstructor(params Type[] types)
        {
            return typeof(Thing).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, types, null);
        }

        Thing CreateThing(string json)
        {
            var ctor = new ObjectConstructor(typeof(Thing));
            var context = JsonConvert.CreateImportContext();
            var result = ctor.CreateObject(context, JsonText.CreateReader(json));
            return (Thing) result.Object;
        }

        class Thing
        {
            // ReSharper disable UnusedMember.Local
            public Thing(int num)
                : this(num, false, null, null, (ConstructorInfo)MethodBase.GetCurrentMethod()) {}

            public Thing(int num, bool flag)
                : this(num, flag, null, null, (ConstructorInfo)MethodBase.GetCurrentMethod()) {}

            public Thing(int num, bool flag, string str) :
                this(num, flag, str, null, (ConstructorInfo)MethodBase.GetCurrentMethod()) { }

            public Thing(int num, bool flag, string str, IDictionary obj) :
                this(num, flag, str, obj, (ConstructorInfo) MethodBase.GetCurrentMethod()) {}

            Thing(int num, bool flag, string str, IDictionary obj, ConstructorInfo ctor)
            {
                Number = num;
                Flag = flag;
                String = str;
                Object = obj;
                Constructor = ctor;
            }
            // ReSharper restore UnusedMember.Local

            public int Number { get; }
            public bool Flag { get; }
            public string String { get; }
            public IDictionary Object { get; }
            public ConstructorInfo Constructor { get; }
        }

        [Test]
        public void ValueTypeWithDefaultConstructorConstruction()
        {
            var constructor = new ObjectConstructor(typeof(ValueThing));
            var result = constructor.CreateObject(new ImportContext(), JsonText.CreateReader("{foo:bar}"));
            Assert.IsInstanceOf<ValueThing>(result.Object);
            var tail = result.TailReader;
            tail.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("foo", tail.ReadMember());
            Assert.AreEqual("bar", tail.ReadString());
            tail.ReadToken(JsonTokenClass.EndObject);
        }

        struct ValueThing {}
    }
}

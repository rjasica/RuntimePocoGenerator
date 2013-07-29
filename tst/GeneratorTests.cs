using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace RJ.RuntimePocoGenerator.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        private Generator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new Generator();
        }

        private class ImmutablePoint
        {
            public ImmutablePoint(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            public double X { get; private set; }

            public double Y { get; private set; }
        }

        private class NormalPoint
        {
            public double X { get; set; }

            public double Y { get; set; }
        }

        private class A
        {
            public B B { get; private set; }

            public C C { get; private set; }
        }

        private class B
        {
            public C C { get; private set; }
        }

        private class C
        {
            public int Number { get; private set; }
        }

        private TypeDescription pointTypeDescription = new TypeDescription("Point", new List<IPropertyDescription>()
            {
                new PropertyDescription("X", typeof(int)),
                new PropertyDescription("Y", typeof(int))
            });

        private TypeDescription doublePointTypeDescription = new TypeDescription("DoublePoint", new List<IPropertyDescription>()
            {
                new PropertyDescription("X", typeof(double)),
                new PropertyDescription("Y", typeof(double))
            });

        private void IntPointTestMethod(int x, int y)
        {
        }

        private void DoublePointTestMethod(double x, double y)
        {
        }
        #region TypeDescription

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateType_should_throw_exception_when_type_description_is_null()
        {
            generator.GenerateType((ITypeDescription) null);
        }

        [Test]
        public void GenerateType_should_return_type_from_description()
        {
            var result = generator.GenerateType(pointTypeDescription);

            Assert.NotNull(result);
            dynamic instance = Activator.CreateInstance(result.Type, new object[] {1, 2});
            
            Assert.AreEqual(1, instance.X);
            Assert.AreEqual(2, instance.Y);
        }

        [Test]
        public void GenerateType_should_return_type_from_description_from_cache()
        {
            var firstResult = generator.GenerateType(pointTypeDescription);
            var secondResult = generator.GenerateType(pointTypeDescription);
        
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.AreEqual(secondResult.Type, firstResult.Type);
        }

        #endregion

        #region Method

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateType_should_throw_exception_when_method_info_is_null()
        {
            generator.GenerateType((MethodInfo) null);
        }

        [Test]
        public void GenerateType_should_return_type_from_method()
        {
            var method = this.GetType().GetMethod("IntPointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var result = generator.GenerateType(method);

            Assert.NotNull(result);
            dynamic instance = Activator.CreateInstance(result.Type, new object[] {1, 2});
            Assert.AreEqual(1, instance.X);
            Assert.AreEqual(2, instance.Y);
        }

        [Test]
        public void GenerateType_should_return_type_from_method_from_cache()
        {
            var method = this.GetType().GetMethod("IntPointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);

            var firstResult = generator.GenerateType(method);
            var secondResult = generator.GenerateType(method);

            Assert.AreEqual(firstResult.Type, secondResult.Type);
        }

        #endregion

        #region Type

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateType_should_throw_exception_when_type_is_null()
        {
            generator.GenerateType((Type) null);
        }

        [Test]
        public void GenerateType_should_return_type_from_source_type()
        {
            var generatedType = generator.GenerateType(typeof(ImmutablePoint));

            dynamic instance = Activator.CreateInstance(generatedType.Type, new object[]{1.5d, 2.25d});
            Assert.AreEqual(1.5, instance.X);
            Assert.AreEqual(2.25, instance.Y);
        }

        [Test]
        public void GenerateType_should_return_type_from_cache()
        {
            var generatedTypeFirst = generator.GenerateType(typeof(ImmutablePoint));
            var generatedTypeSecond = generator.GenerateType(typeof(ImmutablePoint));

            Assert.AreEqual(generatedTypeFirst.Type, generatedTypeSecond.Type);
        }

        [Test]
        public void GenerateType_should_return_type_from_complex_source_type()
        {
            var result = generator.GenerateType(typeof(A));

            dynamic instance = Activator.CreateInstance(result.Type);

            Assert.NotNull(instance);

            var aType = (Type)instance.GetType();
            var bType = aType.GetProperty("B").PropertyType;
            var cType = aType.GetProperty("C").PropertyType;
            
            aType.GetProperty("B").GetSetMethod().Invoke(instance, new object[] {Activator.CreateInstance(bType)});
            bType.GetProperty("C").GetSetMethod().Invoke(instance.B, new object[] { Activator.CreateInstance(cType) });
            aType.GetProperty("C").GetSetMethod().Invoke(instance, new object[] { Activator.CreateInstance(cType) });
            
            instance.B.C.Number = 1;
            instance.C.Number = 2;

            Assert.AreEqual(1, instance.B.C.Number);
            Assert.AreEqual(2, instance.C.Number);
        }

        #endregion

        #region Raw

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateType_should_throw_exception_when_name_is_null()
        {
            generator.GenerateType(null, new Dictionary<string, Type>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateType_should_throw_exception_when_properties_is_null()
        {
            generator.GenerateType("Test", null);
        }

        [Test]
        public void GenerateType_should_return_type_from_raw_data()
        {
            var result = generator.GenerateType("Point", new Dictionary<string, Type>()
                {
                    {"X", typeof(decimal)},
                    {"Y", typeof(decimal)}
                });

            Assert.NotNull(result);
            dynamic instance = Activator.CreateInstance(result.Type, new object[] {1m, 2m});
            Assert.AreEqual(1m, instance.X);
            Assert.AreEqual(2m, instance.Y);
        }

        #endregion

        #region TypeDescriptions
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateTypes_should_throw_exception_when_type_description_is_null()
        {
            generator.GenerateTypes((IEnumerable<ITypeDescription>)null);
        }

        [Test]
        public void GenerateTypes_should_return_many_types_from_many_source_type()
        {
            var result = generator.GenerateTypes(new ITypeDescription[]
                {
                    pointTypeDescription,
                    doublePointTypeDescription
                });

            var intPointResult = result.Where(x => x.Name == pointTypeDescription.Name).SingleOrDefault();
            Assert.NotNull(intPointResult);

            var doublePointResult = result.Where(x => x.Name == doublePointTypeDescription.Name).SingleOrDefault();
            Assert.NotNull(doublePointResult);
        }

        [Test]
        public void GenerateTypes_should_return_types_from_many_source_type_from_cache()
        {
            var firstResult = generator.GenerateTypes(new ITypeDescription[]
                {
                    pointTypeDescription,
                    doublePointTypeDescription
                });
            var secondResult = generator.GenerateTypes(new ITypeDescription[]
                {
                    pointTypeDescription,
                    doublePointTypeDescription
                });

            var firstIntPointResult = firstResult.Where(x => x.Name == pointTypeDescription.Name).SingleOrDefault();
            var firstDoublePointResult = firstResult.Where(x => x.Name == doublePointTypeDescription.Name).SingleOrDefault();

            var secondIntPointResult = secondResult.Where(x => x.Name == pointTypeDescription.Name).SingleOrDefault();
            var secondDoublePointResult = secondResult.Where(x => x.Name == doublePointTypeDescription.Name).SingleOrDefault();
            
            Assert.AreEqual(secondIntPointResult.Type, firstIntPointResult.Type);
            Assert.AreEqual(secondDoublePointResult.Type, firstDoublePointResult.Type);
        }

        public void GenerateTypes_should_return_type_from_many_source_type_from_cache()
        {
            var firstResult = generator.GenerateTypes(new ITypeDescription[]
                {
                    pointTypeDescription,
                    doublePointTypeDescription
                });
            var secondResult = generator.GenerateTypes(new ITypeDescription[]
                {
                    pointTypeDescription
                });

            var firstIntPointResult = firstResult.Where(x => x.Name == pointTypeDescription.Name).SingleOrDefault();
            var firstDoublePointResult = firstResult.Where(x => x.Name == doublePointTypeDescription.Name).SingleOrDefault();

            var secondIntPointResult = secondResult.Where(x => x.Name == pointTypeDescription.Name).SingleOrDefault();

            Assert.AreEqual(secondIntPointResult.Type, firstIntPointResult);
            Assert.NotNull(firstDoublePointResult);
        }

        #endregion

        #region Methods

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateTypes_should_throw_exception_when_method_info_is_null()
        {
            generator.GenerateTypes((IEnumerable<MethodInfo>) null);
        }

        [Test]
        public void GenerateType_should_return_types_from_methods()
        {
            var firstMethod = this.GetType().GetMethod("IntPointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var secondMethod = this.GetType().GetMethod("DoublePointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);


            var result = generator.GenerateTypes(new MethodInfo[]
                {
                    firstMethod,
                    secondMethod
                });

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GenerateType_should_return_types_from_methods_from_cache()
        {
            var firstMethod = this.GetType().GetMethod("IntPointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var secondMethod = this.GetType().GetMethod("DoublePointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);

            var firstResult = generator.GenerateTypes(new MethodInfo[]
                {
                    firstMethod,
                    secondMethod
                });
            var secondResult = generator.GenerateTypes(new MethodInfo[]
                {
                    firstMethod,
                    secondMethod
                });
            var intFromFirst = firstResult.Where(x => x.Name.Contains("Int")).First();
            var intFromSecond = secondResult.Where(x => x.Name.Contains("Int")).First();
            var doubleFromFirst = firstResult.Where(x => x.Name.Contains("Double")).First();
            var doubleFromSecond = secondResult.Where(x => x.Name.Contains("Double")).First();

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.AreEqual(intFromFirst.Type, intFromSecond.Type);
            Assert.AreEqual(doubleFromFirst.Type, doubleFromSecond.Type);
        }

        [Test]
        public void GenerateType_should_return_type_from_methods_from_cache()
        {
            var firstMethod = this.GetType().GetMethod("IntPointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var secondMethod = this.GetType().GetMethod("DoublePointTestMethod", BindingFlags.NonPublic | BindingFlags.Instance);

            var firstResult = generator.GenerateTypes(new MethodInfo[]
                {
                    firstMethod
                });
            var secondResult = generator.GenerateTypes(new MethodInfo[]
                {
                    firstMethod,
                    secondMethod
                });
            var intFromFirst = firstResult.First();
            var intFromSecond = secondResult.First();
            var doubleFromSecond = secondResult.Last();

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.AreEqual(intFromFirst.Type, intFromSecond.Type);
            Assert.NotNull(doubleFromSecond);
        }

        #endregion


        #region Types

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateTypes_should_throw_exception_when_type_is_null()
        {
            generator.GenerateTypes((IEnumerable<Type>) null);
        }

        [Test]
        public void GenerateTypes_should_return_many_types_from_source_types()
        {
            var result = generator.GenerateTypes(new []{typeof(ImmutablePoint), typeof(NormalPoint)});

            var normal = result.Where(x => x.Name == typeof (NormalPoint).FullName).SingleOrDefault();
            Assert.NotNull(normal);

            dynamic instance1 = Activator.CreateInstance(normal.Type, new object[] { 1.5d, 2.25d });
            Assert.AreEqual(1.5, instance1.X);
            Assert.AreEqual(2.25, instance1.Y);

            var immutable = result.Where(x => x.Name == typeof(ImmutablePoint).FullName).SingleOrDefault();
            Assert.NotNull(normal);

            dynamic instance2 = Activator.CreateInstance(immutable.Type, new object[] { 2d, 3d });
            Assert.AreEqual(2, instance2.X);
            Assert.AreEqual(3, instance2.Y);
        }

        [Test]
        public void GenerateTypes_should_return_many_types_from_source_types_from_cache()
        {
            var resultFirst = generator.GenerateTypes(new[] { typeof(ImmutablePoint), typeof(NormalPoint) });
            var resultSecond = generator.GenerateTypes(new[] { typeof(ImmutablePoint), typeof(NormalPoint) });

            var immutableFirst = resultFirst.Where(x => x.Name == typeof(ImmutablePoint).FullName).SingleOrDefault();
            var normalFirst = resultFirst.Where(x => x.Name == typeof(NormalPoint).FullName).SingleOrDefault();

            var immutableSecond = resultSecond.Where(x => x.Name == typeof(ImmutablePoint).FullName).SingleOrDefault();
            var normalSecond = resultSecond.Where(x => x.Name == typeof(NormalPoint).FullName).SingleOrDefault();
            
            Assert.AreEqual(immutableSecond, immutableFirst);
            Assert.AreEqual(normalSecond, normalFirst);
        }

        #endregion

    }
}

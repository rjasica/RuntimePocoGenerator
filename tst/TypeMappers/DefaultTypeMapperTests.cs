using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RJ.RuntimePocoGenerator.TypeMappers;

namespace RJ.RuntimePocoGenerator.Tests.TypeMappers
{
    [TestFixture]
    public class DefaultTypeMapperTests
    {
        private DefaultTypeMapper typeMapper;

        private Dictionary<ITypeDescription, IGeneratedType> typeDescriptions;

        [SetUp]
        public void SetUp()
        {
            this.typeDescriptions = new Dictionary<ITypeDescription, IGeneratedType>();
            this.typeMapper = new DefaultTypeMapper(typeDescriptions);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_should_throws_exception_when_type_descriptions_is_null()
        {
            new DefaultTypeMapper(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_should_throws_exception_when_mapper_is_null()
        {
            new DefaultTypeMapper(new Dictionary<ITypeDescription, IGeneratedType>(), null);
        }

        [Test]
        public void GetType_should_return_source_type_when_type_is_not_registered()
        {
            var result = typeMapper.GetType(this.GetType());

            Assert.AreEqual(result, this.GetType());
        }

        [Test]
        public void GetType_should_return_mapped_type_when_type_is_registered()
        {
            var typeDescription = new TypeDescription("test", new IPropertyDescription[0]);
            var generatedType = new GeneratedType("test", typeof(int), typeDescription);
            typeMapper.RegisterNewType(typeDescription, generatedType);

            var result = typeMapper.GetType(this.GetType());

            Assert.AreEqual(result, this.GetType());
        }

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        [Test]
        public void GetRequiredTypeToGenerate_should_not_return_system_type()
        {
            var result = typeMapper.GetRequiredTypeToGenerate(typeof(Point));

            Assert.AreEqual(1, result.Count());
        }

        public class Rectangle
        {
            public Point Begin { get; set; }
            public Point End { get; set; }
        }

        [Test]
        public void GetRequiredTypeToGenerate_should_return_distinct_result_set()
        {
            var result = typeMapper.GetRequiredTypeToGenerate(typeof(Rectangle));

            CollectionAssert.AllItemsAreUnique(result);
            Assert.AreEqual(2, result.Count());
        }
    }
}

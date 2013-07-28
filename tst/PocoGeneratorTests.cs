using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RJ.RuntimePocoGenerator.Tests
{
    [TestFixture]
    public class PocoGeneratorTests
    {
        private PocoEmitGenertor generator;
        private TypeDescription typeDescription;

        [SetUp]
        public void SetUp()
        {
            generator = new PocoEmitGenertor();
            typeDescription = new TypeDescription("Sample.Class", new List<IPropertyDescription>()
                {
                    new PropertyDescription("Number", typeof(int)),
                    new PropertyDescription("Text", typeof(string))
                });
        }

        [Test]
        public void GenerateTypes_should_generate_default_constructor()
        {
            var result = generator.GenerateTypes(new []{typeDescription}).FirstOrDefault();
            
            Assert.NotNull(result);
            Assert.AreEqual(result.TypeDescription, result.TypeDescription);

            dynamic instance = Activator.CreateInstance(result.Type);
            instance.Number = 123;
            instance.Text = "456";

            Assert.AreEqual(123, instance.Number);
            Assert.AreEqual("456", instance.Text);
        }

        [Test]
        public void GenerateTypes_should_generate_constructor()
        {
            var result = generator.GenerateTypes(new[] { typeDescription }).FirstOrDefault();

            Assert.NotNull(result);
            Assert.AreEqual(result.TypeDescription, result.TypeDescription);

            dynamic instance = Activator.CreateInstance(result.Type, new object[]{123, "456"});

            Assert.AreEqual(123, instance.Number);
            Assert.AreEqual("456", instance.Text);
        }

        [Test]
        public void GenerateTypes_should_generate_empty_type()
        {
            var result = generator.GenerateTypes(new[] {new TypeDescription("Test", new List<IPropertyDescription>())}).FirstOrDefault();

            Assert.NotNull(result);
            Assert.AreEqual(result.TypeDescription, result.TypeDescription);

            dynamic instance = Activator.CreateInstance(result.Type);

            Type type = instance.GetType();
            Assert.AreEqual(0, type.GetProperties().Count());
        }

        [Test]
        public void GenerateTypes_should_generate_single_assembly_for_many_types()
        {
            var result = generator.GenerateTypes(new[]
                {
                    typeDescription,
                    new TypeDescription("Point", new List<IPropertyDescription>()
                        {
                            new PropertyDescription("X", typeof(int)),
                            new PropertyDescription("Y", typeof(int))
                        }), 
                });

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
            var first = result.First();
            Assert.NotNull(first);
            var second = result.Last();
            Assert.NotNull(second);
            
            Assert.AreEqual(first.Type.Assembly, second.Type.Assembly);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateTypes_should_throw_exception_when_type_descriptions_is_null()
        {
            generator.GenerateTypes(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_type_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    null
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_name_of_type_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    new TypeDescription(null, new List<IPropertyDescription>())
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_property_descriptions_of_type_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    new TypeDescription("Type", null)
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_property_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    new TypeDescription("Type", new List<IPropertyDescription>()
                        {
                            null
                        }) 
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_name_of_property_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    new TypeDescription("Type", new List<IPropertyDescription>()
                        {
                            new PropertyDescription(null, typeof(int))
                        })
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTypes_should_throw_exception_when_type_of_property_description_is_null()
        {
            generator.GenerateTypes(new ITypeDescription[]
                {
                    new TypeDescription("Type", new List<IPropertyDescription>()
                        {
                            new PropertyDescription("Prop", null)
                        })
                });
        }
    }
}

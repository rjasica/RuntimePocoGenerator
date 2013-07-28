using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RJ.RuntimePocoGenerator.PropertySources;


namespace RJ.RuntimePocoGenerator.Tests.PropertySource
{
    [TestFixture]
    public class ClassSourceTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_should_throw_exception_when_value_is_null()
        {
            ClassSource classSource = new ClassSource(null);
        }

        private class SimpleClass
        {
            public int Number { get; set; }
            public string Text { get; set; }
        }

        [Test]
        public void GetProperties_should_return_all_properties_from_simple_class()
        {
            ClassSource classSource = new ClassSource(typeof(SimpleClass));

            var result = classSource.GetProperties();
            Assert.AreEqual(2, result.Count());
            result.ShouldContainsKeyAndValue("Number", typeof(int));
            result.ShouldContainsKeyAndValue("Text", typeof(string));
        }

        private interface IOnlyGet
        {
             int Number { get;}
        }

        [Test]
        public void GetProperties_should_return_property_with_only_get_method()
        {
            ClassSource classSource = new ClassSource(typeof(IOnlyGet));

            var result = classSource.GetProperties();
            Assert.AreEqual(1, result.Count());
            result.ShouldContainsKeyAndValue("Number", typeof(int));
        }

        private interface IOnlySet
        {
            string Text { get; }
        }

        [Test]
        public void GetProperties_should_return_property_with_only_set_method()
        {
            ClassSource classSource = new ClassSource(typeof(IOnlySet));

            var result = classSource.GetProperties();
            Assert.AreEqual(1, result.Count());
            result.ShouldContainsKeyAndValue("Text", typeof(string));
        }
    }
}

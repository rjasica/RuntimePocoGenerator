using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RJ.RuntimePocoGenerator.PropertySources;


namespace RJ.RuntimePocoGenerator.Tests.PropertySource
{
    [TestFixture]
    public class MethodSourceTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_should_throw_exception_when_value_is_null()
        {
            MethodSource methodSource = new MethodSource(null);
        }

        private void SimpleClass(int number, string text)
        {
        }

        [Test]
        public void GetProperties_should_return_all_parameters_from_simple_method()
        {
            var method = this.GetType().GetMethod("SimpleClass", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodSource methodSource = new MethodSource(method);

            var result = methodSource.GetProperties();
            Assert.AreEqual(2, result.Count());
            result.ShouldContainsKeyAndValue("Number", typeof(int));
            result.ShouldContainsKeyAndValue("Text", typeof(string));
        }
    }
}

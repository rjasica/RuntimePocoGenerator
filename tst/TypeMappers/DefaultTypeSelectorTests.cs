using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RJ.RuntimePocoGenerator.TypeMappers;

namespace RJ.RuntimePocoGenerator.Tests.TypeMappers
{
    [TestFixture]
    public class DefaultTypeSelectorTests
    {
        private DefaultTypeSelector selector;

        [SetUp]
        public void Setup()
        {
            this.selector = new DefaultTypeSelector();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldTypeBeGenerated_should_throw_exception_when_parameter_is_null()
        {
            selector.ShouldTypeBeGenerated(null);
        }

        [Test]
        public void ShouldTypeBeGenerated_should_false_for_sysyem_type()
        {
            var result = selector.ShouldTypeBeGenerated(typeof(int));
            Assert.False(result);
        }

        [Test]
        public void ShouldTypeBeGenerated_should_true_for_custome_type()
        {
            var result = selector.ShouldTypeBeGenerated(this.GetType());
            Assert.True(result);
        }
    }
}

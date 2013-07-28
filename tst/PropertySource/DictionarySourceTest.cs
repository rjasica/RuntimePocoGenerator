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
    public class DictionarySourceTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_should_throw_exception_when_value_is_null()
        {
            DictionarySource dictionarySource = new DictionarySource(null);
        }

        [Test]
        public void GetProperties_should_return_all_dictionary_items()
        {
            DictionarySource dictionarySource = new DictionarySource(new Dictionary<string, Type>()
                {
                    {"Number", typeof(int)},
                    {"Text", typeof(string)},
                });

            var result = dictionarySource.GetProperties();
            Assert.AreEqual(2, result.Count());
            result.ShouldContainsKeyAndValue("Number", typeof(int));
            result.ShouldContainsKeyAndValue("Text", typeof(string));
        }
    }
}

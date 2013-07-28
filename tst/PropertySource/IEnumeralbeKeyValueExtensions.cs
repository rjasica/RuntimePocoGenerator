using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace RJ.RuntimePocoGenerator.Tests.PropertySource
{
    internal static class IEnumeralbeIPropertyDescriptionExtensions
    {
        public static void ShouldContainsKeyAndValue<TKey, TValue>(this IEnumerable<IPropertyDescription> properties, TKey key, TValue value)
        {
            Assert.IsTrue(properties.Where(x => x.Name.Equals(key)).Any());
            var result = properties.Where(x => x.Name.Equals(key)).Single();
            Assert.AreEqual(result.Type, value);
        }
    }
}

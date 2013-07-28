using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.PropertySources
{
    internal class DictionarySource : IPropertySource
    {
        private readonly IDictionary<string, Type> dictionary;

        public DictionarySource(IDictionary<string, Type> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            this.dictionary = dictionary;
        }

        public IEnumerable<IPropertyDescription> GetProperties()
        {
            foreach (var item in dictionary)
            {
               yield return new PropertyDescription(item.Key, item.Value);
            }
        }
    }
}

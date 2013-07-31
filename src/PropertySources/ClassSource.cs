using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RJ.RuntimePocoGenerator.PropertySources
{
    public class ClassSource : IPropertySource
    {
        private readonly Type type;

        public ClassSource(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            this.type = type;
        }

        public IEnumerable<IPropertyDescription> GetProperties()
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = property.Name;
                var propertyType = property.PropertyType;
                yield return new PropertyDescription(name, propertyType);
            }
        }
    }
}

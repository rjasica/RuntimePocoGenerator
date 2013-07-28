using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RJ.RuntimePocoGenerator.Extensions;

namespace RJ.RuntimePocoGenerator.PropertySources
{
    internal class MethodSource : IPropertySource
    {
        private readonly MethodInfo method;

        public MethodSource(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            this.method = method;
        }

        public IEnumerable<IPropertyDescription> GetProperties()
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                var name = parameter.Name.UppercaseFirst();
                var type = parameter.ParameterType;
                yield return  new PropertyDescription(name, type);
            }
        }
    }
}

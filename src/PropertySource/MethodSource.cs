using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RJ.RuntimePocoGenerator.PropertySource
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
                var name = UppercaseFirst(parameter.Name);
                var type = parameter.ParameterType;
                yield return  new PropertyDescription(name, type);
            }
        }

        private static string UppercaseFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            char[] c = text.ToCharArray();
            c[0] = char.ToUpper(c[0]);
            return new string(c);
        }
    }
}

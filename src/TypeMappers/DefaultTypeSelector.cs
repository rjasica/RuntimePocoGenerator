using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.TypeMappers
{
    public class DefaultTypeSelector : ITypeSelector
    {
        public bool ShouldTypeBeGenerated(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return !type.FullName.StartsWith("System.");
        }
    }
}

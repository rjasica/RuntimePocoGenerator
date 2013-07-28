using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.PropertySource
{
    internal interface IPropertySource
    {
        IEnumerable<IPropertyDescription> GetProperties();
    }
}

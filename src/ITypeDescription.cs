using System.Collections.Generic;

namespace RJ.RuntimePocoGenerator
{
    public interface ITypeDescription
    {
        string Name { get; }
        IEnumerable<IPropertyDescription> PropertyDescriptions { get; }
    }
}
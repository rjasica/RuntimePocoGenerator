using System;

namespace RJ.RuntimePocoGenerator
{
    public interface IPropertyDescription
    {
        string Name { get; }
        Type Type { get; }
    }
}
using System;

namespace RJ.RuntimePocoGenerator
{
    public interface IGeneratedType
    {
        string Name { get; }

        Type Type { get; }

        ITypeDescription TypeDescription { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.TypeMappers
{
    public interface ITypeMapper
    {
        Type GetType(Type sourceType);

        void RegisterNewType(ITypeDescription source, IGeneratedType generatedType);

        IEnumerable<Type> GetRequiredTypeToGenerate(Type type);
    }
}

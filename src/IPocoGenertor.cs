using System.Collections.Generic;
using RJ.RuntimePocoGenerator.TypeMappers;

namespace RJ.RuntimePocoGenerator
{
    public interface IPocoGenertor
    {
        IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions, ITypeMapper typeMapper);
    }
}
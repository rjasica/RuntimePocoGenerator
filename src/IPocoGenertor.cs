using System.Collections.Generic;

namespace RJ.RuntimePocoGenerator
{
    public interface IPocoGenertor
    {
        IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions);
    }
}
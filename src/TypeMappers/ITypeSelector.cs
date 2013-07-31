using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.TypeMappers
{
    public interface ITypeSelector
    {
        bool ShouldTypeBeGenerated(Type type);
    }
}

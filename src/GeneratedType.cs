using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator
{
    public class GeneratedType : IGeneratedType
    {
        public GeneratedType(string name, Type type, ITypeDescription typeDescription)
        {
            this.Name = name;
            this.Type = type;
            this.TypeDescription = typeDescription;
        }

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public ITypeDescription TypeDescription { get; private set; }
    }
}

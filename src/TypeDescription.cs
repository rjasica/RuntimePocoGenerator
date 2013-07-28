using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator
{
    public class TypeDescription : ITypeDescription, IEquatable<TypeDescription>
    {
        public TypeDescription(string name, IEnumerable<IPropertyDescription> propertyDescriptions)
        {
            this.Name = name;
            this.PropertyDescriptions = propertyDescriptions;
        }

        public string Name { get; private set; }

        public IEnumerable<IPropertyDescription> PropertyDescriptions { get; private set; }

        public bool Equals(TypeDescription other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(this.Name, other.Name) && 
                Equals(this.PropertyDescriptions, other.PropertyDescriptions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TypeDescription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Name != null ? this.Name.GetHashCode() : 0)*397) ^ 
                    (this.PropertyDescriptions != null ? this.PropertyDescriptions.GetHashCode() : 0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator
{
    public class PropertyDescription : IPropertyDescription, IEquatable<PropertyDescription>
    {
        public PropertyDescription(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public bool Equals(PropertyDescription other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            return string.Equals(this.Name, other.Name) &&
                Equals(this.Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (obj.GetType() != this.GetType()) 
                return false;
            return Equals((PropertyDescription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Name != null ? this.Name.GetHashCode() : 0)*397) ^ 
                    (this.Type != null ? this.Type.GetHashCode() : 0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.TypeMappers
{
    public class DefaultTypeMapper : ITypeMapper
    {
        private readonly IDictionary<ITypeDescription, IGeneratedType> typeDescritpions;
        private readonly ITypeSelector typeSelector;

        public DefaultTypeMapper(
            IDictionary<ITypeDescription, IGeneratedType> typeDescritpions,
            ITypeSelector typeSelector)
        {
            if (typeDescritpions == null)
            {
                throw new ArgumentNullException("typeDescritpions");
            }

            if (typeSelector == null)
            {
                throw new ArgumentNullException("typeSelector");
            }

            this.typeDescritpions = typeDescritpions;
            this.typeSelector = typeSelector;
        }

        public DefaultTypeMapper(
            IDictionary<ITypeDescription, IGeneratedType> typeDescritpions)
            : this( typeDescritpions, new DefaultTypeSelector())
        {
        }

        public Type GetType(Type sourceType)
        {
            var result = typeDescritpions.Where(x => x.Key.Name == sourceType.FullName)
                .Select(x => x.Value.Type)
                .FirstOrDefault();

            return result ?? sourceType;
        }

        public void RegisterNewType(ITypeDescription source, IGeneratedType generatedType)
        {
            typeDescritpions[source] = generatedType;
        }

        public IEnumerable<Type> GetRequiredTypeToGenerate(Type type)
        {
            HashSet<Type> result = new HashSet<Type>();
            Queue<Type> workList = new Queue<Type>();
            workList.Enqueue(type);

            do
            {
                var item = workList.Dequeue();
                if (!result.Contains(item))
                {
                    if (typeSelector.ShouldTypeBeGenerated(item))
                    {
                        result.Add(item);
                        foreach (var propertyType in item.GetProperties().Select(x => x.PropertyType).Distinct())
                        {
                            workList.Enqueue(propertyType);
                        }
                    }
                }
            } while (workList.Count != 0);

            return result;
        }
    }
}

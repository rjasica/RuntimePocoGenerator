using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RJ.RuntimePocoGenerator.PropertySource;

namespace RJ.RuntimePocoGenerator
{
    public class Generator
    {
        private readonly IPocoGenertor pocoGenerator;

        private readonly IDictionary<Type, IGeneratedType> typeCache;

        private readonly IDictionary<ITypeDescription, IGeneratedType> typeDescritpionCache;

        public Generator(IPocoGenertor pocoGenerator)
        {
            this.pocoGenerator = pocoGenerator;
            this.typeCache = new Dictionary<Type, IGeneratedType>();
            this.typeDescritpionCache = new Dictionary<ITypeDescription, IGeneratedType>();
        }

        public Generator()
            : this(new PocoEmitGenertor())
        {
        }

        public IGeneratedType GenerateType(string typeName, IDictionary<string, Type> properties)
        {
            return GenerateType(GetTypeDescription(typeName, properties));
        }

        private static TypeDescription GetTypeDescription(string typeName, IDictionary<string, Type> properties)
        {
            var propertySource = new DictionarySource(properties);
            var typeDescription = new TypeDescription(typeName, propertySource.GetProperties());
            return typeDescription;
        }

        public IGeneratedType GenerateType(Type sourceClass)
        {
            IGeneratedType generatedType;
            if (!this.typeCache.TryGetValue(sourceClass, out generatedType))
            {
                generatedType = GenerateType(GetTypeDescription(sourceClass));
                this.typeCache[sourceClass] = generatedType;
            }
            return generatedType;
        }

        private static TypeDescription GetTypeDescription(Type sourceClass)
        {
            var propertySource = new ClassSource(sourceClass);
            var typeDesctiption = new TypeDescription(sourceClass.FullName, propertySource.GetProperties());
            return typeDesctiption;
        }

        public IGeneratedType GenerateType(MethodInfo method)
        {
            return GenerateType(GetTypeDescription(method));
        }

        private static TypeDescription GetTypeDescription(MethodInfo method)
        {
            var propertySource = new MethodSource(method);
            var typeDescription = new TypeDescription(method.DeclaringType.FullName + "+" + method.Name, propertySource.GetProperties());
            return typeDescription;
        }

        public IGeneratedType GenerateType(ITypeDescription typeDescription)
        {
            return this.GenerateTypes(new[] { typeDescription }).Single();
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<Type> types)
        {
            var toGenerate = new List<Type>();
            var cached = types.Select(x =>
            {
                IGeneratedType generatedType;
                if (this.typeCache.TryGetValue(x, out generatedType))
                {
                    return generatedType;
                }
                else
                {
                    toGenerate.Add(x);
                    return null;
                }
            }).Where(x => x != null).ToList();
            if (toGenerate.Count != 0)
            {
                var typeDescriptions = toGenerate.Select(x => new { 
                    TypeDescription = (ITypeDescription)GetTypeDescription(x),
                    Type = x
                }).ToDictionary(x => x.TypeDescription, x => x.Type);
                var generatedTypes = GenerateTypes(typeDescriptions.Keys);

                foreach (var generatedType in generatedTypes)
                {
                    var type = typeDescriptions[generatedType.TypeDescription];
                    this.typeCache[type] = generatedType;
                }
                
                return cached.Union(generatedTypes);
            }
            else
            {
                return cached;
            }
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<MethodInfo> methods)
        {
            var typeDescriptions = methods.Select(x => GetTypeDescription(x)).ToList();
            return GenerateTypes(typeDescriptions);
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions)
        {
            var toGenerate = new List<ITypeDescription>();
            var cached = typeDescriptions.Select(x =>
                {
                    IGeneratedType generatedType;
                    if (this.typeDescritpionCache.TryGetValue(x, out generatedType))
                    {
                        return generatedType;
                    }
                    else
                    {
                        toGenerate.Add(x);
                        return null;
                    }
                }).Where(x => x != null).ToList();
            if (toGenerate.Count != 0)
            {
                var generated = this.pocoGenerator.GenerateTypes(toGenerate);
                return cached.Union(generated);
            }
            else
            {
                return cached;
            }
        }
    }
}

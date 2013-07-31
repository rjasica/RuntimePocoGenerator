using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RJ.RuntimePocoGenerator.PropertySources;
using RJ.RuntimePocoGenerator.TypeMappers;

namespace RJ.RuntimePocoGenerator
{
    public class Generator
    {
        private readonly IPocoGenertor pocoGenerator;

        private readonly ITypeMapper typeMapper; 

        private readonly IDictionary<Type, IGeneratedType> typeCache;

        private readonly IDictionary<MethodInfo, IGeneratedType> methodCache;

        private readonly IDictionary<ITypeDescription, IGeneratedType> typeDescritpionCache;

        public Generator(IPocoGenertor pocoGenerator, ITypeMapper typeMapper)
        {
            if (pocoGenerator == null)
            {
                throw new ArgumentNullException("pocoGenerator");
            }

            this.typeCache = new Dictionary<Type, IGeneratedType>();
            this.typeDescritpionCache = new Dictionary<ITypeDescription, IGeneratedType>();
            this.methodCache = new Dictionary<MethodInfo, IGeneratedType>();
            this.typeMapper = typeMapper;
            this.pocoGenerator = pocoGenerator;

            if (this.typeMapper == null)
            {
                this.typeMapper = new DefaultTypeMapper(this.typeDescritpionCache);
            }
        }

        public Generator(IPocoGenertor pocoGenerator)
            : this(pocoGenerator, null)
        {
        }

        public Generator()
            : this(new PocoEmitGenertor(), null)
        {
        }

        public IGeneratedType GenerateType(string typeName, IDictionary<string, Type> properties)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

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
            if (sourceClass == null)
            {
                throw new ArgumentNullException("sourceClass");
            }

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
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            IGeneratedType generatedType;
            if (!this.methodCache.TryGetValue(method, out generatedType))
            {
                generatedType = GenerateType(GetTypeDescription(method));
                this.methodCache[method] = generatedType;
            }

            return generatedType;
        }

        private static TypeDescription GetTypeDescription(MethodInfo method)
        {
            var propertySource = new MethodSource(method);
            var typeDescription = new TypeDescription(method.DeclaringType.FullName + "+" + method.Name, propertySource.GetProperties());
            return typeDescription;
        }

        public IGeneratedType GenerateType(ITypeDescription typeDescription)
        {
            if (typeDescription == null)
            {
                throw new ArgumentNullException("typeDescription");
            }

            return this.GenerateTypes(new[] {typeDescription}).Where(x => x.Name == typeDescription.Name).Single();
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

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
            if (methods == null)
            {
                throw new ArgumentNullException("methods");
            }

            var toGenerate = new List<MethodInfo>();
            var cached = methods.Select(x =>
            {
                IGeneratedType generatedType;
                if (this.methodCache.TryGetValue(x, out generatedType))
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
                var descriptionToMethod = new Dictionary<ITypeDescription, MethodInfo>();
                var typeDescriptions = toGenerate.Select(x =>
                    {
                        var typeDescription = (ITypeDescription) GetTypeDescription(x);
                        descriptionToMethod[typeDescription] = x;
                        return new
                            {
                                TypeDescription = typeDescription,
                                Type = x
                            };
                    }).ToDictionary(x => x.TypeDescription, x => x.Type);
                var generatedTypes = GenerateTypes(typeDescriptions.Keys);

                foreach (var generatedType in generatedTypes)
                {
                    var method = descriptionToMethod[generatedType.TypeDescription];
                    this.methodCache[method] = generatedType;
                }

                return cached.Union(generatedTypes);
            }
            else
            {
                return cached;
            }
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions)
        {
            if (typeDescriptions == null)
            {
                throw new ArgumentNullException("typeDescriptions");
            }

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
                var requiredTypes = toGenerate.SelectMany(x => x.PropertyDescriptions.SelectMany(
                    y => this.typeMapper.GetRequiredTypeToGenerate(y.Type))).Distinct().ToList();
                var requiredTypeDescriptions = requiredTypes.Select(x => GetTypeDescription(x));

                var all = toGenerate.Union(requiredTypeDescriptions).Distinct().ToList();

                var sorted = DependencySort.TopologicalSort.PerformTopoSort(all,
                    (item, dependencies) =>
                        {
                            var dep = all.Where(x => x.PropertyDescriptions.Where(y => y.Type.FullName == item.Name).Any()).ToList();
                            return dep;
                        });
                
                var generated = this.pocoGenerator.GenerateTypes(sorted, this.typeMapper);
                foreach (var generatedType in generated)
                {
                    this.typeDescritpionCache[generatedType.TypeDescription] = generatedType;
                }
                return cached.Union(generated);
            }
            else
            {
                return cached;
            }
        }
    }
}

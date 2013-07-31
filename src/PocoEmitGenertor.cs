using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using RJ.RuntimePocoGenerator.Extensions;
using RJ.RuntimePocoGenerator.TypeMappers;

namespace RJ.RuntimePocoGenerator
{
    public class PocoEmitGenertor : IPocoGenertor
    {
        private readonly string assemblyName;

        private readonly bool getTypeFromDiskAssembly;

        public PocoEmitGenertor()
            :this(false)
        {
        }

        public PocoEmitGenertor(bool getTypeFromDiskAssembly)
        {
            this.getTypeFromDiskAssembly = getTypeFromDiskAssembly;
            this.assemblyName = "poco.dll";
        }

        public PocoEmitGenertor(string assemblyName)
        {
            this.getTypeFromDiskAssembly = true;
            this.assemblyName = assemblyName;
        }

        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions, ITypeMapper typeMapper)
        {
            if (typeDescriptions == null)
            {
                throw new ArgumentNullException("typeDescriptions");
            }

            if (typeMapper == null)
            {
                throw new ArgumentNullException("typeMapper");
            }

            Validate(typeDescriptions);

            var name = "PocoAssembly";
            var assemblyName = new AssemblyName(name);
            var appDomain = Thread.GetDomain();

            AssemblyBuilder assemblyBuilder;
            ModuleBuilder moduleBuilder;

            if (this.getTypeFromDiskAssembly)
            {
                assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, this.assemblyName);
            }
            else
            {
                assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);    
            }

            var generatedTypes = new List<IGeneratedType>();
            foreach (var typeDescription in typeDescriptions)
            {
                var result = GenerateType(moduleBuilder, typeDescription, typeMapper);
                typeMapper.RegisterNewType(typeDescription, result);
                generatedTypes.Add(result);
            }

            if (this.getTypeFromDiskAssembly)
            {
                var generatedConvertedTypes = this.LoadFromDisk(assemblyBuilder, generatedTypes);

                return generatedConvertedTypes;
            }
            else
            {
                return generatedTypes;
            } 
        }

        private List<IGeneratedType> LoadFromDisk(AssemblyBuilder assemblyBuilder, List<IGeneratedType> generatedTypes)
        {
            assemblyBuilder.Save(this.assemblyName);
            var assembly = Assembly.LoadFrom(this.assemblyName);
            var conversion = assembly.GetTypes().ToDictionary(x => x.FullName, x => x);

            var generatedConvertedTypes = new List<IGeneratedType>(generatedTypes.Count);

            foreach (var generatedType in generatedTypes)
            {
                var fromDiskType = new GeneratedType(
                    generatedType.Name,
                    conversion[generatedType.Name],
                    generatedType.TypeDescription);
                generatedConvertedTypes.Add(fromDiskType);
            }
            return generatedConvertedTypes;
        }

        private void Validate(IEnumerable<ITypeDescription> typeDescriptions)
        {
            foreach (var typeDescription in typeDescriptions)
            {
                if (typeDescription == null)
                {
                    throw new ArgumentException("TypeDescription can not be null.", "typeDescriptions");
                }

                if (typeDescription.PropertyDescriptions == null)
                {
                    throw new ArgumentException("PropertyDescriptions of TypeDescription is required.", "typeDescriptions");
                }

                if (string.IsNullOrEmpty(typeDescription.Name))
                {
                    throw new ArgumentException("Name of TypeDescription is required.", "typeDescriptions");
                }

                foreach (var property in typeDescription.PropertyDescriptions)
                {
                    if (property == null)
                    {
                        throw new ArgumentException("PropertyDescription can not be null.", "typeDescriptions");
                    }

                    if (property.Type == null)
                    {
                        throw new ArgumentException("Type of PropertyDescription is required.", "typeDescriptions");
                    }

                    if (string.IsNullOrEmpty(property.Name))
                    {
                        throw new ArgumentException("Name of PropertyDescription is required.", "typeDescriptions");
                    }
                }
            }
        }

        private static IGeneratedType GenerateType(ModuleBuilder moduleBuilder, ITypeDescription typeDescription, ITypeMapper typeMapper)
        {
            var typeBuilder = moduleBuilder.DefineType(typeDescription.Name, TypeAttributes.Public | TypeAttributes.Class);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var types = typeDescription.PropertyDescriptions.Select(x => x.Type).ToArray();
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);

            var index = 1;
            foreach (var property in typeDescription.PropertyDescriptions)
            {
                constructorBuilder.DefineParameter(index, ParameterAttributes.None, property.Name.LowercaseFirst());
                index++;
            }

            var ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(Type.EmptyTypes));

            index = 1;
            foreach (var property in typeDescription.PropertyDescriptions)
            {
                var targetPropertyType = typeMapper.GetType(property.Type);
                EmitForPropertyDescription(typeBuilder, property, ctorIl, index, targetPropertyType);
                index++;
            }

            ctorIl.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            return new GeneratedType(typeDescription.Name, type, typeDescription);
        }

        private static void EmitForPropertyDescription(TypeBuilder typeBuilder, IPropertyDescription property, ILGenerator ctorIl, int index, Type targetType)
        {
            var field = typeBuilder.DefineField(property.Name, targetType, FieldAttributes.Private);

            EmitProperty(typeBuilder, property.Name, field, targetType);
            EmitConstructorFieldInit(ctorIl, index, field);
        }

        private static void EmitConstructorFieldInit(ILGenerator ctorIl, int index, FieldBuilder field)
        {
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg, index);
            ctorIl.Emit(OpCodes.Stfld, field);
        }

        private static void EmitProperty(TypeBuilder typeBuilder, string name, FieldBuilder field, Type targetType)
        {
            var prop = typeBuilder.DefineProperty(name, PropertyAttributes.None, targetType, null);

            var getter = EmitGetter(typeBuilder, name, targetType, field);
            var setter = EmittSetter(typeBuilder, name, targetType, field);

            prop.SetGetMethod(getter);
            prop.SetSetMethod(setter);
        }

        private static MethodBuilder EmittSetter(TypeBuilder typeBuilder, string name, Type type, FieldBuilder field)
        {
            MethodBuilder setter = typeBuilder.DefineMethod("set_" +name, MethodAttributes.Public, null, new Type[] {type});
            var setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, field);
            setterIl.Emit(OpCodes.Ret);
            return setter;
        }

        private static MethodBuilder EmitGetter(TypeBuilder typeBuilder, string name, Type type, FieldBuilder field)
        {
            var getter = typeBuilder.DefineMethod("get_" +name, MethodAttributes.Public, type, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, field);
            getterIl.Emit(OpCodes.Ret);
            return getter;
        }
    }
}

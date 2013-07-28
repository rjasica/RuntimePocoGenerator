using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace RJ.RuntimePocoGenerator
{
    public class PocoEmitGenertor : IPocoGenertor
    {
        public IEnumerable<IGeneratedType> GenerateTypes(IEnumerable<ITypeDescription> typeDescriptions)
        {
            if (typeDescriptions == null)
            {
                throw new ArgumentNullException("typeDescriptions");
            }

            Validate(typeDescriptions);

            var name = "PocoAssembly";
            var assemblyName = new AssemblyName(name);
            var appDomain = Thread.GetDomain();

            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            
            var generatedTypes = new List<IGeneratedType>();
            foreach (var typeDescription in typeDescriptions)
            {
                var result = GenerateType(moduleBuilder, typeDescription);
                generatedTypes.Add(result);
            }

            return generatedTypes;
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

        private static IGeneratedType GenerateType(ModuleBuilder moduleBuilder, ITypeDescription typeDescription)
        {
            var typeBuilder = moduleBuilder.DefineType(typeDescription.Name, TypeAttributes.Public | TypeAttributes.Class);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var types = typeDescription.PropertyDescriptions.Select(x => x.Type).ToArray();
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);

            var ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(Type.EmptyTypes));

            int index = 1;
            foreach (var property in typeDescription.PropertyDescriptions)
            {
                EmitForPropertyDescription(typeBuilder, property, ctorIl, index);
                index++;
            }

            ctorIl.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            return new GeneratedType(typeDescription.Name, type, typeDescription);
        }

        private static void EmitForPropertyDescription(TypeBuilder typeBuilder, IPropertyDescription property, ILGenerator ctorIl, int index)
        {
            var field = typeBuilder.DefineField(property.Name, property.Type, FieldAttributes.Private);

            EmitProperty(typeBuilder, property, field);
            EmitConstructorFieldInit(ctorIl, index, field);
        }

        private static void EmitConstructorFieldInit(ILGenerator ctorIl, int index, FieldBuilder field)
        {
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg, index);
            ctorIl.Emit(OpCodes.Stfld, field);
        }

        private static void EmitProperty(TypeBuilder typeBuilder, IPropertyDescription property, FieldBuilder field)
        {
            var prop = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.Type, null);

            var getter = EmitGetter(typeBuilder, property, field);
            var setter = EmittSetter(typeBuilder, property, field);

            prop.SetGetMethod(getter);
            prop.SetSetMethod(setter);
        }

        private static MethodBuilder EmittSetter(TypeBuilder typeBuilder, IPropertyDescription property, FieldBuilder field)
        {
            MethodBuilder setter = typeBuilder.DefineMethod("set_" + property.Name, MethodAttributes.Public, null, new Type[] {property.Type});
            var setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, field);
            setterIl.Emit(OpCodes.Ret);
            return setter;
        }

        private static MethodBuilder EmitGetter(TypeBuilder typeBuilder, IPropertyDescription property, FieldBuilder field)
        {
            var getter = typeBuilder.DefineMethod("get_" + property.Name, MethodAttributes.Public, property.Type, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, field);
            getterIl.Emit(OpCodes.Ret);
            return getter;
        }
    }
}

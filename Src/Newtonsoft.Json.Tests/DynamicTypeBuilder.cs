#if (NET45)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests
{
    public static class DynamicTypeBuilder
    {
        private static int Count = 0;

        public static Type CreateType(Dictionary<string, Type> fields)
        {
            Count++;

            TypeBuilder typeBuilder = CreateTypeBuilder();

            foreach (KeyValuePair<string, Type> field in fields)
            {
                CreatePropertyBuilder(typeBuilder, field.Key, field.Value);
            }

            return typeBuilder.CreateType();
        }

        private static TypeBuilder CreateTypeBuilder()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicAssembly" + Count),
                AssemblyBuilderAccess.RunAndCollect);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule" + Count);

            TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicClass" + Count,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit |TypeAttributes.AutoLayout,
                    null);

            ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public |
                MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            return typeBuilder;
        }

        private static void CreatePropertyBuilder(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropertyMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);

            ILGenerator getILGenerator = getPropertyMethodBuilder.GetILGenerator();
            getILGenerator.Emit(OpCodes.Ldarg_0);
            getILGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getILGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropertyMethodBuilder);

            MethodBuilder setPropertyMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setILGenerator = setPropertyMethodBuilder.GetILGenerator();
            Label modifyProperty = setILGenerator.DefineLabel();
            Label exitSet = setILGenerator.DefineLabel();

            setILGenerator.MarkLabel(modifyProperty);
            setILGenerator.Emit(OpCodes.Ldarg_0);
            setILGenerator.Emit(OpCodes.Ldarg_1);
            setILGenerator.Emit(OpCodes.Stfld, fieldBuilder);

            setILGenerator.Emit(OpCodes.Nop);
            setILGenerator.MarkLabel(exitSet);
            setILGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setPropertyMethodBuilder);
        }
    }
}
#endif

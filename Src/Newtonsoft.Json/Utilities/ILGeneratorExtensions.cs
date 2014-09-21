#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(PORTABLE40 || PORTABLE || NETFX_CORE)
using System;
using System.Reflection.Emit;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
    internal static class ILGeneratorExtensions
    {
        public static void PushInstance(this ILGenerator generator, Type type)
        {
            generator.Emit(OpCodes.Ldarg_0);
            if (type.IsValueType())
                generator.Emit(OpCodes.Unbox, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void PushArrayInstance(this ILGenerator generator, int argsIndex, int arrayIndex)
        {
            generator.Emit(OpCodes.Ldarg, argsIndex);
            generator.Emit(OpCodes.Ldc_I4, arrayIndex);
            generator.Emit(OpCodes.Ldelem_Ref);
        }

        public static void BoxIfNeeded(this ILGenerator generator, Type type)
        {
            if (type.IsValueType())
                generator.Emit(OpCodes.Box, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void UnboxIfNeeded(this ILGenerator generator, Type type)
        {
            if (type.IsValueType())
                generator.Emit(OpCodes.Unbox_Any, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void CallMethod(this ILGenerator generator, MethodInfo methodInfo)
        {
            if (methodInfo.IsFinal || !methodInfo.IsVirtual)
                generator.Emit(OpCodes.Call, methodInfo);
            else
                generator.Emit(OpCodes.Callvirt, methodInfo);
        }

        public static void Return(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Ret);
        }
    }
}
#endif
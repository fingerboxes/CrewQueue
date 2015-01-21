/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Alexander Taylor
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FingerboxLib
{
    public static class MiscUtils
    {
        private const int pflag = 38;

        public static object Invoke(object target, string methodName, object[] parameters)
        {
            MethodInfo targetMethod = target.GetType().GetMethod(methodName, (BindingFlags)pflag);
            return targetMethod.Invoke(target, parameters);
        }

        public static object Invoke(object target, string methodName)
        {
            return Invoke(target, methodName, new object[0]);
        }

        public static object GetField(object target, string fieldName) 
        {
            return target.GetType().GetField(fieldName, (BindingFlags)pflag).GetValue(target);
        }

        public static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, (BindingFlags)pflag).SetValue(target, value);
        }

        public static T[] GetFields<T>(object target)
        {
            FieldInfo[] fields = target.GetType().GetFields((BindingFlags)pflag);
            fields = fields.ToList<FieldInfo>().FindAll(f => f.FieldType == typeof(T)).ToArray();

            T[] retValue = new T[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                retValue[i] = (T)fields[i].GetValue(target);
            }

            return retValue;
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        } 
    }
}

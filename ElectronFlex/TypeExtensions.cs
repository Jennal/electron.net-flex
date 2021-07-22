using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ElectronFlex
{
    public static class TypeExtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                       x.Namespace == y.Namespace &&
                       x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] genericTypes, params Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                if (!method.IsGenericMethod) continue;
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method.MakeGenericMethod(genericTypes);
                }
            }

            return null;
        }
        
        public static MethodInfo GetMethod(this Type type, string name, params Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                if (!method.IsGenericMethod) continue;
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }
        
        public static MethodInfo GetMethod(this Type type, string name, params object[] args)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                if (!method.IsGenericMethod) continue;
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(args.Select(o => o.GetType()), new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }
        
        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags, params object[] args)
        {
            var methods = type.GetMethods(flags);
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                if (methodParameterTypes.SequenceEqual(args.Select(o => o.GetType()), new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }
    }
}
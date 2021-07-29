using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ElectronFlex
{
    public static class TypeExtensions
    {
        private static Dictionary<string, string> s_builtinTypeMap = new()
        {
            {"object", "Object"},
            {"int", "Int32"},
            {"long", "Int64"},
            {"float", "Float"},
            {"double", "Double"},
            {"bool", "Boolean"},
            {"string", "String"},
        };
        
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

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] genericTypes,
            params Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                if (!method.IsGenericMethod) continue;
                if (method.GetGenericArguments().Length != genericTypes.Length) continue;
                
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
            string[] genericTypes;
            (name, genericTypes) = SplitName(name);

            var methods = type.GetMethods(flags);
            foreach (var method in methods.Where(m => m.Name == name))
            {
                if (genericTypes != null && genericTypes.Length > 0)
                {
                    if (!method.IsGenericMethod) continue;
                    if (method.GetGenericArguments().Length != genericTypes.Length) continue;
                }
                
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                if (methodParameterTypes.SequenceEqual(args.Select(o => o.GetType()), new SimpleTypeComparer()))
                {
                    if (genericTypes != null && genericTypes.Length > 0)
                    {
                        var types = ToTypeArray(genericTypes);
                        return method.MakeGenericMethod(types);
                    }
                    
                    return method;
                }
            }

            return null;
        }

        public static Type ToType(string typeName)
        {
            var (name, genericTypes) = SplitName(typeName);
            if (s_builtinTypeMap.ContainsKey(name)) name = s_builtinTypeMap[name];
            
            if (genericTypes == null || genericTypes.Length <= 0)
            {
                return GetTypeFromAllAssemblies(name);
            }
            else
            {
                var type = GetTypeFromAllAssemblies($"{name}`{genericTypes.Length}");
                var typeGenericArgs = ToTypeArray(genericTypes);
                return type.MakeGenericType(typeGenericArgs);
            }
        }
        
        private static (string, string[]) SplitName(string name)
        {
            string[] genericTypes = null;
            if (name.Contains("<") && name.Contains(">"))
            {
                var start = name.IndexOf("<");
                var end = name.LastIndexOf(">");
                var methodGenericTypes = name.Substring(start + 1, end - start - 1);
                genericTypes = SplitGenericTypes(methodGenericTypes);
                name = name.Substring(0, start);
            }

            return (name, genericTypes);
        }
        
        private static Type[] ToTypeArray(string[] typeNames)
        {
            var result = new List<Type>();
            foreach (var typeName in typeNames)
            {
                result.Add(ToType(typeName));
            }
            
            return result.ToArray();
        }

        private static Type GetTypeFromAllAssemblies(string name, Predicate<Type> predicate=null)
        {
            var (ns, typeName) = SplitNs(name);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (!string.IsNullOrEmpty(ns))
            {
                assemblies = assemblies.Where(o => o.GetName().Name.StartsWith(ns)).ToArray();
            }
            
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(o => o.Name == typeName && (predicate == null || predicate(o)));
                if (type != null) return type;
            }

            return null;
        }

        private static string[] SplitGenericTypes(string typeArgs)
        {
            var result = new List<string>();
            var start = 0;
            var intend = 0;
            for (int i = 0; i < typeArgs.Length; i++)
            {
                var c = typeArgs[i];
                switch (c)
                {
                    case '<':
                        intend++;
                        break;
                    case '>':
                        intend--;
                        break;
                    case ',':
                        if (intend != 0) continue;
                        result.Add(typeArgs.Substring(start, i - start).Trim());
                        start = i + 1;
                        break;
                }
            }

            result.Add(typeArgs.Substring(start, typeArgs.Length - start).Trim());
            return result.ToArray();
        }
        
        private static Tuple<string, string> SplitNs(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            
            var idx = name.LastIndexOf(".");
            if (idx < 0) return new Tuple<string, string>(null, name);
            
            return new Tuple<string, string>(
                name.Substring(0, idx), 
                name.Substring(idx + 1, name.Length - idx - 1)
            );
        }
    }
}
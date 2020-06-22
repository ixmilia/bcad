using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IxMilia.BCad.Server
{
    public class ContractGenerator
    {
        public string OutFile { get; }

        private IReadOnlyDictionary<Type, string> _wellKnownTypes = new Dictionary<Type, string>()
        {
            { typeof(bool), "boolean" },
            { typeof(byte), "number" },
            { typeof(double), "number" },
            { typeof(int), "number" },
            { typeof(long), "number" },
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(JObject), "object" },
        };

        public ContractGenerator(string outFile)
        {
            OutFile = outFile;
        }

        public void Run()
        {
            var avoidTypes = new HashSet<Type>(_wellKnownTypes.Keys);
            avoidTypes.Add(typeof(void));
            avoidTypes.Add(typeof(Task));
            avoidTypes.Add(typeof(ValueType));
            var typeList = new HashSet<Type>();
            var agentType = typeof(ServerAgent);

            // find types
            var methodBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            foreach (var methodInfo in agentType.GetMethods(methodBindingFlags))
            {
                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    AddInterface(parameterInfo.ParameterType);
                }

                AddInterface(methodInfo.ReturnType);
            }

            // emit types
            var sb = new StringBuilder();
            foreach (var type in typeList)
            {
                if (type.IsEnum)
                {
                    sb.AppendLine($"export enum {TypeName(type)} {{");
                    foreach (var value in Enum.GetValues(type))
                    {
                        var name = Enum.GetName(type, value);
                        sb.AppendLine($"    {name} = {(int)value},");
                    }
                }
                else
                {
                    sb.AppendLine($"export interface {TypeName(type)} {{");
                    foreach (var propertyInfo in type.GetProperties(propertyBindingFlags))
                    {
                        sb.AppendLine($"    {PropertyName(propertyInfo)}: {TypeName(propertyInfo.PropertyType)};");
                    }
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

            // add commands
            sb.AppendLine("export abstract class ClientAgent {");
            sb.AppendLine("    abstract postNotification(method: string, params: any): void;");
            foreach (var methodInfo in agentType.GetMethods(methodBindingFlags))
            {
                if (!methodInfo.IsSpecialName)
                {
                    var parameters = methodInfo.GetParameters();
                    var args = string.Join(", ", parameters.Select(p => $"{p.Name}: {TypeName(p.ParameterType)}"));
                    sb.AppendLine();
                    sb.AppendLine($"    {CamelCase(methodInfo.Name)}({args}): void {{");
                    sb.AppendLine($"        this.postNotification('{methodInfo.Name}', {{ {string.Join(", ", parameters.Select(p => p.Name))} }});");
                    sb.AppendLine("    }");
                }
            }

            sb.AppendLine("}");

            string TypeName(Type type)
            {
                var typeName = _wellKnownTypes.ContainsKey(type)
                    ? _wellKnownTypes[type]
                    : type.Name;
                var underlying = Nullable.GetUnderlyingType(type);
                if (underlying is { })
                {
                    return TypeName(underlying);
                }

                if (type.IsArray)
                {
                    return $"{TypeName(type.GetElementType())}[]";
                }

                if (type.IsGenericType)
                {
                    return $"{TypeName(type.GenericTypeArguments[0])}[]";
                }

                return typeName;
            }

            string PropertyName(PropertyInfo propertyInfo)
            {
                var name = propertyInfo.Name;
                if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) is { })
                {
                    name += "?";
                }

                return name;
            }

            void AddInterface(Type type)
            {
                var underlying = Nullable.GetUnderlyingType(type);
                if (underlying is { })
                {
                    type = underlying;
                }
                else
                {
                    while (type.IsGenericType)
                    {
                        type = type.GenericTypeArguments[0];
                    }
                }

                if (type.IsArray)
                {
                    type = type.GetElementType();
                }

                if (avoidTypes.Contains(type) ||
                    (type.IsGenericType && avoidTypes.Contains(type.BaseType)))
                {
                    return;
                }

                if (typeList.Add(type))
                {
                    foreach (var propertyInfo in type.GetProperties(propertyBindingFlags))
                    {
                        AddInterface(propertyInfo.PropertyType);
                    }
                }
            }

            File.WriteAllText(OutFile, sb.ToString());
        }

        private static string CamelCase(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Display;
using IxMilia.BCad.FileHandlers;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace IxMilia.BCad.Rpc
{
    public class ContractGenerator
    {
        public IEnumerable<string> OutFiles { get; }

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

        private HashSet<Type> _ignoredTypes = new HashSet<Type>()
        {
            typeof(void),
            typeof(JsonRpc),
            typeof(Task),
            typeof(ValueType),
        };

        private HashSet<string> _optionalProperties = new HashSet<string>()
        {
            $"{nameof(ClientPropertyPaneValue)}.{nameof(ClientPropertyPaneValue.AllowedValues)}",
            $"{nameof(ClientPropertyPaneValue)}.{nameof(ClientPropertyPaneValue.IsUnrepresentable)}",
            $"{nameof(ClientPropertyPaneValue)}.{nameof(ClientPropertyPaneValue.Value)}",
        };

        public static HashSet<Type> EnumsAsNumbers = new HashSet<Type>()
        {
            typeof(CursorState),
            typeof(ModifierKeys),
        };

        public ContractGenerator(IEnumerable<string> outFiles)
        {
            OutFiles = outFiles;
        }

        public void Run()
        {
            var avoidTypes = new HashSet<Type>(_wellKnownTypes.Keys);
            foreach (var ignoredType in _ignoredTypes)
            {
                avoidTypes.Add(ignoredType);
            }

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

            // special-cased types
            AddInterface(typeof(ClientDownload));
            AddInterface(typeof(ClientUpdate));
            AddInterface(typeof(DxfFileSettings));

            // emit types
            var sb = new StringBuilder();
            foreach (var type in typeList)
            {
                if (type.IsEnum)
                {
                    sb.AppendLine($"export enum {TypeName(type)} {{");
                    var values = new List<object>();
                    foreach (var value in Enum.GetValues(type))
                    {
                        values.Add(value);
                    }

                    foreach (var value in values.Distinct())
                    {
                        var name = Enum.GetName(type, value);
                        var displayValue = EnumsAsNumbers.Contains(type)
                            ? ((int)value).ToString()
                            : string.Concat("\"", Enum.GetName(type, value), "\"");
                        sb.AppendLine($"    {name} = {displayValue},");
                    }
                }
                else
                {
                    sb.AppendLine($"export interface {TypeName(type)} {{");
                    foreach (var propertyInfo in type.GetProperties(propertyBindingFlags))
                    {
                        sb.AppendLine($"    {PropertyName(type, propertyInfo)}: {TypeName(propertyInfo.PropertyType)};");
                    }
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

            // add commands
            sb.AppendLine("export abstract class ClientAgent {");
            sb.AppendLine("    abstract postNotification(method: string, params: any): void;");
            sb.AppendLine("    abstract invoke(method: string, params: any): Promise<any>;");
            foreach (var methodInfo in agentType.GetMethods(methodBindingFlags))
            {
                if (!methodInfo.IsSpecialName)
                {
                    var parameters = methodInfo.GetParameters();
                    var args = string.Join(", ", parameters.Select(p => $"{p.Name}: {TypeName(p.ParameterType)}"));
                    sb.AppendLine();
                    if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == typeof(Task))
                    {
                        sb.AppendLine($"    {CamelCase(methodInfo.Name)}({args}): void {{");
                        sb.AppendLine($"        this.postNotification('{methodInfo.Name}', {{ {string.Join(", ", parameters.Select(p => p.Name))} }});");
                        sb.AppendLine("    }");
                    }
                    else
                    {
                        sb.AppendLine($"    {CamelCase(methodInfo.Name)}({args}): Promise<{TypeName(methodInfo.ReturnType)}> {{");
                        sb.AppendLine($"        return this.invoke('{methodInfo.Name}', {{ {string.Join(", ", parameters.Select(p => p.Name))} }});");
                        sb.AppendLine("    }");
                    }
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
                    return $"{TypeName(underlying)} | undefined";
                }

                if (type.IsArray)
                {
                    return $"{TypeName(type.GetElementType())}[]";
                }

                if (type.IsGenericType)
                {
                    typeName = $"{TypeName(type.GenericTypeArguments[0])}";
                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        typeName += "[]";
                    }
                }

                return typeName;
            }

            string PropertyName(Type parentType, PropertyInfo propertyInfo)
            {
                var name = propertyInfo.Name;
                if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) is { } ||
                    _optionalProperties.Contains($"{parentType.Name}.{propertyInfo.Name}"))
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

            var content = sb.ToString();
            foreach (var outFile in OutFiles)
            {
                File.WriteAllText(outFile, content);
            }
        }

        private static string CamelCase(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }
    }
}

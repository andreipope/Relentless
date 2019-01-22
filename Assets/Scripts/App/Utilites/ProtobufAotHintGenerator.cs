using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Loom.Google.Protobuf.Reflection;

namespace Loom.ZombieBattleground
{
    public class ProtobufAotHintGenerator
    {
        public string ProtobufNamespace { get; }

        public IEnumerable<MessageDescriptor> MessageDescriptors { get; }

        public uint LeadingSpaces { get; set; } = 8;

        private HashSet<string> _addedTypes = new HashSet<string>();

        public ProtobufAotHintGenerator(string protobufNamespace, IEnumerable<MessageDescriptor> messageDescriptors)
        {
            ProtobufNamespace = protobufNamespace;
            MessageDescriptors = messageDescriptors.OrderBy(descriptor => descriptor.Name);
        }

        public string GenerateAotHint()
        {
            _addedTypes.Clear();

            StringBuilder sb = new StringBuilder();

            foreach (MessageDescriptor descriptorMessageType in MessageDescriptors)
            {
                ProcessMessageDescriptor(sb, descriptorMessageType);
            }

            return sb.ToString();
        }

        private void WriteReflectionHelperForTypes(StringBuilder stringBuilder, Type declaringType, Type type)
        {
            string declaringTypeFullString = GetGenericTypeName(declaringType, true, true);
            string propertyTypeFullString = GetGenericTypeName(type, true, true);
            foreach (string typeFullString in new [] {declaringTypeFullString, propertyTypeFullString})
            {
                if (_addedTypes.Contains(typeFullString))
                    continue;

                stringBuilder.Append(' ', (int) LeadingSpaces);
                stringBuilder.Append($"Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<{typeFullString}>();");
                stringBuilder.Append(Environment.NewLine);

                _addedTypes.Add(typeFullString);
            }
        }

        private void ProcessMessageDescriptor(StringBuilder sb, MessageDescriptor messageDescriptor)
        {
            foreach (MessageDescriptor nestedMethodDescriptor in messageDescriptor.NestedTypes.OrderBy(descriptor => descriptor.Name))
            {
                ProcessMessageDescriptor(sb, nestedMethodDescriptor);
            }

            foreach (FieldDescriptor fieldDescriptor in messageDescriptor.Fields.InFieldNumberOrder().Concat(messageDescriptor.Oneofs.SelectMany(of => of.Fields)))
            {
                PropertyInfo propertyInfo = fieldDescriptor.ContainingType.ClrType.GetProperty(
                    (string) fieldDescriptor
                        .GetType()
                        .GetField("propertyName", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(fieldDescriptor)
                );
                WriteReflectionHelperForTypes(sb, propertyInfo.DeclaringType, propertyInfo.PropertyType);
            }

            foreach (OneofDescriptor oneof in messageDescriptor.Oneofs.OrderBy(descriptor => descriptor.Name))
            {
                string oneofPascalCaseName = oneof.Name.First().ToString().ToUpperInvariant() + oneof.Name.Substring(1);
                string oneofCaseName = oneofPascalCaseName + "OneofCase";
                Type oneofCaseType = oneof.ContainingType.ClrType.GetNestedType(oneofCaseName);
                if (oneofCaseType == null)
                    throw new Exception(oneofCaseName + " not found");

                WriteReflectionHelperForTypes(sb, oneof.ContainingType.ClrType, oneofCaseType);
            }
        }

        private static string GetUsualTypeName(string name)
        {
            name = name
                .Replace("&", "")
                .Replace("+", ".")
                .Split('`')[0];

            string shortName = name;

            switch (shortName)
            {
                case "Int16":
                    return "short";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "UInt16":
                    return "ushort";
                case "UInt32":
                    return "uint";
                case "UInt64":
                    return "ulong";
                case "Boolean":
                    return "bool";
                case "String":
                    return "string";
                case "String[]":
                    return "string[]";
                case "Void":
                    return "void";
                case "Byte":
                    return "byte";
                case "Byte[]":
                    return "byte[]";
                case "Object[]":
                    return "object[]";
                case "Object":
                    return "object";
            }

            return name;
        }

        private static string GetUsualTypeName(Type type, bool isFullName = false)
        {
            return GetUsualTypeName(isFullName ? type.FullName : type.Name);
        }

        private static string GetGenericTypeName(
            Type paramType,
            bool fullName = false,
            bool useGlobalNamespace = false)
        {
            string genericTypeName = null;

            bool exitFlag = false;
            if (paramType.GetGenericArguments().Length != 0)
            {
                Type[] genericArgumentsType = paramType.GetGenericArguments();

                List<string> genericArgumentsList = new List<string>();
                foreach (Type type in genericArgumentsType)
                {
                    if (type.IsGenericParameter)
                        continue;

                    genericArgumentsList.Add(
                        GetGenericTypeName(
                            type,
                            fullName: fullName,
                            useGlobalNamespace: useGlobalNamespace));
                }

                string usualTypeName = GetUsualTypeName(paramType, fullName);

                genericTypeName = usualTypeName + "<" + string.Join(", ", genericArgumentsList.ToArray()) + ">";
                exitFlag = true;
            }

            if (!exitFlag)
            {
                genericTypeName = GetUsualTypeName(paramType, fullName);
            }

            if (genericTypeName == null)
                throw new Exception("genericTypeName == null for " + paramType.FullName);

            if (fullName && useGlobalNamespace)
            {
                genericTypeName = "global::" + genericTypeName;
            }

            return genericTypeName;
        }
    }
}

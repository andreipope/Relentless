using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Loom.Client.Protobuf;
using Loom.Google.Protobuf.Reflection;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;

namespace Loom.ZombieBattleground.Editor {
    public static class AotHintFileUpdater {
        private const string ProtobufAotCompilerHintFilePath = "Assets/Scripts/Generated/AotCompilerHint.cs";  
        
        public static void UpdateAotHint()
        {
            MonoScript aotCompilerHintFileAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(ProtobufAotCompilerHintFilePath);
            if (aotCompilerHintFileAsset == null)
                throw new FileNotFoundException(ProtobufAotCompilerHintFilePath);

            // Protobuf
            IEnumerable<MessageDescriptor> messageDescriptors =
                new MessageDescriptor[0]
                    .Concat(ZbCallsReflection.Descriptor.MessageTypes)
                    .Concat(ZbDataReflection.Descriptor.MessageTypes)
                    .Concat(ZbEnumsReflection.Descriptor.MessageTypes)
                    .Concat(TypesReflection.Descriptor.MessageTypes)
                    .Concat(PlasmaCashReflection.Descriptor.MessageTypes)
                    .Concat(AddressMapperReflection.Descriptor.MessageTypes)
                    .Concat(LoomReflection.Descriptor.MessageTypes)
                    .Concat(EvmReflection.Descriptor.MessageTypes)
                    .Concat(TransferGatewayReflection.Descriptor.MessageTypes)
                    .Concat(TransferGatewayReflection.Descriptor.MessageTypes);

            ProtobufAotHintGenerator protobufAotHintGenerator =
                new ProtobufAotHintGenerator("Loom.Google.Protobuf", messageDescriptors)
                {
                    LeadingSpaces = 12
                };

            string protobufAotHint = protobufAotHintGenerator.GenerateAotHint();

            string aotHintFilePath = AssetDatabase.GetAssetPath(aotCompilerHintFileAsset);
            string aotHintFileText = File.ReadAllText(aotHintFilePath);
            List<string> aotHintFileLines = Regex.Split(aotHintFileText, @"\r?\n|\r").ToList();
            int startIndex =
                aotHintFileLines
                    .Select((s, i) => new
                    {
                        line = s,
                        index = i
                    })
                    .First(o => o.line.Contains("// start"))
                    .index;
            int endIndex =
                aotHintFileLines
                    .Select((s, i) => new
                    {
                        line = s,
                        index = i
                    })
                    .First(o => o.line.Contains("// end"))
                    .index;

            aotHintFileLines.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
            aotHintFileLines.Insert(startIndex + 1, protobufAotHint);

            aotHintFileText = String.Join(Environment.NewLine, aotHintFileLines);
            File.WriteAllText(aotHintFilePath, aotHintFileText);

            AssetDatabase.ImportAsset(aotHintFilePath);
        }
    }
}

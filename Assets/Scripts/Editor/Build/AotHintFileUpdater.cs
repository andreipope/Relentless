#if ENABLE_IL2CPP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Loom.Client.Protobuf;
using Loom.Google.Protobuf.Reflection;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class AotHintFileUpdater : IPreprocessBuildWithReport
    {
        private const string ProtobufAotCompilerHintFilePath = "Assets/Scripts/Generated/AotCompilerHint.cs";

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != )
                Debug.Log("Updating AOT hint");

            MonoScript aotCompilerHintFileAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(ProtobufAotCompilerHintFilePath);
            if (aotCompilerHintFileAsset == null)
                throw new FileNotFoundException(ProtobufAotCompilerHintFilePath);

            // Protobuf
            IEnumerable<MessageDescriptor> messageDescriptors =
                new MessageDescriptor[0]
                    .Concat(ZbReflection.Descriptor.MessageTypes)
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

#endif

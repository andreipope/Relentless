using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.Editor
{
    public class BuildMetaInfoGenerator : IPreprocessBuildWithReport
    {
        private const string BuildMetaInfoPath = "Assets/Resources/" + BuildMetaInfo.ResourcesPath + ".asset";

        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PreBuildInternal();
        }

        [InitializeOnLoadMethod]
        public static void CreateIfMissing()
        {
            GetBuildMetaInfo();
        }

        private static void PreBuildInternal()
        {
            BuildMetaInfo buildMetaInfo = GetBuildMetaInfo();
            buildMetaInfo.BuildDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ");
            buildMetaInfo.BuildDayOfYear = DateTime.UtcNow.DayOfYear;

#if !UNITY_CLOUD_BUILD
            string output;
            int exitCode;
            try
            {
                ExecuteCommand("git", "rev-parse --abbrev-ref HEAD", out output, out exitCode, timeout: 20000);
                if (exitCode != 0)
                    throw new Exception("exitCode != 0");

                buildMetaInfo.GitBranchName = output;
            }
            catch (Exception e)
            {
                buildMetaInfo.GitBranchName = "[error]";
                Debug.LogException(e);
            }

            try
            {
                ExecuteCommand("git", "log --pretty=format:%h -n 1", out output, out exitCode, timeout: 20000);
                if (exitCode != 0)
                    throw new Exception("exitCode != 0");

                buildMetaInfo.GitCommitHash = output;
            }
            catch (Exception e)
            {
                buildMetaInfo.GitCommitHash = "[error]";
                Debug.LogException(e);
            }

#endif

            UnityEditor.EditorUtility.SetDirty(buildMetaInfo);
        }

        // ReSharper disable once RedundantNameQualifier
        public static void PreCloudBuildExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
#if UNITY_CLOUD_BUILD
            Debug.Log("Cloud Build manifest:\r\n" + manifest.ToJson());
#endif
#if SECOND_PVP_BUILD
            PlayerSettings.applicationIdentifier = "games.loom.battleground2";
            PlayerSettings.productName = "Zombie Battleground Second PVP Build";
#endif
#if MAC_APPSTORE
            PlayerSettings.applicationIdentifier = "games.loom.battlegroundmacos";
            PlayerSettings.useMacAppStoreValidation = true;
#endif
            BuildMetaInfo buildMetaInfo = GetBuildMetaInfo();
            buildMetaInfo.GitBranchName = manifest.GetValue<string>("scmBranch");
            buildMetaInfo.GitCommitHash = manifest.GetValue<string>("scmCommitId");
            buildMetaInfo.CloudBuildBuildNumber = Convert.ToInt32(manifest.GetValue<string>("buildNumber"));
            buildMetaInfo.CloudBuildTargetName = manifest.GetValue<string>("cloudBuildTargetName");

            const int gitShortHashLength = 8;
            buildMetaInfo.GitCommitHash = buildMetaInfo.GitCommitHash.Substring(0,
                buildMetaInfo.GitCommitHash.Length > gitShortHashLength ?
                    gitShortHashLength :
                    buildMetaInfo.GitCommitHash.Length);

            UnityEditor.EditorUtility.SetDirty(buildMetaInfo);
        }

        private static BuildMetaInfo GetBuildMetaInfo()
        {
            BuildMetaInfo instance = BuildMetaInfo.Instance;
            if (instance != null)
            {
                return instance;
            }

            instance = ScriptableObject.CreateInstance<BuildMetaInfo>();
            AssetDatabase.CreateAsset(instance, BuildMetaInfoPath);

            return instance;
        }

        private static void ExecuteCommand(
            string fileName,
            string arguments,
            out string output,
            out int exitCode,
            string standardInput = null,
            Encoding standardInputEncoding = null,
            int timeout = 5000)
        {
            Process program = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = standardInput != null,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
            program.Start();
            if (standardInput != null)
            {
                StreamWriter streamWriter = new StreamWriter(program.StandardInput.BaseStream,
                    standardInputEncoding ?? new UTF8Encoding(false));
                streamWriter.Write(standardInput);
                streamWriter.Close();
            }

            string result = program.StandardOutput.ReadToEnd().Trim();
            bool exited = program.WaitForExit(timeout);
            if (!exited)
            {
                if (!program.HasExited)
                {
                    program.Kill();
                }

                throw new TimeoutException($"'{fileName} {arguments}' did not finish in time, output: \r\n{result}");
            }

            output = result;
            exitCode = program.ExitCode;
        }
    }
}

#if ENABLE_IL2CPP

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Loom.ZombieBattleground.Editor
{
    public class AotHintFileUpdateBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            AotHintFileUpdater.UpdateAotHint();
        }
    }
}

#endif

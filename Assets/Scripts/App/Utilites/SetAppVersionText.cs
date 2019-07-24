using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    [RequireComponent(typeof(Text))]
    public class SetAppVersionText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Text>().text = BuildMetaInfo.Instance.FullVersionName;
        }
    }
}

using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Loom.ZombieBattleground
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SetAppVersionTextMeshPro : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<TextMeshProUGUI>().text = BuildMetaInfo.Instance.FullVersionName;
        }
    }
}
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    [RequireComponent(typeof(Text))]
    public class SetAppVersionText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Text>().text = Constants.KCurrentVersionDevelopmentStage + " " + BuildMetaInfo.Instance.FullVersionName;
        }
    }
}

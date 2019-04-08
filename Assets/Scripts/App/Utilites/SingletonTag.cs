using System.Linq;
using log4net;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    [DisallowMultipleComponent]
    public class SingletonTag : MonoBehaviour
    {
        private static readonly ILog Log = Logging.GetLog(nameof(SingletonTag));

        private void Start()
        {
            string tag = gameObject.tag;
            SingletonTag existingSingleton =
                FindObjectsOfType<SingletonTag>()
                    .FirstOrDefault(singletonTag => singletonTag != this && singletonTag.gameObject.CompareTag(tag));

            if (existingSingleton != null)
            {
                gameObject.SetActive(false);
                Log.Warn($"Found another {nameof(SingletonTag)} with tag {tag}, disabled itself");
            }
        }
    }
}

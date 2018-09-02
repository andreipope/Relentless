using System.Linq;
using UnityEngine;

namespace LoomNetwork.CZB
{
    [DisallowMultipleComponent]
    public class SingletonTag : MonoBehaviour
    {
        private void Start()
        {
            string tag = gameObject.tag;
            SingletonTag existingSingleton =
                FindObjectsOfType<SingletonTag>()
                    .FirstOrDefault(singletonTag => singletonTag != this && singletonTag.gameObject.CompareTag(tag));

            if (existingSingleton != null)
            {
                gameObject.SetActive(false);
                Debug.LogWarning($"Found another {nameof(SingletonTag)} with tag {tag}, disabled itself");
            }
        }
    }
}

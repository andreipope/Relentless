using System;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class HiddenUIOpener : MonoBehaviour
    {
        public GameObject HiddenUI;
        public BaseRaycaster[] AlwaysActiveRaycasters = new BaseRaycaster[0];

        private AFPSCounter _afpsCounter;
        private bool _isVisible;

        private bool ShouldBeVisible => _afpsCounter.OperationMode == OperationMode.Normal;

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            _afpsCounter = FindObjectOfType<AFPSCounter>();
            if (_afpsCounter == null)
                throw new Exception("AFPSCounter instance not found in scene");
        }

        // Update is called once per frame
        void Update()
        {
            if (_isVisible != ShouldBeVisible)
            {
                BaseRaycaster[] raycasters = FindObjectsOfType<BaseRaycaster>();
                bool raycastersEnabled = !ShouldBeVisible;
                foreach (BaseRaycaster raycaster in raycasters)
                {
                    if (AlwaysActiveRaycasters.Contains(raycaster))
                        continue;

                    raycaster.enabled = raycastersEnabled;
                }

                // Update UI
                if (this.HiddenUI != null)
                {
                    HiddenUI.gameObject.SetActive(ShouldBeVisible);
                }

                _isVisible = ShouldBeVisible;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class ResolutionScaler : MonoBehaviour
    {
        [SerializeField]
        private Canvas[] _canvases;

        [SerializeField]
        private Camera[] _cameras;

        [SerializeField]
        private float _squareFactorCanvasMatch = 0.6f;

        [SerializeField]
        private float _squareFactorCameraSize = 9.6f;

        private Vector2 _screenSize;

        private bool _squareFactorScreen;

        private const float Scale_Factor = 1.5f;

        void Start()
        {
            SettingScreenSize();
            SettingCanvases();
            SettingCameras();

            SettingsWithCreditsPopup.OnResolutionOrScreenModeHasChanged += () =>
            {
                SettingScreenSize();
                SettingCanvases();
                SettingCameras();
            };
        }

        private void SettingScreenSize()
        {
            _screenSize = new Vector2(Screen.width, Screen.height);

            if (_canvases.Length > 0)
            {
                int displayIndex = _canvases[0].targetDisplay;
                if (displayIndex > 0 && displayIndex < Display.displays.Length)
                {
                    Display disp = Display.displays[displayIndex];
                    _screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
                }
            }

            if (_screenSize.x / _screenSize.y < Scale_Factor)
                _squareFactorScreen = true;
        }

        private void SettingCanvases()
        {
            float canvasMatchParam = _squareFactorScreen ? _squareFactorCanvasMatch : 1f;

            foreach (Canvas canvas in _canvases)
                canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasMatchParam;
        }

        private void SettingCameras()
        {
            float cameraSize = _squareFactorScreen ? _squareFactorCameraSize : _cameras[0].orthographicSize;

            foreach (Camera camera in _cameras)
                camera.orthographicSize = cameraSize;
        }
    }
}

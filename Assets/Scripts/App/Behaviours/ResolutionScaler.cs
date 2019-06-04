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

        private const float SquareFactorCanvasMatch = 0.6f;

        private const float SquareFactorCameraSize = 9.6f;
        
        private const float NonSquareFactorCameraSize = 7.6f;

        private const float ScaleFactor = 1.5f;

        private Vector2 _screenSize;

        private bool _squareFactorScreen;

        void Start()
        {
            UpdateScale();

            ApplicationSettingsManager.OnResolutionChanged += UpdateScale;
        }

        private void OnDestroy()
        {
            ApplicationSettingsManager.OnResolutionChanged -= UpdateScale;
        }

        private void UpdateScale()
        {
            SettingScreenSize();
            SettingCanvases();
            SettingCameras();
        }

        private void SettingScreenSize()
        {
            _screenSize = new Vector2(Screen.width, Screen.height);

            if (_canvases.Length > 0 && _canvases[0] != null)
            {
                int displayIndex = _canvases[0].targetDisplay;
                if (displayIndex > 0 && displayIndex < Display.displays.Length)
                {
                    Display disp = Display.displays[displayIndex];
                    _screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
                }
            }

            _squareFactorScreen = _screenSize.x / _screenSize.y < ScaleFactor;
        }

        private void SettingCanvases()
        {
            if (_canvases == null || _canvases.Length == 0 || _canvases[0] == null)
                return;
                
            float canvasMatchParam = _squareFactorScreen ? SquareFactorCanvasMatch : 1f;

            foreach (Canvas canvas in _canvases)
                canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasMatchParam;
        }

        private void SettingCameras()
        {
            if (_cameras == null || _cameras [0] == null)
                return;
                
            float cameraSize = _squareFactorScreen ? SquareFactorCameraSize : NonSquareFactorCameraSize;
           
            foreach (Camera camera in _cameras)
                camera.orthographicSize = cameraSize;
        }
    }
}

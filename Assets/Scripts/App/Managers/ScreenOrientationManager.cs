// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class ScreenOrientationManager : IService, IScreenOrientationManager
    {
        private IUIManager _uiManager;

        private CanvasScaler _uiCanvasScaler;

        private Vector2 _defaultReferenceResolution,
                        _invertReferenceResolution;

        private Enumerators.ScreenOrientationMode _currentOrientation;

        public Enumerators.ScreenOrientationMode CurrentOrientation
        {
            get
            {
                return _currentOrientation;
            }
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();

            _uiCanvasScaler = _uiManager.Canvas.GetComponent<CanvasScaler>();

            _defaultReferenceResolution = _uiCanvasScaler.referenceResolution;
            _invertReferenceResolution = new Vector2(_defaultReferenceResolution.y, _defaultReferenceResolution.x);

            if (_uiCanvasScaler.referenceResolution.x < _uiCanvasScaler.referenceResolution.y)
                _currentOrientation = Enumerators.ScreenOrientationMode.PORTRAIT;
            else
                _currentOrientation = Enumerators.ScreenOrientationMode.PORTRAIT;
        }

        public void Update()
        {
        }

        public void SwitchOrientation(Enumerators.ScreenOrientationMode mode)
        {
            switch(mode)
            {
                case Enumerators.ScreenOrientationMode.PORTRAIT:
                    {
                        if (_defaultReferenceResolution.x < _defaultReferenceResolution.y)
                            _uiCanvasScaler.referenceResolution = _defaultReferenceResolution;
                        else
                            _uiCanvasScaler.referenceResolution = _invertReferenceResolution;

                        Screen.orientation = ScreenOrientation.Portrait;
                        _currentOrientation = Enumerators.ScreenOrientationMode.PORTRAIT;
                    }
                    break;
                case Enumerators.ScreenOrientationMode.LANDSCAPE:
                    {
                        if (_defaultReferenceResolution.x < _defaultReferenceResolution.y)
                            _uiCanvasScaler.referenceResolution = _invertReferenceResolution;
                        else
                            _uiCanvasScaler.referenceResolution = _defaultReferenceResolution;
                        
                        Screen.orientation = ScreenOrientation.LandscapeLeft;
                        _currentOrientation = Enumerators.ScreenOrientationMode.LANDSCAPE;
                    }
                    break;

                default: break;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.CZB.Gameplay
{
    public class CameraManager : IService, ICameraManager
    {
        private IUIManager _uiManager;
        private ITimerManager _timerManager;

        private CanvasGroup _fadeImageGroup;

        private float _fadeSpeed = 10f,
                      _fadeThreshold = 0.01f,
                      _fadeDelay = 0.01f,
                      _fadeGoalValue = 1f;

        private bool _isFading = false;
        private Enumerators.FadeState _currentFadeState;

        public bool IsFading
        {
            get
            {
                return _isFading;
            }
        }

        public Enumerators.FadeState CurrentFadeState
        {
            get 
            {
                return _currentFadeState;
            }
        }

        public void Dispose()
        {

        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _fadeImageGroup = _uiManager.Canvas.transform.Find("Image_Fade").GetComponent<CanvasGroup>();
            _fadeImageGroup.alpha = 0f;
        }

        public void Update()
        {
        }

        public void FadeIn(Action callback = null)
        {
			_fadeGoalValue = 1f;
			PrepareFading(true);
            _timerManager.AddTimer(Fade, new object[] { true, callback }, _fadeDelay, true);
        }

		public void FadeIn(float fadeValue)
		{
            _fadeGoalValue = fadeValue;
			PrepareFading(true);
			_timerManager.AddTimer(Fade, new object[] { true, null }, _fadeDelay, true);
		}

        public void FadeOut(Action callback = null)
        {
            PrepareFading(false);
            _timerManager.AddTimer(Fade, new object[] { false, callback }, _fadeDelay, true);
        }

        private void PrepareFading(bool fadeIn)
        {
            _timerManager.StopTimer(Fade);
            _fadeImageGroup.alpha = fadeIn ? 0f : _fadeGoalValue;
            _fadeImageGroup.transform.SetAsLastSibling();
            _fadeImageGroup.gameObject.SetActive(true);
            _isFading = true;
        }

        private void Fade(object[] param)
        {
            bool fadeIn = (bool)param[0];
            Action callback = (param[1] == null) ? null : (Action)param[1];

            float speed = Time.deltaTime * _fadeSpeed;
            float to = fadeIn ? _fadeGoalValue : 0f;

            _fadeImageGroup.alpha = Mathf.Lerp(_fadeImageGroup.alpha, to, speed);

            if (Mathf.Abs(_fadeImageGroup.alpha - to) < _fadeThreshold)
            {
                _currentFadeState = fadeIn ? Enumerators.FadeState.FADED : Enumerators.FadeState.DEFAULT;

                if (_currentFadeState == Enumerators.FadeState.DEFAULT)
                    _fadeImageGroup.gameObject.SetActive(false);

                _fadeImageGroup.alpha = to;
                _isFading = false;
                _timerManager.StopTimer(Fade);

                if (callback != null)
                    callback();
            }
        }
    }
}
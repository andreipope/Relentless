// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB.Gameplay
{
    public class CameraManager : IService, ICameraManager
    {
        private IUIManager _uiManager;
        private ITimerManager _timerManager;

		private CanvasGroup[] _fadeImageGroups;

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
            _fadeImageGroups = new CanvasGroup[3];
			//            _selfPage.transform.SetParent(GameObject.Find("CanvasTutorial").transform, false);

			_fadeImageGroups[0] = GameObject.Find("Canvas1/Image_Fade").GetComponent<CanvasGroup>();
			_fadeImageGroups[1] = GameObject.Find("Canvas2/Image_Fade").GetComponent<CanvasGroup>();
			_fadeImageGroups[2] = GameObject.Find("Canvas3/Image_Fade").GetComponent<CanvasGroup>();
			_fadeImageGroups[0].alpha = 0f;
			_fadeImageGroups[1].alpha = 0f;
			_fadeImageGroups[2].alpha = 0f;
			_fadeImageGroups[0].gameObject.SetActive(false);
			_fadeImageGroups[1].gameObject.SetActive(false);
			_fadeImageGroups[2].gameObject.SetActive(false);
                                   
        }

        public void Update()
        {
        }

        public void FadeIn(Action callback = null, int level = 0)
        {
			_fadeGoalValue = 1f;
			PrepareFading(true, level);
            _timerManager.AddTimer(Fade, new object[] { true, callback, level }, _fadeDelay, true);
        }

		public void FadeIn(float fadeValue, int level = 0)
		{
            _fadeGoalValue = fadeValue;
			PrepareFading(true, level);
			_timerManager.AddTimer(Fade, new object[] { true, null, level }, _fadeDelay, true);
		}

        public void FadeOut(Action callback = null, int level = 0, bool immediately = false)
        {
            if(immediately)
            {
                _fadeImageGroups[level].alpha = 0;
                _fadeImageGroups[level].gameObject.SetActive(false);
                return;
            }

			if (_timerManager == null)
				return;
            PrepareFading(false, level);
            _timerManager.AddTimer(Fade, new object[] { false, callback, level }, _fadeDelay, true);
        }

        private void PrepareFading(bool fadeIn, int level)
        {
            _timerManager.StopTimer(Fade);
            _fadeImageGroups[level].alpha = fadeIn ? 0f : _fadeGoalValue;
            _fadeImageGroups[level].transform.SetAsLastSibling();
            _fadeImageGroups[level].gameObject.SetActive(true);
            _isFading = true;
        }

        private void Fade(object[] param)
        {
            bool fadeIn = (bool)param[0];
            int level = (int)param[2];
            Action callback = (param[1] == null) ? null : (Action)param[1];

            float speed = Time.deltaTime * _fadeSpeed;
            float to = fadeIn ? _fadeGoalValue : 0f;

            _fadeImageGroups[level].alpha = Mathf.Lerp(_fadeImageGroups[level].alpha, to, speed);

            if (Mathf.Abs(_fadeImageGroups[level].alpha - to) < _fadeThreshold)
            {
                _currentFadeState = fadeIn ? Enumerators.FadeState.FADED : Enumerators.FadeState.DEFAULT;

                if (_currentFadeState == Enumerators.FadeState.DEFAULT)
                    _fadeImageGroups[level].gameObject.SetActive(false);

                _fadeImageGroups[level].alpha = to;
                _isFading = false;
                _timerManager.StopTimer(Fade);

                if (callback != null)
                    callback();
            }
        }
    }
}
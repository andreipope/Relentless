using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB.Gameplay
{
    public class CameraManager : IService, ICameraManager
    {
        private readonly float _fadeSpeed = 10f;

        private readonly float _fadeThreshold = 0.01f;

        private readonly float _fadeDelay = 0.01f;

        private IUIManager _uiManager;

        private ITimerManager _timerManager;

        private CanvasGroup[] _fadeImageGroups;

        private float _fadeGoalValue = 1f;

        public bool IsFading { get; private set; }

        public Enumerators.FadeState CurrentFadeState { get; private set; }

        public void FadeIn(Action callback = null, int level = 0, bool isLastSibling = true)
        {
            _fadeGoalValue = 1f;
            PrepareFading(true, level, isLastSibling);
            _timerManager.AddTimer(Fade, new object[] { true, callback, level }, _fadeDelay, true);
        }

        public void FadeIn(float fadeValue, int level = 0, bool isLastSibling = true)
        {
            _fadeGoalValue = fadeValue;
            PrepareFading(true, level, isLastSibling);
            _timerManager.AddTimer(Fade, new object[] { true, null, level }, _fadeDelay, true);
        }

        public void FadeOut(Action callback = null, int level = 0, bool immediately = false)
        {
            if (!_fadeImageGroups[level].gameObject.activeInHierarchy)
                return;

            if (immediately)
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

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _fadeImageGroups = new CanvasGroup[3];

            // _selfPage.transform.SetParent(GameObject.Find("CanvasTutorial").transform, false);
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

        private void PrepareFading(bool fadeIn, int level, bool isLastSibling = true)
        {
            _timerManager.StopTimer(Fade);
            _fadeImageGroups[level].alpha = fadeIn?0f:_fadeGoalValue;
            if (isLastSibling)
            {
                _fadeImageGroups[level].transform.SetAsLastSibling();
            }

            _fadeImageGroups[level].gameObject.SetActive(true);
            IsFading = true;
        }

        private void Fade(object[] param)
        {
            bool fadeIn = (bool)param[0];
            int level = (int)param[2];
            Action callback = param[1] == null?null:(Action)param[1];

            float speed = Time.deltaTime * _fadeSpeed;
            float to = fadeIn?_fadeGoalValue:0f;

            _fadeImageGroups[level].alpha = Mathf.Lerp(_fadeImageGroups[level].alpha, to, speed);

            if (Mathf.Abs(_fadeImageGroups[level].alpha - to) < _fadeThreshold)
            {
                CurrentFadeState = fadeIn?Enumerators.FadeState.FADED:Enumerators.FadeState.DEFAULT;

                if (CurrentFadeState == Enumerators.FadeState.DEFAULT)
                {
                    _fadeImageGroups[level].gameObject.SetActive(false);
                }

                _fadeImageGroups[level].alpha = to;
                IsFading = false;
                _timerManager.StopTimer(Fade);

                callback?.Invoke();
            }
        }
    }
}

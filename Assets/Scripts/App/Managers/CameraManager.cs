using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground.Gameplay
{
    public class CameraManager : IService, ICameraManager
    {
        private const float FadeSpeed = 10f;

        private const float FadeThreshold = 0.01f;

        private const float FadeDelay = 0.01f;

        private ITimerManager _timerManager;

        private IUIManager _uiManager;

        private CanvasGroup[] _fadeImageGroups;

        private float _fadeGoalValue = 1f;

        private GameObject _gameplayCamerasObject;

        private GameObject _otherCamerasObject;

        private Dictionary<Enumerators.ShakeType, Vector3[]> _shakePoints;

        public bool IsFading { get; private set; }

        public Enumerators.FadeState CurrentFadeState { get; private set; }

        public void FadeIn(Action callback = null, int level = 0, bool isLastSibling = true)
        {
            _fadeGoalValue = 1f;
            PrepareFading(true, level, isLastSibling);
            _timerManager.AddTimer(Fade, new object[]
            {
                true, callback, level
            }, FadeDelay, true);
        }

        public void FadeIn(float fadeValue, int level = 0, bool isLastSibling = true)
        {
            _fadeGoalValue = fadeValue;
            PrepareFading(true, level, isLastSibling);
            _timerManager.AddTimer(Fade, new object[]
            {
                true, null, level
            }, FadeDelay, true);
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
            _timerManager.AddTimer(Fade, new object[]
            {
                false, callback, level
            }, FadeDelay, true);
        }

        public void ShakeGameplay(Enumerators.ShakeType type)
        {
            Vector3[] uiPoints = new Vector3[_shakePoints[type].Length];
            for (int i = 0; i < _shakePoints[type].Length; i++)
            {
                uiPoints[i] = Camera.main.ScreenToViewportPoint(_shakePoints[type][i]);
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_gameplayCamerasObject.transform.DOPath(_shakePoints[type], 0.05f));
            sequence.Append(_otherCamerasObject.transform.DOPath(_shakePoints[type], 0.05f));
            sequence.Append(_uiManager.GetPage<GameplayPage>().Self.transform.DOPath(uiPoints, 0.05f));
        }

        public Transform GetGameplayCameras()
        {
            return _gameplayCamerasObject.transform;
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _timerManager = GameClient.Get<ITimerManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _fadeImageGroups = new CanvasGroup[3];

            _fadeImageGroups[0] = GameObject.Find("Canvas1/Image_Fade").GetComponent<CanvasGroup>();
            _fadeImageGroups[1] = GameObject.Find("Canvas2/Image_Fade").GetComponent<CanvasGroup>();
            _fadeImageGroups[2] = GameObject.Find("Canvas3/Image_Fade").GetComponent<CanvasGroup>();
            _fadeImageGroups[0].alpha = 0f;
            _fadeImageGroups[1].alpha = 0f;
            _fadeImageGroups[2].alpha = 0f;
            _fadeImageGroups[0].gameObject.SetActive(false);
            _fadeImageGroups[1].gameObject.SetActive(false);
            _fadeImageGroups[2].gameObject.SetActive(false);

            _otherCamerasObject = GameObject.Find("Cameras");

            FillShakePoints();

            GameClient.Get<IGameplayManager>().GameInitialized += GameInitializedEventHandler;
        }

        private void FillShakePoints()
        {
            _shakePoints = new Dictionary<Enumerators.ShakeType, Vector3[]>();
            _shakePoints.Add(Enumerators.ShakeType.Short, new Vector3[]
            {
                new Vector2(0, 0.19f),
                new Vector2(0, -0.03f),
                Vector3.zero
            });
            _shakePoints.Add(Enumerators.ShakeType.Medium, new Vector3[]
            {
                new Vector2(0.07f, -0.125f),
                new Vector2(0.1f, 0.027f),
                new Vector2(0.83f, -0.10f),
                new Vector2(0.83f, 0.054f),
                Vector3.zero
            });
        }

        public void Update()
        {

        }

        private void GameInitializedEventHandler()
        {
            _gameplayCamerasObject = GameObject.Find("GamePlayCameras");
        }

        private void PrepareFading(bool fadeIn, int level, bool isLastSibling = true)
        {
            _timerManager.StopTimer(Fade);
            _fadeImageGroups[level].alpha = fadeIn ? 0f : _fadeGoalValue;
            if (isLastSibling)
            {
                _fadeImageGroups[level].transform.SetAsLastSibling();
            }

            _fadeImageGroups[level].gameObject.SetActive(true);
            IsFading = true;
        }

        private void Fade(object[] param)
        {
            bool fadeIn = (bool) param[0];
            int level = (int) param[2];
            Action callback = param[1] == null ? null : (Action) param[1];

            float speed = Time.deltaTime * FadeSpeed;
            float to = fadeIn ? _fadeGoalValue : 0f;

            _fadeImageGroups[level].alpha = Mathf.Lerp(_fadeImageGroups[level].alpha, to, speed);

            if (Mathf.Abs(_fadeImageGroups[level].alpha - to) < FadeThreshold)
            {
                CurrentFadeState = fadeIn ? Enumerators.FadeState.FADED : Enumerators.FadeState.DEFAULT;

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

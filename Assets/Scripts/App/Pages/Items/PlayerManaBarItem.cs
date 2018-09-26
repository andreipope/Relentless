using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PlayerManaBarItem
    {
        private const int MeterArrowStep = 12;

        private readonly GameObject _selfObject;

        private readonly GameObject _arrowObject;

        private readonly GameObject _gooMeterObject;

        private readonly TextMeshPro _gooAmountText;

        private readonly List<GooBottleItem> _gooBottles;

        private readonly Vector3 _overflowPos;

        private readonly string _overflowPrefabPath;

        private GameObject _overflowObject;

        private TextMeshPro _overflowGooAmountText;
		
		private TextMeshPro _nameText;

        private Transform _overflowBottleContainer;

        private int _maxValue, _currentValue;

        private string _name;

        private bool _isInOverflow;

        public PlayerManaBarItem(GameObject gameObject, string overflowPrefabName, Vector3 overflowPos, string name)
        {
            _overflowPrefabPath = "Prefabs/" + overflowPrefabName;
            _overflowPos = overflowPos;
            _selfObject = gameObject;
            _gooMeterObject = _selfObject.transform.Find("GooMeter").gameObject;
            _gooAmountText = _gooMeterObject.transform.Find("Text").GetComponent<TextMeshPro>();
            _arrowObject = _gooMeterObject.transform.Find("ArrowCenter").gameObject;
            _gooBottles = new List<GooBottleItem>();
            for (int i = 0; i < _selfObject.transform.childCount; i++)
            {
                GameObject bottle = _selfObject.transform.GetChild(i).gameObject;
                if (bottle.name.Contains("BottleGoo"))
                {
                    _gooBottles.Add(new GooBottleItem(bottle));
                }
            }

            _isInOverflow = false;
            _name = name;
            _arrowObject.transform.localEulerAngles = Vector3.forward * 90;

            GameClient.Get<IGameplayManager>().GameEnded += GameEndedHandler;
        }

        public void SetGoo(int gooValue)
        {
            _currentValue = gooValue;
            _gooAmountText.text = _currentValue + "/" + _maxValue;

            UpdateGooOVerflow();

            for (int i = 0; i < _gooBottles.Count; i++)
            {
                if (i < _currentValue)
                {
                    Active(_gooBottles[i]);
                }
                else
                {
                    Disactive(_gooBottles[i]);
                }
            }

            UpdateGooMeter();
        }

        public void SetVialGoo(int maxValue)
        {
            _maxValue = maxValue;
            _gooAmountText.text = _currentValue + "/" + _maxValue;
            for (int i = 0; i < _gooBottles.Count; i++)
            {
                _gooBottles[i].Self.SetActive(i < _maxValue ? true : false);
            }

            UpdateGooOVerflow();
        }

        public void Active(GooBottleItem item)
        {
            item.FullBoottle.DOFade(1.0f, 0.5f);
            item.GlowBottle.DOFade(1.0f, 0.5f);
        }

        public void Disactive(GooBottleItem item)
        {
            item.FullBoottle.DOFade(0.0f, 0.5f);
            item.GlowBottle.DOFade(0.0f, 0.5f);
        }

        private void UpdateGooOVerflow()
        {
            if (_currentValue > _maxValue && !_isInOverflow)
            {
                CreateOverflow();

                _isInOverflow = true;
            }
            else if (_currentValue <= _maxValue && _isInOverflow)
            {
                DestroyOverflow();

                _isInOverflow = false;
            }

            if (_overflowGooAmountText != null)
            {
                _overflowGooAmountText.text = _currentValue + "/" + _maxValue;
                for (int i = 0; i < _overflowBottleContainer.childCount; i++)
                {
                    _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue ? true : false);
                }
            }
        }

        private void UpdateGooMeter()
        {
            int targetRotation = 90 - MeterArrowStep * _currentValue;
            if (targetRotation < -90)
            {
                targetRotation = -90;
            }

            _arrowObject.transform.DORotate(Vector3.forward * targetRotation, 1f);
        }

        private void CreateOverflow()
        {
            _overflowObject = Object.Instantiate(GameClient.Get<ILoadObjectsManager>()
                .GetObjectByPath<GameObject>(_overflowPrefabPath));
            _overflowObject.transform.localPosition = _overflowPos;
            _overflowGooAmountText = _overflowObject.transform.Find("clock/Text").GetComponent<TextMeshPro>();
            _overflowBottleContainer = _overflowObject.transform.Find("Bottles").transform;
			_nameText = _overflowObject.transform.Find("NameText").GetComponent<TextMeshPro>();
            _nameText.text = _name;
            for (int i = 0; i < _overflowBottleContainer.childCount; i++)
            {
                _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue ? true : false);
            }

            _selfObject.SetActive(false);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.GOO_OVERFLOW_FADE_IN,
                Constants.BattlegroundEffectsSoundVolume);

            GameClient.Get<ITimerManager>().AddTimer(PlayOverflowLoopDelay, null,
                GameClient.Get<ISoundManager>().GetSoundLength(Enumerators.SoundType.GOO_OVERFLOW_FADE_IN));
        }

        private void PlayOverflowLoopDelay(object[] param)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.GOO_OVERFLOW_FADE_LOOP,
                Constants.BattlegroundEffectsSoundVolume, true);
        }

        private void DestroyOverflow()
        {
            Object.Destroy(_overflowObject);
            _overflowObject = null;
            _overflowBottleContainer = null;
            _overflowGooAmountText = null;
            _selfObject.SetActive(true);

            StopOverfowSounds();

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.GOO_OVERFLOW_FADE_OUT,
                Constants.BattlegroundEffectsSoundVolume);
        }

        private void StopOverfowSounds()
        {
            GameClient.Get<ITimerManager>().StopTimer(PlayOverflowLoopDelay);

            GameClient.Get<ISoundManager>().StopPlaying(Enumerators.SoundType.GOO_OVERFLOW_FADE_IN);
            GameClient.Get<ISoundManager>().StopPlaying(Enumerators.SoundType.GOO_OVERFLOW_FADE_LOOP);
            GameClient.Get<ISoundManager>().StopPlaying(Enumerators.SoundType.GOO_OVERFLOW_FADE_OUT);
        }

        private void GameEndedHandler(Enumerators.EndGameType obj)
        {
            StopOverfowSounds();
            _gooMeterObject.SetActive(false);

            _isInOverflow = false;

            GameClient.Get<IGameplayManager>().GameEnded -= GameEndedHandler;
        }

        public struct GooBottleItem
        {
            public SpriteRenderer FullBoottle, GlowBottle;

            public GameObject Self;

            public GooBottleItem(GameObject gameObject)
            {
                Self = gameObject;
                FullBoottle = Self.transform.Find("Goo").GetComponent<SpriteRenderer>();
                GlowBottle = Self.transform.Find("BottleGlow").GetComponent<SpriteRenderer>();
            }
        }
    }
}

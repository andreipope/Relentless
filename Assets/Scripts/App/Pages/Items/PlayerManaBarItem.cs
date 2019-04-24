using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PlayerManaBarItem
    {
        private const int MeterArrowStep = 18;       

        private readonly GameObject _selfObject;

        private readonly GameObject _arrowObject;

        private readonly GameObject _gooMeterObject;

        private readonly TextMeshPro _gooAmountText;

        private readonly List<GooBottleItem> _gooBottles;

        private readonly Vector3 _overflowPos;

        private readonly string _overflowPrefabPath;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private GameObject _overflowObject;

        private GameObject _vialGooPrefab;

        private TextMeshPro _overflowGooAmountText;
        
		private TextMeshPro _nameText;

        private Transform _overflowBottleContainer;
        private Transform _overflowYellowBottleContainer;

        private int _maxValue, _currentValue;

        private string _name;

        private bool _isInOverflow, _isAfterOverflow;

        private bool _isLocalOwner;

        public PlayerManaBarItem(GameObject gameObject, string overflowPrefabName, Vector3 overflowPos, string name, string objectName)
        {
            _overflowPrefabPath = "Prefabs/" + overflowPrefabName;
            _overflowPos = overflowPos;
            _selfObject = gameObject;
            _gooMeterObject = GameObject.Find(objectName + "/OverlordArea/RegularModel/RegularPosition/Gauge/CZB_3D_Overlord_gauge_LOD0").gameObject;
            _gooAmountText = _gooMeterObject.transform.Find("Text").GetComponent<TextMeshPro>();
            _arrowObject = _gooMeterObject.transform.Find("gauge_indicator_LOD0").gameObject;
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
            _isAfterOverflow = false;
            _name = name;
            _arrowObject.transform.localEulerAngles = new Vector3(_arrowObject.transform.localEulerAngles.x,
                                                                  _arrowObject.transform.localEulerAngles.y,
                                                                  -90);

            _isLocalOwner = objectName == Constants.Player;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _gameplayManager.GameEnded += GameEndedHandler;
            _gameplayManager.GameInitialized += GameInitializedHandler;
        }

        private void ManaBarSelectedEventHandler(GameObject obj)
        {
            if(_selfObject == obj)
            {
                if (_isLocalOwner)
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerManaBarSelected);
                }
            }
        }

        public void SetGoo(int gooValue)
        {
            InternalTools.DoActionDelayed(() =>
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
                _isAfterOverflow = false;
                UpdateGooMeter();

            }, 0.1f);
        }

        public void SetViaGooPrefab(GameObject gooPrefab)
        {
            _vialGooPrefab = gooPrefab;
        }

        public void SetVialGoo(int maxValue, bool disableAddedBottles = false)
        {
            int oldMaxValue = _maxValue;
            _maxValue = maxValue;
            _gooAmountText.text = _currentValue + "/" + _maxValue;
            for (int i = 0; i < _gooBottles.Count; i++)
            {
                _gooBottles[i].Self.SetActive(i < _maxValue ? true : false);
                if(i >= oldMaxValue && i < _maxValue)
                {
                    CreateVialGooVfx(_gooBottles[i]);

                    if(disableAddedBottles)
                    {
                        Disactive(_gooBottles[i]);
                    }
                }
            }
            _vialGooPrefab = null;
            UpdateGooOVerflow();
        }

        private void CreateVialGooVfx(GooBottleItem bootle)
        {
            if (_vialGooPrefab == null)
                return;

            bootle.Self.SetActive(false);
            GameObject VfxObject = Object.Instantiate(_vialGooPrefab);
            VfxObject.transform.position = bootle.Self.transform.position;
            _gameplayManager.GetController<ParticlesController>().RegisterParticleSystem(VfxObject, true, 4.5f);
            InternalTools.DoActionDelayed(() =>
            {
                bootle.Self.SetActive(true);
                bootle.ChangeStateParticles(false);
            }, 1.5f);
        }

        public void Active(GooBottleItem item)
        {
            if (item.Self.activeInHierarchy)
            {
                item.selfAnimator.SetBool("IsFull", true);
                if (_isAfterOverflow)
                {
                    item.selfAnimator.Play("gooFilling", 0, 1);
                }
            }

            item.ChangeStateParticles(true);
        }

        public void Disactive(GooBottleItem item)
        {
            if (item.selfAnimator.gameObject.activeInHierarchy) {
			    item.selfAnimator.SetBool("IsFull", false);
                item.ChangeStateParticles(false);
            }
        }

        private void UpdateGooOVerflow()
        {
            if (_currentValue > _maxValue && !_isInOverflow)
            {
                CreateOverflow();

                _isInOverflow = true;

                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, "CZB_AUD_Overflow_Card_Mechanic_F1_EXP", Constants.SfxSoundVolume, false, true);
            }
            else if (_currentValue <= _maxValue && _isInOverflow)
            {
                DestroyOverflow();
                _isAfterOverflow = true;
                _isInOverflow = false;
            }

            if (_overflowGooAmountText != null)
            {
                _overflowGooAmountText.text = _currentValue + "/" + _maxValue;
                for (int i = 0; i < _overflowBottleContainer.childCount; i++)
                {
                    _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _maxValue ? true : false);
                }
                for (int i = 0; i < _overflowYellowBottleContainer.childCount; i++)
                {
                    _overflowYellowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue && i >= _maxValue ? true : false);
                }
            }
        }

        private void UpdateGooMeter()
        {
            int targetRotation = -90 + MeterArrowStep * _currentValue;
            if (targetRotation > 90)
            {
                targetRotation = 90;
            }
            Vector3 rotation = _arrowObject.transform.eulerAngles;
            rotation.z = targetRotation;
            _arrowObject.transform.DORotate(rotation, 1f);
        }

        private void CreateOverflow()
        {
            _overflowObject = Object.Instantiate(GameClient.Get<ILoadObjectsManager>()
                .GetObjectByPath<GameObject>(_overflowPrefabPath));
            _overflowObject.transform.localPosition = _overflowPos;
            _overflowGooAmountText = _overflowObject.transform.Find("clock/Text").GetComponent<TextMeshPro>();
            _overflowBottleContainer = _overflowObject.transform.Find("Bottles").transform;
            _overflowYellowBottleContainer = _overflowObject.transform.Find("Bottle_Overflow").transform;
            _nameText = _overflowObject.transform.Find("NameText").GetComponent<TextMeshPro>();
            _nameText.text = _name;
            for (int i = 0; i < _overflowBottleContainer.childCount; i++)
            {
                _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _maxValue ? true : false);
            }
            for (int i = 0; i < _overflowYellowBottleContainer.childCount; i++)
            {
                _overflowYellowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue && i >= _maxValue ? true : false);
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
            _overflowYellowBottleContainer = null;
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

            _gameplayManager.GameEnded -= GameEndedHandler;
        }

        private void GameInitializedHandler()
        {
            if (_tutorialManager.IsTutorial)
            {
                _gameplayManager.GetController<InputController>().ManaBarSelected += ManaBarSelectedEventHandler;
                _gameplayManager.GetController<InputController>().ManaBarPointerEntered += ManaBarSelectedEventHandler;
            }          
        }

        public struct GooBottleItem
        {
            public GameObject Self;
                
			public Animator selfAnimator;

            public ParticleSystem glow;

            private ParticleSystem _buble;

            public GooBottleItem(GameObject gameObject)
            {
                Self = gameObject;
                selfAnimator = Self.GetComponent<Animator>();
                glow = Self.transform.Find("GlowBottle").GetComponent<ParticleSystem>();
                _buble = Self.transform.Find("New Sprite Mask/Buble").GetComponent<ParticleSystem>();
            }

            public void ChangeStateParticles(bool state)
            {
                if(state)
                {
                    glow.Play();
                    _buble.Play();
                }
                else
                {
                    glow.Stop();
                    _buble.Stop();
                }
            }
        }
    }
}

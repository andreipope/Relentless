using System;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.View;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class VfxController : IController
    {
        private const string TagZoneForTouching = "BattlegroundTouchingArea";

        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private ParticlesController _particlesController;

        private BattlegroundController _battlegroundController;

        private GameObject _battlegroundTouchPrefab;

        private List<UnitDeathAnimation> _unitDeathAnimations;

        public void Init()
        {
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _particlesController = _gameplayManager.GetController<ParticlesController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _battlegroundTouchPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/TouchingBattleground/ZB_ANM_touching_battleground");

            _unitDeathAnimations = new List<UnitDeathAnimation>();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (_gameplayManager.IsGameStarted)
            {
                ChechTouchOnBattleground();

                if(_unitDeathAnimations != null)
                {
                    foreach(UnitDeathAnimation deathAnimation in _unitDeathAnimations)
                    {
                        deathAnimation.Update();
                    }
                }
            }
        }

        public void ResetAll()
        {
            _unitDeathAnimations.Clear();
        }

        public void PlayAttackVfx(BoardUnitModel model, Vector3 target)
        {
            Enumerators.CardType type = model.Card.LibraryCard.CardType;
            int damage = model.CurrentDamage;
            GameObject effect;
            GameObject vfxPrefab;

            if (type == Enumerators.CardType.HEAVY)
            {
                target = Utilites.CastVfxPosition(target);
            }

            if (model.GameMechanicDescriptionsOnUnit.Exists(x => x == Enumerators.GameMechanicDescriptionType.Chainsaw))
            {
                vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/ChainSawAttack");
                effect = Object.Instantiate(vfxPrefab);
                effect.transform.position = target;
                _soundManager.PlaySound(Enumerators.SoundType.SPELLS, "ChainSaw_Impact", Constants.CreatureAttackSoundVolume, isLoop: false);

                _particlesController.RegisterParticleSystem(effect, true, 5f);
            }
            else
            {
                switch (type)
                {
                    case Enumerators.CardType.FERAL:
                        {
                            vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FeralAttackVFX");
                            effect = Object.Instantiate(vfxPrefab);
                            effect.transform.position = target;
                            _soundManager.PlaySound(Enumerators.SoundType.FERAL_ATTACK, Constants.CreatureAttackSoundVolume,
                                false, false, true);

                            _particlesController.RegisterParticleSystem(effect, true, 5f);

                            if (damage > 3 && damage < 7)
                            {
                                _timerManager.AddTimer(
                                    a =>
                                    {
                                        effect = Object.Instantiate(vfxPrefab);
                                        effect.transform.position = target;
                                        effect.transform.localScale = new Vector3(-1, 1, 1);
                                        _particlesController.RegisterParticleSystem(effect, true, 5f);
                                    },
                                    null,
                                    0.5f);
                            }

                            if (damage > 6)
                            {
                                _timerManager.AddTimer(
                                    a =>
                                    {
                                        effect = Object.Instantiate(vfxPrefab);
                                        effect.transform.position = target - Vector3.right;
                                        effect.transform.eulerAngles = Vector3.forward * 90;

                                        _particlesController.RegisterParticleSystem(effect, true, 5f);
                                    });
                            }

                            break;
                        }
                    case Enumerators.CardType.HEAVY:
                        {
                            Enumerators.SoundType soundType = Enumerators.SoundType.HEAVY_ATTACK_1;
                            string prefabName = "Prefabs/VFX/HeavyAttackVFX";
                            if (damage > 4)
                            {
                                prefabName = "Prefabs/VFX/HeavyAttack2VFX";
                                soundType = Enumerators.SoundType.HEAVY_ATTACK_2;
                            }

                            vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>(prefabName);
                            effect = Object.Instantiate(vfxPrefab);
                            effect.transform.position = target;

                            _particlesController.RegisterParticleSystem(effect, true, 5f);

                            _soundManager.PlaySound(soundType, Constants.CreatureAttackSoundVolume, false, false, true);
                            break;
                        }
                    default:
                        {
                            vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WalkerAttackVFX");
                            effect = Object.Instantiate(vfxPrefab);
                            effect.transform.position = target;

                            _particlesController.RegisterParticleSystem(effect, true, 5f);

                            _soundManager.PlaySound(Enumerators.SoundType.WALKER_ATTACK, Constants.CreatureAttackSoundVolume,
                                false, false, true);

                            if (damage > 4)
                            {
                                _timerManager.AddTimer(
                                    a =>
                                    {
                                        effect = Object.Instantiate(vfxPrefab);
                                        effect.transform.position = target;

                                        effect.transform.localScale = new Vector3(-1, 1, 1);
                                        _particlesController.RegisterParticleSystem(effect, true, 5f);
                                    },
                                    null,
                                    0.5f);
                            }

                            break;
                        }
                }
            }
        }

        public void CreateVfx(GameObject prefab, object target, bool autoDestroy = true, float delay = 3f, bool isIgnoreCastVfx = false)
        {
            if (prefab == null)
                return;

            Vector3 position = Vector3.zero;

            switch (target)
            {
                case BoardUnitView unit:
                    position = unit.Transform.position;
                    break;
                case BoardUnitModel unit:
                    position = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                    break;
                case Player player:
                    position = player.AvatarObject.transform.position;
                    break;
                case Transform transform:
                    position = transform.transform.position;
                    break;
                case Vector3 actualPosition:
                    position = actualPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            GameObject particle = Object.Instantiate(prefab);
            if (isIgnoreCastVfx)
            {
                particle.transform.position = position;
            }
            else
            {
                particle.transform.position = Utilites.CastVfxPosition(position + Vector3.forward);
            }
            _particlesController.RegisterParticleSystem(particle, autoDestroy, delay);
        }

        public void CreateVfx(GameObject prefab, Vector3 position, bool autoDestroy = true, float delay = 3f)
        {
            if (prefab == null)
                return;

            GameObject particle = Object.Instantiate(prefab);
            particle.transform.position = position;
            _particlesController.RegisterParticleSystem(particle, autoDestroy, delay);
        }

        public void CreateSkillBuildVfx(GameObject prefabBuild, GameObject prefab, Vector3 from, object target, Action<object> callbackComplete, bool isDirection = false)
        {
            if (target == null)
                return;

            GameObject particleSystem = Object.Instantiate(prefabBuild);
            particleSystem.transform.position = Utilites.CastVfxPosition(from + Vector3.forward);

            Vector3 castVfxPosition;
            switch (target)
            {
                case Player player:
                    castVfxPosition = player.AvatarObject.transform.position;
                    break;
                case BoardUnitView unit:
                    castVfxPosition = unit.Transform.position;
                    break;
                case BoardUnitModel unit:
                    castVfxPosition = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            Vector3 targetPosition = Utilites.CastVfxPosition(castVfxPosition);

            if (isDirection)
            {
                float angle = AngleBetweenVector3(particleSystem.transform.position, targetPosition);
                var main = particleSystem.GetComponent<ParticleSystem>().main;
                main.startRotationZ = angle * Mathf.Deg2Rad;
                ParticleSystem.MainModule subMain = new ParticleSystem.MainModule();
                foreach (var item in particleSystem.GetComponentsInChildren<ParticleSystem>())
                {
                    subMain = item.main;
                    subMain.startRotationZ = angle * Mathf.Deg2Rad;
                }
            }
            _particlesController.RegisterParticleSystem(particleSystem, true, 5f);
            InternalTools.DoActionDelayed(() =>
            {
                CreateSkillVfx(prefab, from, target, callbackComplete, isDirection);
            }, 3f);
        }

        public void CreateSkillVfx(GameObject prefab, Vector3 from, object target, Action<object> callbackComplete, bool isDirection = false)
        {
            if (target == null)
            {
                InternalTools.DoActionDelayed(() => callbackComplete(target), Time.deltaTime);
                return;
            }

            GameObject particleSystem = Object.Instantiate(prefab);
            particleSystem.transform.position = Utilites.CastVfxPosition(from + Vector3.forward);

            Vector3 castVfxPosition = Vector3.zero;
            switch (target)
            {
                case Player player:
                    castVfxPosition = player.AvatarObject.transform.position;
                    break;
                case BoardUnitView unit:
                    castVfxPosition = unit.Transform.position;
                    break;
                case BoardUnitModel unit:
                    castVfxPosition = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                    break;
                case HandBoardCard cardInHand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            Vector3 targetPosition = Utilites.CastVfxPosition(castVfxPosition);

            if (isDirection)
            {
                float angle = AngleBetweenVector3(particleSystem.transform.position, targetPosition);
                var main = particleSystem.GetComponent<ParticleSystem>().main;
                main.startRotationZ = angle * Mathf.Deg2Rad;
                ParticleSystem.MainModule subMain = new ParticleSystem.MainModule();
                foreach (var item in particleSystem.GetComponentsInChildren<ParticleSystem>())
                {
                    subMain = item.main;
                    subMain.startRotationZ = angle * Mathf.Deg2Rad;
                }
            }

            particleSystem.transform
                .DOMove(targetPosition, .5f).OnComplete(
                    () =>
                    {
                        callbackComplete(target);

                        if (particleSystem != null)
                        {
                            Object.Destroy(particleSystem);
                        }
                    });
        }

        public void SpawnGotDamageEffect(IView onObject, int count)
        {
            Transform target = null;

            switch (onObject)
            {
                case BoardUnitView unit:
                    target = unit.Transform;
                    break;
                case BoardUnitModel unit:
                    target = _battlegroundController.GetBoardUnitViewByModel(unit).Transform;
                    break;
                case Player _:
                    target = ((Player)onObject).AvatarObject.transform;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(onObject), onObject, null);
            }

            GameObject effect =
                Object.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Item_GotDamageEffect"));
            effect.transform.Find("Text_Info").GetComponent<TextMeshPro>().text = count.ToString();
            effect.transform.SetParent(target, false);
            effect.transform.localPosition = Vector3.zero;

            Object.Destroy(effect, 2.5f);
        }

        private void ChechTouchOnBattleground()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.tag != TagZoneForTouching)
                            return;
                    }
                }
                if (hits.Length > 0)
                {
                    CreateBattlegroundTouchEffect(mousePos);
                }
            }
        }

        private void CreateBattlegroundTouchEffect(Vector3 position)
        {
            GameObject effect = Object.Instantiate(_battlegroundTouchPrefab);
            effect.transform.position = Utilites.CastVfxPosition(position);
            _particlesController.RegisterParticleSystem(effect, true, 5f);
            _soundManager.PlaySound(Enumerators.SoundType.BATTLEGROUND_TOUCH_EFFECT, Constants.SfxSoundVolume);
        }

        public float AngleBetweenVector3(Vector3 from, Vector3 target)
        {
            Vector3 diference = target - from;
            float sign = (target.x < from.x) ? -1.0f : 1.0f;
            return Vector3.Angle(Vector3.forward, diference) * sign;
        }

        public void CreateDeathZombieAnimation(BoardUnitView unitView, Action endOfDestroyAnimationCallback, Action endOfAnimationCallback, Action completeCallback)
        {
            bool withEffect = true;

            if (unitView.Model.LastAttackingSetType == Enumerators.Faction.ITEM)
            {
                withEffect = false;
            }

            UnitDeathAnimation deathAnimation = new UnitDeathAnimation(unitView, withEffect);

            deathAnimation.DestroyUnitTriggered += (x) =>
            {
                endOfDestroyAnimationCallback?.Invoke();

                if (deathAnimation.SelfObject)
                {
                    unitView.ForceSetGameObject(deathAnimation.SelfObject);
                }
            };
            deathAnimation.AnimationEnded += (x) =>
            {
                _unitDeathAnimations.Remove(deathAnimation);
                endOfAnimationCallback?.Invoke();

                _gameplayManager.GetController<BoardController>().UpdateWholeBoard(() =>
                {
                    completeCallback?.Invoke();
                });   
            };

            _unitDeathAnimations.Add(deathAnimation);
        }
    }

    public class UnitDeathAnimation
    {
        public event Action<UnitDeathAnimation> AnimationEnded;
        public event Action<UnitDeathAnimation> DestroyUnitTriggered;

        private ILoadObjectsManager _loadObjectsManager;
        private ISoundManager _soundManager;
        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private BoardController _boardController;

        private AnimationEventTriggering AnimationEventTriggeringHandler;
        private Animator EffectAnimator;
        private ParticleSystem ParticleSystem;

        private float _initialAnimationSpeed;
        private float _deathSoundDuration;
        private bool _isDeathSoundEnded;
        private bool _readyForContinueDeathAnimation;
        private bool _withEffect;
        private float _defaultDeathAnimationLength = 0.7f;

        private int _effectSoundIdentificator;

        public BoardUnitView BoardUnitView;

        public GameObject SelfObject;

        public UnitDeathAnimation(BoardUnitView unitView, bool withEffect)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _boardController = _gameplayManager.GetController<BoardController>();

            BoardUnitView = unitView;

            _withEffect = withEffect;

            if (_withEffect)
            {
                SelfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UniqueArrivalAnimations/ZB_ANM_" +
                                                InternalTools.FormatStringToPascaleCase(BoardUnitView.Model.LastAttackingSetType.ToString()) +
                                                "DeathAnimation"));

                SelfObject.transform.position = BoardUnitView.Transform.position;

                AnimationEventTriggeringHandler = SelfObject.GetComponent<AnimationEventTriggering>();
                EffectAnimator = SelfObject.GetComponent<Animator>();

                ParticleSystem = SelfObject.transform.Find("VFX_All").GetComponent<ParticleSystem>();

                AnimationEventTriggeringHandler.AnimationEventTriggered = AnimationEventReceived;

                _initialAnimationSpeed = EffectAnimator.speed;
            }
            else
            {
                BoardUnitView.Transform.DOShakePosition(_defaultDeathAnimationLength, 0.25f, 10, 90, false, false);

                InternalTools.DoActionDelayed(DefaultDeathAnimationEnded, _defaultDeathAnimationLength);
            }

            PlayEffectSound();
            PlayDeathSound();
        }

        public void Update()
        {
            if (BoardUnitView != null && !BoardUnitView.WasDestroyed && SelfObject != null)
            {
                if (_withEffect)
                {
                    SelfObject.transform.position = BoardUnitView.Transform.position;
                }
            }
        }

        public void Dispose()
        {
            if (SelfObject != null)
            {
                Object.Destroy(SelfObject);
            }
        }

        private void PlayDeathSound()
        {
            string cardDeathSoundName = BoardUnitView.Model.Card.LibraryCard.Name.ToLowerInvariant() + "_" + Constants.CardSoundDeath;

            if (!BoardUnitView.Model.OwnerPlayer.Equals(_gameplayManager.CurrentTurnPlayer))
            {
                _deathSoundDuration = _soundManager.GetSoundLength(Enumerators.SoundType.CARDS, cardDeathSoundName);

                _soundManager.PlaySound(Enumerators.SoundType.CARDS, cardDeathSoundName,
                    Constants.ZombieDeathVoDelayBeforeFadeout, Constants.ZombiesSoundVolume,
                    Enumerators.CardSoundType.DEATH);

                InternalTools.DoActionDelayed(EndOfDeathSoundEvent, _deathSoundDuration);
            }
            else
            {
                EndOfDeathSoundEvent();
            }
        }

        private void PlayEffectSound()
        {
            _effectSoundIdentificator = _soundManager.PlaySound(Enumerators.SoundType.ZOMBIE_DEATH_ANIMATIONS,
                "ZB_AUD_" +
                InternalTools.FormatStringToPascaleCase(BoardUnitView.Model.LastAttackingSetType.ToString()) +
                "ZombieDeath_F1_EXP",
                Constants.ZombiesSoundVolume, isLoop: false);
        }

        public void EndDeathAnimation()
        {
            if (_withEffect)
            {
                ParticleSystem.Play(true);
                EffectAnimator.speed = _initialAnimationSpeed;
                ChangeSoundState(false);
            }
            else
            {
                AnimationEventReceived("End");
            }

            DestroyUnitTriggered?.Invoke(this);
        }

        private void ChangeSoundState(bool pause)
        {
            _soundManager.SetSoundPaused(_effectSoundIdentificator, pause);
        }

        private void AnimationEventReceived(string method)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            switch (method)
            {
                case "Pause":
                    ParticleSystem.Pause(true);
                    ChangeSoundState(true);
                    EffectAnimator.speed = 0;

                    if (_isDeathSoundEnded)
                    {
                        EndDeathAnimation();
                    }
                    else
                    {
                        _readyForContinueDeathAnimation = true;
                    }
                    break;
                case "End":
                    if (_withEffect)
                    {
                        ParticleSystem.Stop();
                        EffectAnimator.StopPlayback();
                    }
                    AnimationEnded?.Invoke(this);
                    Dispose();
                    break;
            }
        }

        private void EndOfDeathSoundEvent()
        {
            if (_readyForContinueDeathAnimation)
            {
                EndDeathAnimation();
            }

            _isDeathSoundEnded = true;
        }

        private void DefaultDeathAnimationEnded()
        {
            if (_isDeathSoundEnded)
            {
                EndDeathAnimation();
            }
            else
            {
                _readyForContinueDeathAnimation = true;
            }
        }
    }
}

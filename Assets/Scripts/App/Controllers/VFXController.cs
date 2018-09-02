using System;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class VfxController : IController
    {
        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private ParticlesController _particlesController;

        public Sprite FeralFrame { get; set; }

        public Sprite HeavyFrame { get; set; }

        public void Init()
        {
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _particlesController = _gameplayManager.GetController<ParticlesController>();

            FeralFrame = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UnitFrames/feral");
            HeavyFrame = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UnitFrames/heavy");
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void PlayAttackVfx(Enumerators.CardType type, Vector3 target, int damage)
        {
            GameObject effect;
            GameObject vfxPrefab;
            target = Utilites.CastVfxPosition(target);
            Vector3 offset = Vector3.forward * 1;

            switch (type)
            {
                case Enumerators.CardType.FERAL: {
                    vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FeralAttackVFX");
                    effect = Object.Instantiate(vfxPrefab);
                    effect.transform.position = target - offset;
                    _soundManager.PlaySound(Enumerators.SoundType.FERAL_ATTACK, Constants.CreatureAttackSoundVolume, false, false, true);

                    _particlesController.RegisterParticleSystem(effect, true, 5f);

                    if (damage > 3 && damage < 7)
                    {
                        _timerManager.AddTimer(
                            a =>
                            {
                                effect = Object.Instantiate(vfxPrefab);
                                effect.transform.position = target - offset;
                                effect.transform.localScale = new Vector3(-1, 1, 1);
                                _particlesController.RegisterParticleSystem(effect, true, 5f);
                            },
                            null,
                            0.5f,
                            false);
                    }

                    if (damage > 6)
                    {
                        _timerManager.AddTimer(
                            a =>
                            {
                                effect = Object.Instantiate(vfxPrefab);
                                effect.transform.position = target - Vector3.right - offset;
                                effect.transform.eulerAngles = Vector3.forward * 90;

                                _particlesController.RegisterParticleSystem(effect, true, 5f);
                            },
                            null,
                            1.0f,
                            false);
                    }

                    // GameClient.Get<ITimerManager>().AddTimer((a) =>
                    // {
                    // _soundManager.PlaySound(Enumerators.SoundType.FERAL_ATTACK, Constants.CREATURE_ATTACK_SOUND_VOLUME, false, false, true);
                    // }, null, 0.75f, false);
                    break;
                }
                case Enumerators.CardType.HEAVY: {
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

                    /* GameClient.Get<ITimerManager>().AddTimer((a) =>
                                     {
                                     }, null, 0.75f, false);*/
                    break;
                }
                default: {
                    vfxPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WalkerAttackVFX");
                    effect = Object.Instantiate(vfxPrefab);
                    effect.transform.position = target - offset;

                    _particlesController.RegisterParticleSystem(effect, true, 5f);

                    if (damage > 4)
                    {
                        _timerManager.AddTimer(
                            a =>
                            {
                                effect = Object.Instantiate(vfxPrefab);
                                effect.transform.position = target - offset;

                                effect.transform.localScale = new Vector3(-1, 1, 1);
                                _particlesController.RegisterParticleSystem(effect, true, 5f);
                            },
                            null,
                            0.5f,
                            false);

                        // GameClient.Get<ITimerManager>().AddTimer((a) =>
                        // {
                        _soundManager.PlaySound(Enumerators.SoundType.WALKER_ATTACK_2, Constants.CreatureAttackSoundVolume, false, false, true);

                        // }, null, 0.75f, false);
                    }
                    else
                    {
                        // GameClient.Get<ITimerManager>().AddTimer((a) =>
                        // {
                        _soundManager.PlaySound(Enumerators.SoundType.WALKER_ATTACK_1, Constants.CreatureAttackSoundVolume, false, false, true);

                        // }, null, 0.75f, false);
                    }

                    break;
                }
            }
        }

        public void CreateVfx(Enumerators.SetType setType, Vector3 position, bool autoDestroy = true, float delay = 3f)
        {
            GameObject prefab = null;

            switch (setType)
            {
                case Enumerators.SetType.WATER:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBolt_ImpactVFX");
                    break;
                case Enumerators.SetType.TOXIC:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ToxicAttackVFX");
                    break;
                case Enumerators.SetType.FIRE:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBolt_ImpactVFX");
                    break;
                case Enumerators.SetType.LIFE:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX");
                    break;
                case Enumerators.SetType.EARTH:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"); // todo improve particle
                    break;
                case Enumerators.SetType.AIR:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
                    break;
            }

            GameObject particle = Object.Instantiate(prefab);
            particle.transform.position = Utilites.CastVfxPosition(position + Vector3.forward);
            _particlesController.RegisterParticleSystem(particle, autoDestroy, delay);
        }

        public void CreateVfx(GameObject prefab, object target, bool autoDestroy = true, float delay = 3f)
        {
            if (prefab == null)
                return;

            Vector3 position = Vector3.zero;

            switch (target)
            {
                case BoardUnit unit:
                    position = unit.Transform.position;
                    break;
                case Player player:
                    position = player.AvatarObject.transform.position;
                    break;
                case Transform transform:
                    position = transform.transform.position;
                    break;
            }

            GameObject particle = Object.Instantiate(prefab);
            particle.transform.position = Utilites.CastVfxPosition(position + Vector3.forward);
            _particlesController.RegisterParticleSystem(particle, autoDestroy, delay);
        }

        public void CreateSkillVfx(GameObject prefab, Vector3 from, object target, Action<object> callbackComplete)
        {
            if (target == null)
                return;

            GameObject particleSystem = Object.Instantiate(prefab);
            particleSystem.transform.position = Utilites.CastVfxPosition(from + Vector3.forward);

            switch (target)
            {
                case Player player:
                    particleSystem.transform.DOMove(Utilites.CastVfxPosition(player.AvatarObject.transform.position), .5f).OnComplete(
                        () =>
                        {
                            callbackComplete(target);

                            if (particleSystem != null)
                            {
                                Object.Destroy(particleSystem);
                            }
                        });
                    break;
                case BoardUnit unit:
                    particleSystem.transform.DOMove(Utilites.CastVfxPosition(unit.Transform.position), .5f).OnComplete(
                        () =>
                        {
                            callbackComplete(target);

                            if (particleSystem != null)
                            {
                                Object.Destroy(particleSystem);
                            }
                        });
                    break;
            }
        }

        public void SpawnGotDamageEffect(object onObject, int count)
        {
            Transform target = null;

            switch (onObject)
            {
                case BoardUnit unit:
                    target = unit.Transform;
                    break;
                case Player _:
                    target = ((Player) onObject).AvatarObject.transform;
                    break;
            }

            GameObject effect = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Item_GotDamageEffect"));
            effect.transform.Find("Text_Info").GetComponent<TextMeshPro>().text = count.ToString();
            effect.transform.SetParent(target, false);
            effect.transform.localPosition = Vector3.zero;

            Object.Destroy(effect, 2.5f);
        }
    }
}

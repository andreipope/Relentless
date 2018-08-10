// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SkillsController : IController
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;
        private IUIManager _uiManager;
        private ITimerManager _timerManager;
        private ISoundManager _soundManager;

        private VFXController _vfxController;
        private BattleController _battleController;
        private ActionsQueueController _actionsQueueController;
        private CardsController _cardsController;

        private BoardSkill _playerPrimarySkill,
                           _playerSecondarySkill;

        public BoardSkill opponentPrimarySkill,
                          opponentSecondarySkill;

        private bool _skillsInitialized = false;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _vfxController = _gameplayManager.GetController<VFXController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _cardsController = _gameplayManager.GetController<CardsController>();

            _gameplayManager.OnGameEndedEvent += _gameplayManager_OnGameEndedEvent;
        }

        public void Update()
        {
            if(_skillsInitialized)
            {
                _playerPrimarySkill.Update();
                _playerSecondarySkill.Update();
                opponentPrimarySkill.Update();
                opponentSecondarySkill.Update();
            }
        }

        public void ResetAll()
        {
        }

        public void InitializeSkills()
        {
            var rootPage = _uiManager.GetPage<GameplayPage>();


            rootPage.playerPrimarySkillHandler.OnMouseDownEvent += PrimarySkillHandlerOnMouseDownEventHandler;
            rootPage.playerPrimarySkillHandler.OnMouseUpEvent += PrimarySkillHandlerOnMouseUpEventHandler;

            rootPage.playerSecondarySkillHandler.OnMouseDownEvent += SecondarySkillHandlerOnMouseDownEventHandler;
            rootPage.playerSecondarySkillHandler.OnMouseUpEvent += SecondarySkillHandlerOnMouseUpEventHandler;


            int primary = _gameplayManager.CurrentPlayer.SelfHero.primarySkill;
            int secondary = _gameplayManager.CurrentPlayer.SelfHero.secondarySkill;

            if (primary < _gameplayManager.CurrentPlayer.SelfHero.skills.Count && secondary < _gameplayManager.CurrentPlayer.SelfHero.skills.Count)
                SetPlayerSkills(rootPage, _gameplayManager.CurrentPlayer.SelfHero.skills[primary], _gameplayManager.CurrentPlayer.SelfHero.skills[secondary]);

            primary = _gameplayManager.OpponentPlayer.SelfHero.primarySkill;
            secondary = _gameplayManager.OpponentPlayer.SelfHero.secondarySkill;

            if (primary < _gameplayManager.OpponentPlayer.SelfHero.skills.Count && secondary < _gameplayManager.OpponentPlayer.SelfHero.skills.Count)
                SetOpponentSkills(rootPage, _gameplayManager.OpponentPlayer.SelfHero.skills[primary], _gameplayManager.OpponentPlayer.SelfHero.skills[secondary]);

            _skillsInitialized = true;
        }

        public void DisableSkillsContent(Player player)
        {
            if(player.IsLocalPlayer)
            {
                _playerPrimarySkill.Hide();
                _playerSecondarySkill.Hide();
            }
            else
            {
                opponentPrimarySkill.Hide();
                opponentSecondarySkill.Hide();
            }
        }

        public void BlockSkill(Player player, Enumerators.SkillType type)
        {
            if (player.IsLocalPlayer)
            {
                _playerPrimarySkill.BlockSkill();
                _playerSecondarySkill.BlockSkill();
            }
            else
            {
                opponentPrimarySkill.BlockSkill();
                opponentSecondarySkill.BlockSkill();
            }
        }


        private void _gameplayManager_OnGameEndedEvent(Enumerators.EndGameType obj)
        {
            _skillsInitialized = false;
        }

        private void PrimarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if(_playerPrimarySkill != null)
            _playerPrimarySkill.OnMouseDownEventHandler();
        }

        private void PrimarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerPrimarySkill != null)
                _playerPrimarySkill.OnMouseUpEventHandler();
        }

        private void SecondarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
                _playerSecondarySkill.OnMouseDownEventHandler();
        }

        private void SecondarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
                _playerSecondarySkill.OnMouseUpEventHandler();
        }

        public void SetPlayerSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            _playerPrimarySkill = new BoardSkill(rootPage.playerPrimarySkillHandler.gameObject, _gameplayManager.CurrentPlayer, primary, 3, true);
            _playerSecondarySkill = new BoardSkill(rootPage.playerSecondarySkillHandler.gameObject, _gameplayManager.CurrentPlayer, secondary, 4, false);
        }

        public void SetOpponentSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            opponentPrimarySkill = new BoardSkill(rootPage.opponentPrimarySkillHandler, _gameplayManager.OpponentPlayer, primary, 3, true);
            opponentSecondarySkill = new BoardSkill(rootPage.opponentSecondarySkillHandler, _gameplayManager.OpponentPlayer, secondary, 4, false);
        }

        private void SkillParticleActionCompleted(object target)
        {
            //switch (skillType)
            //{
            //    case Enumerators.SetType.WATER:
            //        FreezeAction(target);
            //        break;
            //    case Enumerators.SetType.TOXIC:
            //        ToxicDamageAction(target);
            //        break;
            //    case Enumerators.SetType.FIRE:
            //        FireDamageAction(target);
            //        break;
            //    case Enumerators.SetType.LIFE:
            //        HealAnyAction(target);
            //        break;
            //    case Enumerators.SetType.AIR:
            //        //   CardReturnAction(target);
            //        break;
            //    default:
            //        break;
            //}
        }

        public void DoSkillAction(BoardSkill skill, object target = null)
        {
            if (skill == null)
                return;

            if (skill.IsUsing)
            {
                if (skill.fightTargetingArrow != null)
                {
                    if (skill.fightTargetingArrow.selectedPlayer != null)
                    {
                        var targetPlayer = skill.fightTargetingArrow.selectedPlayer;

                        _vfxController.CreateSkillVFX(GetVFXPrefabBySkill(skill), skill.selfObject.transform.position, targetPlayer, (x) =>
                        {
                            skill.UseSkill(targetPlayer);
                            DoActionByType(skill, targetPlayer);
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                        });
                    }
                    else if (skill.fightTargetingArrow.selectedCard != null)
                    {
                        var targetUnit = skill.fightTargetingArrow.selectedCard;

                        _vfxController.CreateSkillVFX(GetVFXPrefabBySkill(skill), skill.selfObject.transform.position, targetUnit, (x) =>
                        {
                            DoActionByType(skill, targetUnit);
                            skill.UseSkill(targetUnit);
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                        });  
                    }

                    skill.CancelTargetingArrows();
                    skill.fightTargetingArrow = null;
                }
                else if(target != null)
                {
                    _vfxController.CreateSkillVFX(GetVFXPrefabBySkill(skill), skill.selfObject.transform.position, target, (x) =>
                    {
                        DoActionByType(skill, target);
                        skill.UseSkill(target);
                        _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                    });   
                }
            }
        }

        private GameObject GetVFXPrefabBySkill(BoardSkill skill)
        {
            GameObject prefab = null;

            switch (skill.skill.overlordSkill)
            {
                case Enumerators.OverlordSkill.ICE_BOLT:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");
                    break;
                case Enumerators.OverlordSkill.FREEZE:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX");
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
                    break;
                case Enumerators.OverlordSkill.FIREBALL:
                case Enumerators.OverlordSkill.FIRE_BOLT:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX");
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX");
                    break;
                case Enumerators.OverlordSkill.TOXIC_POWER:
                case Enumerators.OverlordSkill.MEND:
                case Enumerators.OverlordSkill.HARDEN:
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.PUSH:
                case Enumerators.OverlordSkill.DRAW:
                default:
                    prefab = new GameObject();
                    break;
            }

            return prefab;
        }

        private void DoActionByType(BoardSkill skill, object target)
        {
            switch(skill.skill.overlordSkill)
            {
                case Enumerators.OverlordSkill.FREEZE:
                    FreezeAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.ICE_BOLT:
                    IceBoltAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                    PoisonDartAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.TOXIC_POWER:
                    ToxicPowerAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                    HealingTouchAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.MEND:
                    MendAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.FIRE_BOLT:
                     FireBoltAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.RABIES:
                    RabiesAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.HARDEN:
                    HardenAction(skill.owner, skill, skill.skill);
                    break;
                case Enumerators.OverlordSkill.STONE_SKIN:
                    StoneskinAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.PUSH:
                    PushAction(skill.owner, skill, skill.skill, target);
                    break;
                case Enumerators.OverlordSkill.DRAW:
                    DrawAction(skill.owner, skill, skill.skill, target);
                    break;
                default: break;
            }
        }

        #region actions

        private void FreezeAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is BoardUnit)
            {
                var unit = target as BoardUnit;
                unit.Stun(Enumerators.StunType.FREEZE, skill.value);

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX"), unit);

                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.STUN_UNIT_BY_SKILL, new object[]
                {
                    owner,
                    unit
                }));
            }
            else if (target is Player)
            {
                var player = target as Player;

                player.Stun(Enumerators.StunType.FREEZE, skill.value);

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"), player);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower() + "_Impact", Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);


                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.STUN_PLAYER_BY_SKILL, new object[]
                {
                    owner,
                    player
                }));
            }
        }

        private void PoisonDartAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"), target);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower() + "_Impact", Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
        }

        private void FireBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX"), target);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
        }

        private void HealingTouchAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is Player)
            {
                var player = target as Player;

                _battleController.HealPlayerBySkill(owner, skill, player);

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), player);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
            }
            else
            {
                var unit = target as BoardUnit;

                _battleController.HealUnitBySkill(owner, skill, unit);

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
            }
        }

        private void HardenAction(Player owner, BoardSkill boardSkill, HeroSkill skill)
        {
            _battleController.HealPlayerBySkill(owner, skill, owner);

            //TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position -= Vector3.up * 3.3f;

            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform);
        }

        private void AttackWithModifiers(Player owner, BoardSkill boardSkill, HeroSkill skill, object target, Enumerators.SetType attackType, Enumerators.SetType setType)
        {
            if (target is Player)
            {
                var player = target as Player;
                //TODO additional damage to heros

                _battleController.AttackPlayerBySkill(owner, skill, player);
            }
            else
            {
                var creature = target as BoardUnit;
                var attackModifier = 0;

              //  if (creature.Card.libraryCard.cardSetType == setType)
               //     attackModifier = 1;

                _battleController.AttackUnitBySkill(owner, skill, creature, attackModifier);
            }
        }
        
        private void PushAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            BoardUnit targetUnit = (target as BoardUnit);
            Player unitOwner = targetUnit.ownerPlayer;
            WorkingCard returningCard = targetUnit.Card;
            Vector3 unitPosition = targetUnit.transform.position;

            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX"), targetUnit);

            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);

            _timerManager.AddTimer((x) =>
            {

                // STEP 1 - REMOVE UNIT FROM BOARD
                unitOwner.BoardCards.Remove(targetUnit);

                // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE;
                targetUnit.Die(true);
                MonoBehaviour.Destroy(targetUnit.gameObject);

                // STEP 3 - REMOVE WORKING CARD FROM BOARD
                unitOwner.RemoveCardFromBoard(returningCard);

                // STEP 4 - RETURN CARD TO HAND
                _cardsController.ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                // STEP 4 - REARRANGE HANDS
                _gameplayManager.RearrangeHands();

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.RETURN_TO_HAND_CARD_SKILL, new object[]
                {
                owner,
                skill,
                targetUnit
                }));

                //_gameplayManager.GetController<RanksController>().UpdateRanksBuffs(unitOwner);
            }, null, 2f);
        }


        private void DrawAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _cardsController.AddCardToHand(owner);

            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/DrawCardVFX"), owner);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.DRAW_CARD_SKILL, new object[]
            {
                owner,
                skill
            }));
        }

        private void StoneskinAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {

                BoardUnit unit = target as BoardUnit;

                if (_cardsController.GetSetOfCard(unit.Card.libraryCard).Equals(owner.SelfHero.element))
                {
                    unit.BuffedHP += skill.value;
                    unit.CurrentHP += skill.value;
                }

                //TODO: remove this empty gameobject logic
                Transform transform = new GameObject().transform;
                transform.position = unit.transform.position;
                transform.position -= Vector3.up * 3.3f;

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
            }
        }

        private void RabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.SetAsFeralUnit();

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
            }
        }

        private void ToxicPowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {

                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, skill, unit, 0);

                unit.BuffedDamage += skill.attack;
                unit.CurrentDamage += skill.attack;

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ToxicAttackVFX"), unit);
            }
        }

        private void IceBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {

                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, skill, unit, 0);

                if (unit.CurrentHP > 0)
                {
                    unit.Stun(Enumerators.StunType.FREEZE, 1);
                }

                _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBolt_Impact"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
            }
        }

        private void MendAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            owner.HP = Mathf.Clamp(owner.HP + skill.value, 0, owner.MaxCurrentHP);
            //TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position += Vector3.up * 2;
            _vfxController.CreateVFX(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MendVFX"), transform);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.title.Trim().ToLower(), Constants.OVERLORD_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
        }

        #endregion
    }
}
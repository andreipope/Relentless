using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SkillsController : IController
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private ITimerManager _timerManager;

        private ISoundManager _soundManager;

        private VfxController _vfxController;

        private BattleController _battleController;

        private ActionsQueueController _actionsQueueController;

        private CardsController _cardsController;

        private BattlegroundController _battlegroundController;

        private bool _skillsInitialized;

        public BoardSkill OpponentPrimarySkill { get; private set; }
        public BoardSkill OpponentSecondarySkill { get; private set; }
        public BoardSkill PlayerPrimarySkill { get; private set; }
        public BoardSkill PlayerSecondarySkill { get; private set; }

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

            _vfxController = _gameplayManager.GetController<VfxController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _gameplayManager.GameEnded += GameplayManagerGameEnded;
        }

        public void Update()
        {
            if (_skillsInitialized)
            {
                PlayerPrimarySkill.Update();
                PlayerSecondarySkill.Update();
                OpponentPrimarySkill.Update();
                OpponentSecondarySkill.Update();
            }
        }

        public void ResetAll()
        {
        }

        public void InitializeSkills()
        {
            GameplayPage rootPage = _uiManager.GetPage<GameplayPage>();

            rootPage.PlayerPrimarySkillHandler.MouseDownTriggered += PrimarySkillHandlerMouseDownTriggeredHandler;
            rootPage.PlayerPrimarySkillHandler.MouseUpTriggered += PrimarySkillHandlerMouseUpTriggeredHandler;

            rootPage.PlayerSecondarySkillHandler.MouseDownTriggered += SecondarySkillHandlerMouseDownTriggeredHandler;
            rootPage.PlayerSecondarySkillHandler.MouseUpTriggered += SecondarySkillHandlerMouseUpTriggeredHandler;
            rootPage.OpponentPrimarySkillHandler.MouseDownTriggered +=
                OpponentPrimarySkillHandlerMouseDownTriggeredHandler;
            rootPage.OpponentPrimarySkillHandler.MouseUpTriggered += OpponentPrimarySkillHandlerMouseUpTriggeredHandler;

            rootPage.OpponentSecondarySkillHandler.MouseDownTriggered +=
                OpponentSecondarySkillHandlerMouseDownTriggeredHandler;
            rootPage.OpponentSecondarySkillHandler.MouseUpTriggered +=
                OpponentSecondarySkillHandlerMouseUpTriggeredHandler;

            int primary = _gameplayManager.CurrentPlayer.SelfHero.PrimarySkill;
            int secondary = _gameplayManager.CurrentPlayer.SelfHero.SecondarySkill;

            if (primary < _gameplayManager.CurrentPlayer.SelfHero.Skills.Count &&
                secondary < _gameplayManager.CurrentPlayer.SelfHero.Skills.Count)
            {
                SetPlayerSkills(rootPage, _gameplayManager.CurrentPlayer.SelfHero.Skills[primary],
                    _gameplayManager.CurrentPlayer.SelfHero.Skills[secondary]);
            }

            primary = _gameplayManager.OpponentPlayer.SelfHero.PrimarySkill;
            secondary = _gameplayManager.OpponentPlayer.SelfHero.SecondarySkill;

            if (primary < _gameplayManager.OpponentPlayer.SelfHero.Skills.Count &&
                secondary < _gameplayManager.OpponentPlayer.SelfHero.Skills.Count)
            {
                SetOpponentSkills(rootPage, _gameplayManager.OpponentPlayer.SelfHero.Skills[primary],
                    _gameplayManager.OpponentPlayer.SelfHero.Skills[secondary]);
            }

            _skillsInitialized = true;
        }

        public void DisableSkillsContent(Player player)
        {
            if (player.IsLocalPlayer)
            {
                PlayerPrimarySkill.Hide();
                PlayerSecondarySkill.Hide();
            }
            else
            {
                OpponentPrimarySkill.Hide();
                OpponentSecondarySkill.Hide();
            }
        }

        public void BlockSkill(Player player, Enumerators.SkillType type)
        {
            if (player.IsLocalPlayer)
            {
                PlayerPrimarySkill.BlockSkill();
                PlayerSecondarySkill.BlockSkill();
            }
            else
            {
                OpponentPrimarySkill.BlockSkill();
                OpponentSecondarySkill.BlockSkill();
            }
        }

        public void SetPlayerSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            PlayerPrimarySkill = new BoardSkill(rootPage.PlayerPrimarySkillHandler.gameObject,
                _gameplayManager.CurrentPlayer, primary, true);
            PlayerSecondarySkill = new BoardSkill(rootPage.PlayerSecondarySkillHandler.gameObject,
                _gameplayManager.CurrentPlayer, secondary, false);
        }

        public void SetOpponentSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            OpponentPrimarySkill = new BoardSkill(rootPage.OpponentPrimarySkillHandler.gameObject,
                _gameplayManager.OpponentPlayer, primary, true);
            OpponentSecondarySkill = new BoardSkill(rootPage.OpponentSecondarySkillHandler.gameObject,
                _gameplayManager.OpponentPlayer, secondary, false);
        }

        public void DoSkillAction(BoardSkill skill, object target = null)
        {
            if (skill == null)
                return;

            if (!skill.IsUsing)
                return;

            if (skill.FightTargetingArrow != null)
            {
                if (skill.FightTargetingArrow.SelectedPlayer != null)
                {
                    Player targetPlayer = skill.FightTargetingArrow.SelectedPlayer;

                    string soundFile = GetSoundBySkills(skill);
                    if (!string.IsNullOrEmpty(soundFile))
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, soundFile, Constants.OverlordAbilitySoundVolume, false);
                    }

                    skill.UseSkill();
                    _vfxController.CreateSkillVfx(
                        GetVfxPrefabBySkill(skill),
                        skill.SelfObject.transform.position,
                        targetPlayer,
                        x =>
                        {
                            DoActionByType(skill, targetPlayer);
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                        });


                }
                else if (skill.FightTargetingArrow.SelectedCard != null)
                {
                    BoardUnit targetUnit = skill.FightTargetingArrow.SelectedCard;

                    string soundFile = GetSoundBySkills(skill);
                    if (!string.IsNullOrEmpty(soundFile))
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, soundFile, Constants.OverlordAbilitySoundVolume, false);
                    }

                    skill.UseSkill();
                    _vfxController.CreateSkillVfx(
                        GetVfxPrefabBySkill(skill),
                        skill.SelfObject.transform.position,
                        targetUnit,
                        async (x) =>
                        {
                            DoActionByType(skill, targetUnit);
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                        });
                }

                skill.CancelTargetingArrows();
                skill.FightTargetingArrow = null;
            }
            else if (target != null)
            {
                string soundFile = GetSoundBySkills(skill);
                if (!string.IsNullOrEmpty(soundFile))
                {
                    _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, soundFile, Constants.OverlordAbilitySoundVolume, false);
                }

                skill.UseSkill();
                _vfxController.CreateSkillVfx(
                    GetVfxPrefabBySkill(skill),
                    skill.SelfObject.transform.position,
                    target,
                    async (x) =>
                    {
                        DoActionByType(skill, target);
                        _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);

                        if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
                        {
                            await _gameplayManager.GetController<OpponentController>().ActionUseOverlordSkill(skill.OwnerPlayer, skill, target);
                        }
                    });
            }
        }

        private void GameplayManagerGameEnded(Enumerators.EndGameType obj)
        {
            _skillsInitialized = false;
        }

        private void PrimarySkillHandlerMouseDownTriggeredHandler(GameObject obj)
        {
            PlayerPrimarySkill?.OnMouseDownEventHandler();
        }

        private void PrimarySkillHandlerMouseUpTriggeredHandler(GameObject obj)
        {
            PlayerPrimarySkill?.OnMouseUpEventHandler();
        }

        private void SecondarySkillHandlerMouseDownTriggeredHandler(GameObject obj)
        {
            PlayerSecondarySkill?.OnMouseDownEventHandler();
        }

        private void SecondarySkillHandlerMouseUpTriggeredHandler(GameObject obj)
        {
            PlayerSecondarySkill?.OnMouseUpEventHandler();
        }

        private void OpponentPrimarySkillHandlerMouseDownTriggeredHandler(GameObject obj)
        {
            OpponentPrimarySkill?.OnMouseDownEventHandler();
        }

        private void OpponentPrimarySkillHandlerMouseUpTriggeredHandler(GameObject obj)
        {
            OpponentPrimarySkill?.OnMouseUpEventHandler();
        }

        private void OpponentSecondarySkillHandlerMouseDownTriggeredHandler(GameObject obj)
        {
            OpponentSecondarySkill?.OnMouseDownEventHandler();
        }

        private void OpponentSecondarySkillHandlerMouseUpTriggeredHandler(GameObject obj)
        {
            OpponentSecondarySkill?.OnMouseUpEventHandler();
        }

        private GameObject GetVfxPrefabBySkill(BoardSkill skill)
        {
            GameObject prefab;

            switch (skill.Skill.OverlordSkill)
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
                case Enumerators.OverlordSkill.BLIZZARD:
                case Enumerators.OverlordSkill.BREAKOUT:
                case Enumerators.OverlordSkill.ENHANCE:
                case Enumerators.OverlordSkill.EPIDEMIC:
                case Enumerators.OverlordSkill.FORTIFY:
                case Enumerators.OverlordSkill.FORTRESS:
                case Enumerators.OverlordSkill.ICE_WALL:
                case Enumerators.OverlordSkill.INFECT:
                case Enumerators.OverlordSkill.LEVITATE:
                case Enumerators.OverlordSkill.MASS_RABIES:
                case Enumerators.OverlordSkill.METEOR_SHOWER:
                case Enumerators.OverlordSkill.PHALANX:
                case Enumerators.OverlordSkill.REANIMATE:
                case Enumerators.OverlordSkill.RESSURECT:
                case Enumerators.OverlordSkill.RETREAT:
                case Enumerators.OverlordSkill.SHATTER:
                case Enumerators.OverlordSkill.WIND_SHIELD:
                default:
                    prefab = new GameObject();
                    break;
            }

            return prefab;
        }

        private string GetSoundBySkills(BoardSkill skill)
        {
            string soundFileName = string.Empty;
            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.ICE_BOLT:
                case Enumerators.OverlordSkill.FREEZE:
                case Enumerators.OverlordSkill.POISON_DART:
                case Enumerators.OverlordSkill.FIRE_BOLT:
                    soundFileName = skill.Skill.OverlordSkill.ToString().ToLower();
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                case Enumerators.OverlordSkill.FIREBALL:
                case Enumerators.OverlordSkill.TOXIC_POWER:
                case Enumerators.OverlordSkill.MEND:
                case Enumerators.OverlordSkill.HARDEN:
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.PUSH:
                case Enumerators.OverlordSkill.DRAW:
                default:
                    break;
            }

            return soundFileName;
        }

        private void DoActionByType(BoardSkill skill, object target)
        {
            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.FREEZE:
                    FreezeAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.ICE_BOLT:
                    IceBoltAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                    PoisonDartAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.TOXIC_POWER:
                    ToxicPowerAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                    HealingTouchAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.MEND:
                    MendAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.FIRE_BOLT:
                    FireBoltAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.RABIES:
                    RabiesAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.HARDEN:
                    HardenAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.STONE_SKIN:
                    StoneskinAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.PUSH:
                    PushAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.DRAW:
                    DrawAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;          
                case Enumerators.OverlordSkill.WIND_SHIELD:
                    WindShieldAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.LEVITATE:
                    Levitate(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.RETREAT:
                    RetreatAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.BREAKOUT:
                    BreakoutAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.INFECT:
                    InfectAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.EPIDEMIC:
                    EpidemicAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.RESSURECT:
                    RessurectAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.REANIMATE:
                    ReanimateAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.ENHANCE:
                    EnhanceAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.ICE_WALL:
                    IceWallAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.SHATTER:
                    ShatterAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.BLIZZARD:
                    BlizzardAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.MASS_RABIES:
                    MassRabiesAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.METEOR_SHOWER:
                    MeteorShowerAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.FIREBALL:
                    FireballAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.FORTIFY:
                    FortifyAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.FORTRESS:
                    FortressAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.PHALANX:
                    PhalanxAction(skill.OwnerPlayer, skill, skill.Skill, target);
                    break;
            }
        }

        #region actions

        // ADDITINAL

        private void AttackWithModifiers(
            Player owner,
            BoardSkill boardSkill,
            HeroSkill skill,
            object target,
            Enumerators.SetType attackType,
            Enumerators.SetType setType)
        {
            if (target is Player player)
            {
                // TODO additional damage to heros
                _battleController.AttackPlayerBySkill(owner, boardSkill, player);
            }
            else
            {
                BoardUnit creature = target as BoardUnit;
                int attackModifier = 0;
                _battleController.AttackUnitBySkill(owner, boardSkill, creature, attackModifier);
            }
        }


        // AIR

        private void PushAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            int goo = owner.Goo;

            owner.Goo -= goo;

            BoardUnit targetUnit = target as BoardUnit;
            Player unitOwner = targetUnit.OwnerPlayer;
            WorkingCard returningCard = targetUnit.Card;

            returningCard.InitialCost = returningCard.LibraryCard.Cost;
            returningCard.RealCost = returningCard.InitialCost;

            Vector3 unitPosition = targetUnit.Transform.position;

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX"),
                targetUnit);

            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Skill.Trim().ToLower(),
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            _timerManager.AddTimer(
                x =>
                {
                    // STEP 1 - REMOVE UNIT FROM BOARD
                    unitOwner.BoardCards.Remove(targetUnit);

                    // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE;
                    targetUnit.Die(true);
                    Object.Destroy(targetUnit.GameObject);

                    // STEP 3 - REMOVE WORKING CARD FROM BOARD
                    unitOwner.RemoveCardFromBoard(returningCard);

                    // STEP 4 - RETURN CARD TO HAND
                    _cardsController.ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                    // STEP 4 - REARRANGE HANDS
                    _gameplayManager.RearrangeHands();

                    // _gameplayManager.GetController<RanksController>().UpdateRanksBuffs(unitOwner);
                },
                null,
                2f);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Push,
                        Target = target
                    }
                }
            });
        }

        private void DrawAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _cardsController.AddCardToHand(owner);

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/DrawCardVFX"),
                owner);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });
        }

        private void WindShieldAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = InternalTools.GetRandomElementsFromList(owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.AIR), skill.Value);

            foreach (var unit in units)
            {
                unit.BuffShield();
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = target
                    }
                }
            });
        } 

        private void Levitate(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _cardsController.LowGooCostOfCardInHand(owner, null, skill.Value);
        }

        private void RetreatAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = new List<BoardUnit>();
            units.AddRange(_gameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(_gameplayManager.OpponentPlayer.BoardCards);

            foreach (BoardUnit unit in units)
            {
                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX"), unit); // retreat vfx

                _cardsController.ReturnCardToHand(unit);
            }
        }

        // TOXIC

        private void ToxicPowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0);

                unit.BuffedDamage += skill.Attack;
                unit.CurrentDamage += skill.Attack;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ToxicAttackVFX"), unit);
                //_soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Skill.Trim().ToLower(),
                //  Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
        }

        private void PoisonDartAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
            _vfxController.CreateVfx(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"), target);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower() + "_Impact",
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            Enumerators.ActionType actionType = Enumerators.ActionType.None;

            if(target is Player)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnOverlord;
            }
            else if(target is BoardUnit)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnCard;
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target
                    }
                }
            });
        }

        private void BreakoutAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<object> targets = new List<object>();

            var opponent = _gameplayManager.GetOpponentByPlayer(owner);

            targets.Add(opponent);
            targets.AddRange(opponent.BoardCards);

            targets = InternalTools.GetRandomElementsFromList(targets, skill.Count);

            foreach (object targetObject in targets)
            {
                AttackWithModifiers(owner, boardSkill, skill, targetObject, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"), target);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower() + "_Impact",
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = target
                    }
                }
            });
        }

        private void InfectAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.TOXIC);

            if (units.Count == 0)
                return;

            BoardUnit unit = units[UnityEngine.Random.Range(0, units.Count)];

            int unitAtk = unit.CurrentDamage;

            _battlegroundController.DestroyBoardUnit(unit);

            var opponentUnits = _gameplayManager.GetOpponentByPlayer(owner).BoardCards;

            if (opponentUnits.Count == 0)
                return;

            unit = opponentUnits[UnityEngine.Random.Range(0, opponentUnits.Count)];

            _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0);
        }

        private void EpidemicAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.TOXIC);

            if (units.Count == 0)
                return;

            units = InternalTools.GetRandomElementsFromList(units, skill.Count);
            List<BoardUnit> opponentUnits = InternalTools.GetRandomElementsFromList(_gameplayManager.GetOpponentByPlayer(owner).BoardCards, skill.Count);

            int unitAtk = 0;

            BoardUnit opponentUnit = null;
            for (int i = 0; i < units.Count; i++)
            {
                unitAtk = units[i].CurrentDamage;

                _battlegroundController.DestroyBoardUnit(units[i]);

                if (opponentUnits.Count > 0)
                {
                    opponentUnit = opponentUnits[UnityEngine.Random.Range(0, opponentUnits.Count)];

                    _battleController.AttackUnitBySkill(owner, boardSkill, opponentUnit, 0);

                    opponentUnits.Remove(opponentUnit);
                }
            }
        }

        // LIFE

        private void HealingTouchAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is Player)
            {
                Player player = target as Player;

                _battleController.HealPlayerBySkill(owner, boardSkill, player);

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), player);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
            else
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.HealUnitBySkill(owner, boardSkill, unit);

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Feral,
                        Target = target
                    }
                }
            });
        }

        private void MendAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            owner.Health = Mathf.Clamp(owner.Health + skill.Value, 0, owner.MaxCurrentHp);

            // TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position += Vector3.up * 2;
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MendVFX"),
                transform);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });
        }

        private void RessurectAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<WorkingCard> cards = owner.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == Enumerators.SetType.LIFE
                                                                      && x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE
                                                                      && x.RealCost == skill.Value);

            cards = InternalTools.GetRandomElementsFromList(cards, skill.Count);

            foreach (WorkingCard card in cards)
                _cardsController.SpawnUnitOnBoard(owner, card.LibraryCard.Name);
        }

        private void EnhanceAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<object> targets = new List<object>();

            targets.Add(owner);
            targets.AddRange(owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.LIFE));

            foreach (object targetObject in targets)
            {
                switch (targetObject)
                {
                    case BoardUnit unit:
                        _battleController.HealUnitBySkill(owner, boardSkill, unit);
                        break;
                    case Player player:
                        _battleController.HealPlayerBySkill(owner, boardSkill, player);
                        break;
                }
            }
        }

        private void ReanimateAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<WorkingCard> cards = owner.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == Enumerators.SetType.LIFE
                                                                       && x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);

            cards = InternalTools.GetRandomElementsFromList(cards, skill.Count);

            foreach (WorkingCard card in cards)
                _cardsController.SpawnUnitOnBoard(owner, card.LibraryCard.Name);
        }

        // WATER

        private void FreezeAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            switch (target)
            {
                case BoardUnit unit:
                    unit.Stun(Enumerators.StunType.FREEZE, skill.Value);

                    _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"), unit);

                    _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.OverlordSkill.ToString().ToLower() + "_Impact", Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);

                    break;
                case Player player:
                    player.Stun(Enumerators.StunType.FREEZE, skill.Value);

                    _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"), player);
                    _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.OverlordSkill.ToString().ToLower() + "_Impact", Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);

                    break;
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target
                    }
                }
            });
        }

        private void IceBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0);

                if (unit.CurrentHp > 0)
                {
                    unit.Stun(Enumerators.StunType.FREEZE, 1);
                }

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBolt_Impact"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower() + "_Impact",
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target
                    }
                }
            });
        }

        private void IceWallAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is BoardUnit unit)
            {
                unit.BuffedHp += skill.Value;
                unit.CurrentHp += skill.Value;
            }
            else if (target is Player player)
            {
                _battleController.HealPlayerBySkill(owner, boardSkill, player);
            }

            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
        }

        private void ShatterAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _battlegroundController.DestroyBoardUnit(target as BoardUnit);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                        Target = target
                    }
                }
            });
        }

        private void BlizzardAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = _gameplayManager.GetOpponentByPlayer(owner).BoardCards;
            units = InternalTools.GetRandomElementsFromList(units, skill.Count);

            foreach (var unit in units)
            {
                unit.Stun(Enumerators.StunType.FREEZE, skill.Value);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX"), unit);
            }

            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
        }

        // FIRE

        private void FireBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBolt_ImpactVFX"),
                target);
            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower() + "_Impact",
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            Enumerators.ActionType actionType = Enumerators.ActionType.None;

            if (target is Player)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnOverlord;
            }
            else if (target is BoardUnit)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnCard;
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target
                    }
                }
            });
        }

        private void RabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.SetAsFeralUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
        }

        private void FireballAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX"), target); // vfx Fireball

            _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);

            Enumerators.ActionType actionType = Enumerators.ActionType.None;

            if (target is Player)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnOverlord;
            }
            else if (target is BoardUnit)
            {
                actionType = Enumerators.ActionType.UweOverlordPowerOnCard;
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target
                    }
                }
            });
        }

        private void MassRabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = InternalTools.GetRandomElementsFromList(owner.BoardCards, skill.Value);

            foreach (var unit in units)
            {
                unit.SetAsFeralUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
        }

        private void MeteorShowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = new List<BoardUnit>();

            units.AddRange(_gameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(_gameplayManager.OpponentPlayer.BoardCards);

            foreach (var unit in units)
            {
                AttackWithModifiers(owner, boardSkill, skill, unit, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX"), unit); // meteor

                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
        }

        // EARTH

        private void StoneskinAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.BuffedHp += skill.Value;
                unit.CurrentHp += skill.Value;

                // TODO: remove this empty gameobject logic
                Transform transform = new GameObject().transform;
                transform.position = unit.Transform.position;
                transform.position -= Vector3.up * 3.3f;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform);
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = target
                    }
                }
            });
        }

        private void HardenAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _battleController.HealPlayerBySkill(owner, boardSkill, owner);

            // TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position -= Vector3.up * 3.3f;

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"),
                transform);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });
        }

        private void FortifyAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target != null && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.SetAsHeavyUnit();

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FortifyVFX"), unit);

                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.OverlordSkill.ToString().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UweOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Heavy,
                        Target = target
                    }
                }
            });
        }

        private void PhalanxAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.EARTH);

            Transform transform;
            foreach (var unit in units)
            {
                unit.BuffedHp += skill.Value;
                unit.CurrentHp += skill.Value;

                // TODO: remove this empty gameobject logic
                transform = new GameObject().transform;
                transform.position = unit.Transform.position;
                transform.position -= Vector3.up * 3.3f;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform); // vfx phalanx
                _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, skill.Title.Trim().ToLower(),
                    Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.NONE);
            }
        }

        private void FortressAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            List<BoardUnit> units = InternalTools.GetRandomElementsFromList(owner.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == Enumerators.SetType.EARTH), skill.Count);

            foreach(BoardUnit unit in units)
            {
                unit.SetAsHeavyUnit();
            }
        }

        #endregion
    }
}

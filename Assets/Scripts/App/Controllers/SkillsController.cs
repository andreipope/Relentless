using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using Loom.ZombieBattleground.View;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

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

        private IMatchManager _matchManager;

        private IPvPManager _pvpManager;

        private VfxController _vfxController;

        private BattleController _battleController;

        private ActionsQueueController _actionsQueueController;

        private CardsController _cardsController;

        private BattlegroundController _battlegroundController;

        private AbilitiesController _abilitiesController;

        private bool _skillsInitialized;

        private bool _isDirection;

        private List<ParametrizedAbilityBoardObject> _targets;

        private GameObject _buildParticlePrefab;

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
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _vfxController = _gameplayManager.GetController<VfxController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            _gameplayManager.GameEnded += GameplayManagerGameEnded;
        }

        public void Update()
        {
            if (_skillsInitialized)
            {
                PlayerPrimarySkill?.Update();
                PlayerSecondarySkill?.Update();
                OpponentPrimarySkill?.Update();
                OpponentSecondarySkill?.Update();
            }
        }

        public void ResetAll()
        {
            PlayerPrimarySkill = null;
            PlayerSecondarySkill = null;
            OpponentPrimarySkill = null;
            OpponentSecondarySkill = null;
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


            Enumerators.OverlordSkill primarySkillType;
            Enumerators.OverlordSkill secondarySkillType;
            if (_matchManager.MatchType == Enumerators.MatchType.PVP)
            {
                primarySkillType = (Enumerators.OverlordSkill) _gameplayManager.CurrentPlayer.InitialPvPPlayerState.Deck.PrimarySkill;
                secondarySkillType = (Enumerators.OverlordSkill) _gameplayManager.CurrentPlayer.InitialPvPPlayerState.Deck.SecondarySkill;
            }
            else
            {
                primarySkillType = _gameplayManager.CurrentPlayerDeck.PrimarySkill;
                secondarySkillType = _gameplayManager.CurrentPlayerDeck.SecondarySkill;
            }

            HeroSkill primary = _gameplayManager.CurrentPlayer.SelfHero.GetSkill(primarySkillType);
            HeroSkill secondary = _gameplayManager.CurrentPlayer.SelfHero.GetSkill(secondarySkillType);

            rootPage.SetupSkills(primary, secondary, false);
            SetPlayerSkills(rootPage, primary, secondary);

            primary = _gameplayManager.OpponentPlayer.SelfHero.GetSkill(_gameplayManager.OpponentPlayerDeck.PrimarySkill);
            secondary = _gameplayManager.OpponentPlayer.SelfHero.GetSkill(_gameplayManager.OpponentPlayerDeck.SecondarySkill);

            rootPage.SetupSkills(primary, secondary, true);
            SetOpponentSkills(rootPage, primary, secondary);

            _skillsInitialized = true;
        }

        public void DisableSkillsContent(Player player)
        {
            if (player.IsLocalPlayer)
            {
                PlayerPrimarySkill?.Hide();
                PlayerSecondarySkill?.Hide();
            }
            else
            {
                OpponentPrimarySkill?.Hide();
                OpponentSecondarySkill?.Hide();
            }
        }

        public void BlockSkill(Player player, Enumerators.SkillType type)
        {
            if (player.IsLocalPlayer)
            {
                PlayerPrimarySkill?.BlockSkill();
                PlayerSecondarySkill?.BlockSkill();
            }
            else
            {
                OpponentPrimarySkill?.BlockSkill();
                OpponentSecondarySkill?.BlockSkill();
            }
        }

        public void UnBlockSkill(Player player)
        {
            if (player.IsLocalPlayer)
            {
                PlayerPrimarySkill?.UnBlockSkill();
                PlayerSecondarySkill?.UnBlockSkill();
            }
            else
            {
                OpponentPrimarySkill?.UnBlockSkill();
                OpponentSecondarySkill?.UnBlockSkill();
            }
        }

        public void SetPlayerSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            if (primary != null)
            {
                PlayerPrimarySkill = new BoardSkill(rootPage.PlayerPrimarySkillHandler.gameObject,
                    _gameplayManager.CurrentPlayer,
                    primary,
                    true);
            }
            if (secondary != null)
            {
                PlayerSecondarySkill = new BoardSkill(rootPage.PlayerSecondarySkillHandler.gameObject,
                    _gameplayManager.CurrentPlayer,
                    secondary,
                    false);
            }
        }

        public void SetOpponentSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            if (primary != null)
            {
                OpponentPrimarySkill = new BoardSkill(rootPage.OpponentPrimarySkillHandler.gameObject,
                    _gameplayManager.OpponentPlayer,
                    primary,
                    true);
            }
            if (secondary != null)
            {
                OpponentSecondarySkill = new BoardSkill(rootPage.OpponentSecondarySkillHandler.gameObject,
                    _gameplayManager.OpponentPlayer,
                    secondary,
                    false);
            }
        }

        public void DoSkillAction(BoardSkill skill, Action completeCallback, List<ParametrizedAbilityBoardObject> targets)
        {
            if (skill == null || !skill.IsUsing)
            {
                completeCallback?.Invoke();
                return;
            }

            if (skill.FightTargetingArrow != null)
            {
                if (skill.FightTargetingArrow.SelectedPlayer != null)
                {
                    if (CheckSkillByType(skill))
                    {
                        Player targetPlayer = skill.FightTargetingArrow.SelectedPlayer;

                        string soundFile = GetSoundBySkills(skill);
                        if (!string.IsNullOrEmpty(soundFile))
                        {
                            _soundManager.PlaySound(
                                Enumerators.SoundType.OVERLORD_ABILITIES,
                                soundFile,
                                Constants.OverlordAbilitySoundVolume,
                                false);
                        }

                        targets = new List<ParametrizedAbilityBoardObject>()
                        {
                            new ParametrizedAbilityBoardObject(targetPlayer)
                        };

                        skill.UseSkill();
                        skill.SkillUsedAction(targets);
                        CreateSkillVfx(
                            GetVfxPrefabBySkill(skill),
                            skill.SelfObject.transform.position,
                            targetPlayer,
                            (x) =>
                            {
                                DoActionByType(skill, targets, completeCallback);
                            }, _isDirection);

                        if (_gameplayManager.CurrentTurnPlayer == _gameplayManager.CurrentPlayer)
                        {
                            PlayOverlordSkill playOverlordSkill = new PlayOverlordSkill(skill, targets);
                            _gameplayManager.PlayerMoves.AddPlayerMove(
                                new PlayerMove(Enumerators.PlayerActionType.PlayOverlordSkill, playOverlordSkill));
                        }
                    }
                    else
                    {
                        completeCallback?.Invoke();
                    }
                }
                else if (skill.FightTargetingArrow.SelectedCard != null)
                {
                    if (CheckSkillByType(skill))
                    {
                        BoardUnitView targetUnitView = skill.FightTargetingArrow.SelectedCard;

                        string soundFile = GetSoundBySkills(skill);
                        if (!string.IsNullOrEmpty(soundFile))
                        {
                            _soundManager.PlaySound(
                                Enumerators.SoundType.OVERLORD_ABILITIES,
                                soundFile,
                                Constants.OverlordAbilitySoundVolume,
                                false);
                        }

                        if (targets == null || targets.Count == 0)
                        {
                            targets = new List<ParametrizedAbilityBoardObject>()
                            {
                                new ParametrizedAbilityBoardObject(targetUnitView.Model)
                            };
                        }
                        skill.UseSkill();
                        _targets = targets;
                        CreateSkillVfx(
                            GetVfxPrefabBySkill(skill),
                            skill.SelfObject.transform.position,
                            targetUnitView,
                            (x) =>
                            {
                                DoActionByType(skill, targets, completeCallback);
                                skill.SkillUsedAction(_targets);
                            }, _isDirection);

                        if (_gameplayManager.CurrentTurnPlayer == _gameplayManager.CurrentPlayer)
                        {
                            PlayOverlordSkill playOverlordSkill = new PlayOverlordSkill(skill, targets);
                            _gameplayManager.PlayerMoves.AddPlayerMove(
                                new PlayerMove(Enumerators.PlayerActionType.PlayOverlordSkill, playOverlordSkill));
                        }
                    }
                    else
                    {
                        completeCallback?.Invoke();
                    }
                }
                else
                {
                    completeCallback?.Invoke();
                }

                skill.CancelTargetingArrows();
            }
            else if (targets != null && targets.Count > 0)
            {
                if (CheckSkillByType(skill))
                {
                    string soundFile = GetSoundBySkills(skill);
                    if (!string.IsNullOrEmpty(soundFile))
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.OVERLORD_ABILITIES, soundFile, Constants.OverlordAbilitySoundVolume, false);
                    }
                    skill.UseSkill();
                    _targets = targets;
                    CreateSkillVfx(
                        GetVfxPrefabBySkill(skill),
                        skill.SelfObject.transform.position,
                        targets[0].BoardObject,
                        (x) =>
                        {
                            DoActionByType(skill, targets, completeCallback);
                            skill.SkillUsedAction(_targets);
                        }, _isDirection);

                    if (_gameplayManager.CurrentTurnPlayer == _gameplayManager.CurrentPlayer)
                    {
                        PlayOverlordSkill playOverlordSkill = new PlayOverlordSkill(skill, targets);
                        _gameplayManager.PlayerMoves.AddPlayerMove(
                            new PlayerMove(Enumerators.PlayerActionType.PlayOverlordSkill, playOverlordSkill));
                    }
                }
                else
                {
                    completeCallback?.Invoke();
                }
            }
            else
            {
                completeCallback?.Invoke();
            }
        }

        private void CreateSkillVfx(GameObject prefab, Vector3 from, object target, Action<object> callbackComplete, bool isDirection = false)
        {
            if (_buildParticlePrefab == null)
            {
                _vfxController.CreateSkillVfx(prefab, from, target, callbackComplete, isDirection);
            }
            else
            {
                _vfxController.CreateSkillBuildVfx(_buildParticlePrefab, prefab, from, target, callbackComplete, isDirection);
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
            _isDirection = false;
            _buildParticlePrefab = null;
            GameObject prefab;
            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.ICE_BOLT:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");
                    _isDirection = true;
                    break;
                case Enumerators.OverlordSkill.FREEZE:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX");
                    break;
                case Enumerators.OverlordSkill.SHATTER:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Shatter_Projectile");
                    _buildParticlePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Shatter_BuildUp");
                    _isDirection = true;
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                case Enumerators.OverlordSkill.TOXIC_POWER:
                case Enumerators.OverlordSkill.INFECT:
                    _isDirection = true;
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
                    break;
                case Enumerators.OverlordSkill.FIREBALL:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBallVFX");
                    break;
                case Enumerators.OverlordSkill.FIRE_BOLT:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX");
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                case Enumerators.OverlordSkill.MEND:
                case Enumerators.OverlordSkill.HARDEN:
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.PUSH:
                case Enumerators.OverlordSkill.BREAKOUT:
                case Enumerators.OverlordSkill.DRAW:
                case Enumerators.OverlordSkill.BLIZZARD:
                case Enumerators.OverlordSkill.ENHANCE:
                case Enumerators.OverlordSkill.EPIDEMIC:
                case Enumerators.OverlordSkill.FORTIFY:
                case Enumerators.OverlordSkill.FORTRESS:
                case Enumerators.OverlordSkill.ICE_WALL:
                case Enumerators.OverlordSkill.LEVITATE:
                case Enumerators.OverlordSkill.MASS_RABIES:
                case Enumerators.OverlordSkill.METEOR_SHOWER:
                case Enumerators.OverlordSkill.PHALANX:
                case Enumerators.OverlordSkill.REANIMATE:
                case Enumerators.OverlordSkill.RESSURECT:
                case Enumerators.OverlordSkill.RETREAT:
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
                    soundFileName = skill.Skill.OverlordSkill.ToString().ToLowerInvariant();
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

        private bool CheckSkillByType(BoardSkill skill)
        {
            bool state = true;

            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.RESSURECT:
                    state = skill.OwnerPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == Enumerators.SetType.LIFE
                               && x.Prototype.CardKind == Enumerators.CardKind.CREATURE
                               && x.InstanceCard.Cost == skill.Skill.Value
                               && !skill.OwnerPlayer.BoardCards.Any(c => c.Model.Card == x)).Count > 0;
                    break;
                default:
                    break;
            }

            return state;
        }

        private void DoActionByType(BoardSkill skill, List<ParametrizedAbilityBoardObject> targets, Action completeCallback)
        {
            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.FREEZE:
                    FreezeAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.ICE_BOLT:
                    IceBoltAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                    PoisonDartAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.TOXIC_POWER:
                    ToxicPowerAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                    HealingTouchAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.MEND:
                    MendAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.FIRE_BOLT:
                    FireBoltAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.RABIES:
                    RabiesAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.HARDEN:
                    HardenAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.STONE_SKIN:
                    StoneskinAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.PUSH:
                    PushAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.DRAW:
                    DrawAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.WIND_SHIELD:
                    WindShieldAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.LEVITATE:
                    Levitate(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.RETREAT:
                    RetreatAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.BREAKOUT:
                    BreakoutAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.INFECT:
                    InfectAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.EPIDEMIC:
                    EpidemicAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.RESSURECT:
                    RessurectAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.REANIMATE:
                    ReanimateAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.ENHANCE:
                    EnhanceAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.ICE_WALL:
                    IceWallAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.SHATTER:
                    ShatterAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.BLIZZARD:
                    BlizzardAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.MASS_RABIES:
                    MassRabiesAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.METEOR_SHOWER:
                    MeteorShowerAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.FIREBALL:
                    FireballAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.FORTIFY:
                    FortifyAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.FORTRESS:
                    FortressAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                case Enumerators.OverlordSkill.PHALANX:
                    PhalanxAction(skill.OwnerPlayer, skill, skill.Skill, targets);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill.Skill.OverlordSkill), skill.Skill.OverlordSkill, null);
            }

            completeCallback?.Invoke();
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
                BoardUnitModel creature = (BoardUnitModel) target;
                int attackModifier = 0;
                _battleController.AttackUnitBySkill(owner, boardSkill, creature, attackModifier);
            }
        }

        private Dictionary<T, int> GetRandomTargetsByAmount<T>(List<T> root, int count)
        {
            Dictionary<T, int> targets = InternalTools.GetRandomElementsFromList(root, count).ToDictionary(x => x, с => 1);

            if (targets.Count < count)
            {
                int delta = count - targets.Count;
                for (int i = 0; i < delta; i++)
                {
                    targets[InternalTools.GetRandomElementsFromList(root, 1)[0]]++;
                }
            }
            return targets;
        }

        // AIR

        private void PushAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            owner.CurrentGoo = 0;

            BoardUnitModel targetUnit = (BoardUnitModel) targets[0].BoardObject;

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX"),
                targetUnit);

            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.OverlordSkill.ToString().ToLowerInvariant(),
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            _cardsController.ReturnCardToHand(_battlegroundController.GetBoardUnitViewByModel(targetUnit));

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Push,
                        Target = targets[0].BoardObject
                    }
                }
            });
        }

        private void DrawAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            _cardsController.AddCardToHand(owner);

            owner.PlayDrawCardVFX();

            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.OverlordSkill.ToString().ToLowerInvariant(),
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });
        }

        private void WindShieldAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();
            List<BoardUnitView> units;
            if (!boardSkill.IsLocal && targets != null)
            {
                units = targets.Select(target => _battlegroundController.GetBoardUnitViewByModel(target.BoardObject as BoardUnitModel)).ToList();
            }
            else
            {
                units =
                InternalTools.GetRandomElementsFromList(
                    owner.BoardCards.FindAll(x => x.Model.Card.Prototype.CardSetType == Enumerators.SetType.AIR),
                    skill.Value);

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target.Model)).ToList();
            }           

            foreach (BoardUnitView unit in units)
            {
                unit.Model.AddBuffShield();

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                    Target = unit
                });
            }

            if (TargetEffects.Count > 0)
            {
                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = TargetEffects
                });
            }
        }

        private void Levitate(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            int value = -skill.Value;
            WorkingCard card = null;
            if(!boardSkill.IsLocal && targets != null && targets.Count > 0)
            {
                card = owner.CardsInHand.FirstOrDefault(cardInHand => cardInHand.InstanceId.Id.ToString() == targets[0].Parameters.CardName);
            }

            card = _cardsController.LowGooCostOfCardInHand(owner, card, value);

            if(boardSkill.IsLocal)
            {
                _targets = new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(owner,
                        new ParametrizedAbilityParameters()
                        {
                            CardName = card.InstanceId.Id.ToString()
                        })
                };
            }

            if (owner.IsLocalPlayer)
            {
                BoardCardView boardCardView = _battlegroundController.PlayerHandCards.First(x => x.BoardUnitModel.Card.Equals(card));
                GameObject particle = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/LevitateVFX"));
                particle.transform.position = boardCardView.Transform.position;
                particle.transform.SetParent(boardCardView.Transform, true);
                particle.transform.localEulerAngles = Vector3.zero;
                _gameplayManager.GetController<ParticlesController>().RegisterParticleSystem(particle, true, 6f);
            }

            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.OverlordSkill.ToString().ToLowerInvariant(),
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.LowGooCost,
                        Target = card,
                        HasValue = true,
                        Value = value
                    }
                }
            });
        }

        private void RetreatAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardUnitView> units = new List<BoardUnitView>();
            units.AddRange(_gameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(_gameplayManager.OpponentPlayer.BoardCards);

            Vector3 position = Vector3.left * 2f;

            _vfxController.CreateVfx(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RetreatVFX"),
                position, delay: 6f);

            InternalTools.DoActionDelayed(() =>
            {
                foreach (BoardUnitView unit in units)
                {
                    _cardsController.ReturnCardToHand(unit);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ReturnToHand,
                        Target = unit
                    });
                }
            }, 2f);
            InternalTools.DoActionDelayed(() =>
            {
                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                    Caller = boardSkill,
                    TargetEffects = TargetEffects
                });
            }, 4f);
        }

        // TOXIC

        private void ToxicPowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0);

                unit.BuffedDamage += skill.Attack;
                unit.CurrentDamage += skill.Attack;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ToxicPowerVFX"),
                    unit, isIgnoreCastVfx: true);

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.AttackBuff,
                            Target = unit,
                            HasValue = true,
                            Value = skill.Attack
                        }
                    }
                });
            }
        }

        private void PoisonDartAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            AttackWithModifiers(owner, boardSkill, skill, targets[0].BoardObject, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
            _vfxController.CreateVfx(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"),
                targets[0].BoardObject, isIgnoreCastVfx: true);
            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            Enumerators.ActionType actionType;

            switch (targets[0].BoardObject)
            {
                case Player _:
                    actionType = Enumerators.ActionType.UseOverlordPowerOnOverlord;
                    break;
                case BoardUnitModel _:
                    actionType = Enumerators.ActionType.UseOverlordPowerOnCard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targets), targets[0].BoardObject, null);
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
                        Target = targets[0].BoardObject,
                        HasValue = true,
                        Value = -skill.Value
                    }
                }
            });
        }

        private void BreakoutAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            
            Dictionary<BoardObject, int> sortedTargets = null;

            if (!boardSkill.IsLocal && targets != null)
            {
                sortedTargets = targets.ToDictionary(target => target.BoardObject, target => target.Parameters.Attack);
            }
            else
            {
                List<BoardObject> boardObjects = new List<BoardObject>();
                Player opponent = _gameplayManager.GetOpponentByPlayer(owner);

                boardObjects.Add(opponent);

                List<BoardUnitModel> boardCradsModels = opponent.BoardCards.Select((x) => x.Model).ToList();

                boardObjects.AddRange(boardCradsModels);

                sortedTargets = GetRandomTargetsByAmount(boardObjects, skill.Count);

                _targets = sortedTargets.Select(target =>
                    new ParametrizedAbilityBoardObject(target.Key,
                        new ParametrizedAbilityParameters()
                        {
                            Attack = target.Value * skill.Value
                        })
                ).ToList();
            }

            GameObject prefabMovedVfx = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
            GameObject prefabImpactVfx = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX");

            int basicValue = skill.Value;
            int value = 0;

            foreach (BoardObject targetObject in sortedTargets.Keys)
            {
                _vfxController.CreateSkillVfx(
                prefabMovedVfx,
                boardSkill.SelfObject.transform.position,
                targetObject,
                (x) =>
                {
                    value = basicValue * sortedTargets[targetObject];

                    switch (targetObject)
                    {
                        case Player player:
                            _battleController.AttackPlayerBySkill(owner, boardSkill, player, value);
                            break;
                        case BoardUnitModel unit:
                            _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0, value);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(targetObject), targetObject, null);
                    }

                    _vfxController.CreateVfx(
                        prefabImpactVfx,
                        targetObject, isIgnoreCastVfx: true);
                    _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.Title.Trim().ToLowerInvariant() + "_Impact",
                        Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = targetObject,
                        HasValue = true,
                        Value = -value
                    });
                }, true);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnCardsWithOverlord,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        private void InfectAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                int unitAtk = unit.CurrentDamage;

                _vfxController.CreateVfx(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/InfectVFX"),
                targets[0].BoardObject, delay: 8f, isIgnoreCastVfx:true);

                BoardUnitView targetUnit = null;
                if (boardSkill.IsLocal)
                {
                    IReadOnlyList<BoardUnitView> opponentUnits = _gameplayManager.GetOpponentByPlayer(owner).BoardCards;

                    if (opponentUnits.Count == 0)
                        return;

                    targetUnit = opponentUnits[UnityEngine.Random.Range(0, opponentUnits.Count)];

                    _targets.Add(new ParametrizedAbilityBoardObject(targetUnit.Model));
                }
                else
                {
                    if (targets.Count == 1)
                        return;

                    targetUnit = _battlegroundController.GetBoardUnitViewByModel(targets[1].BoardObject as BoardUnitModel);
                }

                InternalTools.DoActionDelayed(() =>
                {
                    _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Infect_ExplosionVFX"),
                    unit, delay: 6f, isIgnoreCastVfx: true);
                    _battlegroundController.DestroyBoardUnit(unit, false, true);

                    _vfxController.CreateSkillVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX"),
                    _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position,
                    targetUnit,
                    (x) =>
                    {
                        _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"),
                        targetUnit, isIgnoreCastVfx: true);
                        _battleController.AttackUnitBySkill(owner, boardSkill, targetUnit.Model, 0);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                            Caller = boardSkill,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                            {
                                new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                                    Target = unit,
                                    HasValue = true,
                                    Value = -unitAtk
                                },
                                new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                                    Target = unit,
                                }
                            }
                        });
                    }, true);                    
                }, 3.5f);
            }
        }

        private void EpidemicAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<BoardUnitView> units = null;
            List<BoardUnitView> opponentUnits = null;
            Dictionary<BoardUnitView, int> unitAttacks = new Dictionary<BoardUnitView, int>();
            List<BoardUnitView> opponentUnitsTakenDamage = new List<BoardUnitView>();
            int unitAtk = 0;
            BoardUnitView opponentUnitView = null;
            BoardUnitView unitView = null;
            Action<BoardUnitView> callback = null;
            int count = 0;

            if (!boardSkill.IsLocal && targets != null)
            {
                count = targets.Count;
            }
            else
            {
                units = owner.BoardCards.FindAll(x => x.Model.Card.Prototype.CardSetType == Enumerators.SetType.TOXIC);
                units = InternalTools.GetRandomElementsFromList(units, skill.Count);
                count = units.Count;
                opponentUnits = InternalTools.GetRandomElementsFromList(_gameplayManager.GetOpponentByPlayer(owner).BoardCards, skill.Count);

                _targets = new List<ParametrizedAbilityBoardObject>();
            }

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                callback = null;
                if (boardSkill.IsLocal)
                {
                    unitView = units[i];
                    unitAtk = unitView.Model.CurrentDamage;
                    opponentUnitView = null;

                    if (opponentUnits.Count > 0)
                    {
                        opponentUnitView = opponentUnits[UnityEngine.Random.Range(0, opponentUnits.Count)];

                        opponentUnits.Remove(opponentUnitView);
                        opponentUnitsTakenDamage.Add(opponentUnitView);
                    }
                    else if (opponentUnitsTakenDamage.Count > 0)
                    {
                        opponentUnitView = opponentUnitsTakenDamage[UnityEngine.Random.Range(0, opponentUnitsTakenDamage.Count)];
                    }
                }
                else
                {
                    unitView = _battlegroundController.GetBoardUnitViewByModel(targets[i].BoardObject as BoardUnitModel);
                    unitAtk = unitView.Model.CurrentDamage;
                    opponentUnitView = _gameplayManager.GetOpponentByPlayer(owner).BoardCards.FirstOrDefault(card => card.Model.InstanceId.Id.ToString() == targets[i].Parameters.CardName);
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                    Target = unitView
                });

                if (opponentUnitView != null)
                {
                    if (unitAttacks.ContainsKey(opponentUnitView))
                    {
                        unitAttacks[opponentUnitView] += unitAtk;
                        TargetEffects.Find(x => x.Target == opponentUnitView).Value -= unitAtk;
                    }
                    else
                    {
                        unitAttacks.Add(opponentUnitView, unitAtk);
                        TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = opponentUnitView,
                            HasValue = true,
                            Value = -unitAtk
                        });
                    }

                    callback = (unit) =>
                    {
                        if (unitAttacks.ContainsKey(opponentUnitView))
                        {
                            _battleController.AttackUnitBySkill(owner, boardSkill, unit.Model, 0, unitAttacks[unit]);
                            unitAttacks.Remove(unit);
                        }
                    };

                    if (boardSkill.IsLocal)
                    {
                        _targets.Add(new ParametrizedAbilityBoardObject(unitView.Model,
                            new ParametrizedAbilityParameters()
                            {
                                CardName = opponentUnitView.Model.InstanceId.Id.ToString()
                            }));
                    }
                }
                EpidemicUnit(owner, boardSkill, skill, unitView, opponentUnitView, callback);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }
    
        private void EpidemicUnit(Player owner, BoardSkill boardSkill, HeroSkill skill, BoardUnitView unit, BoardUnitView target, Action<BoardUnitView> callback)
        {
            _vfxController.CreateVfx(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/InfectVFX"),
            unit, delay: 8f, isIgnoreCastVfx: true);

            InternalTools.DoActionDelayed(() =>
            {
                _vfxController.CreateVfx(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Infect_ExplosionVFX"),
                unit, delay: 6f, isIgnoreCastVfx: true);
                _battlegroundController.DestroyBoardUnit(unit.Model, false, true);

                if (target != null)
                {
                    _vfxController.CreateSkillVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX"),
                    _battlegroundController.GetBoardUnitViewByModel(unit.Model).Transform.position,
                    target,
                    (x) =>
                    {
                        callback?.Invoke(target);
                        _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"),
                        target);
                    }, true);
                }
            }, 3.5f);
        }

        // LIFE

        private void HealingTouchAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets.Count > 0)
            {
                if (targets[0].BoardObject is Player player)
                {
                    _battleController.HealPlayerBySkill(owner, boardSkill, player);

                    _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"),
                        player);
                    _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.OverlordSkill.ToString().ToLowerInvariant(),
                        Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);
                }
                else
                {
                    BoardUnitModel unit = (BoardUnitModel)targets[0].BoardObject;

                    _battleController.HealUnitBySkill(owner, boardSkill, unit);

                    _vfxController.CreateVfx(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"),
                        unit);
                    _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.OverlordSkill.ToString().ToLowerInvariant(),
                        Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);
                }

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                            Target = targets[0].BoardObject,
                            HasValue = true,
                            Value = skill.Value
                        }
                    }
                });
            }
        }

        private void MendAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            owner.Defense = Mathf.Clamp(owner.Defense + skill.Value, 0, owner.MaxCurrentHp);

            // TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position += Vector3.up * 2;
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MendVFX"), transform);
            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.OverlordSkill.ToString().ToLowerInvariant(),
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                   new PastActionsPopup.TargetEffectParam()
                   {
                       ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                       Target = owner,
                       HasValue = true,
                       Value = skill.Value
                   }
                }
            });
        }

        private void RessurectAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<WorkingCard> cards = null;

            if (!boardSkill.IsLocal && targets != null)
            {
                List<WorkingCard> foundCards = new List<WorkingCard>();

                foreach (ParametrizedAbilityBoardObject boardObject in targets)
                {
                    foundCards.Add(owner.CardsInGraveyard.FirstOrDefault(card => card.InstanceId.Id.ToString() == boardObject.Parameters.CardName));
                }

                cards = foundCards;
            }
            else
            {
                cards = owner.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == Enumerators.SetType.LIFE
                                                       && x.Prototype.CardKind == Enumerators.CardKind.CREATURE
                                                       && x.InstanceCard.Cost == skill.Value
                                                       && !owner.BoardCards.Any(c => c.Model.Card == x));

                cards = InternalTools.GetRandomElementsFromList(cards, skill.Count);

                _targets = cards
                    .Select(target => new ParametrizedAbilityBoardObject(owner,
                        new ParametrizedAbilityParameters()
                        {
                            CardName = target.InstanceId.Id.ToString()
                        }))
                    .ToList();
            }

            BoardUnitView unit = null;

            foreach (WorkingCard card in cards)
            {
                unit = _cardsController.SpawnUnitOnBoard(
                    owner,
                    card,
                    ItemPosition.End,
                    onComplete: () =>
                    {
                        _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ResurrectVFX"),
                            unit,
                            delay: 6,
                            isIgnoreCastVfx: true);
                        InternalTools.DoActionDelayed(() =>
                            {
                                unit.ChangeModelVisibility(true);
                                unit.StopSleepingParticles();

                                if (!unit.Model.OwnerPlayer.Equals(_gameplayManager.CurrentTurnPlayer))
                                {
                                    unit.Model.IsPlayable = true;
                                }

                                if (unit.Model.OwnerPlayer.IsLocalPlayer)
                                {
                                    _abilitiesController.ActivateAbilitiesOnCard(unit.Model, unit.Model.Card, unit.Model.OwnerPlayer);
                                }
                            },
                            3f);

                        TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                            Target = unit,
                        });
                    });
                unit.ChangeModelVisibility(false);
                owner.RemoveCardFromGraveyard(card);
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        private void EnhanceAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardObject> boardObjects = new List<BoardObject>();

            if (!boardSkill.IsLocal && targets != null)
            {
                boardObjects = targets.Select(target => target.BoardObject).ToList();
            }
            else
            {
                boardObjects.Add(owner);
                boardObjects.AddRange(owner.BoardCards.FindAll(x => x.Model.Card.Prototype.CardSetType == Enumerators.SetType.LIFE).Select(unit => unit.Model));

                _targets = boardObjects.Select(target => new ParametrizedAbilityBoardObject(target)).ToList();
            }

            foreach (BoardObject targetObject in boardObjects)
            {
                switch (targetObject)
                {
                    case BoardUnitModel unit:
                        {
                            _battleController.HealUnitBySkill(owner, boardSkill, unit);
                            _vfxController.CreateVfx(
                            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"),
                            _battlegroundController.GetBoardUnitViewByModel(unit));
                            _soundManager.PlaySound(
                                Enumerators.SoundType.OVERLORD_ABILITIES,
                                skill.OverlordSkill.ToString().ToLowerInvariant(),
                                Constants.OverlordAbilitySoundVolume,
                                Enumerators.CardSoundType.NONE);
                        }
                        break;
                    case Player player:
                        {
                            _battleController.HealPlayerBySkill(owner, boardSkill, player);
                            Transform transform = new GameObject().transform;
                            transform.position = owner.AvatarObject.transform.position;
                            transform.position += Vector3.up * 2.25f;
                            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MendVFX"), transform);
                            _soundManager.PlaySound(
                                Enumerators.SoundType.OVERLORD_ABILITIES,
                                skill.OverlordSkill.ToString().ToLowerInvariant(),
                                Constants.OverlordAbilitySoundVolume,
                                Enumerators.CardSoundType.NONE);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetObject), targetObject, null);
                }

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.LifeGain,
                    Target = targetObject,
                    HasValue = true,
                    Value = skill.Value
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnCardsWithOverlord,
                Caller = boardSkill,
                TargetEffects = targetEffects
            });
        }

        private void ReanimateAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<WorkingCard> cards = null;

            if (!boardSkill.IsLocal && targets != null)
            {
                List<WorkingCard> foundCards = new List<WorkingCard>();

                foreach(ParametrizedAbilityBoardObject boardObject in targets)
                {
                    foundCards.Add(owner.CardsInGraveyard.FirstOrDefault(card => card.InstanceId.Id.ToString() == boardObject.Parameters.CardName));
                }

                cards = foundCards;
            }
            else
            {
                cards = owner.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == Enumerators.SetType.LIFE
                                                        && x.Prototype.CardKind == Enumerators.CardKind.CREATURE
                                                        && !owner.BoardCards.Any(c => c.Model.Card == x));

                cards = InternalTools.GetRandomElementsFromList(cards, skill.Count);

                _targets = cards
                    .Select(target => new ParametrizedAbilityBoardObject(owner,
                        new ParametrizedAbilityParameters()
                        {
                            CardName = target.InstanceId.Id.ToString()
                        }))
                    .ToList();
            }

            List<BoardUnitView> units = new List<BoardUnitView>();

            foreach (WorkingCard card in cards)
            {
                if (owner.BoardCards.Count >= owner.MaxCardsInPlay)
                    break;

                units.Add(_cardsController.SpawnUnitOnBoard(
                    owner,
                    card,
                    ItemPosition.End,
                    onComplete: () =>
                    {
                        ReanimateUnit(units);
                    }));
                units[units.Count - 1].ChangeModelVisibility(false);
                owner.RemoveCardFromGraveyard(card);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Reanimate,
                    Target = units[units.Count - 1]
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        private void ReanimateUnit(List<BoardUnitView> units)
        {
            foreach (BoardUnitView unit in units)
            {
                unit.StopSleepingParticles();
                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ResurrectVFX"), unit, delay: 6, isIgnoreCastVfx: true);
                InternalTools.DoActionDelayed(() =>
                {
                    unit.ChangeModelVisibility(true);

                    if (!unit.Model.OwnerPlayer.Equals(_gameplayManager.CurrentTurnPlayer))
                    {
                        unit.Model.IsPlayable = true;
                    }

                    if (unit.Model.OwnerPlayer.IsLocalPlayer)
                    {
                        _abilitiesController.ActivateAbilitiesOnCard(unit.Model, unit.Model.Card, unit.Model.OwnerPlayer);
                    }
                }, 3f);
            }
        }

        // WATER

        private void FreezeAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets.Count > 0)
            {
                Enumerators.ActionType actionType;

                switch (targets[0].BoardObject)
                {
                    case BoardUnitModel unit:
                        unit.Stun(Enumerators.StunType.FREEZE, skill.Value);

                        _vfxController.CreateVfx(
                            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"),
                            unit, isIgnoreCastVfx: true);

                        _soundManager.PlaySound(
                            Enumerators.SoundType.OVERLORD_ABILITIES,
                            skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                            Constants.OverlordAbilitySoundVolume,
                            Enumerators.CardSoundType.NONE);

                        actionType = Enumerators.ActionType.UseOverlordPowerOnCard;

                        break;
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, skill.Value);

                        _vfxController.CreateVfx(
                            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"),
                            player, isIgnoreCastVfx: true);
                        _soundManager.PlaySound(
                            Enumerators.SoundType.OVERLORD_ABILITIES,
                            skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                            Constants.OverlordAbilitySoundVolume,
                            Enumerators.CardSoundType.NONE);

                        actionType = Enumerators.ActionType.UseOverlordPowerOnOverlord;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targets), targets, null);
                }

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Freeze,
                        Target = targets[0].BoardObject
                    }
                }
                });
            }
        }

        private void IceBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                _battleController.AttackUnitBySkill(owner, boardSkill, unit, 0);

                if (unit.CurrentHp > 0)
                {
                    unit.Stun(Enumerators.StunType.FREEZE, 1);
                }

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBolt_Impact"),
                    unit, isIgnoreCastVfx: true);
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = unit,
                            HasValue = true,
                            Value = -skill.Value
                        },
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Freeze,
                            Target = unit
                        }
                    }
                });
            }
        }

        private void IceWallAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                BoardObject target = targets[0].BoardObject;

                Enumerators.ActionType actionType;

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceWallVFX"), target, delay: 8, isIgnoreCastVfx: true);

                InternalTools.DoActionDelayed(() =>
                {
                    switch (target)
                    {
                        case BoardUnitModel unit:
                            unit.BuffedHp += skill.Value;
                            unit.CurrentHp += skill.Value;
                            actionType = Enumerators.ActionType.UseOverlordPowerOnCard;
                            break;
                        case Player player:
                            _battleController.HealPlayerBySkill(owner, boardSkill, player);
                            actionType = Enumerators.ActionType.UseOverlordPowerOnOverlord;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(target), target, null);
                    }

                    _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.Title.Trim().ToLowerInvariant(),
                        Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);


                    _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                        ActionType = actionType,
                        Caller = boardSkill,
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                            Target = target,
                            HasValue = true,
                            Value = skill.Value
                        }
                        }
                    });
                }, 2f);
            }
        }

        private void ShatterAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                BoardObject target = targets[0].BoardObject;
                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Shatter_ImpactVFX"), target, isIgnoreCastVfx: true);

                if (target is BoardUnitModel boardUnitModel)
                {
                    Vector3 position = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel)target).Transform.position + Vector3.up * 0.34f;

                    boardUnitModel.LastAttackingSetType = owner.SelfHero.HeroElement;
                    _battlegroundController.DestroyBoardUnit(boardUnitModel, false, true);

                    _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                        Constants.OverlordAbilitySoundVolume,
                        Enumerators.CardSoundType.NONE);
                }

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
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
        }

        private void BlizzardAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<BoardUnitView> units = null;

            if (targets != null && !boardSkill.IsLocal)
            {
                units = targets.Select(target => _battlegroundController.GetBoardUnitViewByModel(target.BoardObject as BoardUnitModel)).ToList();
            }
            else
            {
                units = _gameplayManager.GetOpponentByPlayer(owner).BoardCards;
                units = InternalTools.GetRandomElementsFromList(units, skill.Count);

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target.Model)).ToList();
            }

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/BlizzardVFX"), Vector3.zero, true, 8);

            GameObject prefabFreeze = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Blizzard_Freeze");
            Vector3 targetPosition = Vector3.zero;

            foreach (BoardUnitView unit in units)
            {
                targetPosition = unit.Transform.position + Vector3.up * 0.7f;

                _vfxController.CreateVfx(prefabFreeze, targetPosition, true, 6);

                InternalTools.DoActionDelayed(() =>
                {
                    unit.Model.Stun(Enumerators.StunType.FREEZE, skill.Value);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Freeze,
                        Target = unit
                    });
                }, 3.5f);
            }

            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skill.Title.Trim().ToLowerInvariant(),
                Constants.OverlordAbilitySoundVolume,
                Enumerators.CardSoundType.NONE);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        // FIRE

        private void FireBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                BoardObject target = targets[0].BoardObject;
                AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBolt_ImpactVFX"), target);
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.OverlordSkill.ToString().ToLowerInvariant() + "_Impact",
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                Enumerators.ActionType actionType;

                switch (target)
                {
                    case Player _:
                        actionType = Enumerators.ActionType.UseOverlordPowerOnOverlord;
                        break;
                    case BoardUnitModel _:
                        actionType = Enumerators.ActionType.UseOverlordPowerOnCard;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
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
                        Target = target,
                        HasValue = true,
                        Value = -skill.Value
                    }
                }
                });
            }
        }

        private void RabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                unit.SetAsFeralUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"),
                    unit, delay: 14f, isIgnoreCastVfx: true);
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.OverlordSkill.ToString().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Feral,
                            Target = unit
                        }
                    }
                });
            }
        }

        private void FireballAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                BoardObject target = targets[0].BoardObject;
                AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBall_ImpactVFX"), target, isIgnoreCastVfx: true); // vfx Fireball
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.Title.Trim().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                Enumerators.ActionType actionType;

                switch (target)
                {
                    case Player _:
                        actionType = Enumerators.ActionType.UseOverlordPowerOnOverlord;
                        break;
                    case BoardUnitModel _:
                        actionType = Enumerators.ActionType.UseOverlordPowerOnCard;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
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
                        Target = target,
                        HasValue = true,
                        Value = -skill.Value
                    }
                }
                });
            }
        }

        private void MassRabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<BoardUnitView> units = null;

            if (!boardSkill.IsLocal && targets != null)
            {
                units = targets.Select(target => _battlegroundController.GetBoardUnitViewByModel(target.BoardObject as BoardUnitModel)).ToList();
            }
            else
            {
                units = owner.BoardCards.FindAll(
                    x => !x.Model.HasFeral && x.Model.Card.Prototype.CardSetType == owner.SelfHero.HeroElement);

                units = InternalTools.GetRandomElementsFromList(units, skill.Count);

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target.Model)).ToList();
            }

            foreach (BoardUnitView unit in units)
            {
                unit.Model.SetAsFeralUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"),
                    unit, delay: 14f, isIgnoreCastVfx: true);
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.Title.Trim().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Feral,
                    Target = unit
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = targetEffects
            });
        }

        private void MeteorShowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardUnitModel> units = new List<BoardUnitModel>();

            if (!boardSkill.IsLocal && targets != null)
            {
                units = targets.Select(target => target.BoardObject as BoardUnitModel).ToList();
            }
            else
            {
                List<BoardUnitView> unitsViews = new List<BoardUnitView>();

                unitsViews.AddRange(_gameplayManager.CurrentPlayer.BoardCards);
                unitsViews.AddRange(_gameplayManager.OpponentPlayer.BoardCards);

                units = unitsViews.Select((x) => x.Model).ToList();

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target)).ToList();
            }

            GameObject vfxObject = null;

            foreach (BoardUnitModel unit in units)
            {
                InternalTools.DoActionDelayed(() =>
                {
                    AttackWithModifiers(owner, boardSkill, skill, unit, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
                }, 2.5f);

                vfxObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MeteorShowerVFX"));
                vfxObject.transform.position =  _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                _gameplayManager.GetController<ParticlesController>().RegisterParticleSystem(vfxObject, true, 8);

                string skillTitle = skill.OverlordSkill.ToString().ToLowerInvariant();

                ParticleSystem particle = vfxObject.transform.Find("Particle System/MeteorShowerVFX").GetComponent<ParticleSystem>();
                MeteorShowerEmit(particle, 3, skillTitle);

                vfxObject.transform.Find("Particle System/MeteorShowerVFX/Quad").GetComponent<OnBehaviourHandler>().OnParticleCollisionEvent += (obj) =>
                    MeteorShowerImpact(obj, skillTitle);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                    Target = unit,
                    HasValue = true,
                    Value = -skill.Value
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        private void MeteorShowerEmit(ParticleSystem particle, int count, string skillTitle)
        {
            if (particle == null)
                return;

            if(count > 0)
            {
                particle.Emit(1);

                _soundManager.PlaySound(
                        Enumerators.SoundType.OVERLORD_ABILITIES,
                        skillTitle + "_Moving_0" + UnityEngine.Random.Range(0, 4).ToString(),
                        Constants.OverlordAbilitySoundVolume,
                        false);

                count--;
                float delay = UnityEngine.Random.Range(0.2f, 0.5f);
                InternalTools.DoActionDelayed(() => MeteorShowerEmit(particle, count, skillTitle), delay);
            }
        }

        private void MeteorShowerImpact(GameObject obj, string skillTitle)
        {
            _soundManager.PlaySound(
                Enumerators.SoundType.OVERLORD_ABILITIES,
                skillTitle + "_Impact_0" + UnityEngine.Random.Range(0, 4).ToString(),
                Constants.OverlordAbilitySoundVolume,
                false);
        }
        // EARTH

        private void StoneskinAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                unit.BuffedHp += skill.Value;
                unit.CurrentHp += skill.Value;

                Vector3 position = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                position -= Vector3.up * 3.6f;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HardenStoneSkinVFX"),
                    position, isIgnoreCastVfx:true);
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.OverlordSkill.ToString().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                            Target = unit,
                            HasValue = true,
                            Value = skill.Value
                        }
                    }
                });
            }
        }

        private void HardenAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            _battleController.HealPlayerBySkill(owner, boardSkill, owner);

            Vector3 position = owner.AvatarObject.transform.position;
            position -= new Vector3(0.07f, 4f, 0f);

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HardenStoneSkinVFX"),
                position, isIgnoreCastVfx: true);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPower,
                Caller = boardSkill,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = owner,
                        HasValue = true,
                        Value = skill.Value
                    }
                }
            });
        }

        private void FortifyAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            if (targets != null && targets.Count > 0 && targets[0].BoardObject is BoardUnitModel unit)
            {
                unit.SetAsHeavyUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FortressVFX"),
                    unit, isIgnoreCastVfx: true);

                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.OverlordSkill.ToString().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                    Caller = boardSkill,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Heavy,
                            Target = unit
                        }
                    }
                });
            }
        }

        private void PhalanxAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            IReadOnlyList<BoardUnitView> units = null;

            if (!boardSkill.IsLocal && targets != null)
            {
                units = targets.Select(target => _battlegroundController.GetBoardUnitViewByModel(target.BoardObject as BoardUnitModel)).ToList();
            }
            else
            {
                units = owner.BoardCards.FindAll(x => x.Model.Card.Prototype.CardSetType == Enumerators.SetType.EARTH);

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target.Model)).ToList();
            }

            Vector3 position;
            foreach (BoardUnitView unit in units)
            {
                unit.Model.BuffedHp += skill.Value;
                unit.Model.CurrentHp += skill.Value;

                position = unit.Transform.position;
                position -= Vector3.up * 3.65f;

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HardenStoneSkinVFX"),
                    position, delay: 8f, isIgnoreCastVfx: true); // vfx phalanx
                _soundManager.PlaySound(
                    Enumerators.SoundType.OVERLORD_ABILITIES,
                    skill.Title.Trim().ToLowerInvariant(),
                    Constants.OverlordAbilitySoundVolume,
                    Enumerators.CardSoundType.NONE);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                    Target = unit,
                    HasValue = true,
                    Value = skill.Value
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        private void FortressAction(Player owner, BoardSkill boardSkill, HeroSkill skill, List<ParametrizedAbilityBoardObject> targets)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardUnitView> units = null;
            if (!boardSkill.IsLocal && targets != null)
            {
                units = targets.Select(target => _battlegroundController.GetBoardUnitViewByModel(target.BoardObject as BoardUnitModel)).ToList();
            }
            else
            {
                units = InternalTools.GetRandomElementsFromList(
                        owner.BoardCards.FindAll(x => x.Model.Card.Prototype.CardSetType == Enumerators.SetType.EARTH),
                        skill.Count);

                _targets = units.Select(target => new ParametrizedAbilityBoardObject(target.Model)).ToList();
            }

            foreach (BoardUnitView unit in units)
            {
                unit.Model.SetAsHeavyUnit();

                _vfxController.CreateVfx(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FortressVFX"), unit.Transform.position, true, 6f);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Heavy,
                    Target = unit
                });
            }

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnMultilpleCards,
                Caller = boardSkill,
                TargetEffects = TargetEffects
            });
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground
{
    public class CardModel : IOwnableBoardObject, IInstanceIdOwner
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardModel));

        public bool AttackedThisTurn;

        public bool HasFeral;

        public bool HasHeavy;

        public int NumTurnsOnBoard;

        public bool HasUsedBuffShield;

        public bool HasSwing;

        public bool CanAttackByDefault;

        public UniqueList<IBoardObject> AttackedBoardObjectsThisTurn;

        public Enumerators.AttackRestriction AttackRestriction = Enumerators.AttackRestriction.ANY;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly BattlegroundController _battlegroundController;

        private readonly BattleController _battleController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly AbilitiesController _abilitiesController;

        private readonly PlayerController _playerController;

        private readonly IPvPManager _pvpManager;

        private int _stunTurns;

        public bool IsDead { get; private set; }

        public InstanceId InstanceId => Card.InstanceId;

        public Player OwnerPlayer => Card.Owner;

        public List<Enumerators.SkillTarget> AttackTargetsAvailability;

        public int TutorialObjectId => Card.TutorialObjectId;

        public Sprite CardPicture { get; private set; }

        public CardModel(WorkingCard card)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _pvpManager = GameClient.Get<IPvPManager>();

            BuffsOnUnit = new List<Enumerators.BuffType>();
            AttackedBoardObjectsThisTurn = new UniqueList<IBoardObject>();

            IsCreatedThisTurn = true;

            CanAttackByDefault = true;

            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;

            AttackTargetsAvailability = new List<Enumerators.SkillTarget>()
            {
                Enumerators.SkillTarget.OPPONENT,
                Enumerators.SkillTarget.OPPONENT_CARD
            };

            IsAllAbilitiesResolvedAtStart = true;

            SetObjectInfo(card);
        }

        public event Action TurnStarted;

        public event Action TurnEnded;

        public event Action<bool> Stunned;

        public event Action UnitDied;

        public event Action UnitDying;

        public event Action<IBoardObject, int, bool> UnitAttacked;

        public event Action UnitAttackedEnded;

        public event Action<IBoardObject> UnitDamaged;

        public event Action<IBoardObject> PrepairingToDie;

        public event PropertyChangedEvent<int> UnitDefenseChanged;

        public event PropertyChangedEvent<int> UnitDamageChanged;

        public event Action<Enumerators.CardType> CardTypeChanged;

        public event Action<Enumerators.BuffType> BuffApplied;

        public event Action<bool> BuffShieldStateChanged;

        public event Action CreaturePlayableForceSet;

        public event Action UnitFromDeckRemoved;

        public event Action UnitDistracted;

        public event Action<bool> UnitDistractEffectStateChanged;

        public event Action<CardModel> KilledUnit;

        public event Action<bool> BuffSwingStateChanged;

        public event Action GameMechanicDescriptionsOnUnitChanged;

        public event Action CardPictureWasUpdated;

        public Enumerators.CardType InitialUnitType { get; private set; }

        public int MaxCurrentDamage => Card.Prototype.Damage + BuffedDamage;

        public int BuffedDamage { get; set; }

        public int CurrentDamage
        {
            get => Card.InstanceCard.Damage;
            set
            {
                int oldValue = Card.InstanceCard.Damage;
                value = Mathf.Max(value, 0);
                if (oldValue == value)
                    return;

                Card.InstanceCard.Damage = value;
                UnitDamageChanged?.Invoke(oldValue, value);
            }
        }

        public int MaxCurrentDefense => Card.Prototype.Defense + BuffedDefense;

        public int BuffedDefense { get; set; }

        public int CurrentDefense
        {
            get => Card.InstanceCard.Defense;
            set
            {
                int oldValue = Card.InstanceCard.Defense;
                value = Mathf.Clamp(value, 0, 99);
                if (oldValue == value)
                    return;

                Card.InstanceCard.Defense = value;
                UnitDefenseChanged?.Invoke(oldValue, value);
            }
        }

        public bool IsPlayable { get; set; }

        public WorkingCard Card { get; set; }

        public bool IsStun => _stunTurns > 0;

        public bool IsCreatedThisTurn { get; private set; }

        public List<Enumerators.BuffType> BuffsOnUnit { get; }

        public bool HasBuffRush { get; set; }

        public bool HasBuffHeavy { get; set; }

        public bool HasBuffShield { get; set; }

        public bool TakeFreezeToAttacked { get; set; }

        public int AdditionalDamage { get; set; }

        public int DamageDebuffUntillEndOfTurn { get; set; }

        public int HpDebuffUntillEndOfTurn { get; set; }

        public bool IsAttacking { get; private set; }

        public bool IsAllAbilitiesResolvedAtStart { get; set; }

        public bool IsReanimated { get; set; }

        public bool AttackAsFirst { get; set; }

        public bool AgileEnabled { get; private set; }

        public Enumerators.UnitSpecialStatus UnitSpecialStatus { get; set; }

        public Enumerators.Faction LastAttackingSetType { get; set; }

        public bool CantAttackInThisTurnBlocker { get; set; } = false;

        public IFightSequenceHandler FightSequenceHandler;

        public bool IsHeavyUnit => HasBuffHeavy || HasHeavy;

        public List<Enumerators.GameMechanicDescription> GameMechanicDescriptionsOnUnit { get; } = new List<Enumerators.GameMechanicDescription>();

        public GameplayActionQueueAction<object> ActionForDying;

        public bool WasDistracted { get; private set; }

        public bool IsUnitActive { get; private set; } = true;

        public int MaximumDamageFromAnySource { get; private set; } = 999;

        // =================== REMOVE HARD

        public Player Owner
        {
            get => Card.Owner;
            set => Card.Owner = value;
        }

        public CardInstanceSpecificData InstanceCard => Card.InstanceCard;

        public IReadOnlyCard Prototype => Card.Prototype;

        public string Name => Card.Prototype.Name;

        public Enumerators.Faction Faction => Card.Prototype.Faction;

        // ===================

        public void HandleDefenseBuffer(int damage)
        {
            if(CurrentDefense - damage <= 0 && !HasBuffShield)
            {
                SetUnitActiveStatus(false);
            }
        }

        public void SetUnitActiveStatus(bool isActive)
        {
            IsUnitActive = isActive;
        }

        public void Die(bool forceUnitDieEvent= false, bool withDeathEffect = true, bool updateBoard = true, bool isDead = true)
        {
            UnitDying?.Invoke();

            IsDead = isDead;
            if (!forceUnitDieEvent)
            {
                _battlegroundController.KillBoardCard(this, withDeathEffect, updateBoard);
            }
            else
            {
                InvokeUnitDied();
            }
        }

        public void ResolveBuffShield () {
            if (HasUsedBuffShield) {
                HasUsedBuffShield = false;
                UseShieldFromBuff();
            }
        }

        public void AddBuff(Enumerators.BuffType type)
        {
            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            BuffsOnUnit.Add(type);
        }

        public void ApplyBuff(Enumerators.BuffType type)
        {
            switch (type)
            {
                case Enumerators.BuffType.ATTACK:
                    CurrentDamage++;
                    BuffedDamage++;
                    AddBuff(Enumerators.BuffType.ATTACK);
                    break;
                case Enumerators.BuffType.DAMAGE:
                    break;
                case Enumerators.BuffType.DEFENCE:
                    CurrentDefense++;
                    BuffedDefense++;
                    break;
                case Enumerators.BuffType.FREEZE:
                    TakeFreezeToAttacked = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Freeze);
                    break;
                case Enumerators.BuffType.HEAVY:
                    HasBuffHeavy = true;
                    break;
                case Enumerators.BuffType.BLITZ:
                    if (NumTurnsOnBoard == 0 && !HasFeral)
                    {
                        AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Blitz);
                        HasBuffRush = true;
                    }
                    break;
                case Enumerators.BuffType.GUARD:
                    HasBuffShield = true;
                    break;
                case Enumerators.BuffType.REANIMATE:
                    if (!GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Reanimate))
                    {
                        _abilitiesController.BuffUnitByAbility(
                            Enumerators.AbilityType.REANIMATE_UNIT,
                            this,
                            Card.Prototype.Kind,
                            this,
                            OwnerPlayer
                            );
                    }
                    break;
                case Enumerators.BuffType.DESTROY:
                    if (!GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Destroy))
                    {
                        _abilitiesController.BuffUnitByAbility(
                        Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        this,
                        Card.Prototype.Kind,
                        this,
                        OwnerPlayer
                        );
                    }
                    break;
            }

            BuffApplied?.Invoke(type);

            UpdateCardType();
        }

        public void UseShieldFromBuff()
        {
            if (!HasBuffShield)
                return;

            HasBuffShield = false;
            BuffsOnUnit.Remove(Enumerators.BuffType.GUARD);
            BuffShieldStateChanged?.Invoke(false);

            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Guard);
        }

        public void AddBuffShield()
        {
            AddBuff(Enumerators.BuffType.GUARD);
            HasBuffShield = true;
            BuffShieldStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Guard);
        }

        public void AddBuffSwing()
        {
            HasSwing = true;
            BuffSwingStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.SwingX);
        }

        public void UpdateCardType()
        {
            if (HasBuffHeavy)
            {
                SetAsHeavyUnit();
            }
            else
            {
                switch (InitialUnitType)
                {
                    case Enumerators.CardType.WALKER:
                        SetAsWalkerUnit();
                        break;
                    case Enumerators.CardType.FERAL:
                        SetAsFeralUnit();
                        break;
                    case Enumerators.CardType.HEAVY:
                        SetAsHeavyUnit();
                        break;
                }
            }
        }

        private void ClearUnitTypeEffects()
        {
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Heavy);
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Feral);
        }

        public void SetAsHeavyUnit()
        {
            if (HasHeavy)
                return;

            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Heavy);

            HasHeavy = true;
            HasFeral = false;
            InitialUnitType = Enumerators.CardType.HEAVY;
            CardTypeChanged?.Invoke(InitialUnitType);

            if (!AttackedThisTurn && NumTurnsOnBoard == 0)
            {
                IsPlayable = false;
            }
        }

        public void SetAsWalkerUnit()
        {
            if (!HasHeavy && !HasFeral && !HasBuffHeavy)
                return;

            ClearUnitTypeEffects();

            HasHeavy = false;
            HasFeral = false;
            HasBuffHeavy = false;
            InitialUnitType = Enumerators.CardType.WALKER;

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void SetAsFeralUnit()
        {
            if (HasFeral)
                return;

            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Feral);

            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = true;
            InitialUnitType = Enumerators.CardType.FERAL;

            if (!AttackedThisTurn && !IsPlayable)
            {
                IsPlayable = true;
            }

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void SetInitialUnitType()
        {
            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = false;

            ClearUnitTypeEffects();

            InitialUnitType = Card.Prototype.Type;

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription gameMechanic)
        {
            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardWithAbilityPlayed, this, gameMechanic.ToString());

            if (!GameMechanicDescriptionsOnUnit.Contains(gameMechanic))
            {
                GameMechanicDescriptionsOnUnit.Add(gameMechanic);
                GameMechanicDescriptionsOnUnitChanged?.Invoke();
            }
        }

        public void RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription gameMechanic)
        {
            if (GameMechanicDescriptionsOnUnit.Contains(gameMechanic))
            {
                GameMechanicDescriptionsOnUnit.Remove(gameMechanic);
                GameMechanicDescriptionsOnUnitChanged?.Invoke();
            }
        }

        public void ClearEffectsOnUnit()
        {
            GameMechanicDescriptionsOnUnit.Clear();

            GameMechanicDescriptionsOnUnitChanged?.Invoke();
        }

        private void SetObjectInfo(WorkingCard card)
        {
            Card = card;

            CurrentDamage = card.Prototype.Damage;
            CurrentDefense = card.Prototype.Defense;

            BuffedDamage = 0;
            BuffedDefense = 0;

            InitialUnitType = Card.Prototype.Type;

            LastAttackingSetType = Faction;

            ClearUnitTypeEffects();

            SetPicture();
        }

        public void OnStartTurn()
        {
            AttackedBoardObjectsThisTurn.Clear();
            NumTurnsOnBoard++;

            if (_stunTurns > 0)
            {
                _stunTurns--;
            }

            if (_stunTurns == 0)
            {
                IsPlayable = true;
                UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            }

            if (OwnerPlayer != null && _gameplayManager.CurrentTurnPlayer == OwnerPlayer)
            {
                if (IsPlayable)
                {
                    AttackedThisTurn = false;

                    IsCreatedThisTurn = false;
                }

                // RANK buff attack should be removed at next player turn
                if (BuffsOnUnit != null)
                {
                    int attackToRemove = BuffsOnUnit.FindAll(x => x == Enumerators.BuffType.ATTACK).Count;

                    if (attackToRemove > 0)
                    {
                        BuffsOnUnit.RemoveAll(x => x == Enumerators.BuffType.ATTACK);

                        BuffedDamage -= attackToRemove;
                        CurrentDamage -= attackToRemove;
                    }
                }
            }

            TurnStarted?.Invoke();
        }

        public void OnEndTurn()
        {
            IsPlayable = false;
            if(HasBuffRush)
            {
                RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Blitz);
                HasBuffRush = false;
            }
            CantAttackInThisTurnBlocker = false;
            TurnEnded?.Invoke();
        }

        public void Stun(Enumerators.StunType stunType, int turns)
        {
            if (AttackedThisTurn || NumTurnsOnBoard == 0 || _gameplayManager.CurrentTurnPlayer != OwnerPlayer)
                turns++;

            if (turns > _stunTurns)
            {
                _stunTurns = turns;
            }

            IsPlayable = false;

            UnitSpecialStatus = Enumerators.UnitSpecialStatus.FROZEN;

            Stunned?.Invoke(true);
        }

        public void RevertStun()
        {
            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            _stunTurns = 0;
            Stunned?.Invoke(false);
        }

        public void Distract()
        {
            WasDistracted = true;

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Distract);

            UpdateVisualStateOfDistract(true);
            UnitDistracted?.Invoke();
        }

        public void DisableDistract()
        {
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Distract);

            UpdateVisualStateOfDistract(false);
        }

        public void SetAgileStatus(bool status)
        {
            AgileEnabled = status;
        }

        public void SetMaximumDamageToUnit(int maximumDamage)
        {
            MaximumDamageFromAnySource = maximumDamage;
        }

        public void UpdateVisualStateOfDistract(bool status)
        {
            UnitDistractEffectStateChanged?.Invoke(status);
        }

        public void ForceSetCreaturePlayable()
        {
            if (IsStun)
                return;

            IsPlayable = true;
            CreaturePlayableForceSet?.Invoke();
        }

        public bool CanBePlayed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                return _playerController.IsActive; // && owner.manaStat.effectiveValue >= manaCost;
            }
            else
            {
                return true;
            }
        }

        public bool CanBeBuyed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                if (_gameplayManager.AvoidGooCost)
                    return true;

                return owner.CurrentGoo >= Card.InstanceCard.Cost;
            }
            else
            {
                return true;
            }
        }

        public void DoCombat(IBoardObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            IsAttacking = true;

            switch (target)
            {
                case Player targetPlayer:
                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            if (targetPlayer.Defense <= 0)
                            {
                                IsPlayable = true;
                                AttackedThisTurn = false;
                                IsAttacking = false;
                                completeCallback?.Invoke();
                                return;
                            }


                            if (_gameplayManager.IsTutorial &&
                                !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                                SpecificBattlegroundInfo.DisabledInitialization && OwnerPlayer.IsLocalPlayer)
                            {
                                if (!_tutorialManager.GetCurrentTurnInfo().UseBattleframesSequence.Exists(info => info.TutorialObjectId == TutorialObjectId &&
                                 info.Target == Enumerators.SkillTarget.OPPONENT))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                                    IsPlayable = true;
                                    AttackedThisTurn = false;
                                    IsAttacking = false;
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }

                            if (!AttackedBoardObjectsThisTurn.Contains(targetPlayer))
                            {
                                AttackedBoardObjectsThisTurn.Add(targetPlayer);
                            }

                            FightSequenceHandler.HandleAttackPlayer(
                                completeCallback,
                                targetPlayer,
                                () =>
                                {
                                    if(!_pvpManager.UseBackendGameLogic)
                                        _battleController.AttackPlayerByUnit(this, targetPlayer);
                                },
                                () =>
                                {
                                    IsAttacking = false;
                                    UnitAttackedEnded?.Invoke();
                                }
                            );
                        }, Enumerators.QueueActionType.UnitCombat);
                    break;
                case CardModel targetCardModel:

                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            if(targetCardModel.CurrentDefense <= 0 || targetCardModel.IsDead)
                            {
                                IsPlayable = true;
                                AttackedThisTurn = false;
                                IsAttacking = false;
                                completeCallback?.Invoke();
                                return;
                            }

                            if (_tutorialManager.IsTutorial && OwnerPlayer.IsLocalPlayer)
                            {
                                if (_tutorialManager.GetCurrentTurnInfo() != null &&
                                    !_tutorialManager.GetCurrentTurnInfo().UseBattleframesSequence.Exists(info =>
                                     info.TutorialObjectId == TutorialObjectId &&
                                     (info.TargetTutorialObjectId == targetCardModel.TutorialObjectId ||
                                         info.TargetTutorialObjectId == 0 && info.Target != Enumerators.SkillTarget.OPPONENT)))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                                    IsPlayable = true;
                                    AttackedThisTurn = false;
                                    IsAttacking = false;
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }

                            ActionForDying = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.UnitDeath, blockQueue: true);
                            targetCardModel.ActionForDying = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.UnitDeath, blockQueue: true);

                            if (!AttackedBoardObjectsThisTurn.Contains(targetCardModel))
                            {
                                AttackedBoardObjectsThisTurn.Add(targetCardModel);
                            }

                            FightSequenceHandler.HandleAttackCard(
                                completeCallback,
                                targetCardModel,
                                () =>
                                {
                                    _battleController.AttackUnitByUnit(this, targetCardModel, AdditionalDamage);

                                    if (HasSwing)
                                    {
                                        List<CardModel> adjacent = _battlegroundController.GetAdjacentUnitsToUnit(targetCardModel);

                                        foreach (CardModel unit in adjacent)
                                        {
                                            _battleController.AttackUnitByUnit(this, unit, AdditionalDamage, false);
                                        }
                                    }

                                    if (TakeFreezeToAttacked && targetCardModel.CurrentDefense > 0)
                                    {
                                        targetCardModel.Stun(Enumerators.StunType.FREEZE, 1);
                                        targetCardModel.UseShieldFromBuff();
                                    }

                                    targetCardModel.ResolveBuffShield();
                                    this.ResolveBuffShield();
                                },
                                () =>
                                {
                                    IsAttacking = false;
                                    UnitAttackedEnded?.Invoke();
                                }
                                );
                        }, Enumerators.QueueActionType.UnitCombat);
                    break;
                default:
                    throw new NotSupportedException(target.GetType().ToString());
            }
        }

        public bool UnitCanBeUsable()
        {
            if (IsDead ||
                CurrentDefense <= 0 ||
                CurrentDamage <= 0 ||
                IsStun ||
                CantAttackInThisTurnBlocker ||
                !CanAttackByDefault || !IsUnitActive)
            {
                return false;
            }

            if (IsPlayable)
            {
                if (HasFeral)
                {
                    return true;
                }

                if (NumTurnsOnBoard >= 1)
                {
                    return true;
                }
            }
            else if (!AttackedThisTurn && HasBuffRush)
            {
                return true;
            }

            return false;
        }

        public void MoveUnitFromBoardToDeck()
        {
            try
            {
                Die(true);

                RemoveUnitFromBoard();
            }
            catch (Exception ex)
            {
                Helpers.ExceptionReporter.SilentReportException(ex);
                Log.Warn(ex.ToString());
            }
        }

        public void InvokeUnitDamaged(IBoardObject from)
        {
            UnitDamaged?.Invoke(from);
        }

        public void InvokeUnitAttacked(IBoardObject target, int damage, bool isAttacker)
        {
            UnitAttacked?.Invoke(target, damage, isAttacker);
        }

        public void InvokeUnitDied()
        {
            UnitDied?.Invoke();
        }

        public void InvokeKilledUnit(CardModel card)
        {
            KilledUnit?.Invoke(card);
        }

        public IReadOnlyList<CardModel> GetEnemyUnitsList(CardModel unit)
        {
            if (_gameplayManager.CurrentPlayer.CardsOnBoard.Contains(unit))
            {
                return _gameplayManager.OpponentPlayer.CardsOnBoard;
            }

            return _gameplayManager.CurrentPlayer.CardsOnBoard;
        }

        public void RemoveUnitFromBoard()
        {
            _battlegroundController.UnregisterCardView(_battlegroundController.GetCardViewByModel<BoardUnitView>(this), this.OwnerPlayer);
            OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(this);
            OwnerPlayer.PlayerCardsController.AddCardToGraveyard(this);

            UnitFromDeckRemoved?.Invoke();
        }

        public void InvokeUnitPrepairingToDie()
        {
            PrepairingToDie?.Invoke(this);
        }

        public void SetPicture(string name = "", string attribute = "")
        {
            string imagePath = $"{Constants.PathToCardsIllustrations}";

            if (!string.IsNullOrEmpty(name))
            {
                imagePath += $"{name}";
            }
            else
            {
                imagePath += $"{Prototype.Picture.ToLowerInvariant()}";
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                imagePath += $"_{attribute}";
            }

            CardPicture = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);
            CardPictureWasUpdated?.Invoke();

            Resources.UnloadUnusedAssets();
        }

        public void ArriveUnitOnBoard()
        {
            switch (InitialUnitType)
            {
                case Enumerators.CardType.FERAL:
                    HasFeral = true;
                    IsPlayable = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Feral);
                    break;
                case Enumerators.CardType.HEAVY:
                    HasHeavy = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Heavy);
                    break;
                case Enumerators.CardType.WALKER:
                default:
                    break;
            }

            if (Card.InstanceCard.Abilities != null)
            {
                foreach (AbilityData ability in Card.InstanceCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = GameClient.Get<IDataManager>().GetGameMechanicInfo(ability.GameMechanicDescription);

                    if (gameMechanicInfo != null && !string.IsNullOrEmpty(gameMechanicInfo.Name))
                    {
                        AddGameMechanicDescriptionOnUnit(ability.GameMechanicDescription);
                    }
                }
            }
        }

        public void ResetToInitial()
        {
            Card.InstanceCard.CopyFrom(Card.Prototype);
            InitialUnitType = Card.Prototype.Type;
            BuffedDefense = 0;
            BuffedDamage = 0;
            NumTurnsOnBoard = 0;
            _stunTurns = 0;
            HpDebuffUntillEndOfTurn = 0;
            DamageDebuffUntillEndOfTurn = 0;
            WasDistracted = false;
            IsCreatedThisTurn = true;
            CanAttackByDefault = true;
            TakeFreezeToAttacked = false;
            HasSwing = false;
            AttackedThisTurn = false;
            HasBuffRush = false;
            HasBuffHeavy = false;
            HasFeral = false;
            IsPlayable = false;
            IsAttacking = false;
            IsDead = false;
            AttackAsFirst = false;
            IsUnitActive = true;
            CantAttackInThisTurnBlocker = false;
            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            AttackRestriction = Enumerators.AttackRestriction.ANY;
            LastAttackingSetType = Card.Prototype.Faction;
            BuffsOnUnit.Clear();
            AttackedBoardObjectsThisTurn.Clear();
            UseShieldFromBuff();
            ClearUnitTypeEffects();
            MaximumDamageFromAnySource = 999;
        }

        public override string ToString()
        {
            return $"([{nameof(CardModel)}] {nameof(OwnerPlayer)}: {OwnerPlayer}, {nameof(Card)}: {Card}, {nameof(Faction)}: {Faction})";
        }
    }
}

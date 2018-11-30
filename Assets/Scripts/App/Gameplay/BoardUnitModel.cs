using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardUnitModel : OwnableBoardObject
    {
        public bool AttackedThisTurn;

        public bool HasFeral;

        public bool HasHeavy;

        public int NumTurnsOnBoard;

        public int InitialDamage;

        public int InitialHp;

        public bool HasUsedBuffShield;

        public bool HasSwing;

        public bool CanAttackByDefault;

        public List<BoardObject> AttackedBoardObjectsThisTurn;

        public Enumerators.AttackRestriction AttackRestriction = Enumerators.AttackRestriction.NONE;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly BattlegroundController _battlegroundController;

        private readonly BattleController _battleController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly AbilitiesController _abilitiesController;

        private int _currentDamage;

        private int _currentHealth;

        private int _stunTurns;

        public bool IsDead { get; private set; }

        public List<Enumerators.SkillTargetType> AttackTargetsAvailability;

        public BoardUnitModel()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            BuffsOnUnit = new List<Enumerators.BuffType>();
            AttackedBoardObjectsThisTurn = new List<BoardObject>();

            IsCreatedThisTurn = true;

            CanAttackByDefault = true;

            UnitStatus = Enumerators.UnitStatusType.NONE;

            AttackTargetsAvailability = new List<Enumerators.SkillTargetType>()
            {
                Enumerators.SkillTargetType.OPPONENT,
                Enumerators.SkillTargetType.OPPONENT_CARD
            };

            IsAllAbilitiesResolvedAtStart = true;

            _gameplayManager.CanDoDragActions = false;
        }

        public event Action TurnStarted;

        public event Action TurnEnded;

        public event Action<bool> Stunned;

        public event Action UnitDied;

        public event Action UnitDying;

        public event Action<BoardObject, int, bool> UnitAttacked;

        public event Action UnitAttackedEnded;

        public event Action<BoardObject> UnitDamaged;

        public event Action<BoardObject> PrepairingToDie;

        public event Action UnitHpChanged;

        public event Action UnitDamageChanged;

        public event Action<Enumerators.CardType> CardTypeChanged;

        public event Action<Enumerators.BuffType> BuffApplied;

        public event Action<bool> BuffShieldStateChanged;

        public event Action CreaturePlayableForceSet;

        public event Action UnitFromDeckRemoved;

        public event Action UnitDistracted;

        public event Action<bool> UnitDistractEffectStateChanged;

        public event Action<BoardUnitModel> KilledUnit;

        public event Action<bool> BuffSwingStateChanged;

        public event Action GameMechanicDescriptionsOnUnitChanged;

        public Enumerators.CardType InitialUnitType { get; private set; }

        public int MaxCurrentDamage => InitialDamage + BuffedDamage;

        public int BuffedDamage { get; set; }

        public int CurrentDamage
        {
            get => _currentDamage;
            set
            {
                _currentDamage = Mathf.Clamp(value, 0, 99999);
                UnitDamageChanged?.Invoke();
            }
        }

        public int MaxCurrentHp => InitialHp + BuffedHp;

        public int BuffedHp { get; set; }

        public int CurrentHp
        {
            get => _currentHealth;
            set
            {
                _currentHealth = Mathf.Clamp(value, 0, 99);
                UnitHpChanged?.Invoke();
            }
        }

        public bool IsPlayable { get; set; }

        public WorkingCard Card { get; private set; }

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

        public Enumerators.UnitStatusType UnitStatus { get; set; }

        public Enumerators.SetType LastAttackingSetType { get; set; }

        public bool CantAttackInThisTurnBlocker { get; set; } = false;

        public IFightSequenceHandler FightSequenceHandler;

        public bool IsHeavyUnit => HasBuffHeavy || HasHeavy;

        public List<Enumerators.GameMechanicDescriptionType> GameMechanicDescriptionsOnUnit { get; private set; } = new List<Enumerators.GameMechanicDescriptionType>();

        public void Die(bool returnToHand = false)
        {
            UnitDying?.Invoke();

            IsDead = true;
            if (!returnToHand)
            {
                _battlegroundController.KillBoardCard(this);
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
            BuffsOnUnit.Add(type);
        }

        public void ApplyBuff(Enumerators.BuffType type)
        {
            switch (type)
            {
                case Enumerators.BuffType.ATTACK:
                    CurrentDamage++;
                    break;
                case Enumerators.BuffType.DAMAGE:
                    break;
                case Enumerators.BuffType.DEFENCE:
                    CurrentHp++;
                    break;
                case Enumerators.BuffType.FREEZE:
                    TakeFreezeToAttacked = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Freeze);
                    break;
                case Enumerators.BuffType.HEAVY:
                    HasBuffHeavy = true;
                    break;
                case Enumerators.BuffType.RUSH:
                    if (NumTurnsOnBoard == 0)
                    {
                        HasBuffRush = true;
                    }
                    break;
                case Enumerators.BuffType.GUARD:
                    HasBuffShield = true;
                    break;
                case Enumerators.BuffType.REANIMATE:
                    _abilitiesController.BuffUnitByAbility(
                        Enumerators.AbilityType.REANIMATE_UNIT,
                        this,
                        Card.LibraryCard.CardKind,
                        Card.LibraryCard,
                        OwnerPlayer
                        );
                    break;
                case Enumerators.BuffType.DESTROY:
                    _abilitiesController.BuffUnitByAbility(
                        Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        this,
                        Card.LibraryCard.CardKind,
                        Card.LibraryCard,
                        OwnerPlayer
                        );
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

            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType.Guard);
        }

        public void AddBuffShield()
        {
            AddBuff(Enumerators.BuffType.GUARD);
            HasBuffShield = true;
            BuffShieldStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Guard);
        }

        public void AddBuffSwing()
        {
            HasSwing = true;
            BuffSwingStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.SwingX);
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
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType.Heavy);
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType.Feral);
        }

        public void SetAsHeavyUnit()
        {
            if (HasHeavy)
                return;

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Heavy);

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

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Feral);

            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = true;
            InitialUnitType = Enumerators.CardType.FERAL;

            CardTypeChanged?.Invoke(InitialUnitType);

            if (!AttackedThisTurn && !IsPlayable)
            {
                IsPlayable = true;
            }
        }

        public void SetInitialUnitType()
        {
            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = false;

            ClearUnitTypeEffects();

            InitialUnitType = Card.LibraryCard.CardType;

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType gameMechanic)
        {
            if (!GameMechanicDescriptionsOnUnit.Contains(gameMechanic))
            {
                GameMechanicDescriptionsOnUnit.Add(gameMechanic);
                GameMechanicDescriptionsOnUnitChanged?.Invoke();
            }
        }

        public void RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType gameMechanic)
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

        public void SetObjectInfo(WorkingCard card)
        {
            Card = card;

            CurrentDamage = card.InstanceCard.Damage;
            CurrentHp = card.InstanceCard.Health;

            BuffedDamage = 0;
            BuffedHp = 0;

            InitialDamage = CurrentDamage;
            InitialHp = CurrentHp;

            InitialUnitType = Card.LibraryCard.CardType;

            ClearUnitTypeEffects();

            switch (InitialUnitType)
            {
                case Enumerators.CardType.FERAL:
                    HasFeral = true;
                    IsPlayable = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Feral);
                    break;
                case Enumerators.CardType.HEAVY:
                    HasHeavy = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Heavy);
                    break;
                case Enumerators.CardType.WALKER:
                default:
                    break;
            }

            if (Card.LibraryCard.Abilities != null)
            {
                foreach (AbilityData ability in Card.LibraryCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = GameClient.Get<IDataManager>().GetGameMechanicInfo(ability.GameMechanicDescriptionType);

                    if (gameMechanicInfo != null && !string.IsNullOrEmpty(gameMechanicInfo.Name))
                    {
                        AddGameMechanicDescriptionOnUnit(ability.GameMechanicDescriptionType);
                    }
                }
            }
        }

        public void OnStartTurn()
        {
            Debug.Log("OnStartTurn");
            AttackedBoardObjectsThisTurn.Clear();
            NumTurnsOnBoard++;

            if (_stunTurns > 0)
            {
                _stunTurns--;
            }

            if (_stunTurns == 0)
            {
                IsPlayable = true;
                UnitStatus = Enumerators.UnitStatusType.NONE;
            }

            if (OwnerPlayer != null && IsPlayable && _gameplayManager.CurrentTurnPlayer.Equals(OwnerPlayer))
            {
                AttackedThisTurn = false;

                IsCreatedThisTurn = false;
            }

            TurnStarted?.Invoke();
        }

        public void OnEndTurn()
        {
            HasBuffRush = false;
            CantAttackInThisTurnBlocker = false;
            TurnEnded?.Invoke();
        }

        public void Stun(Enumerators.StunType stunType, int turns)
        {
            if (AttackedThisTurn || NumTurnsOnBoard == 0)
                turns++;

            if (turns > _stunTurns)
            {
                _stunTurns = turns;
            }

            IsPlayable = false;

            UnitStatus = Enumerators.UnitStatusType.FROZEN;

            Stunned?.Invoke(true);
        }

        public void RevertStun()
        {
            UnitStatus = Enumerators.UnitStatusType.NONE;
            _stunTurns = 0;
            Stunned?.Invoke(false);
        }

        public void Distract()
        {
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Distract);

            UpdateVisualStateOfDistract(true);
            UnitDistracted?.Invoke();
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

        public void DoCombat(BoardObject target)
        {
            if (target == null)
                return;

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

                            AttackedBoardObjectsThisTurn.Add(targetPlayer);

                            FightSequenceHandler.HandleAttackPlayer(
                                completeCallback,
                                targetPlayer,
                                () =>
                                {
                                    _battleController.AttackPlayerByUnit(this, targetPlayer);
                                },
                                () =>
                                {
                                    IsAttacking = false;
                                    UnitAttackedEnded?.Invoke();
                                }
                            );
                        });
                    break;
                case BoardUnitModel targetCardModel:
                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            if(targetCardModel.CurrentHp <= 0)
                            {
                                IsPlayable = true;
                                AttackedThisTurn = false;
                                IsAttacking = false;
                                completeCallback?.Invoke();
                                return;
                            }

                            AttackedBoardObjectsThisTurn.Add(targetCardModel);
                            FightSequenceHandler.HandleAttackCard(
                                completeCallback,
                                targetCardModel,
                                () =>
                                {
                                    _battleController.AttackUnitByUnit(this, targetCardModel, AdditionalDamage);

                                    if (HasSwing)
                                    {
                                        List<BoardUnitView> adjacent = _battlegroundController.GetAdjacentUnitsToUnit(targetCardModel);

                                        foreach (BoardUnitView unit in adjacent)
                                        {
                                            _battleController.AttackUnitByUnit(this, unit.Model, AdditionalDamage, false);
                                        }
                                    }

                                    if (TakeFreezeToAttacked && targetCardModel.CurrentHp > 0)
                                    {
                                        if (!targetCardModel.HasBuffShield)
                                        {
                                            targetCardModel.Stun(Enumerators.StunType.FREEZE, 1);
                                        } else {
                                            targetCardModel.HasUsedBuffShield = true;
                                        }
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
                        });
                    break;
                default:
                    throw new NotSupportedException(target.GetType().ToString());
            }
        }

        public bool UnitCanBeUsable()
        {
            if (IsDead || CurrentHp <= 0 ||
                CurrentDamage <= 0 || IsStun ||
                CantAttackInThisTurnBlocker  || !CanAttackByDefault)
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
                Debug.LogError(ex.Message);
            }
        }

        public void InvokeUnitDamaged(BoardObject from)
        {
            UnitDamaged?.Invoke(from);
        }

        public void InvokeUnitAttacked(BoardObject target, int damage, bool isAttacker)
        {
            UnitAttacked?.Invoke(target, damage, isAttacker);
        }

        public void InvokeUnitDied()
        {
            UnitDied?.Invoke();
        }

        public void InvokeKilledUnit(BoardUnitModel boardUnit)
        {
            KilledUnit?.Invoke(boardUnit);
        }

        public List<BoardUnitView> GetEnemyUnitsList(BoardUnitModel unit)
        {
            if (_gameplayManager.CurrentPlayer.BoardCards.Select(x => x.Model).Contains(unit))
            {
                return _gameplayManager.OpponentPlayer.BoardCards;
            }

            return _gameplayManager.CurrentPlayer.BoardCards;
        }

        public void RemoveUnitFromBoard()
        {
            OwnerPlayer.BoardCards.Remove(_battlegroundController.GetBoardUnitViewByModel(this));
            OwnerPlayer.RemoveCardFromBoard(Card);
            OwnerPlayer.AddCardToGraveyard(Card);

            UnitFromDeckRemoved?.Invoke();
        }

        public void InvokeUnitPrepairingToDie()
        {
            PrepairingToDie?.Invoke(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AbilitiesController : IController
    {
        private readonly object _lock = new object();

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private CardsController _cardsController;

        private PlayerController _playerController;

        private BattlegroundController _battlegroundController;

        private ActionsQueueController _actionsQueueController;

        private ulong _castedAbilitiesIds;

        private List<ActiveAbility> _activeAbilities;

        public void Init()
        {
            _activeAbilities = new List<ActiveAbility>();

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
        }

        public void ResetAll()
        {
            Reset();
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (ActiveAbility item in _activeAbilities)
                {
                    item.Ability.Update();
                }
            }
        }

        public void Dispose()
        {
            Reset();
        }

        public static uint[] AbilityTypeToUintArray(List<Enumerators.AbilityType> abilities)
        {
            uint[] abils = new uint[abilities.Count];
            for (int i = 0; i < abilities.Count; i++)
            {
                abils[i] = (uint)abilities[i];
            }

            return abils;
        }

        public static List<Enumerators.AbilityType> AbilityTypeToUintArray(uint[] abilities)
        {
            List<Enumerators.AbilityType> abils = new List<Enumerators.AbilityType>();
            for (int i = 0; i < abilities.Length; i++)
            {
                abils[i] = (Enumerators.AbilityType)abilities[i];
            }

            return abils;
        }

        public void Reset()
        {
            lock (_lock)
            {
                foreach (ActiveAbility item in _activeAbilities)
                {
                    item.Ability.Dispose();
                }

                _activeAbilities.Clear();
            }

            _castedAbilitiesIds = 0;
        }

        public void DeactivateAbility(ulong id)
        {
            lock (_lock)
            {
                ActiveAbility item = _activeAbilities.Find(x => x.Id == id);
                if (_activeAbilities.Contains(item))
                {
                    _activeAbilities.Remove(item);
                }

                if ((item != null) && (item.Ability != null))
                {
                    item.Ability.Dispose();
                }
            }
        }

        public List<AbilityBase> GetAbilitiesConnectedToUnit(BoardUnit unit)
        {
            return _activeAbilities.FindAll(x => x.Ability.TargetUnit == unit).Select(y => y.Ability).ToList();
        }

        public ActiveAbility CreateActiveAbility(
            AbilityData ability,
            Enumerators.CardKind kind,
            object boardObject,
            Player caller,
            Card cardOwner,
            WorkingCard workingCard)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility
                {
                    Id = _castedAbilitiesIds++,
                    Ability = CreateAbilityByType(kind, ability)
                };

                activeAbility.Ability.ActivityId = activeAbility.Id;
                activeAbility.Ability.PlayerCallerOfAbility = caller;
                activeAbility.Ability.CardOwnerOfAbility = cardOwner;
                activeAbility.Ability.MainWorkingCard = workingCard;

                if (boardObject != null)
                {
                    if (boardObject is BoardCard)
                    {
                        activeAbility.Ability.BoardCard = boardObject as BoardCard;
                    }
                    else
                    {
                        if (kind == Enumerators.CardKind.Creature)
                        {
                            activeAbility.Ability.AbilityUnitOwner = boardObject as BoardUnit;
                        }
                        else
                        {
                            activeAbility.Ability.BoardSpell = boardObject as BoardSpell;
                        }
                    }
                }

                _activeAbilities.Add(activeAbility);

                return activeAbility;
            }
        }

        public bool HasTargets(AbilityData ability)
        {
            if (ability.AbilityTargetTypes.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityActive(AbilityData ability)
        {
            if (ability.AbilityActivityType == Enumerators.AbilityActivityType.Active)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCallsAtStart(AbilityData ability)
        {
            if (ability.AbilityCallType == Enumerators.AbilityCallType.Entry)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCanActivateTargetAtStart(AbilityData ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && IsAbilityActive(ability))
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCanActivateWithoutTargetAtStart(AbilityData ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && !IsAbilityActive(ability))
            {
                return true;
            }

            return false;
        }

        public bool CheckActivateAvailability(Enumerators.CardKind kind, AbilityData ability, Player localPlayer)
        {
            bool available = false;

            Player opponent = localPlayer.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;

            lock (_lock)
            {
                foreach (Enumerators.AbilityTargetType item in ability.AbilityTargetTypes)
                {
                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OpponentCard:
                        {
                            if (opponent.BoardCards.Count > 0)
                            {
                                available = true;
                            }
                        }

                            break;
                        case Enumerators.AbilityTargetType.PlayerCard:
                        {
                            if ((localPlayer.BoardCards.Count > 1) || (kind == Enumerators.CardKind.Spell))
                            {
                                available = true;
                            }
                        }

                            break;
                        case Enumerators.AbilityTargetType.Player:
                        case Enumerators.AbilityTargetType.Opponent:
                        case Enumerators.AbilityTargetType.All:
                            available = true;
                            break;
                    }
                }
            }

            return available;
        }

        public int GetStatModificatorByAbility(BoardUnit attacker, BoardUnit attacked, bool isAttackking)
        {
            int value = 0;

            Card attackedCard = attacked.Card.LibraryCard;
            Card attackerCard = attacker.Card.LibraryCard;

            List<AbilityData> abilities = null;

            if (isAttackking)
            {
                abilities = attackerCard.Abilities.FindAll(x => x.AbilityType == Enumerators.AbilityType.ModificatorStats);

                for (int i = 0; i < abilities.Count; i++)
                {
                    if (attackedCard.CardSetType == abilities[i].AbilitySetType)
                    {
                        value += abilities[i].Value;
                    }
                }
            }

            abilities = attackerCard.Abilities.FindAll(x => x.AbilityType == Enumerators.AbilityType.AdditionalDamageToHeavyInAttack);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (attacked.IsHeavyUnit())
                {
                    value += abilities[i].Value;
                }
            }

            return value;
        }

        public bool HasSpecialUnitOnBoard(WorkingCard workingCard, AbilityData ability)
        {
            if (ability.AbilityTargetTypes.Count == 0)
            {
                return false;
            }

            Player opponent = workingCard.Owner.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
            Player player = workingCard.Owner;

            foreach (Enumerators.AbilityTargetType target in ability.AbilityTargetTypes)
            {
                if (target.Equals(Enumerators.AbilityTargetType.PlayerCard))
                {
                    List<BoardUnit> units = player.BoardCards.FindAll(x => (x.InitialUnitType == ability.TargetCardType) && (x.UnitStatus == ability.TargetUnitStatusType));

                    if (units.Count > 0)
                    {
                        return true;
                    }
                }
                else if (target.Equals(Enumerators.AbilityTargetType.OpponentCard))
                {
                    List<BoardUnit> units = opponent.BoardCards.FindAll(x => (x.InitialUnitType == ability.TargetCardType) && (x.UnitStatus == ability.TargetUnitStatusType));

                    if (units.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasSpecialUnitStatusOnBoard(WorkingCard workingCard, AbilityData ability)
        {
            if (ability.AbilityTargetTypes.Count == 0)
            {
                return false;
            }

            Player opponent = workingCard.Owner.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
            Player player = workingCard.Owner;

            foreach (Enumerators.AbilityTargetType target in ability.AbilityTargetTypes)
            {
                if (target.Equals(Enumerators.AbilityTargetType.PlayerCard))
                {
                    List<BoardUnit> units = player.BoardCards.FindAll(x => x.UnitStatus == ability.TargetUnitStatusType);

                    if (units.Count > 0)
                    {
                        return true;
                    }
                }
                else if (target.Equals(Enumerators.AbilityTargetType.OpponentCard))
                {
                    List<BoardUnit> units = opponent.BoardCards.FindAll(x => x.UnitStatus == ability.TargetUnitStatusType);

                    if (units.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void CallAbility(
            Card libraryCard,
            BoardCard card,
            WorkingCard workingCard,
            Enumerators.CardKind kind,
            object boardObject,
            Action<BoardCard> action,
            bool isPlayer,
            Action onCompleteCallback,
            object target = null,
            HandBoardCard handCard = null)
        {
            ResolveAllAbilitiesOnUnit(boardObject, false);

            Vector3 postionOfCardView = Vector3.zero;

            if ((card != null) && (card.GameObject != null))
            {
                postionOfCardView = card.Transform.position;
            }

            bool canUseAbility = false;
            ActiveAbility activeAbility = null;
            foreach (AbilityData item in libraryCard.Abilities)
            {
                // todo improve it bcoz can have queue of abilities with targets
                activeAbility = CreateActiveAbility(item, kind, boardObject, workingCard.Owner, libraryCard, workingCard);

                // Debug.Log(_abilitiesController.IsAbilityCanActivateTargetAtStart(item));
                if (IsAbilityCanActivateTargetAtStart(item))
                {
                    canUseAbility = true;
                }
                else
                {
                    // if (_abilitiesController.IsAbilityCanActivateWithoutTargetAtStart(item))
                    activeAbility.Ability.Activate();
                }
            }

            if (kind == Enumerators.CardKind.Spell)
            {
                // if (isPlayer)
                // currentSpellCard = card;
            }
            else
            {
                workingCard.Owner.RemoveCardFromHand(workingCard);
                workingCard.Owner.AddCardToBoard(workingCard);
            }

            if (kind == Enumerators.CardKind.Spell)
            {
                if ((handCard != null) && isPlayer)
                {
                    handCard.GameObject.SetActive(false);
                }
            }

            if (canUseAbility)
            {
                AbilityData ability = libraryCard.Abilities.Find(x => IsAbilityCanActivateTargetAtStart(x));

                if (((ability.TargetCardType != Enumerators.CardType.None) && !HasSpecialUnitOnBoard(workingCard, ability)) || ((ability.TargetUnitStatusType != Enumerators.UnitStatusType.None) && !HasSpecialUnitStatusOnBoard(workingCard, ability)))
                {
                    CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);

                    onCompleteCallback?.Invoke();

                    ResolveAllAbilitiesOnUnit(boardObject);

                    return;
                }

                if (CheckActivateAvailability(kind, ability, workingCard.Owner))
                {
                    activeAbility.Ability.Activate();

                    if (isPlayer)
                    {
                        activeAbility.Ability.ActivateSelectTarget(
                            callback: () =>
                            {
                                if ((kind == Enumerators.CardKind.Spell) && isPlayer)
                                {
                                    card.WorkingCard.Owner.Goo -= card.ManaCost;
                                    _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MoveCard);

                                    handCard.GameObject.SetActive(true);
                                    card.RemoveCardParticle.Play(); // move it when card should call hide action

                                    workingCard.Owner.RemoveCardFromHand(workingCard, true);
                                    workingCard.Owner.AddCardToBoard(workingCard);

                                    GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[] { card }, 0.5f, false);

                                    GameClient.Get<ITimerManager>().AddTimer(
                                        creat =>
                                        {
                                            workingCard.Owner.GraveyardCardsCount++;

                                            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PlaySpellCard, new object[] { workingCard.Owner, card }));
                                        },
                                        null,
                                        1.5f);
                                }

                                action?.Invoke(card);

                                onCompleteCallback?.Invoke();

                                ResolveAllAbilitiesOnUnit(boardObject);
                            },
                            failedCallback: () =>
                            {
                                if ((kind == Enumerators.CardKind.Spell) && isPlayer)
                                {
                                    handCard.GameObject.SetActive(true);
                                    handCard.ResetToHandAnimation();
                                    handCard.CheckStatusOfHighlight();

                                    workingCard.Owner.CardsInHand.Add(card.WorkingCard);
                                    _battlegroundController.PlayerHandCards.Add(card);
                                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                                    _playerController.IsCardSelected = false;

                                    // currentSpellCard = null;

                                    // GameClient.Get<IUIManager>().GetPage<GameplayPage>().SetEndTurnButtonStatus(true);
                                }
                                else
                                {
                                    Debug.Log("RETURN CARD TO HAND MAYBE.. SHOULD BE CASE !!!!!");
                                    action?.Invoke(card);
                                }

                                onCompleteCallback?.Invoke();

                                ResolveAllAbilitiesOnUnit(boardObject);
                            });
                    }
                    else
                    {
                        if (target is BoardUnit)
                        {
                            activeAbility.Ability.TargetUnit = target as BoardUnit;
                        }
                        else if (target is Player)
                        {
                            activeAbility.Ability.TargetPlayer = target as Player;
                        }

                        activeAbility.Ability.SelectedTargetAction(true);

                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();

                        // Debug.LogError(activeAbility.ability.abilityType.ToString() + " ABIITY WAS ACTIVATED!!!! on " + (target == null ? target : target.GetType()));
                        onCompleteCallback?.Invoke();

                        ResolveAllAbilitiesOnUnit(boardObject);
                    }
                }
                else
                {
                    CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                    onCompleteCallback?.Invoke();

                    ResolveAllAbilitiesOnUnit(boardObject);
                }
            }
            else
            {
                CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                onCompleteCallback?.Invoke();

                ResolveAllAbilitiesOnUnit(boardObject);
            }
        }

        public Player GetOpponentPlayer(AbilityBase ability)
        {
            return ability.PlayerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
        }

        public void BuffUnitByAbility(Enumerators.AbilityType ability, object target, Card card, Player owner)
        {
            ActiveAbility activeAbility = CreateActiveAbility(GetAbilityDataByType(ability), card.CardKind, target, owner, card, null);
            activeAbility.Ability.Activate();
        }

        public void CallAbilitiesInHand(BoardCard boardCard, WorkingCard card)
        {
            List<AbilityData> handAbilities = card.LibraryCard.Abilities.FindAll(x => x.AbilityCallType.Equals(Enumerators.AbilityCallType.InHand));
            foreach (AbilityData ability in handAbilities)
            {
                CreateActiveAbility(ability, card.LibraryCard.CardKind, boardCard, card.Owner, card.LibraryCard, card).Ability.Activate();
            }
        }

        private AbilityBase CreateAbilityByType(Enumerators.CardKind cardKind, AbilityData abilityData)
        {
            AbilityBase ability = null;
            switch (abilityData.AbilityType)
            {
                case Enumerators.AbilityType.Heal:
                    ability = new HealTargetAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DamageTarget:
                    ability = new DamageTargetAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DamageTargetAdjustments:
                    ability = new DamageTargetAdjustmentsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AddGooVial:
                    ability = new AddGooVialsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ModificatorStats:
                    ability = new ModificateStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.MassiveDamage:
                    ability = new MassiveDamageAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ChangeStat:
                    ability = new ChangeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Stun:
                    ability = new StunAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.StunOrDamageAdjustments:
                    ability = new StunOrDamageAdjustmentsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Summon:
                    ability = new SummonsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CardReturn:
                    ability = new ReturnToHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Weapon:
                    ability = new HeroWeaponAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ChangeStatOfCreaturesByType:
                    ability = new ChangeUnitsOfTypeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AttackNumberOfTimesPerTurn:
                    ability = new AttackNumberOfTimesPerTurnAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DrawCard:
                    ability = new DrawCardAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DevourZombiesAndCombineStats:
                    ability = new DevourZombiesAndCombineStatsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DestroyUnitByType:
                    ability = new DestroyUnitByTypeAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.LowerCostOfCardInHand:
                    ability = new LowerCostOfCardInHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.OverflowGoo:
                    ability = new OverflowGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.LoseGoo:
                    ability = new LoseGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DisableNextTurnGoo:
                    ability = new DisableNextTurnGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Rage:
                    ability = new RageAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FreezeUnits:
                    ability = new FreezeUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeDamageRandomEnemy:
                    ability = new TakeDamageRandomEnemyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeControlEnemyUnit:
                    ability = new TakeControlEnemyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Guard:
                    ability = new ShieldAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DestroyFrozenUnit:
                    ability = new DestroyFrozenZombieAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.UseAllGooToIncreaseStats:
                    ability = new UseAllGooToIncreaseStatsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FirstUnitInPlay:
                    ability = new FirstUnitInPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AllyUnitsOfTypeInPlayGetStats:
                    ability = new AllyUnitsOfTypeInPlayGetStatsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DamageEnemyUnitsAndFreezeThem:
                    ability = new DamageEnemyUnitsAndFreezeThemAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ReturnUnitsOnBoardToOwnersDecks:
                    ability = new ReturnUnitsOnBoardToOwnersDecksAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeUnitTypeToAdjacentAllyUnits:
                    ability = new TakeUnitTypeToAdjacentAllyUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.EnemyThatAttacksBecomeFrozen:
                    ability = new EnemyThatAttacksBecomeFrozenAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeUnitTypeToAllyUnit:
                    ability = new TakeUnitTypeToAllyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ReviveDiedUnitsOfTypeFromMatch:
                    ability = new ReviveDiedUnitsOfTypeFromMatchAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ChangeStatUntillEndOfTurn:
                    ability = new ChangeStatUntillEndOfTurnAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AttackOverlord:
                    ability = new AttackOverlordAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AdjacentUnitsGetHeavy:
                    ability = new AdjacentUnitsGetHeavyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FreezeNumberOfRandomAlly:
                    ability = new FreezeNumberOfRandomAllyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AddCardByNameToHand:
                    ability = new AddCardByNameToHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DealDamageToThisAndAdjacentUnits:
                    ability = new DealDamageToThisAndAdjacentUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.Swing:
                    ability = new SwingAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeDefenseIfOverlordHasLessDefenseThan:
                    ability = new TakeDefenseIfOverlordHasLessDefenseThanAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.AdditionalDamageToHeavyInAttack:
                    ability = new AdditionalDamageToHeavyInAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GainNumberOfLifeForEachDamageThisDeals:
                    ability = new GainNumberOfLifeForEachDamageThisDealsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.UnitWeapon:
                    ability = new UnitWeaponAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TakeDamageAtEndOfTurnToThis:
                    ability = new TakeDamageAtEndOfTurnToThis(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DelayedLoseHeavyGainAttack:
                    ability = new DelayedLoseHeavyGainAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DelayedGainAttack:
                    ability = new DelayedGainAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ReanimateUnit:
                    ability = new ReanimateAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.PriorityAttack:
                    ability = new PriorityAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DestroyTargetUnitAfterAttack:
                    ability = new DestroyTargetUnitAfterAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CostsLessIfCardTypeInHand:
                    ability = new CostsLessIfCardTypeInHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ReturnUnitsOnBoardToOwnersHands:
                    ability = new ReturnUnitsOnBoardToOwnersHandsAbility(cardKind, abilityData);
                    break;
            }

            return ability;
        }

        private void ResolveAllAbilitiesOnUnit(object boardObject, bool status = true)
        {
            if (boardObject != null)
            {
                if (boardObject is BoardUnit)
                {
                    (boardObject as BoardUnit).IsAllAbilitiesResolvedAtStart = status;
                }
            }
        }

        private void CallPermanentAbilityAction(
            bool isPlayer,
            Action<BoardCard> action,
            BoardCard card,
            object target,
            ActiveAbility activeAbility,
            Enumerators.CardKind kind)
        {
            if (isPlayer)
            {
                if (kind == Enumerators.CardKind.Spell)
                {
                    card.WorkingCard.Owner.Goo -= card.ManaCost;
                    _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MoveCard);

                    card.GameObject.SetActive(true);
                    card.RemoveCardParticle.Play(); // move it when card should call hide action

                    card.WorkingCard.Owner.RemoveCardFromHand(card.WorkingCard);
                    card.WorkingCard.Owner.AddCardToBoard(card.WorkingCard);

                    GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[] { card }, 0.5f, false);

                    GameClient.Get<ITimerManager>().AddTimer(
                        creat =>
                        {
                            card.WorkingCard.Owner.GraveyardCardsCount++;

                            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PlaySpellCard, new object[] { card.WorkingCard.Owner, card }));
                        },
                        null,
                        1.5f);
                }

                action?.Invoke(card);
            }
            else
            {
                if (activeAbility == null)

                    return;

                if (target is BoardUnit)
                {
                    activeAbility.Ability.TargetUnit = target as BoardUnit;
                }
                else if (target is Player)
                {
                    activeAbility.Ability.TargetPlayer = target as Player;
                }

                activeAbility.Ability.SelectedTargetAction(true);
            }

            _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
            _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
        }

        private AbilityData GetAbilityDataByType(Enumerators.AbilityType ability)
        {
            AbilityData abilityData = null;

            switch (ability)
            {
                case Enumerators.AbilityType.ReanimateUnit:
                    abilityData = new AbilityData();
                    abilityData.BuffType = "REANIMATE";
                    abilityData.Type = "REANIMATE_UNIT";
                    abilityData.ActivityType = "PASSIVE";
                    abilityData.CallType = "DEATH";
                    abilityData.ParseData();
                    break;
                case Enumerators.AbilityType.DestroyTargetUnitAfterAttack:
                    abilityData = new AbilityData();
                    abilityData.BuffType = "DESTROY";
                    abilityData.Type = "DESTROY_TARGET_UNIT_AFTER_ATTACK";
                    abilityData.ActivityType = "PASSIVE";
                    abilityData.CallType = "ATTACK";
                    abilityData.ParseData();
                    break;
            }

            return abilityData;
        }

        public class ActiveAbility
        {
            public ulong Id;

            public AbilityBase Ability;
        }
    }
}

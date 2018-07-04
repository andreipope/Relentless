// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using System.Collections.Generic;


namespace LoomNetwork.CZB
{
    public class BoardSkill : MonoBehaviour
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;

        private PlayerController _playerController;
        private BattleController _battleController;
        private BattlegroundController _battlegroundController;


        [HideInInspector]
        public Player ownerPlayer;

        public GameObject fightTargetingArrowPrefab,
                                _fireDamageVFXprefab,
                                _healVFXprefab,
                                _airPickUpCardVFXprefab,
                                _frozenVFXprefab,
                                _toxicVFXprefab,
                                _fireDamageVFX,
                                _healVFX,
                                _airPickUpCardVFX,
                                _frozenVFX,
                                _toxicVFX;

        [SerializeField]
        protected ParticleSystem sleepingParticles;

        [HideInInspector]
        public BoardArrow abilitiesTargetingArrow;
        [HideInInspector]
        public BattleBoardArrow fightTargetingArrow;

        public int manaCost;
        private int _skillPower;
        public Enumerators.SetType skillType;
        private int _cooldown;
        private int _initialCooldown;




        private bool _isUsed;
        private bool _isOpponent = false;

        private SpriteRenderer _glow;

        public int SkillPower { get { return _skillPower; } }


        private void Awake()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _fireDamageVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FIreBall_Impact");
            _healVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/HealVFX");
            _airPickUpCardVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
            _toxicVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Toxic_Impact");
            _frozenVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

            _glow = transform.Find("Glow").GetComponent<SpriteRenderer>();
            _glow.gameObject.SetActive(false);
        }

        public void SetSkill(Hero hero = null)
        {
            //manaCost = hero.skill.cost;
            //_skillPower = hero.skill.value;
            skillType = hero.heroElement;
            //_cooldown = hero.primarySkill;
        }

        public void OnStartTurn()
        {
           if (!_gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
            return;

            if (ownerPlayer.Mana >= manaCost)
                SetHighlightingEnabled(true);
        }

        public void OnEndTurn()
        {
          if (!_gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
            return;
            
            _isUsed = false;
            CancelTargetingArrows();
            SetHighlightingEnabled(false);
        }

        public void SetHighlightingEnabled(bool isActive)
        {
            _glow.gameObject.SetActive(isActive);
        }

        public void CancelTargetingArrows()
        {
            if (fightTargetingArrow != null)
            {
                Destroy(fightTargetingArrow.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (ownerPlayer == null)
                return;

            if (collider.transform.parent != null)
            {
                var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (targetingArrow != null)
                {
                    targetingArrow.OnCardSelected(null);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (ownerPlayer == null)
                return;

            if (collider.transform.parent != null)
            {
                var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (targetingArrow != null)
                {
                    targetingArrow.OnCardUnselected(null);
                }
            }
        }

        private void OnMouseDown()
        {
            if (ownerPlayer == null)
                return;

            if (!ownerPlayer.IsLocalPlayer || !_gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
                return;

            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep != 29)
                return;

            if (_isUsed)
                return;

            if (skillType != Enumerators.SetType.EARTH)
            {
                if (manaCost <= ownerPlayer.Mana)
                {
                    fightTargetingArrow = Instantiate(fightTargetingArrowPrefab).GetComponent<BattleBoardArrow>();
                    fightTargetingArrow.BoardCards = _gameplayManager.PlayersInGame.Find(x => x != ownerPlayer).BoardCards;

                    fightTargetingArrow.targetsType = new List<Enumerators.SkillTargetType>()
                {
                    Enumerators.SkillTargetType.OPPONENT_CARD,
                    Enumerators.SkillTargetType.PLAYER_CARD,
                    Enumerators.SkillTargetType.PLAYER,
                    Enumerators.SkillTargetType.OPPONENT
                };

                    if (skillType == Enumerators.SetType.AIR)
                    {
                        fightTargetingArrow.targetsType = new List<Enumerators.SkillTargetType>()
                    {
                        Enumerators.SkillTargetType.OPPONENT_CARD,
                        Enumerators.SkillTargetType.PLAYER_CARD,
                    };
                    }

                    fightTargetingArrow.Begin(transform.position);

                    if (_tutorialManager.IsTutorial)
                        _tutorialManager.DeactivateSelectTarget();
                }
            }
        }

        private void OnMouseUp()
        {
            if (ownerPlayer == null)
                return;

            if (!ownerPlayer.IsLocalPlayer || !_gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
                return;

            DoOnUpSkillAction();
        }

        public void DoOnUpSkillAction()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
                return;

            if (_isUsed)
                return;

            //Made cooldown condition mechanics
            //if (ownerPlayer.SelfHero. <= ownerPlayer.Mana)
            //{

            if (!_isOpponent && _tutorialManager.IsTutorial)
                _tutorialManager.ActivateSelectTarget();

            if (skillType == Enumerators.SetType.EARTH)
            {
                DoSkillAction(null);
            }
            else
            {
                if (!_isOpponent)
                {
                    if (fightTargetingArrow != null)
                    {
                        ResolveCombat();
                        _playerController.IsCardSelected = false;
                    }
                }
                else
                {
                    ResolveCombat();
                }
            }
            //}
        }

        public void ResolveCombat()
        {
            var sortingGroup = GetComponent<SortingGroup>();
            if (fightTargetingArrow != null)
            {
                if (fightTargetingArrow.selectedPlayer != null)
                {
                    var targetPlayer = fightTargetingArrow.selectedPlayer;
                    DoSkillAction(targetPlayer);

                    _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                }

                else if (fightTargetingArrow.selectedCard != null)
                {
                    var targetCard = fightTargetingArrow.selectedCard;
                    if (targetCard.gameObject != gameObject)// && targetCard.gameObject.GetComponent<HandBoardCard>() == null)
                    {
                        DoSkillAction(targetCard);
                    }
                }
                CancelTargetingArrows();
                fightTargetingArrow = null;
            }
        }

        private void DoSkillAction(object target)
        {
           // CreateVFX(target);
            SetHighlightingEnabled(false);
            _isUsed = true;
        }




        /*
        private void CardReturnAction(object target)
        {
            var targetCreature = target as BoardCreature;

            Debug.Log("RETURN CARD");

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            Player creatureOwner = GetOwnerOfCreature(targetCreature);
            RuntimeCard returningCard = targetCreature.card;
            Vector3 creaturePosition = targetCreature.transform.position;

            // Debug.LogError("<color=white>------return card of " + creatureOwner.GetType() + "; human " + creatureOwner.isHuman + "; to hand-------</color>");
            // Debug.LogError("<color=white>------returning card " + returningCard.instanceId + " to hand-------</color>");

            // STEP 1 - REMOVE CREATURE FROM BOARD
            if (creatureOwner.playerBoardCardsList.Contains(targetCreature)) // hack
                creatureOwner.playerBoardCardsList.Remove(targetCreature);

            // STEP 2 - DESTROY CREATURE ON THE BOARD OR ANIMATE
            CreateVFX(creaturePosition);
            MonoBehaviour.Destroy(targetCreature.gameObject);

            // STEP 3 - REMOVE RUNTIME CARD FROM BOARD
            creatureOwner.playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(returningCard);
            creatureOwner.boardZone.RemoveCard(returningCard);

            var serverCurrentPlayer = creatureOwner.Equals(ownerPlayer) ? creatureOwner.GetServer().gameState.currentPlayer : creatureOwner.GetServer().gameState.currentOpponent;

            // STEP 4 - REMOVE CARD FROM SERVER BOARD
            var boardRuntimeCard = serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == returningCard.instanceId);
            serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);

            // STEP 5 - CREATE AND ADD TO SERVER NEW RUNTIME CARD FOR HAND
            var card = CreateRuntimeCard(targetCreature, creatureOwner.playerInfo, returningCard.instanceId);
            serverCurrentPlayer.namedZones[Constants.ZONE_HAND].cards.Add(card);

            // STEP 6 - CREATE NET CARD AND SIMULATE ANIMATION OF RETURNING CARD TO HAND
            var netCard = CreateNetCard(card);
            creatureOwner.ReturnToHandRuntimeCard(netCard, creatureOwner.playerInfo, creaturePosition);

            // STEP 7 - REARRANGE CREATURES ON THE BOARD
            GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        private WorkingCard CreateRuntimeCard(BoardCreature targetCreature, PlayerInfo playerInfo, int instanceId)
        {
            var card = new RuntimeCard();
            card.cardId = targetCreature.card.cardId;
            card.instanceId = instanceId;// playerInfo.currentCardInstanceId++;
            card.ownerPlayer = playerInfo;
            card.stats[0] = targetCreature.card.stats[0];
            card.stats[1] = targetCreature.card.stats[1];
            card.namedStats[Constants.STAT_DAMAGE] = targetCreature.card.namedStats[Constants.STAT_DAMAGE].Clone();
            card.namedStats[Constants.STAT_HP] = targetCreature.card.namedStats[Constants.STAT_HP].Clone();
            card.namedStats[Constants.STAT_DAMAGE].baseValue = card.namedStats[Constants.STAT_DAMAGE].originalValue;
            card.namedStats[Constants.STAT_HP].baseValue = card.namedStats[Constants.STAT_HP].originalValue;
            return card;
        }

        private NetCard CreateNetCard(RuntimeCard card)
        {
            var netCard = new NetCard();
            netCard.cardId = card.cardId;
            netCard.instanceId = card.instanceId;
            netCard.stats = new NetStat[card.stats.Count];

            var idx = 0;

            foreach (var entry in card.stats)
                netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);

            netCard.keywords = new NetKeyword[card.keywords.Count];

            idx = 0;

            foreach (var entry in card.keywords)
                netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);

            netCard.connectedAbilities = card.connectedAbilities.ToArray();

            return netCard;
        } 
         */

        private Player GetOwnerOfCreature(BoardUnit creature)
        {
            if (_battlegroundController.playerBoardCards.Contains(creature))
                return ownerPlayer;
            else
            {
                if (ownerPlayer.IsLocalPlayer)
                    return _gameplayManager.OpponentPlayer;
                else
                    return ownerPlayer;
            }
        }   
    }
}
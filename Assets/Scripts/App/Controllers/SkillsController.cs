// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SkillsController : IController
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;
        private IUIManager _uiManager;
        private VFXController _vfxController;
        private BattleController _battleController;

        private BoardSkill _playerPrimarySkill,
                           _playerSecondarySkill,
                           _opponentPrimarySkill,
                           _opponentSecondarySkill;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _vfxController = _gameplayManager.GetController<VFXController>();
            _battleController = _gameplayManager.GetController<BattleController>();

        }

        public void Update()
        {
        }

        public void InitializeSkills()
        {
            _uiManager.GetPage<GameplayPage>().playerPrimarySkillHandler.OnMouseDownEvent += PrimarySkillHandlerOnMouseDownEventHandler;
            _uiManager.GetPage<GameplayPage>().playerPrimarySkillHandler.OnMouseUpEvent += PrimarySkillHandlerOnMouseUpEventHandler;

            _uiManager.GetPage<GameplayPage>().playerSecondarySkillHandler.OnMouseDownEvent += SecondarySkillHandlerOnMouseDownEventHandler;
            _uiManager.GetPage<GameplayPage>().playerSecondarySkillHandler.OnMouseUpEvent += SecondarySkillHandlerOnMouseUpEventHandler;


            int primary = _gameplayManager.CurrentPlayer.SelfHero.primarySkill;
            int secondary = _gameplayManager.CurrentPlayer.SelfHero.secondarySkill;

            if (primary < _gameplayManager.CurrentPlayer.SelfHero.skills.Count && secondary < _gameplayManager.CurrentPlayer.SelfHero.skills.Count)
                SetPlayerSkills(_gameplayManager.CurrentPlayer.SelfHero.skills[primary], _gameplayManager.CurrentPlayer.SelfHero.skills[secondary]);

            primary = _gameplayManager.OpponentPlayer.SelfHero.primarySkill;
            secondary = _gameplayManager.OpponentPlayer.SelfHero.secondarySkill;

            if (primary < _gameplayManager.OpponentPlayer.SelfHero.skills.Count && secondary < _gameplayManager.OpponentPlayer.SelfHero.skills.Count)
                SetOpponentSkills(_gameplayManager.OpponentPlayer.SelfHero.skills[primary], _gameplayManager.OpponentPlayer.SelfHero.skills[secondary]);
        }


        private void PrimarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if(_playerPrimarySkill != null)
            _playerPrimarySkill.StartDoSkill();
        }

        private void PrimarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerPrimarySkill != null)
                _playerPrimarySkill.EndDoSkill();
        }

        private void SecondarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
                _playerSecondarySkill.StartDoSkill();
        }

        private void SecondarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
                _playerSecondarySkill.EndDoSkill();
        }

        public void SetPlayerSkills(HeroSkill primary, HeroSkill secondary)
        {
            _playerPrimarySkill = new BoardSkill(_uiManager.GetPage<GameplayPage>().playerPrimarySkillHandler.gameObject, 
                                                 _gameplayManager.CurrentPlayer, primary);

            _playerSecondarySkill = new BoardSkill(_uiManager.GetPage<GameplayPage>().playerSecondarySkillHandler.gameObject,
                                                   _gameplayManager.CurrentPlayer, secondary);
        }

        public void SetOpponentSkills(HeroSkill primary, HeroSkill secondary)
        {
            _opponentPrimarySkill = new BoardSkill(_uiManager.GetPage<GameplayPage>().opponentPrimarySkillHandler,
                                                   _gameplayManager.OpponentPlayer, primary);

            _opponentSecondarySkill = new BoardSkill(_uiManager.GetPage<GameplayPage>().opponentSecondarySkillHandler,
                                                     _gameplayManager.OpponentPlayer, secondary);
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
                        skill.UseSkill(targetPlayer);
                        DoActionByType(skill, targetPlayer);
                        _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                    }
                    else if (skill.fightTargetingArrow.selectedCard != null)
                    {
                        var targetUnit = skill.fightTargetingArrow.selectedCard;
                        DoActionByType(skill, targetUnit);
                        skill.UseSkill(targetUnit);
                    }

                    skill.CancelTargetingArrows();
                    skill.fightTargetingArrow = null;
                }
                else
                {
                    DoActionByType(skill, target);
                    skill.UseSkill(target);
                }
            }
        }


        private void DoActionByType(BoardSkill skill, object target)
        {
            switch(skill.owner.SelfHero.heroElement)
            {
                case Enumerators.SetType.WATER:
                    FreezeAction(skill.owner, skill.skill, target);
                    break;
                case Enumerators.SetType.TOXIC:
                    ToxicDamageAction(skill.owner, skill.skill, target);
                    break;
                case Enumerators.SetType.FIRE:
                    FireDamageAction(skill.owner, skill.skill, target);
                    break;
                case Enumerators.SetType.LIFE:
                    HealAnyAction(skill.owner, skill.skill, target);
                    break;
                case Enumerators.SetType.EARTH:
                    HealAction(skill.owner, skill.skill);
                    break;
                case Enumerators.SetType.AIR:
                    //CardReturnAction(skill.owner, skill.skill, target);
                    break;
                default:
                    break;
            }
        }

        #region actions

        private void FreezeAction(Player owner, HeroSkill skill, object target)
        {
            if (target is BoardUnit)
            {
                var unit = target as BoardUnit;
                unit.Stun(skill.value);
                _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, unit.transform.position, target, SkillParticleActionCompleted);
            }
            else if (target is Player)
            {

            }
        }

        private void ToxicDamageAction(Player owner, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, skill, target, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
        }

        private void FireDamageAction(Player owner, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
        }

        private void HealAnyAction(Player owner, HeroSkill skill, object target)
        {
            if (target is Player)
            {
                var player = target as Player;

                _battleController.HealPlayerBySkill(owner, skill, player);

            //    _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, player.AvatarObject.transform.position, owner, SkillParticleActionCompleted);
            }
            else
            {
                var unit = target as BoardUnit;

                _battleController.HealCreatureBySkill(owner, skill, unit);

            //    _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, unit.transform.position, unit, SkillParticleActionCompleted);
            }
        }

        private void HealAction(Player owner, HeroSkill skill)
        {
            _battleController.HealPlayerBySkill(owner, skill, owner);

            _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, owner.AvatarObject.transform.position - Vector3.right * 2.3f, owner, SkillParticleActionCompleted);
        }

        private void AttackWithModifiers(Player owner, HeroSkill skill, object target, Enumerators.SetType attackType, Enumerators.SetType setType)
        {
            if (target is Player)
            {
                var player = target as Player;
                //TODO additional damage to heros

                _battleController.AttackPlayerBySkill(owner, skill, player);

              //  _vfxController.CreateSkillVFX(attackType, player.AvatarObject.transform.position, owner, SkillParticleActionCompleted);
            }
            else
            {
                var creature = target as BoardUnit;
                var attackModifier = 0;

                if (creature.Card.libraryCard.cardSetType == setType)
                    attackModifier = 1;

                _battleController.AttackCreatureBySkill(owner, skill, creature, attackModifier);

             //   _vfxController.CreateSkillVFX(attackType, creature.transform.position, owner, SkillParticleActionCompleted);
            }
        }


        /*
        private void CardReturnAction(Player owner, HeroSkill skill, object target)
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
        #endregion
    }
}
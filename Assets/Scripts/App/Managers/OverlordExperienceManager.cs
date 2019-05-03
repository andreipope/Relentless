using System;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using UnityEngine;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using OverlordLevelingData = Loom.ZombieBattleground.Data.OverlordLevelingData;
using OverlordSkill = Loom.ZombieBattleground.Data.OverlordSkill;

namespace Loom.ZombieBattleground
{
    public class OverlordExperienceManager : IService, IOverlordExperienceManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordExperienceManager));

        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        
        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        public ExperienceInfo PlayerMatchExperienceInfo { get; private set; }
        public ExperienceInfo OpponentMatchExperienceInfo { get; private set; }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void InitializeExperienceInfoInMatch(OverlordModel overlord)
        {
            PlayerMatchExperienceInfo = new ExperienceInfo();
            PlayerMatchExperienceInfo.LevelAtBegin = overlord.Level;
            PlayerMatchExperienceInfo.ExperienceAtBegin = overlord.Experience;
            PlayerMatchExperienceInfo.ExperienceReceived = 0;
        }

        public void InitializeOpponentExperienceInfoInMatch(OverlordModel overlord)
        {
            OpponentMatchExperienceInfo = new ExperienceInfo();
            OpponentMatchExperienceInfo.LevelAtBegin = overlord.Level;
            OpponentMatchExperienceInfo.ExperienceAtBegin = overlord.Experience;
            OpponentMatchExperienceInfo.ExperienceReceived = 0;
        }

        public async Task GetLevelAndRewards(OverlordModel overlord)
        {
            await Task.CompletedTask;
            /*try
            {
                GetOverlordLevelResponse overlordLevelResponse = await _backendFacade.GetOverlordLevel(
                    _backendDataControlMediator.UserDataModel.UserId,
                    overlord.OverlordId);

                overlord.Experience = overlordLevelResponse.Experience;
                int level = (int)overlordLevelResponse.Level;

                CheckLevel(overlord, level);
            }
            catch (TimeoutException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Warn("Time out ==", e);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
            }
            catch (Client.RpcClientException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Warn("RpcException ==", e);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Info("Result ===", e);
                OpenAlertDialog($"Not able Get Level and Rewards : " + e.Message);
                return;
            }*/
        }

        public long GetRequiredExperienceForNewLevel(OverlordModel overlord)
        {
            return _dataManager.CachedOverlordLevelingData.Fixed + _dataManager.CachedOverlordLevelingData.ExperienceStep * (overlord.Level + 1);
        }

        public void ReportExperienceAction(Enumerators.ExperienceActionType actionType, bool isOpponent = false)
        {
            if (_gameplayManager.IsTutorial)
                return;

            Data.ExperienceAction action = _dataManager.CachedOverlordLevelingData.ExperienceActions.Find(x => x.Action == actionType);
            int value = action.Experience;
            if (isOpponent)
                OpponentMatchExperienceInfo.ExperienceReceived += value;
            else
                PlayerMatchExperienceInfo.ExperienceReceived += value;
        }

        public Data.LevelReward GetLevelReward(OverlordModel overlord)
        {
            return _dataManager.CachedOverlordLevelingData.Rewards.Find(x => x.Level == overlord.Level);
        }

        private void CheckLevel(OverlordModel overlord)
        {
            while (overlord.Experience >= GetRequiredExperienceForNewLevel(overlord) &&
                   overlord.Level < _dataManager.CachedOverlordLevelingData.MaxLevel)
            {
                LevelUp(overlord);
            }
        }

        private void CheckLevel(OverlordModel overlord, int updatedlevel)
        {
            while (overlord.Level < updatedlevel)
            {
                LevelUp(overlord);
            }
        }

        private void LevelUp(OverlordModel overlord)
        {
            overlord.Level++;

            ApplyReward(overlord);
        }

        private void ApplyReward(OverlordModel overlord)
        {
            Data.LevelReward levelReward = GetLevelReward(overlord);
            return;
            // FIXME: LEVELING load from server?
            switch (levelReward)
            {
                case null:
                    break;
                case Data.OverlordSkillRewardItem overlordSkillRewardItem:
                    OverlordSkill unlockedSkill = overlord.GetSkill(overlordSkillRewardItem.SkillIndex);
                    unlockedSkill.Unlocked = true;

                    SaveSkillInDecks(overlord.OverlordId, unlockedSkill);

                    PlayerMatchExperienceInfo.GotRewards.Add(levelReward);
                    break;
                case Data.UnitRewardItem unitRewardItem:
                    List<Card> cards = _dataManager.CachedCardsLibraryData.Cards
                        .Where(x => x.Rank == unitRewardItem.Rank)
                        .ToList();
                    Card card = cards[UnityEngine.Random.Range(0, cards.Count)];
                    CollectionCardData foundCard = _dataManager.CachedCollectionData.Cards.Find(x => x.MouldId == card.MouldId);
                    if (foundCard != null)
                    {
                        foundCard.Amount += unitRewardItem.Count;
                    }
                    else
                    {
                        _dataManager.CachedCollectionData.Cards.Add(new CollectionCardData(card.MouldId, unitRewardItem.Count));
                    }

                    PlayerMatchExperienceInfo.GotRewards.Add(levelReward);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(levelReward));
            }
        }

        private void SaveSkillInDecks(int overlordId, OverlordSkill skill)
        {
            List<Deck> decks = _dataManager.CachedDecksData.Decks.FindAll((x) =>
                x.OverlordId == overlordId &&
                (x.PrimarySkill == Enumerators.Skill.NONE || x.SecondarySkill == Enumerators.Skill.NONE));

            foreach (Deck deck in decks)
            {
                if (deck.OverlordId == overlordId)
                {
                    if (deck.PrimarySkill == Enumerators.Skill.NONE)
                    {
                        deck.PrimarySkill = skill.Skill;
                        SaveDeck(deck);
                    }
                    else if (deck.SecondarySkill == Enumerators.Skill.NONE)
                    {
                        deck.SecondarySkill = skill.Skill;
                        SaveDeck(deck);
                    }
                }
            }
        }

        private async void SaveDeck(Deck deck)
        {
            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deck);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);

                OpenAlertDialog("Not able to Save Deck: \n" + e.Message);
            }
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            GameClient.Get<IUIManager>().DrawPopup<WarningPopup>(msg);
        }
    }
}

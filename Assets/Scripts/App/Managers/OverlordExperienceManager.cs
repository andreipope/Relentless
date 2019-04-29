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
using OverlordSkill = Loom.ZombieBattleground.Data.OverlordSkill;

namespace Loom.ZombieBattleground
{
    public class OverlordExperienceManager : IService, IOverlordExperienceManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordExperienceManager));

        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;

        private OvelordExperienceInfo _overlordXPInfo;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        public ExperienceInfo MatchExperienceInfo { get; private set; }
        public ExperienceInfo OpponentMatchExperienceInfo { get; private set; }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _overlordXPInfo = JsonConvert.DeserializeObject<OvelordExperienceInfo>(
                            _loadObjectsManager.GetObjectByPath<TextAsset>("Data/overlord_experience_data").text);

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
            MatchExperienceInfo = new ExperienceInfo();
            MatchExperienceInfo.LevelAtBegin = overlord.Level;
            MatchExperienceInfo.ExperienceAtBegin = overlord.Experience;
            MatchExperienceInfo.ExperienceReceived = 0;
        }

        public void InitializeOpponentExperienceInfoInMatch(OverlordModel overlord)
        {
            OpponentMatchExperienceInfo = new ExperienceInfo();
            OpponentMatchExperienceInfo.LevelAtBegin = overlord.Level;
            OpponentMatchExperienceInfo.ExperienceAtBegin = overlord.Experience;
            OpponentMatchExperienceInfo.ExperienceReceived = 0;
        }

        public async Task ApplyExperienceFromMatch(OverlordModel overlord)
        {
            try
            {
                SetOverlordExperienceResponse experienceResponse = await _backendFacade.SetOverlordExperience(
                    _backendDataControlMediator.UserDataModel.UserId,
                    overlord.OverlordId,
                    MatchExperienceInfo.ExperienceReceived);

                overlord.Experience = experienceResponse.Experience;
                CheckLevel(overlord);
            }
            catch (Exception e)
            {
                Debug.LogError("Not able to save Experience, " + e);
            }

            _dataManager.SaveCache(Enumerators.CacheDataType.OVERLORDS_DATA);
            _dataManager.SaveCache(Enumerators.CacheDataType.COLLECTION_DATA);
        }

        public async Task GetLevelAndRewards(OverlordModel overlord)
        {
            try
            {
                GetOverlordLevelResponse overlordLevelResponse = await _backendFacade.GetOverlordLevel(
                    _backendDataControlMediator.UserDataModel.UserId,
                    overlord.OverlordId);

                overlord.Experience = overlordLevelResponse.Experience;
                int level = (int)overlordLevelResponse.Level;

                CheckLevel(overlord, level);
            }
            catch (Exception e)
            {
                Debug.LogError("Not able to get level, " + e);
            }
        }


        public async void ApplyExperience(OverlordModel overlord, int experience)
        {
            try
            {
                SetOverlordExperienceResponse experienceResponse = await _backendFacade.SetOverlordExperience(
                    _backendDataControlMediator.UserDataModel.UserId,
                    overlord.OverlordId,
                    experience);

                overlord.Experience = experienceResponse.Experience;
                CheckLevel(overlord);
            }
            catch (Exception e)
            {
                Debug.LogError("Not able to save Experience, " + e);
            }

            _dataManager.SaveCache(Enumerators.CacheDataType.OVERLORDS_DATA);
            _dataManager.SaveCache(Enumerators.CacheDataType.COLLECTION_DATA);
        }

        public int GetRequiredExperienceForNewLevel(OverlordModel overlord)
        {
            return _overlordXPInfo.Fixed + _overlordXPInfo.ExperienceStep * (overlord.Level + 1);
        }

        public void ChangeExperience(OverlordModel overlord, int value)
        {
            if (_gameplayManager.IsTutorial)
                return;

            MatchExperienceInfo.ExperienceReceived += value;
        }

        public void ReportExperienceAction(OverlordModel overlord, Enumerators.ExperienceActionType actionType)
        {
            ExperienceAction action = _overlordXPInfo.ExperienceActions.Find(x => x.Action == actionType);

            ChangeExperience(overlord, action.Experience);
        }

        public void ReportExperienceAction(Enumerators.ExperienceActionType actionType, bool isOpponent = false)
        {
            if (_gameplayManager.IsTutorial)
                return;

            ExperienceAction action = _overlordXPInfo.ExperienceActions.Find(x => x.Action == actionType);
            int value = action.Experience;
            if (isOpponent)
                OpponentMatchExperienceInfo.ExperienceReceived += value;
            else
                MatchExperienceInfo.ExperienceReceived += value;
        }

        public LevelReward GetLevelReward(OverlordModel overlord)
        {
            return _overlordXPInfo.Rewards.Find(x => x.Level == overlord.Level);
        }

        private void CheckLevel(OverlordModel overlord)
        {
            while (overlord.Experience >= GetRequiredExperienceForNewLevel(overlord) &&
                   overlord.Level < _overlordXPInfo.MaxLevel)
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
            LevelReward levelReward = GetLevelReward(overlord);
            if (levelReward != null)
            {
                if (levelReward.UnitReward != null)
                {
                    List<Card> cards = _dataManager.CachedCardsLibraryData.Cards
                        .Where(x => x.Rank.ToString() == levelReward.UnitReward.Rank)
                        .ToList();
                    Card card = cards[UnityEngine.Random.Range(0, cards.Count)];
                    CollectionCardData foundCard = _dataManager.CachedCollectionData.Cards.Find(x => x.CardName == card.Name);
                    if (foundCard != null)
                    {
                        foundCard.Amount += levelReward.UnitReward.Count;
                    }
                    else
                    {
                        _dataManager.CachedCollectionData.Cards.Add(new CollectionCardData()
                        {
                            Amount = levelReward.UnitReward.Count,
                            CardName = card.Name
                        });
                    }
                }
                else if (levelReward.SkillReward != null)
                {
                    OverlordSkill unlockedSkill = overlord.GetSkill(levelReward.SkillReward.SkillIndex);
                    unlockedSkill.Unlocked = true;

                    SaveSkillInDecks(overlord.OverlordId, unlockedSkill);
                }

                MatchExperienceInfo.GotRewards.Add(levelReward);
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

        public class LevelReward
        {
            public int Level;
            public OverlordSkillRewardItem SkillReward;
            public UnitRewardItem UnitReward;

            public class UnitRewardItem 
            {
                public string Rank;
                public int Count;
            }

            public class OverlordSkillRewardItem
            {
                public int SkillIndex;
            }
        }

        public class ExperienceAction
        {
            public Enumerators.ExperienceActionType Action;
            public int Experience;
        }

        public class OvelordExperienceInfo
        {
            public List<LevelReward> Rewards;
            public List<ExperienceAction> ExperienceActions;
            public int Fixed;
            public int ExperienceStep;
            public int GooRewardStep;
            public int MaxLevel;
        }


        public class ExperienceInfo
        {
            public int LevelAtBegin;
            public long ExperienceAtBegin;
            public long ExperienceReceived;

            public List<LevelReward> GotRewards = new List<LevelReward>();
        }
    }
}

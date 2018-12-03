using System;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class OverlordExperienceManager : IService, IOverlordExperienceManager
    {
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;

        private OvelordExperienceInfo _overlordXPInfo;

        public ExperienceInfo MatchExperienceInfo { get; private set; }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _overlordXPInfo = JsonConvert.DeserializeObject<OvelordExperienceInfo>(
                            _loadObjectsManager.GetObjectByPath<TextAsset>("Data/overlord_experience_data").text);
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void InitializeExperienceInfoInMatch(Hero hero)
        {
            MatchExperienceInfo = new ExperienceInfo();
            MatchExperienceInfo.LevelAtBegin = hero.Level;
            MatchExperienceInfo.ExperienceAtBegin = hero.Experience;
            MatchExperienceInfo.ExperienceReceived = 0;
        }

        public void ApplyExperienceFromMatch(Hero hero)
        {
            hero.Experience += MatchExperienceInfo.ExperienceReceived;
            CheckLevel(hero);

            _dataManager.SaveCache(Enumerators.CacheDataType.HEROES_DATA);
            _dataManager.SaveCache(Enumerators.CacheDataType.COLLECTION_DATA);
        }

        public int GetRequiredExperienceForNewLevel(Hero hero)
        {
            return _overlordXPInfo.Fixed + _overlordXPInfo.ExperienceStep * (hero.Level + 1);
        }

        public void ChangeExperience(Hero hero, int value)
        {
            if (_gameplayManager.IsTutorial)
                return;

            MatchExperienceInfo.ExperienceReceived += value;
        }

        public void ReportExperienceAction(Hero hero, Enumerators.ExperienceActionType actionType)
        {
            ExperienceAction action = _overlordXPInfo.ExperienceActions.Find(x => x.Action == actionType);

            ChangeExperience(hero, action.Experience);
        }

        public LevelReward GetLevelReward(Hero hero)
        {
            return _overlordXPInfo.Rewards.Find(x => x.Level == hero.Level);
        }

        private void CheckLevel(Hero hero)
        {
            while (hero.Experience >= GetRequiredExperienceForNewLevel(hero) &&
                   hero.Level < _overlordXPInfo.MaxLevel)
            {
                LevelUp(hero);
            }
        }

        private void LevelUp(Hero hero)
        {
            hero.Level++;

            ApplyReward(hero);
        }

        private void ApplyReward(Hero hero)
        {
            LevelReward levelReward = GetLevelReward(hero);
            if (levelReward != null)
            {
                if (levelReward.UnitReward != null)
                {
                    List<Card> cards = _dataManager.CachedCardsLibraryData.Cards
                        .Where(x => x.CardRank.ToString() == levelReward.UnitReward.Rank)
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
                    hero.GetSkill(levelReward.SkillReward.SkillIndex).Unlocked = true;
                }

                MatchExperienceInfo.GotRewards.Add(levelReward);
            }
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

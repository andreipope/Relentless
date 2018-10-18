using System;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class OverlordManager : IService, IOverlordManager
    {
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;

        private OvelordExperienceInfo _ovelordXPInfo;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _ovelordXPInfo = JsonConvert.DeserializeObject<OvelordExperienceInfo>(
                            _loadObjectsManager.GetObjectByPath<TextAsset>("Data/overlord_experience_data").text);
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public int GetRequiredExperienceForNewLevel(Hero hero)
        {
            return _ovelordXPInfo.Fixed + _ovelordXPInfo.ExperienceStep * (hero.Level + 1);
        }

        public void ChangeExperience(Hero hero, int value)
        {
            hero.Experience += value;
            CheckLevel(hero);
        }

        public void ReportExperienceAction(Hero hero, Enumerators.ExperienceActionType actionType)
        {
            ExperienceAction action = _ovelordXPInfo.ExperienceActions.Find(x => x.Action == actionType);

            ChangeExperience(hero, action.Experience);
        }

        private void CheckLevel(Hero hero)
        {
            if (hero.Experience >= GetRequiredExperienceForNewLevel(hero))
            {
                LevelUp(hero);
            }
        }

        private void LevelUp(Hero hero)
        {
            hero.Level++;

            ApplyReward(hero);

            _dataManager.SaveCache(Enumerators.CacheDataType.HEROES_DATA);
            _dataManager.SaveCache(Enumerators.CacheDataType.COLLECTION_DATA);
        }

        private void ApplyReward(Hero hero)
        {
            LevelReward levelReward = _ovelordXPInfo.Rewards.Find(x => x.Level == hero.Level);

            if (levelReward != null)
            {
                switch (levelReward.Reward)
                {
                    case LevelReward.UnitRewardItem unitReward:
                        {
                            List<Card> cards = _dataManager.CachedCardsLibraryData.Cards.FindAll(x => x.CardRank == unitReward.Rank);
                            Card card = cards[UnityEngine.Random.Range(0, cards.Count)];
                            CollectionCardData foundCard = _dataManager.CachedCollectionData.Cards.Find(x => x.CardName == card.Name);
                            if (foundCard != null)
                            {
                                foundCard.Amount += unitReward.Count;
                            }
                            else
                            {
                                _dataManager.CachedCollectionData.Cards.Add(new CollectionCardData()
                                {
                                    Amount = unitReward.Count,
                                    CardName = card.Name
                                });
                            }
                        }
                        break;
                    case LevelReward.OverlordSkillRewardItem skillReward:
                        {
                            //TODO: commented now in perspective of lock funcitonality for release stage
                            //hero.Skills[skillReward.SkillIndex].Unlocked = true;
                        }
                        break;
                    case LevelReward.ItemReward itemReward:
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(levelReward.Reward), levelReward.Reward, null);
                }
            }
        }

        public class LevelReward
        {
            public int Level;
            public ItemReward Reward;

            public class UnitRewardItem : ItemReward
            {
                public Enumerators.CardRank Rank;
                public int Count;
            }

            public class OverlordSkillRewardItem : ItemReward
            {
                public int SkillIndex;
            }

            public class ItemReward
            {

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
        }
    }
}

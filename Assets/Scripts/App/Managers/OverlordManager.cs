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

        private OvelordXPInfo _ovelordXPInfo;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _ovelordXPInfo = JsonConvert.DeserializeObject<OvelordXPInfo>(
                            _loadObjectsManager.GetObjectByPath<TextAsset>("Data/overlord_xp_system_data").text);
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public int GetRequiredXPForNewLevel(Hero hero)
        {
            return _ovelordXPInfo.Fixed + _ovelordXPInfo.XPStep * (hero.Level + 1);
        }

        public void ChangeExperience(Hero hero, int value)
        {
            hero.Experience += value;
            CheckLevel(hero);
        }

        public void ReportXPAction(Hero hero, Enumerators.XPActionType actionType)
        {
            ActionsXP action = _ovelordXPInfo.ActionsXP.Find(x => x.Action == actionType);

            ChangeExperience(hero, action.XP);
        }

        private void CheckLevel(Hero hero)
        {
            if (hero.Experience >= GetRequiredXPForNewLevel(hero))
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
            LevelReward reward = _ovelordXPInfo.Rewards.Find(x => x.Level == hero.Level);

            switch (reward.Reward)
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
                        hero.Skills[skillReward.SkillIndex].Unlocked = true;
                    }
                    break;
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

        public class ActionsXP
        {
            public Enumerators.XPActionType Action;
            public int XP;
        }
        public class OvelordXPInfo
        {
            public List<LevelReward> Rewards;
            public List<ActionsXP> ActionsXP;
            public int Fixed;
            public int XPStep;
            public int GooRewardStep;
        }
    }
}

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
using LevelReward = Loom.ZombieBattleground.Data.LevelReward;
using OverlordLevelingData = Loom.ZombieBattleground.Data.OverlordLevelingData;
using OverlordSkill = Loom.ZombieBattleground.Data.OverlordSkill;

namespace Loom.ZombieBattleground
{
    public class OverlordExperienceManager : IService, IOverlordExperienceManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordExperienceManager));

        private BackendFacade _backendFacade;
        private IDataManager _dataManager;
        private INetworkActionManager _networkActionManager;
        private IGameplayManager _gameplayManager;
        private BackendDataControlMediator _backendDataControlMediator;

        public ExperienceInfo PlayerMatchExperienceInfo { get; private set; }
        public ExperienceInfo OpponentMatchExperienceInfo { get; private set; }

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _dataManager = GameClient.Get<IDataManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void InitializeMatchExperience(OverlordModel playerOverlord, OverlordModel opponentOverlord)
        {
            PlayerMatchExperienceInfo = new ExperienceInfo(playerOverlord.Level, playerOverlord.Experience);
            OpponentMatchExperienceInfo = new ExperienceInfo(opponentOverlord.Level, opponentOverlord.Experience);
        }

        public void ReportExperienceAction(Enumerators.ExperienceActionType actionType, ExperienceInfo experienceInfo)
        {
            if (_gameplayManager.IsTutorial)
                return;

            Data.ExperienceAction action = _dataManager.CachedOverlordLevelingData.ExperienceActions.Single(x => x.Action == actionType);
            experienceInfo.ExperienceReceived += action.Experience;
        }

        public long GetRequiredExperienceForLevel(int level)
        {
            if (level <= 1)
                return 0;

            return _dataManager.CachedOverlordLevelingData.Fixed + _dataManager.CachedOverlordLevelingData.ExperienceStep * level;
        }

        public async Task<ExperienceDeltaInfo> UpdateLevelAndExperience(OverlordModel overlordModel)
        {
            (int? notificationId, ExperienceDeltaInfo experienceDeltaInfo, bool isWin) = await GetExperienceDeltaInfoFromEndMatchNotification();
            if (experienceDeltaInfo != null)
            {
                await _networkActionManager.EnqueueNetworkTask(
                    async () =>
                        await _backendFacade.ClearNotifications(_backendDataControlMediator.UserDataModel.UserId,new[]{ notificationId.Value }));

                overlordModel.Level = experienceDeltaInfo.CurrentLevel;
                overlordModel.Experience = experienceDeltaInfo.CurrentExperience;
            }

            Log.Warn("No EndMatchNotification, returning known values");
            return new ExperienceDeltaInfo(
                overlordModel.Level,
                overlordModel.Experience,
                overlordModel.Level,
                overlordModel.Experience,
                Array.Empty<LevelReward>()
            );
        }

        public async Task<(int? notificationId, ExperienceDeltaInfo experienceDeltaInfo, bool isWin)> GetExperienceDeltaInfoFromEndMatchNotification()
        {
            GetNotificationsResponse notificationsResponse = null;
            await _networkActionManager.EnqueueNetworkTask(async () =>
                notificationsResponse = await _backendFacade.GetNotifications(_backendDataControlMediator.UserDataModel.UserId));

            List<Notification> notifications = notificationsResponse.Notifications.Select(n => n.FromProtobuf()).ToList();
            foreach (Notification notification in notifications)
            {
                if (!(notification is EndMatchNotification endMatchNotification))
                    continue;

                ExperienceDeltaInfo experienceDeltaInfo = new ExperienceDeltaInfo(
                    endMatchNotification.OldLevel,
                    endMatchNotification.OldExperience,
                    endMatchNotification.NewLevel,
                    endMatchNotification.NewExperience,
                    endMatchNotification.Rewards
                );

                return (notification.Id, experienceDeltaInfo, endMatchNotification.IsWin);
            }

            return (null, null, false);
        }

        /*private void SaveSkillInDecks(int overlordId, OverlordSkill skill)
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
        }*/
    }
}

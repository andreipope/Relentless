using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;

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

        public MatchExperienceInfo PlayerMatchMatchExperienceInfo { get; private set; }
        public MatchExperienceInfo OpponentMatchMatchExperienceInfo { get; private set; }

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

        public void InitializeMatchExperience()
        {
            PlayerMatchMatchExperienceInfo = new MatchExperienceInfo();
            OpponentMatchMatchExperienceInfo = new MatchExperienceInfo();
        }

        public void ReportExperienceAction(Enumerators.ExperienceActionType actionType, MatchExperienceInfo matchExperienceInfo)
        {
            if (_gameplayManager.IsTutorial)
                return;

            Data.ExperienceAction action = _dataManager.CachedOverlordLevelingData.ExperienceActions.Single(x => x.Action == actionType);
            matchExperienceInfo.ExperienceReceived += action.Experience;
        }

        public long GetRequiredExperienceForLevel(int level)
        {
            if (level <= 1)
                return 0;

            return _dataManager.CachedOverlordLevelingData.Fixed + _dataManager.CachedOverlordLevelingData.ExperienceStep * (level - 1);
        }

        public async Task<(int? notificationId, EndMatchResults endMatchResults)> GetEndMatchResultsFromEndMatchNotification()
        {
            GetNotificationsResponse notificationsResponse = null;
            await _networkActionManager.EnqueueNetworkTask(async () =>
                notificationsResponse = await _backendFacade.GetNotifications(_backendDataControlMediator.UserDataModel.UserId));

            List<Notification> notifications = notificationsResponse.Notifications.Select(n => n.FromProtobuf()).ToList();
            foreach (Notification notification in notifications)
            {
                if (!(notification is EndMatchNotification endMatchNotification))
                    continue;

                EndMatchResults endMatchResults = new EndMatchResults(
                    endMatchNotification.DeckId,
                    endMatchNotification.OverlordId,
                    endMatchNotification.OldLevel,
                    endMatchNotification.OldExperience,
                    endMatchNotification.NewLevel,
                    endMatchNotification.NewExperience,
                    endMatchNotification.IsWin,
                    endMatchNotification.Rewards
                );

                return (notification.Id, endMatchResults);
            }

            return (null, null);
        }

        /*private void SaveSkillInDecks(int overlordId, OverlordSkillPrototype skill)
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

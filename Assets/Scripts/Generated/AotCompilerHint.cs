#if ENABLE_IL2CPP || UNITY_EDITOR

namespace Loom.ZombieBattleground
{
    internal static class AotCompilerHint
    {
        static AotCompilerHint()
        {
            // start
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Account>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.String>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.Boolean>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.Int64>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.Int32>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.ByteString>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Deck>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.DeckCard>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Card>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardKind.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardSetType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CreatureRank.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CreatureType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardViewInfo>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardAbility>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UniqueAnimationType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Vector3Float>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.Single>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Vector2Int>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Rect>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Card>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardCollectionCard>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DeckCard>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardLibrary>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Hero>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Skill>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListHeroesRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListHeroesResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Hero>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AddHeroExperienceRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AddHeroExperienceResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetHeroRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetHeroResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetHeroSkillsRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetHeroSkillsResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Skill>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.OverlordSkillKind.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.OverlordAbilityTarget.Types.Enum>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UnitSpecialStatus.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardSetType.Types.Enum>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HeroList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardCollectionList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardCollectionCard>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DeckList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Deck>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AIDeck>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AIType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AIDeckList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.AIDeck>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.InitRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Address>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpdateOracle>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpdateInitRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetInitRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetInitResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpdateCardListRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCardListRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCardListResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpsertAccountRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetAccountRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetDeckRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetDeckResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CreateDeckRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CreateDeckResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DeleteDeckRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.EditDeckRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DecksResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListDecksRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListDecksResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.SetAIDecksRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetAIDecksRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetAIDecksResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListCardLibraryRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListCardLibraryResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListHeroLibraryRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.ListHeroLibraryResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpdateHeroLibraryRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCollectionRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCollectionResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerState>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.OverlordInstance>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardInstance>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.InitialPlayerState>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Match>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::System.String>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.InitialPlayerState>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Match.Types.Status>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.MatchMakingInfoList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.MatchMakingInfo>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.MatchMakingInfo>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerAction>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionCardAttack>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionDrawCard>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionEndTurn>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionMulligan>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionCardPlay>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionCardAbilityUsed>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionOverlordSkillUsed>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionLeaveMatch>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionRankBuff>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerAction.ActionOneofCase>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionEvent>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.History>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerProfile>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerPool>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.PlayerProfile>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.MatchCount>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.RegisterPlayerPoolRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerPoolResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.FindMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.FindMatchResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AcceptMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AcceptMatchResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DebugFindMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CancelFindMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetMatchResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.SetMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetGameStateRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetGameStateResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameState>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.SetGameStateRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.BundlePlayerActionRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.PlayerAction>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.BundlePlayerActionResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.HistoryData>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.EndMatchRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.EndMatchResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CheckGameStatusRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameMode>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameModeType>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameModeList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.GameMode>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameModeRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.UpdateGameModeRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DeleteGameModeRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetGameModeRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCustomGameModeCustomUiRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GetCustomGameModeCustomUiResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CustomGameModeCustomUiElement>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CallCustomGameModeFunctionRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.StaticCallCustomGameModeFunctionResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CustomGameModeCustomUiLabel>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CustomGameModeCustomUiButton>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CustomGameModeCustomUiElement>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CustomGameModeCustomUiElement.UiElementOneofCase>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.PlayerState>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardChoosableAbility>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbility>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbility.Types.VisualEffectInfo>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbility.Types.VisualEffectInfo.Types.VisualEffectType>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbilityType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbilityActivityType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbilityTrigger.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardAbilityTarget.Types.Enum>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.StatType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbilityEffect.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AttackRestriction.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardAbility.Types.VisualEffectInfo>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameMechanicDescriptionType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardAbilitySubTrigger.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.CardChoosableAbility>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardInstance>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.DataIdOwner>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.CardDeck>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.InstanceIdOwner>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.OverlordPrototype>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.OverlordSkillInstance>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.OverlordSkillPrototype>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.OverlordSkillPrototype>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Unit>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.AffectObjectType.Types.Enum>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Unit>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.PlayerActionOutcome>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.StartGameAction>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Parameter>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.GameReplay>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Player>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryData>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryCreateGame>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryFullInstance>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryInstance>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryHide>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryEndGame>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.HistoryData.DataOneofCase>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.ZombieBattleground.Protobuf.Player>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.ZombieBattleground.Protobuf.Zone>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.BlockHeader>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.BlockID>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.PartSetHeader>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Validator>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Evidence>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.BigUInt>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Transaction>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.UInt32>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.PlasmaBlock>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.Client.Protobuf.PlasmaTx>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.PlasmaTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::System.UInt64>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.GetCurrentBlockResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.GetBlockRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.GetBlockResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.SubmitBlockToMainnetResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.PlasmaTxRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.DepositRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.AddressMapperMapping>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.AddressMapperAddIdentityMappingRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.AddressMapperAddContractMappingRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.AddressMapperGetMappingRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.AddressMapperGetMappingResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.SignedTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.NonceTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.MessageTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.DeployTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.VMType>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.DeployResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.DeployResponseData>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.CallTx>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.PluginCode>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Request>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EncodingType>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.Response>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.ContractMethodCall>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthFilterEnvelope>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthBlockHashList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthFilterLogList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthTxHashList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthFilterEnvelope.MessageOneofCase>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.Google.Protobuf.ByteString>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EventDataList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.Client.Protobuf.EventData>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EventData>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TxReceiptList>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.Client.Protobuf.EvmTxReceipt>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EvmTxReceipt>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EvmTxObject>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthBlockInfo>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Google.Protobuf.Collections.RepeatedField<global::Loom.Client.Protobuf.EthFilterLog>>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.EthFilterLog>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayContractMappingConfirmed>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayTokenWithdrawalSigned>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayTokenKind>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayWithdrawalReceipt>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayWithdrawERC721Request>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayWithdrawalReceiptRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayWithdrawalReceiptResponse>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayConfirmWithdrawalReceiptRequest>();
            Loom.Google.Protobuf.Reflection.FileDescriptor.ForceReflectionInitialization<global::Loom.Client.Protobuf.TransferGatewayAddContractMappingRequest>();

            // end
        }
    }
}

#endif

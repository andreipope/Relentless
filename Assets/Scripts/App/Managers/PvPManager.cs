using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using Card = Loom.ZombieBattleground.Protobuf.Card;
using SystemText = System.Text;

public class PvPManager : IService
{
    public FindMatchResponse MatchResponse { get; set; }
    public GetGameStateResponse GameStateResponse { get; set; }

    private BackendFacade _backendFacade;
    private BackendDataControlMediator _backendDataControlMediator;

    public OpponentDeck OpponentDeck { get; set; }
    public int OpponentDeckIndex { get; set; }

    public Action OnGameStarted;
    public Action OnGetEndTurnAction;
    public Action<PlayerActionCardPlay> OnCardPlayedAction;

    public void Init()
    {
        _backendFacade = GameClient.Get<BackendFacade>();
        _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

        _backendFacade.PlayerActionEvent += OnGetPlayerActionEventListener;
    }


    private void OnGetPlayerActionEventListener(byte[] data)
    {
        string jsonStr = SystemText.Encoding.UTF8.GetString(data);
        Debug.LogWarning(jsonStr);
        PlayerActionEvent playerActionEvent = JsonConvert.DeserializeObject<PlayerActionEvent>(jsonStr);
        MatchResponse.Match = playerActionEvent.Match.Clone();

        switch (playerActionEvent.Match.Status)
        {
            case Match.Types.Status.Created:
                break;
            case Match.Types.Status.Matching:
                break;
            case Match.Types.Status.Started:
                OnGameStarted?.Invoke();
                break;
            case Match.Types.Status.Playing:
                if (playerActionEvent.UserId == _backendDataControlMediator.UserDataModel.UserId)
                    return;
                OnReceivePlayerActionType(playerActionEvent);
                break;
            case Match.Types.Status.PlayerLeft:
                break;
            case Match.Types.Status.Ended:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
    {
        switch (playerActionEvent.PlayerActionType)
        {
            case PlayerActionType.NoneAction:
                break;
            case PlayerActionType.EndTurn:
                OnGetEndTurnAction?.Invoke();
                break;
            case PlayerActionType.Mulligan:
                break;
            case PlayerActionType.CardPlay:
                OnCardPlayedAction?.Invoke(playerActionEvent.PlayerAction.CardPlay);
                break;
            case PlayerActionType.CardAttack:
                break;
            case PlayerActionType.UseCardAbility:
                break;
            case PlayerActionType.UseOverlordSkill:
                break;
            case PlayerActionType.DrawCard:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool IsCurrentPlayer()
    {
        if (MatchResponse.Match.PlayerStates[GameStateResponse.GameState.CurrentPlayerIndex].Id ==
            _backendDataControlMediator.UserDataModel.UserId)
            return true;

        return false;
    }

    public void Update()
    {
    }

    public void Dispose()
    {
    }
}

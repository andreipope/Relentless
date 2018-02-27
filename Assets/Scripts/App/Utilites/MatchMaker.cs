using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking.Match;
using CCGKit;

public class MatchMaker
{
    private static MatchMaker _instance;

    public static MatchMaker Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MatchMaker();

            return _instance;
        }
    }



#if !ENABLE_MASTER_SERVER_KIT


    public void StartMatch()
    {
        GameNetworkManager.Instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnPlayNowMatchList);
    }

    private void OnPlayNowMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        GameNetworkManager.Instance.OnMatchList(success, extendedInfo, responseData);

        var foundExistingGame = false;
        foreach (var match in responseData)
        {
            if (match.currentSize > 0)
            {
                foundExistingGame = true;
                GameNetworkManager.Instance.matchMaker.JoinMatch(match.networkId, "", string.Empty, string.Empty, 0, 0, OnPlayNowMatchJoined);
                break;
            }
        }

        if (!foundExistingGame)
        {
            GameNetworkManager.Instance.matchMaker.CreateMatch("Game room", 2, true, "", string.Empty, string.Empty, 0, 0, OnPlayNowMatchCreate);
        }
    }

    public void OnPlayNowMatchJoined(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            GameNetworkManager.Instance.OnMatchJoined(success, extendedInfo, responseData);
        }
        else
        {
            Debug.Log("The game could not be joined.");
        }
    }

    private void OnPlayNowMatchCreate(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            GameNetworkManager.Instance.OnMatchCreate(success, extendedInfo, responseData);
        }
        else
        {
                Debug.Log("The game could not be created.");
        }
    }

    private void OnMatchCreate(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            GameNetworkManager.Instance.OnMatchCreate(success, extendedInfo, responseData);
        }
        else
        {
            Debug.Log("The game could not be created.");
        }
    }

#endif
}

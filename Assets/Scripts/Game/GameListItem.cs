// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

using TMPro;

#if ENABLE_MASTER_SERVER_KIT

using MasterServerKit;

#endif

public class GameListItem : MonoBehaviour
{
    public TextMeshProUGUI gameNameText;
    public Image lockIcon;

    public BaseScene scene;
#if ENABLE_MASTER_SERVER_KIT
    public SpawnedGameNetwork matchInfo;
#else
    public MatchInfoSnapshot matchInfo;
#endif

    private void Awake()
    {
        Assert.IsNotNull(gameNameText);
        Assert.IsNotNull(lockIcon);
    }

    public void OnJoinButtonPressed()
    {
        if (lockIcon.IsActive())
        {
            scene.OpenPopup<PopupPassword>("PopupPassword", popup =>
            {
                popup.button.onClickEvent.AddListener(() => { scene.ClosePopup(); JoinMatch(popup.inputField.text); });
            });
        }
        else
        {
            JoinMatch();
        }
    }

    private void JoinMatch(string password = "")
    {
        scene.OpenPopup<PopupLoading>("PopupLoading", popup =>
        {
            popup.text.text = "Joining game...";
        });
#if ENABLE_MASTER_SERVER_KIT
        ClientAPI.JoinGameRoom(matchInfo.id, password, (ip, port) =>
        {
            ClientAPI.JoinGameServer(ip, port);
        },
        error =>
        {
            var errorMsg = "";
            switch (error)
            {
                case JoinGameRoomError.GameFull:
                    errorMsg = "This game is already full.";
                    break;

                case JoinGameRoomError.GameExpired:
                    errorMsg = "This game does not exist anymore.";
                    break;

                case JoinGameRoomError.InvalidPassword:
                    errorMsg = "Invalid password.";
                    break;
            }
            scene.ClosePopup();
            OpenAlertDialog(errorMsg);
        });
#else
        GameNetworkManager.Instance.matchMaker.JoinMatch(matchInfo.networkId, password, string.Empty, string.Empty, 0, 0, OnMatchJoined);
#endif
    }

    public void OnMatchJoined(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            GameNetworkManager.Instance.OnMatchJoined(success, extendedInfo, responseData);
        }
        else
        {
            scene.ClosePopup();
            OpenAlertDialog("The game could not be joined.");
        }
    }

    private void OpenAlertDialog(string msg)
    {
        scene.OpenPopup<PopupOneButton>("PopupOneButton", popup =>
        {
            popup.text.text = msg;
            popup.buttonText.text = "OK";
            popup.button.onClickEvent.AddListener(() => { popup.Close(); });
        });
    }
}

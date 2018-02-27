// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

using CCGKit;
using GrandDevs.CZB;

/// <summary>
/// This classes manages the game scene.
/// </summary>
public class GameScene : BaseScene
{
    public PopupChat chatPopup;

    private void Start()
    {
        OpenPopup<PopupLoading>("PopupLoading", popup =>
        {
            popup.text.text = "Waiting for game to start...";
        });
        if (GameNetworkManager.Instance.isSinglePlayer)
        {
            Invoke("AddBot", 1.5f);
        }
    }

    private void AddBot()
    {
        ClientScene.AddPlayer(1);
    }

    public void CloseWaitingWindow()
    {
        ClosePopup();
    }

    /// <summary>
    /// Callback for when the end turn button is pressed.
    /// </summary>
    public void OnEndTurnButtonPressed()
    {
        var localPlayer = NetworkingUtils.GetLocalPlayer() as DemoHumanPlayer;
        if (localPlayer != null)
        {
            //var maxHandSize = GameManager.Instance.Config.properties.maxHandSize;
            /*if (localPlayer.HandSize > maxHandSize)
            {
                var diff = localPlayer.HandSize - maxHandSize;
                if (diff == 1)
                    WindowUtils.OpenAlertDialog("You need to discard " + diff + " card from your hand.");
                else
                    WindowUtils.OpenAlertDialog("You need to discard " + diff + " cards from your hand.");
            }*/
            localPlayer.StopTurn();
        }
    }

    /// <summary>
    /// Callback for when the exit game button is pressed.
    /// </summary>
    public void OnExitGameButtonPressed()
    {
        OpenPopup<PopupTwoButtons>("PopupTwoButtons", popup =>
        {
            popup.text.text = "Do you want to leave this game?";
            popup.buttonText.text = "Yes";
            popup.button2Text.text = "No";
            popup.button.onClickEvent.AddListener(() =>
            {
                if (NetworkingUtils.GetLocalPlayer().isServer)
                {
                    GameNetworkManager.Instance.StopHost();
                }
                else
                {
                    GameNetworkManager.Instance.StopClient();
                }

                GameClient.Get<IAppStateManager>().ChangeAppState(GrandDevs.CZB.Common.Enumerators.AppState.MAIN_MENU);

            });
            popup.button2.onClickEvent.AddListener(() => 
            {
                popup.Close();
            });
        });
    }

    /// <summary>
    /// Callback for when the chat button is pressed.
    /// </summary>
    public void OnChatButtonPressed()
    {
        chatPopup.Show();
    }
}

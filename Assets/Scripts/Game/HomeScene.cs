// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

using FullSerializer;
using TMPro;

#if ENABLE_MASTER_SERVER_KIT

using System.Collections;

using MasterServerKit;

#endif

using CCGKit;

/// <summary>
/// For every scene in the demo game, we create a specific game object to handle the user-interface
/// logic belonging to that scene. The home scene just contains the button delegates that trigger
/// transitions to other scenes or exit the game.
/// </summary>
public class HomeScene : BaseScene
{
    [SerializeField]
    private TextMeshProUGUI versionText;

    private fsSerializer serializer = new fsSerializer();

    private void Awake()
    {
        Assert.IsNotNull(versionText);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        versionText.text = "Version " + CCGKitInfo.version;

        var decksPath = Application.persistentDataPath + "/decks.json";
        if (File.Exists(decksPath))
        {
            var file = new StreamReader(decksPath);
            var fileContents = file.ReadToEnd();
            var data = fsJsonParser.Parse(fileContents);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(List<Deck>), ref deserialized).AssertSuccessWithoutWarnings();
            file.Close();
            GameManager.Instance.playerDecks = deserialized as List<Deck>;
        }

        GameNetworkManager.Instance.Initialize();

#if ENABLE_MASTER_SERVER_KIT
        if (!GameManager.Instance.isPlayerLoggedIn)
        {
            StartCoroutine(LoadMSKHome());
        }
#endif
    }

#if ENABLE_MASTER_SERVER_KIT

    private IEnumerator LoadMSKHome()
    {
        var async = SceneManager.LoadSceneAsync("MSK_Home", LoadSceneMode.Additive);
        while (!async.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        ClientAPI.ConnectToMasterServer(() =>
        {
            Debug.Log("Connected to master server.");
        },
        () =>
        {
            Debug.Log("Could not connect to master server.");
        });
        OpenPopup<PopupLogin>("PopupLogin", popup =>
        {
        });
    }

#endif

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OnDecksButtonPressed()
    {
        SceneManager.LoadScene("DeckBuilder");
    }

    public void OnQuitButtonPressed()
    {
        OpenPopup<PopupTwoButtons>("PopupTwoButtons", popup =>
        {
            popup.text.text = "Do you want to quit?";
            popup.buttonText.text = "Yes";
            popup.button2Text.text = "No";
            popup.button.onClickEvent.AddListener(() => { Application.Quit(); });
            popup.button2.onClickEvent.AddListener(() => { popup.Close(); });
        });
    }
}

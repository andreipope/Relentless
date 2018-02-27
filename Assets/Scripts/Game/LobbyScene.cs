// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CCGKit;

using DG.Tweening;
using FullSerializer;
using TMPro;

#if ENABLE_MASTER_SERVER_KIT

using MasterServerKit;

#endif

public class LobbyScene : BaseScene
{
    [SerializeField]
    private CanvasGroup settingsGroup;

    [SerializeField]
    private CanvasGroup createGameGroup;

    [SerializeField]
    private CanvasGroup findGamesGroup;

    [SerializeField]
    private TMP_InputField playerNameInputField;

    [SerializeField]
    private TextMeshProUGUI currentDeckNameText;

    [SerializeField]
    private TextMeshProUGUI currentAIDeckNameText;

    [SerializeField]
    private TMP_InputField gameNameInputField;

    [SerializeField]
    private Toggle passwordToggle;

    [SerializeField]
    private TMP_InputField passwordInputField;

    [SerializeField]
    private TextMeshProUGUI numGamesText;

    [SerializeField]
    private GameObject gameListContent;

    [SerializeField]
    private GameObject gameListItemPrefab;

    private List<Deck> decks;
    private int currentDeckIndex;
    private int currentAIDeckIndex;

    private fsSerializer serializer = new fsSerializer();

    private void Awake()
    {
        Assert.IsNotNull(createGameGroup);
        Assert.IsNotNull(findGamesGroup);
        Assert.IsNotNull(playerNameInputField);
        Assert.IsNotNull(currentDeckNameText);
        Assert.IsNotNull(currentAIDeckNameText);
        Assert.IsNotNull(gameNameInputField);
        Assert.IsNotNull(passwordToggle);
        Assert.IsNotNull(passwordInputField);
        Assert.IsNotNull(numGamesText);
        Assert.IsNotNull(gameListContent);
        Assert.IsNotNull(gameListItemPrefab);
    }

    private void Start()
    {
        /*var defaultDeckTextAsset = Resources.Load<TextAsset>("DefaultDeck");
        if (defaultDeckTextAsset != null)
        {
            GameManager.Instance.defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckTextAsset.text);
        }

        var decksPath = Application.persistentDataPath + "/decks.json";
        if (File.Exists(decksPath))
        {
            var file = new StreamReader(decksPath);
            var fileContents = file.ReadToEnd();
            var data = fsJsonParser.Parse(fileContents);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(List<Deck>), ref deserialized).AssertSuccessWithoutWarnings();
            file.Close();
            decks = deserialized as List<Deck>;
        }

        playerNameInputField.text = PlayerPrefs.GetString("player_name");
        if (decks != null && decks.Count > 0)
        {
            currentDeckIndex = PlayerPrefs.GetInt("default_deck");
            if (currentDeckIndex < decks.Count)
            {
                currentDeckNameText.text = decks[currentDeckIndex].name;
                PlayerPrefs.SetInt("default_deck", currentDeckIndex);
            }
            else
            {
                currentDeckNameText.text = decks[0].name;
                PlayerPrefs.SetInt("default_deck", 0);
            }
            currentAIDeckIndex = PlayerPrefs.GetInt("default_ai_deck");
            if (currentAIDeckIndex < decks.Count)
            {
                currentAIDeckNameText.text = decks[currentAIDeckIndex].name;
                PlayerPrefs.SetInt("default_ai_deck", currentAIDeckIndex);
            }
            else
            {
                currentAIDeckNameText.text = decks[0].name;
                PlayerPrefs.SetInt("default_ai_deck", 0);
            }
        }

#if !ENABLE_MASTER_SERVER_KIT
        GameNetworkManager.Instance.StartMatchMaker();
#endif

        passwordInputField.gameObject.SetActive(passwordToggle.isOn);
*/
		//OnSettingsButtonPressed();

		GameNetworkManager.Instance.isSinglePlayer = true;
		GameNetworkManager.Instance.StartHost();
    }

    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnSettingsButtonPressed()
    {
        HideGroup(findGamesGroup);
        HideGroup(createGameGroup);
        ShowGroup(settingsGroup);
    }

    public void OnSetPlayerName()
    {
        PlayerPrefs.SetString("player_name", playerNameInputField.text);
    }

    public void OnPrevDeckButtonPressed()
    {
        if (decks != null && decks.Count > 0)
        {
            --currentDeckIndex;
            if (currentDeckIndex < 0)
            {
                currentDeckIndex = 0;
            }
            if (currentDeckIndex < decks.Count)
            {
                currentDeckNameText.text = decks[currentDeckIndex].name;
                PlayerPrefs.SetInt("default_deck", currentDeckIndex);
            }
        }
    }

    public void OnNextDeckButtonPressed()
    {
        if (decks != null && decks.Count > 0)
        {
            ++currentDeckIndex;
            if (currentDeckIndex == decks.Count)
            {
                currentDeckIndex = currentDeckIndex - 1;
            }
            if (currentDeckIndex < decks.Count)
            {
                currentDeckNameText.text = decks[currentDeckIndex].name;
                PlayerPrefs.SetInt("default_deck", currentDeckIndex);
            }
        }
    }

    public void OnPrevAIDeckButtonPressed()
    {
        if (decks != null && decks.Count > 0)
        {
            --currentAIDeckIndex;
            if (currentAIDeckIndex < 0)
            {
                currentAIDeckIndex = 0;
            }
            if (currentAIDeckIndex < decks.Count)
            {
                currentAIDeckNameText.text = decks[currentAIDeckIndex].name;
                PlayerPrefs.SetInt("default_ai_deck", currentAIDeckIndex);
            }
        }
    }

    public void OnNextAIDeckButtonPressed()
    {
        if (decks != null && decks.Count > 0)
        {
            ++currentAIDeckIndex;
            if (currentAIDeckIndex == decks.Count)
            {
                currentAIDeckIndex = currentAIDeckIndex - 1;
            }
            if (currentAIDeckIndex < decks.Count)
            {
                currentAIDeckNameText.text = decks[currentAIDeckIndex].name;
                PlayerPrefs.SetInt("default_ai_deck", currentAIDeckIndex);
            }
        }
    }

    public void OnPlayNowButtonPressed()
    {
        OpenPopup<PopupLoading>("PopupLoading", popup =>
        {
            popup.text.text = "Preparing to play...";
        });
#if ENABLE_MASTER_SERVER_KIT
        ClientAPI.PlayNow("GAMEPLAY", 2, new List<MasterServerKit.Property>(),
        (ip, port) =>
        {
            ClientAPI.JoinGameServer(ip, port);
        },
        (ip, port) =>
        {
            ClientAPI.JoinGameServer(ip, port);
        },
        error =>
        {
        });
#else
        GameNetworkManager.Instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnPlayNowMatchList);
#endif
    }

    public void OnSinglePlayerButtonPressed()
    {
        GameNetworkManager.Instance.isSinglePlayer = true;
        GameNetworkManager.Instance.StartHost();
    }

    public void OnOpenCreateGameButtonPressed()
    {
        HideGroup(settingsGroup);
        HideGroup(findGamesGroup);
        ShowGroup(createGameGroup);
    }

    public void OnCreateGameButtonPressed()
    {
        GameNetworkManager.Instance.isSinglePlayer = false;
        CreateGame();
    }

    public void OnPasswordToggleValueChanged()
    {
        passwordInputField.gameObject.SetActive(passwordToggle.isOn);
    }

    public void OnOpenFindGamesButtonPressed()
    {
        HideGroup(settingsGroup);
        HideGroup(createGameGroup);
        ShowGroup(findGamesGroup);
        GameNetworkManager.Instance.isSinglePlayer = false;
        FindGames();
    }

    public void OnFindGamesButtonPressed()
    {
        FindGames();
    }

    public void OnCreateLANGameButtonPressed()
    {
        GameNetworkManager.Instance.isSinglePlayer = false;
        GameNetworkManager.Instance.StartHost();
    }

    public void OnJoinLANGameButtonPressed()
    {
        GameNetworkManager.Instance.isSinglePlayer = false;
        GameNetworkManager.Instance.StartClient();
    }

    private void ShowGroup(CanvasGroup group)
    {
        group.DOFade(1.0f, 0.4f);
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void HideGroup(CanvasGroup group)
    {
        group.DOFade(0.0f, 0.2f);
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void CreateGame()
    {
        OpenPopup<PopupLoading>("PopupLoading", popup =>
        {
            popup.text.text = "Creating new game...";
        });
        var gameName = gameNameInputField.text;
        if (string.IsNullOrEmpty(gameName))
        {
            gameName = "New game";
        }
        var password = passwordInputField.text;
#if ENABLE_MASTER_SERVER_KIT
        ClientAPI.CreateGameRoom(gameName, 2, password, (ip, port) =>
        {
            ClientAPI.JoinGameServer(ip, port);
        },
        error =>
        {
        });
#else
        GameNetworkManager.Instance.matchMaker.CreateMatch(gameName, 2, true, password, string.Empty, string.Empty, 0, 0, OnMatchCreate);
#endif
    }

    private void FindGames()
    {
        OpenPopup<PopupLoading>("PopupLoading", popup =>
        {
            popup.text.text = "Finding games...";
        });
#if ENABLE_MASTER_SERVER_KIT
        var includeProperties = new List<MasterServerKit.Property>();
        var excludeProperties = new List<MasterServerKit.Property>();
        ClientAPI.FindGameRooms(includeProperties, excludeProperties, matches =>
        {
            ClosePopup();

            foreach (Transform child in gameListContent.transform)
            {
                Destroy(child.gameObject);
            }

            numGamesText.text = matches.Length.ToString();
            foreach (var match in matches)
            {
                if (match.numPlayers > 0)
                {
                    var go = Instantiate(gameListItemPrefab) as GameObject;
                    go.transform.SetParent(gameListContent.transform, false);
                    var gameListItem = go.GetComponent<GameListItem>();
                    gameListItem.gameNameText.text = match.name;
                    gameListItem.lockIcon.gameObject.SetActive(match.isPrivate);
                    gameListItem.scene = this;
                    gameListItem.matchInfo = match;
                }
            }
        });
#else
        GameNetworkManager.Instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnMatchList);
#endif
    }

#if !ENABLE_MASTER_SERVER_KIT

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
            ClosePopup();
            OpenPopup<PopupOneButton>("PopupOneButton", popup =>
            {
                popup.text.text = "The game could not be joined.";
                popup.buttonText.text = "OK";
                popup.button.onClickEvent.AddListener(() => { popup.Close(); });
            });
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
            ClosePopup();
            OpenPopup<PopupOneButton>("PopupOneButton", popup =>
            {
                popup.text.text = "The game could not be created.";
                popup.buttonText.text = "OK";
                popup.button.onClickEvent.AddListener(() => { popup.Close(); });
            });
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
            ClosePopup();
            OpenPopup<PopupOneButton>("PopupOneButton", popup =>
            {
                popup.text.text = "The game could not be created.";
                popup.buttonText.text = "OK";
                popup.button.onClickEvent.AddListener(() => { popup.Close(); });
            });
        }
    }

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        ClosePopup();

        GameNetworkManager.Instance.OnMatchList(success, extendedInfo, responseData);

        foreach (Transform child in gameListContent.transform)
        {
            Destroy(child.gameObject);
        }

        numGamesText.text = responseData.Count.ToString();
        foreach (var match in responseData)
        {
            if (match.currentSize > 0)
            {
                var go = Instantiate(gameListItemPrefab) as GameObject;
                go.transform.SetParent(gameListContent.transform, false);
                var gameListItem = go.GetComponent<GameListItem>();
                gameListItem.gameNameText.text = match.name;
                gameListItem.lockIcon.gameObject.SetActive(match.isPrivate);
                gameListItem.scene = this;
                gameListItem.matchInfo = match;
            }
        }
    }

#endif
}

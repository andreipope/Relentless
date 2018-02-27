// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

using TMPro;

using CCGKit;

public class PopupChat : MonoBehaviour
{
    public ScrollRect scrollView;
    public GameObject scrollViewContent;
    public GameObject textLinePrefab;
    public TMP_InputField inputField;
    public Color playerTextColor = new Color(66 / 256.0f, 137 / 256.0f, 166 / 256.0f);
    public Color opponentTextColor = new Color(183 / 256.0f, 86 / 256.0f, 93 / 256.0f);

    private static readonly int maxChatMessageLength = 50;

    private CanvasGroup canvasGroup;

    public bool isVisible { get; private set; }

    private void Awake()
    {
        Assert.IsNotNull(scrollView);
        Assert.IsNotNull(scrollViewContent);
        Assert.IsNotNull(textLinePrefab);
        Assert.IsNotNull(inputField);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        inputField.ActivateInputField();
    }

    public void OnCloseButtonPressed()
    {
        Hide();
    }

    public void OnSendButtonPressed()
    {
        SubmitText();
    }

    public void OnChatInputFieldEditEnded()
    {
        // It seems Unity's InputField OnEndEdit event is called in a lot of contexts
        // other than submitting the text from an input field (e.g, clicking on a
        // scrollbar), so make sure we got here only by pressing Enter on an input
        // field.
        if (!Input.GetButtonDown("Submit"))
        {
            return;
        }

        SubmitText();
    }

    public void SubmitText()
    {
        var localPlayer = NetworkingUtils.GetHumanLocalPlayer();
        if (localPlayer != null)
        {
            var msg = new SendChatTextMessage();
            msg.senderNetId = localPlayer.netId;
            msg.text = inputField.text;
            if (msg.text.Length > maxChatMessageLength)
            {
                msg.text = msg.text.Substring(0, maxChatMessageLength);
            }
            NetworkManager.singleton.client.Send(NetworkProtocol.SendChatTextMessage, msg);
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }
    }

    public void SendText(NetworkInstanceId senderNetId, string text)
    {
        var go = Instantiate(textLinePrefab) as GameObject;
        go.transform.SetParent(scrollViewContent.transform, false);
        go.GetComponent<TextMeshProUGUI>().text = text;
        scrollView.velocity = new Vector2(0.0f, 1000.0f);
        var localPlayer = NetworkingUtils.GetHumanLocalPlayer();
        if (senderNetId == localPlayer.netId)
        {
            go.GetComponent<TextMeshProUGUI>().color = playerTextColor;
        }
        else
        {
            go.GetComponent<TextMeshProUGUI>().color = opponentTextColor;
        }
    }

    public void Show()
    {
        isVisible = true;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        isVisible = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

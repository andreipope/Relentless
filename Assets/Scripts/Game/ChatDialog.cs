// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CCGKit
{
    /// <summary>
    /// Holds information about the in-game chat dialog.
    /// </summary>
    public class ChatDialog : MonoBehaviour
    {
        /// <summary>
        /// Chat scroll view.
        /// </summary>
        public ScrollRect ChatScrollView;

        /// <summary>
        /// Chat scroll view content.
        /// </summary>
        public GameObject ChatScrollViewContent;

        /// <summary>
        /// Chat entry prefab.
        /// </summary>
        public GameObject ChatEntryPrefab;

        /// <summary>
        /// Input field component.
        /// </summary>
        public InputField InputField;

        /// <summary>
        /// Maximum length (in characters) allowed for a single chat message.
        /// </summary>
        private static readonly int maxChatMessageLength = 50;

        private void Awake()
        {
            Assert.IsTrue(ChatScrollView != null);
            Assert.IsTrue(ChatScrollViewContent != null);
            Assert.IsTrue(ChatEntryPrefab != null);
            Assert.IsTrue(InputField != null);
        }

        private void Start()
        {
            InputField.ActivateInputField();
        }

        /// <summary>
        /// Send button callback.
        /// </summary>
        public void OnSendButtonPressed()
        {
            SubmitText();
        }

        /// <summary>
        /// Close button callback.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Hide();
        }

        /// <summary>
        /// Chat input field
        /// </summary>
        public void OnChatInputFieldEditEnded()
        {
            // It seems Unity's InputField OnEndEdit event is called in a lot of contexts
            // other than submitting the text from an input field (e.g, clicking on a
            // scrollbar), so make sure we got here only by pressing Enter on an input
            // field.
            if (!Input.GetButtonDown("Submit"))
                return;

            SubmitText();
        }

        /// <summary>
        /// Performs the actual work of submitting the chat text.
        /// </summary>
        public void SubmitText()
        {
            var localPlayer = NetworkingUtils.GetHumanLocalPlayer();
            if (localPlayer != null)
            {
                var msg = new SendChatTextMessage();
                msg.senderNetId = localPlayer.netId;
                msg.text = InputField.text;
                if (msg.text.Length > maxChatMessageLength)
                    msg.text = msg.text.Substring(0, maxChatMessageLength);
                NetworkManager.singleton.client.Send(NetworkProtocol.SendChatTextMessage, msg);
                InputField.text = string.Empty;
                InputField.ActivateInputField();
            }
        }

        /// <summary>
        /// Adds the specified text to the chat dialog.
        /// </summary>
        /// <param name="text">Text to add to the chat dialog.</param>
        public void AddTextEntry(string text)
        {
            var go = Instantiate(ChatEntryPrefab) as GameObject;
            go.transform.SetParent(ChatScrollViewContent.transform, false);
            go.GetComponent<Text>().text = text;
            ChatScrollView.velocity = new Vector2(0.0f, 1000.0f);
        }

        /// <summary>
        /// Shows the chat dialog.
        /// </summary>
        public void Show()
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 1;
            gameObject.GetComponent<CanvasGroup>().interactable = true;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        /// <summary>
        /// Hides the chat dialog.
        /// </summary>
        public void Hide()
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 0;
            gameObject.GetComponent<CanvasGroup>().interactable = false;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
}

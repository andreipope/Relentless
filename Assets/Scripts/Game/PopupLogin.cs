// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if ENABLE_MASTER_SERVER_KIT

using MasterServerKit;
using TMPro;

using CCGKit;

public class PopupLogin : Popup
{
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;

    public void OnSignupButtonPressed()
    {
        parentScene.OpenPopup<PopupSignup>("PopupSignup", popup =>
        {
        });
    }

    public void OnLoginButtonPressed()
    {
        var usernameText = usernameInputField.text;
        var passwordText = passwordInputField.text;

        // Perform some basic validation of the user input locally prior to calling the
        // remote login method. This is a good way to avoid some unnecessary network
        // traffic.
        if (string.IsNullOrEmpty(usernameText))
        {
            OpenAlertDialog("Please enter your username.");
            return;
        }

        if (string.IsNullOrEmpty(passwordText))
        {
            OpenAlertDialog("Please enter your password.");
            return;
        }

        ClientAPI.Login(usernameText, passwordText,
            () =>
            {
                GameManager.Instance.isPlayerLoggedIn = true;
                GameManager.Instance.playerName = ClientAPI.masterServerClient.username;
                Close();
            },
            error =>
            {
                var errorMsg = "";
                switch (error)
                {
                    case LoginError.DatabaseConnectionError:
                        errorMsg = "There was an error connecting to the database.";
                        break;

                    case LoginError.NonexistingUser:
                        errorMsg = "This user does not exist.";
                        break;

                    case LoginError.InvalidCredentials:
                        errorMsg = "Invalid credentials.";
                        break;

                    case LoginError.ServerFull:
                        errorMsg = "The server is full.";
                        break;

                    case LoginError.AuthenticationRequired:
                        errorMsg = "Authentication is required.";
                        break;

                    case LoginError.UserAlreadyLoggedIn:
                        errorMsg = "This user is already logged in.";
                        break;
                }
                OpenAlertDialog(errorMsg);
            });
    }

    private void OpenAlertDialog(string msg)
    {
        parentScene.OpenPopup<PopupOneButton>("PopupOneButton", popup =>
        {
            popup.text.text = msg;
            popup.buttonText.text = "OK";
            popup.button.onClickEvent.AddListener(() => { popup.Close(); });
        });
    }
}

#else

public class PopupLogin : Popup
{
}

#endif

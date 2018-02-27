// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if ENABLE_MASTER_SERVER_KIT

using System.Text.RegularExpressions;

using MasterServerKit;
using TMPro;

public class PopupSignup : Popup
{
    public TMP_InputField emailInputField;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField passwordRepeatInputField;

    public void OnSignupButtonPressed()
    {
        var emailText = emailInputField.text;
        var usernameText = usernameInputField.text;
        var passwordText = passwordInputField.text;
        var passwordRepeatText = passwordRepeatInputField.text;

        // Perform some basic validation of the user input locally prior to calling the
        // remote registration method. This is a good way to avoid some unnecessary
        // network traffic.
        if (string.IsNullOrEmpty(emailText))
        {
            OpenAlertDialog("Please enter your email address.");
            return;
        }

        if (!IsValidEmail(emailText))
        {
            OpenAlertDialog("The email address you entered is not valid.");
            return;
        }

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

        if (string.IsNullOrEmpty(passwordRepeatText))
        {
            OpenAlertDialog("Please enter your password again.");
            return;
        }

        if (passwordText != passwordRepeatText)
        {
            OpenAlertDialog("The passwords do not match.");
            return;
        }

        ClientAPI.Register(emailText, usernameText, passwordText,
            () =>
            {
                OpenAlertDialog("Welcome!");
                Close();
            },
            error =>
            {
                var errorMsg = "";
                switch (error)
                {
                    case RegistrationError.DatabaseConnectionError:
                        errorMsg = "There was an error connecting to the database.";
                        break;

                    case RegistrationError.MissingEmailAddress:
                        errorMsg = "You need to enter an email address.";
                        break;

                    case RegistrationError.MissingUsername:
                        errorMsg = "You need to enter a username.";
                        break;

                    case RegistrationError.MissingPassword:
                        errorMsg = "You need to enter a password.";
                        break;

                    case RegistrationError.AlreadyExistingEmailAddress:
                        errorMsg = "This email adress is already registered.";
                        break;

                    case RegistrationError.AlreadyExistingUsername:
                        errorMsg = "This username is already registered.";
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

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email,
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
            RegexOptions.IgnoreCase);
    }
}

#else

public class PopupSignup : Popup
{
}

#endif

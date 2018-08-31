using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	/// <summary>
	/// This class implements a popup that asks the user for their password.
	/// </summary>
	class CommandSecurityPopup
	{
		private readonly ConsoleCommandSecurity _consoleCommandSecurity;
		private readonly CommandHandler _commandToExecute;
		private readonly string[] _parameters;
		private string _enteredPassword = "";
		private float _incorrectPasswordEnteredTimeout = 0;
		private GUISkin _skin;
		private float _screenScaleFactor;

		public CommandSecurityPopup(ConsoleCommandSecurity consoleCommandSecurity, CommandHandler commandToExecute, string[] parameters)
		{
			_consoleCommandSecurity = consoleCommandSecurity;
			_commandToExecute = commandToExecute;
			_parameters = parameters;
		}

		public void OnGUI()
		{
			if (_skin == null)
			{
				CreateSkin();
			}
			
			int width = (int) Mathf.Min(300 * _screenScaleFactor, Screen.width);
			int height = (int) (140 * _screenScaleFactor);
			var windowId = typeof (CommandSecurityPopup).GetHashCode();
			
			GUI.ModalWindow(windowId,
				new Rect(Screen.width/2 - width/2, Screen.height/2 - height/2, width, height), WindowFunc, "", _skin.window);
			
		}

		private void CreateSkin()
		{
			var dpi = Screen.dpi == 0 ? 140 : Screen.dpi;
			_screenScaleFactor = dpi/140;
			int defaultFontSize = 13;
			int scaledFontSize = (int) (defaultFontSize*_screenScaleFactor);

			// Clone the default skin
			_skin = (GUISkin) Object.Instantiate(GUI.skin);

			// The default window style is transparent, which makes the text really hard to read.
			_skin.window = new GUIStyle(GUI.skin.window)
			{
				normal = {background = Shared.Utils.UIUtilities.CreateTexture(new Color(0.2f, 0.2f, 0.2f, 1.0f))},
				padding = new RectOffset((int) (10 * _screenScaleFactor), (int) (10 * _screenScaleFactor), (int) (20 * _screenScaleFactor), (int) (10 * _screenScaleFactor)),
				fontSize = (int)(defaultFontSize * _screenScaleFactor)
			};

			_skin.label.fontSize = scaledFontSize;
			_skin.button.fontSize = scaledFontSize;
			_skin.textField.fontSize = scaledFontSize;
		}

		private void WindowFunc(int id)
		{
			var oldSkin = GUI.skin;
			GUI.skin = _skin;

			GUILayout.Label("This command requires a password to execute.");

			var oldColor = GUI.color;
			GUI.color = Color.red;
			GUILayout.Label(Time.realtimeSinceStartup < _incorrectPasswordEnteredTimeout ? "Incorrect password entered, try again." : " ");
			GUI.color = oldColor;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Password:", GUILayout.Width(100 * _screenScaleFactor));
			// The default text field expands as the user enters text, which is a bit odd. This fixes that.
			var rect = GUILayoutUtility.GetRect(100 * _screenScaleFactor, 10000 * _screenScaleFactor, 0, 20 * _screenScaleFactor, GUI.skin.textField);
			_enteredPassword = GUI.PasswordField(rect, _enteredPassword, '*');
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Cancel"))
			{
				_consoleCommandSecurity.PopupClosed(false);
			}

			if (GUILayout.Button("Login"))
			{
				if (_enteredPassword == ConsoleCommandSecurity.PASSWORD)
				{
					Debug.Log("Logged in using password");
					_consoleCommandSecurity.PopupClosed(true);
					
					if (_consoleCommandSecurity._SavePassword)
					{
						PlayerPrefs.SetString("Opencoding.Console.ConsoleCommandSecurity.Password", _enteredPassword);
						PlayerPrefs.Save();
					}

					// This executes the original command that the user entered with the parameter they specified
					_commandToExecute.Invoke(_parameters);
				}
				else
				{
					// Show the error message for 3 seconds
					_incorrectPasswordEnteredTimeout = Time.realtimeSinceStartup + 3;
				}
			}
			GUILayout.EndHorizontal();
			
			GUI.skin = oldSkin;
		}
	}
}
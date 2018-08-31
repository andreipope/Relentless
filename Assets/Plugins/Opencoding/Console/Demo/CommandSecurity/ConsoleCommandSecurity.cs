using Opencoding.CommandHandlerSystem;
using Opencoding.Console;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	/// <summary>
	/// This class implements a very basic security system. It's not intended to stand up to any real
	/// attempt to hack the game, it's more to just an example of how you could protect yourself from 
	/// your testers executing commands that might mess up internal tests, public betas etc.
	/// You could modify this to implement a more advanced system if you wish!
	/// </summary>
	public class ConsoleCommandSecurity : MonoBehaviour
	{
		private bool _authenticated = false;
		private CommandSecurityPopup _commandSecurityPopup;
		public const string PASSWORD = "password"; // You might want something slightly more secure!
		public bool _SavePassword = false; // If this is set, the password is saved in PlayerPrefs.
 
		private void Awake()
		{
			// This hook is called just before each command is executed. If the attached method returns 
			// true, then the command is executed, otherwise it isn't.
			CommandHandlers.BeforeCommandExecutedHook += BeforeCommandExecuted;
			DontDestroyOnLoad(gameObject);

			if (_SavePassword)
			{
				if (PlayerPrefs.GetString("Opencoding.Console.ConsoleCommandSecurity.Password") == PASSWORD)
					_authenticated = true;
			}
		}

		private void OnGUI()
		{
			if(_commandSecurityPopup != null)
				_commandSecurityPopup.OnGUI();
		}

		private bool BeforeCommandExecuted(CommandHandler commandHandler, string[] strings)
		{
			if (_authenticated)
				return true; // already authenticated - execute the command as normal

			// Check to see if the CommandHandler method or property has the SecureCommandHandler attribute on it
			bool isSecureCommandHandler =
				commandHandler.MemberInfo.GetCustomAttributes(typeof (SecureCommandHandlerAttribute), false).Length != 0;
			
			if (!isSecureCommandHandler)
				return true; // not a 'secured' command - execute the command as normal

			if (_commandSecurityPopup == null)
			{
				_commandSecurityPopup = new CommandSecurityPopup(this, commandHandler, strings);
			}
			return false; // prevent the command executing
		}

		public void PopupClosed(bool authenticated)
		{
			_commandSecurityPopup = null;
			_authenticated = authenticated;
		}
	}
}
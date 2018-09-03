using Opencoding.CommandHandlerSystem;
using Opencoding.Console;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Opencoding.Demo.CommandSecurity
{
	public class ExampleCommands : MonoBehaviour
	{
		private void Awake()
		{
			CommandHandlers.RegisterCommandHandlers(this);
		}

		[CommandHandler(Description = "This is a secured command")]
		[SecureCommandHandler]		
		private void SecureCommand()
		{
			Debug.Log("This is a secure command");
		}

		[CommandHandler(Description = "This is an insecure command - anyone can execute it")]
		private void InsecureCommand()
		{
			Debug.Log("This is an insecure command - you don't need a password to execute it");
		}

		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
			
			var instructionLabelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = Screen.dpi > 0 ? (int)(Screen.dpi * 0.1f) : 20,
				wordWrap = true,
				normal = new GUIStyleState() { textColor = Color.black }
			};

			if (DebugConsole.Instance != null && !DebugConsole.IsVisible)
			{
				GUILayout.Label("This scene provides a demo of how you can implement a very basic security system so users can't execute every command. This is useful for public betas etc.\n\nThe default password is 'password'.", instructionLabelStyle, GUILayout.ExpandWidth(true));
				GUILayout.FlexibleSpace();
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				GUILayout.Label("To open the console, swipe down with two fingers.", instructionLabelStyle, GUILayout.ExpandWidth(true));
#else
				GUILayout.Label("To open the console, press the 'tilde' key (~).", instructionLabelStyle, GUILayout.ExpandWidth(true));
#endif
			}
			GUILayout.EndArea();
		}
	}
}
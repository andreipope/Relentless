using System.Collections.Generic;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	static class BallGameGeneralCommandHandlers
	{
		public static void Initialize()
		{
			CommandHandlers.RegisterCommandHandlers(typeof(BallGameGeneralCommandHandlers));	
		}

		[CommandHandler(Description = "This is a property that changes the strength of gravity")]
		private static float Gravity
		{
			get { return Physics2D.gravity.y; }
			set { Physics2D.gravity = new Vector3(0, value); }
		}

		[CommandHandler(Description = "Change the colour of the background. Predefined named colours are available.")]
		private static void SetBackgroundColor(Color color)
		{
			Camera.main.backgroundColor = color;
		}

		[CommandHandler(Description = "This command is an example of custom auto-completion. It doesn't do anything.")]
		private static void LoadLevel([Autocomplete(typeof(DemoController), "LevelAutocomplete")] string levelName)
		{
			Debug.Log("This is a fake command to demonstrate auto-completion. Imagine you've loaded the level '" + levelName + "'!");
		}

		public static IEnumerable<string> LevelAutocomplete()
		{
			return new[] { "Map", "Main Menu", "Level1", "Level2", "Level3", "Intro Movie", "Game Over Screen" };
		}

		[CommandHandler]
		private static void ChangeColor([Autocomplete(typeof(BallGameGeneralCommandHandlers), "ColorAutoComplete")] string color)
		{
			Debug.Log("ChangeColor executed: " + color);
		}

		public static IEnumerable<string> ColorAutoComplete()
		{
			return new[] {"Red", "Green", "Blue"};
		}

		[CommandHandler]
		private static void ChangeText([Autocomplete(typeof(BallGameGeneralCommandHandlers), "TextAutoComplete")] string text)
		{
			Debug.Log("ChangeText executed: " + text);
		}

		public static IEnumerable<string> TextAutoComplete()
		{
			return new[] { "This", "Is", "Text" };
		}
	}
}

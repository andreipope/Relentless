using System;
using System.Collections.Generic;
using System.IO;
using Opencoding.CommandHandlerSystem;
using Opencoding.Console;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	public class DemoController : MonoBehaviour
	{
		[SerializeField]
		private GameObject _sphere;

		private int _numberOfBallsCreated = 0;

		private bool _isConsoleVisible;

		private void Awake()
		{
			CommandHandlers.RegisterCommandHandlers(this);

			// This allows you to return your save file so that it can be attached to log emails.
			// In your own game, you might serialize out your save file, then return the contents of it as a string here.
			// Of course, you can also return any information you file helpful for debugging here.
			DebugConsole.SaveFileProvider += () => new[]
			{
			    new SaveFileData("save.json", "{\"test\": 42}", SaveFileDataType.JSON),
                new SaveFileData("info.txt", "This is some plain-text data", SaveFileDataType.TEXT)
			};

			// Use this to add information to the 'info' tab of the log emails.
			DebugConsole.GameInfoProvider += () =>
			{
				return new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Number of balls created", _numberOfBallsCreated.ToString()),
					new KeyValuePair<string, string>("Sky color", Camera.main.backgroundColor.ToString()),
					new KeyValuePair<string, string>("Gravity", Physics.gravity.ToString())
				};
			};
		}

#if !UNITY_WEBPLAYER
		private void Start()
		{
			// This is an example of how you can customise the email sending.
			// This allows you to attach extra files or information that may be useful for debugging errors (save files, screenshots etc).
			DebugConsole.CompleteLogEmailPreprocessor += email =>
			{
				File.WriteAllText(Application.persistentDataPath + "/save.dat", "Imaginary Save File");
				email.Attachments.Add(new Email.Attachment(Application.persistentDataPath + "/save.dat", "application/octet-stream"));
				email.Message += "\n\nYour imaginary save file is attached!";
				return true;
			};
		}
#endif

		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
			GUILayout.FlexibleSpace();
			var instructionLabelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = Screen.dpi > 0 ? (int)(Screen.dpi * 0.1f) : 20,
				wordWrap = true,
				normal = new GUIStyleState() { textColor = Color.black }
			};

			if (!_isConsoleVisible)
			{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				GUILayout.Label("To open the console, swipe down with two fingers.", instructionLabelStyle, GUILayout.ExpandWidth(true));
#else
				GUILayout.Label("To open the console, press the 'tilde' key (~).", instructionLabelStyle, GUILayout.ExpandWidth(true));
#endif
			}
			GUILayout.EndArea();
		}

		private void Update()
		{
		    Debug.Log("Test" + new String(' ', 65535));
			_isConsoleVisible = DebugConsole.Instance != null && DebugConsole.IsVisible;
		}

		private void OnDestroy()
		{
			CommandHandlers.UnregisterCommandHandlers(this);
		}

		[CommandHandler(Description = "This command creates a sphere just above the ground")]
		private void CreateBall()
		{
			Debug.Log("Created ball");
			var ball = (GameObject)Instantiate(_sphere, new Vector3(0, 1, 0), Quaternion.identity);
			ball.SetActive(true);
			_numberOfBallsCreated++;
		}

		[CommandHandler(Description = "This command creates a sphere just above the ground")]
		private bool Test
		{
			get; set; }

		[CommandHandler(Description = "You can enter Vectors by entering numbers separated by commas (e.g. 1,2,1)")]
		private void CreateBallAtPosition(Vector3 position)
		{
			var ball = (GameObject)Instantiate(_sphere, position, Quaternion.identity);
			ball.SetActive(true);
			_numberOfBallsCreated++;
		}

		[CommandHandler(Description = "Change the colour of the sky. Predefined named colours are available.")]
		private void SetSkyColor(Color color)
		{
			Camera.main.backgroundColor = color;
		}

		[CommandHandler(Description = "Throws an exception to test how the console handles displays it.")]
		private void ThrowException()
		{
			throw new InvalidOperationException("This is a test exception");
		}
		 
		[CommandHandler]
		private void LoremIpsum()
		{
			Debug.Log("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus facilisis, nisl id venenatis lacinia, diam metus eleifend felis, ac aliquam lacus sapien eget justo. Donec auctor consequat posuere. Integer efficitur blandit accumsan. Cras vehicula dictum sapien, sed ultricies leo laoreet nec. Integer egestas ex tempus metus vulputate lacinia. Cras ut rutrum leo, quis ornare tellus. Sed dolor odio, volutpat a dignissim a, eleifend vel urna. Nam pellentesque enim ac mauris eleifend, et ultrices augue mollis. Etiam eget neque facilisis, tempor odio nec, feugiat turpis.\n\nEtiam nunc augue, finibus id consequat id, iaculis ac diam. Pellentesque condimentum dolor id quam finibus, id ultrices felis finibus. Proin faucibus nibh vitae sapien aliquet, sit amet sagittis nunc viverra. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In vestibulum eleifend pulvinar. Phasellus at congue erat. Fusce at orci egestas, congue est posuere, facilisis ante. Donec semper justo non porta hendrerit. Cras interdum, ipsum non interdum vulputate, sem ipsum imperdiet felis, eget euismod orci tortor vitae nunc.\n\nNam ornare tempor nisi sed posuere. Etiam gravida porttitor fringilla. Donec id dui lacinia, elementum magna et, posuere metus. Proin sed neque risus. Curabitur nunc tortor, rhoncus ac varius a, pellentesque sit amet elit. Duis tempor rhoncus leo, eu lacinia ligula consectetur in. Praesent ut pellentesque ligula. Curabitur tempus mauris lobortis semper hendrerit. Proin mattis, quam nec elementum placerat, arcu erat posuere lacus, ac lobortis est magna quis neque. Sed at mauris sed libero facilisis mattis. Maecenas eget elementum ex. Maecenas vitae sollicitudin est, a elementum sem. Fusce hendrerit felis sit amet pellentesque dapibus.\n\nNullam at maximus quam. Etiam id metus accumsan, condimentum enim sed, malesuada est. Nulla ut urna consequat, porttitor ligula at, malesuada turpis. Quisque egestas lectus non pretium tincidunt. Ut erat ligula, dignissim a pellentesque nec, convallis in orci. Vivamus eu efficitur turpis. Pellentesque luctus ex nec feugiat pharetra. Duis cursus nulla non quam sollicitudin dictum.\n\nMaecenas mauris mauris, tempus sit amet ligula eget, convallis congue arcu. Morbi vel posuere ante. Integer justo odio, tristique ac lorem at, congue congue velit. Maecenas mattis magna lacus, eget rhoncus quam gravida eu. Vestibulum vitae tristique tellus. Etiam sit amet ligula quis arcu tempor laoreet eu eu erat. Nunc ultrices ligula ipsum. Ut eget purus imperdiet, laoreet neque ac, ultrices nunc. Morbi urna nisi, maximus vitae gravida vel, luctus et turpis. Etiam id vestibulum ligula. Quisque tempus sit amet nulla sit amet fermentum. Aenean luctus enim id ex suscipit, eu maximus tortor accumsan. Sed aliquam ex sit amet erat aliquet, vitae finibus ipsum laoreet. Nam luctus in tortor id tristique. Phasellus placerat neque a nibh scelerisque dignissim. Donec in urna ipsum.");
		}

		[CommandHandler(Description = "This is a method that demonstrates that optional arguments work as you would expect")]
		private void OptionalArgumentExample(float c = 50)
		{

		}

		[CommandHandler(Description = "This is a property that changes the strength of gravity")]
		private float Gravity
		{
			get
			{
				return Physics.gravity.y;
			}

			set
			{
				Physics.gravity = new Vector3(0, value, 0);
			}
		}

		[CommandHandler]
		private void LoadLevel([Autocomplete(typeof(DemoController), "LevelAutocomplete")] string levelName)
		{
			Debug.Log("This is a fake command to demonstrate auto-completion. Imagine you've loaded the level '" + levelName + "'!");
		}

		public static IEnumerable<string> LevelAutocomplete()
		{
			return new[] {"Map", "Main Menu", "Level1", "Level2", "Level3", "Intro Movie", "Game Over Screen"};
		}
	}
}
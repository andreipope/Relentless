using UnityEngine;

namespace Opencoding.Console.Demo
{
	static class BallGameSaveSystem
	{
		public static int BallsCollected { get; set; }
		public static int MaxLevelReached { get; set; }

		private static void Load()
		{
			BallsCollected = PlayerPrefs.GetInt("OpencodingConsoleDemo/BallsCollected", 0);
			MaxLevelReached = PlayerPrefs.GetInt("OpencodingConsoleDemo/MaxLevelReached", 0);
		}

		public static void Save()
		{
			PlayerPrefs.SetInt("OpencodingConsoleDemo/BallsCollected", BallsCollected);
			PlayerPrefs.SetInt("OpencodingConsoleDemo/MaxLevelReached", MaxLevelReached);
		}

		public static string AsString()
		{
			return "Balls Collected: " + BallsCollected + "\n" +
			       "Max Level Reached: " + MaxLevelReached;
		}
	}
}

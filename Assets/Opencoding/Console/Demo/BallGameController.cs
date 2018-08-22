using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Opencoding.CommandHandlerSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Opencoding.Console.Demo
{
	class BallGameController : MonoBehaviour
	{
		public int Level { get; private set; }
		public int BallsDestroyedThisLevel { get; private set; }
		private int _turnsLeft;

		[CommandHandler]
		public int TurnsLeft
		{
			get { return _turnsLeft; }
			private set
			{
				if (value > TotalTurns)
					throw new InvalidOperationException("TurnsLeft can't be greater than TotalTurns");
				_turnsLeft = value;
			}
		}

		[CommandHandler]
		public int InitialBalls { get; private set; }

		public int TotalTurns { get; private set; }
		public int BallTargetThisLevel { get; private set; }
		public bool GameIsOver { get; private set; }

		[SerializeField] private GameObject _ballPrefab;
		[SerializeField] private BallSpawner[] _ballSpawners;
		[SerializeField] private BallGameGameOverUI _gameOverUI;
		[SerializeField] private BallGameHUD _hud;

		private readonly HashSet<DemoBall> _balls = new HashSet<DemoBall>();

        private void Start()
		{
			Application.targetFrameRate = 60;
			InitialBalls = 80;

			SetupDebugConsole();

			// This is a static class that contains some tweaks
			BallGameGeneralCommandHandlers.Initialize();

			// Register the command handlers in this instance (i.e. non-static command handlers)
			CommandHandlers.RegisterCommandHandlers(this);

			_gameOverUI.Setup(this);
			_hud.Setup(this);

			_ballPrefab.SetActive(false);

			foreach (var ballSpawner in _ballSpawners)
			{
				ballSpawner.Setup(this, _ballPrefab);
			}

			BeginGame();
		}

	    private void OnDestroy()
		{
			CommandHandlers.UnregisterCommandHandlers(this);
		}

		private void SetupDebugConsole()
		{
			// Adds the save to the 'save' tab of the log emails. This also supports binary data, if need be.
			DebugConsole.SaveFileProvider += () => new[]
            {
                new SaveFileData("save.txt", BallGameSaveSystem.AsString(), SaveFileDataType.TEXT),
                new SaveFileData("data.json", "{\"health\": 42, \"player_name\": \"Player17\"}", SaveFileDataType.JSON)
            };

            // Use this to add information to the 'info' tab of the log emails.
            DebugConsole.GameInfoProvider += () => new List<KeyValuePair<string, string>>
			{
			    new KeyValuePair<string, string>("Current level", Level.ToString()),
			    new KeyValuePair<string, string>("Turns left", TurnsLeft.ToString()),
			    new KeyValuePair<string, string>("Is game over?", GameIsOver.ToString())
			};
		}

		private void BeginGame()
		{
			GameIsOver = false;
			Level = 1;
			SpawnBalls(InitialBalls);
			BeginRound();
		}

		public void RegisterBall(DemoBall demoBall)
		{
			_balls.Add(demoBall);
		}

		public void BallCollected(DemoBall ball)
		{
			_balls.Remove(ball);
		}

		private void BeginRound()
		{
			TotalTurns = 5 + (10 - Level);
			TurnsLeft = TotalTurns;
			BallsDestroyedThisLevel = 0;
			BallTargetThisLevel = 5 + Level * 7;
		}

		private void EndOfRound()
		{
			Debug.Log("Round " + Level + " over");
			Level++;
			BallGameSaveSystem.MaxLevelReached = Mathf.Max(BallGameSaveSystem.MaxLevelReached, Level);
			int ballsToSpawn = 18 + Mathf.Clamp((8 - Level) * 2, 0, 20);
			SpawnBalls(ballsToSpawn);
			BeginRound();
		}

		[CommandHandler(Description = "Spawn the specified number of balls")]
		private void SpawnBalls(int ballsToSpawn)
		{
			Debug.Log(string.Format("Spawning {0} balls", ballsToSpawn));

			foreach (var ballSpawner in _ballSpawners)
			{
				ballSpawner.SpawnBalls(ballsToSpawn / _ballSpawners.Length);
			}
		}

		public void DestroyAdjacentMatchingBalls(DemoBall demoBall)
		{
			if (DebugConsole.IsVisible)
				return;

			if (GameIsOver)
				return;

			int colorIndex = demoBall.BallColorIndex;

			Debug.Log("Clicked on ball with color " + colorIndex);

			float limitSquared = Mathf.Pow(1.1f, 2);
			HashSet<DemoBall> matchedBalls = new HashSet<DemoBall> {demoBall};

			int matchedBallCount;
			do
			{
				matchedBallCount = matchedBalls.Count;
				FindAdjacentMatchingBalls(matchedBalls, colorIndex, limitSquared);
			} while (matchedBallCount != matchedBalls.Count);

			Debug.Log(string.Format("Destroying {0} balls", matchedBalls.Count));

			foreach (var ball in matchedBalls)
			{
				Destroy(ball.gameObject);
				BallsDestroyedThisLevel++;
				BallGameSaveSystem.BallsCollected++;
			}

			if (BallsDestroyedThisLevel >= BallTargetThisLevel)
			{
				EndOfRound();
			}
			else
			{
				TurnsLeft--;
				if (TurnsLeft == 0)
				{
					EndGame();
				}
			}
		}

		private void FindAdjacentMatchingBalls(HashSet<DemoBall> matchedBalls, int colorIndex, float limitSquared)
		{
			foreach (var ball in _balls)
			{
				if (matchedBalls.Contains(ball))
					continue;

				if (ball.BallColorIndex != colorIndex)
					continue;

				bool adjacentToMatched = false;
				foreach (var matchedBall in matchedBalls)
				{
					if (Vector3.SqrMagnitude(matchedBall.transform.position - ball.transform.position) < limitSquared)
					{
						adjacentToMatched = true;
						break;
					}
				}

				if (adjacentToMatched)
				{
					matchedBalls.Add(ball);
				}
			}
		}

		[CommandHandler]
		private void EndGame()
		{
			Debug.Log("Game is over");
			_gameOverUI.Show();
			GameIsOver = true;
			BallGameSaveSystem.Save();
		}

		[CommandHandler(Description="Restarts the game")]
		public void Restart()
		{
			Debug.Log("Restarting game");
			foreach (var ball in _balls)
			{
				Destroy(ball.gameObject);
			}

			BeginGame();
		}
	}
}

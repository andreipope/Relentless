using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Opencoding.Console.Demo
{
	class DemoBall : MonoBehaviour
	{
		private BallGameController _ballGameController = null;

		[SerializeField] private Color[] _ballColors;

		public int BallColorIndex
		{
			get; private set;
		}

		public void Setup(BallGameController ballGameController)
		{
			_ballGameController = ballGameController;

			BallColorIndex = Random.Range(0, _ballColors.Length);
			GetComponent<SpriteRenderer>().color = _ballColors[BallColorIndex];

			ballGameController.RegisterBall(this);
		}

		private void OnMouseUp()
		{
			_ballGameController.DestroyAdjacentMatchingBalls(this);
		}

		private void OnDestroy()
		{
			if (_ballGameController != null)
				_ballGameController.BallCollected(this);
		}
	}
}

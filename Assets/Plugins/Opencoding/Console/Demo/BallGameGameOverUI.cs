using UnityEngine;
using UnityEngine.UI;

namespace Opencoding.Console.Demo
{
	class BallGameGameOverUI : MonoBehaviour
	{
		[SerializeField] private Button _restartButton;
		private BallGameController _ballGameController;

		private void Awake()
		{
			gameObject.SetActive(false);
		}

		public void Setup(BallGameController ballGameController)
		{
			_ballGameController = ballGameController;
			_restartButton.onClick.AddListener(OnRestartPressed);
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		private void OnRestartPressed()
		{
			gameObject.SetActive(false);
			_ballGameController.Restart();
		}
	}
}

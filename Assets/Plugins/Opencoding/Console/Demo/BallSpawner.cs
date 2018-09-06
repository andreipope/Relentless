using System.Collections;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	class BallSpawner : MonoBehaviour
	{
		private GameObject _ballPrefab;
		private BallGameController _ballGameController;

		[SerializeField] private float _xFraction = 0.5f;

		private void Awake()
		{
			var screenWidth = Camera.main.orthographicSize*Camera.main.aspect;
			transform.position = new Vector3(2 * screenWidth * _xFraction - screenWidth, transform.position.y, transform.position.z);
		}

		public void SpawnBalls(int count)
		{
			StartCoroutine(SpawnBallCoroutine(count));
		}

		private IEnumerator SpawnBallCoroutine(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				var spawnedBall = (GameObject) Instantiate(_ballPrefab, transform.position, Quaternion.identity);
				spawnedBall.GetComponent<DemoBall>().Setup(_ballGameController);
				spawnedBall.SetActive(true);
				spawnedBall.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-3, 3), Random.Range(-2,-4));
				yield return new WaitForSeconds(0.1f);
			}
		}

		public void Setup(BallGameController ballGameController, GameObject ballPrefab)
		{
			_ballGameController = ballGameController;
			_ballPrefab = ballPrefab;
		}
	}
}

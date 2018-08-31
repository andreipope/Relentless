using UnityEngine;

namespace Opencoding.Console.Demo
{
	class AttachToScreenEdge : MonoBehaviour
	{
		private enum Edge
		{
			LEFT,
			RIGHT,
			BOTTOM
		}

		[SerializeField]
		private Edge _edge;

		private void Start()
		{
			float width = 15.0f;
			float halfWidth = width / 2;

			var mainCamera = Camera.main;
			var orthographicHeight = mainCamera.orthographicSize;
			var orthographicWidth = orthographicHeight * mainCamera.aspect;

			switch (_edge)
			{
				case Edge.LEFT:
					transform.position = new Vector3(-orthographicWidth - halfWidth, 0, 0);
					transform.localScale = new Vector3(width, orthographicHeight * 2, 1);
					break;
				case Edge.RIGHT:
					transform.position = new Vector3(orthographicWidth + halfWidth, 0, 0);
					transform.localScale = new Vector3(width, orthographicHeight * 2, 1);
					break;
				case Edge.BOTTOM:
					transform.position = new Vector3(0, -orthographicHeight - halfWidth, 0);
					transform.localScale = new Vector3(orthographicWidth * 2, width, 1);
					break;
			}
		}
	}
}

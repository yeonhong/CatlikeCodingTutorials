using UnityEngine;

namespace MeshBasics
{
	public class CircleGizmo : MonoBehaviour
	{
		public int resolution = 10;
		public bool bAdvance = false;

		private void OnDrawGizmosSelected() {
			float step = 2f / resolution;
			for (int i = 0; i <= resolution; i++) {
				ShowPoint(i * step - 1f, -1f, bAdvance);
				ShowPoint(i * step - 1f, 1f, bAdvance);
			}
			for (int i = 1; i < resolution; i++) {
				ShowPoint(-1f, i * step - 1f, bAdvance);
				ShowPoint(1f, i * step - 1f, bAdvance);
			}
		}

		private void ShowPoint(float x, float y, bool advance = false) {
			Vector2 square = new Vector2(x, y);
			Vector2 circle = square.normalized;

			if (advance) {
				circle.x = square.x * Mathf.Sqrt(1f - square.y * square.y * 0.5f);
				circle.y = square.y * Mathf.Sqrt(1f - square.x * square.x * 0.5f);
			}

			Gizmos.color = Color.black;
			Gizmos.DrawSphere(square, 0.025f);

			Gizmos.color = Color.white;
			Gizmos.DrawSphere(circle, 0.025f);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(square, circle);

			Gizmos.color = Color.gray;
			Gizmos.DrawLine(circle, Vector2.zero);
		}
	}
}
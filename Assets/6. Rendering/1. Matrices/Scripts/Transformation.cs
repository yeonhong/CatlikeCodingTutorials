using UnityEngine;

namespace Rendering
{
	public abstract class Transformation : MonoBehaviour
	{
		public abstract Matrix4x4 Matrix { get; }

		public Vector3 Apply(Vector3 point) {
			return Matrix.MultiplyPoint(point);
		}
	}
}
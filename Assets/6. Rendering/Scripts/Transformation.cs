using UnityEngine;

namespace Rendering
{
	public abstract class Transformation : MonoBehaviour
	{
		public abstract Vector3 Apply(Vector3 point);
	}
}
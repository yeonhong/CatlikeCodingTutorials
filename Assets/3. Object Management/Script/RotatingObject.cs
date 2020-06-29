using UnityEngine;

namespace ObjectManagement
{
	public class RotatingObject : PersistableObject
	{
		[SerializeField] private Vector3 angularVelocity = Vector3.zero;

		private void Update() {
			transform.Rotate(angularVelocity * Time.deltaTime);
		}
	}
}
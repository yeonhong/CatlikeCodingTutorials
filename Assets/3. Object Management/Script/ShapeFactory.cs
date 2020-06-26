using UnityEngine;

namespace ObjectManagement
{
	[CreateAssetMenu]
	public class ShapeFactory : ScriptableObject
	{
		[SerializeField] private Shape[] prefabs = null;

		public Shape Get(int shapeId) {
			return Instantiate(prefabs[shapeId]);
		}

		public Shape GetRandom() {
			return Get(Random.Range(0, prefabs.Length));
		}
	}
}
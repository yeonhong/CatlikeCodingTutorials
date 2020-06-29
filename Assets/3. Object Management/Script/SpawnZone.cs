using UnityEngine;

namespace ObjectManagement
{
	public abstract class SpawnZone : PersistableObject
	{
		public abstract Vector3 SpawnPoint { get; }
	}
}
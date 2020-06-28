using UnityEngine;

namespace ObjectManagement
{
	public class GameLevel : PersistableObject
	{
		public static GameLevel Current { get; private set; }

		[SerializeField] private SpawnZone spawnZone = null;

		public Vector3 SpawnPoint {
			get {
				return spawnZone.SpawnPoint;
			}
		}

		private void OnEnable() {
			Current = this;
		}

		public override void Save(GameDataWriter writer) { }

		public override void Load(GameDataReader reader) { }
	}
}
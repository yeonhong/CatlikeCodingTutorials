using UnityEngine;

namespace ObjectManagement
{
	public class GameLevel : PersistableObject
	{
		public static GameLevel Current { get; private set; }

		[SerializeField] private SpawnZone spawnZone = null;
		[SerializeField] private PersistableObject[] persistentObjects;

		public Vector3 SpawnPoint {
			get {
				return spawnZone.SpawnPoint;
			}
		}

		private void OnEnable() {
			Current = this;
			if (persistentObjects == null) {
				persistentObjects = new PersistableObject[0];
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(persistentObjects.Length);
			for (int i = 0; i < persistentObjects.Length; i++) {
				persistentObjects[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			int savedCount = reader.ReadInt();
			for (int i = 0; i < savedCount; i++) {
				persistentObjects[i].Load(reader);
			}
		}
	}
}
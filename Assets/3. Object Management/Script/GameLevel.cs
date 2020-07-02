using UnityEngine;

namespace ObjectManagement
{
	public class GameLevel : PersistableObject
	{
		public static GameLevel Current { get; private set; }

		[SerializeField] private SpawnZone spawnZone = null;
		[SerializeField] private PersistableObject[] persistentObjects;
		[SerializeField] private int populationLimit = 100;

		public int PopulationLimit => populationLimit;

		public void SpawnShapes() {
			spawnZone.SpawnShapes();
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
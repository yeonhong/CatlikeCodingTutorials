using UnityEngine;

namespace ObjectManagement
{
	public partial class GameLevel : PersistableObject
	{
		public static GameLevel Current { get; private set; }

		[SerializeField] private SpawnZone spawnZone = null;

		// 이름변경으로 인한 링크깨지는것을 막아준다.
		[UnityEngine.Serialization.FormerlySerializedAs("persistentObjects")]
		[SerializeField] private GameLevelObject[] levelObjects;
		[SerializeField] private int populationLimit = 100;

		public int PopulationLimit => populationLimit;

		public void SpawnShapes() {
			spawnZone.SpawnShapes();
		}

		private void OnEnable() {
			Current = this;
			if (levelObjects == null) {
				levelObjects = new GameLevelObject[0];
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(levelObjects.Length);
			for (int i = 0; i < levelObjects.Length; i++) {
				levelObjects[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			int savedCount = reader.ReadInt();
			for (int i = 0; i < savedCount; i++) {
				levelObjects[i].Load(reader);
			}
		}

		public void GameUpdate() {
			for (int i = 0; i < levelObjects.Length; i++) {
				levelObjects[i].GameUpdate();
			}
		}
	}
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense
{
	[CreateAssetMenu]
	public class GameTileContentFactory : ScriptableObject
	{
		[SerializeField] private GameTileContent destinationPrefab = default;
		[SerializeField] private GameTileContent emptyPrefab = default;
		[SerializeField] private GameTileContent wallPrefab = default;
		[SerializeField] private GameTileContent spawnPointPrefab = default;

		private Scene contentScene = default;

		public void Reclaim(GameTileContent content) {
			Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
			Destroy(content.gameObject);
		}

		public GameTileContent Get(GameTileContentType type) {
			switch (type) {
				case GameTileContentType.Destination: return Get(destinationPrefab);
				case GameTileContentType.Empty: return Get(emptyPrefab);
				case GameTileContentType.Wall: return Get(wallPrefab);
				case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
			}
			Debug.Assert(false, "Unsupported type: " + type);
			return null;
		}

		private GameTileContent Get(GameTileContent prefab) {
			GameTileContent instance = Instantiate(prefab);
			instance.OriginFactory = this;
			MoveToFactoryScene(instance.gameObject);
			return instance;
		}

		private void MoveToFactoryScene(GameObject o) {
			if (!contentScene.isLoaded) {
				if (Application.isEditor) {
					contentScene = SceneManager.GetSceneByName(name);
					if (!contentScene.isLoaded) {
						contentScene = SceneManager.CreateScene(name);
					}
				} else {
					contentScene = SceneManager.CreateScene(name);
				}
			}
			SceneManager.MoveGameObjectToScene(o, contentScene);
		}
	}
}
using UnityEngine;

namespace TowerDefense
{
	public enum GameTileContentType
	{
		Empty, Destination, Wall, SpawnPoint, Tower
	}

	public class GameTileContent : MonoBehaviour
	{
		[SerializeField] private GameTileContentType type = default;
		public GameTileContentType Type => type;
		public bool BlocksPath => 
			Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

		private GameTileContentFactory originFactory;

		public GameTileContentFactory OriginFactory {
			get => originFactory;
			set {
				Debug.Assert(originFactory == null, "Redefined origin factory!");
				originFactory = value;
			}
		}

		public void Recycle() {
			originFactory.Reclaim(this);
		}
	}
}
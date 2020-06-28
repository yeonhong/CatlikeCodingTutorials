using UnityEngine;

namespace ObjectManagement
{
	public class GameLevel : MonoBehaviour
	{
		[SerializeField] private SpawnZone spawnZone = null;

		private void Start() {
			Game.Instance.SpawnZoneOfLevel = spawnZone;
		}
	}
}
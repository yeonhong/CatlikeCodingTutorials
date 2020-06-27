using UnityEngine;

namespace ObjectManagement
{
	public class GameLevel : MonoBehaviour
	{
		[SerializeField] private SpawnZone spawnZone;

		private void Start() {
			Game.Instance.SpawnZoneOfLevel = spawnZone;
		}
	}
}
using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu]
	public class WarFactory : GameObjectFactory
	{
		[SerializeField] private Explosion explosionPrefab = default;
		[SerializeField] private Shell shellPrefab = default;

		public Shell Shell => Get(shellPrefab);
		public Explosion Explosion => Get(explosionPrefab);

		private T Get<T>(T prefab) where T : WarEntity {
			T instance = CreateGameObjectInstance(prefab);
			instance.OriginFactory = this;
			return instance;
		}

		public void Reclaim(WarEntity entity) {
			Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");
			Destroy(entity.gameObject);
		}
	}
}
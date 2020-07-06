using UnityEngine;

namespace TowerDefense
{
	public class Enemy : MonoBehaviour
	{
		private EnemyFactory originFactory;

		public EnemyFactory OriginFactory {
			get => originFactory;
			set {
				Debug.Assert(originFactory == null, "Redefined origin factory!");
				originFactory = value;
			}
		}
	}
}
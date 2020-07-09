using UnityEngine;

namespace TowerDefense
{
	public abstract class WarEntity : GameBehavior
	{
		private WarFactory originFactory;

		public WarFactory OriginFactory {
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
using UnityEngine;

namespace TowerDefense
{
	public class MortarTower : Tower
	{
		[SerializeField, Range(0.5f, 2f)]
		private float shotsPerSecond = 1f;
		[SerializeField]
		private Transform mortar = default;

		public override TowerType TowerType => TowerType.Mortar;
	}
}
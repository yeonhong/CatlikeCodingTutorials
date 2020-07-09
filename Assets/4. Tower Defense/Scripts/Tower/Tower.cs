using UnityEngine;

namespace TowerDefense
{
	public abstract class Tower : GameTileContent
	{
		private static Collider[] targetsBuffer = new Collider[100];
		private const int enemyLayerMask = 1 << 11;
		[SerializeField, Range(1.5f, 10.5f)]
		protected float targetingRange = 2.5f;

		public abstract TowerType TowerType { get; }

		protected bool TrackTarget(ref TargetPoint target) {
			if (target == null) {
				return false;
			}

			Vector3 a = transform.localPosition;
			Vector3 b = target.Position;
			float x = a.x - b.x;
			float z = a.z - b.z;
			float r = targetingRange + 0.125f * target.Enemy.Scale;
			if (x * x + z * z > r * r) {
				target = null;
				return false;
			}

			return true;
		}

		protected bool AcquireTarget(out TargetPoint target) {
			Vector3 a = transform.localPosition;
			Vector3 b = a;
			b.y += 2f;
			int hits = Physics.OverlapCapsuleNonAlloc(
				a, b, targetingRange, targetsBuffer, enemyLayerMask
			);
			if (hits > 0) {
				target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
				Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
				return true;
			}
			target = null;
			return false;
		}

		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;
			Vector3 position = transform.localPosition;
			position.y += 0.01f;
			Gizmos.DrawWireSphere(position, targetingRange);
		}
	}
}
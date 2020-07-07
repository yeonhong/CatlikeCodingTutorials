using UnityEngine;

namespace TowerDefense
{
	public class Tower : GameTileContent
	{
		[SerializeField, Range(1.5f, 10.5f)]
		private float targetingRange = 2.5f;
		private TargetPoint target;
		private const int enemyLayerMask = 1 << 11;
		private static Collider[] targetsBuffer = new Collider[1];

		public override void GameUpdate() {
			if (TrackTarget() || AcquireTarget()) {
				Debug.Log("Locked on target!");
			}
		}

		private bool TrackTarget() {
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

		bool AcquireTarget() {
			Vector3 a = transform.localPosition;
			Vector3 b = a;
			b.y += 2f;
			int hits = Physics.OverlapCapsuleNonAlloc(
				a, b, targetingRange, targetsBuffer, enemyLayerMask
			);
			if (hits > 0) {
				target = targetsBuffer[0].GetComponent<TargetPoint>();
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

			if (target != null) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(position, target.Position);
			}
		}
	}
}
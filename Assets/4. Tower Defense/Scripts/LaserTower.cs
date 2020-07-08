using UnityEngine;

namespace TowerDefense
{
	public class LaserTower : Tower
	{
		[SerializeField]
		private Transform turret = default, laserBeam = default;
		[SerializeField, Range(1f, 100f)]
		private float damagePerSecond = 10f;
		private TargetPoint target;
		private Vector3 laserBeamScale;

		private void Awake() {
			laserBeamScale = laserBeam.localScale;
		}

		public override void GameUpdate() {
			if (TrackTarget(ref target) || AcquireTarget(out target)) {
				Shoot();
			} else {
				laserBeam.localScale = Vector3.zero;
			}
		}

		private void Shoot() {
			Vector3 point = target.Position;
			turret.LookAt(point);
			laserBeam.localRotation = turret.localRotation;

			float d = Vector3.Distance(turret.position, point);
			laserBeamScale.z = d;
			laserBeam.localScale = laserBeamScale;
			laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

			target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
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
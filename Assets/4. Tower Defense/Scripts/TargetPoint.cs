using UnityEngine;

namespace TowerDefense
{
	[RequireComponent(typeof(SphereCollider))]
	public class TargetPoint : MonoBehaviour
	{
		public Enemy Enemy { get; private set; }
		public Vector3 Position => transform.position;

		private void Awake() {
			Enemy = transform.root.GetComponent<Enemy>();
			Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
			Debug.Assert(GetComponent<SphereCollider>() != null,
			"Target point without sphere collider!", this
			);
			Debug.Assert(gameObject.layer == 11, "Target point on wrong layer!", this);
			Enemy.TargetPointCollider = GetComponent<Collider>();
		}

		private const int enemyLayerMask = 1 << 11;
		private static Collider[] buffer = new Collider[100];
		public static int BufferedCount { get; private set; }
		public static TargetPoint RandomBuffered => 
			GetBuffered(Random.Range(0, BufferedCount));

		public static bool FillBuffer(Vector3 position, float range) {
			Vector3 top = position;
			top.y += 3f;
			BufferedCount = Physics.OverlapCapsuleNonAlloc(
				position, top, range, buffer, enemyLayerMask
			);
			return BufferedCount > 0;
		}

		public static TargetPoint GetBuffered(int index) {
			var target = buffer[index].GetComponent<TargetPoint>();
			Debug.Assert(target != null, "Targeted non-enemy!", buffer[0]);
			return target;
		}
	}
}
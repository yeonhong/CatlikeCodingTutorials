﻿using UnityEngine;

namespace ObjectManagement
{
	public class SphereSpawnZone : SpawnZone
	{
		[SerializeField] private bool surfaceOnly = true;

		public override Vector3 SpawnPoint {
			get {
				return transform.TransformPoint(
					surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere
				);
			}
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(Vector3.zero, 1f);
		}
	}
}

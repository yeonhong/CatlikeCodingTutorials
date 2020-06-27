﻿using UnityEngine;

namespace ObjectManagement
{
	public class CompositeSpawnZone : SpawnZone
	{
		[SerializeField] private SpawnZone[] spawnZones = null;

		public override Vector3 SpawnPoint {
			get {
				int index = Random.Range(0, spawnZones.Length);
				return spawnZones[index].SpawnPoint;
			}
		}
	}
}
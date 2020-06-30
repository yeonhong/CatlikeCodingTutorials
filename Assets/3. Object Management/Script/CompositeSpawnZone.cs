﻿using UnityEngine;

namespace ObjectManagement
{
	public class CompositeSpawnZone : SpawnZone
	{
		[SerializeField] private SpawnZone[] spawnZones = null;
		[SerializeField] private bool sequential = true;
		[SerializeField] private bool overrideConfig;
		private int nextSequentialIndex;

		public override Vector3 SpawnPoint {
			get {
				int index;
				if (sequential) {
					index = nextSequentialIndex++;
					if (nextSequentialIndex >= spawnZones.Length) {
						nextSequentialIndex = 0;
					}
				}
				else {
					index = Random.Range(0, spawnZones.Length);
				}
				return spawnZones[index].SpawnPoint;
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(nextSequentialIndex);
		}

		public override void Load(GameDataReader reader) {
			nextSequentialIndex = reader.ReadInt();
		}

		public override void ConfigureSpawn(Shape shape) {
			if (overrideConfig) {
				base.ConfigureSpawn(shape);
			}
			else {
				int index;
				if (sequential) {
					index = nextSequentialIndex++;
					if (nextSequentialIndex >= spawnZones.Length) {
						nextSequentialIndex = 0;
					}
				}
				else {
					index = Random.Range(0, spawnZones.Length);
				}
				spawnZones[index].ConfigureSpawn(shape);
			}
		}
	}
}
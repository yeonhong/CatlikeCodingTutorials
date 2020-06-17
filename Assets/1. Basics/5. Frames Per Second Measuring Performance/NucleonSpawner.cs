using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NucleonSpawner : MonoBehaviour
{
	public float timeBetweenSpawns = 0.05f;
	public float spawnDistance = 15f;
	public Nucleon[] nucleonPrefabs;

	private float timeSinceLastSpawn;

	private void FixedUpdate() {
		timeSinceLastSpawn += Time.deltaTime;
		if (timeSinceLastSpawn >= timeBetweenSpawns) {
			timeSinceLastSpawn -= timeBetweenSpawns;
			SpawnNucleon();
		}
	}

	void SpawnNucleon() {
		var prefab = nucleonPrefabs[Random.Range(0, nucleonPrefabs.Length)];
		var spawn = Instantiate(prefab);
		spawn.transform.localPosition = Random.onUnitSphere * spawnDistance;
	}
}
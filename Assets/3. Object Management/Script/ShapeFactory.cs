﻿using UnityEngine;

namespace ObjectManagement
{
	[CreateAssetMenu]
	public class ShapeFactory : ScriptableObject
	{
		[SerializeField] private Shape[] prefabs = null;

		public Shape Get(int shapeId) {
			Shape instance = Instantiate(prefabs[shapeId]);
			instance.ShapeId = shapeId;
			return instance;
		}

		public Shape GetRandom() {
			return Get(Random.Range(0, prefabs.Length));
		}
	}
}
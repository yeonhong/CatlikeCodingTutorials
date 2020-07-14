﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectManagement
{
	[CreateAssetMenu(menuName = "ObjectManagement/ShapeFactory")]
	public class ShapeFactory : ScriptableObject
	{
		[SerializeField] private Shape[] prefabs = null;
		[SerializeField] private Material[] materials = null;
		[SerializeField] private bool recycle = true;

		private List<Shape>[] pools;

		public int FactoryId {
			get => factoryId;
			set {
				if (factoryId == int.MinValue && value != int.MinValue) {
					factoryId = value;
				}
				else {
					Debug.Log("Not allowed to change factoryId.");
				}
			}
		}

		// Unity doesn't save private fields of scriptable objects that aren't marked as serialized
		[System.NonSerialized] private int factoryId = int.MinValue;

		/*
		 * Shape가 변경 될 때 게임 성능에 부정적인 영향을 줄 수 있습니다.
		 * 객체의 활성 또는 변환 상태가 변경 될 때마다 모든 상위 객체에 변경 사항이 통보됩니다.
		 * 따라서 꼭 필요한 것이 아닌 경우 다른 개체의 자식 개체를 만들지 않는 것이 가장 좋습니다.
		 */
		private Scene poolScene;

		public Shape Get(int shapeId = 0, int materialId = 0) {
			Shape instance;
			if (recycle) {
				if (pools == null) {
					CreatePools();
				}

				List<Shape> pool = pools[shapeId];
				int lastIndex = pool.Count - 1;
				if (lastIndex >= 0) {
					instance = pool[lastIndex];
					instance.gameObject.SetActive(true);
					pool.RemoveAt(lastIndex);
				}
				else {
					instance = Instantiate(prefabs[shapeId]);
					instance.OriginFactory = this;
					instance.ShapeId = shapeId;
					SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
				}
			}
			else {
				instance = Instantiate(prefabs[shapeId]);
				instance.ShapeId = shapeId;
			}

			instance.SetMaterial(materials[materialId], materialId);
			Game.Instance.AddShape(instance);
			return instance;
		}

		public void Reclaim(Shape shapeToRecycle) {
			if (shapeToRecycle.OriginFactory != this) {
				Debug.LogError("Tried to reclaim shape with wrong factory.");
				return;
			}

			if (recycle) {
				if (pools == null) {
					CreatePools();
				}
				pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
				shapeToRecycle.gameObject.SetActive(false);
			}
			else {
				Destroy(shapeToRecycle.gameObject);
			}
		}

		public Shape GetRandom() {
			return Get(
				Random.Range(0, prefabs.Length),
				Random.Range(0, materials.Length)
			);
		}

		private void CreatePools() {
			pools = new List<Shape>[prefabs.Length];
			for (int i = 0; i < pools.Length; i++) {
				pools[i] = new List<Shape>();
			}

			// Recovering from Recompilation.
			if (Application.isEditor) {
				poolScene = SceneManager.GetSceneByName(name);
				if (poolScene.isLoaded) {
					GameObject[] rootObjects = poolScene.GetRootGameObjects();
					for (int i = 0; i < rootObjects.Length; i++) {
						Shape pooledShape = rootObjects[i].GetComponent<Shape>();
						if (!pooledShape.gameObject.activeSelf) {
							pools[pooledShape.ShapeId].Add(pooledShape);
						}
					}
					return;
				}
			}
			poolScene = SceneManager.CreateScene(name);
		}
	}
}
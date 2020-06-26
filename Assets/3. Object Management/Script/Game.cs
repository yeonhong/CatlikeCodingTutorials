﻿using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
	public class Game : PersistableObject
	{
		private const int saveVersion = 1;

		public ShapeFactory shapeFactory = null;
		public PersistentStorage storage = null;
		public KeyCode createKey = KeyCode.C;
		public KeyCode newGameKey = KeyCode.N;
		public KeyCode saveKey = KeyCode.S;
		public KeyCode loadKey = KeyCode.L;
		public KeyCode destroyKey = KeyCode.X;

		private List<Shape> shapes = null;

		private void Awake() {
			shapes = new List<Shape>();
		}

		private void Update() {
			if (Input.GetKeyDown(createKey)) {
				CreateShape();
				Debug.Log($"key - {createKey}");
			}
			else if (Input.GetKey(newGameKey)) {
				BeginNewGame();
				Debug.Log($"key - {newGameKey}");
			}
			else if (Input.GetKeyDown(saveKey)) {
				storage.Save(this, saveVersion);
				Debug.Log($"key - {saveKey}");
			}
			else if (Input.GetKeyDown(loadKey)) {
				BeginNewGame();
				storage.Load(this);
				Debug.Log($"key - {loadKey}");
			}
			else if (Input.GetKeyDown(destroyKey)) {
				DestroyShape();
				Debug.Log($"key - {destroyKey}");
			}
		}

		private void BeginNewGame() {
			for (int i = 0; i < shapes.Count; i++) {
				Destroy(shapes[i].gameObject);
			}
			shapes.Clear();
		}

		private void CreateShape() {
			Shape instance = shapeFactory.GetRandom();
			Transform t = instance.transform;
			t.localPosition = Random.insideUnitSphere * 5f;
			t.localRotation = Random.rotation;
			t.localScale = Vector3.one * Random.Range(0.1f, 1f);
			instance.SetColor(Random.ColorHSV(
				hueMin: 0f, hueMax: 1f,
				saturationMin: 0.5f, saturationMax: 1f,
				valueMin: 0.25f, valueMax: 1f,
				alphaMin: 1f, alphaMax: 1f
			));
			shapes.Add(instance);
		}

		private void DestroyShape() {
			if (shapes.Count > 0) {
				int index = Random.Range(0, shapes.Count);
				Destroy(shapes[index].gameObject);

				// list 삭제 최적화. list는 배열로 구현되어 있어 내부적으로 단순히 사용하면 오래걸림.
				int lastIndex = shapes.Count - 1;
				shapes[index] = shapes[lastIndex];
				shapes.RemoveAt(lastIndex);
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(shapes.Count);
			for (int i = 0; i < shapes.Count; i++) {
				writer.Write(shapes[i].ShapeId);
				writer.Write(shapes[i].MaterialId);
				shapes[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			int version = reader.Version;
			if (version > saveVersion) {
				Debug.LogError("Unsupported future save version " + version);
				return;
			}
			int count = version <= 0 ? -version : reader.ReadInt();
			for (int i = 0; i < count; i++) {
				int shapeId = version > 0 ? reader.ReadInt() : 0;
				int materialId = version > 0 ? reader.ReadInt() : 0;
				Shape instance = shapeFactory.Get(shapeId, materialId);
				instance.Load(reader);
				shapes.Add(instance);
			}
		}
	}
}

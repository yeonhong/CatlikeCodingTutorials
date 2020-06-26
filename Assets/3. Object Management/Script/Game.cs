using System.Collections.Generic;
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

		private List<Shape> shapes = null;

		private void Awake() {
			shapes = new List<Shape>();
		}

		private void Update() {
			if (Input.GetKeyDown(createKey)) {
				CreateObject();
				Debug.Log($"key - {createKey}");
			}
			else if (Input.GetKey(newGameKey)) {
				BeginNewGame();
				Debug.Log($"key - {newGameKey}");
			}
			else if (Input.GetKeyDown(saveKey)) {
				storage.Save(this);
				Debug.Log($"key - {saveKey}");
			}
			else if (Input.GetKeyDown(loadKey)) {
				BeginNewGame();
				storage.Load(this);
				Debug.Log($"key - {loadKey}");
			}
		}

		private void BeginNewGame() {
			for (int i = 0; i < shapes.Count; i++) {
				Destroy(shapes[i].gameObject);
			}
			shapes.Clear();
		}

		private void CreateObject() {
			Shape instance = shapeFactory.GetRandom();
			Transform t = instance.transform;
			t.localPosition = Random.insideUnitSphere * 5f;
			t.localRotation = Random.rotation;
			t.localScale = Vector3.one * Random.Range(0.1f, 1f);
			shapes.Add(instance);
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(-saveVersion);
			writer.Write(shapes.Count);
			for (int i = 0; i < shapes.Count; i++) {
				writer.Write(shapes[i].ShapeId);
				writer.Write(shapes[i].MaterialId);
				shapes[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			int version = -reader.ReadInt();
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

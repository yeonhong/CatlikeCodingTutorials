using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
	public class Game : PersistableObject
	{
		public PersistableObject prefab = null;
		public PersistentStorage storage = null;
		public KeyCode createKey = KeyCode.C;
		public KeyCode newGameKey = KeyCode.N;
		public KeyCode saveKey = KeyCode.S;
		public KeyCode loadKey = KeyCode.L;

		private List<PersistableObject> objects = null;

		private void Awake() {
			objects = new List<PersistableObject>();
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
			for (int i = 0; i < objects.Count; i++) {
				Destroy(objects[i].gameObject);
			}
			objects.Clear();
		}

		private void CreateObject() {
			var o = Instantiate(prefab);
			var t = o.transform;
			t.localPosition = Random.insideUnitSphere * 5f;
			t.localRotation = Random.rotation;
			t.localScale = Vector3.one * Random.Range(0.1f, 1f);
			objects.Add(o);
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(objects.Count);
			for (int i = 0; i < objects.Count; i++) {
				objects[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			int count = reader.ReadInt();
			for (int i = 0; i < count; i++) {
				PersistableObject o = Instantiate(prefab);
				o.Load(reader);
				objects.Add(o);
			}
		}
	}
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ObjectManagement
{
	public class Game : MonoBehaviour
	{
		public Transform prefab = null;
		public KeyCode createKey = KeyCode.C;
		public KeyCode newGameKey = KeyCode.N;
		public KeyCode saveKey = KeyCode.S;
		public KeyCode loadKey = KeyCode.L;

		private List<Transform> objects = null;
		private string savePath;

		private void Awake() {
			objects = new List<Transform>();
			savePath = Path.Combine(Application.persistentDataPath, "saveFile");
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
				Save();
				Debug.Log($"key - {saveKey}");
			}
			else if (Input.GetKeyDown(loadKey)) {
				Load();
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
			Transform t = Instantiate(prefab);
			t.localPosition = Random.insideUnitSphere * 5f;
			t.localRotation = Random.rotation;
			t.localScale = Vector3.one * Random.Range(0.1f, 1f);
			objects.Add(t);
		}

		private void Save() {
			using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))) {
				writer.Write(objects.Count);
				for (int i = 0; i < objects.Count; i++) {
					Transform t = objects[i];
					writer.Write(t.localPosition.x);
					writer.Write(t.localPosition.y);
					writer.Write(t.localPosition.z);
				}
			}
		}

		private void Load() {
			BeginNewGame();

			using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open))) {
				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++) {
					Vector3 p;
					p.x = reader.ReadSingle();
					p.y = reader.ReadSingle();
					p.z = reader.ReadSingle();

					Transform t = Instantiate(prefab);
					t.localPosition = p;
					objects.Add(t);
				}
			}
		}
	}
}

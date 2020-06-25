using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
	public class Game : MonoBehaviour
	{
		public Transform prefab = null;
		public KeyCode createKey = KeyCode.C;
		public KeyCode newGameKey = KeyCode.N;

		List<Transform> objects = null;

		void Awake() {
			objects = new List<Transform>();
		}

		private void Update() {
			if (Input.GetKeyDown(createKey)) {
				CreateObject();
			}
			else if (Input.GetKey(newGameKey)) {
				BeginNewGame();
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
	}
}

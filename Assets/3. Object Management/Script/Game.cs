using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ObjectManagement
{
	public class Game : PersistableObject
	{
		private const int saveVersion = 6;
		public static Game Instance { get; private set; }

		[SerializeField] private ShapeFactory[] shapeFactories = null;
		[SerializeField] private PersistentStorage storage = null;
		[SerializeField] private KeyCode createKey = KeyCode.C;
		[SerializeField] private KeyCode newGameKey = KeyCode.N;
		[SerializeField] private KeyCode saveKey = KeyCode.S;
		[SerializeField] private KeyCode loadKey = KeyCode.L;
		[SerializeField] private KeyCode destroyKey = KeyCode.X;
		[SerializeField] private int levelCount = 2;
		[SerializeField] private bool reseedOnLoad = false;
		[SerializeField] private Slider creationSpeedSlider = null;
		[SerializeField] private Slider destructionSpeedSlider = null;

		public float CreationSpeed { get; set; }
		public float DestructionSpeed { get; set; }

		private float creationProgress, destructionProgress;
		private int loadedLevelBuildIndex;
		private Random.State mainRandomState;

		private bool inGameUpdateLoop = false;
		private List<Shape> shapes = null;
		private List<ShapeInstance> killList = null;

		private void Start() {
			mainRandomState = Random.state;
			shapes = new List<Shape>();
			killList = new List<ShapeInstance>();

			if (Application.isEditor) {
				for (int i = 0; i < SceneManager.sceneCount; i++) {
					Scene loadedScene = SceneManager.GetSceneAt(i);
					if (loadedScene.name.Contains("Level ")) {
						SceneManager.SetActiveScene(loadedScene);
						loadedLevelBuildIndex = loadedScene.buildIndex;
						return;
					}
				}
			}

			BeginNewGame();
			StartCoroutine(LoadLevel(1));
		}

		private void OnEnable() {
			Instance = this;

			if (shapeFactories[0].FactoryId != 0) {
				for (int i = 0; i < shapeFactories.Length; i++) {
					shapeFactories[i].FactoryId = i;
				}
			}
		}

		private void Update() {
			if (Input.GetKeyDown(createKey)) {
				GameLevel.Current.SpawnShapes();
				Debug.Log($"key - {createKey}");
			}
			else if (Input.GetKey(newGameKey)) {
				BeginNewGame();
				StartCoroutine(LoadLevel(loadedLevelBuildIndex));
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
			else {
				for (int i = 1; i <= levelCount; i++) {
					if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
						BeginNewGame();
						StartCoroutine(LoadLevel(i));
						return;
					}
				}
			}
		}

		private void FixedUpdate() {
			for (int i = 0; i < shapes.Count; i++) {
				shapes[i].GameUpdate();
			}

			creationProgress += Time.deltaTime * CreationSpeed;
			while (creationProgress >= 1f) {
				creationProgress -= 1f;
				GameLevel.Current.SpawnShapes();
			}

			destructionProgress += Time.deltaTime * DestructionSpeed;
			while (destructionProgress >= 1f) {
				destructionProgress -= 1f;
				DestroyShape();
			}

			int limit = GameLevel.Current.PopulationLimit;
			if (limit > 0) {
				while (shapes.Count > limit) {
					DestroyShape();
				}
			}

			if (killList.Count > 0) {
				for (int i = 0; i < killList.Count; i++) {
					if (killList[i].IsValid) { // 위에서 이미 제거한걸 중복해서 제거할 가능성이 있음.
						KillImmediately(killList[i].Shape);
					}
				}
				killList.Clear();
			}
		}

		private void BeginNewGame() {
			Random.state = mainRandomState;
			int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
			mainRandomState = Random.state;
			Random.InitState(seed);

			creationSpeedSlider.value = CreationSpeed = 0;
			destructionSpeedSlider.value = DestructionSpeed = 0;

			for (int i = 0; i < shapes.Count; i++) {
				shapes[i].Recycle();
			}
			shapes.Clear();
		}

		public void AddShape(Shape shape) {
			shape.SaveIndex = shapes.Count;
			shapes.Add(shape);
		}

		public Shape GetShape(int index) {
			return shapes[index];
		}

		private void DestroyShape() {
			if (shapes.Count > 0) {
				Shape shape = shapes[Random.Range(0, shapes.Count)];
				KillImmediately(shape);
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(shapes.Count);
			writer.Write(Random.state);
			writer.Write(CreationSpeed);
			writer.Write(creationProgress);
			writer.Write(DestructionSpeed);
			writer.Write(destructionProgress);
			writer.Write(loadedLevelBuildIndex);
			GameLevel.Current.Save(writer);
			for (int i = 0; i < shapes.Count; i++) {
				writer.Write(shapes[i].OriginFactory.FactoryId);
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

			StartCoroutine(LoadGame(reader));
		}

		private IEnumerator LoadGame(GameDataReader reader) {
			int version = reader.Version;

			int count = version <= 0 ? -version : reader.ReadInt();

			if (version >= 3) {
				Random.State state = reader.ReadRandomState();
				if (!reseedOnLoad) {
					Random.state = state;
				}
				CreationSpeed = creationSpeedSlider.value = reader.ReadFloat();
				creationProgress = reader.ReadFloat();
				DestructionSpeed = destructionSpeedSlider.value = reader.ReadFloat();
				destructionProgress = reader.ReadFloat();
			}

			yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
			if (version >= 3) {
				GameLevel.Current.Load(reader);
			}

			for (int i = 0; i < count; i++) {
				int factoryId = version >= 5 ? reader.ReadInt() : 0;
				int shapeId = version > 0 ? reader.ReadInt() : 0;
				int materialId = version > 0 ? reader.ReadInt() : 0;
				Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
				instance.Load(reader);
			}

			for (int i = 0; i < shapes.Count; i++) {
				shapes[i].ResolveShapeInstances();
			}
		}

		private IEnumerator LoadLevel(int levelBuildIndex) {
			enabled = false;
			if (loadedLevelBuildIndex > 0) {
				yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
			}
			yield return SceneManager.LoadSceneAsync(
				levelBuildIndex, LoadSceneMode.Additive
			);
			SceneManager.SetActiveScene(
				SceneManager.GetSceneByBuildIndex(levelBuildIndex)
			);
			loadedLevelBuildIndex = levelBuildIndex;
			enabled = true;
		}

		public void Kill(Shape shape) {
			if (inGameUpdateLoop) {
				killList.Add(shape);
			}
			else {
				KillImmediately(shape);
			}
		}

		private void KillImmediately(Shape shape) {
			int index = shape.SaveIndex;
			shape.Recycle();

			// list 삭제 최적화. list는 배열로 구현되어 있어 내부적으로 단순히 사용하면 오래걸림.
			int lastIndex = shapes.Count - 1;
			shapes[lastIndex].SaveIndex = index;
			shapes[index] = shapes[lastIndex];
			shapes.RemoveAt(lastIndex);
		}
	}
}

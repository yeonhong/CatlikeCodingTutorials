using UnityEngine;

namespace TowerDefense
{
	public class Game : MonoBehaviour
	{
		[SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
		[SerializeField] private GameBoard board = default;
		[SerializeField] private GameTileContentFactory tileContentFactory = default;
		[SerializeField] private EnemyFactory enemyFactory = default;
		[SerializeField, Range(0.1f, 10f)] private float spawnSpeed = 4f;
		[SerializeField] private WarFactory warFactory = default;

		private GameBehaviorCollection enemies = new GameBehaviorCollection();
		private GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
		private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
		private float spawnProgress;
		private TowerType selectedTowerType = TowerType.Laser;
		private static Game instance;

		public static Shell SpawnShell() {
			Shell shell = instance.warFactory.Shell;
			instance.nonEnemies.Add(shell);
			return shell;
		}

		public static Explosion SpawnExplosion() {
			Explosion explosion = instance.warFactory.Explosion;
			instance.nonEnemies.Add(explosion);
			return explosion;
		}

		private void OnEnable() {
			instance = this;
		}

		private void OnValidate() {
			if (boardSize.x < 2) {
				boardSize.x = 2;
			}
			if (boardSize.y < 2) {
				boardSize.y = 2;
			}
		}

		private void Awake() {
			board.Initialize(boardSize, tileContentFactory);
			board.ShowGrid = true;
		}

		private void Update() {
			if (Input.GetMouseButtonDown(0)) {
				HandleTouch();
			} else if (Input.GetMouseButtonDown(1)) {
				HandleAlternativeTouch();
			}

			if (Input.GetKeyDown(KeyCode.V)) {
				board.ShowPaths = !board.ShowPaths;
			} else if (Input.GetKeyDown(KeyCode.G)) {
				board.ShowGrid = !board.ShowGrid;
			} else if (Input.GetKeyDown(KeyCode.Alpha1)) {
				selectedTowerType = TowerType.Laser;
			} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				selectedTowerType = TowerType.Mortar;
			}


			spawnProgress += spawnSpeed * Time.deltaTime;
			while (spawnProgress >= 1f) {
				spawnProgress -= 1f;
				SpawnEnemy();
			}

			enemies.GameUpdate();
			Physics.SyncTransforms(); //물리위치Sync
			board.GameUpdate();
			nonEnemies.GameUpdate();
		}

		private void SpawnEnemy() {
			GameTile spawnPoint =
				board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
			Enemy enemy = enemyFactory.Get();
			enemy.SpawnOn(spawnPoint);
			enemies.Add(enemy);
		}

		private void HandleAlternativeTouch() {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					board.ToggleDestination(tile);
				} else {
					board.ToggleSpawnPoint(tile);
				}
			}
		}

		private void HandleTouch() {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					board.ToggleTower(tile, selectedTowerType);
				} else {
					board.ToggleWall(tile);
				}
			}
		}
	}
}
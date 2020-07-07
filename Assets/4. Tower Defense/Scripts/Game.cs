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

		private EnemyCollection enemies = new EnemyCollection();
		private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
		private float spawnProgress;

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
			}

			spawnProgress += spawnSpeed * Time.deltaTime;
			while (spawnProgress >= 1f) {
				spawnProgress -= 1f;
				SpawnEnemy();
			}

			enemies.GameUpdate();
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
				board.ToggleWall(tile);
			}
		}
	}
}
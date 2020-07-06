using UnityEngine;

namespace TowerDefense
{
	public class Game : MonoBehaviour
	{
		[SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
		[SerializeField] private GameBoard board = default;
		[SerializeField] private GameTileContentFactory tileContentFactory = default;

		private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

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
			}
			else if (Input.GetKeyDown(KeyCode.G)) {
				board.ShowGrid = !board.ShowGrid;
			}
		}

		void HandleAlternativeTouch() {
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
﻿using UnityEngine;
using UnityEngine.UI;

namespace HexMap
{
	public class HexGrid : MonoBehaviour
	{
		public int width = 6;
		public int height = 6;
		public HexCell cellPrefab;
		private HexCell[] cells;

		public Text cellLabelPrefab;
		private Canvas gridCanvas;

		private HexMesh hexMesh;

		public Color defaultColor = Color.white;
		public Color touchedColor = Color.magenta;

		private void Awake() {
			gridCanvas = GetComponentInChildren<Canvas>();
			hexMesh = GetComponentInChildren<HexMesh>();

			cells = new HexCell[height * width];

			for (int z = 0, i = 0; z < height; z++) {
				for (int x = 0; x < width; x++) {
					CreateCell(x, z, i++);
				}
			}
		}

		private void Start() {
			hexMesh.Triangulate(cells);
		}

		private void CreateCell(int x, int z, int i) {
			Vector3 position;
			position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
			position.y = 0f;
			position.z = z * (HexMetrics.outerRadius * 1.5f);

			HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
			cell.transform.SetParent(transform, false);
			cell.transform.localPosition = position;
			cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
			cell.color = defaultColor;

			Text label = Instantiate(cellLabelPrefab);
			label.rectTransform.SetParent(gridCanvas.transform, false);
			label.rectTransform.anchoredPosition =
				new Vector2(position.x, position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
		}

		private void Update() {
			if (Input.GetMouseButtonDown(0)) {
				HandleInput();
			}
		}

		private void HandleInput() {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				TouchCell(hit.point);
			}
		}

		private void TouchCell(Vector3 position) {
			position = transform.InverseTransformPoint(position);
			HexCoordinates coordinates = HexCoordinates.FromPosition(position);
			int index = GetIndex(coordinates);
			HexCell cell = cells[index];
			cell.color = touchedColor;
			hexMesh.Triangulate(cells);
		}

		private int GetIndex(HexCoordinates coordinates) {
			return coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		}
	}
}
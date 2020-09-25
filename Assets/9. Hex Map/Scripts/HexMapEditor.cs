﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap
{
	public class HexMapEditor : MonoBehaviour
	{
		public Color[] colors;
		public HexGrid hexGrid;
		private Color activeColor;
		private int activeElevation;

		private void Awake() {
			SelectColor(0);
		}

		private void Update() {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
				HandleInput();
			}
		}

		private void HandleInput() {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				EditCell(hexGrid.GetCell(hit.point));
			}
		}

		private void EditCell(HexCell cell) {
			cell.Color = activeColor;
			cell.Elevation = activeElevation;
		}

		public void SetElevation(float elevation) {
			activeElevation = (int)elevation;
		}

		public void SelectColor(int index) {
			activeColor = colors[index];
		}
	}
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap
{
	public class HexMapEditor : MonoBehaviour
	{
		public HexGrid hexGrid;
		public Material terrainMaterial;

		private int activeTerrainTypeIndex;
		private int activeElevation;
		private int activeWaterLevel;
		private int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;

		private bool applyElevation = false;
		private bool applyWaterLevel = false;
		private bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex;
		private int brushSize;
		private bool editMode;

		private enum OptionalToggle
		{
			Ignore, Yes, No
		}

		private OptionalToggle riverMode, roadMode, walledMode;

		private bool isDrag;
		private HexDirection dragDirection;
		private HexCell previousCell;

		private void Awake() {
			terrainMaterial.DisableKeyword("GRID_ON");
		}

		private void Update() {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
				HandleInput();
			}
			else {
				previousCell = null;
			}
		}

		private void HandleInput() {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				HexCell currentCell = hexGrid.GetCell(hit.point);
				if (previousCell && previousCell != currentCell) {
					ValidateDrag(currentCell);
				}
				else {
					isDrag = false;
				}
				if (editMode) {
					EditCells(currentCell);
				}
				previousCell = currentCell;
			}
			else {
				previousCell = null;
			}
		}

		private void ValidateDrag(HexCell currentCell) {
			for (
				dragDirection = HexDirection.NE;
				dragDirection <= HexDirection.NW;
				dragDirection++
			) {
				if (previousCell.GetNeighbor(dragDirection) == currentCell) {
					isDrag = true;
					return;
				}
			}
			isDrag = false;
		}

		private void EditCells(HexCell center) {
			int centerX = center.coordinates.X;
			int centerZ = center.coordinates.Z;

			for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
				for (int x = centerX - r; x <= centerX + brushSize; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}

			for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
				for (int x = centerX - brushSize; x <= centerX + r; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}
		}

		private void EditCell(HexCell cell) {
			if (cell) {
				if (activeTerrainTypeIndex >= 0) {
					cell.TerrainTypeIndex = activeTerrainTypeIndex;
				}
				if (applyElevation) {
					cell.Elevation = activeElevation;
				}
				if (applyWaterLevel) {
					cell.WaterLevel = activeWaterLevel;
				}
				if (applySpecialIndex) {
					cell.SpecialIndex = activeSpecialIndex;
				}
				if (applyUrbanLevel) {
					cell.UrbanLevel = activeUrbanLevel;
				}
				if (applyFarmLevel) {
					cell.FarmLevel = activeFarmLevel;
				}
				if (applyPlantLevel) {
					cell.PlantLevel = activePlantLevel;
				}
				if (riverMode == OptionalToggle.No) {
					cell.RemoveRiver();
				}
				if (walledMode != OptionalToggle.Ignore) {
					cell.Walled = walledMode == OptionalToggle.Yes;
				}
				if (roadMode == OptionalToggle.No) {
					cell.RemoveRoads();
				}
				if (isDrag) {
					HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
					if (otherCell) {
						if (riverMode == OptionalToggle.Yes) {
							otherCell.SetOutgoingRiver(dragDirection);
						}
						if (roadMode == OptionalToggle.Yes) {
							otherCell.AddRoad(dragDirection);
						}
					}
				}
			}
		}

		public void SetTerrainTypeIndex(int index) {
			activeTerrainTypeIndex = index;
		}

		public void SetApplyElevation(bool toggle) {
			applyElevation = toggle;
		}

		public void SetElevation(float elevation) {
			activeElevation = (int)elevation;
		}

		public void SetBrushSize(float size) {
			brushSize = (int)size;
		}

		public void SetRiverMode(int mode) {
			riverMode = (OptionalToggle)mode;
		}

		public void SetRoadMode(int mode) {
			roadMode = (OptionalToggle)mode;
		}

		public void SetWalledMode(int mode) {
			walledMode = (OptionalToggle)mode;
		}

		public void SetApplyWaterLevel(bool toggle) {
			applyWaterLevel = toggle;
		}

		public void SetWaterLevel(float level) {
			activeWaterLevel = (int)level;
		}

		public void SetApplyUrbanLevel(bool toggle) {
			applyUrbanLevel = toggle;
		}

		public void SetUrbanLevel(float level) {
			activeUrbanLevel = (int)level;
		}

		public void SetApplyFarmLevel(bool toggle) {
			applyFarmLevel = toggle;
		}

		public void SetFarmLevel(float level) {
			activeFarmLevel = (int)level;
		}

		public void SetApplyPlantLevel(bool toggle) {
			applyPlantLevel = toggle;
		}

		public void SetPlantLevel(float level) {
			activePlantLevel = (int)level;
		}

		public void SetApplySpecialIndex(bool toggle) {
			applySpecialIndex = toggle;
		}

		public void SetSpecialIndex(float index) {
			activeSpecialIndex = (int)index;
		}

		public void ShowGrid(bool visible) {
			if (visible) {
				terrainMaterial.EnableKeyword("GRID_ON");
			}
			else {
				terrainMaterial.DisableKeyword("GRID_ON");
			}
		}

		public void SetEditMode(bool toggle) {
			editMode = toggle;
			hexGrid.ShowUI(!toggle);
		}
	}
}
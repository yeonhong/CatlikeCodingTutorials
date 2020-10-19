﻿using System.IO;
using UnityEngine;

namespace HexMap
{
	public class HexCell : MonoBehaviour
	{
		public HexCoordinates coordinates;

		private int terrainTypeIndex;
		public Color Color => HexMetrics.colors[terrainTypeIndex];

		public int TerrainTypeIndex {
			get => terrainTypeIndex;
			set {
				if (terrainTypeIndex != value) {
					terrainTypeIndex = value;
					Refresh();
				}
			}
		}

		[SerializeField]
		private HexCell[] neighbors = null;

		[SerializeField]
		private bool[] roads = null;

		public Vector3 Position => transform.localPosition;

		private int elevation = int.MinValue;
		public int Elevation {
			get => elevation;
			set {
				if (elevation == value) {
					return;
				}

				elevation = value;
				RefreshPosition();
				ValidateRivers();

				// Road Exception
				for (int i = 0; i < roads.Length; i++) {
					if (roads[i] && GetElevationDifference((HexDirection)i) > 1) {
						SetRoad(i, false);
					}
				}

				Refresh();
			}
		}

		private void RefreshPosition() {
			Vector3 position = transform.localPosition;
			position.y = elevation * HexMetrics.elevationStep;
			position.y +=
				(HexMetrics.SampleNoise(position).y * 2f - 1f) *
				HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
		}

		public RectTransform uiRect;
		public HexGridChunk chunk;

		#region Rivers
		private bool hasIncomingRiver, hasOutgoingRiver;
		private HexDirection incomingRiver, outgoingRiver;

		public bool HasIncomingRiver => hasIncomingRiver;
		public bool HasOutgoingRiver => hasOutgoingRiver;
		public HexDirection IncomingRiver => incomingRiver;
		public HexDirection OutgoingRiver => outgoingRiver;

		public bool HasRiver => hasIncomingRiver || hasOutgoingRiver;
		public bool HasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;
		public bool HasRiverThroughEdge(HexDirection direction) {
			return
				hasIncomingRiver && incomingRiver == direction ||
				hasOutgoingRiver && outgoingRiver == direction;
		}

		public void RemoveIncomingRiver() {
			if (!hasIncomingRiver) {
				return;
			}
			hasIncomingRiver = false;
			RefreshSelfOnly();

			HexCell neighbor = GetNeighbor(incomingRiver);
			neighbor.hasOutgoingRiver = false;
			neighbor.RefreshSelfOnly();
		}

		public void RemoveOutgoingRiver() {
			if (!hasOutgoingRiver) {
				return;
			}
			hasOutgoingRiver = false;
			RefreshSelfOnly();

			HexCell neighbor = GetNeighbor(outgoingRiver);
			neighbor.hasIncomingRiver = false;
			neighbor.RefreshSelfOnly();
		}

		public void RemoveRiver() {
			RemoveOutgoingRiver();
			RemoveIncomingRiver();
		}

		private bool IsValidRiverDestination(HexCell neighbor) {
			return neighbor && (
				elevation >= neighbor.elevation || waterLevel == neighbor.elevation
			);
		}

		public void SetOutgoingRiver(HexDirection direction) {
			if (hasOutgoingRiver && outgoingRiver == direction) {
				return;
			}

			HexCell neighbor = GetNeighbor(direction);
			if (!IsValidRiverDestination(neighbor)) {
				return;
			}

			RemoveOutgoingRiver();
			if (hasIncomingRiver && incomingRiver == direction) {
				RemoveIncomingRiver();
			}

			hasOutgoingRiver = true;
			outgoingRiver = direction;
			specialIndex = 0;

			neighbor.RemoveIncomingRiver();
			neighbor.hasIncomingRiver = true;
			neighbor.incomingRiver = direction.Opposite();
			neighbor.specialIndex = 0;

			SetRoad((int)direction, false);
		}

		public float StreamBedY =>
			(elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
		public float RiverSurfaceY =>
			(elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
		public HexDirection RiverBeginOrEndDirection => hasIncomingRiver ? incomingRiver : outgoingRiver;

		private void ValidateRivers() {
			if (
				hasOutgoingRiver &&
				!IsValidRiverDestination(GetNeighbor(outgoingRiver))
			) {
				RemoveOutgoingRiver();
			}
			if (
				hasIncomingRiver &&
				!GetNeighbor(incomingRiver).IsValidRiverDestination(this)
			) {
				RemoveIncomingRiver();
			}
		}

		#endregion // Rivers

		#region Roads
		public bool HasRoadThroughEdge(HexDirection direction) {
			return roads[(int)direction];
		}

		public bool HasRoads {
			get {
				for (int i = 0; i < roads.Length; i++) {
					if (roads[i]) {
						return true;
					}
				}
				return false;
			}
		}

		public int GetElevationDifference(HexDirection direction) {
			int difference = elevation - GetNeighbor(direction).elevation;
			return difference >= 0 ? difference : -difference;
		}

		public void AddRoad(HexDirection direction) {
			if (!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
				!IsSpecial && !GetNeighbor(direction).IsSpecial &&
				GetElevationDifference(direction) <= 1) {
				SetRoad((int)direction, true);
			}
		}

		public void RemoveRoads() {
			for (int i = 0; i < neighbors.Length; i++) {
				if (roads[i]) {
					SetRoad(i, false);
				}
			}
		}

		private void SetRoad(int index, bool state) {
			roads[index] = state;
			neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
			neighbors[index].RefreshSelfOnly();
			RefreshSelfOnly();
		}
		#endregion

		#region Water
		private int waterLevel;
		public int WaterLevel {
			get => waterLevel;
			set {
				if (waterLevel == value) {
					return;
				}
				waterLevel = value;
				ValidateRivers();
				Refresh();
			}
		}

		public bool IsUnderwater => waterLevel > elevation;

		public float WaterSurfaceY =>
			(waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;

		#endregion

		#region Features
		public int UrbanLevel {
			get => urbanLevel;
			set {
				if (urbanLevel != value) {
					urbanLevel = value;
					RefreshSelfOnly();
				}
			}
		}

		public int FarmLevel {
			get => farmLevel;
			set {
				if (farmLevel != value) {
					farmLevel = value;
					RefreshSelfOnly();
				}
			}
		}

		public int PlantLevel {
			get => plantLevel;
			set {
				if (plantLevel != value) {
					plantLevel = value;
					RefreshSelfOnly();
				}
			}
		}

		private int urbanLevel, farmLevel, plantLevel;

		public int SpecialIndex {
			get => specialIndex;
			set {
				if (specialIndex != value && !HasRiver) {
					specialIndex = value;
					RemoveRoads();
					RefreshSelfOnly();
				}
			}
		}

		public bool IsSpecial => specialIndex > 0;

		private int specialIndex;
		#endregion

		#region Walls
		public bool Walled {
			get => walled;
			set {
				if (walled != value) {
					walled = value;
					Refresh();
				}
			}
		}

		private bool walled;
		#endregion

		public HexCell GetNeighbor(HexDirection direction) {
			return neighbors[(int)direction];
		}

		public void SetNeighbor(HexDirection direction, HexCell cell) {
			neighbors[(int)direction] = cell;
			cell.neighbors[(int)direction.Opposite()] = this;
		}

		public HexEdgeType GetEdgeType(HexDirection direction) {
			return HexMetrics.GetEdgeType(
				elevation, neighbors[(int)direction].elevation
			);
		}

		public HexEdgeType GetEdgeType(HexCell otherCell) {
			return HexMetrics.GetEdgeType(
				elevation, otherCell.elevation
			);
		}

		private void Refresh() {
			if (chunk) {
				chunk.Refresh();
				for (int i = 0; i < neighbors.Length; i++) {
					HexCell neighbor = neighbors[i];
					if (neighbor != null && neighbor.chunk != chunk) {
						neighbor.chunk.Refresh();
					}
				}
			}
		}

		private void RefreshSelfOnly() {
			chunk.Refresh();
		}

		public void Save(BinaryWriter writer) {
			writer.Write(terrainTypeIndex);
			writer.Write(elevation);
			writer.Write(waterLevel);
			writer.Write(urbanLevel);
			writer.Write(farmLevel);
			writer.Write(plantLevel);
			writer.Write(specialIndex);

			writer.Write(walled);

			writer.Write(hasIncomingRiver);
			writer.Write((int)incomingRiver);
			writer.Write(hasOutgoingRiver);
			writer.Write((int)outgoingRiver);

			for (int i = 0; i < roads.Length; i++) {
				writer.Write(roads[i]);
			}
		}

		public void Load(BinaryReader reader) {
			terrainTypeIndex = reader.ReadInt32();
			elevation = reader.ReadInt32();
			RefreshPosition();
			waterLevel = reader.ReadInt32();
			urbanLevel = reader.ReadInt32();
			farmLevel = reader.ReadInt32();
			plantLevel = reader.ReadInt32();
			specialIndex = reader.ReadInt32();

			walled = reader.ReadBoolean();

			hasIncomingRiver = reader.ReadBoolean();
			incomingRiver = (HexDirection)reader.ReadInt32();

			hasOutgoingRiver = reader.ReadBoolean();
			outgoingRiver = (HexDirection)reader.ReadInt32();

			for (int i = 0; i < roads.Length; i++) {
				roads[i] = reader.ReadBoolean();
			}
		}
	}
}
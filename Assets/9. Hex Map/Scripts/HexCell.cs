using UnityEngine;

namespace HexMap
{
	public class HexCell : MonoBehaviour
	{
		public HexCoordinates coordinates;

		private Color color;
		public Color Color {
			get => color;
			set {
				if (color == value) {
					return;
				}
				color = value;
				Refresh();
			}
		}

		[SerializeField]
		private HexCell[] neighbors = null;

		public Vector3 Position => transform.localPosition;

		private int elevation = int.MinValue;
		public int Elevation {
			get => elevation;
			set {
				if (elevation == value) {
					return;
				}

				elevation = value;

				Vector3 position = transform.localPosition;
				position.y = value * HexMetrics.elevationStep;
				position.y +=
					(HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
				transform.localPosition = position;

				Vector3 uiPosition = uiRect.localPosition;
				uiPosition.z = -position.y;
				uiRect.localPosition = uiPosition;

				// River Exception
				if (
					hasOutgoingRiver &&
					elevation < GetNeighbor(outgoingRiver).elevation) {
					RemoveOutgoingRiver();
				}
				if (
					hasIncomingRiver &&
					elevation > GetNeighbor(incomingRiver).elevation
				) {
					RemoveIncomingRiver();
				}

				Refresh();
			}
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

		public void SetOutgoingRiver(HexDirection direction) {
			if (hasOutgoingRiver && outgoingRiver == direction) {
				return;
			}

			HexCell neighbor = GetNeighbor(direction);
			if (!neighbor || elevation < neighbor.elevation) {
				return;
			}

			RemoveOutgoingRiver();
			if (hasIncomingRiver && incomingRiver == direction) {
				RemoveIncomingRiver();
			}

			hasOutgoingRiver = true;
			outgoingRiver = direction;
			RefreshSelfOnly();

			neighbor.RemoveIncomingRiver();
			neighbor.hasIncomingRiver = true;
			neighbor.incomingRiver = direction.Opposite();
			neighbor.RefreshSelfOnly();
		}
		#endregion // Rivers

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
	}
}
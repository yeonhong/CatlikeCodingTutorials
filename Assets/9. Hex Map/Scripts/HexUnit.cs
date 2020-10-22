﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HexMap
{
	public class HexUnit : MonoBehaviour
	{
		const float travelSpeed = 4f;

		public static HexUnit unitPrefab;

		public HexCell Location {
			get => location;
			set {
				if (location) {
					location.Unit = null;
				}
				location = value;
				value.Unit = this;
				transform.localPosition = value.Position;
			}
		}

		private HexCell location;

		public float Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
				transform.localRotation = Quaternion.Euler(0f, value, 0f);
			}
		}

		private float orientation;
		private List<HexCell> pathToTravel;

		void OnEnable() {
			if (location) {
				transform.localPosition = location.Position;
			}
		}

		public void ValidateLocation() {
			transform.localPosition = location.Position;
		}

		public void Die() {
			location.Unit = null;
			Destroy(gameObject);
		}

		public void Save(BinaryWriter writer) {
			location.coordinates.Save(writer);
			writer.Write(orientation);
		}

		public static void Load(BinaryReader reader, HexGrid grid) {
			HexCoordinates coordinates = HexCoordinates.Load(reader);
			float orientation = reader.ReadSingle();
			grid.AddUnit(
				Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
			);
		}

		public bool IsValidDestination(HexCell cell) {
			return !cell.IsUnderwater && !cell.Unit;
		}

		public void Travel(List<HexCell> path) {
			Location = path[path.Count - 1];
			pathToTravel = path;
			StopAllCoroutines();
			StartCoroutine(TravelPath());
		}

		private void OnDrawGizmos() {
			if (pathToTravel == null || pathToTravel.Count == 0) {
				return;
			}

			for (int i = 1; i < pathToTravel.Count; i++) {
				Vector3 a = pathToTravel[i - 1].Position;
				Vector3 b = pathToTravel[i].Position;
				for (float t = 0f; t < 1f; t += 0.1f) {
					Gizmos.DrawSphere(Vector3.Lerp(a, b, t), 2f);
				}
			}
		}

		private IEnumerator TravelPath() {
			for (int i = 1; i < pathToTravel.Count; i++) {
				Vector3 a = pathToTravel[i - 1].Position;
				Vector3 b = pathToTravel[i].Position;
				for (float t = 0f; t < 1f; t += Time.deltaTime * travelSpeed) {
					transform.localPosition = Vector3.Lerp(a, b, t);
					yield return null;
				}
			}
		}
	}
}
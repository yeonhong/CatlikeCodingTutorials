using UnityEngine;

namespace TowerDefense
{
	public class GameTile : MonoBehaviour
	{
		[SerializeField] private Transform arrow = default;
		private GameTile north, east, south, west, nextOnPath;
		private int distance;
		public bool HasPath => distance != int.MaxValue;

		public static void MakeEastWestNeighbors(GameTile east, GameTile west) {
			Debug.Assert(
				west.east == null && east.west == null, "Redefined neighbors!"
			);
			west.east = east;
			east.west = west;
		}

		public static void MakeNorthSouthNeighbors(GameTile north, GameTile south) {
			Debug.Assert(
				south.north == null && north.south == null, "Redefined neighbors!"
			);
			south.north = north;
			north.south = south;
		}

		public void ClearPath() {
			distance = int.MaxValue;
			nextOnPath = null;
		}

		public void BecomeDestination() {
			distance = 0;
			nextOnPath = null;
		}
	}
}
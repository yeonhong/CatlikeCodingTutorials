﻿using UnityEngine;

namespace ObjectManagement
{
	public class Shape : PersistableObject
	{
		public int ShapeId {
			get => shapeId;
			set {
				if (shapeId == int.MinValue && value != int.MinValue) {
					shapeId = value;
				}
				else {
					Debug.LogError("Not allowed to change shapeId.");
				}
			}
		}

		private int shapeId = int.MinValue;
	}
}

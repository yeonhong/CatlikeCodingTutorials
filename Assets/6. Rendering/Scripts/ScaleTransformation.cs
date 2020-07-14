﻿using UnityEngine;

namespace Rendering
{
	public class ScaleTransformation : Transformation
	{
		public Vector3 scale = Vector3.one;

		public override Vector3 Apply(Vector3 point) {
			point.x *= scale.x;
			point.y *= scale.y;
			point.z *= scale.z;
			return point;
		}
	}
}
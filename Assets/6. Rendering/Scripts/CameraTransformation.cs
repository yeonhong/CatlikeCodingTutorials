using UnityEngine;

namespace Rendering
{
	public class CameraTransformation : Transformation
	{
		public bool isOrtho = false;
		public float focalLength = 1f;

		public override Matrix4x4 Matrix {

			get {
				Matrix4x4 matrix = new Matrix4x4();
				if (isOrtho) {
					matrix.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
					matrix.SetRow(1, new Vector4(0f, 1f, 0f, 0f));
					matrix.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
					matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
				} else {
					matrix.SetRow(0, new Vector4(focalLength, 0f, 0f, 0f));
					matrix.SetRow(1, new Vector4(0f, focalLength, 0f, 0f));
					matrix.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
					matrix.SetRow(3, new Vector4(0f, 0f, 1f, 0f));
				}
				return matrix;
			}
		}
	}
}
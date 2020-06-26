using UnityEngine;

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

		public int MaterialId { get; private set; }

		public void SetMaterial(Material material, int materialId) {
			GetComponent<MeshRenderer>().material = material;
			MaterialId = materialId;
		}
	}
}

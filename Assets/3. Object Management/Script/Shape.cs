using UnityEngine;

namespace ObjectManagement
{
	public class Shape : PersistableObject
	{
		private int shapeId = int.MinValue;
		public int MaterialId { get; private set; }
		private Color color;

		private static int colorPropertyId = Shader.PropertyToID("_Color");
		private static MaterialPropertyBlock sharedPropertyBlock;

		public Vector3 AngularVelocity { get; set; }
		public Vector3 Velocity { get; set; }

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

		[SerializeField] private MeshRenderer[] meshRenderers = null;

		private Color[] colors;
		public int ColorCount => colors.Length;

		public ShapeFactory OriginFactory {
			get => originFactory;
			set {
				if (originFactory == null) {
					originFactory = value;
				}
				else {
					Debug.LogError("Not allowed to change origin factory.");
				}
			}
		}

		private ShapeFactory originFactory;

		private void Awake() {
			colors = new Color[meshRenderers.Length];
		}

		public void SetMaterial(Material material, int materialId) {
			for (int i = 0; i < meshRenderers.Length; i++) {
				meshRenderers[i].material = material;
			}
			MaterialId = materialId;
		}

		public void SetColor(Color color) {
			if (sharedPropertyBlock == null) {
				sharedPropertyBlock = new MaterialPropertyBlock();
			}
			sharedPropertyBlock.SetColor(colorPropertyId, color);
			for (int i = 0; i < meshRenderers.Length; i++) {
				colors[i] = color;
				meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
			}
		}

		public void SetColor(Color color, int index) {
			if (sharedPropertyBlock == null) {
				sharedPropertyBlock = new MaterialPropertyBlock();
			}
			sharedPropertyBlock.SetColor(colorPropertyId, color);
			colors[index] = color;
			meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
		}

		public override void Save(GameDataWriter writer) {
			base.Save(writer);
			writer.Write(colors.Length);
			for (int i = 0; i < colors.Length; i++) {
				writer.Write(colors[i]);
			}
			writer.Write(AngularVelocity);
			writer.Write(Velocity);
		}

		public override void Load(GameDataReader reader) {
			base.Load(reader);
			if (reader.Version >= 5) {
				LoadColors(reader);
			}
			else {
				SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
			}
			AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
			Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
		}

		public void GameUpdate() {
			transform.Rotate(AngularVelocity * Time.deltaTime);
			transform.localPosition += Velocity * Time.deltaTime;
		}

		private void LoadColors(GameDataReader reader) {
			int count = reader.ReadInt();
			int max = count <= colors.Length ? count : colors.Length;
			int i = 0;
			for (; i < max; i++) {
				SetColor(reader.ReadColor(), i);
			}

			if (count > colors.Length) {
				for (; i < count; i++) {
					reader.ReadColor();
				}
			}
			else if (count < colors.Length) {
				for (; i < colors.Length; i++) {
					SetColor(Color.white, i);
				}
			}
		}

		public void Recycle() {
			OriginFactory.Reclaim(this);
		}
	}
}

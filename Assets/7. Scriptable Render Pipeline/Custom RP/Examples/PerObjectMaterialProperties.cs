using UnityEngine;

namespace CustomRP
{
	[DisallowMultipleComponent]
	public class PerObjectMaterialProperties : MonoBehaviour
	{
		private static int baseColorId = Shader.PropertyToID("_BaseColor");
		private static MaterialPropertyBlock block;

		[SerializeField]
		private Color baseColor = Color.white;

		void Awake() {
			OnValidate();
		}

		void OnValidate() {
			if (block == null) {
				block = new MaterialPropertyBlock();
			}
			block.SetColor(baseColorId, baseColor);
			GetComponent<Renderer>().SetPropertyBlock(block);
		}
	}
}
using UnityEngine;

namespace Rendering
{
	public class EmissiveOscillator : MonoBehaviour
	{
		MeshRenderer emissiveRenderer;
		Material emissiveMaterial;

		void Start() {
			emissiveRenderer = GetComponent<MeshRenderer>();
			emissiveMaterial = emissiveRenderer.material;
		}

		private void Update() {
			Color c = Color.Lerp(
				Color.white, Color.black,
				Mathf.Sin(Time.time * Mathf.PI) * 0.5f + 0.5f
			);
			emissiveMaterial.SetColor("_Emission", c);
			//emissiveRenderer.UpdateGIMaterials();
			DynamicGI.SetEmissive(emissiveRenderer, c);
		}
	} 
}
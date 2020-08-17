using System;
using UnityEngine;

namespace Rendering
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class DepthOfFieldEffect : MonoBehaviour
	{
		const int circleOfConfusionPass = 0;

		[HideInInspector]
		public Shader dofShader;

		[NonSerialized]
		private Material dofMaterial;

		[Range(0.1f, 100f)] public float focusDistance = 10f;
		[Range(0.1f, 10f)] public float focusRange = 3f;

		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (dofMaterial == null) {
				dofMaterial = new Material(dofShader);
				dofMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			dofMaterial.SetFloat("_FocusDistance", focusDistance);
			dofMaterial.SetFloat("_FocusRange", focusRange);

			var coc = RenderTexture.GetTemporary(
				source.width, source.height, 0,
				RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear
			);

			Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);
			Graphics.Blit(coc, destination);

			RenderTexture.ReleaseTemporary(coc);
		}
	}
}
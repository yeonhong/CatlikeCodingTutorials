using UnityEngine;
using System;

namespace Rendering
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class BloomEffect : MonoBehaviour
	{
		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			Graphics.Blit(source, destination);
		}
	} 
}
﻿using System;
using UnityEngine;

namespace Rendering
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class DepthOfFieldEffect : MonoBehaviour
	{
		[HideInInspector]
		public Shader dofShader;

		[NonSerialized]
		private Material dofMaterial;

		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (dofMaterial == null) {
				dofMaterial = new Material(dofShader);
				dofMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			Graphics.Blit(source, destination, dofMaterial);
		}
	}
}
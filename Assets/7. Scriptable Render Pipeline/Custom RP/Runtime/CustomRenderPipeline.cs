﻿using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public class CustomRenderPipeline : RenderPipeline
	{
		private CameraRenderer renderer = new CameraRenderer();
		bool useDynamicBatching, useGPUInstancing;

		public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher) {
			this.useDynamicBatching = useDynamicBatching;
			this.useGPUInstancing = useGPUInstancing;
			GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
		}

		protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
			foreach (Camera camera in cameras) {
				renderer.Render(
					context, camera, useDynamicBatching, useGPUInstancing
				);
			}
		}
	}
}
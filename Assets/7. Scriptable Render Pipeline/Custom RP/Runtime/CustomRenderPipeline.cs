using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public partial class CustomRenderPipeline : RenderPipeline
	{
		private CameraRenderer renderer = new CameraRenderer();
		private bool useDynamicBatching, useGPUInstancing, useLightsPerObject;
		private ShadowSettings shadowSettings;
		private PostFXSettings postFXSettings;

		public CustomRenderPipeline(
			bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
			bool useLightsPerObject, ShadowSettings shadowSettings,
			PostFXSettings postFXSettings
		) {
			this.postFXSettings = postFXSettings;
			this.shadowSettings = shadowSettings;
			this.useDynamicBatching = useDynamicBatching;
			this.useGPUInstancing = useGPUInstancing;
			this.useLightsPerObject = useLightsPerObject;
			GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
			GraphicsSettings.lightsUseLinearIntensity = true;
			InitializeForEditor();
		}

		protected override void Render(
			ScriptableRenderContext context, Camera[] cameras
		) {
			foreach (Camera camera in cameras) {
				renderer.Render(
					context, camera,
					useDynamicBatching, useGPUInstancing, useLightsPerObject,
					shadowSettings, postFXSettings
				);
			}
		}
	}
}
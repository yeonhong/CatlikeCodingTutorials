using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public partial class CustomRenderPipeline : RenderPipeline
	{
		private CameraRenderer renderer = new CameraRenderer();
		private bool useDynamicBatching, useGPUInstancing;
		private ShadowSettings shadowSettings;

		public CustomRenderPipeline(
			bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, 
			ShadowSettings shadowSettings) {

			this.shadowSettings = shadowSettings;
			this.useDynamicBatching = useDynamicBatching;
			this.useGPUInstancing = useGPUInstancing;
			GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
			GraphicsSettings.lightsUseLinearIntensity = true;

			InitializeForEditor();
		}

		protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
			foreach (Camera camera in cameras) {
				renderer.Render(
					context, camera, useDynamicBatching, useGPUInstancing, shadowSettings
				);
			}
		}
	}
}
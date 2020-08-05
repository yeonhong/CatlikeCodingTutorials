using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public partial class CameraRenderer
	{
		private ScriptableRenderContext context;
		private Camera camera;
		private const string bufferName = "Render Camera";
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private CullingResults cullingResults;

		private static ShaderTagId	unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
									litShaderTagId = new ShaderTagId("CustomLit");

		Lighting lighting = new Lighting();

		public void Render(
			ScriptableRenderContext context, Camera camera,
			bool useDynamicBatching, bool useGPUInstancing,
			ShadowSettings shadowSettings
		) {
			this.context = context;
			this.camera = camera;

			PrepareBuffer();
			PrepareForSceneWindow(); // draw ugui
			if (!Cull(shadowSettings.maxDistance)) {
				return;
			}

			buffer.BeginSample(SampleName);
			ExecuteBuffer();
			lighting.Setup(context, cullingResults, shadowSettings);
			buffer.EndSample(SampleName);
			Setup();
			DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
			DrawUnsupportedShaders();
			DrawGizmos();
			lighting.Cleanup();
			Submit();
		}

		private void ExecuteBuffer() {
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		private void Setup() {
			context.SetupCameraProperties(camera);

			CameraClearFlags flags = camera.clearFlags;
			buffer.ClearRenderTarget(
				flags <= CameraClearFlags.Depth,
				flags == CameraClearFlags.Color,
				flags == CameraClearFlags.Color ?
					camera.backgroundColor.linear : Color.clear
			);
			buffer.BeginSample(SampleName);
			ExecuteBuffer();
		}

		private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing) {
			// draw opaque
			var sortingSettings = new SortingSettings(camera) {
				criteria = SortingCriteria.CommonOpaque
			};
			var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) {
				enableDynamicBatching = useDynamicBatching,
				enableInstancing = useGPUInstancing,
				perObjectData = PerObjectData.Lightmaps |
								PerObjectData.ShadowMask |
								PerObjectData.LightProbe |
								PerObjectData.OcclusionProbe |
								PerObjectData.OcclusionProbeProxyVolume |
								PerObjectData.LightProbeProxyVolume
			};
			drawingSettings.SetShaderPassName(1, litShaderTagId);

			var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
			context.DrawRenderers(
				cullingResults, ref drawingSettings, ref filteringSettings
			);

			// draw skybox
			context.DrawSkybox(camera);

			// draw transparent
			sortingSettings.criteria = SortingCriteria.CommonTransparent;
			drawingSettings.sortingSettings = sortingSettings;
			filteringSettings.renderQueueRange = RenderQueueRange.transparent;

			context.DrawRenderers(
				cullingResults, ref drawingSettings, ref filteringSettings
			);
		}

		private void Submit() {
			buffer.EndSample(SampleName);
			ExecuteBuffer();

			context.Submit();
		}

		private bool Cull(float maxShadowDistance) {
			if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
				p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
				cullingResults = context.Cull(ref p);
				return true;
			}
			return false;
		}
	}
}
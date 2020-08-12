using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public partial class CameraRenderer
	{
		private const string bufferName = "Render Camera";
		private static ShaderTagId
			unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
			litShaderTagId = new ShaderTagId("CustomLit");
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

		private ScriptableRenderContext context;
		private Camera camera;
		private CullingResults cullingResults;
		private Lighting lighting = new Lighting();
		private PostFXStack postFXStack = new PostFXStack();

		public void Render(
			ScriptableRenderContext context, Camera camera,
			bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
			ShadowSettings shadowSettings, PostFXSettings postFXSettings
		) {
			this.context = context;
			this.camera = camera;

			PrepareBuffer();
			PrepareForSceneWindow();
			if (!Cull(shadowSettings.maxDistance)) {
				return;
			}

			buffer.BeginSample(SampleName);
			ExecuteBuffer();
			lighting.Setup(context, cullingResults, shadowSettings, useLightsPerObject);
			postFXStack.Setup(context, camera, postFXSettings);
			buffer.EndSample(SampleName);
			Setup();
			DrawVisibleGeometry(useDynamicBatching, useGPUInstancing, useLightsPerObject);
			DrawUnsupportedShaders();
			DrawGizmosBeforeFX();
			if (postFXStack.IsActive) {
				postFXStack.Render(frameBufferId);
			}
			DrawGizmosAfterFX();
			Cleanup();
			Submit();
		}

		private bool Cull(float maxShadowDistance) {
			if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
				p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
				cullingResults = context.Cull(ref p);
				return true;
			}
			return false;
		}

		private void Setup() {
			context.SetupCameraProperties(camera);
			CameraClearFlags flags = camera.clearFlags;

			if (postFXStack.IsActive) {
				if (flags > CameraClearFlags.Color) {
					flags = CameraClearFlags.Color;
				}
				buffer.GetTemporaryRT(
					frameBufferId, camera.pixelWidth, camera.pixelHeight,
					32, FilterMode.Bilinear, RenderTextureFormat.Default
				);
				buffer.SetRenderTarget(
					frameBufferId,
					RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
				);
			}

			buffer.ClearRenderTarget(
				flags <= CameraClearFlags.Depth,
				flags == CameraClearFlags.Color,
				flags == CameraClearFlags.Color ?
					camera.backgroundColor.linear : Color.clear
			);
			buffer.BeginSample(SampleName);
			ExecuteBuffer();
		}

		void Cleanup() {
			lighting.Cleanup();
			if (postFXStack.IsActive) {
				buffer.ReleaseTemporaryRT(frameBufferId);
			}
		}

		private void Submit() {
			buffer.EndSample(SampleName);
			ExecuteBuffer();
			context.Submit();
		}

		private void ExecuteBuffer() {
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		private void DrawVisibleGeometry(
			bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject
		) {
			PerObjectData lightsPerObjectFlags = useLightsPerObject ?
				PerObjectData.LightData | PerObjectData.LightIndices :
				PerObjectData.None;
			var sortingSettings = new SortingSettings(camera) {
				criteria = SortingCriteria.CommonOpaque
			};
			var drawingSettings = new DrawingSettings(
				unlitShaderTagId, sortingSettings
			) {
				enableDynamicBatching = useDynamicBatching,
				enableInstancing = useGPUInstancing,
				perObjectData =
					PerObjectData.ReflectionProbes |
					PerObjectData.Lightmaps | PerObjectData.ShadowMask |
					PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
					PerObjectData.LightProbeProxyVolume |
					PerObjectData.OcclusionProbeProxyVolume |
					lightsPerObjectFlags
			};
			drawingSettings.SetShaderPassName(1, litShaderTagId);

			var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

			context.DrawRenderers(
				cullingResults, ref drawingSettings, ref filteringSettings
			);

			context.DrawSkybox(camera);

			sortingSettings.criteria = SortingCriteria.CommonTransparent;
			drawingSettings.sortingSettings = sortingSettings;
			filteringSettings.renderQueueRange = RenderQueueRange.transparent;

			context.DrawRenderers(
				cullingResults, ref drawingSettings, ref filteringSettings
			);
		}
	}
}
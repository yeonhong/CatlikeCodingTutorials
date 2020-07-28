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

		private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

		public void Render(ScriptableRenderContext context, Camera camera) {
			this.context = context;
			this.camera = camera;

			PrepareForSceneWindow(); // draw ugui
			if (!Cull()) {
				return;
			}

			Setup();
			DrawVisibleGeometry();
			DrawUnsupportedShaders();
			DrawGizmos();
			Submit();
		}

		private void ExecuteBuffer() {
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		private void Setup() {
			context.SetupCameraProperties(camera);

			buffer.ClearRenderTarget(true, true, Color.clear);
			buffer.BeginSample(bufferName);
			ExecuteBuffer();
		}

		private void DrawVisibleGeometry() {
			// draw opaque
			var sortingSettings = new SortingSettings(camera) {
				criteria = SortingCriteria.CommonOpaque
			};
			var drawingSettings = new DrawingSettings(
				unlitShaderTagId, sortingSettings
			);
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
			buffer.EndSample(bufferName);
			ExecuteBuffer();

			context.Submit();
		}

		private bool Cull() {
			if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
				cullingResults = context.Cull(ref p);
				return true;
			}
			return false;
		}
	}
}
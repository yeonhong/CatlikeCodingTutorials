using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public class Shadows
	{
		private const string bufferName = "Shadows";
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private ScriptableRenderContext context;
		private CullingResults cullingResults;
		private ShadowSettings settings;

		private const int maxShadowedDirectionalLightCount = 4;
		private int ShadowedDirectionalLightCount;

		private struct ShadowedDirectionalLight
		{
			public int visibleLightIndex;
		}

		private ShadowedDirectionalLight[] ShadowedDirectionalLights =
			new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

		private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

		public void Setup(
			ScriptableRenderContext context, CullingResults cullingResults,
			ShadowSettings settings
		) {
			this.context = context;
			this.cullingResults = cullingResults;
			this.settings = settings;
			ShadowedDirectionalLightCount = 0;
		}

		private void ExecuteBuffer() {
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		public void ReserveDirectionalShadows(Light light, int visibleLightIndex) {

			if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
				light.shadows != LightShadows.None && light.shadowStrength > 0f &&
				cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
			) {
				ShadowedDirectionalLights[ShadowedDirectionalLightCount++] =
					new ShadowedDirectionalLight {
						visibleLightIndex = visibleLightIndex
					};
			}
		}

		public void Render() {
			if (ShadowedDirectionalLightCount > 0) {
				RenderDirectionalShadows();
			}
		}

		private void RenderDirectionalShadows() {
			int atlasSize = (int)settings.directional.atlasSize;
			buffer.GetTemporaryRT(
				dirShadowAtlasId, atlasSize, atlasSize,
				32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
			);

			buffer.SetRenderTarget(
				dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
			);

			buffer.ClearRenderTarget(true, false, Color.clear);

			buffer.BeginSample(bufferName);
			ExecuteBuffer();

			int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
			int tileSize = atlasSize / split;
			for (int i = 0; i < ShadowedDirectionalLightCount; i++) {
				RenderDirectionalShadows(i, split, tileSize);
			}
			buffer.EndSample(bufferName);
			ExecuteBuffer();
		}

		private void SetTileViewport(int index, int split, float tileSize) {
			Vector2 offset = new Vector2(index % split, index / split);
			buffer.SetViewport(new Rect(
				offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
			));
		}

		private void RenderDirectionalShadows(int index, int split, int tileSize) {
			ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
			var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);

			cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
				light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
				out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
				out ShadowSplitData splitData
			);

			shadowSettings.splitData = splitData;
			SetTileViewport(index, split, tileSize);
			buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			ExecuteBuffer();
			context.DrawShadows(ref shadowSettings);
		}

		public void Cleanup() {
			if (ShadowedDirectionalLightCount > 0) {
				buffer.ReleaseTemporaryRT(dirShadowAtlasId);
				ExecuteBuffer();
			}
		}
	}
}
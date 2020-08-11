using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public class Shadows
	{
		private const int maxShadowedDirectionalLightCount = 4, maxShadowedOtherLightCount = 16;
		private const int maxCascades = 4;

		private const string bufferName = "Shadows";
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private ScriptableRenderContext context;
		private CullingResults cullingResults;
		private ShadowSettings settings;

		private struct ShadowedDirectionalLight
		{
			public int visibleLightIndex;
			public float slopeScaleBias;
			public float nearPlaneOffset;
		}

		private ShadowedDirectionalLight[] shadowedDirectionalLights =
			new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

		private int ShadowedDirectionalLightCount, shadowedOtherLightCount;

		private static int
			dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
			dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
			otherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas"),
			otherShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices"),
			cascadeCountId = Shader.PropertyToID("_CascadeCount"),
			cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
			cascadeDataId = Shader.PropertyToID("_CascadeData"),
			shadowAtlastSizeId = Shader.PropertyToID("_ShadowAtlasSize"),
			shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

		private static string[] directionalFilterKeywords = {
			"_DIRECTIONAL_PCF3",
			"_DIRECTIONAL_PCF5",
			"_DIRECTIONAL_PCF7",
		};

		private static string[] otherFilterKeywords = {
			"_OTHER_PCF3",
			"_OTHER_PCF5",
			"_OTHER_PCF7",
		};

		private static string[] cascadeBlendKeywords = {
			"_CASCADE_BLEND_SOFT",
			"_CASCADE_BLEND_DITHER"
		};

		private static Matrix4x4[]
			dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades],
			otherShadowMatrices = new Matrix4x4[maxShadowedOtherLightCount];

		private static Vector4[]
			cascadeCullingSpheres = new Vector4[maxCascades],
			cascadeData = new Vector4[maxCascades];

		private static string[] shadowMaskKeywords = {
			"_SHADOW_MASK_ALWAYS",
			"_SHADOW_MASK_DISTANCE"
		};

		private bool useShadowMask;
		private Vector4 atlasSizes;

		public void Setup(
			ScriptableRenderContext context, CullingResults cullingResults,
			ShadowSettings settings) {

			this.context = context;
			this.cullingResults = cullingResults;
			this.settings = settings;
			ShadowedDirectionalLightCount = shadowedOtherLightCount = 0;
			useShadowMask = false;
		}

		public void Cleanup() {
			if (ShadowedDirectionalLightCount > 0) {
				buffer.ReleaseTemporaryRT(dirShadowAtlasId);
				ExecuteBuffer();
			}
			if (shadowedOtherLightCount > 0) {
				buffer.ReleaseTemporaryRT(otherShadowAtlasId);
				ExecuteBuffer();
			}
		}

		public Vector4 ReserveDirectionalShadows(Light light, int visibleLightIndex) {

			if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
				light.shadows != LightShadows.None && light.shadowStrength > 0f) {

				float maskChannel = -1;
				LightBakingOutput lightBaking = light.bakingOutput;
				if (lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
					lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask) {
					useShadowMask = true;
					maskChannel = lightBaking.occlusionMaskChannel;
				}

				if (!cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
					return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
				}

				shadowedDirectionalLights[ShadowedDirectionalLightCount] =
					new ShadowedDirectionalLight {
						visibleLightIndex = visibleLightIndex,
						slopeScaleBias = light.shadowBias,
						nearPlaneOffset = light.shadowNearPlane
					};
				return new Vector4(
					light.shadowStrength,
					settings.directional.cascadeCount * ShadowedDirectionalLightCount++,
					light.shadowNormalBias, maskChannel
				);
			}

			return Vector4.zero;
		}

		public void Render() {
			if (ShadowedDirectionalLightCount > 0) {
				RenderDirectionalShadows();
			}

			if (shadowedOtherLightCount > 0) {
				RenderOtherShadows();
			}

			buffer.BeginSample(bufferName);
			SetKeywords(shadowMaskKeywords, useShadowMask ?
				(QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1) : -1);

			buffer.SetGlobalInt(
				cascadeCountId,
				ShadowedDirectionalLightCount > 0 ? settings.directional.cascadeCount : 0);
			float f = 1f - settings.directional.cascadeFade;
			buffer.SetGlobalVector(
				shadowDistanceFadeId, new Vector4(
					1f / settings.maxDistance, 1f / settings.distanceFade,
					1f / (1f - f * f)
				)
			);
			buffer.SetGlobalVector(shadowAtlastSizeId, atlasSizes);

			buffer.EndSample(bufferName);
			ExecuteBuffer();
		}

		private void RenderDirectionalShadows() {
			int atlasSize = (int)settings.directional.atlasSize;
			atlasSizes.x = atlasSize;
			atlasSizes.y = 1f / atlasSize;

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

			int tiles = ShadowedDirectionalLightCount * settings.directional.cascadeCount;
			int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
			int tileSize = atlasSize / split;
			for (int i = 0; i < ShadowedDirectionalLightCount; i++) {
				RenderDirectionalShadows(i, split, tileSize);
			}

			buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
			buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
			buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

			SetKeywords(directionalFilterKeywords, (int)settings.directional.filter - 1);
			SetKeywords(cascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1);

			buffer.EndSample(bufferName);
			ExecuteBuffer();
		}

		private void RenderDirectionalShadows(int index, int split, int tileSize) {
			ShadowedDirectionalLight light = shadowedDirectionalLights[index];
			var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
			int cascadeCount = settings.directional.cascadeCount;
			int tileOffset = index * cascadeCount;
			Vector3 ratios = settings.directional.CascadeRatios;
			float cullingFactor = Mathf.Max(0f, 0.8f - settings.directional.cascadeFade);

			for (int i = 0; i < cascadeCount; i++) {
				cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
					light.visibleLightIndex, i, cascadeCount, ratios, tileSize, light.nearPlaneOffset,
					out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
					out ShadowSplitData splitData
				);
				splitData.shadowCascadeBlendCullingFactor = cullingFactor;
				shadowSettings.splitData = splitData;
				if (index == 0) {
					SetCascadeData(i, splitData.cullingSphere, tileSize);
				}
				int tileIndex = tileOffset + i;
				dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
					projectionMatrix * viewMatrix,
					SetTileViewport(tileIndex, split, tileSize), split
				);
				buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
				buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
				ExecuteBuffer();
				context.DrawShadows(ref shadowSettings);
				buffer.SetGlobalDepthBias(0f, 0f);
			}
		}

		private void RenderOtherShadows() {
			int atlasSize = (int)settings.other.atlasSize;
			atlasSizes.z = atlasSize;
			atlasSizes.w = 1f / atlasSize;
			buffer.GetTemporaryRT(
				otherShadowAtlasId, atlasSize, atlasSize,
				32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
			);
			buffer.SetRenderTarget(
				otherShadowAtlasId,
				RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
			);
			buffer.ClearRenderTarget(true, false, Color.clear);
			buffer.BeginSample(bufferName);
			ExecuteBuffer();

			int tiles = shadowedOtherLightCount;
			int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
			int tileSize = atlasSize / split;

			for (int i = 0; i < shadowedOtherLightCount; i++) {
				//RenderDirectionalShadows(i, split, tileSize);
			}

			//buffer.SetGlobalVectorArray(
			//	cascadeCullingSpheresId, cascadeCullingSpheres
			//);
			//buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
			buffer.SetGlobalMatrixArray(otherShadowMatricesId, otherShadowMatrices);
			SetKeywords(
				otherFilterKeywords, (int)settings.other.filter - 1
			);
			//SetKeywords(
			//	cascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1
			//);
			buffer.EndSample(bufferName);
			ExecuteBuffer();
		}

		private void SetCascadeData(int index, Vector4 cullingSphere, float tileSize) {
			float texelSize = 2f * cullingSphere.w / tileSize;
			float filterSize = texelSize * ((float)settings.directional.filter + 1f);
			cullingSphere.w -= filterSize;
			cullingSphere.w *= cullingSphere.w;
			cascadeCullingSpheres[index] = cullingSphere;
			cascadeData[index] = new Vector4(
				1f / cullingSphere.w,
				filterSize * 1.4142136f // 루트2
			);
		}

		private void SetKeywords(string[] keywords, int enabledIndex) {
			for (int i = 0; i < keywords.Length; i++) {
				if (i == enabledIndex) {
					buffer.EnableShaderKeyword(keywords[i]);
				}
				else {
					buffer.DisableShaderKeyword(keywords[i]);
				}
			}
		}

		private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split) {
			if (SystemInfo.usesReversedZBuffer) {
				m.m20 = -m.m20;
				m.m21 = -m.m21;
				m.m22 = -m.m22;
				m.m23 = -m.m23;
			}

			float scale = 1f / split;
			m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
			m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
			m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
			m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
			m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
			m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
			m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
			m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
			m.m20 = 0.5f * (m.m20 + m.m30);
			m.m21 = 0.5f * (m.m21 + m.m31);
			m.m22 = 0.5f * (m.m22 + m.m32);
			m.m23 = 0.5f * (m.m23 + m.m33);
			return m;
		}

		private Vector2 SetTileViewport(int index, int split, float tileSize) {
			Vector2 offset = new Vector2(index % split, index / split);
			buffer.SetViewport(new Rect(
				offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
			));
			return offset;
		}

		private void ExecuteBuffer() {
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		public Vector4 ReserveOtherShadows(Light light, int visibleLightIndex) {
			if (light.shadows == LightShadows.None || light.shadowStrength <= 0f) {
				return new Vector4(0f, 0f, 0f, -1f);
			}

			float maskChannel = -1f;
			LightBakingOutput lightBaking = light.bakingOutput;
			if (
				lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
				lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask
			) {
				useShadowMask = true;
				maskChannel = lightBaking.occlusionMaskChannel;
			}

			if (
				shadowedOtherLightCount >= maxShadowedOtherLightCount ||
				!cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
			) {
				return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
			}

			return new Vector4(
				light.shadowStrength, shadowedOtherLightCount++, 0f,
				maskChannel
			);
		}

	}
}
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public class Lighting
	{
		private const string bufferName = "Lighting";
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private const int maxDirLightCount = 4;
		private static int
			//dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
			//dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
			dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
			dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
			dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

		private static Vector4[]
			dirLightColors = new Vector4[maxDirLightCount],
			dirLightDirections = new Vector4[maxDirLightCount];

		private CullingResults cullingResults;

		public void Setup(ScriptableRenderContext context, CullingResults cullingResults) {
			this.cullingResults = cullingResults;
			buffer.BeginSample(bufferName);
			SetupLights();
			buffer.EndSample(bufferName);
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		private void SetupLights() {
			NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

			int dirLightCount = 0;
			for (int i = 0; i < visibleLights.Length; i++) {
				VisibleLight visibleLight = visibleLights[i];
				if (visibleLight.lightType == LightType.Directional) {
					SetupDirectionalLight(dirLightCount++, ref visibleLight);
					if (dirLightCount >= maxDirLightCount) {
						break;
					}
				}
			}

			buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
			buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
			buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
		}

		private void SetupDirectionalLight(int index, ref VisibleLight visibleLight) {
			dirLightColors[index] = visibleLight.finalColor;
			dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
		}
	}
}
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
	public class CustomRenderPipelineAsset : RenderPipelineAsset
	{
		[SerializeField]
		private bool
			useDynamicBatching = true,
			useGPUInstancing = true,
			useSRPBatcher = true,
			useLightsPerObject = true;

		[SerializeField] private ShadowSettings shadows = default;
		[SerializeField] PostFXSettings postFXSettings = default;

		protected override RenderPipeline CreatePipeline() {
			return new CustomRenderPipeline(
				useDynamicBatching, useGPUInstancing, useSRPBatcher,
				useLightsPerObject, shadows, postFXSettings
			);
		}
	} 
}
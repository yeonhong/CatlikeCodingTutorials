using UnityEditor;
using UnityEngine;

namespace CustomRP
{
	[CustomEditorForRenderPipeline(typeof(Light), typeof(CustomRenderPipelineAsset))]
	internal class CustomLightEditor : LightEditor
	{
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (!settings.lightType.hasMultipleDifferentValues &&
				(LightType)settings.lightType.enumValueIndex == LightType.Spot) {
				settings.DrawInnerAndOuterSpotAngle();
				settings.ApplyModifiedProperties();
			}
		}
	}
}
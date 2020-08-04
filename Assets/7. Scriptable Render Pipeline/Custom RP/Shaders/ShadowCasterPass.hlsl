#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex(Attributes input) {
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	output.positionCS = TransformWorldToHClip(positionWS);
	output.baseUV = TransformBaseUV(input.baseUV);
	return output;
}

void ShadowCasterPassFragment(Varyings input) {
	UNITY_SETUP_INSTANCE_ID(input);
	float4 base = GetBase(input.baseUV);

#if defined(_CLIPPING)
	clip(base.a - GetCutoff(input.baseUV));
#endif

#if defined(_SHADOWS_CLIP)
	clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
#endif
}

#endif
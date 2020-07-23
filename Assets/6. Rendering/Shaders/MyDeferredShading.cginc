#if !defined(MY_DEFERRED_SHADING)
#define MY_DEFERRED_SHADING

#include "UnityCG.cginc"

struct VertexData {
	float4 vertex : POSITION;
};

struct Interpolators {
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
};

Interpolators VertexProgram(VertexData v) {
	Interpolators i;
	i.pos = UnityObjectToClipPos(v.vertex);
	i.uv = ComputeScreenPos(i.pos);
	return i;
}

float4 FragmentProgram(Interpolators i) : SV_Target {
	float2 uv = i.uv.xy / i.uv.w;
	return 0;
}

#endif
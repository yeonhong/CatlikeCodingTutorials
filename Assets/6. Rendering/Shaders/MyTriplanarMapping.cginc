#if !defined(MY_TRIPLANAR_MAPPING_INCLUDED)
#define MY_TRIPLANAR_MAPPING_INCLUDED

#define NO_DEFAULT_UV

#include "MySurface.cginc"
#include "My Lighting Input.cginc"

struct TriplanarUV {
	float2 x, y, z;
};

TriplanarUV GetTriplanarUV(SurfaceParameters parameters) {
	TriplanarUV triUV;
	float3 p = parameters.position;
	triUV.x = p.zy;
	triUV.y = p.xz;
	triUV.z = p.xy;
	if (parameters.normal.x < 0) {
		triUV.x.x = -triUV.x.x;
	}
	if (parameters.normal.y < 0) {
		triUV.y.x = -triUV.y.x;
	}
	if (parameters.normal.z >= 0) {
		triUV.z.x = -triUV.z.x;
	}
	triUV.x.y += 0.5;
	triUV.z.x += 0.5;
	return triUV;
}

float3 GetTriplanarWeights(SurfaceParameters parameters) {
	float3 triW = abs(parameters.normal);
	return triW / (triW.x + triW.y + triW.z);
}

void MyTriPlanarSurfaceFunction (
	inout SurfaceData surface, SurfaceParameters parameters
) {
	TriplanarUV triUV = GetTriplanarUV(parameters);

	float3 albedoX = tex2D(_MainTex, triUV.x).rgb;
	float3 albedoY = tex2D(_MainTex, triUV.y).rgb;
	float3 albedoZ = tex2D(_MainTex, triUV.z).rgb;

	float3 triW = GetTriplanarWeights(parameters);

	surface.albedo = albedoX * triW.x + albedoY * triW.y + albedoZ * triW.z;
}

#define SURFACE_FUNCTION MyTriPlanarSurfaceFunction

#endif
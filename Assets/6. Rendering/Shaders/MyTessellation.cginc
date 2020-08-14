#if !defined(TESSELLATION_INCLUDED)
#define TESSELLATION_INCLUDED

struct TessellationFactors {
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
#if TESSELLATION_TANGENT
	float4 tangent : TANGENT;
#endif
	float2 uv : TEXCOORD0;
#if TESSELLATION_UV1
	float2 uv1 : TEXCOORD1;
#endif
#if TESSELLATION_UV2
	float2 uv2 : TEXCOORD2;
#endif
};

float _TessellationUniform;
float _TessellationEdgeLength;

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("MyPatchConstantFunction")]
TessellationControlPoint MyHullProgram(
	InputPatch<TessellationControlPoint, 3> patch,
	uint id : SV_OutputControlPointID
) {
	return patch[id];
}

[UNITY_domain("tri")]
InterpolatorsVertex MyDomainProgram(
	TessellationFactors factors,
	OutputPatch<TessellationControlPoint, 3> patch,
	float3 barycentricCoordinates : SV_DomainLocation
) {
	VertexData data;

#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)

#if TESSELLATION_TANGENT
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
#endif

	MY_DOMAIN_PROGRAM_INTERPOLATE(uv)

#if TESSELLATION_UV1
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv1)
#endif

#if TESSELLATION_UV2
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv2)
#endif

	return MyVertexProgram(data);
}

TessellationControlPoint MyTessellationVertexProgram(VertexData v) {
	TessellationControlPoint p;
	p.vertex = v.vertex;
	p.normal = v.normal;
#if TESSELLATION_TANGENT
	p.tangent = v.tangent;
#endif
	p.uv = v.uv;
#if TESSELLATION_UV1
	p.uv1 = v.uv1;
#endif
#if TESSELLATION_UV2
	p.uv2 = v.uv2;
#endif
	return p;
}

float TessellationEdgeFactor(float3 p0, float3 p1) {
#if defined(_TESSELLATION_EDGE)
	float edgeLength = distance(p0, p1);
	float3 edgeCenter = (p0 + p1) * 0.5;
	float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);
	return edgeLength * _ScreenParams.y /
		(_TessellationEdgeLength * viewDistance);
#else
	return _TessellationUniform;
#endif
}

TessellationFactors MyPatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch) {
	float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;
	TessellationFactors f;
	f.edge[0] = TessellationEdgeFactor(p1, p2);
	f.edge[1] = TessellationEdgeFactor(p2, p0);
	f.edge[2] = TessellationEdgeFactor(p0, p1);
	f.inside =
		(TessellationEdgeFactor(p1, p2) +
			TessellationEdgeFactor(p2, p0) +
			TessellationEdgeFactor(p0, p1)) * (1 / 3.0);
	return f;
}

#endif
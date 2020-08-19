﻿#if !defined(MY_TRIPLANAR_MAPPING_INCLUDED)
#define MY_TRIPLANAR_MAPPING_INCLUDED

#define NO_DEFAULT_UV

#include "MySurface.cginc"
#include "My Lighting Input.cginc"

void MyTriPlanarSurfaceFunction (
	inout SurfaceData surface, SurfaceParameters parameters
) {
	surface.albedo = tex2D(_MainTex, parameters.position.xy);
}

#define SURFACE_FUNCTION MyTriPlanarSurfaceFunction

#endif
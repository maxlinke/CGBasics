#ifndef UNITY_CG_INCLUDED
#include "UnityCG.cginc"
#endif

// ----------------------------------------------------------------
// properties
// ----------------------------------------------------------------

fixed4 _FrontColor;
fixed4 _BackColor;
float4 _LightDir;
fixed4 _LightColorFront;
fixed4 _LightColorBack;
fixed4 _LightColorAmbient;
fixed4 _ClippingOverlayColor;
float _SpecularIntensity;
float _SpecularHardness;

float4 _SpecialCamPos;
float4x4 _SpecialModelMatrix;
float4x4 _SpecialClippingMatrix;
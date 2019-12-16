#ifndef UNITY_CG_INCLUDED
#include "UnityCG.cginc"
#endif

#ifndef LIGHTING_INCLUDED
#include "Lighting.cginc"
#endif

// structs

struct lm_input {
    half3 normal;
    half3 lightDir;
    half3 viewDir;
};

struct lm_appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR;
};

struct lm_v2f {
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float3 worldNormal : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float3 lightDir : TEXCOORD2;
    float3 viewDir : TEXCOORD3;
};

lm_input GetLMInput (lm_v2f i) {
    lm_input o;
    o.normal = normalize(i.worldNormal);
    o.lightDir = normalize(i.lightDir);
    o.viewDir = normalize(i.viewDir);
    return o;
}

// the universal vertex shader

lm_v2f lm_vert (lm_appdata v) {
    lm_v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.lightDir = WorldSpaceLightDir(v.vertex);
    o.viewDir = _WorldSpaceCameraPos - o.worldPos.xyz;
    return o;
}

// universal properties

fixed4 _Color;

// lighting models

half3 Diffuse_Ambient (lm_input input) {
    return ShadeSH9(half4(input.normal, 1.0));
}

half Diffuse_Lambert (lm_input input) {
    return saturate(dot(input.normal, input.lightDir));
}

// fragment shaders using the lighting models

fixed4 lm_frag_lambert (lm_v2f i) : SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed4 diff = _LightColor0 * Diffuse_Lambert(li);
    diff.rgb += Diffuse_Ambient(li);
    return diff * col;
}
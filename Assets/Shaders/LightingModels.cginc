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
float _Roughness;
float _MinnaertExp;

// lighting models

half3 Diffuse_Ambient (lm_input input) {
    return ShadeSH9(half4(input.normal, 1.0));
}

half Diffuse_Lambert (lm_input input) {
    return saturate(dot(input.normal, input.lightDir));
}

// half3 Diffuse_Oren_Nayar (lm_input input) {
//     half nDotL = dot(input.normal, input.lightDir);
//     half nDotV = dot(input.normal, input.viewDir);
//     half lDotV = dot(input.lightDir, input.viewDir);

//     half roughSQ = _Roughness * _Roughness;
//     half3 orenNayarFraction = roughSQ / (roughSQ + half3(0.33, 0.13, 0.09));
//     half3 orenNayar = half3(1,0,0) + half3(-0.5, 0.17, 0.45) * orenNayarFraction;
//     half orenNayarS = lDotV - nDotL * nDotV;
//     orenNayarS /= lerp(max(nDotL, nDotV), 1, step(orenNayarS, 0));

//     half3 finalFactor = orenNayar.x;
//     finalFactor += _Color * orenNayar.y;
//     finalFactor += orenNayar.z * orenNayarS;	

//     return saturate(nDotL) * finalFactor * _LightColor0.rgb;
// }

// real oren nayer
// modified from this:
// https://books.google.de/books?id=pCwwxlMuNycC&pg=PA99&lpg=PA99&dq=minnaert+lighting&source=bl&ots=vWsa-7SVAU&sig=ACfU3U3Osz9Lc1lsctr8addHH1uLDXcuEw&hl=en&sa=X&ved=2ahUKEwjjx6z7srvmAhVFUMAKHeu1CmQQ6AEwDXoECAwQAQ#v=onepage&q=minnaert%20lighting&f=false
// and this:
// https://www.gamasutra.com/view/feature/131269/implementing_modular_hlsl_with_.php?page=3
half Diffuse_Oren_Nayar (lm_input input) {
    half nDotL = dot(input.normal, input.lightDir);
    half nDotV = dot(input.normal, input.viewDir);
    
    half sigmaSQ = _Roughness * _Roughness;
    half a = 1 - (0.5 * (sigmaSQ / (sigmaSQ + 0.33)));
    half b = 0.45 * (sigmaSQ / (sigmaSQ + 0.09));

    half thetaI = acos(nDotL);
    half thetaR = acos(nDotV);
    half alpha = max(thetaI, thetaR);
    half beta = min(thetaI, thetaR);

    half3 phiR = normalize(input.viewDir - input.normal * nDotV);
    half3 phiI = normalize(input.lightDir - input.normal * nDotL);
    half cosC = dot(phiR, phiI);  // dot of vectors in place of cos(phiR - phiR)

    // half orenNayar = saturate(nDotL) * saturate((a + (b * sin(alpha) * tan(beta) * max(0, cosC))));  // since cos(x) is only in [-1, 1] saturate(y) does the same job as max(0, y)
    half orenNayar = saturate(nDotL) * saturate((a + (b * sin(alpha) * tan(beta) * saturate(cosC))));
    return orenNayar;
}

half Diffuse_Minnaert (lm_input input) {
    half nDotL = dot(input.normal, input.lightDir);
    half nDotV = dot(input.normal, input.viewDir);

    half minnaert = pow(saturate(nDotL * nDotV), _MinnaertExp);
    return minnaert;
}

// fragment shaders using the lighting models

fixed4 lm_frag_lambert (lm_v2f i) : SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed3 diff = _LightColor0.rgb * Diffuse_Lambert(li) + Diffuse_Ambient(li);
    col.rgb *= diff;
    return col;
}

fixed4 lm_frag_oren_nayar (lm_v2f i) : SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed3 diff = _LightColor0.rgb * Diffuse_Oren_Nayar(li) + Diffuse_Ambient(li);
    col.rgb *= diff;
    return col;
}

fixed4 lm_frag_minnaert (lm_v2f i) : SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed3 diff = _LightColor0.rgb * Diffuse_Minnaert(li) + Diffuse_Ambient(li);
    col.rgb *= diff;
    return col;
}
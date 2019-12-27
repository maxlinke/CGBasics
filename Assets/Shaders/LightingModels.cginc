#ifndef UNITY_CG_INCLUDED
#include "UnityCG.cginc"
#endif

#ifndef LIGHTING_INCLUDED
#include "Lighting.cginc"
#endif

// ----------------------------------------------------------------
// structs
// ----------------------------------------------------------------

struct lm_input {
    half3 normal;
    half3 lightDir;
    half3 viewDir;
    half3 halfVec;
    half nDotL;
    half nDotV;
    half nDotH;
    half3 tangent;
    half3 bitangent;
};

struct lm_appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR;
    float4 tangent : TANGENT;
};

struct lm_v2f {
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float3 worldNormal : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float3 lightDir : TEXCOORD2;
    float3 viewDir : TEXCOORD3;
    float3 worldTangent : TEXCOORD4;
};

// ----------------------------------------------------------------
// the universal vertex shader
// ----------------------------------------------------------------

lm_v2f lm_vert (lm_appdata v) {
    lm_v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.lightDir = WorldSpaceLightDir(v.vertex);
    o.viewDir = _WorldSpaceCameraPos - o.worldPos.xyz;
    o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
    return o;
}

// ----------------------------------------------------------------
// universal properties
// ----------------------------------------------------------------

fixed4 _Color;
float _Roughness;
float _MinnaertExp;

fixed4 _SpecularColor;
float _SpecularIntensity;
float _SpecularHardness;
float _SpecularHardnessX;
float _SpecularHardnessY;

// ----------------------------------------------------------------
// helper functions
// ----------------------------------------------------------------

lm_input GetLMInput (lm_v2f i) {
    lm_input o;
    o.normal = normalize(i.worldNormal);
    o.lightDir = normalize(i.lightDir);
    o.viewDir = normalize(i.viewDir);
    o.halfVec = normalize(o.lightDir + o.viewDir);
    o.nDotL = dot(o.normal, o.lightDir);
    o.nDotV = dot(o.normal, o.viewDir);
    o.nDotH = dot(o.normal, o.halfVec);
    o.tangent = normalize(i.worldTangent);
    o.bitangent = normalize(cross(o.normal, o.tangent));
    return o;
}

half SlopeRMSFromHardness (half hardness) {
    return sqrt(2.0 / (hardness + 2));   // from https://simonstechblog.blogspot.com/2011/12/microfacet-brdf.html
}

// https://en.wikipedia.org/wiki/Specular_highlight#Beckmann_distribution
half Beckmann_Distribution (lm_input input) {
    half alpha = acos(input.nDotH);

    half m = SlopeRMSFromHardness(_SpecularHardness);
    half m2 = m * m;
    half tanAlpha = tan(alpha);
    half tanAlpha2 = tanAlpha * tanAlpha;
    half cosAlpha = cos(alpha);
    half cosAlpha4 = cosAlpha * cosAlpha * cosAlpha * cosAlpha;

    return exp(-tanAlpha2 / m2) / (UNITY_PI * m2 * cosAlpha4);
}

// https://en.wikipedia.org/wiki/Schlick%27s_approximation
// half Schlicks_Fresnel_Approximation_IOR (lm_input input) {
//     float n1 = 1;   // assuming air as refractive medium
//     float n2 = _SpecularIndexOfRefraction;

//     float sqrtR0 = (n1 - n2) / (n1 + n2);
//     float r0 = sqrtR0 * sqrtR0;

//     float cosTheta = input.nDotL;
//     return r0 + (1.0 - r0) * pow(1 - cosTheta, 5);
// }

// https://en.wikibooks.org/wiki/GLSL_Programming/Unity/Specular_Highlights_at_Silhouettes
half Schlicks_Fresnel_Approximation_Intensity (lm_input input) {
    float fLambda = _SpecularIntensity;
    float hDotV = dot(input.halfVec, input.viewDir);
    return fLambda + ((1.0 - fLambda) * pow(1.0 - hDotV, 5));
}

// https://en.wikipedia.org/wiki/Specular_highlight#Cook%E2%80%93Torrance_model
half Geometric_Attenuation (lm_input input) {
    half vDotH = dot(input.viewDir, input.halfVec);
    half a = (2 * input.nDotH * input.nDotV) / vDotH;
    half b = (2 * input.nDotH * input.nDotL) / vDotH;
    return min(1, min(a, b));
}

// ----------------------------------------------------------------
// lighting models
// ----------------------------------------------------------------

half3 Diffuse_Ambient (lm_input input) {
    return ShadeSH9(half4(input.normal, 1.0));
}

half Diffuse_Lambert (lm_input input) {
    return saturate(input.nDotL);
}

half Diffuse_Wrap (lm_input input) {
    return saturate((input.nDotL + 1.0) / 2.0);
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
    half nDotL = input.nDotL;
    half nDotV = input.nDotV;
    
    half sigmaSQ = _Roughness * _Roughness;
    half a = 1 - (0.5 * (sigmaSQ / (sigmaSQ + 0.33)));
    half b = 0.45 * (sigmaSQ / (sigmaSQ + 0.09));

    half thetaI = acos(nDotL);
    half thetaR = acos(nDotV);
    half alpha = max(thetaI, thetaR);
    half beta = min(thetaI, thetaR);

    half3 vProj = input.viewDir - (input.normal * input.nDotV);
    half3 lProj = input.lightDir - (input.normal * input.nDotL);
    half cosC = dot(vProj, lProj) / (length(vProj) * length(lProj));    // cosC is difference in azimuth angles (phiI - phiR). angle between projected vectors is the same.

    half orenNayar = saturate(nDotL) * (a + (b * sin(alpha) * tan(beta) * max(0, cosC)));   // saturating the second term makes the edges darker. it's also not in the original formula (neither is saturating n*l though...)
    return orenNayar;
}

half Diffuse_Minnaert (lm_input input) {
    return pow(saturate(input.nDotL * input.nDotV), _MinnaertExp);
}

half Specular_Phong (lm_input input) {
    // half3 r = input.lightDir - 2 * input.normal * dot(input.normal, input.lightDir);
    half3 r = reflect(input.lightDir, input.normal);     // the easy way
    half3 e = -input.viewDir;
    return saturate(_SpecularIntensity * pow(saturate(dot(r, e)), _SpecularHardness));  //  * step(0, input.nDotV) ? 
}

half Specular_Blinn_Phong (lm_input input) {
    return saturate(_SpecularIntensity * pow(saturate(input.nDotH), _SpecularHardness));
    // return saturate(_SpecularIntensity * pow(input.nDotH, _SpecularHardness));       // doesn't change anything...
}

// https://en.wikipedia.org/wiki/Specular_highlight#Cook%E2%80%93Torrance_model
half Specular_Cook_Torrance (lm_input input) {
    half d = Beckmann_Distribution(input);
    // half f = Schlicks_Fresnel_Approximation_IOR(input);
    half f = Schlicks_Fresnel_Approximation_Intensity(input);
    half g = Geometric_Attenuation(input);
    
    // return saturate((d * f * g) / (UNITY_PI * input.nDotV * input.nDotL));  //  * step(0, input.nDotV) ? 
    return (d * f * g) / (UNITY_PI * input.nDotV * input.nDotL);  //  * step(0, input.nDotV) ? 
}

half Ward (lm_input input, half roughness, half exponent) {
    half root = sqrt(input.nDotL * input.nDotV);
    return saturate(_SpecularIntensity * input.nDotL * exp(exponent) / (root * 4.0 * UNITY_PI * roughness * roughness));  // ndotl isn't in the original paper but it's necessary
}

half Specular_Ward_Iso (lm_input input) {
    half alpha = SlopeRMSFromHardness(_SpecularHardness);
    
    half delta = acos(input.nDotH);     // delta is the ANGLE!!! and the dot product is the cosine of the angle...
    half tanD = tan(delta);
    half exponent = -(tanD * tanD) / (alpha * alpha);

    // half delta = input.nDotH;
    // half tanD = tan(delta / (alpha * alpha));    // very trippy but wrong
    // half exponent = -(tanD * tanD);

    return Ward(input, alpha, exponent);
}

half Specular_Ward_Aniso (lm_input input) {
    half roughX = SlopeRMSFromHardness(_SpecularHardnessX);
    half roughY = SlopeRMSFromHardness(_SpecularHardnessY);
    half expA = dot(input.halfVec, input.tangent) / roughX;
    half expB = dot(input.halfVec, input.bitangent) / roughY;
    half expAB = -2.0 * (expA * expA + expB * expB) / (1.0 + input.nDotH);
    return Ward (input, sqrt(roughX * roughY), expAB);
}

// ----------------------------------------------------------------
// fragment shaders using the lighting models
// ----------------------------------------------------------------

fixed4 lm_frag_lambert (lm_v2f i) : SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed3 diff = _LightColor0.rgb * Diffuse_Lambert(li) + Diffuse_Ambient(li);
    col.rgb *= diff;
    return col;
}

fixed4 lm_frag_wrap (lm_v2f i) :SV_TARGET {
    fixed4 col = _Color * i.color;
    lm_input li = GetLMInput(i);
    fixed3 diff = _LightColor0.rgb * Diffuse_Wrap(li) + Diffuse_Ambient(li);
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

fixed4 lm_frag_phong (lm_v2f i) : SV_TARGET {
    fixed4 col = _SpecularColor;
    lm_input li = GetLMInput(i);
    fixed3 spec = _LightColor0.rgb * Specular_Phong(li);
    col.rgb *= spec;
    return col;
}

fixed4 lm_frag_blinn_phong (lm_v2f i) : SV_TARGET {
    fixed4 col = _SpecularColor;
    lm_input li = GetLMInput(i);
    fixed3 spec = _LightColor0.rgb * Specular_Blinn_Phong(li);
    col.rgb *= spec;
    return col;
}

fixed4 lm_frag_cook_torrance (lm_v2f i) : SV_TARGET {
    fixed4 col = _SpecularColor;
    lm_input li = GetLMInput(i);
    fixed3 spec = _LightColor0.rgb * Specular_Cook_Torrance(li);
    col.rgb *= spec;
    return col;
}

fixed4 lm_frag_ward_iso (lm_v2f i) : SV_TARGET {
    fixed4 col = _SpecularColor;
    lm_input li = GetLMInput(i);
    fixed3 spec = _LightColor0.rgb * Specular_Ward_Iso(li);
    col.rgb *= spec;
    return col;
}

fixed4 lm_frag_ward_aniso (lm_v2f i) : SV_TARGET {
    fixed4 col = _SpecularColor;
    lm_input li = GetLMInput(i);
    fixed3 spec = _LightColor0.rgb * Specular_Ward_Aniso(li);
    col.rgb *= spec;
    return col;
}
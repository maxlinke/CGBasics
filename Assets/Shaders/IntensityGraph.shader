Shader "Custom/Blit/IntensityGraph" {

// if-else is okay in here because these if-statements are going to be the same for all clusters of pixels
// so they won't slow down rendering like they would in a regular shader, where the evaulation might 
// be different for neighboring pixels...

    Properties {
        _GraphScale ("Graph Scale", Float) = 1.0
        _LightDir ("Light Dir", Vector) = (1.0, 1.0, 0.0, 0.0)
        _ViewDir ("View Dir", Vector) = (-1.0, 1.0, 0.0, 0.0)
        _ForegroundColor ("Foreground Color", Color) = (0.8, 0.8, 0.8, 1.0)
        _BackgroundColor ("Background Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _MajorLineWidth ("Major Line Width", Float) = 0.1
        _MajorLineOpacity ("Major Line Opacity", Float) = 0.667
        _MinorLineWidth ("Minor Line Width", Float) = 0.1
        _MinorLineOpacity ("Minor Line Opacity", Float) = 0.333
        // the conditions
        _SpecBlinnPhong ("_SpecBlinnPhong", Range(0, 1)) = 0
        _SpecCookTorr ("_SpecCookTorr", Range(0, 1)) = 0
        _DiffWrap ("_DiffWrap", Range(0, 1)) = 0
        _DiffLambert ("_DiffLambert", Range(0, 1)) = 0
        _DiffMinnaert ("_DiffMinnaert", Range(0, 1)) = 0
        _DiffOrenNayar ("_DiffOrenNayar", Range(0, 1)) = 0
        _SpecPhong ("_SpecPhong", Range(0, 1)) = 0
        _SpecWardAniso ("_SpecWardAniso", Range(0, 1)) = 0
        _SpecWardIso ("_SpecWardIso", Range(0, 1)) = 0
        // planar mode
        _PlanarMode ("Planar Mode", Range(0, 1)) = 1
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "LightingModels.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 pixelUV : TEXCOORD1;
            };

            float _GraphScale;
            float4 _LightDir;
            float4 _ViewDir;
            fixed4 _ForegroundColor;
            fixed4 _BackgroundColor;
            float _MajorLineWidth;
            float _MajorLineOpacity;
            float _MinorLineWidth;
            float _MinorLineOpacity;

            float _SpecBlinnPhong;
            float _SpecCookTorr;
            float _DiffWrap;
            float _DiffLambert;
            float _DiffMinnaert;
            float _DiffOrenNayar;
            float _SpecPhong;
            float _SpecWardAniso;
            float _SpecWardIso;

            float _PlanarMode;

            fixed4 _LightCol;
            fixed4 _AmbientCol;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv *= 2.0;
                o.uv -= 1.0;
                if(_ScreenParams.y < _ScreenParams.x){
                    o.uv *= float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                }else{
                    o.uv *= float2(1.0, _ScreenParams.y / _ScreenParams.x);
                }
                o.uv *= _GraphScale;
                o.pixelUV = v.uv * _ScreenParams.xy;
                return o;
            }

            float normalizeWithLineWidth (float inputValue, float lineWidth) {
                float normMin = -lineWidth / 2;
                float normMax = +lineWidth / 2;
                return (inputValue - normMin) / (normMax - normMin);
            }

            float triCos (float x) {
                return 2.0 * (saturate(2.0 * (frac(x) - 0.5)) + saturate(2.0 * (0.5 - frac(x)))) - 1;
            }

            float concentricLineMultiplier (float pos, float lineWidth, float lineOpacity) {
                float lineCos = (triCos(pos) + 1.0) / 2.0;
                float lines = smoothstep(-0.002, 0.002, lineCos - lineWidth);
                float lineTransparency = 1 - lineOpacity;
                return lineTransparency + (lines * (1 - lineTransparency));
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 pixelVec = float3(normalize(i.uv), 0);
                float3 lightDir = normalize(float3(_LightDir.x, _LightDir.y, 0));
                float3 normal;
                float3 viewDir;
                if(_PlanarMode > 0.5){
                    normal = float3(0,1,0);
                    viewDir = pixelVec;
                }else{
                    normal = pixelVec;
                    viewDir = normalize(float3(_ViewDir.x, _ViewDir.y, 0));
                }
                float3 tangent = float3(0,0,1);
                float distToCenter = length(i.uv);
                
                // synthetic v2f
                lm_v2f t;
                t.vertex = float4(0,0,0,0);
                t.color = float4(0,0,0,0);
                t.worldNormal = normal;
                t.worldPos = float3(0,0,0);
                t.lightDir = lightDir;
                t.viewDir = viewDir;
                t.worldTangent = tangent;
                // the spicy part
                lm_input li = GetLMInput(t);
                float3 evalCol = float3(0, 0, 0);
                if(_SpecBlinnPhong > 0.5){
                    evalCol += _LightCol * Specular_Blinn_Phong(li) * _SpecularColor;
                }
                if(_SpecCookTorr > 0.5){
                    evalCol += _LightCol * Specular_Cook_Torrance(li) * _SpecularColor;
                }
                if(_DiffWrap > 0.5){
                    evalCol += (_LightCol * Diffuse_Wrap(li) + _AmbientCol) * _Color;
                }
                if(_DiffLambert > 0.5){
                    evalCol += (_LightCol * Diffuse_Lambert(li) + _AmbientCol) * _Color;
                }
                if(_DiffMinnaert > 0.5){
                    evalCol += (_LightCol * Diffuse_Minnaert(li) + _AmbientCol) * _Color;
                }
                if(_DiffOrenNayar > 0.5){
                    evalCol += (_LightCol * Diffuse_Oren_Nayar(li) + _AmbientCol) * _Color;
                }
                if(_SpecPhong > 0.5){
                    evalCol += _LightCol * Specular_Phong(li) * _SpecularColor;
                }
                if(_SpecWardAniso > 0.5){
                    evalCol += _LightCol * Specular_Ward_Aniso(li) * _SpecularColor;
                }
                if(_SpecWardIso > 0.5){
                    evalCol += _LightCol * Specular_Ward_Iso(li) * _SpecularColor;
                }

                float evalLum = 0.299 * evalCol.x + 0.587 * evalCol.y + 0.114 * evalCol.z;
                float colorLookup;
                if((evalLum > 1) && (distToCenter < evalLum) && (distToCenter > 1)){
                    // float2 pixelFrac = frac(i.pixelUV / 2) * 2;
                    // float pixelID = abs(pixelFrac.x - pixelFrac.y);
                    // colorLookup = pixelID;
                    colorLookup = 0.5;
                }else{
                    colorLookup = step(0, (distToCenter - evalLum));
                }
                
                // debug
                // float2 vPos = viewDir.xy * 1.3f;
                // float distToVPos = length(vPos - i.uv);
                // float s = step(0.02, distToVPos);
                // colorLookup *= s;
                // float2 lPos = lightDir.xy * 1.4f;
                // float distToLPos = length(lPos - i.uv);
                // s = step(0.02, distToLPos);
                // colorLookup *= s;

                colorLookup *= concentricLineMultiplier(distToCenter + 0.5, _MajorLineWidth, _MajorLineOpacity);
                float lineSubdivCount = 10;
                for(float lCounter=1; lCounter<lineSubdivCount; lCounter+=1.0){
                    float minorLineOffset = lCounter / lineSubdivCount;
                    colorLookup *= concentricLineMultiplier(distToCenter + 0.5 + minorLineOffset, _MinorLineWidth, _MinorLineOpacity);
                }

                fixed4 col = lerp(_ForegroundColor, _BackgroundColor, saturate(colorLookup));
                return col;
            }
			
            ENDCG
        }
    }
}

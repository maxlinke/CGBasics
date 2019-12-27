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
        _LineWidth ("Line Width", Float) = 0.1
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
            float _LineWidth;

            float _SpecBlinnPhong;
            float _SpecCookTorr;
            float _DiffWrap;
            float _DiffLambert;
            float _DiffMinnaert;
            float _DiffOrenNayar;
            float _SpecPhong;
            float _SpecWardAniso;
            float _SpecWardIso;

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

            fixed4 frag (v2f i) : SV_Target {
                float3 pixelVec = float3(normalize(i.uv), 0);
                float3 normal = pixelVec;
                float3 tangent = float3(0,0,1);
                float distToCenter = length(i.uv);
                float3 lightDir = normalize(float3(_LightDir.x, _LightDir.y, 0));
                float3 viewDir = normalize(float3(_ViewDir.x, _ViewDir.y, 0));
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

                float lerpMin = -_LineWidth / 2;
                float lerpMax = +_LineWidth / 2;
                float lerpVal = ((distToCenter - evalLum) - lerpMin) / (lerpMax - lerpMin);

                float2 pixelFrac = frac(i.pixelUV / 2) * 2;
                float pixelID = abs(pixelFrac.x - pixelFrac.y);
                
                float colorLookup;
                if((evalLum > 1) && (distToCenter < evalLum) && (distToCenter > 1)){
                    colorLookup = pixelID;
                }else{
                    colorLookup = saturate(lerpVal);
                }
                

                // debug
                float2 vPos = viewDir.xy * 1.3f;
                float distToVPos = length(vPos - i.uv);
                float s = step(0.02, distToVPos);
                colorLookup *= s;
                float2 lPos = lightDir.xy * 1.4f;
                float distToLPos = length(lPos - i.uv);
                s = step(0.02, distToLPos);
                colorLookup *= s;

                fixed4 col = lerp(_ForegroundColor, _BackgroundColor, colorLookup);
                // col = frac(col);
                
                // fixed4 col = fixed4(i.uv.x, i.uv.y, 1, 1);
                // fixed4 col = fixed4(distToCenter, distToCenter, distToCenter, 1);
                // col = frac(col);
                return col;
            }
			
            ENDCG
        }
    }
}

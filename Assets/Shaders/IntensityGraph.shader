Shader "Custom/Blit/IntensityGraph" {

// if-else is okay in here because these if-statements are going to be the same for all clusters of pixels
// so they won't slow down rendering like they would in a regular shader, where the evaulation might 
// be different for neighboring pixels...

    Properties {
        // _GraphScale ("Graph Scale", Float) = 1.0
        // _LightDir ("Light Dir", Vector) = (1.0, 1.0, 0.0, 0.0)
        // _ViewDir ("View Dir", Vector) = (-1.0, 1.0, 0.0, 0.0)
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
            };

            float _GraphScale;
            float4 _LightDir;
            float4 _ViewDir;

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
                float eval = Diffuse_Oren_Nayar(li);
                fixed colVal = eval <= distToCenter ? 1 : 0;
                fixed4 col = fixed4(colVal, colVal, colVal, 1); 

                // fixed4 col = fixed4(i.uv.x, i.uv.y, 1, 1);
                // fixed4 col = fixed4(distToCenter, distToCenter, distToCenter, 1);
                // col = frac(col);
                return col;
            }
			
            ENDCG
        }
    }
}

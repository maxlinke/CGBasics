Shader "Custom/Debug/ColorsAsNormals" {

    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _LightDir ("Light Direction", Vector) = (1, 0.9, 0.8, 0)
        _FrontLightColor ("Front Light", Color) = (1, 1, 1, 1)
        _RearLightColor ("Rear Light", Color) = (0.333, 0.667, 1, 1)
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature USE_CUSTOM_MATRICES

            #include "UnityCG.cginc"

            fixed4 _Color;
            float4 _LightDir;
            fixed4 _FrontLightColor;
            fixed4 _RearLightColor;

            float4x4 _CustomModelMatrix;
            float4x4 _CustomInverseModelMatrix;

            struct appdata {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                float3 normal = v.color;
                normal *= 2;
                normal -= 1;
                #if USE_CUSTOM_MATRICES
                    o.worldNormal = normalize(mul(normal, (float3x3)_CustomInverseModelMatrix));
                #else
                    o.worldNormal = UnityObjectToWorldNormal(normal);
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = _Color;
                float3 l = normalize(_LightDir.xyz);
                float3 n = normalize(i.worldNormal);
                float nDotL = dot(n, l);
                col *= (saturate(nDotL) * _FrontLightColor) + (saturate(-nDotL) * _RearLightColor);
                // col.rgb = i.color;
                return col;
            }
			
            ENDCG
        }
    }
}

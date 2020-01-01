Shader "Custom/Debug/ColorsAsNormals" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _LightDir ("Light Direction", Vector) = (1,1,1,0)
        _FrontLightColor ("Front Light", Color) = (1,1,1,1)
        _RearLightColor ("Rear Light", Color) = (0,0,1,1)
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            float4 _LightDir;
            fixed4 _FrontLightColor;
            fixed4 _RearLightColor;

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
                o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = _Color;
                float nDotL = dot(_LightDir.xyz, i.worldNormal);
                col *= (saturate(nDotL) * _FrontLightColor) + (saturate(-nDotL) * _RearLightColor);
                return col;
            }
			
            ENDCG
        }
    }
}

Shader "Custom/FlatShading2" {

    Properties {
        _FrontColor ("Front Face Color", Color) = (1, 1, 1, 1)
        _BackColor ("Back Face Color", Color) = (1, 0, 1, 1)
        _LightDir ("Light Direction", Vector) = (1, 1, 1, 0)
        _LightColorFront ("Light Color Front", Color) = (0.8, 0.9, 0.9, 1)
        _LightColorBack ("Light Color Back", Color) = (0.2, 0.2, 0.5, 1)
        _LightColorAmbient ("Light Color Ambient", Color) = (0.2, 0.1, 0.1, 1)
        [Toggle(LIT_BACKFACES)] _LitBackfaces("Lit Backfaces", Int) = 0
        [Toggle(SOLID_BACKFACES)] _SolidBackfaces("Solid Backfaces", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Int) = 0
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull [_Cull]

        Pass {
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma shader_feature LIT_BACKFACES
            #pragma shader_feature SOLID_BACKFACES

            #include "UnityCG.cginc"

            fixed4 _FrontColor;
            fixed4 _BackColor;
            float4 _LightDir;
            fixed4 _LightColorFront;
            fixed4 _LightColorBack;
            fixed4 _LightColorAmbient;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f {
                v2f data;
                float3 normal : TEXCOORD2;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle v2f i[3], inout TriangleStream<g2f> stream) {
                g2f g0, g1, g2;

                g0.data = i[0];
                g1.data = i[1];
                g2.data = i[2];

                float3 p0 = i[0].worldPos;
	            float3 p1 = i[1].worldPos;
	            float3 p2 = i[2].worldPos;

                float3 faceNormal = normalize(cross(p1 - p0, p2 - p0));

                g0.normal = faceNormal;
                g1.normal = faceNormal;
                g2.normal = faceNormal;

                stream.Append(g0);
                stream.Append(g1);
                stream.Append(g2);
            }

            fixed4 frag (g2f i, fixed face : VFACE) : SV_Target {
                fixed faceStep = step(0, face);
                fixed4 col = face * _FrontColor + (1 - face) * _BackColor;
                float nDotL = dot(i.normal, normalize(_LightDir));
                fixed4 diffFront = saturate(nDotL) * _LightColorFront;
                fixed4 diffBack = saturate(-nDotL) * _LightColorBack;
                fixed4 diff = _LightColorAmbient + diffFront + diffBack;
                #ifndef LIT_BACKFACES
                    diff = lerp(fixed4(1,1,1,1), diff, faceStep);
                #endif
                #ifndef SOLID_BACKFACES
                    float2 pixelPos = i.data.vertex.xy;;
                    pixelPos = frac(pixelPos / 2) * 2;
                    fixed pixelID = abs(pixelPos.x - pixelPos.y);
                    clip(face + pixelID);
                #endif
                return col * diff;
            }
			
            ENDCG
        }
    }
}

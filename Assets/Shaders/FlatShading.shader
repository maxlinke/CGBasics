Shader "Custom/FlatShading" {

    Properties { }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f {
                v2f data;
                float3 normal : TEXCOORD1;
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

            fixed4 frag (g2f i) : SV_Target {
                fixed4 col = fixed4(1,1,1,1);
                col.rgb = i.normal;
                return col;
            }
			
            ENDCG
        }
    }
}

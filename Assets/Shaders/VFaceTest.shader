Shader "Custom/VFaceTest" {

	Properties {
		_FrontCol ("Front Color", Color) = (1,1,1,1)
        _BackCol ("Back Color", Color) = (1,0,1,1)
	}

	SubShader {

		Cull Off

		Tags { "RenderType"="Opaque" }

        Pass {
		
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _FrontCol;
            fixed4 _BackCol;

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i, fixed face : VFACE) : SV_TARGET {
                fixed4 col = fixed4(1,1,1,1);
                col *= i.color;
                col *= lerp(_BackCol, _FrontCol, step(0, face));
                return col;
            }

            ENDCG

        }
	}
}

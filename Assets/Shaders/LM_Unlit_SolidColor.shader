Shader "Custom/LightingModels/LM_Unlit_SolidColor" {

    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)]       _SrcBlend ("SrcBlend", Int) = 5.0 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)]       _DstBlend ("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
        [Enum(Off, 0, On, 1)]                         _ZWrite ("ZWrite", Int) = 1.0 // On
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4.0 // LEqual
        [Enum(UnityEngine.Rendering.CullMode)]        _Cull ("Cull", Int) = 0.0 // Off
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
		
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _Color * i.color;
            }
			
            ENDCG
        }
    }
}

Shader "Custom/LightingModels/LM_Unlit_Black" {

    Properties {
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

            float4 vert (float4 vertex : POSITION) : SV_POSITION {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag (float4 pos : SV_POSITION) : SV_Target {
                return fixed4(0,0,0,1);
            }
			
            ENDCG
        }
    }
}

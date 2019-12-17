Shader "Custom/LightingModels/LM_Diffuse_Wrap" {

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

            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
		
            CGPROGRAM
			
            #pragma vertex lm_vert
            #pragma fragment lm_frag_wrap
            #pragma multi_compile_fwdbase

            #include "LightingModels.cginc"
			
            ENDCG
        }

        Pass {

            Tags { "LightMode" = "ForwardAdd" }

            Blend One One
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
		
            CGPROGRAM
			
            #pragma vertex lm_vert
            #pragma fragment lm_frag_wrap
            #pragma multi_compile_fwdadd

            #include "LightingModels.cginc"
			
            ENDCG
        }
    }
}

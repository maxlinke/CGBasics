Shader "Custom/ClippingVolumeVis" {

    Properties {
        _OutsideTint ("Outside Tint", Color) = (0, 0, 0, 0.5)
    }
	
    SubShader {
	
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            Cull Back
            ZTest GEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _OutsideTint;
            float4 vert (float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
            fixed4 frag () : SV_Target { return _OutsideTint; }
            ENDCG
        }

        Pass {
            Cull Front
            ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _OutsideTint;
            float4 vert (float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
            fixed4 frag () : SV_Target { return _OutsideTint; }
            ENDCG
        }
    }
}

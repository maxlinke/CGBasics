// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// modified by Max Linke (2019)

// Simple "just colors" shader that's used for built-in debug visualizations,
// in the editor etc. Just outputs _Color * vertex color; and blend/Z/cull/bias
// controlled by material parameters.

Shader "Custom/InternalColoredWithCulling" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _SrcBlend ("SrcBlend", Int) = 5.0 // SrcAlpha
        _DstBlend ("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
        _ZWrite ("ZWrite", Int) = 1.0 // On
        _ZTest ("ZTest", Int) = 4.0 // LEqual
        _Cull ("Cull", Int) = 0.0 // Off
        _ZBias ("ZBias", Float) = 0.0
        _ClippingOverlayColor ("Clipping Overlay Color", Color) = (0, 0, 0, 0.8)
    }

    SubShader {

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass {

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
            Offset [_ZBias], [_ZBias]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #pragma shader_feature SHOW_CLIPPING
            #pragma shader_feature USE_SPECIAL_MODEL_MATRIX
            #pragma shader_feature USE_SPECIAL_CLIPPING_MATRIX

            #include "UnityCG.cginc"

            fixed4 _ClippingOverlayColor;
            float4x4 _SpecialModelMatrix;
            float4x4 _SpecialClippingMatrix;

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
                float4 clippingPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;

            v2f vert (appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                float4 worldPos;
                #ifdef USE_SPECIAL_MODEL_MATRIX
                    worldPos = mul(_SpecialModelMatrix, v.vertex);
                #else
                    worldPos = mul(unity_ObjectToWorld, v.vertex);
                #endif
                #ifdef USE_SPECIAL_CLIPPING_MATRIX
                    o.clippingPos = mul(_SpecialClippingMatrix, v.vertex);
                #else
                    o.clippingPos = worldPos;
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 output = i.color;
                #ifdef SHOW_CLIPPING
                    float4 cPos = i.clippingPos;
                    cPos /= cPos.w;
                    fixed inOrOut = saturate(            //0 = in, 1 = out
                        step(1, abs(cPos.x)) + 
                        step(1, abs(cPos.y)) + 
                        step(1, abs(cPos.z))
                    );
                    fixed oa = _ClippingOverlayColor.a;
                    fixed3 outputClipped = (1 - oa) * output.rgb + oa * _ClippingOverlayColor.rgb;
                    output.rgb = lerp(output.rgb, outputClipped, inOrOut);                    
                #endif
                return output;
            }

            ENDCG
        }
    }
}

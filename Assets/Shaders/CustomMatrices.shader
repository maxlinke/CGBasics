Shader "Custom/CustomMatrices" {
    
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EnvTex ("Environment Map", Cube) = "" {}
        _EnvMip ("Environment Mip Level", Range(0, 20)) = 10
        _EnvLevel ("Environment Map Intensity", Range(0, 2)) = 1
    }

    SubShader {

        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        ZTest LEqual
        ZWrite On
        Cull Back

        Pass {

            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            samplerCUBE _EnvTex;
            float _EnvMip;
            float _EnvLevel;

            fixed4 _LightColor0;

            float4x4 CustomMVPMatrix;
            float4x4 CustomModelMatrix;
            float4x4 CustomInverseModelMatrix;

            float3 CustomCameraWorldPos;

            struct appdata {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 lightDir : TEXCOORD3;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = mul(CustomMVPMatrix, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float4 f4worldPos = mul(CustomModelMatrix, v.vertex);
                // o.worldPos = f4worldPos / f4worldPos.w;
                o.worldPos = f4worldPos;
                o.worldNormal = normalize(mul(v.normal, CustomInverseModelMatrix).xyz);
                o.lightDir = WorldSpaceLightDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                fixed3 diff = saturate(dot(i.lightDir, i.worldNormal)) * _LightColor0.rgb;
                diff += _EnvLevel * texCUBElod(_EnvTex, float4(i.worldNormal, _EnvMip));
                col.rgb *= diff;
                // #ifdef DARKEN_CLIPPING
                    // float3 mult = abs(i.worldPos);
                    // mult = step(mult, 1);
                    // mult = 1 - mult;
                    // float singleMult = dot(mult, 1);
                    // singleMult = saturate(singleMult);
                    // singleMult *= 0.667;
                    // singleMult = 1 - singleMult;
                    // col.rgb *= singleMult;
                // #endif
                col = fixed4(1,1,1,1);
                col.rgb *= frac(i.worldPos.xyz / i.worldPos.w);
                return col;
            }

            ENDCG
        }
    }

    FallBack "VertexLit"
}

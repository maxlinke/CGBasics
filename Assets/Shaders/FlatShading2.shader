﻿Shader "Custom/FlatShading2" {

    Properties {
        _FrontColor ("Front Face Color", Color) = (1, 1, 1, 1)
        _BackColor ("Back Face Color", Color) = (1, 0, 1, 1)
        _LightDir ("Light Direction", Vector) = (1, 1, 1, 0)
        _LightColorFront ("Light Color Front", Color) = (0.8, 0.9, 0.9, 1)
        _LightColorBack ("Light Color Back", Color) = (0.2, 0.2, 0.5, 1)
        _LightColorAmbient ("Light Color Ambient", Color) = (0.2, 0.1, 0.1, 1)
        _ClippingOverlayColor ("Clipping Overlay Color", Color) = (0, 0, 0, 0.8)
        _SpecularIntensity ("Specular Intensity", Range(0, 1)) = 1
        _SpecularHardness ("Specular Hardness", Range(0, 1)) = 0.8
        [Toggle(LIT_BACKFACES)] _LitBackfaces("Lit Backfaces", Int) = 0
        [Toggle(SOLID_BACKFACES)] _SolidBackfaces("Solid Backfaces", Int) = 0
        // [Toggle(SHOW_CLIPPING)] _ShowClipping("Show Clipping", Int) = 0
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
            // gui-toggleable
            #pragma shader_feature LIT_BACKFACES
            #pragma shader_feature SOLID_BACKFACES
            // no gui-toggle
            #pragma shader_feature SHOW_CLIPPING
            #pragma shader_feature USE_SPECIAL_MODEL_MATRIX
            #pragma shader_feature USE_SPECIAL_CLIPPING_MATRIX

            #include "UnityCG.cginc"

            fixed4 _FrontColor;
            fixed4 _BackColor;
            float4 _LightDir;
            fixed4 _LightColorFront;
            fixed4 _LightColorBack;
            fixed4 _LightColorAmbient;
            fixed4 _ClippingOverlayColor;
            float _SpecularIntensity;
            float _SpecularHardness;

            float4 _SpecialCamPos;
            float4x4 _SpecialModelMatrix;
            float4x4 _SpecialClippingMatrix;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float4 clippingPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            struct g2f {
                v2f data;
                float3 normal : TEXCOORD3;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef USE_SPECIAL_MODEL_MATRIX
                    o.worldPos = mul(_SpecialModelMatrix, v.vertex);
                    o.viewDir = normalize((o.worldPos.xyz / o.worldPos.w) - _SpecialCamPos.xyz);
                #else
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize((o.worldPos.xyz / o.worldPos.w) - _WorldSpaceCameraPos.xyz);
                #endif
                #ifdef USE_SPECIAL_CLIPPING_MATRIX
                    o.clippingPos = mul(_SpecialClippingMatrix, v.vertex);
                #else
                    o.clippingPos = o.worldPos;
                #endif
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle v2f i[3], inout TriangleStream<g2f> stream) {
                g2f g0, g1, g2;

                g0.data = i[0];
                g1.data = i[1];
                g2.data = i[2];

                i[0].worldPos.xyz /= i[0].worldPos.w;
                i[1].worldPos.xyz /= i[1].worldPos.w;
                i[2].worldPos.xyz /= i[2].worldPos.w;

                float3 p0 = i[0].worldPos.xyz;
	            float3 p1 = i[1].worldPos.xyz;
	            float3 p2 = i[2].worldPos.xyz;

                float3 faceNormal = normalize(cross(p1 - p0, p2 - p0));

                g0.normal = faceNormal;
                g1.normal = faceNormal;
                g2.normal = faceNormal;

                stream.Append(g0);
                stream.Append(g1);
                stream.Append(g2);
            }

            fixed3 specularLight (float3 lightDir, float3 normal, float3 viewDir, fixed3 lightCol) {
                float3 halfVec = normalize(lightDir + viewDir);
                float3 raw = pow(saturate(dot(normal, halfVec)), _SpecularHardness * 128.0) * lightCol * _SpecularIntensity;
                return raw * step(0, dot(normal, lightDir));
            }

            fixed4 frag (g2f i, fixed face : VFACE) : SV_Target {
                fixed faceStep = step(0, face);
                fixed3 col = face * _FrontColor.rgb + (1 - face) * _BackColor.rgb;
                float3 normedLightDir = normalize(_LightDir.xyz);
                float3 normedViewDir = normalize(i.data.viewDir);
                float nDotL = dot(i.normal, normedLightDir);
                fixed3 diffFront = saturate(nDotL) * _LightColorFront.rgb;
                fixed3 diffBack = saturate(-nDotL) * _LightColorBack.rgb;
                fixed3 diff = _LightColorAmbient.rgb + diffFront + diffBack;
                fixed3 specFront = specularLight(normedLightDir, i.normal, -normedViewDir, _LightColorFront.rgb);
                fixed3 specBack = specularLight(-normedLightDir, i.normal, -normedViewDir, _LightColorBack.rgb);
                fixed3 spec = specFront + specBack;
                #ifndef LIT_BACKFACES
                    diff = lerp(fixed4(1,1,1,1), diff, faceStep);
                    spec = lerp(fixed4(0,0,0,0), spec, faceStep);
                #endif
                #ifndef SOLID_BACKFACES
                    float2 pixelPos = i.data.vertex.xy;;
                    pixelPos = frac(pixelPos / 2) * 2;
                    fixed pixelID = abs(pixelPos.x - pixelPos.y);
                    clip(face + pixelID);
                #endif
                fixed3 output = (col * diff) + spec;
                #ifdef SHOW_CLIPPING
                    float4 cPos = i.data.clippingPos;
                    cPos /= cPos.w;
                    fixed inOrOut = saturate(            //0 = in, 1 = out
                        step(1, abs(cPos.x)) + 
                        step(1, abs(cPos.y)) + 
                        step(1, abs(cPos.z))
                    );
                    fixed oa = _ClippingOverlayColor.a;
                    fixed3 outputClipped = (1 - oa) * output + oa * _ClippingOverlayColor.rgb;
                    output = lerp(output, outputClipped, inOrOut);                    
                #endif
                return fixed4(output, 1);
            }
			
            ENDCG
        }
    }
}

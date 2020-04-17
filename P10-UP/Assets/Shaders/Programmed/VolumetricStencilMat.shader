Shader "Stencils/VolumetricStencilMat"
{
    Properties
    {
        Vector1_38A4E771("Opacity", Float) = 0.2
        Color_EDAE10A("Color", Color) = (1, 1, 1, 0)
        Vector1_A53BC3D7("EdgeFalloff", Float) = 2
        [ToggleUI]Boolean_24749E14("UseTexture", Float) = 0
        [ToggleUI]Boolean_1D631463("HaveDistanceFalloff", Float) = 1
        [NoScaleOffset]Texture2D_A40A22E0("Texture2D", 2D) = "white" {}
        _StencilValue("Stencil Value", Float) = 0
    }
        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Transparent"
                "Queue" = "Geometry"
            }

            Stencil 
            {
               Ref [_StencilValue]
               Comp Equal
            }

            Pass
            {
                Name "Pass"
                Tags
                {
                // LightMode: <None>
            }

            // Render State
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Back
            ZTest LEqual
            ZWrite Off
            // ColorMask: <None>


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS 
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define SHADERPASS_UNLIT
            #define REQUIRE_DEPTH_TEXTURE

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_38A4E771;
            float4 Color_EDAE10A;
            float Vector1_A53BC3D7;
            float Boolean_24749E14;
            float Boolean_1D631463;
            float _StencilValue;
            CBUFFER_END
            TEXTURE2D(Texture2D_A40A22E0); SAMPLER(samplerTexture2D_A40A22E0); float4 Texture2D_A40A22E0_TexelSize;
            SAMPLER(_SampleTexture2D_37ABCB7F_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2D_3AF4D147_Sampler_3_Linear_Repeat);

            // Graph Functions

            void Unity_Branch_float(float Predicate, float True, float False, out float Out)
            {
                Out = lerp(False, True, Predicate);
            }

            void Unity_Distance_float3(float3 A, float3 B, out float Out)
            {
                Out = distance(A, B);
            }

            void Unity_Subtract_float(float A, float B, out float Out)
            {
                Out = A - B;
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
            {
                Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Lerp_float(float A, float B, float T, out float Out)
            {
                Out = lerp(A, B, T);
            }

            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }

            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }

            // Graph Vertex
            // GraphVertex: <None>

            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
                float3 WorldSpacePosition;
                float3 AbsoluteWorldSpacePosition;
                float4 ScreenPosition;
                float4 uv0;
                float3 TimeParameters;
            };

            struct SurfaceDescription
            {
                float3 Color;
                float Alpha;
                float AlphaClipThreshold;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _Property_F4AFB0A2_Out_0 = Color_EDAE10A;
                float _Property_6DFD3FCE_Out_0 = Boolean_24749E14;
                float _Property_38CEDF2A_Out_0 = Boolean_1D631463;
                float4 _UV_4FCCC73A_Out_0 = IN.uv0;
                float _Split_B5D12E44_R_1 = _UV_4FCCC73A_Out_0[0];
                float _Split_B5D12E44_G_2 = _UV_4FCCC73A_Out_0[1];
                float _Split_B5D12E44_B_3 = _UV_4FCCC73A_Out_0[2];
                float _Split_B5D12E44_A_4 = _UV_4FCCC73A_Out_0[3];
                float _Vector1_A5420D63_Out_0 = 1;
                float _Branch_AFB73BF_Out_3;
                Unity_Branch_float(_Property_38CEDF2A_Out_0, _Split_B5D12E44_G_2, _Vector1_A5420D63_Out_0, _Branch_AFB73BF_Out_3);
                float _Distance_47B3418F_Out_2;
                Unity_Distance_float3(_WorldSpaceCameraPos, IN.AbsoluteWorldSpacePosition, _Distance_47B3418F_Out_2);
                float _Subtract_3022506E_Out_2;
                Unity_Subtract_float(_Distance_47B3418F_Out_2, 0.5, _Subtract_3022506E_Out_2);
                float _Clamp_2EC55C05_Out_3;
                Unity_Clamp_float(_Subtract_3022506E_Out_2, 0, 1, _Clamp_2EC55C05_Out_3);
                float _SceneDepth_6EE328B0_Out_1;
                Unity_SceneDepth_Linear01_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_6EE328B0_Out_1);
                float _Multiply_F5E3A6D2_Out_2;
                Unity_Multiply_float(_SceneDepth_6EE328B0_Out_1, _ProjectionParams.z, _Multiply_F5E3A6D2_Out_2);
                float4 _ScreenPosition_C0A8EBA5_Out_0 = IN.ScreenPosition;
                float _Split_E7F1684F_R_1 = _ScreenPosition_C0A8EBA5_Out_0[0];
                float _Split_E7F1684F_G_2 = _ScreenPosition_C0A8EBA5_Out_0[1];
                float _Split_E7F1684F_B_3 = _ScreenPosition_C0A8EBA5_Out_0[2];
                float _Split_E7F1684F_A_4 = _ScreenPosition_C0A8EBA5_Out_0[3];
                float _Subtract_F3D8FDD5_Out_2;
                Unity_Subtract_float(_Multiply_F5E3A6D2_Out_2, _Split_E7F1684F_A_4, _Subtract_F3D8FDD5_Out_2);
                float _Clamp_B0EE4959_Out_3;
                Unity_Clamp_float(_Subtract_F3D8FDD5_Out_2, 0, 1, _Clamp_B0EE4959_Out_3);
                float _Property_D7514138_Out_0 = Vector1_38A4E771;
                float _Property_B18E438B_Out_0 = Vector1_A53BC3D7;
                float _FresnelEffect_25B14398_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_B18E438B_Out_0, _FresnelEffect_25B14398_Out_3);
                float _Clamp_F39FB499_Out_3;
                Unity_Clamp_float(_FresnelEffect_25B14398_Out_3, 0, 1, _Clamp_F39FB499_Out_3);
                float _Lerp_81991F7C_Out_3;
                Unity_Lerp_float(_Property_D7514138_Out_0, 0, _Clamp_F39FB499_Out_3, _Lerp_81991F7C_Out_3);
                float _Multiply_DBDE0961_Out_2;
                Unity_Multiply_float(_Clamp_B0EE4959_Out_3, _Lerp_81991F7C_Out_3, _Multiply_DBDE0961_Out_2);
                float _Multiply_56753FE8_Out_2;
                Unity_Multiply_float(_Clamp_2EC55C05_Out_3, _Multiply_DBDE0961_Out_2, _Multiply_56753FE8_Out_2);
                float _Multiply_FF496BEA_Out_2;
                Unity_Multiply_float(_Branch_AFB73BF_Out_3, _Multiply_56753FE8_Out_2, _Multiply_FF496BEA_Out_2);
                float _Multiply_C59D37B6_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, 0.02, _Multiply_C59D37B6_Out_2);
                float2 _TilingAndOffset_C3CF963C_Out_3;
                Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_C59D37B6_Out_2.xx), _TilingAndOffset_C3CF963C_Out_3);
                float4 _SampleTexture2D_37ABCB7F_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_C3CF963C_Out_3);
                float _SampleTexture2D_37ABCB7F_R_4 = _SampleTexture2D_37ABCB7F_RGBA_0.r;
                float _SampleTexture2D_37ABCB7F_G_5 = _SampleTexture2D_37ABCB7F_RGBA_0.g;
                float _SampleTexture2D_37ABCB7F_B_6 = _SampleTexture2D_37ABCB7F_RGBA_0.b;
                float _SampleTexture2D_37ABCB7F_A_7 = _SampleTexture2D_37ABCB7F_RGBA_0.a;
                float _Multiply_F64A45F0_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, 0.01, _Multiply_F64A45F0_Out_2);
                float2 _TilingAndOffset_92A30583_Out_3;
                Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_F64A45F0_Out_2.xx), _TilingAndOffset_92A30583_Out_3);
                float4 _SampleTexture2D_3AF4D147_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_92A30583_Out_3);
                float _SampleTexture2D_3AF4D147_R_4 = _SampleTexture2D_3AF4D147_RGBA_0.r;
                float _SampleTexture2D_3AF4D147_G_5 = _SampleTexture2D_3AF4D147_RGBA_0.g;
                float _SampleTexture2D_3AF4D147_B_6 = _SampleTexture2D_3AF4D147_RGBA_0.b;
                float _SampleTexture2D_3AF4D147_A_7 = _SampleTexture2D_3AF4D147_RGBA_0.a;
                float _Add_41B37CA_Out_2;
                Unity_Add_float(_SampleTexture2D_37ABCB7F_R_4, _SampleTexture2D_3AF4D147_R_4, _Add_41B37CA_Out_2);
                float _Multiply_90BD8805_Out_2;
                Unity_Multiply_float(_Multiply_FF496BEA_Out_2, _Add_41B37CA_Out_2, _Multiply_90BD8805_Out_2);
                float _Branch_D2C1007B_Out_3;
                Unity_Branch_float(_Property_6DFD3FCE_Out_0, _Multiply_90BD8805_Out_2, _Multiply_FF496BEA_Out_2, _Branch_D2C1007B_Out_3);
                surface.Color = (_Property_F4AFB0A2_Out_0.xyz);
                surface.Alpha = _Branch_D2C1007B_Out_3;
                surface.AlphaClipThreshold = 0;
                return surface;
            }

            // --------------------------------------------------
            // Structs and Packing

            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_Position;
                float3 positionWS;
                float3 normalWS;
                float4 texCoord0;
                float3 viewDirectionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
            };

            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_Position;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                float3 interp00 : TEXCOORD0;
                float3 interp01 : TEXCOORD1;
                float4 interp02 : TEXCOORD2;
                float3 interp03 : TEXCOORD3;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                output.interp01.xyz = input.normalWS;
                output.interp02.xyzw = input.texCoord0;
                output.interp03.xyz = input.viewDirectionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                return output;
            }

            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                output.texCoord0 = input.interp02.xyzw;
                output.viewDirectionWS = input.interp03.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                return output;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                output.WorldSpaceNormal = input.normalWS;
                output.WorldSpaceViewDirection = input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
                output.WorldSpacePosition = input.positionWS;
                output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
                output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
                output.uv0 = input.texCoord0;
                output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }


            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

                // Render State
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                Cull Back
                ZTest LEqual
                ZWrite On
                // ColorMask: <None>


                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                // Debug
                // <None>

                // --------------------------------------------------
                // Pass

                // Pragmas
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma target 2.0
                #pragma multi_compile_instancing

                // Keywords
                #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
                // GraphKeywords: <None>

                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define VARYINGS_NEED_POSITION_WS 
                #define VARYINGS_NEED_NORMAL_WS
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_VIEWDIRECTION_WS
                #define SHADERPASS_SHADOWCASTER
                #define REQUIRE_DEPTH_TEXTURE

                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

                // --------------------------------------------------
                // Graph

                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float Vector1_38A4E771;
                float4 Color_EDAE10A;
                float Vector1_A53BC3D7;
                float Boolean_24749E14;
                float Boolean_1D631463;
                float _StencilValue;
                CBUFFER_END
                TEXTURE2D(Texture2D_A40A22E0); SAMPLER(samplerTexture2D_A40A22E0); float4 Texture2D_A40A22E0_TexelSize;
                SAMPLER(_SampleTexture2D_37ABCB7F_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_3AF4D147_Sampler_3_Linear_Repeat);

                // Graph Functions

                void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                {
                    Out = lerp(False, True, Predicate);
                }

                void Unity_Distance_float3(float3 A, float3 B, out float Out)
                {
                    Out = distance(A, B);
                }

                void Unity_Subtract_float(float A, float B, out float Out)
                {
                    Out = A - B;
                }

                void Unity_Clamp_float(float In, float Min, float Max, out float Out)
                {
                    Out = clamp(In, Min, Max);
                }

                void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
                {
                    Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
                }

                void Unity_Multiply_float(float A, float B, out float Out)
                {
                    Out = A * B;
                }

                void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
                {
                    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
                }

                void Unity_Lerp_float(float A, float B, float T, out float Out)
                {
                    Out = lerp(A, B, T);
                }

                void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
                {
                    Out = UV * Tiling + Offset;
                }

                void Unity_Add_float(float A, float B, out float Out)
                {
                    Out = A + B;
                }

                // Graph Vertex
                // GraphVertex: <None>

                // Graph Pixel
                struct SurfaceDescriptionInputs
                {
                    float3 WorldSpaceNormal;
                    float3 WorldSpaceViewDirection;
                    float3 WorldSpacePosition;
                    float3 AbsoluteWorldSpacePosition;
                    float4 ScreenPosition;
                    float4 uv0;
                    float3 TimeParameters;
                };

                struct SurfaceDescription
                {
                    float Alpha;
                    float AlphaClipThreshold;
                };

                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float _Property_6DFD3FCE_Out_0 = Boolean_24749E14;
                    float _Property_38CEDF2A_Out_0 = Boolean_1D631463;
                    float4 _UV_4FCCC73A_Out_0 = IN.uv0;
                    float _Split_B5D12E44_R_1 = _UV_4FCCC73A_Out_0[0];
                    float _Split_B5D12E44_G_2 = _UV_4FCCC73A_Out_0[1];
                    float _Split_B5D12E44_B_3 = _UV_4FCCC73A_Out_0[2];
                    float _Split_B5D12E44_A_4 = _UV_4FCCC73A_Out_0[3];
                    float _Vector1_A5420D63_Out_0 = 1;
                    float _Branch_AFB73BF_Out_3;
                    Unity_Branch_float(_Property_38CEDF2A_Out_0, _Split_B5D12E44_G_2, _Vector1_A5420D63_Out_0, _Branch_AFB73BF_Out_3);
                    float _Distance_47B3418F_Out_2;
                    Unity_Distance_float3(_WorldSpaceCameraPos, IN.AbsoluteWorldSpacePosition, _Distance_47B3418F_Out_2);
                    float _Subtract_3022506E_Out_2;
                    Unity_Subtract_float(_Distance_47B3418F_Out_2, 0.5, _Subtract_3022506E_Out_2);
                    float _Clamp_2EC55C05_Out_3;
                    Unity_Clamp_float(_Subtract_3022506E_Out_2, 0, 1, _Clamp_2EC55C05_Out_3);
                    float _SceneDepth_6EE328B0_Out_1;
                    Unity_SceneDepth_Linear01_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_6EE328B0_Out_1);
                    float _Multiply_F5E3A6D2_Out_2;
                    Unity_Multiply_float(_SceneDepth_6EE328B0_Out_1, _ProjectionParams.z, _Multiply_F5E3A6D2_Out_2);
                    float4 _ScreenPosition_C0A8EBA5_Out_0 = IN.ScreenPosition;
                    float _Split_E7F1684F_R_1 = _ScreenPosition_C0A8EBA5_Out_0[0];
                    float _Split_E7F1684F_G_2 = _ScreenPosition_C0A8EBA5_Out_0[1];
                    float _Split_E7F1684F_B_3 = _ScreenPosition_C0A8EBA5_Out_0[2];
                    float _Split_E7F1684F_A_4 = _ScreenPosition_C0A8EBA5_Out_0[3];
                    float _Subtract_F3D8FDD5_Out_2;
                    Unity_Subtract_float(_Multiply_F5E3A6D2_Out_2, _Split_E7F1684F_A_4, _Subtract_F3D8FDD5_Out_2);
                    float _Clamp_B0EE4959_Out_3;
                    Unity_Clamp_float(_Subtract_F3D8FDD5_Out_2, 0, 1, _Clamp_B0EE4959_Out_3);
                    float _Property_D7514138_Out_0 = Vector1_38A4E771;
                    float _Property_B18E438B_Out_0 = Vector1_A53BC3D7;
                    float _FresnelEffect_25B14398_Out_3;
                    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_B18E438B_Out_0, _FresnelEffect_25B14398_Out_3);
                    float _Clamp_F39FB499_Out_3;
                    Unity_Clamp_float(_FresnelEffect_25B14398_Out_3, 0, 1, _Clamp_F39FB499_Out_3);
                    float _Lerp_81991F7C_Out_3;
                    Unity_Lerp_float(_Property_D7514138_Out_0, 0, _Clamp_F39FB499_Out_3, _Lerp_81991F7C_Out_3);
                    float _Multiply_DBDE0961_Out_2;
                    Unity_Multiply_float(_Clamp_B0EE4959_Out_3, _Lerp_81991F7C_Out_3, _Multiply_DBDE0961_Out_2);
                    float _Multiply_56753FE8_Out_2;
                    Unity_Multiply_float(_Clamp_2EC55C05_Out_3, _Multiply_DBDE0961_Out_2, _Multiply_56753FE8_Out_2);
                    float _Multiply_FF496BEA_Out_2;
                    Unity_Multiply_float(_Branch_AFB73BF_Out_3, _Multiply_56753FE8_Out_2, _Multiply_FF496BEA_Out_2);
                    float _Multiply_C59D37B6_Out_2;
                    Unity_Multiply_float(IN.TimeParameters.x, 0.02, _Multiply_C59D37B6_Out_2);
                    float2 _TilingAndOffset_C3CF963C_Out_3;
                    Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_C59D37B6_Out_2.xx), _TilingAndOffset_C3CF963C_Out_3);
                    float4 _SampleTexture2D_37ABCB7F_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_C3CF963C_Out_3);
                    float _SampleTexture2D_37ABCB7F_R_4 = _SampleTexture2D_37ABCB7F_RGBA_0.r;
                    float _SampleTexture2D_37ABCB7F_G_5 = _SampleTexture2D_37ABCB7F_RGBA_0.g;
                    float _SampleTexture2D_37ABCB7F_B_6 = _SampleTexture2D_37ABCB7F_RGBA_0.b;
                    float _SampleTexture2D_37ABCB7F_A_7 = _SampleTexture2D_37ABCB7F_RGBA_0.a;
                    float _Multiply_F64A45F0_Out_2;
                    Unity_Multiply_float(IN.TimeParameters.x, 0.01, _Multiply_F64A45F0_Out_2);
                    float2 _TilingAndOffset_92A30583_Out_3;
                    Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_F64A45F0_Out_2.xx), _TilingAndOffset_92A30583_Out_3);
                    float4 _SampleTexture2D_3AF4D147_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_92A30583_Out_3);
                    float _SampleTexture2D_3AF4D147_R_4 = _SampleTexture2D_3AF4D147_RGBA_0.r;
                    float _SampleTexture2D_3AF4D147_G_5 = _SampleTexture2D_3AF4D147_RGBA_0.g;
                    float _SampleTexture2D_3AF4D147_B_6 = _SampleTexture2D_3AF4D147_RGBA_0.b;
                    float _SampleTexture2D_3AF4D147_A_7 = _SampleTexture2D_3AF4D147_RGBA_0.a;
                    float _Add_41B37CA_Out_2;
                    Unity_Add_float(_SampleTexture2D_37ABCB7F_R_4, _SampleTexture2D_3AF4D147_R_4, _Add_41B37CA_Out_2);
                    float _Multiply_90BD8805_Out_2;
                    Unity_Multiply_float(_Multiply_FF496BEA_Out_2, _Add_41B37CA_Out_2, _Multiply_90BD8805_Out_2);
                    float _Branch_D2C1007B_Out_3;
                    Unity_Branch_float(_Property_6DFD3FCE_Out_0, _Multiply_90BD8805_Out_2, _Multiply_FF496BEA_Out_2, _Branch_D2C1007B_Out_3);
                    surface.Alpha = _Branch_D2C1007B_Out_3;
                    surface.AlphaClipThreshold = 0;
                    return surface;
                }

                // --------------------------------------------------
                // Structs and Packing

                // Generated Type: Attributes
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };

                // Generated Type: Varyings
                struct Varyings
                {
                    float4 positionCS : SV_Position;
                    float3 positionWS;
                    float3 normalWS;
                    float4 texCoord0;
                    float3 viewDirectionWS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                };

                // Generated Type: PackedVaryings
                struct PackedVaryings
                {
                    float4 positionCS : SV_Position;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    float3 interp00 : TEXCOORD0;
                    float3 interp01 : TEXCOORD1;
                    float4 interp02 : TEXCOORD2;
                    float3 interp03 : TEXCOORD3;
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };

                // Packed Type: Varyings
                PackedVaryings PackVaryings(Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp00.xyz = input.positionWS;
                    output.interp01.xyz = input.normalWS;
                    output.interp02.xyzw = input.texCoord0;
                    output.interp03.xyz = input.viewDirectionWS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    return output;
                }

                // Unpacked Type: Varyings
                Varyings UnpackVaryings(PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.positionWS = input.interp00.xyz;
                    output.normalWS = input.interp01.xyz;
                    output.texCoord0 = input.interp02.xyzw;
                    output.viewDirectionWS = input.interp03.xyz;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    return output;
                }

                // --------------------------------------------------
                // Build Graph Inputs

                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                    output.WorldSpaceNormal = input.normalWS;
                    output.WorldSpaceViewDirection = input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
                    output.WorldSpacePosition = input.positionWS;
                    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
                    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
                    output.uv0 = input.texCoord0;
                    output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                    return output;
                }


                // --------------------------------------------------
                // Main

                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

                ENDHLSL
            }

            Pass
            {
                Name "DepthOnly"
                Tags
                {
                    "LightMode" = "DepthOnly"
                }

                    // Render State
                    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                    Cull Back
                    ZTest LEqual
                    ZWrite On
                    ColorMask 0


                    HLSLPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                    // Debug
                    // <None>

                    // --------------------------------------------------
                    // Pass

                    // Pragmas
                    #pragma prefer_hlslcc gles
                    #pragma exclude_renderers d3d11_9x
                    #pragma target 2.0
                    #pragma multi_compile_instancing

                    // Keywords
                    // PassKeywords: <None>
                    // GraphKeywords: <None>

                    // Defines
                    #define _SURFACE_TYPE_TRANSPARENT 1
                    #define ATTRIBUTES_NEED_NORMAL
                    #define ATTRIBUTES_NEED_TANGENT
                    #define ATTRIBUTES_NEED_TEXCOORD0
                    #define VARYINGS_NEED_POSITION_WS 
                    #define VARYINGS_NEED_NORMAL_WS
                    #define VARYINGS_NEED_TEXCOORD0
                    #define VARYINGS_NEED_VIEWDIRECTION_WS
                    #define SHADERPASS_DEPTHONLY
                    #define REQUIRE_DEPTH_TEXTURE

                    // Includes
                    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

                    // --------------------------------------------------
                    // Graph

                    // Graph Properties
                    CBUFFER_START(UnityPerMaterial)
                    float Vector1_38A4E771;
                    float4 Color_EDAE10A;
                    float Vector1_A53BC3D7;
                    float Boolean_24749E14;
                    float Boolean_1D631463;
                    float _StencilValue;
                    CBUFFER_END
                    TEXTURE2D(Texture2D_A40A22E0); SAMPLER(samplerTexture2D_A40A22E0); float4 Texture2D_A40A22E0_TexelSize;
                    SAMPLER(_SampleTexture2D_37ABCB7F_Sampler_3_Linear_Repeat);
                    SAMPLER(_SampleTexture2D_3AF4D147_Sampler_3_Linear_Repeat);

                    // Graph Functions

                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }

                    void Unity_Distance_float3(float3 A, float3 B, out float Out)
                    {
                        Out = distance(A, B);
                    }

                    void Unity_Subtract_float(float A, float B, out float Out)
                    {
                        Out = A - B;
                    }

                    void Unity_Clamp_float(float In, float Min, float Max, out float Out)
                    {
                        Out = clamp(In, Min, Max);
                    }

                    void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
                    {
                        Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
                    }

                    void Unity_Multiply_float(float A, float B, out float Out)
                    {
                        Out = A * B;
                    }

                    void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
                    {
                        Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
                    }

                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }

                    void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
                    {
                        Out = UV * Tiling + Offset;
                    }

                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }

                    // Graph Vertex
                    // GraphVertex: <None>

                    // Graph Pixel
                    struct SurfaceDescriptionInputs
                    {
                        float3 WorldSpaceNormal;
                        float3 WorldSpaceViewDirection;
                        float3 WorldSpacePosition;
                        float3 AbsoluteWorldSpacePosition;
                        float4 ScreenPosition;
                        float4 uv0;
                        float3 TimeParameters;
                    };

                    struct SurfaceDescription
                    {
                        float Alpha;
                        float AlphaClipThreshold;
                    };

                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float _Property_6DFD3FCE_Out_0 = Boolean_24749E14;
                        float _Property_38CEDF2A_Out_0 = Boolean_1D631463;
                        float4 _UV_4FCCC73A_Out_0 = IN.uv0;
                        float _Split_B5D12E44_R_1 = _UV_4FCCC73A_Out_0[0];
                        float _Split_B5D12E44_G_2 = _UV_4FCCC73A_Out_0[1];
                        float _Split_B5D12E44_B_3 = _UV_4FCCC73A_Out_0[2];
                        float _Split_B5D12E44_A_4 = _UV_4FCCC73A_Out_0[3];
                        float _Vector1_A5420D63_Out_0 = 1;
                        float _Branch_AFB73BF_Out_3;
                        Unity_Branch_float(_Property_38CEDF2A_Out_0, _Split_B5D12E44_G_2, _Vector1_A5420D63_Out_0, _Branch_AFB73BF_Out_3);
                        float _Distance_47B3418F_Out_2;
                        Unity_Distance_float3(_WorldSpaceCameraPos, IN.AbsoluteWorldSpacePosition, _Distance_47B3418F_Out_2);
                        float _Subtract_3022506E_Out_2;
                        Unity_Subtract_float(_Distance_47B3418F_Out_2, 0.5, _Subtract_3022506E_Out_2);
                        float _Clamp_2EC55C05_Out_3;
                        Unity_Clamp_float(_Subtract_3022506E_Out_2, 0, 1, _Clamp_2EC55C05_Out_3);
                        float _SceneDepth_6EE328B0_Out_1;
                        Unity_SceneDepth_Linear01_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_6EE328B0_Out_1);
                        float _Multiply_F5E3A6D2_Out_2;
                        Unity_Multiply_float(_SceneDepth_6EE328B0_Out_1, _ProjectionParams.z, _Multiply_F5E3A6D2_Out_2);
                        float4 _ScreenPosition_C0A8EBA5_Out_0 = IN.ScreenPosition;
                        float _Split_E7F1684F_R_1 = _ScreenPosition_C0A8EBA5_Out_0[0];
                        float _Split_E7F1684F_G_2 = _ScreenPosition_C0A8EBA5_Out_0[1];
                        float _Split_E7F1684F_B_3 = _ScreenPosition_C0A8EBA5_Out_0[2];
                        float _Split_E7F1684F_A_4 = _ScreenPosition_C0A8EBA5_Out_0[3];
                        float _Subtract_F3D8FDD5_Out_2;
                        Unity_Subtract_float(_Multiply_F5E3A6D2_Out_2, _Split_E7F1684F_A_4, _Subtract_F3D8FDD5_Out_2);
                        float _Clamp_B0EE4959_Out_3;
                        Unity_Clamp_float(_Subtract_F3D8FDD5_Out_2, 0, 1, _Clamp_B0EE4959_Out_3);
                        float _Property_D7514138_Out_0 = Vector1_38A4E771;
                        float _Property_B18E438B_Out_0 = Vector1_A53BC3D7;
                        float _FresnelEffect_25B14398_Out_3;
                        Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_B18E438B_Out_0, _FresnelEffect_25B14398_Out_3);
                        float _Clamp_F39FB499_Out_3;
                        Unity_Clamp_float(_FresnelEffect_25B14398_Out_3, 0, 1, _Clamp_F39FB499_Out_3);
                        float _Lerp_81991F7C_Out_3;
                        Unity_Lerp_float(_Property_D7514138_Out_0, 0, _Clamp_F39FB499_Out_3, _Lerp_81991F7C_Out_3);
                        float _Multiply_DBDE0961_Out_2;
                        Unity_Multiply_float(_Clamp_B0EE4959_Out_3, _Lerp_81991F7C_Out_3, _Multiply_DBDE0961_Out_2);
                        float _Multiply_56753FE8_Out_2;
                        Unity_Multiply_float(_Clamp_2EC55C05_Out_3, _Multiply_DBDE0961_Out_2, _Multiply_56753FE8_Out_2);
                        float _Multiply_FF496BEA_Out_2;
                        Unity_Multiply_float(_Branch_AFB73BF_Out_3, _Multiply_56753FE8_Out_2, _Multiply_FF496BEA_Out_2);
                        float _Multiply_C59D37B6_Out_2;
                        Unity_Multiply_float(IN.TimeParameters.x, 0.02, _Multiply_C59D37B6_Out_2);
                        float2 _TilingAndOffset_C3CF963C_Out_3;
                        Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_C59D37B6_Out_2.xx), _TilingAndOffset_C3CF963C_Out_3);
                        float4 _SampleTexture2D_37ABCB7F_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_C3CF963C_Out_3);
                        float _SampleTexture2D_37ABCB7F_R_4 = _SampleTexture2D_37ABCB7F_RGBA_0.r;
                        float _SampleTexture2D_37ABCB7F_G_5 = _SampleTexture2D_37ABCB7F_RGBA_0.g;
                        float _SampleTexture2D_37ABCB7F_B_6 = _SampleTexture2D_37ABCB7F_RGBA_0.b;
                        float _SampleTexture2D_37ABCB7F_A_7 = _SampleTexture2D_37ABCB7F_RGBA_0.a;
                        float _Multiply_F64A45F0_Out_2;
                        Unity_Multiply_float(IN.TimeParameters.x, 0.01, _Multiply_F64A45F0_Out_2);
                        float2 _TilingAndOffset_92A30583_Out_3;
                        Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_F64A45F0_Out_2.xx), _TilingAndOffset_92A30583_Out_3);
                        float4 _SampleTexture2D_3AF4D147_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_A40A22E0, samplerTexture2D_A40A22E0, _TilingAndOffset_92A30583_Out_3);
                        float _SampleTexture2D_3AF4D147_R_4 = _SampleTexture2D_3AF4D147_RGBA_0.r;
                        float _SampleTexture2D_3AF4D147_G_5 = _SampleTexture2D_3AF4D147_RGBA_0.g;
                        float _SampleTexture2D_3AF4D147_B_6 = _SampleTexture2D_3AF4D147_RGBA_0.b;
                        float _SampleTexture2D_3AF4D147_A_7 = _SampleTexture2D_3AF4D147_RGBA_0.a;
                        float _Add_41B37CA_Out_2;
                        Unity_Add_float(_SampleTexture2D_37ABCB7F_R_4, _SampleTexture2D_3AF4D147_R_4, _Add_41B37CA_Out_2);
                        float _Multiply_90BD8805_Out_2;
                        Unity_Multiply_float(_Multiply_FF496BEA_Out_2, _Add_41B37CA_Out_2, _Multiply_90BD8805_Out_2);
                        float _Branch_D2C1007B_Out_3;
                        Unity_Branch_float(_Property_6DFD3FCE_Out_0, _Multiply_90BD8805_Out_2, _Multiply_FF496BEA_Out_2, _Branch_D2C1007B_Out_3);
                        surface.Alpha = _Branch_D2C1007B_Out_3;
                        surface.AlphaClipThreshold = 0;
                        return surface;
                    }

                    // --------------------------------------------------
                    // Structs and Packing

                    // Generated Type: Attributes
                    struct Attributes
                    {
                        float3 positionOS : POSITION;
                        float3 normalOS : NORMAL;
                        float4 tangentOS : TANGENT;
                        float4 uv0 : TEXCOORD0;
                        #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : INSTANCEID_SEMANTIC;
                        #endif
                    };

                    // Generated Type: Varyings
                    struct Varyings
                    {
                        float4 positionCS : SV_Position;
                        float3 positionWS;
                        float3 normalWS;
                        float4 texCoord0;
                        float3 viewDirectionWS;
                        #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : CUSTOM_INSTANCE_ID;
                        #endif
                        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                        #endif
                        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                        #endif
                        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                        #endif
                    };

                    // Generated Type: PackedVaryings
                    struct PackedVaryings
                    {
                        float4 positionCS : SV_Position;
                        #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : CUSTOM_INSTANCE_ID;
                        #endif
                        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                        #endif
                        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                        #endif
                        float3 interp00 : TEXCOORD0;
                        float3 interp01 : TEXCOORD1;
                        float4 interp02 : TEXCOORD2;
                        float3 interp03 : TEXCOORD3;
                        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                        #endif
                    };

                    // Packed Type: Varyings
                    PackedVaryings PackVaryings(Varyings input)
                    {
                        PackedVaryings output;
                        output.positionCS = input.positionCS;
                        output.interp00.xyz = input.positionWS;
                        output.interp01.xyz = input.normalWS;
                        output.interp02.xyzw = input.texCoord0;
                        output.interp03.xyz = input.viewDirectionWS;
                        #if UNITY_ANY_INSTANCING_ENABLED
                        output.instanceID = input.instanceID;
                        #endif
                        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        output.cullFace = input.cullFace;
                        #endif
                        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                        #endif
                        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                        #endif
                        return output;
                    }

                    // Unpacked Type: Varyings
                    Varyings UnpackVaryings(PackedVaryings input)
                    {
                        Varyings output;
                        output.positionCS = input.positionCS;
                        output.positionWS = input.interp00.xyz;
                        output.normalWS = input.interp01.xyz;
                        output.texCoord0 = input.interp02.xyzw;
                        output.viewDirectionWS = input.interp03.xyz;
                        #if UNITY_ANY_INSTANCING_ENABLED
                        output.instanceID = input.instanceID;
                        #endif
                        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        output.cullFace = input.cullFace;
                        #endif
                        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                        #endif
                        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                        #endif
                        return output;
                    }

                    // --------------------------------------------------
                    // Build Graph Inputs

                    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                    {
                        SurfaceDescriptionInputs output;
                        ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                        output.WorldSpaceNormal = input.normalWS;
                        output.WorldSpaceViewDirection = input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
                        output.WorldSpacePosition = input.positionWS;
                        output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
                        output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
                        output.uv0 = input.texCoord0;
                        output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                    #else
                    #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                    #endif
                    #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                        return output;
                    }


                    // --------------------------------------------------
                    // Main

                    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

                    ENDHLSL
                }

        }
            FallBack "Hidden/Shader Graph/FallbackError"
}
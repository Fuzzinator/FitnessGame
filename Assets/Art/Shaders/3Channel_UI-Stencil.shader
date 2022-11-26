Shader "Custom/UI/3Channel_Stencil"
{
    Properties
    {        
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Red_Channel_Color("Red Channel Color", Color) = (1, 1, 1, 1)
        _Green_Channel_PrimaryColor("Green Channel PrimaryColor", Color) = (1, 1, 1, 1)
        _BlueAccentColor("BlueAccentColor", Color) = (1, 1, 1, 1)
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        _ClipAlpha ("Alpha Clip", Float) = 0.1

        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"=""
        }

        ColorMask [_ColorMask]
        Pass
        {
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            Name "Sprite Unlit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Lighting Off
            ZTest LEqual
            ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM
            // Pragmas
            #pragma target 2.0
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEFORWARD
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                float4 texCoord0;
                float4 color;
                #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                float4 uv0;
            };

            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 interp0 : INTERP0;
                float4 interp1 : INTERP1;
                float4 interp2 : INTERP2;
                #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyzw = input.texCoord0;
                output.interp2.xyzw = input.color;
                #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
                #endif
                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp0.xyz;
                output.texCoord0 = input.interp1.xyzw;
                output.color = input.interp2.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
                #endif
                return output;
            }


            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _BlueAccentColor;
            float4 _Red_Channel_Color;
            float4 _Green_Channel_PrimaryColor;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _ClipAlpha;

            // Graph Includes
            // GraphIncludes: <None>

            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif

            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif

            // Graph Functions

            void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

            // Graph Vertex
            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                description.Normal = IN.ObjectSpaceNormal;
                description.Tangent = IN.ObjectSpaceTangent;
                return description;
            }

            #ifdef FEATURES_GRAPH_VERTEX
            Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
            {
                return output;
            }

            #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
            #endif

            // Graph Pixel
            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _Property_d2b23e2c0cac4af7b8aae3cbe51567ad_Out_0 = _Red_Channel_Color;
                UnityTexture2D _Property_0c444154941b4ddf8511fb6776b6de2e_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MainTex);
                float4 _SampleTexture2D_9f8be1640232492799089587d68491e8_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_0c444154941b4ddf8511fb6776b6de2e_Out_0.tex,
                    _Property_0c444154941b4ddf8511fb6776b6de2e_Out_0.samplerstate,
                    _Property_0c444154941b4ddf8511fb6776b6de2e_Out_0.GetTransformedUV(IN.uv0.xy));
                float _SampleTexture2D_9f8be1640232492799089587d68491e8_R_4 =
                    _SampleTexture2D_9f8be1640232492799089587d68491e8_RGBA_0.r;
                float _SampleTexture2D_9f8be1640232492799089587d68491e8_G_5 =
                    _SampleTexture2D_9f8be1640232492799089587d68491e8_RGBA_0.g;
                float _SampleTexture2D_9f8be1640232492799089587d68491e8_B_6 =
                    _SampleTexture2D_9f8be1640232492799089587d68491e8_RGBA_0.b;
                float _SampleTexture2D_9f8be1640232492799089587d68491e8_A_7 =
                    _SampleTexture2D_9f8be1640232492799089587d68491e8_RGBA_0.a;
                float4 _Multiply_d267783b3a5b4b7392bf635555035057_Out_2;
                Unity_Multiply_float4_float4(_Property_d2b23e2c0cac4af7b8aae3cbe51567ad_Out_0,
                                             (_SampleTexture2D_9f8be1640232492799089587d68491e8_R_4.xxxx),
                                             _Multiply_d267783b3a5b4b7392bf635555035057_Out_2);
                float4 _Property_7e1719796edd4144853f614e080b4315_Out_0 = _Green_Channel_PrimaryColor;
                float4 _Multiply_40c9b6e1c9cb475994901229beeb86c8_Out_2;
                Unity_Multiply_float4_float4((_SampleTexture2D_9f8be1640232492799089587d68491e8_G_5.xxxx),
                                             _Property_7e1719796edd4144853f614e080b4315_Out_0,
                                             _Multiply_40c9b6e1c9cb475994901229beeb86c8_Out_2);
                float4 _Add_4cb2694111b445b3adc73c1ec52f8bbc_Out_2;
                Unity_Add_float4(_Multiply_d267783b3a5b4b7392bf635555035057_Out_2,
                                 _Multiply_40c9b6e1c9cb475994901229beeb86c8_Out_2,
                                 _Add_4cb2694111b445b3adc73c1ec52f8bbc_Out_2);
                float4 _Property_c3918901407a4d998fca974dfef7ed8c_Out_0 = _BlueAccentColor;
                float4 _Multiply_6cb8ebfb98384f57a409aec173d832ab_Out_2;
                Unity_Multiply_float4_float4((_SampleTexture2D_9f8be1640232492799089587d68491e8_B_6.xxxx),
                                             _Property_c3918901407a4d998fca974dfef7ed8c_Out_0,
                                             _Multiply_6cb8ebfb98384f57a409aec173d832ab_Out_2);
                float4 _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2;
                Unity_Add_float4(_Add_4cb2694111b445b3adc73c1ec52f8bbc_Out_2,
                                 _Multiply_6cb8ebfb98384f57a409aec173d832ab_Out_2,
                                 _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2);
                float _Split_9db4d2d6c8574a1486df0e05623cd756_R_1 = _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2[0];
                float _Split_9db4d2d6c8574a1486df0e05623cd756_G_2 = _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2[1];
                float _Split_9db4d2d6c8574a1486df0e05623cd756_B_3 = _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2[2];
                float _Split_9db4d2d6c8574a1486df0e05623cd756_A_4 = _Add_9fd612c48c3942ab8c360c599b8a289f_Out_2[3];
                float _Clamp_088d188b093245a8aeb9eb4185168951_Out_3;
                Unity_Clamp_float(_Split_9db4d2d6c8574a1486df0e05623cd756_A_4, 0, 1,
                                  _Clamp_088d188b093245a8aeb9eb4185168951_Out_3);
                surface.BaseColor = (_Add_9fd612c48c3942ab8c360c599b8a289f_Out_2.xyz);
                surface.Alpha = _Clamp_088d188b093245a8aeb9eb4185168951_Out_3;
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (surface.Alpha - _ClipAlpha);
                #endif
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                output.uv0 = input.texCoord0;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}
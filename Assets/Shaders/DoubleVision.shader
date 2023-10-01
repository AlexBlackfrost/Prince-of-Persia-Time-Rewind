// This shader fills the mesh shape with a color predefined in the code.
Shader "DoubleVIsion/DoubleVision"{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties {
        _Color("Color", Color) = (.25, .5, .5, 1)
        [IntRange]_StencilID("Stencil ID", Range(0, 255)) = 0
    }

    // The SubShader block containing the Shader code. 
    SubShader {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Transaprent" "Queue"="Geometry+1" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass {
            // The HLSL code block. Unity SRP uses the HLSL language.
            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            Stencil{
                Ref[_StencilID]
				Comp Equal
                Pass Keep
                Fail Keep

            }
            HLSLPROGRAM

            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes {
                // The positionOS variable contains the vertex positions in object space.
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float4 _Color;
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            CBUFFER_START(UnityPerMaterial)
                            // The following line declares the _BaseMap_ST variable, so that you
                            // can use the _BaseMap variable in the fragment shader. The _ST 
                            // suffix is necessary for the tiling and offset function to work.
            float4 _MaskTex_ST;
            CBUFFER_END
            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN) {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MaskTex);
                // Returning the output.
                return OUT;
            }

            // The fragment shader definition.            
            float4 frag(Varyings i) : SV_Target {
                // Defining the color variable and returning it.
                i.uv.x += 0.2;
                float4 color = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                color.r = 0;
                color.a = 0.5;
                return color;
            }
            ENDHLSL
        }
    }
}
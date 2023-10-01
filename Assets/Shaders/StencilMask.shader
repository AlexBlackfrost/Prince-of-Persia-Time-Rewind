Shader "DoubleVision/StencilMask"{
	Properties{
		[IntRange]_StencilID("Stencil ID", Range(0, 255)) = 0
	}

	SubShader{
		Tags{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry-1"
		}
		

		Pass{
			Blend Zero One
			ZWrite Off

			Stencil {
                Ref [_StencilID]
                Comp Always
			    Pass Replace
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
            // This example uses the Attributes structure as an input structure in the vertex shader.
            struct Attributes {
                            // The positionOS variable contains the vertex positions in object space.
                float4 vertex : POSITION;
            };

            struct Varyings {
                            // The positions in this struct must have the SV_POSITION semantic.
                float4 pos : SV_POSITION;
            };
            
            
            Varyings vert(Attributes IN) {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions from object space to homogenous space
                OUT.pos = TransformObjectToHClip(IN.vertex);
                // Returning the output.
                return  OUT;
            }

            // The fragment shader definition.            
            float4 frag(Varyings i) : SV_Target {
                return float4(0.5, 0, 1, 1);
            } 
            ENDHLSL
		}
	}
}
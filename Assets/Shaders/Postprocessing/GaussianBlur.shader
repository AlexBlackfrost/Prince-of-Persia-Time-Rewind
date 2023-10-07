// Source: https://github.com/daniel-ilett/shaders-gaussian-blur/blob/main/Assets/Resources/Blur.shader
Shader "PostProcessing/GaussianBlur" {
    Properties{
        _MainTex ("Texture", 2D) = "white" { }
        _Spread("Standard Deviation (Spread)", Float) = 0 
        _GridSize("Grid Size", Int) = 1
        _DoubleVisionAlpha("Double Vision Alpha", Float) = 1
    }
    
    SubShader{
        Tags { 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        sampler2D _MainTex;
        uniform sampler2D _ZoomedColorTexture;
        uniform sampler2D _MaskTexture;
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            uniform float4 _ZoomedColorTexture_TexelSize;
            uniform float4 _MaskTexture_TexelSize;
            uint _GridSize;
            float _Spread;
            float _DoubleVisionAlpha;
        CBUFFER_END
        
        float gaussianBlur(int x) {
            float sigmaSquared = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigmaSquared)) * exp( -(x*x) / (2*sigmaSquared));
        }

        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        v2f vert(appdata v) {
            v2f o;
            o.vertex = TransformObjectToHClip(v.vertex);
            o.uv = v.uv;
            return o;
        }
        ENDHLSL   
        
 
        Pass{
            Name "Horizontal"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragHorizontal

            float4 fragHorizontal(v2f i) : SV_Target  {
                float4 maskTexture = tex2D(_MaskTexture, i.uv);
                
                float3 color = float3(0, 0, 0);
                float gridSum = 0.0f;
                int gridHalfWidth = (_GridSize - 1) / 2;
                for( int x = -gridHalfWidth; x<=gridHalfWidth; x++){
                    float gaussian = gaussianBlur(x);
                    gridSum += gaussian;
                    float2 uv = i.uv + float2(_MainTex_TexelSize.x * x, 0.0f);
                    color += gaussian * tex2D(_MainTex, uv).rgb;
                }
                color /= gridSum;
                
                return float4(color, maskTexture.r);
}

            ENDHLSL
        }

        Pass{
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragVertical

            float4 fragVertical(v2f i) : SV_Target {
                float4 maskTexture = tex2D(_MaskTexture, i.uv);
    
                float3 color = float3(0, 0, 0);
                float gridSum = 0.0f;

                int gridHalfHeight = (_GridSize - 1) / 2;
                for (int y = -gridHalfHeight; y <= gridHalfHeight; y++)
                {
                    float gaussian = gaussianBlur(y);
                    gridSum += gaussian;
                    float2 uv = i.uv + float2(0.0f, _MainTex_TexelSize.y * y);
                    color += gaussian * tex2D(_MainTex, uv).rgb;
                }
                color /= gridSum;
                return float4(color, maskTexture.r);
            }

            ENDHLSL
        }
        Pass{
            Name "Combine"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target {
                float4 maskTexture = tex2D(_MaskTexture, i.uv);
    
                float4 background =  tex2D(_ZoomedColorTexture, i.uv);
                float4 doubleVisionBlurred = tex2D(_MainTex, i.uv);
                float backgroundAlpha = 1.0 - _DoubleVisionAlpha;
                
                float alpha = maskTexture.r * _DoubleVisionAlpha;
                return float4(saturate(doubleVisionBlurred.r * alpha + background.r * (1 - alpha)),
                              saturate(doubleVisionBlurred.g * alpha + background.g * (1 - alpha)),
                              saturate(doubleVisionBlurred.b * alpha + background.b * (1 - alpha)),
                              1);
                
            }
            ENDHLSL
        }
    }
}
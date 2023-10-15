// Source: https://github.com/daniel-ilett/shaders-gaussian-blur/blob/main/Assets/Resources/Blur.shader
Shader "UI/TimeRewindLoad" {
    Properties{
        
        //_MainTex ("Texture", 2D) = "black" { }
        _Spread("Standard Deviation (Spread)", Float) = 0 
        _GridSize("Grid Size", Int) = 1
    }
    
    SubShader{
        Tags { 
            "QUEUE"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "UniversalMaterialType"="Unlit" 
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        sampler2D _MainTex;
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            uint _GridSize;
            float _Spread;
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
        
        // Modified Daniel Ilett's shader to account for transparency, check this post https://computergraphics.stackexchange.com/questions/5509/gaussian-blur-with-transparency
        Pass{
            Name "Horizontal"
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragHorizontal

            float4 fragHorizontal(v2f i) : SV_Target  {
                if (_GridSize <= 0) {
                    return float4(0, 0, 0, 0);
                }
                float4 color = float4(0, 0, 0, 0);
                float gridSum = 0.0f;
                int gridHalfWidth = (_GridSize - 1) / 2;
    
                for( int x = -gridHalfWidth; x<=gridHalfWidth; x++){
                    float gaussian = gaussianBlur(x);
                    gridSum += gaussian;
                    float2 uv = i.uv + float2(_MainTex_TexelSize.x * x, 0.0f);
                    //float4 texture = tex2D(_MainTex, uv);
                    color.rgb += gaussian * tex2D(_MainTex, uv).rgb * tex2D(_MainTex, uv).a;
                    color.a += tex2D(_MainTex, uv).a * gaussian;

                }
                color /= gridSum;
                
                gridSum = 0.0f;
                int gridHalfHeight = (_GridSize - 1) / 2;
                for (int y = -gridHalfHeight; y <= gridHalfHeight; y++) {
                    float gaussian = gaussianBlur(y);
                    gridSum += gaussian;
                    float2 uv = i.uv + float2(0.0f, _MainTex_TexelSize.y * y);
                    float4 tex = tex2D(_MainTex, uv);
                    color.rgb += gaussian * tex.rgb * tex.a;
                    color.a += tex.a * gaussian;

                }
                color /= gridSum;
                float4 tex = tex2D(_MainTex, i.uv);
                return float4(saturate(tex.rgb + color.rgb), saturate(color.a + tex.a));
                return float4(color.rgb, length(color.rgb));
}

            ENDHLSL
        }

    }
}
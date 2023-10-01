    //Make sure its only included once!
#ifndef MASK_TEXTURE_INCLUDED
#define MASK_TEXTURE_INCLUDED

TEXTURE2D(_GlobalMaskTexture);
SAMPLER(sampler_GlobalMaskTexture);
float4 _GlobalMaskTexture_TexelSize;
     
void getMaskTexture_float(in float2 uv, out float4 color)
{
    color = SAMPLE_TEXTURE2D(_GlobalMaskTexture, sampler_GlobalMaskTexture, uv).rgba;
}
#endif

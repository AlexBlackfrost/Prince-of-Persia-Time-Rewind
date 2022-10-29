// This is a modification of Ned Makes Games toon shader.
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


// This is a neat trick to work around a bug in the shader graph when
// enabling shadow keywords. Created by @cyanilux
// https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	#if (SHADERPASS != SHADERPASS_FORWARD)
		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	#endif
#endif

struct CustomLightingData {
	float3 positionWS;
	float3 normalWS;
	float3 viewDirectionWS;
	float3 baseColor;
	float3 sss;
	float specularIntensity;
	float ambientOcclusion;
	float specularSize;
	float innerLines;
	float smoothness;
	float specularIntensityFactor;
	float4 shadowCoord;
	float3 ambientColor;
	float smoothEdgeStart;
	float smoothEdgeEnd;
};


float GetSmoothnessPower(float rawSmoothness) {
	return exp2(10 * rawSmoothness + 1);
}


#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light, bool isMainLight) {
	/* light.color here doesn't have any effect when toon shading is being used. 
	Only makes sense when the last 2 lines are not commented and classic blinn-phong is used instead. */
	float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation); 

	float diffuse = saturate(dot(d.normalWS, light.direction));
	
	float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
	float specular = pow(specularDot, d.specularSize * GetSmoothnessPower(d.smoothness)) * diffuse * d.specularIntensity * d.specularIntensityFactor;

		//toon diffuse
	float diffuseToonMask = saturate(smoothstep(d.smoothEdgeStart, d.smoothEdgeStart + d.smoothEdgeEnd, diffuse * radiance));

	if (isMainLight) {
		if (d.ambientOcclusion <= 0) {
			diffuseToonMask = 0;
		}else if (d.ambientOcclusion >= 1) {
			diffuseToonMask = 1;
		}
	}
	float3 toonDiffuse = lerp(d.sss *light.color, d.baseColor * light.color, diffuseToonMask);

		//ambient
	if (isMainLight) {
		toonDiffuse += d.ambientColor; // adds indirect diffuse lighting, ambient lighting
	}
		//toon specular
	float3 specularToonMask = smoothstep(0, 0.0001, specular * radiance);
	float3 toonSpecular = lerp(float3(0,0,0), d.baseColor * light.color, specularToonMask);

	return (toonDiffuse + toonSpecular) * d.innerLines;

	// classic blinn-phong
	//float3 color = d.baseColor * radiance * (diffuse + specular);
	//return color;
}
#endif


float3 CalculateCustomLighting(CustomLightingData d) {
#ifdef SHADERGRAPH_PREVIEW
	float3 lightDir = float3(0.5, 0.5, 0);
    float intensity = saturate(dot(d.normalWS, lightDir)) +
        pow(saturate(dot(d.normalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
    return d.baseColor * intensity;

#else
	Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

	float3 color = 0;
	color += CustomLightHandling(d, mainLight, true);

	#ifdef _ADDITIONAL_LIGHTS
		uint numAdditionalLights = GetAdditionalLightsCount();
		for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
			Light light = GetAdditionalLight(lightI, d.positionWS, 1);
			color += CustomLightHandling(d, light, false);
		}
	#endif

	return color;
#endif
}


void CalculateCustomLighting_float(float3 Position, float3 Normal, float3 ViewDirection, float3 BaseColor, 
								   float3 SSS, float SpecularIntensity, float AmbientOcclusion, float SpecularSize, 
								   float InnerLines, float Smoothness, float SpecularIntensityFactor,
								   float3 AmbientColor, float SmoothEdgeStart, float SmoothEdgeEnd, out float3 Color) {
	CustomLightingData d;
	d.positionWS = Position;
	d.normalWS = Normal;
	//d.normalWS = TransformObjectToWorldNormal(Normal);
	d.viewDirectionWS = ViewDirection;
	d.baseColor = BaseColor;
	d.sss = SSS;
	d.specularSize = SpecularSize;
	d.specularIntensity = SpecularIntensity;
	d.ambientOcclusion = AmbientOcclusion;
	d.innerLines = InnerLines;
	d.ambientColor = AmbientColor;
	d.smoothness = Smoothness;
	d.specularIntensityFactor = SpecularIntensityFactor;
	d.smoothEdgeStart = SmoothEdgeStart;
	d.smoothEdgeEnd = SmoothEdgeEnd;

	#ifdef SHADERGRAPH_PREVIEW
		d.shadowCoord = 0;
	#else
		#if SHADOWS_SCREEN
			float4 positionCS = TransformWorldToHClip(Position);
			d.shadowCoord = ComputeScreenPos(positionCS);
		#else
			d.shadowCoord = TransformWorldToShadowCoord(Position);
		#endif
	#endif

	Color = CalculateCustomLighting(d);
}
#endif
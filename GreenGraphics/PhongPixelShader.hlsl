#include "Header.hlsli"
#define ShadowBias 0.01f
Texture2D<float> ShadowMap : register(t0);
TextureCube EnvironmentMap : register(t2);
SamplerState Sampler : register(s0);

float4 main(VertexScreenPositionNormalTexture surface) : SV_TARGET
{
	if(surface.Position.z < 0.f && MirrorMode) discard;

	float3 dLight = normalize(LightPosition - surface.Position);
	float3 dCamera = normalize(CameraPosition - surface.Position);
	float3 dReflected = reflect(-dLight, surface.Normal);
	float ambient = Reflectivity.x * Intensity.x;
	float diffuse = Reflectivity.y * max(dot(dLight, surface.Normal), 0.f) * Intensity.y;
	float specular = (diffuse > 0.f ? Reflectivity.z * pow(max(dot(dReflected, dCamera), 0.f), Reflectivity.w) * Intensity.z : 0.f);	

	float4 shadowTempCoords = mul(LightViewProjection, float4(surface.Position, 1.f));
	float2 shadowScreenCoords = shadowTempCoords.xy / shadowTempCoords.w;
	float2 shadowTexCoords = float2(0.f, 1.f) - (shadowScreenCoords / 2.f + 0.5f) / float2(-1.f, 1.f);
	float lightDistanceShadowMap = ShadowMap.Sample(Sampler, shadowTexCoords);
	float lightDistance = length(LightPosition - surface.Position);

	float intensity = (ambient + (1 - dot(shadowScreenCoords, shadowScreenCoords)) * (diffuse + specular) / (lightDistance * lightDistance));
	if(length(shadowScreenCoords) >= 1.f || lightDistanceShadowMap - lightDistance < -ShadowBias)
		intensity = ambient;

	float4 selfIntensity = float4(intensity, intensity, intensity, 1.0f);

	
	float3 dCameraReflected = reflect(-dCamera, surface.Normal);
	float4 environmentIntensity = EnvironmentMap.Sample(Sampler, float3(dCameraReflected.x, dCameraReflected.z, -dCameraReflected.y)).bgra * Intensity.w;
	
	return Color * (selfIntensity + environmentIntensity * 3.f);
}
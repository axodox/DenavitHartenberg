#include "Header.hlsli"
Texture2D ReflectionMap : register(t1);
TextureCube EnvironmentMap : register(t2);
SamplerState Sampler : register(s0);

float4 main(VertexScreenPositionNormalTexture surface) : SV_TARGET
{
	float3 dCamera = normalize(CameraPosition - surface.Position);
	float3 dReflected = reflect(-dCamera, surface.Normal);
	float4 environmentColor = EnvironmentMap.Sample(Sampler, float3(dReflected.x, dReflected.z, -dReflected.y)).bgra;

	float4 reflectedColor = ReflectionMap.Load(int3(surface.Screen.x, surface.Screen.y, 0));
	
	if(dot(reflectedColor, reflectedColor) < 4.f)
		return reflectedColor * Intensity.w;
	else
		return (reflectedColor + environmentColor) * Intensity.w;
}
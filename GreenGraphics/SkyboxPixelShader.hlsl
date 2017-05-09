#include "Header.hlsli"
TextureCube Texture : register(t2);
SamplerState Sampler : register(s0);

float4 main(VertexScreenPositionNormalTexture v) : SV_TARGET
{
	return Texture.Sample(Sampler, v.Position).bgra;
}
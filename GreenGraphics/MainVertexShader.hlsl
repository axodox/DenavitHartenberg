#include "Header.hlsli"
VertexScreenPositionNormalTexture main(VertexPositionNormalTexture vi)
{
	VertexScreenPositionNormalTexture vo;
	vo.Screen = mul(WorldViewProjection, float4(vi.Position, 1.f));
	vo.Position = mul(World, float4(vi.Position, 1.f));
	vo.Normal = normalize(mul(NormalWorld, float4(vi.Normal, 0.f)).xyz);
	vo.Texture = vi.Texture;
	return vo;
}

//#include "Header.hlsli"
//VertexPositionTextureOut main(VertexPositionTextureIn vi)
//{
//	float4 modelPos = float4(vi.Position, 1);
//	float4 screenPos;
//	screenPos = mul(WorldViewProjection, modelPos);	
//	VertexPositionTextureOut vo;
//	vo.Position = screenPos;
//	vo.Texture = vi.Texture;
//	return vo;
//}
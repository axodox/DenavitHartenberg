#include "Header.hlsli"
VertexScreenPositionColor main(VertexPositionColorIn vi)
{
	VertexScreenPositionColor vo;
	vo.Screen = mul(WorldViewProjection, float4(vi.Position, 1));
	vo.Position = mul(World, float4(vi.Position, 1));
	vo.Color = vi.Color;
	return vo;
}
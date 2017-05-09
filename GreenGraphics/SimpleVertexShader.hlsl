#include "Header.hlsli"
VertexPositionTextureOut main(VertexPositionTextureIn vi)
{
	VertexPositionTextureOut vo;
	vo.Position = float4(vi.Position, 1.f);
	vo.Texture = vi.Texture;
	return vo;
}
#include "Header.hlsli"

float main(VertexScreenPositionNormalTexture surface) : SV_TARGET
{
	return length(LightPosition - surface.Position);
}
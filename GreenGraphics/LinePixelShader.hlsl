#include "Header.hlsli"
float4 main(VertexScreenPositionColor vo) : SV_TARGET
{
	if(vo.Position.z < 0.f && MirrorMode) discard;
	return vo.Color;
}
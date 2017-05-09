#include "Header.hlsli"

[maxvertexcount(6)]
void main(
	triangle VertexScreenPositionNormalTexture input[3], 
	inout LineStream<VertexScreenPositionColor> output
)
{
	VertexScreenPositionColor o;
	float4 end;
	for (uint i = 0; i < 3; i++)
	{
		o.Screen = input[i].Screen;
		o.Position = input[i].Position;
		o.Color = float4(1.f, 0.f, 1.f, 1.f);
		output.Append(o);

		end = float4(input[i].Position + input[i].Normal * 0.025f, 1.f);
		o.Screen = mul(ViewProjection, end);
		o.Position = mul(World, end);
		o.Color = float4(1.f, 0.f, 1.f, 1.f);
		output.Append(o);
		output.RestartStrip();
	}
}
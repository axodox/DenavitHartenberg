cbuffer MainConstants : register(b0)
{
	float4x4 WorldViewProjection;
	float4x4 ViewProjection;
	float4x4 World;
	float4x4 NormalWorld;	
};

cbuffer LightingConstants : register(b1)
{
	float4x4 LightViewProjection;	
	//x: ambient, y: diffuse, z: specular reflection constant; w: shininess
	float4 Reflectivity;
	float3 CameraPosition;
	float3 LightPosition;
	//x: ambient, y: diffuse, z: specular intensity
	float4 Intensity;
	bool MirrorMode;
}

cbuffer JointConstants : register(b2)
{
	float4 Color;
}

struct VertexPositionNormalTexture
{
	float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 Texture : TEXCOORD0;	
};

struct VertexScreenPositionNormalTexture
{
	float4 Screen : SV_POSITION;
	float3 Position : POSITION1;
	float3 Normal : NORMAL0;
	float2 Texture : TEXCOORD0;
};

struct VertexPositionTextureIn
{
	float3 Position : POSITION0;
	float2 Texture : TEXCOORD0;
};

struct VertexPositionTextureOut
{
	float4 Position : SV_POSITION;
	float2 Texture : TEXCOORD0;
};

struct VertexPositionColorIn
{
	float3 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexScreenPositionColor
{
	float4 Screen : SV_POSITION;
	float3 Position : POSITIONT;
	float4 Color : COLOR0;
};
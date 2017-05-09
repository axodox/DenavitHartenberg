#pragma once
#pragma unmanaged
#include "stdafx.h"
using namespace DirectX;

namespace Green
{
	namespace Graphics
	{
		class VertexDefinition
		{
		public:
			D3D11_INPUT_ELEMENT_DESC* Description;
			int ElementCount;

			VertexDefinition(
				D3D11_INPUT_ELEMENT_DESC* description,
				int elementCount)
			{
				Description = description;
				ElementCount = elementCount;
			}

			~VertexDefinition()
			{
				if(ElementCount > 0)
					delete [ElementCount] Description;
			}

			static D3D11_INPUT_ELEMENT_DESC CreateDescription(
				LPCSTR semanticName,
				UINT semanticIndex,
				DXGI_FORMAT format,
				UINT inputSlot,
				D3D11_INPUT_CLASSIFICATION inputSlotClass,
				UINT instanceDataStepRate,
				UINT alignedByteOffset = D3D11_APPEND_ALIGNED_ELEMENT)
			{
				D3D11_INPUT_ELEMENT_DESC ied;
				ied.SemanticName = semanticName;
				ied.SemanticIndex = semanticIndex;
				ied.Format = format;
				ied.InputSlot = inputSlot;
				ied.AlignedByteOffset = alignedByteOffset;
				ied.InputSlotClass = inputSlotClass;
				ied.InstanceDataStepRate = instanceDataStepRate;
				return ied;
			}

			static VertexDefinition
				*VertexPosition,
				*VertexPositionColor,
				*VertexPositionTexture,
				*VertexPositionNormalTexture;

			static void Init()
			{
				D3D11_INPUT_ELEMENT_DESC *ied;
				
				ied = new D3D11_INPUT_ELEMENT_DESC[1];
				ied[0] = CreateDescription(
					"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				VertexPosition = new VertexDefinition(ied, 1);

				ied = new D3D11_INPUT_ELEMENT_DESC[2];
				ied[0] = CreateDescription(
					"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				ied[1] = CreateDescription(
					"COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				VertexPositionColor = new VertexDefinition(ied, 2);

				ied = new D3D11_INPUT_ELEMENT_DESC[2];
				ied[0] = CreateDescription(
					"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				ied[1] = CreateDescription(
					"TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				VertexPositionTexture = new VertexDefinition(ied, 2);

				ied = new D3D11_INPUT_ELEMENT_DESC[3];
				ied[0] = CreateDescription(
					"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				ied[1] = CreateDescription(
					"NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				ied[2] = CreateDescription(
					"TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
					0, D3D11_INPUT_PER_VERTEX_DATA, 0);
				VertexPositionNormalTexture = new VertexDefinition(ied, 3);
			}
		};

		__interface IVertexDefinition
		{
		public:
			const VertexDefinition* GetVertexDefinition();
		};

		VertexDefinition* VertexDefinition::VertexPosition = 0;
		VertexDefinition* VertexDefinition::VertexPositionColor = 0;
		VertexDefinition* VertexDefinition::VertexPositionTexture = 0;
		VertexDefinition* VertexDefinition::VertexPositionNormalTexture = 0;

		struct VertexPosition
		{
			XMFLOAT3 Position;
			VertexPosition() {}
			VertexPosition(XMFLOAT3 position) : Position(position) {}
			VertexPosition(float x, float y, float z) : Position(XMFLOAT3(x, y, z)) {}

			static const VertexDefinition* GetVertexDefinition()
			{
				return VertexDefinition::VertexPosition;
			}
		};

		struct VertexPositionColor
		{
			XMFLOAT3 Position;
			XMFLOAT4 Color;
			VertexPositionColor() {}
			VertexPositionColor(XMFLOAT3 position, XMFLOAT4 color) : Position(position), Color(color) {}

			static const VertexDefinition* GetVertexDefinition()
			{
				return VertexDefinition::VertexPositionColor;
			}
		};

		struct VertexPositionTexture
		{
			XMFLOAT3 Position;
			XMFLOAT2 Texture;
			VertexPositionTexture() {}
			VertexPositionTexture(XMFLOAT3 position, XMFLOAT2 texture) : Position(position), Texture(texture) {}
			
			static const VertexDefinition* GetVertexDefinition()
			{
				return VertexDefinition::VertexPositionTexture;
			}
		};

		struct VertexPositionNormalTexture
		{
			XMFLOAT3 Position;
			XMFLOAT3 Normal;
			XMFLOAT2 Texture;
			VertexPositionNormalTexture() {}
			VertexPositionNormalTexture(XMFLOAT3 position, XMFLOAT3 normal, XMFLOAT2 texture) : Position(position), Normal(normal), Texture(texture) {}

			static const VertexDefinition* GetVertexDefinition()
			{
				return VertexDefinition::VertexPositionNormalTexture;
			}
		};
	}
}
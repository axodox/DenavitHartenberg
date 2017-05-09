#pragma once
#pragma unmanaged
#define WIN32_LEAN_AND_MEAN
#define GaussCoeffCount 9
#include "stdafx.h"
#include "Helper.h"
#include "GreenGraphicsClasses.h"
#include "GreenGraphicsModules.h"
using namespace std;
using namespace DirectX;
using namespace Gdiplus;

#define CylinderDivisions 32
#define ShadowMapWidth 1024
#define ShadowMapHeight 1024
#define FloorSize 2
#define SkyboxSize 5

namespace Green
{
	namespace Graphics
	{
		class DirectXWindow
		{
		private:
			ULONG_PTR GdiPlusToken;

			struct MainConstants {
				XMFLOAT4X4 WorldViewProjection;
				XMFLOAT4X4 ViewProjection;
				XMFLOAT4X4 World;
				XMFLOAT4X4 NormalWorld;
			} MainOptions;

			struct LightingConstants
			{
				XMFLOAT4X4 LightViewProjection;
				//x: ambient, y: diffuse, z: specular reflection constant; w: shininess
				XMFLOAT4 Reflectivity;
				XMFLOAT4 CameraPosition;
				XMFLOAT4 LightPosition;
				//x: ambient, y: diffuse, z: specular intensity; w: unused
				XMFLOAT4 Intensity;			
				bool MirrorMode;
			} LightingOptions;

			struct JointConstans
			{
				XMFLOAT4 Color;
			} JointOptions;

			//Graphics resources
			GraphicsDeviceWithSwapChain *Device;
			RenderTarget2DWithDepthBuffer *RTShadowMap, *RTReflection;
			VertexShader *VSMain, *VSLine, *VSSimple;
			GeometryShader *GSNormal;
			PixelShader *PSSimple, *PSLine, *PSPhong, *PSShadowMap, *PSReflection, *PSSkybox;
			ConstantBuffer<MainConstants>* CBMain;			
			ConstantBuffer<LightingConstants>* CBLighting;
			ConstantBuffer<JointConstans>* CBJoint;
			RasterizerState *RSCullClockwise, *RSCullCounterClockwise;
			TextureCube* TEnvironmentMap;
			Cube* MainCube;
			Cylinder* MainCylinder;
			Quad* TestQuad;
			Cross* MainCross;
			Square* MainSquare;
			SamplerState *SSMain;
			BlendState *BSOpaque, *BSAdditive;		

			//Rendering parameters
			XMFLOAT4X4 FloorWorld, SkyboxWorld, ViewProjection, CameraViewProjection, ReflectedViewProjection, LightViewProjection, SkyboxViewProjection, Projection;
			float RotX, RotY, Distance, CubeSize, JointSize;
			bool CoordSystemsAreVisible, MoveLightWithCamera, SkyboxVisible;
			XMFLOAT4 BackgroundColor, FloorColor, BaseColor, ManipulatorColor, HighlightColor;
			XMFLOAT3 LastLightPosition;
			enum class RenderingPasses { Shadow, Reflection, Visible } Pass;

			//Model parameters
			int HighlightedJoint;
			XMFLOAT4X4 ManipulatorWorld, ManipulatorBase;
			struct Joint
			{
				XMFLOAT4X4 World, Base, Pivot, SegmentA, SegmentB;
				bool DrawBase, DrawPivot, DrawSegmentA, DrawSegmentB, SegmentAIsCylinder,  PivotIsCylinder, SegmentBIsCylinder, BaseIsCylinder;
			};
			Joint* Joints;
			int JointCount;
			
			void SetView()
			{
				//Set up camera
				XMFLOAT3 cameraPos = XMFLOAT3(
					Distance * cos(RotX) * cos(RotY),
					Distance * sin(RotX) * cos(RotY),
					Distance * sin(RotY));
				XMFLOAT3 up = XMFLOAT3(0, 0, 1);
				XMMATRIX cameraView = XMMatrixLookAtRH(XMLoadFloat3(&cameraPos), XMVectorZero(), XMLoadFloat3(&up));
				XMMATRIX cameraProjection = XMLoadFloat4x4(&Projection);
				XMMATRIX cameraViewProjection = cameraView * cameraProjection;
				XMStoreFloat4x4(&CameraViewProjection, cameraViewProjection);
				XMMATRIX reflectedViewProjection = XMMatrixScaling(1.f, 1.f, -1.f) * cameraViewProjection;
				XMStoreFloat4x4(&ReflectedViewProjection, reflectedViewProjection);

				//Set up lighting
				XMFLOAT3 lightPos;
				if(MoveLightWithCamera)
				{
					lightPos = LastLightPosition = XMFLOAT3(
						1.2f * Distance * cos(RotX) * cos(RotY),
						1.2f * Distance * sin(RotX) * cos(RotY),
						1.2f * Distance * sin(RotY));
				}
				else
					lightPos = LastLightPosition;
				LightingOptions.CameraPosition = XMFLOAT4(cameraPos.x, cameraPos.y, cameraPos.z, 0.f);
				LightingOptions.LightPosition = XMFLOAT4(lightPos.x, lightPos.y, lightPos.z, 0.f);
				XMMATRIX lightView = XMMatrixLookAtRH(XMLoadFloat3(&lightPos), XMVectorZero(), XMLoadFloat3(&up));;
				XMMATRIX lightProjection = XMMatrixPerspectiveFovRH(XM_PIDIV4, (float)ShadowMapWidth / ShadowMapHeight, 1, 100);
				XMMATRIX lightViewProjection = lightView * lightProjection;
				XMStoreFloat4x4(&LightViewProjection, lightViewProjection);
				LightingOptions.LightViewProjection = LightViewProjection;
				CBLighting->Update(&LightingOptions);

				//Set up skybox
				XMMATRIX skyboxView = XMMatrixLookAtRH(XMVectorZero(), -XMLoadFloat3(&cameraPos), XMLoadFloat3(&up));
				XMMATRIX skyboxViewProjection = XMMatrixRotationX(XM_PIDIV2) * skyboxView * cameraProjection;
				XMStoreFloat4x4(&SkyboxViewProjection, skyboxViewProjection);

				Draw();
			}

			static XMMATRIX CreateDenavitHartenbergBaseTransform(float q, float d, float a, float alpha)
			{
				XMMATRIX dq = XMMatrixRotationZ(q);
				XMMATRIX dd = XMMatrixTranslation(0, 0, d);
				XMMATRIX da = XMMatrixTranslation(a, 0, 0);
				XMMATRIX dalpha = XMMatrixRotationX(alpha);
				return dalpha * da * dd * dq;
			}

			static XMMATRIX CreateDenavitHartenbergPivotTransform(float q, float d)
			{
				XMMATRIX dq = XMMatrixRotationZ(q);
				XMMATRIX dd = XMMatrixTranslation(0, 0, d);
				return dd * dq;
			}

			Joint* GenerateJointData(float* model, byte* params, int jointCount, XMFLOAT4X4 &manipulatorBase, XMFLOAT4X4 &manipulatorWorld)
			{
				Joint* joints = new Joint[jointCount], *joint = joints;

				enum Parameters : byte { Q, D, A, Alpha };
				Parameters* param = (Parameters*)params;
				
				XMMATRIX base, pivot, pivotBase, segmentA, segmentB, world = XMMatrixIdentity();
				float* p = model, q, d, a, alpha;
				for(int i = 0; i < jointCount; i++)
				{
					//Geting parameters
					q = p[0];
					d = p[1];
					a = p[2];
					alpha = p[3];

					//What and how should we draw?
					joint->DrawBase = d != 0.f;
					joint->DrawPivot = a != 0.f;
					joint->DrawSegmentA = d != 0.f;
					joint->DrawSegmentB = a != 0.f;
					joint->SegmentAIsCylinder = *param == Q || *param == D;
					joint->SegmentBIsCylinder = *param == Alpha || *param == A;
					joint->BaseIsCylinder = *param == Q && a == 0.f;
					joint->PivotIsCylinder = *param == Q || *param == Alpha;
					
					//Where we should draw?
					base = XMMatrixScaling(CubeSize, CubeSize, CubeSize) * world;
					pivotBase = CreateDenavitHartenbergPivotTransform(q, d) * world;
					segmentA = XMMatrixScaling(JointSize, JointSize, abs(d)) * XMMatrixTranslation(0, 0, d/2) * world;
					segmentB = XMMatrixScaling(JointSize, JointSize, abs(a)) * XMMatrixTranslation(0, 0, a/2) * XMMatrixRotationY(XM_PIDIV2) * pivotBase;
					pivot = XMMatrixScaling(CubeSize, CubeSize, CubeSize) * pivotBase;
					if(*param == Alpha) pivot = XMMatrixRotationY(XM_PIDIV2) * pivot;

					XMStoreFloat4x4(&joint->World, world);
					world = CreateDenavitHartenbergBaseTransform(q, d, a, alpha) * world;
					XMStoreFloat4x4(&joint->Base, base);
					XMStoreFloat4x4(&joint->SegmentA, segmentA);
					XMStoreFloat4x4(&joint->Pivot, pivot);
					XMStoreFloat4x4(&joint->SegmentB, segmentB);

					//Next
					p += 4;
					joint++;
					param++;
				}

				//Manipulator parameters
				XMStoreFloat4x4(&manipulatorWorld, world);
				XMStoreFloat4x4(&manipulatorBase, XMMatrixScaling(CubeSize, CubeSize, CubeSize) * world);

				return joints;
			}

			void SetWorld(XMFLOAT4X4 world)
			{
				MainOptions.WorldViewProjection = Multiply(world, ViewProjection);
				MainOptions.ViewProjection = ViewProjection;
				MainOptions.World = world;
				MainOptions.NormalWorld = ToNormalTransform(world);				
				CBMain->Update(&MainOptions);
			}

			void DrawJoint(Joint* joint)
			{
				//Base
				if(joint->DrawBase)
				{
					SetWorld(joint->Base);
					if(joint->BaseIsCylinder)
						MainCylinder->Draw();
					else
						MainCube->Draw();
				}

				//SegmentA
				if(joint->DrawSegmentA)
				{
					SetWorld(joint->SegmentA);
					if(joint->SegmentAIsCylinder)
						MainCylinder->Draw();
					else
						MainCube->Draw();
				}

				//Pivot
				if(joint->DrawPivot)
				{
					SetWorld(joint->Pivot);
					if(joint->PivotIsCylinder)
						MainCylinder->Draw();
					else
						MainCube->Draw();
				}

				//SegmentB
				if(joint->DrawSegmentB)
				{
					SetWorld(joint->SegmentB);
					if(joint->SegmentBIsCylinder)
						MainCylinder->Draw();
					else
						MainCube->Draw();
				}		
			}
			
			void DrawScene()
			{
				if(JointCount == 0) return;		

				//Draw floor
				SetWorld(FloorWorld);
				if(Pass == RenderingPasses::Shadow)
				{
					Device->SetShaders(VSMain, PSShadowMap);
					MainSquare->Draw();
				}
				else
				{
					Device->SetShaders(VSMain, PSPhong);
					JointOptions.Color = FloorColor;
					CBJoint->Update(&JointOptions);
					MainSquare->Draw();
				}
				if(Pass == RenderingPasses::Visible)
				{
					Device->SetAsRenderTargetWithoutDepthBuffer();
					BSAdditive->Apply();
					Device->SetShaders(VSMain, PSReflection);
				MainSquare->Draw();
					BSOpaque->Apply();
					Device->SetAsRenderTarget();
				}
				
				//Draw joints
				Joint* joint = Joints;
				for(int i = 0; i < JointCount; i++)
				{
					//Draw joint
					if(Pass == RenderingPasses::Shadow)
					{
						Device->SetShaders(VSMain, PSShadowMap);
					}
					else
					{
						Device->SetShaders(VSMain, PSPhong);
						JointOptions.Color = (i == HighlightedJoint ? HighlightColor : BaseColor);
						CBJoint->Update(&JointOptions);
					}
					DrawJoint(joint);

					//Draw cross
					if(Pass != RenderingPasses::Shadow && (i == HighlightedJoint || (HighlightedJoint != - 1 && i == HighlightedJoint + 1) || (i == 0 && CoordSystemsAreVisible)))
					{
						SetWorld(joint->World);
						Device->SetShaders(VSLine, PSLine);
						MainCross->Draw();
					}

					joint++;
				}

				//Draw manipulator
				if(Pass == RenderingPasses::Shadow)
				{
					Device->SetShaders(VSMain, PSShadowMap);
				}
				else
				{
					Device->SetShaders(VSMain, PSPhong);
					JointOptions.Color = ManipulatorColor;
					CBJoint->Update(&JointOptions);
				}				
				SetWorld(ManipulatorBase);
				MainCube->Draw();
				
				//Draw manipulator cross
				if(Pass != RenderingPasses::Shadow && (CoordSystemsAreVisible || HighlightedJoint == JointCount - 1))
				{
					SetWorld(ManipulatorWorld);
					Device->SetShaders(VSLine, PSLine);
					MainCross->Draw();
				}
			}

			void Draw()
			{
				SSMain->SetForPS();

				//Shadow map pass
				Pass = RenderingPasses::Shadow;
				RSCullClockwise->Set();
				ViewProjection = LightViewProjection;
				RTShadowMap->Clear();
				RTShadowMap->SetAsRenderTarget();
				DrawScene();

				//Reflection pass
				LightingOptions.MirrorMode = true;
				CBLighting->Update(&LightingOptions);
				Pass = RenderingPasses::Reflection;
				RSCullCounterClockwise->Set();
				ViewProjection = ReflectedViewProjection;
				RTReflection->Clear((float*)&BackgroundColor);
				RTReflection->SetAsRenderTarget();
				RTShadowMap->SetForPS(0);
				DrawScene();
				LightingOptions.MirrorMode = false;
				CBLighting->Update(&LightingOptions);

				//Visible pass
				Pass = RenderingPasses::Visible;
				Device->Clear((float*)&BackgroundColor);

				if(SkyboxVisible)
				{
					Device->SetAsRenderTargetWithoutDepthBuffer();
					RSCullCounterClockwise->Set();
					ViewProjection = SkyboxViewProjection;
					TEnvironmentMap->SetForPS(2);
					SetWorld(SkyboxWorld);
					Device->SetShaders(VSMain, PSSkybox);
					MainCube->Draw();
				}

				Device->SetAsRenderTarget();
				RSCullClockwise->Set();
				ViewProjection = CameraViewProjection;
				RTShadowMap->SetForPS(0);
				RTReflection->SetForPS(1);
				DrawScene();


				/*RSCullCounterClockwise->Set();
				Device->SetAsRenderTarget();
				Device->Clear((float*)&BackgroundColor);
				Device->SetShaders(VSSimple, PSSimple);
				RTReflection->SetForPS();
				TestQuad->Draw();*/

				Device->Present();
			}
		public:
			DirectXWindow(HWND hWnd)
			{
				GdiplusStartupInput gsi;
				GdiplusStartup(&GdiPlusToken, &gsi, 0);

				Device = new GraphicsDeviceWithSwapChain(hWnd);
				
				//Shaders
				VSMain = new VertexShader(Device, L"MainVertexShader.cso");
				VSMain->SetInputLayout(VertexDefinition::VertexPositionNormalTexture);
				VSLine = new VertexShader(Device, L"LineVertexShader.cso");
				VSLine->SetInputLayout(VertexDefinition::VertexPositionColor);
				VSSimple = new VertexShader(Device, L"SimpleVertexShader.cso");
				VSSimple->SetInputLayout(VertexDefinition::VertexPositionTexture);
				GSNormal = new GeometryShader(Device, L"NormalGeometryShader.cso");
				PSSimple = new PixelShader(Device, L"SimplePixelShader.cso");
				PSLine = new PixelShader(Device, L"LinePixelShader.cso");
				PSPhong = new PixelShader(Device, L"PhongPixelShader.cso");
				PSShadowMap = new PixelShader(Device, L"ShadowMapPixelShader.cso");
				PSReflection = new PixelShader(Device, L"ReflectionPixelShader.cso");
				PSSkybox = new PixelShader(Device, L"SkyboxPixelShader.cso");

				//Constant buffers
				CBMain = new ConstantBuffer<MainConstants>(Device);
				CBLighting = new ConstantBuffer<LightingConstants>(Device);
				CBJoint = new ConstantBuffer<JointConstans>(Device);

				CBMain->SetForVS();
				CBMain->SetForGS();
				CBMain->SetForPS();
				CBLighting->SetForPS(1);
				CBJoint->SetForPS(2);

				//Textures
				TEnvironmentMap = TextureCube::FromFile(Device, L"environmentMap.png");

				//States
				RSCullClockwise = new RasterizerState(Device, RasterizerState::CullClockwise);
				RSCullCounterClockwise = new RasterizerState(Device, RasterizerState::CullCounterClockwise);
				SSMain = new SamplerState(Device, D3D11_FILTER_ANISOTROPIC, D3D11_TEXTURE_ADDRESS_CLAMP);
				BSOpaque = new BlendState(Device, BlendState::Opaque);
				BSAdditive = new BlendState(Device, BlendState::Additive);

				//Render targets
				RTShadowMap = new RenderTarget2DWithDepthBuffer(Device, ShadowMapWidth, ShadowMapHeight, DXGI_FORMAT_R32_FLOAT);
				RTReflection = new RenderTarget2DWithDepthBuffer(Device, DXGI_FORMAT_R8G8B8A8_UNORM);

				//Models
				MainCross = new Cross(Device, 1);
				MainCube = new Cube(Device);
				MainCylinder = new Cylinder(Device, CylinderDivisions);
				MainSquare = new Square(Device);
				TestQuad = new Quad(Device);

				//Init variables
				MoveLightWithCamera = true;
				XMStoreFloat4x4(&FloorWorld, XMMatrixScaling(FloorSize, FloorSize, FloorSize));
				XMStoreFloat4x4(&SkyboxWorld, XMMatrixScaling(SkyboxSize, SkyboxSize, SkyboxSize));

				ZeroMemory(&MainOptions, sizeof(MainOptions));
				ZeroMemory(&LightingOptions, sizeof(LightingOptions));

				LightingOptions.LightPosition = XMFLOAT4(5, 5, 5, 0);
				LightingOptions.Intensity = XMFLOAT4(0.2f, 4, 2, 0);
				LightingOptions.Reflectivity = XMFLOAT4(1, 1, 1, 5);
				
				Joints = nullptr;
				JointCount = 0;
			}

			void SetView(float rotX, float rotY, float distance)
			{
				RotX = XMConvertToRadians(rotX);
				RotY = XMConvertToRadians(rotY);
				Distance = distance;
				SetView();
			}

			void SetShading(bool coordSystemsAreVisible,
				float* backgroundColor, float* floorColor, float* baseColor, 
				float* manipulatorColor, float* highlightColor, bool moveLightWithCamera,
				float ambientLight, float diffuseLight, float specularLight, float totalReflectivity,
				bool skyboxVisible)
			{
				CoordSystemsAreVisible = coordSystemsAreVisible;				
				BackgroundColor = XMFLOAT4(backgroundColor);
				FloorColor = XMFLOAT4(floorColor);
				BaseColor = XMFLOAT4(baseColor);
				ManipulatorColor = XMFLOAT4(manipulatorColor);
				HighlightColor = XMFLOAT4(highlightColor);		
				LightingOptions.Intensity = XMFLOAT4(ambientLight, diffuseLight, specularLight, totalReflectivity);
				SkyboxVisible = skyboxVisible;
				SetView();	
				MoveLightWithCamera = moveLightWithCamera;
			}

			void SetActiveJoint(int index)
			{
				HighlightedJoint = index;
				Draw();
			}

			void SetModel(float* model, byte* params, int jointCount, float componentSize)
			{
				CubeSize = componentSize * 0.1f;
				JointSize = componentSize * 0.05f;
				if(Joints) delete [JointCount] Joints;
				Joints = GenerateJointData(model, params, jointCount, ManipulatorBase, ManipulatorWorld);
				JointCount = jointCount;
				Draw();
			}

			void Resize()
			{
				Device->Resize();

				delete RTReflection;
				RTReflection = new RenderTarget2DWithDepthBuffer(Device, DXGI_FORMAT_R8G8B8A8_UNORM);

				XMStoreFloat4x4(&Projection, XMMatrixPerspectiveFovRH(XM_PIDIV4, Device->GetAspectRatio(), 0.1f, 10));
				SetView();
			}
			
			~DirectXWindow()
			{
				delete VSMain;
				delete VSLine;
				delete VSSimple;
				delete GSNormal;
				delete PSSimple;
				delete PSLine;
				delete PSPhong;
				delete PSShadowMap;
				delete PSReflection;
				delete PSSkybox;

				delete CBMain;
				delete CBLighting;
				delete CBJoint;
				delete TEnvironmentMap;

				delete RSCullClockwise;
				delete RSCullCounterClockwise;
				delete SSMain;
				delete BSOpaque;
				delete BSAdditive;

				delete RTShadowMap;
				delete RTReflection;

				delete MainCross;
				delete MainCube;
				delete MainCylinder;
				delete MainSquare;
				delete TestQuad;
				delete Device;

				GdiplusShutdown(GdiPlusToken);
			}
		};
	}
}
#pragma once
#pragma unmanaged
#include "GreenGraphics.h"
#pragma managed
using namespace std;
using namespace System;
using namespace System::Windows;
using namespace System::Windows::Interop;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::IO;

namespace Green
{
	namespace Graphics
	{
		public ref class GraphicsCanvas : public HwndHost
		{
		private:
			DirectXWindow* XWindow;
			HWND Host, Canvas;
			static GraphicsCanvas()
			{
				VertexDefinition::Init();
			}
		protected:
			virtual HandleRef BuildWindowCore(HandleRef hwndParent) override
			{
				HWND parent = (HWND)hwndParent.Handle.ToPointer();

				Host = nullptr;
				Host = CreateWindowEx(
					0, L"static", L"",
					WS_CHILD,
					0, 0, (int)Width, (int)Height,
					parent, 
					0, nullptr, 0);
				XWindow = new DirectXWindow(Host);
				return HandleRef(this, (IntPtr)Host);
			}

			virtual void DestroyWindowCore(HandleRef hwnd) override
			{
				XWindow = nullptr;
				delete XWindow;
				DestroyWindow((HWND)hwnd.Handle.ToPointer());
			}

			bool ResizeNeeded;
			virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, bool %handled) override
			{
				static int resizeTimer = 0;
				switch (msg)
				{
				case WM_SIZE:
					resizeTimer = SetTimer((HWND)hwnd.ToPointer(), resizeTimer, 100, 0);
					ResizeNeeded = true;
					break;
				case WM_TIMER:
					if(ResizeNeeded)
					{
						XWindow->Resize();
						ResizeNeeded = false;
					}
					break;
				}
				handled = false;
				return IntPtr::Zero;
			}
		public:
			GraphicsCanvas()
			{
				XWindow = nullptr;
			}

			~GraphicsCanvas()
			{
				delete XWindow;
			}

			DirectXWindow* GetDirectXWindow()
			{
				return XWindow;
			}

			void SetView(float rotX, float rotY, float distance)
			{
				if (XWindow)
				{
					XWindow->SetView(rotX, rotY, distance);
				}
			}

			void SetShading(bool coordSystemsAreVisible,
        cli::array<float>^ backgroundColor, cli::array<float>^ floorColor,
        cli::array<float>^ baseColor, cli::array<float>^ manipulatorColor,
        cli::array<float>^ highlightColor, bool moveLightWithCamera,
				float ambientLight, float diffuseLight, float specularLight,
				float totalReflectivity, bool skyboxVisible)
			{
				if (XWindow)
				{
					pin_ptr<float> pBackgroundColor = &backgroundColor[0];
					pin_ptr<float> pFloorColor = &floorColor[0];
					pin_ptr<float> pBaseColor = &baseColor[0];
					pin_ptr<float> pManipulatorColor = &manipulatorColor[0];
					pin_ptr<float> pHighlightColor = &highlightColor[0];
					XWindow->SetShading( 
						coordSystemsAreVisible,
						pBackgroundColor, pFloorColor, 
						pBaseColor, pManipulatorColor, 
						pHighlightColor, moveLightWithCamera,
						ambientLight, diffuseLight, specularLight,
						totalReflectivity, skyboxVisible);
				}
			}

      property int ActiveJoint {
        int get()
        {
          if (XWindow)
          {
            return XWindow->GetActiveJoint();
          }
          else
          {
            return 0;
          }
        }
        void set(int index)
        {
          if (XWindow)
          {
            XWindow->SetActiveJoint(index);
          }
        }
      }

			void SetModel(cli::array<float>^ model, cli::array<byte>^ params, int jointCount, float componentSize)
			{
				if (!XWindow || model->Length == 0) return;
				pin_ptr<float> pModel = &model[0];
				pin_ptr<byte> pParams = &params[0];
				XWindow->SetModel(pModel, pParams, jointCount, componentSize);
			}
		};

	}
}


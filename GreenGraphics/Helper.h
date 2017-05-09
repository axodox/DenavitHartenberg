#pragma once
#pragma unmanaged
#include "Stdafx.h"
#define Throw(p) { if(p) { throw p; } }
#define SafeDelete(p) { if(p) { delete p; p=nullptr; } }

#define LPWSTRDelete(str) { if(str) { delete [wcslen(str) + 1] str; str=nullptr; } }
#define LPSTRDelete(str) { if(str) { delete [strlen(str) + 1] str; str=nullptr; } }

#define SafeRelease(p) { if(p) { p->Release(); p=nullptr; } }

using namespace DirectX;
using namespace Gdiplus;
int GetEncoderClsid(const WCHAR* form, CLSID* pClsid)
{
	UINT num;
	UINT size;
	ImageCodecInfo* pImageCodecInfo=NULL;
	GetImageEncodersSize(&num,&size);
	if(size==0)
		return -1;
	
	pImageCodecInfo=(ImageCodecInfo*)(malloc(size));
	if(pImageCodecInfo==NULL)
		return -1;
	GetImageEncoders(num,size,pImageCodecInfo);
	for(UINT j=0;j<num;j++)
	{
		if(wcscmp(pImageCodecInfo[j].MimeType,form)==0)
		{
			*pClsid = pImageCodecInfo[j].Clsid;
			free(pImageCodecInfo);
			return j;
		}
	}
	free(pImageCodecInfo);
	return -1;
}

void Release(IUnknown* object)
{
	int count = object->Release();
	if(count)
	{
		char text[10];
		itoa(count, text, 10);
		MessageBoxA(0, text, 0, 0);
	}
}

void Error(HRESULT hr)
{
	if(FAILED(hr))
	{
		_com_error ce = _com_error(hr);
	    //MessageBox(NULL, ce.ErrorMessage(), L"Error", MB_OK);
		FILE* hF = nullptr;
		_wfopen_s(&hF, L"error.txt", L"a");
		
		fputws(ce.ErrorMessage(), hF);
		fputws(L"\r\n", hF);
		fclose(hF);
	}
}

void LoadFile(LPWSTR path, void* &data, int& length)
{
	FILE* hF = nullptr;
	_wfopen_s(&hF, path, L"rb");
	fseek(hF, 0, SEEK_END);
	length = ftell(hF);
	fseek(hF, 0, SEEK_SET);
	data = malloc(length);
	fread_s(data, length, length, 1, hF);
	fclose(hF);
}

#define Load4x4(matrix) XMLoadFloat4x4(&XMFLOAT4X4(matrix))

XMFLOAT4X4 Invert(XMFLOAT4X4 matrix)
{
	XMFLOAT4X4 matrixInverse;
	XMMATRIX m = XMLoadFloat4x4(&matrix);
	XMMATRIX i = XMMatrixInverse(0, m);
	XMStoreFloat4x4(&matrixInverse, i);
	return matrixInverse;
}

XMFLOAT4X4 Multiply(XMFLOAT4X4 matrix1, XMFLOAT4X4 matrix2)
{
	XMFLOAT4X4 r;
	XMStoreFloat4x4(&r, XMLoadFloat4x4(&matrix1) * XMLoadFloat4x4(&matrix2));
	return r;
}

XMFLOAT4X4 ToNormalTransform(XMFLOAT4X4 matrix)
{
	XMFLOAT4X4 r;
	XMStoreFloat4x4(&r, XMMatrixTranspose(XMMatrixInverse(0, XMLoadFloat4x4(&matrix))));
	return r;
}

LPSTR LPWSTRToLPSTR(LPWSTR wstr)
{
	size_t length = wcslen(wstr) + 1;
    size_t convertedChars = 0;
    char* str = new char[length];
    wcstombs_s(&convertedChars, str, length, wstr, _TRUNCATE);
    return str;
}

char ClampToChar(float x)
{
	if (x < -1.f) return -127;
	else if (x > 1.f) return 127;
	else return (byte)(x * 127);
}

byte ClampToByte(float x)
{
	if (x < 0.f) return 0;
	else if (x > 1.f) return 255;
	else return (byte)(x * 255);
}

typedef void (*Callback)(void* param); 

bool IsIdentity(XMFLOAT4X4 matrix)
{
	return 
		matrix._11 == 1.f && matrix._12 == 0.f && matrix._13 == 0.f && matrix._14 == 0.f && 
		matrix._21 == 0.f && matrix._22 == 1.f && matrix._23 == 0.f && matrix._24 == 0.f && 
		matrix._31 == 0.f && matrix._32 == 0.f && matrix._33 == 1.f && matrix._34 == 0.f && 
		matrix._41 == 0.f && matrix._42 == 0.f && matrix._43 == 0.f && matrix._44 == 1.f;
}

#pragma managed
#include <vcclr.h>
LPWSTR StringToLPWSTR(System::String^ str)
{
	pin_ptr<const wchar_t> wch =  PtrToStringChars(str);
	size_t origsize = wcslen(wch) + 1;
	const size_t newsizew = origsize;
    wchar_t *wcstring = new wchar_t[newsizew];
    wcscpy_s(wcstring, newsizew, wch);
	return wcstring;
}

bool Is4x4(array<float, 2>^ array)
{
	return array->GetLength(0) == 4 && array->GetLength(1) == 4;
}

bool Is3x3(array<float, 2>^ array)
{
	return array->GetLength(0) == 3 && array->GetLength(1) == 3;
}

array<float, 2>^ To4x4(array<float, 2>^ input)
{
	array<float, 2>^ o = gcnew array<float, 2>(4, 4);
	for(int j = 0; j < 3; j++)
	for(int i = 0; i < 3; i++)
		o[j, i] = input[j, i];
	o[3, 3] = 1.f;
	return o;
}

array<float, 2>^ Expand4x4(array<float, 2>^ input)
{
	array<float, 2>^ output = gcnew array<float, 2>(4, 4);
	for(int j = 0; j < 3; j++)
	{
		for(int i = 0; i < 2; i++)
			output[j, i] = input[j, i];
		if(j != 2) output[j, 3] = input[j, 2];
	}
	output[2, 2] = input[2, 2];
	output[3, 3] = 1.f;
	return output;
}

using namespace System;
namespace Green
{
	namespace Extensions
	{
		public ref class StatusEventArgs : public EventArgs
		{
		public:
			String^ Text;
			Boolean ShowProgress;
			Double Progress;
			StatusEventArgs(String^ text)
			{
				Text = text;
				ShowProgress = false;
				Progress = float::NaN;
			}
			StatusEventArgs(String^ text, Boolean showProgress)
			{
				Text = text;
				ShowProgress = true;
				Progress = float::NaN;
			}
			StatusEventArgs(String^ text, Boolean showProgress, Double progress)
			{
				Text = text;
				ShowProgress = showProgress;
				Progress = progress;
			}
		};

		public delegate void StatusEventHandler(Object^ sender, StatusEventArgs^ e);
	}
}
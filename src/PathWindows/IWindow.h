#pragma once
#include "pch.h"

class IWindow
{
public:
	virtual ~IWindow() {};

	virtual HWND GetHandle() = 0;

	virtual HRESULT Initialize() = 0;
	virtual HRESULT RunMessageLoop() = 0;
};

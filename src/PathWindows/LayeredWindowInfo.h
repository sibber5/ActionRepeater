#pragma once
#include "pch.h"

namespace PathWindows
{
	class LayeredWindowInfo
	{
	public:
		LayeredWindowInfo(LONG width, LONG height);

		HRESULT Update(HWND hWnd, HDC source);

	private:
		const POINT m_sourcePos;
		SIZE m_size;
		BLENDFUNCTION m_blend;
		UPDATELAYEREDWINDOWINFO m_info;
	};
}

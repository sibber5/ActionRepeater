#include "pch.h"
#include "LayeredWindowInfo.h"

using namespace PathWindows;

LayeredWindowInfo::LayeredWindowInfo(LONG width, LONG height) :
	m_sourcePos(),
	m_size(),
	m_blend(),
	m_info()
{
	m_size = { width, height };

	m_blend.SourceConstantAlpha = 255;
	m_blend.AlphaFormat = AC_SRC_ALPHA;

	m_info.cbSize = sizeof(UPDATELAYEREDWINDOWINFO);
	m_info.pptSrc = &m_sourcePos;
	m_info.pptDst = nullptr;
	m_info.psize = &m_size;
	m_info.pblend = &m_blend;
	m_info.dwFlags = ULW_ALPHA;
}

HRESULT LayeredWindowInfo::Update(HWND hWnd, HDC source)
{
	m_info.hdcSrc = source;
	if (UpdateLayeredWindowIndirect(hWnd, &m_info) == 0)
	{
		return E_FAIL;
	}

	return S_OK;
}

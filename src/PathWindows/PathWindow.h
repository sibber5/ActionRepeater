#pragma once
#include "pch.h"
#include "LayeredWindowInfo.h"
#include <vector>

template<class T>
inline void SafeRelease(T** ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        (*ppT) = NULL;
    }
}

#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

namespace PathWindows
{
    class PathWindow
    {
    public:
        PathWindow();
        ~PathWindow();

        HWND GetHandle();

        HRESULT Initialize();

        HRESULT RunMessageLoop();

        HRESULT AddPoint(POINT point, bool render);
        HRESULT AddPoints(POINT* points, int length);

        HRESULT ClearPoints();

        HRESULT Render();

    private:
        const int WND_WIDTH;
        const int WND_HEIGHT;

        LayeredWindowInfo m_info;

        HWND m_hWnd;

        ID2D1Factory* m_pD2Factory;
        IWICImagingFactory* m_pWICFactory;
        ID2D1StrokeStyle* m_pStrokeStyle;

        ID2D1RenderTarget* m_pRenderTarget;
        ID2D1GdiInteropRenderTarget* m_pInteropTarget;
        ID2D1SolidColorBrush* m_pPathBrush;

        std::vector<D2D1_POINT_2F> m_points;

        HRESULT CreateDeviceIndependentResources();

        HRESULT CreateDeviceResources();

        void DiscardDeviceResources();

        static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
    };
}

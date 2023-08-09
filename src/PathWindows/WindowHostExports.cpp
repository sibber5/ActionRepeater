#include "pch.h"
#include "WindowHost.h"
#include "PathWindow.h"
#include "DrawablePathWindow.h"

using namespace PathWindows;

extern "C" __declspec(dllexport) HRESULT __cdecl CreatePathWindow(POINT* points, int length, WindowHost** ppWrapper)
{
    SetLastError(0);
    HRESULT hr = S_OK;

    WindowHost* wrapper = new WindowHost(new PathWindow());

    hr = wrapper->GetThreadHR();
    if (FAILED(hr))
    {
        (*ppWrapper) = 0;
        return hr;
    }

    (*ppWrapper) = wrapper;

    if (points)
    {
        hr = static_cast<PathWindow*>(wrapper->GetPWindow())->AddPoints(points, length);
    }

    return hr;
}

extern "C" __declspec(dllexport) HRESULT __cdecl CreateDrawablePathWindow(MouseMovement* points, int length, WindowClosingCallback windowClosingCallback, WindowHost** ppWrapper)
{
    SetLastError(0);
    HRESULT hr = S_OK;

    if (!windowClosingCallback) return E_INVALIDARG;

    WindowHost* wrapper = new WindowHost(
        points
        ? new DrawablePathWindow(points, length, windowClosingCallback)
        : new DrawablePathWindow(windowClosingCallback)
    );

    hr = wrapper->GetThreadHR();
    if (FAILED(hr))
    {
        (*ppWrapper) = 0;
        return hr;
    }

    (*ppWrapper) = wrapper;

    return hr;
}

extern "C" __declspec(dllexport) IWindow* __cdecl GetPWindow(WindowHost* pWrapper)
{
    return pWrapper->GetPWindow();
}

extern "C" __declspec(dllexport) HRESULT __cdecl DestroyPathWindow(WindowHost* pWrapper)
{
    SetLastError(0);
    if (!pWrapper) return E_POINTER;

    return pWrapper->CloseWindow();
}

extern "C" __declspec(dllexport) void __cdecl DisposeDangerous(WindowHost* pWrapper)
{
    if (!pWrapper) return;

    delete pWrapper;
}

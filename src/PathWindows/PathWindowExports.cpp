#include "pch.h"
#include "PathWindow.h"

using namespace PathWindows;

extern "C" __declspec(dllexport) HRESULT __cdecl AddPointToPath(PathWindow* pPathWindow, POINT point, bool render)
{
    if (!pPathWindow) return E_POINTER;

    return pPathWindow->AddPoint(point, render);
}

extern "C" __declspec(dllexport) HRESULT __cdecl AddPointsToPath(PathWindow* pPathWindow, POINT* points, int length)
{
    if (!pPathWindow) return E_POINTER;

    return pPathWindow->AddPoints(points, length);
}

extern "C" __declspec(dllexport) HRESULT __cdecl ClearPoints(PathWindow* pPathWindow)
{
    if (!pPathWindow) return E_POINTER;

    return pPathWindow->ClearPoints();
}

extern "C" __declspec(dllexport) HRESULT __cdecl Render(PathWindow* pPathWindow)
{
    if (!pPathWindow) return E_POINTER;

    return pPathWindow->Render();
}

#include "pch.h"
#include "PathWindowWrapper.h"

using namespace PathWindows;

extern "C" __declspec(dllexport) HRESULT __cdecl CreatePathWindow(POINT* points, int length, PathWindowWrapper** ppWrapper)
{
    SetLastError(0);
    HRESULT hr = S_OK;

    PathWindowWrapper* wrapper = new PathWindowWrapper();

    hr = wrapper->m_threadHR;
    if (FAILED(hr))
    {
        (*ppWrapper) = 0;
        return hr;
    }

    (*ppWrapper) = wrapper;

    if (points)
    {
        hr = wrapper->m_pPathWindow->AddPoints(points, length);
    }

    return hr;
}

extern "C" __declspec(dllexport) HRESULT __cdecl DestroyPathWindow(PathWindowWrapper* pWrapper)
{
    SetLastError(0);
    if (!pWrapper) return E_POINTER;

    return pWrapper->ClosePathWindow();
}

extern "C" __declspec(dllexport) void __cdecl DisposeDangerous(PathWindowWrapper* pWrapper)
{
    if (!pWrapper) return;

    delete pWrapper;
}

extern "C" __declspec(dllexport) HRESULT __cdecl AddPointToPath(PathWindowWrapper* pWrapper, POINT point, bool render)
{
    if (!pWrapper) return E_POINTER;
    if (!pWrapper->m_pPathWindow) return E_UNEXPECTED;

    return pWrapper->m_pPathWindow->AddPoint(point, render);
}

extern "C" __declspec(dllexport) HRESULT __cdecl AddPointsToPath(PathWindowWrapper* pWrapper, POINT* points, int length)
{
    if (!pWrapper) return E_POINTER;
    if (!pWrapper->m_pPathWindow) return E_UNEXPECTED;

    return pWrapper->m_pPathWindow->AddPoints(points, length);
}

extern "C" __declspec(dllexport) HRESULT __cdecl ClearPoints(PathWindowWrapper* pWrapper)
{
    if (!pWrapper) return E_POINTER;
    if (!pWrapper->m_pPathWindow) return E_UNEXPECTED;

    return pWrapper->m_pPathWindow->ClearPoints();
}

extern "C" __declspec(dllexport) HRESULT __cdecl Render(PathWindowWrapper* pWrapper)
{
    if (!pWrapper) return E_POINTER;
    if (!pWrapper->m_pPathWindow) return E_UNEXPECTED;

    return pWrapper->m_pPathWindow->Render();
}

PathWindowWrapper::PathWindowWrapper() :
    m_pPathWindow(nullptr),
    m_isRunning(false),
    m_threadHR(S_OK),
    m_wndInitCondVar(),
    m_wndInitMutex(),
    m_thread(&PathWindowWrapper::OpenPathWindow, this)
{
    std::unique_lock<std::mutex> lk(m_wndInitMutex);
    m_wndInitCondVar.wait(lk, [this] { return m_isRunning; });
}

PathWindowWrapper::~PathWindowWrapper()
{
    ClosePathWindow();
}

void PathWindowWrapper::OpenPathWindow()
{
    m_threadHR = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (FAILED(m_threadHR)) return;

    m_pPathWindow = new PathWindow();
    m_threadHR = m_pPathWindow->Initialize();
    if (FAILED(m_threadHR))
    {
        delete m_pPathWindow;
        m_pPathWindow = nullptr;
        return;
    }

    {
        std::lock_guard<std::mutex> lk(m_wndInitMutex);
        m_isRunning = true;
    }
    m_wndInitCondVar.notify_all();

    m_threadHR = m_pPathWindow->RunMessageLoop();

    CoUninitialize();
}

HRESULT PathWindowWrapper::ClosePathWindow()
{
    if (!m_pPathWindow) return E_FAIL;

    if (PostMessage(m_pPathWindow->GetHandle(), WM_CLOSE, 0, 0) == 0) return E_FAIL;

    m_thread.join();
    m_isRunning = false;

    delete m_pPathWindow;
    m_pPathWindow = nullptr;

    return m_threadHR;
}

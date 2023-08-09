#include "pch.h"
#include "WindowHost.h"

using namespace PathWindows;

WindowHost::WindowHost(IWindow* pPathWindow) :
    m_pWindow(pPathWindow),
    m_isRunning(false),
    m_threadHR(S_OK),
    m_wndInitCondVar(),
    m_wndInitMutex(),
    m_thread(&WindowHost::CreateAndShowWindow, this)
{
    std::unique_lock<std::mutex> lk(m_wndInitMutex);
    m_wndInitCondVar.wait(lk, [this] { return m_isRunning; });
}

WindowHost::~WindowHost()
{
    CloseWindow();
    if (m_thread.joinable()) m_thread.join();
}

IWindow* WindowHost::GetPWindow()
{
    return m_pWindow;
}

HRESULT WindowHost::GetThreadHR()
{
    return m_threadHR;
}

void WindowHost::CreateAndShowWindow()
{
    m_threadHR = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (FAILED(m_threadHR)) return;

    m_threadHR = m_pWindow->Initialize();
    if (FAILED(m_threadHR))
    {
        delete m_pWindow;
        m_pWindow = nullptr;
        return;
    }

    {
        std::lock_guard<std::mutex> lk(m_wndInitMutex);
        m_isRunning = true;
    }
    m_wndInitCondVar.notify_all();

    m_threadHR = m_pWindow->RunMessageLoop();

    CoUninitialize();
    
    m_isRunning = false;

    delete m_pWindow;
    m_pWindow = nullptr;
}

HRESULT WindowHost::CloseWindow()
{
    if (!m_pWindow)
    {
        if (m_thread.joinable()) m_thread.join();
        return E_FAIL;
    }

    if (PostMessage(m_pWindow->GetHandle(), WM_CLOSE, 0, 0) == 0) return E_FAIL;

    m_thread.join();

    return m_threadHR;
}

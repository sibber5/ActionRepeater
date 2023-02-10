#include "pch.h"
#include "PathWindow.h"

#define HR(rval) {\
                     hr = rval;\
                     if (FAILED(hr)) return hr;\
                 }

using namespace PathWindows;
using Microsoft::WRL::ComPtr;

PathWindow::PathWindow() :
    WND_WIDTH(GetSystemMetrics(SM_CXVIRTUALSCREEN)),
    WND_HEIGHT(GetSystemMetrics(SM_CYVIRTUALSCREEN)),

    m_info(WND_WIDTH, WND_HEIGHT),

    m_hWnd(nullptr),

    m_pD2Factory(nullptr),

    m_pRenderTarget(nullptr),
    m_pInteropTarget(nullptr),
    m_pPathBrush(nullptr),
    m_pStrokeStyle(nullptr)
{}

PathWindow::~PathWindow()
{
    SafeRelease(&m_pD2Factory);
    SafeRelease(&m_pRenderTarget);
    SafeRelease(&m_pInteropTarget);
    SafeRelease(&m_pPathBrush);
    SafeRelease(&m_pStrokeStyle);
}

HWND PathWindow::GetHandle()
{
    return m_hWnd;
}

HRESULT PathWindow::Initialize()
{
    HRESULT hr;

    HR(CreateDeviceIndependentResources());

    WNDCLASSEX wcex{};
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = PathWindow::WndProc;
    wcex.hInstance = HINST_THISCOMPONENT;
    wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wcex.lpszClassName = L"PathWindow";

    RegisterClassEx(&wcex);


    // In terms of using the correct DPI, to create a window at a specific size like this, the procedure is to first create the window hidden. Then we get
    // the actual DPI from the HWND (which will be assigned by whichever monitor the window is created on). Then we use SetWindowPos to resize it to the
    // correct DPI-scaled size, then we use ShowWindow to show it.

    m_hWnd = CreateWindowEx(
        WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_NOACTIVATE,
        wcex.lpszClassName,
        L"ActionRepeater Path Window",
        WS_POPUP,
        0,
        0,
        0,
        0,
        nullptr,
        nullptr,
        HINST_THISCOMPONENT,
        this
    );

    if (!m_hWnd) return hr;

    float dpi = static_cast<float>(GetDpiForWindow(m_hWnd));
    int primaryMonitorX = GetSystemMetrics(SM_CXSCREEN);
    int primaryMonitorY = GetSystemMetrics(SM_CYSCREEN);

    SetWindowPos(m_hWnd, HWND_TOPMOST, primaryMonitorX - WND_WIDTH, primaryMonitorY - WND_HEIGHT, static_cast<int>(WND_WIDTH * dpi / 96.0f), static_cast<int>(WND_HEIGHT * dpi / 96.0f), 0);
    ShowWindow(m_hWnd, SW_SHOWNORMAL);
    //hr = Render();

    return hr;
}

HRESULT PathWindow::RunMessageLoop()
{
    MSG msg{};
    BOOL bRet;
    while ((bRet = GetMessage(&msg, NULL, 0, 0)) != 0)
    {
        if (bRet == -1)
        {
            return E_FAIL;
        }

        //TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return S_OK;
}

HRESULT PathWindow::AddPoint(POINT point, bool render)
{
    m_points.push_back(D2D1::Point2F(static_cast<float>(point.x), static_cast<float>(point.y)));

    if (render) return Render();
    return S_OK;
}

HRESULT PathWindow::AddPoints(POINT* points, int length)
{
    if (length < 1) return E_INVALIDARG;
    if (!points) return E_INVALIDARG;

    for (int i = 0; i < length; ++i)
    {
        POINT point = points[i];
        m_points.push_back(D2D1::Point2F(static_cast<float>(point.x), static_cast<float>(point.y)));
    }

    return Render();
}

HRESULT PathWindow::ClearPoints()
{
    m_points.clear();

    return Render();
}

HRESULT PathWindow::CreateDeviceIndependentResources()
{
    HRESULT hr = S_OK;

    HR(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &m_pD2Factory));
    HR(m_pD2Factory->CreateStrokeStyle(D2D1::StrokeStyleProperties(D2D1_CAP_STYLE_FLAT, D2D1_CAP_STYLE_FLAT, D2D1_CAP_STYLE_FLAT, D2D1_LINE_JOIN_ROUND), nullptr, 0, &m_pStrokeStyle));

    return hr;
}

HRESULT PathWindow::CreateDeviceResources()
{
    HRESULT hr = S_OK;

    if (m_pRenderTarget) return hr;

    ComPtr<ID3D11Device> pD3Device;
    HR(D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        D3D11_CREATE_DEVICE_BGRA_SUPPORT,
        nullptr, 0, // Highest available feature level
        D3D11_SDK_VERSION,
        &pD3Device,
        nullptr,
        nullptr
    ));

    D3D11_TEXTURE2D_DESC d3TextureDesc{};
    d3TextureDesc.ArraySize = 1;
    d3TextureDesc.BindFlags = D3D11_BIND_RENDER_TARGET;
    d3TextureDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    d3TextureDesc.MipLevels = 1;
    d3TextureDesc.SampleDesc.Count = 1;
    d3TextureDesc.MiscFlags = D3D11_RESOURCE_MISC_GDI_COMPATIBLE;

    RECT rc{};
    GetClientRect(m_hWnd, &rc);
    d3TextureDesc.Width = rc.right - rc.left;
    d3TextureDesc.Height = rc.bottom - rc.top;

    ComPtr<ID3D11Texture2D> pTexture;
    HR(pD3Device->CreateTexture2D(&d3TextureDesc, nullptr, pTexture.GetAddressOf()));

    ComPtr<IDXGISurface> pSurface;
    HR(pTexture.As(&pSurface));

    D2D1_RENDER_TARGET_PROPERTIES renderTargetProps{};
    renderTargetProps.type = D2D1_RENDER_TARGET_TYPE_DEFAULT;
    renderTargetProps.pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED);
    renderTargetProps.dpiX = 0.0f;
    renderTargetProps.dpiY = 0.0f;
    renderTargetProps.usage = D2D1_RENDER_TARGET_USAGE_GDI_COMPATIBLE;
    renderTargetProps.minLevel = D2D1_FEATURE_LEVEL_DEFAULT;

    HR(m_pD2Factory->CreateDxgiSurfaceRenderTarget(pSurface.Get(), renderTargetProps, &m_pRenderTarget));

    HR(m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 0.7f), &m_pPathBrush));

    m_pRenderTarget->SetAntialiasMode(D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);

    hr = m_pRenderTarget->QueryInterface(&m_pInteropTarget);

    return hr;
}

void PathWindow::DiscardDeviceResources()
{
    SafeRelease(&m_pRenderTarget);
    SafeRelease(&m_pInteropTarget);
    SafeRelease(&m_pPathBrush);
}

HRESULT PathWindow::Render()
{
    HRESULT hr = S_OK;

    HR(CreateDeviceResources());

    m_pRenderTarget->BeginDraw();
    m_pRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());
    m_pRenderTarget->Clear(D2D1::ColorF(0.0f, 0.0f, 0.0f, 0.0f));

    //D2D1_SIZE_F rtSize = m_pRenderTarget->GetSize();

    if (m_points.size() > 1)
    {
        ComPtr<ID2D1PathGeometry> pGeometry;
        HR(m_pD2Factory->CreatePathGeometry(pGeometry.GetAddressOf()));

        ComPtr<ID2D1GeometrySink> pSink;
        HR(pGeometry->Open(pSink.GetAddressOf()));

        pSink->BeginFigure(D2D1::Point2F(static_cast<float>(m_points[0].x), static_cast<float>(m_points[0].y)), D2D1_FIGURE_BEGIN_HOLLOW);

        pSink->AddLines(&m_points[1], m_points.size() - 1);

        pSink->EndFigure(D2D1_FIGURE_END_OPEN);

        HR(pSink->Close());

        m_pRenderTarget->DrawGeometry(pGeometry.Get(), m_pPathBrush, 3.0f, m_pStrokeStyle);

        //for (int i = 1; i < m_points.size(); ++i)
        //{
        //    m_pRenderTarget->DrawLine(m_points[i - 1], m_points[i], m_pPathBrush, 3.0f, m_pStrokeStyle);
        //}
    }

    {
        HDC dc;
        HR(m_pInteropTarget->GetDC(D2D1_DC_INITIALIZE_MODE_COPY, &dc));

        hr = m_info.Update(m_hWnd, dc);

        RECT r{};
        m_pInteropTarget->ReleaseDC(&r);

        if (FAILED(hr)) return hr;
    }

    hr = m_pRenderTarget->EndDraw();
    if (hr == D2DERR_RECREATE_TARGET)
    {
        DiscardDeviceResources();
        hr = S_OK;
    }

    return hr;
}

LRESULT CALLBACK PathWindow::WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    if (message == WM_CREATE)
    {
        auto pcs = (LPCREATESTRUCT)lParam;
        auto pPathWindow = (PathWindow*)pcs->lpCreateParams;

        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pPathWindow));

        return 1;
    }

    auto pPathWindow = reinterpret_cast<PathWindow*>(static_cast<LONG_PTR>(GetWindowLongPtr(hWnd, GWLP_USERDATA)));

    if (pPathWindow)
    {
        switch (message)
        {
        case WM_SIZE:
            return 0;

        case WM_DISPLAYCHANGE:
            PostQuitMessage(1);
            return 0;

        case WM_PAINT:
            return 0;

        case WM_CLOSE:
            DestroyWindow(pPathWindow->m_hWnd);
            return 0;

        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
        }
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}

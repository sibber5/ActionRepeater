#include "pch.h"
#include "DrawablePathWindow.h"
#include <Windowsx.h>
#include <string>
#include <functional>

#ifdef DEBUG
#define to_wcstr(s) (std::to_wstring(s).c_str())
#endif

using namespace std::placeholders;
using namespace PathWindows;

DrawablePathWindow::DrawablePathWindow(WindowClosingCallback windowClosingCallback) :
	m_pathWindow(std::bind(&DrawablePathWindow::HandleUnhandledMsg, this, _1, _2, _3, _4), true),
	m_windowClosingCallback(windowClosingCallback)
{}

DrawablePathWindow::DrawablePathWindow(MouseMovement* movs, int length, WindowClosingCallback windowClosingCallback) : DrawablePathWindow(windowClosingCallback)
{
	for (int i = 0; i < length; ++i)
	{
		MouseMovement mov = movs[i];
		m_path.push_back(mov);
		m_pathWindow.AddPoint(mov.Delta, false, mov.DelayDurationNS == 0);
	}
}

HWND DrawablePathWindow::GetHandle()
{
	return m_pathWindow.GetHandle();
}

HRESULT DrawablePathWindow::Initialize()
{
	return m_pathWindow.Initialize();
}

HRESULT DrawablePathWindow::RunMessageLoop()
{
	return m_pathWindow.RunMessageLoop();
}

std::vector<MouseMovement> DrawablePathWindow::GetPath()
{
	return m_path;
}

void DrawablePathWindow::ClearPath()
{
	m_path.clear();
	m_pathWindow.ClearPoints();
}

void DrawablePathWindow::AddPoint(POINT pos, bool newPath)
{
	// mouse movement delay duration will be set (multiplied by a factor that is set by the user (which is basically the cursor speed)) in the app after the window closes
	m_path.push_back(MouseMovement{ pos, newPath ? 0 : 1 });
	m_pathWindow.AddPoint(pos, !newPath, newPath);
}

inline void DrawablePathWindow::Close()
{
	auto path = GetPath();
	auto size = path.size();
	m_windowClosingCallback(size == 0 ? nullptr : &path[0], static_cast<int>(size));
	auto ret = PostMessage(m_pathWindow.GetHandle(), WM_CLOSE, 0, 0);
}

void DrawablePathWindow::HandleUnhandledMsg(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static bool newPath = true;
	POINT pos{};

	switch (message)
	{
	case WM_LBUTTONDOWN:
		pos.x = GET_X_LPARAM(lParam);
		pos.y = GET_Y_LPARAM(lParam);

		newPath = (wParam & MK_SHIFT) == 0;
		AddPoint(pos, newPath);
		break;

	case WM_LBUTTONUP:
		newPath = true;
		break;

	case WM_MOUSEMOVE:
		if ((wParam & MK_LBUTTON) == 0) break;
		if (wParam & MK_SHIFT) break;

		pos.x = GET_X_LPARAM(lParam);
		pos.y = GET_Y_LPARAM(lParam);

		AddPoint(pos, newPath);
		newPath = false;
		break;

	case WM_RBUTTONDOWN:
		ClearPath();
		break;

	case WM_KEYUP:
		switch (wParam)
		{
		case VK_ESCAPE:
			Close();
			break;
		}
		break;
	}
}

#pragma once
#include "pch.h"
#include "PathWindow.h"
#include <vector>

struct MouseMovement
{
	POINT Delta;
	int64_t DelayDurationNS;
};

typedef void(__cdecl *WindowClosingCallback)(MouseMovement*, int);

namespace PathWindows
{
	class DrawablePathWindow : public IWindow
	{
	public:
		DrawablePathWindow(WindowClosingCallback windowClosingCallback);
		DrawablePathWindow(MouseMovement* movs, int length, WindowClosingCallback windowClosingCallback);

		HWND GetHandle();

		HRESULT Initialize();

		HRESULT RunMessageLoop();

		std::vector<MouseMovement> GetPath();
		void ClearPath();

	private:
		PathWindow m_pathWindow;

		std::vector<MouseMovement> m_path;

		WindowClosingCallback m_windowClosingCallback;

		void AddPoint(POINT pos, bool newPath);

		void Close();

		void HandleUnhandledMsg(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
	};
}
#pragma once
#include "pch.h"
#include "IWindow.h"
#include <thread>
#include <condition_variable>

namespace PathWindows
{
	class WindowHost
	{
	public:
		WindowHost(IWindow* pPathWindow);
		~WindowHost();

		IWindow* GetPWindow();
		HRESULT GetThreadHR();

		HRESULT CloseWindow();

	private:
		std::thread m_thread;
		bool m_isRunning;

		std::condition_variable m_wndInitCondVar;
		std::mutex m_wndInitMutex;

		IWindow* m_pWindow;

		HRESULT m_threadHR;

		void CreateAndShowWindow();
	};
}

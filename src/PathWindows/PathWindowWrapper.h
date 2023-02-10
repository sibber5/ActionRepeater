#pragma once
#include "pch.h"
#include "PathWindow.h"
#include <thread>
#include <condition_variable>

namespace PathWindows
{
	class PathWindowWrapper
	{
	public:
		PathWindowWrapper();
		~PathWindowWrapper();

		void OpenPathWindow();
		HRESULT ClosePathWindow();

		PathWindow* m_pPathWindow;

		HRESULT m_threadHR;

	private:
		std::thread m_thread;
		bool m_isRunning;

		std::condition_variable m_wndInitCondVar;
		std::mutex m_wndInitMutex;
	};
}

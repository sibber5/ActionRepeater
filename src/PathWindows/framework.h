#pragma once

// Exclude rarely-used stuff from Windows headers
#define WIN32_LEAN_AND_MEAN
// Windows Header Files
#include <windows.h>

#include <wrl/client.h>

#include <d2d1.h>
#include <d2d1helper.h>
#include <wincodec.h>
#pragma comment(lib, "d2d1")

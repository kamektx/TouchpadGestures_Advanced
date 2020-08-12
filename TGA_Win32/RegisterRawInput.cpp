#include "pch.h"
DLLEXPORT int __stdcall RegisterRawInput(HWND hwnd);

DLLEXPORT int __stdcall RegisterRawInput(HWND hwnd) {
    RAWINPUTDEVICE Rid[1];
    Rid[0].usUsagePage = 0x0D;
    Rid[0].usUsage = 0x05;
    Rid[0].dwFlags = RIDEV_INPUTSINK;
    Rid[0].hwndTarget = hwnd;

    if (RegisterRawInputDevices(Rid, 1, sizeof(Rid[0])) == FALSE) {
        return -1;
    }
    return 0;
}
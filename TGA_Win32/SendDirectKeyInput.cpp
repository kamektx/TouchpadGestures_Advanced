#include "pch.h"

using namespace std;
DLLEXPORT void __stdcall SendDirectKeyInput(char command, int* keys1, int keys1length, int* keys2 = nullptr, int keys2length = 0) {
    switch (command)
    {
    case 'd': {
        PINPUT input = (PINPUT)calloc(keys1length, sizeof(INPUT));
        for (int i = 0; i < keys1length; i++)
        {
            input[i].type = INPUT_KEYBOARD;
            input[i].ki.wVk = 0;
            input[i].ki.wScan = keys1[i];
            input[i].ki.dwFlags = KEYEVENTF_SCANCODE;
            input[i].ki.dwExtraInfo = 0;
            input[i].ki.time = 0;
        }
        SendInput(keys1length, input, sizeof(input[0]));
        free(input);
        break;
    }
    case 'u': {
        PINPUT input = (PINPUT)calloc(keys1length, sizeof(INPUT));
        for (int i = 0; i < keys1length; i++)
        {
            input[i].type = INPUT_KEYBOARD;
            input[i].ki.wVk = 0;
            input[i].ki.wScan = keys1[i];
            input[i].ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            input[i].ki.dwExtraInfo = 0;
            input[i].ki.time = 0;
        }
        SendInput(keys1length, input, sizeof(input[0]));
        free(input);
        break;
    }
    case 'p': {
        PINPUT input = (PINPUT)calloc(keys1length * 2, sizeof(INPUT));
        for (int i = 0; i < keys1length; i++)
        {
            input[i].type = INPUT_KEYBOARD;
            input[i].ki.wVk = 0;
            input[i].ki.wScan = keys1[i];
            input[i].ki.dwFlags = KEYEVENTF_SCANCODE;
            input[i].ki.dwExtraInfo = 0;
            input[i].ki.time = 0;
            input[keys1length * 2 - i - 1].type = INPUT_KEYBOARD;
            input[keys1length * 2 - i - 1].ki.wVk = 0;
            input[keys1length * 2 - i - 1].ki.wScan = keys1[i];
            input[keys1length * 2 - i - 1].ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            input[keys1length * 2 - i - 1].ki.dwExtraInfo = 0;
            input[keys1length * 2 - i - 1].ki.time = 0;
        }
        SendInput(keys1length * 2, input, sizeof(input[0]));
        free(input);
        break;
    }
    case 'w': {
        PINPUT input = (PINPUT)calloc(keys1length, sizeof(INPUT));
        for (int i = 0; i < keys1length; i++)
        {
            input[i].type = INPUT_KEYBOARD;
            input[i].ki.wVk = 0;
            input[i].ki.wScan = keys1[i];
            input[i].ki.dwFlags = KEYEVENTF_SCANCODE;
            input[i].ki.dwExtraInfo = 0;
            input[i].ki.time = 0;
        }
        SendInput(keys1length, input, sizeof(input[0]));
        Sleep(1000);
        for (int i = 0; i < keys1length; i++)
        {
            input[i].type = INPUT_KEYBOARD;
            input[i].ki.wVk = 0;
            input[i].ki.wScan = keys1[i];
            input[i].ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            input[i].ki.dwExtraInfo = 0;
            input[i].ki.time = 0;
        }
        SendInput(keys1length, input, sizeof(input[0]));
        free(input);
        break;
    }
    default:
        break;
    }
    return;
}
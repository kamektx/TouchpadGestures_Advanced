using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace TouchpadGestures_Advanced
{
    static class ForegroundWindowWatcher
    {
        public static void SubscribeToWindowEvents()
        {
            if (windowEventHook == IntPtr.Zero)
            {
                windowEventHook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND, // eventMin
                    EVENT_SYSTEM_FOREGROUND, // eventMax
                    IntPtr.Zero,             // hmodWinEventProc
                    myCallBack,     // lpfnWinEventProc
                    0,                       // idProcess
                    0,                       // idThread
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                if (windowEventHook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        private static readonly WinEventProc myCallBack = WindowEventCallback;

        private static void WindowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            var applicationName = new StringBuilder(260);
            uint dwProcId;

            GetWindowThreadProcessId(hwnd, out dwProcId);
            IntPtr hProc = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead, false, dwProcId);
            GetModuleFileNameEx(hProc, IntPtr.Zero, applicationName, 260);
            CloseHandle(hProc);
            Debug.WriteLine(applicationName.ToString());
        }

        private static IntPtr windowEventHook;

        private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern unsafe IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern unsafe int UnhookWinEvent(IntPtr hWinEventHook);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern unsafe uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            [Out] uint processId
        );
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] uint nSize);
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;

        private const int EVENT_SYSTEM_FOREGROUND = 3;
    }
}

#region Using directives

using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Security.RightsManagement;
using System.Text;

#endregion

namespace TouchpadGestures_Advanced
{
    internal class ControlHost : Window
    {
        private static bool isFirstTime = true;
        internal const int
            WM_INPUT = 0xff;

        internal static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            switch (msg)
            {
                case WM_INPUT:
                    if (isFirstTime)
                    {
                        uint length2 = 1000;
                        var rjson2 = new StringBuilder((int)length2);
                        NativeMethods.HidInit(wParam, lParam, rjson2, length2);
                        Interpreter.InitJson(rjson2.ToString());
                        isFirstTime = false;
                    }
                    uint length = 5000;
                    var rjson = new StringBuilder((int)length);
                    NativeMethods.HidManager(wParam, lParam, rjson, length);
                    Interpreter.SetJson(rjson.ToString());
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
    }
}

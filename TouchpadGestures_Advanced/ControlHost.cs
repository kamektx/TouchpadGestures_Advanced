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
            switch (msg)
            {
                case WM_INPUT:
                    uint length = 5000;
                    var rjson = new StringBuilder((int)length);
                    if (isFirstTime)
                    {
                        //init
                        isFirstTime = false;
                    }
                    NativeMethods.HidManager(wParam, lParam, rjson, length);
                    Interpreter.SetJson(rjson.ToString());
                    break;
            }
            return IntPtr.Zero;
        }
    }
}

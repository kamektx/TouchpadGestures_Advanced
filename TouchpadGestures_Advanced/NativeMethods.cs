using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TouchpadGestures_Advanced
{
    internal class NativeMethods
    {
        [DllImport("TGA_Win32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe int HidManager(IntPtr wParam, IntPtr lParam, [MarshalAs(UnmanagedType.LPUTF8Str), Out] StringBuilder rjson, uint length);

        [DllImport("TGA_Win32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe int RegisterRawInput(IntPtr hwnd);
        [DllImport("TGA_Win32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe void SendDirectKeyInput(char command, [In] IntPtr keys1, int keys1length, [In] IntPtr keys2, int keys2length);       
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TouchpadGestures_Advanced
{
    internal static class Settings
    {
        internal enum Mode
        {
            DispatchKeyboardShortcat
        }
        internal static Dictionary<string, Mode> ApplicationSettings = new Dictionary<string, Mode>()
        {
            {"default", Mode.DispatchKeyboardShortcat}
        };
        internal static string ForegroundApplication { get; set; }
        internal static int Threshold = 60;
        internal static int ThresholdActive = 150;
        internal static int IgnoreTime = 180;
        internal static double IgnoreMagnification = 0.1;
        internal static double ThresholdAngle = 45d;
    }
}

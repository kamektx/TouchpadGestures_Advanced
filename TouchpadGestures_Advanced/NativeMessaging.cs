using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TouchpadGestures_Advanced
{
    public class NativeMessaging
    {
        internal static async void KilledNMC_Detector()
        {
            while (true)
            {
                Process[] NMCs = Process.GetProcessesByName("TGA_NativeMessaging_Cliant");
                Debug.WriteLine("detect: " + NMCs.Length);
                await Task.Delay(5000);
            }
        }
    }
}

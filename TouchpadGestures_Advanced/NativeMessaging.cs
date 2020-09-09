using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace TouchpadGestures_Advanced
{
    public class NativeMessaging
    {
        internal static async void KilledNMC_Detector()
        {
            while (true)
            {
                await Task.Delay(5000);
                int NMC_Running = (int)App.Registry_TGA_NMC.GetValue("NMC_Running");
                for (int i = 0; i < App.NMC_RunningMax; i++)
                {
                    if (((1<<i) & NMC_Running) != 0)
                    {
                        int PID = (int)App.Registry_TGA_NMC.GetValue("NMC" + i + "_PID");
                        bool isRunning = true;
                        try
                        {
                            Process _NMC = Process.GetProcessById(PID);
                            if (_NMC.HasExited || _NMC.ProcessName != "TGA_NativeMessaging_Cliant")
                            {
                                isRunning = false;
                            }
                        }
                        catch (Exception)
                        {
                            isRunning = false;
                        }
                        if (!isRunning)
                        {
                            NMC_Running -= (1 << i);
                            App.Registry_TGA_NMC.SetValue("NMC" + i + "_Key", "", RegistryValueKind.String);
                            App.Registry_TGA_NMC.SetValue("NMC" + i + "_PID", 0, RegistryValueKind.DWord);
                        }
                    }
                }
                App.Registry_TGA_NMC.SetValue("NMC_Running", NMC_Running >= 0 ? NMC_Running : 0, RegistryValueKind.DWord);
                //Process[] NMCs = Process.GetProcessesByName("TGA_NativeMessaging_Cliant");
                //Debug.WriteLine("detect: " + NMCs.Length);
            }
        }
    }
}

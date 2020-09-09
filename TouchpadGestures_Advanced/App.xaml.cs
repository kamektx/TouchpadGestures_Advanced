﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string MutexName = "TouchpadGestures_Advanced_Running";
        private static readonly Mutex Mutex = new Mutex(false, MutexName);
        private static bool MutexHasHandle = false;
        public static RegistryKey Registry_TGA;
        public static RegistryKey Registry_TGA_NMC;
        public static List<string> Registry_TGA_NMC_Values = new List<string>();
        public static int NMC_RunningMax = 31;
        public static Dispatcher DispatcherNow = new Dispatcher("default");

        private static void Registry_TGA_NMC_Values_Init()
        {
            for (int i = 0; i < NMC_RunningMax; i++)
            {
                Registry_TGA_NMC_Values.Add("NMC" + i + "_Key");
                Registry_TGA_NMC_Values.Add("NMC" + i + "_PID");
            }
            Registry_TGA_NMC_Values.Add("NMC_Running");
            Registry_TGA_NMC_Values.Add("Return_Key");
            Registry_TGA_NMC_Values.Add("Return_Command");
            Registry_TGA_NMC_Values.Add("Return_Value1");
            Registry_TGA_NMC_Values.Add("Return_Value2");
            Registry_TGA_NMC_Values.Add("NMC_RunningMax");
            Registry_TGA_NMC_Values.Add("NMC_ChangedKey");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                MutexHasHandle = Mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                MutexHasHandle = true;
            }
            try
            {
                var TGA_init = EventWaitHandle.OpenExisting("TouchpadGestures_Advanced_Init");
                TGA_init.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {

            }
            if (!MutexHasHandle)
            {
                this.Shutdown(0);
                return;
            }

            Registry_TGA = Registry.CurrentUser.CreateSubKey("SOFTWARE\\TouchpadGestures_Advanced", true);
            Registry_TGA_NMC = Registry.CurrentUser.CreateSubKey("SOFTWARE\\TouchpadGestures_Advanced\\NativeMessaging_Cliant", true);
            Registry_TGA_NMC_Values_Init();
            foreach (var item in Registry_TGA_NMC_Values)
            {
                if (!Registry_TGA_NMC.GetValueNames().Contains(item))
                {
                    if (item.Contains("NMC_RunningMax"))
                    {
                        Registry_TGA_NMC.SetValue(item, NMC_RunningMax, RegistryValueKind.DWord);
                    }
                    else if (item.Contains("PID") || item.Contains("Running"))
                    {
                        Registry_TGA_NMC.SetValue(item, 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        Registry_TGA_NMC.SetValue(item, "", RegistryValueKind.String);
                    }
                }
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MutexHasHandle)
            {
                Mutex.ReleaseMutex();
            }
            Mutex.Close();
            Registry_TGA.Close();
            Registry_TGA_NMC.Close();

            base.OnExit(e);
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            var _MainWindow = new MainWindow();
            _MainWindow.Show();
        }
    }
}

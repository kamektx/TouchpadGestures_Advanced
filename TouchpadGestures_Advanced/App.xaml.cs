﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

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
        public static string TGA_AppData;
        public static string NMC_AppData;
        public static List<string> Registry_TGA_NMC_Values = new List<string>();
        public static int NMC_RunningMax = 31;
        public static MyDispatcher DispatcherNow;
        public static SemaphoreSlim DispatcherSemaphore = new SemaphoreSlim(1, 1);
        public static MyNotifyIcon myNotifyIcon;
        public static Settings Settings;
        public static MainWindow MainWindow01;
        public static bool IsRestart = false;
        public static string MyPath = Process.GetCurrentProcess().MainModule.FileName;

        private static void Registry_TGA_NMC_Values_Init()
        {
            for (int i = 0; i < NMC_RunningMax; i++)
            {
                Registry_TGA_NMC_Values.Add("NMC" + i + "_Key");
                Registry_TGA_NMC_Values.Add("NMC" + i + "_PID");
            }
            Registry_TGA_NMC_Values.Add("NMC_Running");
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

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            TGA_AppData = appData + @"\TouchpadGestures_Advanced";
            Directory.CreateDirectory(TGA_AppData + @"\SettingsForActiveApp");
            Directory.CreateDirectory(TGA_AppData + @"\Settings");
            NMC_AppData = TGA_AppData + @"\NMC";

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

            App.Settings = Settings.LoadSettings("default");
            
            DispatcherNow = new MyDispatcher("default");

            myNotifyIcon = new MyNotifyIcon();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MutexHasHandle)
            {
                Mutex.ReleaseMutex();
            }
            Mutex.Close();

            myNotifyIcon?.Dispose();

            if (IsRestart)
            {
                var psi = new ProcessStartInfo(MyPath);
                System.Diagnostics.Process.Start(psi);
            }

            base.OnExit(e);
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            MainWindow01 = new MainWindow();
            MainWindow01.Show();
        }
    }
}

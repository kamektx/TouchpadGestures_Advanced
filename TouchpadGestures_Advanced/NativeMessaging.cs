﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Threading;

namespace TouchpadGestures_Advanced
{
    public static class NativeMessaging
    {
        public static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        public static string DeserializingNMC_Key = "";
        public static NMC_Manager ActiveNMC = null;
        private static Thread TimerThread;

        public static Dictionary<string, NMC_Manager> NMCs = new Dictionary<string, NMC_Manager>(); // Key, NMC_Manager

        internal static void DeleteNMC_Directories()
        {
            var directories = Directory.GetDirectories(App.NMC_AppData);
            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                if (dirInfo.Name.Length == 26 && !NMCs.ContainsKey(dirInfo.Name))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }
        internal static void DeleteNMC(NMC_Manager nmc)
        {
            if (nmc.IsRunning == false)
            {
                nmc.Watcher?.Dispose();
                int NMC_Running = (int)App.Registry_TGA_NMC.GetValue("NMC_Running");
                if (((1 << nmc.ID) & NMC_Running) != 0)
                {
                    NMC_Running -= (1 << nmc.ID);
                    App.Registry_TGA_NMC.SetValue("NMC_Running", NMC_Running >= 0 ? NMC_Running : 0, RegistryValueKind.DWord);
                }
                App.Registry_TGA_NMC.SetValue("NMC" + nmc.ID + "_Key", "", RegistryValueKind.String);
                App.Registry_TGA_NMC.SetValue("NMC" + nmc.ID + "_PID", 0, RegistryValueKind.DWord);
                _ = Task.Run(() =>
                {
                    switch (nmc.WindowState)
                    {
                        case 0:
                            break;
                        case 1:
                            nmc.IsForBrowserUp.WaitOne();
                            goto case 10;
                        case 10:
                            nmc.ForBrowserWindow.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                            break;
                        default:
                            break;
                    }
                    Semaphore.Wait();
                    if (ActiveNMC == nmc)
                    {
                        ActiveNMC = null;
                    }
                    NMCs.Remove(nmc.Key);
                    Semaphore.Release();
                });
            }
        }

        public static void ScanNMC()
        {
            int NMC_Running = (int)App.Registry_TGA_NMC.GetValue("NMC_Running");
            for (int i = 0; i < App.NMC_RunningMax; i++)
            {
                if (((1 << i) & NMC_Running) != 0)
                {
                    Semaphore.Wait();
                    string key = (string)App.Registry_TGA_NMC.GetValue("NMC" + i + "_Key");
                    if (NMCs.ContainsKey(key) == false)
                    {
                        NMCs.Add(key, new NMC_Manager(key, i));
                    }
                    else
                    {
                        NMCs[key].DeleteOldScreenShot();
                        NMCs[key].CheckRunningWithoutWaiting();
                    }
                    Semaphore.Release();
                }
            }
        }
        public static void Timer()
        {
            TimerThread = new Thread(() =>
            {
                int restartCounter = 0;
                int restartMinutes = 20;
                int scanMinutes = 2;
                while (true)
                {
                    if (restartCounter >= restartMinutes)
                    {
                        bool result = App.DispatcherSemaphore.Wait(0);
                        if (result)
                        {
                            App.IsRestart = true;
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.Application.Current.Shutdown();
                            });
                            break;
                        }
                    }
                    ScanNMC();
                    DeleteNMC_Directories();
                    Thread.Sleep(60 * scanMinutes * 1000);
                    restartCounter += scanMinutes;
                }
            });
            TimerThread.IsBackground = true;
            TimerThread.Start();

        }
    }
}

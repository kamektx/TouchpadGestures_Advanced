using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Net.Http.Headers;
using Newtonsoft;
using Newtonsoft.Json;

#pragma warning disable CS4014

namespace TouchpadGestures_Advanced
{
    public class NMC_Manager
    {
        private static readonly int TimeToRead = 29;
        private static readonly int TickTime = 10;
        private int TimerReadMilliSeconds;
        private bool IsTimerRunning;

        public readonly object SyncObj;
        public FileSystemWatcher Watcher;
        public SendingObject SendingObject { get; private set; }
        public int ID { get; private set; }
        public string Key { get; private set; }
        public string MyAppData { get; private set; }
        public int PID { get; private set; }
        public bool IsRunning { get; set; }
        public bool IsActive
        {
            get
            {
                lock (SyncObj)
                {
                    if (SendingObject == null)
                    {
                        return false;
                    }
                    return SendingObject.ActiveWindowID.HasValue;
                }
            }
        }
        public bool AssertRunning()
        {
            lock (SyncObj)
            {
                IsRunning = true;
                try
                {
                    Process _NMC = Process.GetProcessById(PID);
                    if (_NMC.HasExited || _NMC.ProcessName != "TGA_NativeMessaging_Cliant")
                    {
                        IsRunning = false;
                    }
                }
                catch (Exception)
                {
                    IsRunning = false;
                }
                if (IsRunning == false)
                {
                    NativeMessaging.DeleteNMC(this);
                }
                return IsRunning;
            }
        }
        public void ReadJSON()
        {
            string sendingObjectJSON = null;
            int count = 0;
            do
            {
                try
                {
                    sendingObjectJSON = File.ReadAllText(MyAppData + @"\sending_object.json", Encoding.UTF8);
                }
                catch (IOException)
                {
                    count++;
                    if (count > 10)
                    {
                        throw;
                    }
                    Thread.Sleep(20);
                    continue;
                }
            } while (false);
            if (sendingObjectJSON != null)
            {
                lock (SyncObj)
                {
                    SendingObject = JsonConvert.DeserializeObject<SendingObject>(sendingObjectJSON);
                }
            }
        }
        public async Task StartDirectoryWatch()
        {
            int count = 0;
            while (!Directory.Exists(MyAppData) || !File.Exists(MyAppData + @"\sending_object.json"))
            {
                await Task.Delay(20);
                count++;
                if (count > 500)
                {
                    throw new Exception();
                }
            }
            ReadJSON();
            Watcher.Path = MyAppData;
            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Filter = "sending_object.json";
            Watcher.Changed += OnSendingObjectChanged;
            Watcher.EnableRaisingEvents = true;
        }

        public void OnSendingObjectChanged(object source, FileSystemEventArgs e)
        {
            TimerReadMilliSeconds = TimeToRead;
            if (!IsTimerRunning)
            {
                IsTimerRunning = true;
                ReadTimer();
            }
        }
        public async Task ReadTimer()
        {
            while (TimerReadMilliSeconds > 0)
            {
                await Task.Delay(TickTime);
                TimerReadMilliSeconds -= TickTime;
            }
            IsTimerRunning = false;
            ReadJSON();
        }
        public NMC_Manager(string key, int id)
        {
            Watcher = new FileSystemWatcher();
            SyncObj = new object();
            Key = key;
            ID = id;
            PID = (int)App.Registry_TGA_NMC.GetValue("NMC" + ID + "_PID");
            MyAppData = App.NMC_AppData + @"\" + Key;
            IsRunning = true;
            AssertRunning();
            if (IsRunning == true)
            {
                StartDirectoryWatch();
            }
        }
    }



    public class NativeMessaging
    {
        public static object SyncObj = new object();

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
                    catch (Exception)
                    {

                    }
                }
            }
        }
        internal static void DeleteNMC(NMC_Manager nmc)
        {
            if (nmc.IsRunning == false)
            {
                nmc.Watcher.Dispose();
                int NMC_Running = (int)App.Registry_TGA_NMC.GetValue("NMC_Running");
                if (((1 << nmc.ID) & NMC_Running) != 0)
                {
                    NMC_Running -= (1 << nmc.ID);
                    App.Registry_TGA_NMC.SetValue("NMC_Running", NMC_Running >= 0 ? NMC_Running : 0, RegistryValueKind.DWord);
                }
                App.Registry_TGA_NMC.SetValue("NMC" + nmc.ID + "_Key", "", RegistryValueKind.String);
                App.Registry_TGA_NMC.SetValue("NMC" + nmc.ID + "_PID", 0, RegistryValueKind.DWord);
                lock (SyncObj)
                {
                    NMCs.Remove(nmc.Key);
                }
            }
        }

        public static void ScanNMC()
        {
            int NMC_Running = (int)App.Registry_TGA_NMC.GetValue("NMC_Running");
            for (int i = 0; i < App.NMC_RunningMax; i++)
            {
                if (((1 << i) & NMC_Running) != 0)
                {
                    string key = (string)App.Registry_TGA_NMC.GetValue("NMC" + i + "_Key");
                    if (NMCs.ContainsKey(key) == false)
                    {
                        lock (SyncObj)
                        {
                            NMCs.Add(key, new NMC_Manager(key, i));
                        }
                    }
                    else
                    {
                        NMCs[key].AssertRunning();
                    }
                }
            }
        }
        public static async void Timer()
        {
            while (true)
            {
                ScanNMC();
                DeleteNMC_Directories();
                await Task.Delay(10 * 1000);
                //Process[] NMCs = Process.GetProcessesByName("TGA_NativeMessaging_Cliant");
                //Debug.WriteLine("detect: " + NMCs.Length);
            }
        }
    }
}

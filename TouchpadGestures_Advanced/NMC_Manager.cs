using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Threading;

#pragma warning disable CS4014

namespace TouchpadGestures_Advanced
{
    public class NMC_Manager
    {
        private static readonly int TimeToRead = 15;
        private static readonly int TickTime = 5;
        private int TimerReadMilliSeconds;
        private bool IsTimerRunning;
        public ForBrowser ForBrowserWindow;
        public Thread WindowThread;
        public int WindowState;
        public HashSet<string> UsingScreenShot;

        public SemaphoreSlim MySemaphore;
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
                MySemaphore.Wait();
                if (SendingObject == null)
                {
                    return false;
                }
                bool temp = SendingObject.ActiveWindowID.HasValue;
                MySemaphore.Release();
                return temp;
            }
        }
        public bool AssertRunning()
        {
            MySemaphore.Wait();
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
            MySemaphore.Release();
            return IsRunning;
        }
        public void ReadJSON()
        {
            MySemaphore.Wait();
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
                    if (count > 100)
                    {
                        throw;
                    }
                    Thread.Sleep(5);
                    continue;
                }
            } while (false);
            if (sendingObjectJSON != null && sendingObjectJSON[0] != '未')
            {
                lock (NativeMessaging.SyncObj)
                {
                    UsingScreenShot.Clear();
                    NativeMessaging.DeserializingNMC_Key = Key;
                    SendingObject = JsonConvert.DeserializeObject<SendingObject>(sendingObjectJSON);
                }
                while (ForBrowserWindow == null)
                {
                    Thread.Sleep(5);
                    Debug.WriteLine("ForBrowserWindow == null");
                }
                ForBrowserWindow.Dispatcher.BeginInvoke(ForBrowserWindow.Refresh);

            }
            MySemaphore.Release();
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
            UsingScreenShot = new HashSet<string>();
            WindowState = 0;
            Watcher = new FileSystemWatcher();
            MySemaphore = new SemaphoreSlim(1, 1);
            Key = key;
            ID = id;
            PID = (int)App.Registry_TGA_NMC.GetValue("NMC" + ID + "_PID");
            MyAppData = App.NMC_AppData + @"\" + Key;
            IsRunning = true;
            AssertRunning();
            if (IsRunning == true)
            {
                WindowThread = new Thread(() =>
                {
                    WindowState = 1;
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                    ForBrowserWindow = new ForBrowser(this);
                    WindowState = 10;
                    ForBrowserWindow.Closed += (s, e) =>
           Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    ForBrowserWindow.Show();
                    Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.IsBackground = true;
                WindowThread.Start();
                StartDirectoryWatch();
            }
        }
    }
}

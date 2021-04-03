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

    public class CommandToSend
    {
        public string Command { get; set; }
        public int WindowID { get; set; }
        public int TabID { get; set; }
        public int PageID { get; set; }
    }
    public class NMC_Manager
    {
        private static readonly int FileModifiedIgnoreTime = 30;
        private DateTime beforeTime;
        public EventWaitHandle IsForBrowserUp;
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
                if (SendingObject == null)
                {
                    return false;
                }
                bool temp = SendingObject.ActiveWindowID.HasValue;
                return temp;
            }
        }
        public async Task<bool> AssertRunningAsync()
        {
            await MySemaphore.WaitAsync();
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
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    Thread.Sleep(5);
                    sendingObjectJSON = File.ReadAllText(MyAppData + @"\sending_object.json", Encoding.UTF8);
                    break;
                }
                catch (IOException)
                {
                    continue;
                }
            }
            if (sendingObjectJSON != null && sendingObjectJSON[0] != '@')
            {
                lock (NativeMessaging.SyncObj)
                {
                    UsingScreenShot.Clear();
                    NativeMessaging.DeserializingNMC_Key = Key;
                    SendingObject = JsonConvert.DeserializeObject<SendingObject>(sendingObjectJSON);
                }
                IsForBrowserUp.WaitOne();
                if (IsActive)
                {
                    NativeMessaging.ActiveNMC = this;
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
            IsForBrowserUp.WaitOne();
            ForBrowserWindow.Dispatcher.BeginInvoke(ForBrowserWindow.MakeHidden);
            Watcher.Path = MyAppData;
            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Filter = "sending_object.json";
            Watcher.Changed += OnSendingObjectChanged;
            Watcher.EnableRaisingEvents = true;
        }

        public void OnSendingObjectChanged(object source, FileSystemEventArgs e)
        {
            if ((DateTime.Now - beforeTime).TotalMilliseconds < FileModifiedIgnoreTime) return;
            beforeTime = DateTime.Now;
            ReadJSON();
        }
        public void SendCommand()
        {
            var cts = new CommandToSend();
            cts.Command = "ChangeTab";
            cts.TabID = ForBrowserWindow.ColumnIndexVsRowIndexVsTabCommon[ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab.Key][ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab.Value].MyTab.TabID;
            cts.WindowID = ForBrowserWindow.ColumnIndexVsRowIndexVsTabCommon[ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab.Key][ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab.Value].MyTab.WindowID;
            File.WriteAllText(MyAppData + @"\for_receiving.json", JsonConvert.SerializeObject(cts));
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
            beforeTime = DateTime.Now - TimeSpan.FromSeconds(1);
            IsForBrowserUp = new EventWaitHandle(false, EventResetMode.ManualReset);
            if (IsRunning == true)
            {
                WindowThread = new Thread(() =>
                {
                    WindowState = 1;
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                    ForBrowserWindow = new ForBrowser(this, Direction.down); //todo
                    WindowState = 10;
                    IsForBrowserUp.Set();
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

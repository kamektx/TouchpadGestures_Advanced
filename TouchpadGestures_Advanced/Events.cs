using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TouchpadGestures_Advanced
{
    public class Events
    {
        public static EventWaitHandle NMC_Created = new EventWaitHandle(false, EventResetMode.ManualReset, "TouchpadGestures_Advanced_NMC_Created");
        private static Thread EventLoopForNMC_Created;

        public static void RunEventLoopForNMC_Created()
        {
            EventLoopForNMC_Created = new Thread(() =>
            {
                while (true)
                {
                    NMC_Created.WaitOne();
                    NativeMessaging.ScanNMC();
                    NMC_Created.Reset();
                }
            });
            EventLoopForNMC_Created.IsBackground = true;
            EventLoopForNMC_Created.Start();
        }
    }
}

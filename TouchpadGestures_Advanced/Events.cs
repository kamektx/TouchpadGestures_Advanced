using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TouchpadGestures_Advanced
{
    public static class Events
    {
        public static EventWaitHandle NMC_Changed = new EventWaitHandle(false, EventResetMode.ManualReset, "TouchpadGestures_Advanced_NMC_Changed");
    }
}

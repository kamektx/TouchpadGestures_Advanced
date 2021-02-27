using System;
using System.Collections.Generic;
using System.Text;

namespace TouchpadGestures_Advanced
{
    public class Settings
    {        
        public int VerticalThresholdSmall = 75;
        public int HorizontalThreshold = 150;
        public int ThresholdActive = 170;
        public int IgnoreTime = 180;
        public double IgnoreMagnification = 0.1;
        public double ThresholdAngle = 45d;
    }

    public class Status
    {
        public string ForegroundApplication { get; set; }
    }
}

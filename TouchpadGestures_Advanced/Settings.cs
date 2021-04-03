using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace TouchpadGestures_Advanced
{
    public class Settings
    {
        public void SaveSettings(string str = "default")
        {
            File.WriteAllText(App.TGA_AppData + @"\Settings\" + str + ".json", JsonConvert.SerializeObject(this));
        }
        public static Settings LoadSettings(string str = "default")
        {
            string appSettingJSON = null;
            string settingPath = null;
            if (str != "default" && File.Exists(App.TGA_AppData + @"\Settings\" + str + @".json"))
            {
                settingPath = App.TGA_AppData + @"\Settings\" + str + @".json";
            }
            else
            {
                settingPath = App.TGA_AppData + @"\Settings\default.json";
            }
            for (int errorCount = 10; errorCount > 0; errorCount--)
            {
                try
                {
                    appSettingJSON = File.ReadAllText(settingPath, Encoding.UTF8);
                }
                catch (IOException)
                {
                    if (errorCount <= 1)
                    {
                        throw;
                    }
                    Thread.Sleep(5);
                    continue;
                }
                break;
            }
            return JsonConvert.DeserializeObject<Settings>(appSettingJSON);
        }
        private double _ThresholdMultiplier = 1.0;
        public double ThresholdMultiplier
        {
            get { return _ThresholdMultiplier; }
            set { _ThresholdMultiplier = value; }
        }

        private double _VerticalThresholdSmall = 68.0;
        public double VerticalThresholdSmall
        {
            get
            {
                return _VerticalThresholdSmall * ThresholdMultiplier;
            }
            set
            {
                _VerticalThresholdSmall = value;
            }
        }
        private double _VerticalMultiplierForBig = 1.3;
        public double VerticalMultiplierForBig
        {
            get { return _VerticalMultiplierForBig; }
            set { _VerticalMultiplierForBig = value; }
        }
        [JsonIgnore]
        public double VerticalThresholdBig
        {
            get
            {
                return VerticalThresholdSmall * VerticalMultiplierForBig;
            }
        }

        private double _HorizontalThreshold = 90.0;
        public double HorizontalThreshold
        {
            get
            {
                return _HorizontalThreshold * ThresholdMultiplier;
            }
            set
            {
                _HorizontalThreshold = value;
            }
        }
        private double _ThresholdActive = 170;
        public double ThresholdActive
        {
            get { return _ThresholdActive; }
            set { _ThresholdActive = value; }
        }
        private int _IgnoreTime = 160;
        public int IgnoreTime
        {
            get { return _IgnoreTime; }
            set { _IgnoreTime = value; }
        }
        private double _IgnoreMagnification = 0.1;
        public double IgnoreMagnification
        {
            get { return _IgnoreMagnification; }
            set { _IgnoreMagnification = value; }
        }
        private double _ThresholdAngle = 45d;
        public double ThresholdAngle
        {
            get { return _ThresholdAngle; }
            set { _ThresholdAngle = value; }
        }
    }

    public static class Status
    {
        public static string ForegroundApplication { get; set; }
        public static int PrimaryScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
        public static int PrimaryScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
        public static int ForBrowserMaxHeight = PrimaryScreenHeight - 140;
        public static int MinimumHorizontalPadding = 40;
        //public static int MinimumVerticalPadding = 30;
        public static int ForBrowserMaxWidth = PrimaryScreenWidth - MinimumHorizontalPadding * 2;
        public static int MaxRowsOfTabWithImage = ForBrowserMaxHeight / 165;
        public static int WidthForRemainingColumns = 120;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.RightsManagement;
using System.Text;
using System.Windows.Navigation;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TouchpadGestures_Advanced
{
    public class SendingObject
    {
        public class MyWindowsConverter : JsonConverter<Dictionary<int, MyWindow>>
        {
            public override void WriteJson(JsonWriter writer, Dictionary<int, MyWindow> value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override Dictionary<int, MyWindow> ReadJson(JsonReader reader, Type objectType, Dictionary<int, MyWindow> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JToken jt = JToken.Load(reader);
                var myWindows = new Dictionary<int, MyWindow>();
                foreach (var item in jt.Children())
                {
                    myWindows.Add((int)item[0], item[1].ToObject<MyWindow>(serializer));
                }
                return myWindows;
            }
        }
        public Arrangements_ Arrangements { get; set; }
        public string Type { get; set; }

        [JsonConverter(typeof(MyWindowsConverter))]
        public Dictionary<int, MyWindow> Windows { get; set; }
        public int? ActiveWindowID { get; set; }

        public class Arrangements_
        {
            public Arrangement Down { get; set; }
            public Arrangement Up { get; set; }
            public Arrangement Left { get; set; }
            public Arrangement Right { get; set; }
            public HandleDirection_ HandleDirection { get; set; }
            public class Arrangement
            {
                public List<ColumnSetting> Column { get; set; }
                public StartPosition_ StartPosition { get; set; }
                public class ColumnSetting
                {
                    public string Type { get; set; }
                    public int MaxColumns { get; set; }
                    public string Size { get; set; }
                }
                public class StartPosition_
                {
                    public int Index { get; set; }
                    public int Column { get; set; }
                }
            }
            public class HandleDirection_
            {
                public bool Down { get; set; }
                public bool Up { get; set; }
                public bool Left { get; set; }
                public bool Right { get; set; }
            }
        }
        public class MyWindow
        {
            public class MyTabsConverter : JsonConverter<Dictionary<int, MyTab>>
            {
                public override void WriteJson(JsonWriter writer, Dictionary<int, MyTab> value, JsonSerializer serializer)
                {
                    writer.WriteValue(value.ToString());
                }

                public override Dictionary<int, MyTab> ReadJson(JsonReader reader, Type objectType, Dictionary<int, MyTab> existingValue, bool hasExistingValue, JsonSerializer serializer)
                {
                    JToken jt = JToken.Load(reader);
                    var myTabs = new Dictionary<int, MyTab>();
                    foreach (var item in jt.Children())
                    {
                        myTabs.Add((int)item[0], item[1].ToObject<MyTab>(serializer));
                    }
                    return myTabs;
                }
            }
            public bool IsActive { get; set; }
            public int WindowID { get; set; }
            public int? ActiveTabID { get; set; }
            public List<int> RecentTabs { get; set; }
            public List<int> TabsInOrder { get; set; }

            [JsonConverter(typeof(MyTabsConverter))]
            public Dictionary<int, MyTab> Tabs { get; set; }
            public class MyTab
            {
                public bool? IsActive { get; set; }
                public int WindowID { get; set; }
                public int TabID { get; set; }
                public string Status { get; set; }
                public string ScreenShot { get; set; }
                public string Title { get; set; }
                public string URL { get; set; }
                public string Favicon { get; set; }
                public bool? IsHidden { get; set; }
                public bool? IsPinned { get; set; }
            }
        }
    }
}

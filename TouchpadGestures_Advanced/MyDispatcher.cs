using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace TouchpadGestures_Advanced
{

    public enum ActionType
    {
        dontHandle,
        shortcut,
        nativeMessaging
    }
    public class MyDispatcherData
    {
        public Dictionary<Direction, ActionType> ActionType { get; set; }
        public Dictionary<Direction, MyAction> DirectionAction { get; set; }
    }
    public class MyDispatcher
    {
        public MyDispatcherData Data;

        public Direction FirstDirection;
        public bool IsActive = false;
        public int VerticalThreshold
        {
            get
            {
                if (IsActive)
                {
                    return Data.DirectionAction[FirstDirection].VerticalThreshold;
                }
                else
                {
                    return App.Settings.VerticalThresholdSmall;
                }
            }
        }
        public int HorizontalThreshold
        {
            get
            {
                if (IsActive)
                {
                    return Data.DirectionAction[FirstDirection].HorizontalThreshold;
                }
                else
                {
                    return App.Settings.HorizontalThreshold;
                }
            }
        }
        public ActionType FirstStroke(Direction direction)
        {
            IsActive = true;
            FirstDirection = direction;
            Data.DirectionAction[direction].FirstStroke(direction);
            return Data.ActionType[direction];
        }
        public void Stroke(Direction direction)
        {
            if (IsActive)
            {
                Data.DirectionAction[FirstDirection].Stroke(direction);
            }
            else
            {
                Debug.WriteLine("Dispatcher.Stroke() called before FirstStroke()");
            }
        }
        public void Inactivate()
        {
            Data.DirectionAction[FirstDirection].Inactivate();
            IsActive = false;
        }

        public MyDispatcher(string str)
        {
            //Data = new MyDispatcherData();
            //Data.ActionType = new Dictionary<Direction, ActionType>();
            //Data.DirectionAction = new Dictionary<Direction, MyAction>();
            //Data.ActionType[Direction.down] = ActionType.shortcut;
            //Data.ActionType[Direction.up] = ActionType.shortcut;
            //Data.ActionType[Direction.left] = ActionType.dontHandle;
            //Data.ActionType[Direction.right] = ActionType.dontHandle;
            //Data.ActionType[Direction.up] = ActionType.shortcut;
            //Data.DirectionAction[Direction.down] = new Shortcut(
            //    new List<int> { (int)Tools.Key.LeftControl },
            //    new Dictionary<Direction, List<int>> {
            //            { Direction.down, new List<int> { (int)Tools.Key.Tab } },
            //            { Direction.up, new List<int> { (int)Tools.Key.LeftShift, (int)Tools.Key.Tab } }
            //    }
            //);
            //Data.DirectionAction[Direction.up] = new Shortcut(
            //    new List<int> { (int)Tools.Key.LeftControl },
            //    new Dictionary<Direction, List<int>> {
            //            { Direction.down, new List<int> { (int)Tools.Key.Tab } },
            //            { Direction.up, new List<int> { (int)Tools.Key.LeftShift, (int)Tools.Key.Tab } }
            //    }
            //);
            //File.WriteAllText(App.TGA_AppData + @"\AppSettings\default.json", JsonConvert.SerializeObject(Data, new JsonSerializerSettings
            //{
            //    TypeNameHandling = TypeNameHandling.Auto
            //}));

            string appSettingJSON = null;
            string settingPath = null;
            if (File.Exists(App.TGA_AppData + @"\AppSettings\" + str + @".json"))
            {
                settingPath = App.TGA_AppData + @"\AppSettings\" + str + @".json";
            }
            else
            {
                settingPath = App.TGA_AppData + @"\AppSettings\default.json";
            }
            int count = 0;
            do
            {
                try
                {
                    appSettingJSON = File.ReadAllText(settingPath, Encoding.UTF8);
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
            Data = JsonConvert.DeserializeObject<MyDispatcherData>(appSettingJSON, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}

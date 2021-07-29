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
        nativeMessaging,
        error
    }
    public class MyDispatcherData
    {
        public Dictionary<Direction, ActionType> ActionType { get; set; }
        public Dictionary<Direction, MyAction> DirectionAction { get; set; }
    }
    public class MyDispatcher
    {
        private MyDispatcherData Data;
        private bool IsDefault;
        public string ApplicationName { get; private set; }

        public Direction FirstDirection = Direction.down;
        public bool IsActive = false;
        public bool InterpretSize(PointD size)
        {
            switch (WhichActionType())
            {
                case ActionType.dontHandle:
                    Debug.WriteLine("DispatcherInterpretSize() is called when the ActionType is dontHandle.");
                    break;
                case ActionType.shortcut:
                    Data.DirectionAction[FirstDirection].InterpretSize(size);
                    break;
                case ActionType.nativeMessaging:
                    Data.DirectionAction[FirstDirection].InterpretSize(size);
                    break;
                case ActionType.error:
                    Debug.WriteLine("DispatcherInterpretSize() is called before FirstStroke().");
                    break;
                default:
                    break;
            }
            return true;
        }
        public double VerticalThreshold
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
        public double HorizontalThreshold
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
        public ActionType WhichActionType()
        {
            if (IsActive)
            {
                return Data.ActionType[FirstDirection];
            }
            else
            {
                Debug.WriteLine("MyDispatcher.WhichActionType() is called before FirstStroke().");
                return ActionType.error;
            }
        }
        public ActionType WhichActionType(Direction direction)
        {
            return Data.ActionType[direction];
        }
        public void Inactivate()
        {
            Data.DirectionAction[FirstDirection].Inactivate();
            IsActive = false;
        }

        public static void MakeMyDispatcherSetting(string applicationName)
        {
            var data = new MyDispatcherData();
            data.ActionType = new Dictionary<Direction, ActionType>();
            data.DirectionAction = new Dictionary<Direction, MyAction>();
            data.ActionType[Direction.down] = ActionType.nativeMessaging;
            data.ActionType[Direction.up] = ActionType.dontHandle;
            data.ActionType[Direction.left] = ActionType.dontHandle;
            data.ActionType[Direction.right] = ActionType.dontHandle;
            data.DirectionAction[Direction.down] = new NativeMessagingAction();
            File.WriteAllText(App.TGA_AppData + @"\SettingsForActiveApp\" + applicationName + ".json", JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));
        }

        public bool ShouldMakeNewMyDispatcher(string applicationName)
        {
            if (Status.ForegroundApplication == applicationName)
            {
                return false;
            }
            else if (File.Exists(App.TGA_AppData + @"\SettingsForActiveApp\" + applicationName + @".json"))
            {
                return true;
            }
            else if (IsDefault)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public MyDispatcher(string applicationName)
        {
            string appSettingJSON = null;
            string settingPath = null;
            ApplicationName = applicationName;
            if (applicationName != "default" && File.Exists(App.TGA_AppData + @"\SettingsForActiveApp\" + applicationName + @".json"))
            {
                settingPath = App.TGA_AppData + @"\SettingsForActiveApp\" + applicationName + @".json";
                IsDefault = false;
            }
            else
            {
                settingPath = App.TGA_AppData + @"\SettingsForActiveApp\default.json";
                IsDefault = true;
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
            Data = JsonConvert.DeserializeObject<MyDispatcherData>(appSettingJSON, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            foreach (var nmc in NativeMessaging.NMCs.Values)
            {
                nmc.SendCheckFocused();
            }
        }
    }
}

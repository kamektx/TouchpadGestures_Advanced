using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TouchpadGestures_Advanced
{
    public enum Conditions
    {
        idle, distinguish, ignore, active, dontHnadle
    }
    public class InputData
    {
        public class CollectionData
        {
            public int ContactID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string Tip { get; set; }
            public bool IsFinger { get; set; }
            public double NX //Normalized X
            {
                get { return X * 4096d / 1228d; } //todo: 1228 is LogicalMax of TouchPad's X. 
            }
            public double NY //Normalized Y
            {
                get { return Y * 4096d / 1228d; }
            }
            public PointD Point
            {
                get
                {
                    return new PointD(NX, NY);
                }
            }
        }
        public List<CollectionData> LinkCollection { get; set; }
        public int ScanTime { get; set; }
        public int ContactCount { get; set; }
        public bool IsOff
        {
            get
            {
                bool _IsOff = false;
                for (int i = 0; i < ContactCount; i++)
                {
                    if (LinkCollection[i].Tip == "off")
                    {
                        _IsOff = true;
                        break;
                    }
                }
                return _IsOff;
            }
        }
        public bool IsOffMoreThan2
        {
            get
            {
                int offNum = 0;
                for (int i = 0; i < ContactCount; i++)
                {
                    if (LinkCollection[i].Tip == "off")
                    {
                        offNum++;
                    }
                }
                return offNum >= 2;
            }
        }
        public PointD Mean
        {
            get
            {
                PointD _Mean = new PointD(0, 0);
                for (int i = 0; i < 3; i++)
                {
                    _Mean += LinkCollection[i].Point;
                }
                _Mean /= 3;
                return _Mean;
            }
        }
        public PointD Mean2(List<int> linkCollectionList)
        {
            PointD _Mean = new PointD(0, 0);
            foreach (var item in linkCollectionList)
            {
                _Mean += LinkCollection[item].Point;
            }
            _Mean /= linkCollectionList.Count;
            return _Mean;
        }

        public void RemoveNotFinger()
        {
            for (int i = 0; i < ContactCount; i++)
            {
                if (LinkCollection[i].Tip != "no" && LinkCollection[i].IsFinger == false)
                {
                    LinkCollection.RemoveAt(i);
                    ContactCount--;
                    LinkCollection.Add(new CollectionData { X = 0, Y = 0, Tip = "no", IsFinger = false, ContactID = 0 });
                }
            }
        }
    }
    internal static class Interpreter
    {
        internal static InputData _InputData;
        internal static InputData _OldInputData;
        internal static PointD _Size = new PointD(0, 0);
        internal static Conditions Condition = Conditions.idle;
        internal static bool InterpretableAsThreeFingers 
        {
            get
            {
                return 
                    ((Condition == Conditions.active || Condition == Conditions.ignore) && ((_InputData.ContactCount == 2 && _InputData.IsOff == false)|| (_InputData.ContactCount == 3 && _InputData.IsOffMoreThan2 == false)))
                    ||
                    (_InputData.ContactCount == 3 && _InputData.IsOff == false);
            }
        }

        internal static bool GetNewSize(InputData inputData, InputData oldInputData)
        {
            if((inputData.ContactCount == 3 && inputData.IsOff == false) && (oldInputData.ContactCount == 3 && oldInputData.IsOff == false))
            {
                _Size += (inputData.Mean - oldInputData.Mean) * (Condition == Conditions.ignore ? Settings.IgnoreMagnification : 1d);
            }
            else
            {
                var nowContactID_LinkCollection = new Dictionary<int, int>();
                var oldContactID_LinkCollection = new Dictionary<int, int>();
                var nowLinkCollectionList = new List<int>();
                var oldLinkCollectionList = new List<int>();
                for (int i = 0; i < inputData.ContactCount; i++)
                {
                    nowContactID_LinkCollection[inputData.LinkCollection[i].ContactID] = i;
                }
                for (int i = 0; i < oldInputData.ContactCount; i++)
                {
                    oldContactID_LinkCollection[oldInputData.LinkCollection[i].ContactID] = i;
                }
                foreach (var contactID_Index in nowContactID_LinkCollection)
                {
                    if (oldContactID_LinkCollection.ContainsKey(contactID_Index.Key))
                    {
                        nowLinkCollectionList.Add(contactID_Index.Value);
                        oldLinkCollectionList.Add(oldContactID_LinkCollection[contactID_Index.Key]);
                    }
                }
                _Size += (inputData.Mean2(nowLinkCollectionList) - oldInputData.Mean2(oldLinkCollectionList)) * (Condition == Conditions.ignore ? Settings.IgnoreMagnification : 1d);
            }
            return true;
        }

        internal static void SetJson(string argJson)
        {
            _InputData = JsonSerializer.Deserialize<InputData>(argJson);
            _InputData.RemoveNotFinger();
            Interpret();
        }
        internal static void Interpret()
        {
            if (InterpretableAsThreeFingers == false)
            {
                if (Condition != Conditions.idle)
                {
                    _Size = new PointD(0, 0);
                    App.DispatcherNow.Inactivate();
                    Condition = Conditions.idle;
                }
            }
            else
            {
                if (Condition == Conditions.active || Condition == Conditions.ignore)
                {
                    GetNewSize(_InputData, _OldInputData);
                    while (_Size.Height >= App.DispatcherNow.VerticalThreshold)
                    {
                        _Size.Height -= App.DispatcherNow.VerticalThreshold;
                        App.DispatcherNow.Stroke(Direction.down);
                    }
                    while (_Size.Height <= -App.DispatcherNow.VerticalThreshold)
                    {
                        _Size.Height += App.DispatcherNow.VerticalThreshold;
                        App.DispatcherNow.Stroke(Direction.up);
                    }
                    while (_Size.Width >= App.DispatcherNow.HorizontalThreshold)
                    {
                        _Size.Width -= App.DispatcherNow.HorizontalThreshold;
                        App.DispatcherNow.Stroke(Direction.right);
                    }
                    while (_Size.Width <= -App.DispatcherNow.HorizontalThreshold)
                    {
                        _Size.Width += App.DispatcherNow.HorizontalThreshold;
                        App.DispatcherNow.Stroke(Direction.left);
                    }
                }
                else if (Condition == Conditions.distinguish)
                {
                    GetNewSize(_InputData, _OldInputData);
                    if (_Size.Abs > Settings.ThresholdActive)
                    {
                        foreach (Direction item in Enum.GetValues(typeof(Direction)))
                        {
                            if (_Size.IsDirection(item))
                            {
                                if(App.DispatcherNow.Type[item] == DispatcherType.dontHandle)
                                {
                                    Condition = Conditions.dontHnadle;
                                    break;
                                }
                                else
                                {
                                    _Size = new PointD(0, 0);
                                    Condition = Conditions.ignore;
                                    IgnoreTimer();
                                    App.DispatcherNow.FirstStroke(item);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (Condition == Conditions.idle)
                {
                    Condition = Conditions.distinguish;
                }
            }
            _OldInputData = _InputData;
            return;
        }
        private static async void IgnoreTimer()
        {
            await Task.Delay(Settings.IgnoreTime);
            if (Condition == Conditions.ignore)
            {
                Condition = Conditions.active;
            }
        }
    }
}

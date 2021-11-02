using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TouchpadGestures_Advanced
{
    public enum Conditions
    {
        idle, distinguish, ignore, active, dontHnadle
    }
    public class InputDataForInit
    {
        public int XLogicalMax { get; set; }
        public int YLogicalMax { get; set; }
    }
    public class InputData
    {
        public static int XLogicalMax = 1228;
        public static int YLogicalMax = 928;
        public class CollectionData
        {
            public int ContactID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string Tip { get; set; }
            public bool IsFinger { get; set; }
            public double NX //Normalized X
            {
                get { return X * 2784d / InputData.YLogicalMax; }
            }
            public double NY //Normalized Y
            {
                get { return Y * 2784d / InputData.YLogicalMax; }
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
        public bool IsHybrid { get; set; }
        public int DataCount { get; set; }
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

        public InputData RemoveNotFinger()
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
            return this;
        }
        public bool GetCInput(InputData cInput)
        {
            for (int i = this.DataCount; i < this.DataCount + cInput.DataCount; i++)
            {
                LinkCollection[i] = cInput.LinkCollection[i - this.DataCount];
            }
            this.DataCount += cInput.DataCount;
            return this.DataCount == this.ContactCount;
        }
    }
    internal static class Interpreter
    {
        internal static InputData _TandCInputDataTemp;
        internal static InputData _InputData;
        internal static InputData _OldInputData;
        internal static PointD _Size = new PointD(0, 0);
        internal static Conditions Condition = Conditions.idle;
        internal static bool InterpretableAsThreeFingers
        {
            get
            {
                return
                    (
                        (Condition == Conditions.active || Condition == Conditions.ignore)
                        &&
                        (
                            (_InputData.ContactCount == 2 && _InputData.IsOff == false)
                            ||
                            (_InputData.ContactCount == 3 && _InputData.IsOffMoreThan2 == false)
                        )
                    )
                    ||
                    (_InputData.ContactCount == 3 && _InputData.IsOff == false);
            }
        }

        internal static bool GetNewSize(ref PointD size, InputData inputData, InputData oldInputData)
        {
            if ((inputData.ContactCount == 3 && inputData.IsOff == false) && (oldInputData.ContactCount == 3 && oldInputData.IsOff == false))
            {
                var add = (inputData.Mean - oldInputData.Mean) * (Condition == Conditions.ignore ? App.Settings.IgnoreMagnification : 1d);
                size += add;
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
                var add = (inputData.Mean2(nowLinkCollectionList) - oldInputData.Mean2(oldLinkCollectionList)) * (Condition == Conditions.ignore ? App.Settings.IgnoreMagnification : 1d);
                size += add;
            }

            return true;
        }
        internal static void InitJson(string argJson)
        {
            // UI Thread
            var initData = JsonConvert.DeserializeObject<InputDataForInit>(argJson);
            InputData.XLogicalMax = initData.XLogicalMax;
            InputData.YLogicalMax = initData.YLogicalMax;
        }
        internal static void SetJson(string argJson)
        {
            // UI Thread
            var inputData = JsonConvert.DeserializeObject<InputData>(argJson);
            if (inputData.IsHybrid)
            {
                if (inputData.ContactCount == 0)
                {
                    bool status = _TandCInputDataTemp.GetCInput(inputData);
                    if (status)
                    {
                        _InputData = _TandCInputDataTemp.RemoveNotFinger();
                        Interpret();
                    }
                }
                else
                {
                    _TandCInputDataTemp = inputData;
                }
            }
            else
            {
                _InputData = inputData.RemoveNotFinger();
                Interpret();
            }
        }
        internal static void Interpret()
        {
            // UI Thread
            if (InterpretableAsThreeFingers == false)
            {
                if (Condition != Conditions.idle)
                {
                    _Size = new PointD(0, 0);
                    App.DispatcherNow.Inactivate();
                    Condition = Conditions.idle;
                    Task.Run(() =>
                    {
                        Thread.Sleep(20);
                        App.DispatcherSemaphore.Release();
                        ForegroundWindowWatcher.GetForegroundAppNameAndUpdateDispatcherNow();
                    });
                }
            }
            else
            {
                if (Condition == Conditions.active || Condition == Conditions.ignore)
                {
                    GetNewSize(ref _Size, _InputData, _OldInputData);
                    App.DispatcherNow.InterpretSize(_Size);
                }
                else if (Condition == Conditions.distinguish)
                {
                    GetNewSize(ref _Size, _InputData, _OldInputData);
                    if (_Size.Abs > App.Settings.ThresholdActive)
                    {
                        foreach (Direction item in Enum.GetValues(typeof(Direction)))
                        {
                            if (_Size.IsDirection(item))
                            {
                                if (App.DispatcherNow.WhichActionType(item) == ActionType.dontHandle)
                                {
                                    Condition = Conditions.dontHnadle;
                                    break;
                                }
                                else
                                {
                                    _Size = new PointD(0, 0);
                                    App.DispatcherNow.FirstStroke(item);
                                    Condition = Conditions.ignore;
                                    IgnoreTimer();
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (Condition == Conditions.idle)
                {
                    App.DispatcherSemaphore.Wait(); // UI Thread Waiting!! Watch out!!
                    // ✓ This Semaphore should be released immediately from other threads.
                    // ✓ This Thread doesn't wait for this Semaphore anywhere but here. 
                    // ✓ This Semaphore will be released for sure.  
                    Condition = Conditions.distinguish;
                }
            }
            _OldInputData = _InputData;
            return;
        }
        private static async void IgnoreTimer()
        {
            await Task.Delay(App.Settings.IgnoreTime);
            if (Condition == Conditions.ignore)
            {
                Condition = Conditions.active;
            }
        }
    }
}

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
    }
    internal static class Interpreter
    {
        internal static InputData _InputData;
        internal static InputData _OldInputData;
        internal static PointD _Size = new PointD(0, 0);
        internal static Conditions Condition = Conditions.idle;

        internal static void SetJson(string argJson)
        {
            _InputData = JsonSerializer.Deserialize<InputData>(argJson);
            Interpret();
        }
        internal static void Interpret()
        {
            if (_InputData.ContactCount != 3 || _InputData.IsOff)
            {
                if (Condition != Conditions.idle)
                {
                    _Size = new PointD(0, 0);
                    KeyInput.Up(
                        new List<int> { (int)WindowsConstants.Key.LeftControl });
                    Condition = Conditions.idle;
                    Debug.WriteLine("inactivated");
                }
            }
            else
            {
                if (Condition == Conditions.active || Condition == Conditions.ignore)
                {
                    _Size += (_InputData.Mean - _OldInputData.Mean) * (Condition == Conditions.ignore ? Settings.IgnoreMagnification : 1d);
                    while (_Size.Height >= Settings.Threshold)
                    {
                        _Size.Height -= Settings.Threshold;
                        KeyInput.Press(new List<int> { (int)WindowsConstants.Key.Tab });
                    }
                    while (_Size.Height <= -Settings.Threshold)
                    {
                        _Size.Height += Settings.Threshold;
                        KeyInput.Press(new List<int> { (int)WindowsConstants.Key.LeftShift, (int)WindowsConstants.Key.Tab });
                    }
                }
                else if (Condition == Conditions.distinguish)
                {
                    _Size += _InputData.Mean - _OldInputData.Mean;
                    if (_Size.Abs > Settings.ThresholdActive)
                    {
                        bool isDirectionUp = _Size.IsDirection('u');
                        bool isDirectionDown = _Size.IsDirection('d');
                        if (isDirectionUp || isDirectionDown)
                        {
                            _Size = new PointD(0, 0);
                            KeyInput.Down(new List<int> { (int)WindowsConstants.Key.LeftControl });
                            Condition = Conditions.ignore;
                            IgnoreTimer();
                            if (isDirectionDown)
                            {
                                KeyInput.Press(new List<int> { (int)WindowsConstants.Key.Tab });
                            }
                            else if (isDirectionUp)
                            {
                                KeyInput.Press(new List<int> { (int)WindowsConstants.Key.LeftShift, (int)WindowsConstants.Key.Tab });
                            }
                        }
                        else
                        {
                            Condition = Conditions.dontHnadle;
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

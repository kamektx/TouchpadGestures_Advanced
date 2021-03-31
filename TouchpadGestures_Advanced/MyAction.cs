using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace TouchpadGestures_Advanced
{
    public abstract class MyAction
    {
        public ActionType ActionType { get; set; }
        [JsonIgnore]
        public abstract double VerticalThreshold { get; }
        [JsonIgnore]
        public abstract double HorizontalThreshold { get; }
        public abstract void FirstStroke(Direction direction);
        public abstract void InterpretSize(ref PointD size);
        public abstract void Inactivate();
    }

    public class Shortcut : MyAction
    {
        private Direction FirstDirection;
        private bool IsActive = false;
        public List<int> CommonKeys;
        public Dictionary<Direction, List<int>> DirectionKeys1;
        [JsonIgnore]
        public override double VerticalThreshold
        {
            get
            {
                return App.Settings.VerticalThresholdSmall;
            }
        }
        [JsonIgnore]
        public override double HorizontalThreshold
        {
            get
            {
                return App.Settings.HorizontalThreshold;
            }
        }
        public override void FirstStroke(Direction direction)
        {
            IsActive = true;
            FirstDirection = direction;
            KeySend.Down2(CommonKeys);
            Stroke(direction);
        }
        public void Stroke(Direction direction)
        {
            if (IsActive)
            {
                if (DirectionKeys1.ContainsKey(direction))
                {
                    KeySend.Press(DirectionKeys1[direction]);
                }
            }
            else
            {
                Debug.WriteLine("MyAction.Stroke() called before FirstStroke()");
            }
        }
        public override void Inactivate()
        {
            IsActive = false;
            KeySend.Up2(CommonKeys);
        }
        public override void InterpretSize(ref PointD size)
        {
            while (size.Height > VerticalThreshold)
            {
                size.Height -= VerticalThreshold;
                Stroke(Direction.down);
            }
            while (size.Height < -VerticalThreshold)
            {
                size.Height += VerticalThreshold;
                Stroke(Direction.up);
            }
            while (size.Width > HorizontalThreshold)
            {
                size.Width -= HorizontalThreshold;
                Stroke(Direction.right);
            }
            while (size.Width < -HorizontalThreshold)
            {
                size.Width += HorizontalThreshold;
                Stroke(Direction.left);
            }
        }
        public Shortcut(List<int> commonKeys, Dictionary<Direction, List<int>> directionKeys1)
        {
            ActionType = ActionType.shortcut;
            CommonKeys = new List<int>(commonKeys);
            DirectionKeys1 = new Dictionary<Direction, List<int>>(directionKeys1);
            // shallow copy...
        }
    }
    public class NativeMessagingAction : MyAction
    {
        private Direction FirstDirection;
        private bool IsActive = false;
        private NMC_Manager MyNMC = null;
        [JsonIgnore]
        public override double VerticalThreshold
        {
            get
            {
                if (IsActive)
                {
                    var crst = MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab;
                    if (MyNMC.ForBrowserWindow.ColumnIndexVsTabSize[crst.Key] == TabSize.big)
                    {
                        return App.Settings.VerticalThresholdBig;
                    }
                }
                
                return App.Settings.VerticalThresholdSmall;
            }
        }
        [JsonIgnore]
        public override double HorizontalThreshold
        {
            get
            {
                return App.Settings.HorizontalThreshold;
            }
        }

        public override void FirstStroke(Direction direction)
        {
            FirstDirection = direction;
            MyNMC = NativeMessaging.ActiveNMC;
            if (MyNMC != null && MyNMC.IsActive)
            {
                IsActive = true;
                MyNMC.MySemaphore.Wait();
                MyNMC.ForBrowserWindow.Dispatcher.BeginInvoke(MyNMC.ForBrowserWindow.MakeVisible);
            }
            else
            {
                IsActive = false;
            }
        }
        public override void Inactivate()
        {
            if (IsActive)
            {
                MyNMC.MySemaphore.Release();
                MyNMC.ForBrowserWindow?.Dispatcher.BeginInvoke(MyNMC.ForBrowserWindow.MakeHidden);
            }
            IsActive = false;
        }
        public override void InterpretSize(ref PointD size)
        {
            if(IsActive)
            {
                var fbw = MyNMC.ForBrowserWindow;
                var crst = fbw.MyData.ColumnIndexAndRowIndexOfSelectedTab;
                while (size.Width > HorizontalThreshold)
                {
                    if (crst.Key >= fbw.ColumnIndexVsRowIndexVsTabCommon.Count - 1)
                    {
                        size.Width = HorizontalThreshold;
                        break;
                    }
                    size.Width -= HorizontalThreshold;
                    MoveHorizontal(ref size, Direction.right);
                }
                while (size.Width < -HorizontalThreshold)
                {
                    if (crst.Key <= 0)
                    {
                        size.Width = -HorizontalThreshold;
                        break;
                    }
                    size.Width += HorizontalThreshold;
                    MoveHorizontal(ref size, Direction.left);
                }
                while (size.Height > VerticalThreshold)
                {
                    if (crst.Value >= fbw.ColumnIndexVsRowIndexVsTabCommon[crst.Key].Count - 1)
                    {
                        size.Height = VerticalThreshold;
                        break;
                    }
                    size.Height -= VerticalThreshold;
                    MoveVertical(Direction.down);
                }
                while (size.Height < -VerticalThreshold)
                {
                    if (crst.Value <= 0)
                    {
                        size.Height = -VerticalThreshold;
                        break;
                    }
                    size.Height += VerticalThreshold;
                    MoveVertical(Direction.up);
                }
            }
        }
        public void MoveHorizontal(ref PointD size, Direction direction)
        {
            var crst = MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab;
            int hereColumn = crst.Key;
            int hereRow = crst.Value;
            int nextColumn;
            switch (direction)
            {
                case Direction.down:
                    throw new ArgumentException();
                case Direction.up:
                    throw new ArgumentException();
                case Direction.right:
                    nextColumn = hereColumn + 1;
                    break;
                case Direction.left:
                    nextColumn = hereColumn - 1;
                    break;
                default:
                    throw new ArgumentException();
            }
            var vb = MyNMC.ForBrowserWindow.ColumnIndexVsRowIndexVsVerticalBoundary;
            double location = vb[hereColumn][hereRow] + (vb[hereColumn][hereRow + 1] - vb[hereColumn][hereRow]) * ( 0.5 + size.Height / (2.0 * VerticalThreshold));
            if (location < 0) location = 0;
            int nextRow = vb[nextColumn].FindIndex(val => val > location) - 1;
            if (nextRow == -2) nextRow = vb[nextColumn].Count - 2;
            MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(nextColumn, nextRow);
            double nextHeight = (location - (vb[nextColumn][nextRow + 1] + vb[nextColumn][nextRow]) / 2.0) * 2.0 * VerticalThreshold / (vb[nextColumn][nextRow + 1] - vb[nextColumn][nextRow]);
            if (nextHeight > VerticalThreshold) nextHeight = VerticalThreshold;
            size.Height = nextHeight;
        }
        public void MoveVertical(Direction direction)
        {
            var crst = MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab;
            int hereColumn = crst.Key;
            int hereRow = crst.Value;
            int nextRow;
            switch (direction)
            {
                case Direction.right:
                    throw new ArgumentException();
                case Direction.left:
                    throw new ArgumentException();
                case Direction.down:
                    nextRow = hereRow + 1;
                    break;
                case Direction.up:
                    nextRow = hereRow - 1;
                    break;
                default:
                    throw new ArgumentException();
            }
            MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(hereColumn, nextRow);
        }
        public NativeMessagingAction()
        {
            ActionType = ActionType.nativeMessaging;
        }
    }
}

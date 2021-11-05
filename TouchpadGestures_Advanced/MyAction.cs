using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public abstract void InterpretSize(PointD size);
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
            //UI Thread
            IsActive = true;
            FirstDirection = direction;
            KeySend.Down2(CommonKeys);
            Stroke(direction);
        }
        public void Stroke(Direction direction)
        {
            // UI Thread
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
            // UI Thread
            IsActive = false;
            KeySend.Up2(CommonKeys);
        }
        public override void InterpretSize(PointD size)
        {
            // UI Thread
            while (size.Height > VerticalThreshold)
            {
                size.Height -= VerticalThreshold * App.Settings.NextPositionScale;
                Stroke(Direction.down);
            }
            while (size.Height < -VerticalThreshold)
            {
                size.Height += VerticalThreshold * App.Settings.NextPositionScale;
                Stroke(Direction.up);
            }
            while (size.Width > HorizontalThreshold)
            {
                size.Width -= HorizontalThreshold * App.Settings.NextPositionScale;
                Stroke(Direction.right);
            }
            while (size.Width < -HorizontalThreshold)
            {
                size.Width += HorizontalThreshold * App.Settings.NextPositionScale;
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
        private KeyValuePair<int, int> defaultColumnIndexAndRowIndexOfSelectedTab;
        private PointD PreviousSize = new PointD(0, 0);
        private PointD PreviousPosition = new PointD(0, 0);
        private TaskQueueSingleThread TaskQueue = new TaskQueueSingleThread();

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
            // UI Thread until here
            TaskQueue.AddTask(() =>
            {
                if (IsActive) return;

                FirstDirection = direction;
                //NativeMessaging.ActiveNMC?.CheckRunning();
                MyNMC = NativeMessaging.ActiveNMC;
                if (MyNMC != null && MyNMC.IsActive)
                {
                    PreviousSize = new PointD(0, 0);
                    PreviousPosition = new PointD(0, 0);
                    MyNMC.MySemaphore.Wait();
                    defaultColumnIndexAndRowIndexOfSelectedTab = MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab;
                    MyNMC.ForBrowserWindow.Dispatcher.Invoke(MyNMC.ForBrowserWindow.MakeVisible);
                    IsActive = true;
                }
                else
                {
                    IsActive = false;
                }
            });

        }
        public override void Inactivate()
        {
            // UI Thread until here
            TaskQueue.AddTask(() =>
            {
                if (IsActive)
                {
                    IsActive = false;
                    MyNMC.SendChangeTab();
                    MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab = defaultColumnIndexAndRowIndexOfSelectedTab;
                    try
                    {
                        MyNMC.MySemaphore.Release();
                    }
                    catch { }
                    MyNMC.ForBrowserWindow.Dispatcher.Invoke(MyNMC.ForBrowserWindow.MakeHidden);
                }
            });
        }
        public override void InterpretSize(PointD size)
        {
            // UI Thread
            if (!IsActive)
            {
                PreviousSize = new PointD(size);
                return;
            }
            var sizeDiff = size - PreviousSize;
            var fbw = MyNMC.ForBrowserWindow;
            var crst = fbw.MyData.ColumnIndexAndRowIndexOfSelectedTab;
            var currentPosition = PreviousPosition + sizeDiff;
            while (currentPosition.Width > HorizontalThreshold)
            {
                if (crst.Key >= fbw.ColumnIndexVsRowIndexVsTabCommon.Count - 1)
                {
                    currentPosition.Width = HorizontalThreshold;
                    break;
                }
                MoveHorizontal(ref currentPosition, Direction.right);
                currentPosition.Width -= HorizontalThreshold * App.Settings.NextPositionScale;
            }
            while (currentPosition.Width < -HorizontalThreshold)
            {
                if (crst.Key <= 0)
                {
                    currentPosition.Width = -HorizontalThreshold;
                    break;
                }
                MoveHorizontal(ref currentPosition, Direction.left);
                currentPosition.Width += HorizontalThreshold * App.Settings.NextPositionScale;
            }
            while (currentPosition.Height > VerticalThreshold)
            {
                if (crst.Value >= fbw.ColumnIndexVsRowIndexVsTabCommon[crst.Key].Count - 1)
                {
                    currentPosition.Height = VerticalThreshold;
                    break;
                }
                MoveVertical(Direction.down);
                currentPosition.Height -= VerticalThreshold * App.Settings.NextPositionScale;
            }
            while (currentPosition.Height < -VerticalThreshold)
            {
                if (crst.Value <= 0)
                {
                    currentPosition.Height = -VerticalThreshold;
                    break;
                }
                MoveVertical(Direction.up);
                currentPosition.Height += VerticalThreshold * App.Settings.NextPositionScale;
            }
            PreviousPosition = currentPosition;
            PreviousSize = new PointD(size);
        }
        public void MoveHorizontal(ref PointD size, Direction direction)
        {
            // UI Thread
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
            if (nextColumn < 0 || nextColumn > vb.Count - 1)
            {
                return;
            }
            double location = vb[hereColumn][hereRow] + (vb[hereColumn][hereRow + 1] - vb[hereColumn][hereRow]) * (0.5 + size.Height / (2.0 * VerticalThreshold));
            if (location < 0) location = 1;
            int nextRow = vb[nextColumn].FindIndex(val => val > location) - 1;
            if (nextRow == -2) nextRow = vb[nextColumn].Count - 2;
            MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(nextColumn, nextRow);
            double nextHeight = 0;
            size.Height = nextHeight;
        }
        public void MoveVertical(Direction direction)
        {
            // UI Thread
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
            if (nextRow < 0 || nextRow > MyNMC.ForBrowserWindow.ColumnIndexVsRowIndexVsTabCommon[hereColumn].Count - 1)
            {
                return;
            }
            MyNMC.ForBrowserWindow.MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(hereColumn, nextRow);
        }
        public NativeMessagingAction()
        {
            ActionType = ActionType.nativeMessaging;
        }
    }
}

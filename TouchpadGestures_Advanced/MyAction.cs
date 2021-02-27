using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TouchpadGestures_Advanced
{
    public abstract class MyAction
    {
        public ActionType ActionType { get; set; }
        [JsonIgnore]
        public abstract int VerticalThreshold { get; }
        [JsonIgnore]
        public abstract int HorizontalThreshold { get; }
        public abstract void FirstStroke(Direction direction);
        public abstract void Stroke(Direction direction);
        public abstract void ParseSize(ref PointD size);
        public abstract void Inactivate();
    }

    public class Shortcut : MyAction
    {
        private Direction FirstDirection;
        private bool IsActive = false;
        public List<int> CommonKeys;
        public Dictionary<Direction, List<int>> DirectionKeys1;
        [JsonIgnore]
        public override int VerticalThreshold
        {
            get
            {
                return App.Settings.VerticalThresholdSmall;
            }
        }
        [JsonIgnore]
        public override int HorizontalThreshold
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
        public override void Stroke(Direction direction)
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
        public override void ParseSize(ref PointD size)
        {
            throw new NotImplementedException();
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
        [JsonIgnore]
        public override int VerticalThreshold
        {
            get
            {
                return App.Settings.VerticalThresholdSmall;
            }
        }
        [JsonIgnore]
        public override int HorizontalThreshold
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
        }
        public override void Stroke(Direction direction)
        {
            throw new NotImplementedException();
        }
        public override void Inactivate()
        {
            IsActive = false;
        }
        public override void ParseSize(ref PointD size)
        {
            throw new NotImplementedException();
        }
        public NativeMessagingAction()
        {
            ActionType = ActionType.nativeMessaging;
        }
    }
}

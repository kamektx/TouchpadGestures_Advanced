using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TouchpadGestures_Advanced
{
    abstract public class MyAction
    {
        public abstract int VerticalThreshold { get; }
        public abstract int HorizontalThreshold { get; }
        public abstract void FirstStroke(Direction direction);
        public abstract void Stroke(Direction direction);
        public abstract void Inactivate();
    }

    public class Shortcut : MyAction
    {
        public override int VerticalThreshold
        {
            get
            {
                return Settings.VerticalThreshold;
            }
        }
        public override int HorizontalThreshold
        {
            get
            {
                return Settings.HorizontalThreshold;
            }
        }
        private Direction FirstDirection;
        private bool IsActive = false;
        private List<int> CommonKeys;
        private Dictionary<Direction, List<int>> DirectionKeys1;
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
        public Shortcut(List<int> commonKeys, Dictionary<Direction, List<int>> directionKeys1)
        {
            CommonKeys = new List<int>(commonKeys);
            DirectionKeys1 = new Dictionary<Direction, List<int>>(directionKeys1);
            // shallow copy...
        }
    }
}

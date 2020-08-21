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

    public enum DispatcherType
    {
        shortcut,
        nativeMessaging,
        dontHandle
    }
    public class Dispatcher
    {
        public Dictionary<Direction, DispatcherType> Type;
        public Dictionary<Direction, MyAction> DirectionAction;
        public Direction FirstDirection;
        public bool IsActive = false;
        public int VerticalThreshold
        {
            get
            {
                if (IsActive)
                {
                    return DirectionAction[FirstDirection].VerticalThreshold;
                }
                else
                {
                    return Settings.VerticalThreshold;
                }
            }
        }
        public int HorizontalThreshold
        {
            get
            {
                if (IsActive)
                {
                    return DirectionAction[FirstDirection].HorizontalThreshold;
                }
                else
                {
                    return Settings.HorizontalThreshold;
                }
            }
        }
        public void FirstStroke(Direction direction)
        {
            IsActive = true;
            FirstDirection = direction;
            DirectionAction[direction].FirstStroke(direction);
        }
        public void Stroke(Direction direction)
        {
            if (IsActive)
            {
                DirectionAction[FirstDirection].Stroke(direction);
            }
            else
            {
                Debug.WriteLine("Dispatcher.Stroke() called before FirstStroke()");
            }
        }
        public void Inactivate()
        {
            DirectionAction[FirstDirection].Inactivate();
            IsActive = false;
        }

        public Dispatcher(string str)
        {
            Type = new Dictionary<Direction, DispatcherType>();
            DirectionAction = new Dictionary<Direction, MyAction>();
            if (str == "default")
            {
                Type[Direction.down] = DispatcherType.shortcut;
                Type[Direction.up] = DispatcherType.shortcut;
                Type[Direction.left] = DispatcherType.dontHandle;
                Type[Direction.right] = DispatcherType.dontHandle;
                DirectionAction[Direction.down] = new Shortcut(
                    new List<int> { (int)Tools.Key.LeftControl },
                    new Dictionary<Direction, List<int>> {
                        { Direction.down, new List<int> { (int)Tools.Key.Tab } },
                        { Direction.up, new List<int> { (int)Tools.Key.LeftShift, (int)Tools.Key.Tab } }
                    }
                );
                DirectionAction[Direction.up] = new Shortcut(
                    new List<int> { (int)Tools.Key.LeftControl },
                    new Dictionary<Direction, List<int>> {
                        { Direction.down, new List<int> { (int)Tools.Key.Tab } },
                        { Direction.up, new List<int> { (int)Tools.Key.LeftShift, (int)Tools.Key.Tab } }
                    }
                );
            }
            else
            {
                throw new ArgumentException("今のとこdefaultしかないよ");
            }
        }
    }
}

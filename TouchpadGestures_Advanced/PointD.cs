using System;
using System.Collections.Generic;
using System.Text;

namespace TouchpadGestures_Advanced
{
    public class PointD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width {
            get { return X; }
            set { X = value; }
        }
        public double Height {
            get { return Y; }
            set { Y = value; }
        }
        public double Abs
        {
            get { return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)); }
        }
        public double ArgRad
        {
            get { return Math.Atan2(Y, X); }
        }
        public double ArgDeg
        {
            get { return Math.Atan2(Y, X) * 180 / Math.PI; }
        }
        public bool IsDirection(char direction)
        {
            if (Abs < Settings.ThresholdActive) return false;
            switch (direction)
            {
                case 'd':
                    return 90 - Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < 90 + Settings.ThresholdAngle / 2;
                case 'u':
                    return -90 - Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < -90 + Settings.ThresholdAngle / 2;
                case 'r':
                    return -Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < Settings.ThresholdAngle / 2;
                case 'l':
                    return 180 - Settings.ThresholdAngle / 2 < ArgDeg || ArgDeg < -180 + Settings.ThresholdAngle / 2;
                default:
                    return false;
            }
        }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static PointD operator +(PointD a, PointD b)
        {
            return new PointD(a.X + b.X, a.Y + b.Y);
        }
        public static PointD operator -(PointD a, PointD b)
        {
            return new PointD(a.X - b.X, a.Y - b.Y);
        }
        public static PointD operator +(PointD a)
        {
            return new PointD(a.X, a.Y);
        }
        public static PointD operator -(PointD a)
        {
            return new PointD(-a.X, -a.Y);
        }
        public static PointD operator *(PointD a, double b)
        {
            return new PointD(a.X * b, a.Y * b);
        }
        public static PointD operator *(double b, PointD a)
        {
            return new PointD(a.X * b, a.Y * b);
        }
        public static PointD operator /(PointD a, double b)
        {
            return new PointD(a.X / b, a.Y / b);
        }
        //public static bool operator ==(PointD a, PointD b)
        //{
        //    return a.X == b.X && a.Y == b.Y;
        //}
        //public static bool operator !=(PointD a, PointD b)
        //{
        //    return a.X != b.X || a.Y != b.Y;
        //}
    }
}

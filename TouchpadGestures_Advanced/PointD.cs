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
        public bool IsDirection(Direction direction)
        {
            if (Abs < App.Settings.ThresholdActive) return false;
            switch (direction)
            {
                case Direction.down:
                    return 90 - App.Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < 90 + App.Settings.ThresholdAngle / 2;
                case Direction.up:
                    return -90 - App.Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < -90 + App.Settings.ThresholdAngle / 2;
                case Direction.right:
                    return -App.Settings.ThresholdAngle / 2 < ArgDeg && ArgDeg < App.Settings.ThresholdAngle / 2;
                case Direction.left:
                    return 180 - App.Settings.ThresholdAngle / 2 < ArgDeg || ArgDeg < -180 + App.Settings.ThresholdAngle / 2;
                default:
                    return false;
            }
        }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public PointD(in PointD p)
        {
            X = p.X;
            Y = p.Y;
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
        public override string ToString()
        {
            return $"{X:F2} * {Y:F2}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchpadGestures_Advanced
{
    public static class KeyInput
    {
        private static HashSet<int> _Pressing = new HashSet<int>();
        private static HashSet<int> _ReleasedBuffer = new HashSet<int>();
        private static bool _IsTimerWorking = false;

        private static async void Timer()
        {
            while (_Pressing.Count > 0)
            {
                int _PressingCount = _Pressing.Count;
                int[] _array = new int[_PressingCount];
                _Pressing.CopyTo(_array);
                System.IntPtr ptr1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * _PressingCount);
                Marshal.Copy(_array, 0, ptr1, _PressingCount);
                NativeMethods.SendDirectKeyInput('d', ptr1, _PressingCount, IntPtr.Zero, 0);
                await Task.Delay(40);
            }
            _IsTimerWorking = false;
        }
        public static void Press(List<int> keys1)
        {
            System.IntPtr ptr1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * keys1.Count);
            Marshal.Copy(keys1.ToArray(), 0, ptr1, keys1.Count);
            NativeMethods.SendDirectKeyInput('p', ptr1, keys1.Count, IntPtr.Zero, 0);
        }
        public static async void Down(List<int> keys1)
        {
            foreach (var item in keys1)
            {
                _ReleasedBuffer.Remove(item);
            }
            System.IntPtr ptr1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * keys1.Count);
            Marshal.Copy(keys1.ToArray(), 0, ptr1, keys1.Count);
            NativeMethods.SendDirectKeyInput('d', ptr1, keys1.Count, IntPtr.Zero, 0);
            await Task.Delay(700);
            foreach (var item in keys1)
            {
                if (!_ReleasedBuffer.Remove(item))
                {
                    _Pressing.Add(item);
                }
            }
            if (!_IsTimerWorking && _Pressing.Count > 0)
            {
                _IsTimerWorking = true;
                Timer();
            }
        }
        public static void Up(List<int> keys1)
        {
            foreach (var item in keys1)
            {
                _Pressing.Remove(item);
                _ReleasedBuffer.Add(item);
            }
            System.IntPtr ptr1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * keys1.Count);
            Marshal.Copy(keys1.ToArray(), 0, ptr1, keys1.Count);
            NativeMethods.SendDirectKeyInput('u', ptr1, keys1.Count, IntPtr.Zero, 0);
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace TouchpadGestures_Advanced
{
    public class OverlayWindow : Window
    {

        #region DependencyProperties

        #region AltF4Cancel

        public bool AltF4Cancel
        {
            get { return (bool)GetValue(AltF4CancelProperty); }
            set { SetValue(AltF4CancelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AltF4Cancel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltF4CancelProperty =
            DependencyProperty.Register(
                "AltF4Cancel",
                typeof(bool),
                typeof(OverlayWindow),
                new PropertyMetadata(true));

        #endregion

        #region ShowSystemMenu

        public bool ShowSystemMenu
        {
            get { return (bool)GetValue(ShowSystemMenuProperty); }
            set { SetValue(ShowSystemMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowSystemMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowSystemMenuProperty =
            DependencyProperty.Register(
                "ShowSystemMenu",
                typeof(bool),
                typeof(OverlayWindow),
                new PropertyMetadata(
                    false,
                    ShowSystemMenuPropertyChanged));

        private static void ShowSystemMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d is OverlayWindow window)
            {

                window.SetShowSystemMenu((bool)e.NewValue);

            }

        }

        #endregion

        #region ClickThrough

        public bool ClickThrough
        {
            get { return (bool)GetValue(ClickThroughProperty); }
            set { SetValue(ClickThroughProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickThrough.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickThroughProperty =
            DependencyProperty.Register(
                "ClickThrough",
                typeof(bool),
                typeof(OverlayWindow),
                new PropertyMetadata(
                    true,
                    ClickThroughPropertyChanged));

        private static void ClickThroughPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d is OverlayWindow window)
            {

                window.SetClickThrough((bool)e.NewValue);

            }

        }

        #endregion

        #endregion

        #region const values

        private const int GWL_STYLE = (-16); // ウィンドウスタイル
        private const int GWL_EXSTYLE = (-20); // 拡張ウィンドウスタイル

        private const int WS_SYSMENU = 0x00080000; // システムメニュを表示する
        private const int WS_EX_TRANSPARENT = 0x00000020; // 透過ウィンドウスタイル

        private const int WM_SYSKEYDOWN = 0x0104; // Alt + 任意のキー の入力

        private const int VK_F4 = 0x73;

        #endregion

        #region Win32Apis

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);

        #endregion

        public OverlayWindow()
        {
            this.Visibility = Visibility.Hidden;
            // 透過背景
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Colors.Transparent);

            // 全画面表示
            this.WindowState = WindowState.Maximized;

            // 最前面表示
            this.Topmost = true;

            //タスクバーに表示しない
            this.ShowInTaskbar = false;

        }

        protected override void OnSourceInitialized(EventArgs e)
        {

            //システムメニュを非表示
            this.SetShowSystemMenu(this.ShowSystemMenu);

            //クリックをスルー
            this.SetClickThrough(this.ClickThrough);

            //Alt + F4 を無効化
            var handle = new WindowInteropHelper(this).Handle;
            var hwndSource = HwndSource.FromHwnd(handle);
            hwndSource.AddHook(WndProc);

            base.OnSourceInitialized(e);

        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr IParam, ref bool handled)
        {
            handled = false;
            //Alt + F4 が入力されたら
            if (msg == WM_SYSKEYDOWN && wParam.ToInt32() == VK_F4)
            {

                if (this.AltF4Cancel)
                {

                    //処理済みにセットする
                    //(Windowは閉じられなくなる)
                    handled = true;

                }

            }

            return IntPtr.Zero;

        }

        /// <summary>
        /// システムメニュの表示を切り替える
        /// </summary>
        /// <param name="value"><see langword="true"/> = 表示, <see langword="false"/> = 非表示</param>
        protected void SetShowSystemMenu(bool value)
        {

            try
            {

                var handle = new WindowInteropHelper(this).Handle;

                int windowStyle = GetWindowLong(handle, GWL_STYLE);

                if (value)
                {
                    windowStyle |= WS_SYSMENU; //フラグの追加
                }
                else
                {
                    windowStyle &= ~WS_SYSMENU; //フラグを消す
                }

                SetWindowLong(handle, GWL_STYLE, windowStyle);

            }
            catch
            {

            }

        }

        /// <summary>
        /// クリックスルーの設定
        /// </summary>
        /// <param name="value"><see langword="true"/> = クリックをスルー, <see langword="false"/>=クリックを捉える</param>
        protected void SetClickThrough(bool value)
        {

            try
            {

                var handle = new WindowInteropHelper(this).Handle;

                int extendStyle = GetWindowLong(handle, GWL_EXSTYLE);

                if (value)
                {
                    extendStyle |= WS_EX_TRANSPARENT; //フラグの追加
                }
                else
                {
                    extendStyle &= ~WS_EX_TRANSPARENT; //フラグを消す
                }

                SetWindowLong(handle, GWL_EXSTYLE, extendStyle);

            }
            catch
            {

            }

        }

    }
}



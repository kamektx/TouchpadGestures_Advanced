using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Security.RightsManagement;


namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static WindowInteropHelper _helper;
        internal void RawInputActivater(object sender, EventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(_helper.Handle);
            source.AddHook(ControlHost.WindowProc);
            NativeMethods.RegisterRawInput(_helper.Handle);
        }
        internal void OnContentRenderdTask(object sender, EventArgs e)
        {
            NativeMessaging.KilledNMC_Detector();
        }
        public MainWindow()
        {
            InitializeComponent();
            _helper = new WindowInteropHelper(this);
            ContentRendered += RawInputActivater;
            ContentRendered += OnContentRenderdTask;
        }
    }

}

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// TabWithImage.xaml の相互作用ロジック
    /// </summary>
    public partial class TabSmall : UserControl
    {
        public SendingObject.MyWindow.MyTab MyTab;
        public NMC_Manager MyNMC;
        public TabSmallData MyData;
        public StackPanel MySP;
        public BitmapImage MyFaviconSource;
        public TabSmall(NMC_Manager myNMC, SendingObject.MyWindow.MyTab myTab, StackPanel sp)
        {
            this.MyNMC = myNMC;
            this.MyTab = myTab;
            this.MySP = sp;
            this.MyData = new TabSmallData();
            this.DataContext = this.MyData;
            InitializeComponent();
            if (MyTab.IsActive.HasValue && MyTab.IsActive.Value)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4a4d8eff"));
            }
            this.Width = sp.Width;
            this.MyFaviconSource = new BitmapImage();
            this.MyFaviconSource.BeginInit();
            if (MyTab.Favicon != null && File.Exists(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png"))
            {
                this.MyFaviconSource.UriSource = new Uri(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png");
            }
            else
            {
                this.MyFaviconSource.UriSource = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Icon\\firefox.png");
            }
            this.MyFaviconSource.EndInit();
            this.MyTitle.Text = myTab.Title ?? "";
            this.MyFavicon.Source = this.MyFaviconSource;
        }
    }
    public class TabSmallData
    {
        public int MyBorderThickness { get; set; } = 2;
        public int MyBorderPadding { get; set; } = 5;
        public int FaviconGridWidthAndHeight { get; set; } = 28;
        public int FaviconWidthAndHeight { get; set; } = 24;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}

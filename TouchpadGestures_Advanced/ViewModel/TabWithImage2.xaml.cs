using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// TabWithImage.xaml の相互作用ロジック
    /// </summary>
    public partial class TabWithImage2 : UserControl
    {
        public SendingObject.MyWindow.MyTab MyTab;
        public NMC_Manager MyNMC;
        public TabWithImage2Data MyData;
        public StackPanel MySP;
        public BitmapImage MyImageSource;
        public BitmapImage MyFaviconSource;
        public TabWithImage2(NMC_Manager myNMC, SendingObject.MyWindow.MyTab myTab, StackPanel sp)
        {
            this.MyNMC = myNMC;
            this.MyTab = myTab;
            this.MySP = sp;
            this.MyData = new TabWithImage2Data();
            this.DataContext = this.MyData;
            InitializeComponent();
            this.Width = sp.Width;
            this.MyImageSource = new BitmapImage();
            this.MyFaviconSource = new BitmapImage();
            this.MyImageSource.BeginInit();
            this.MyFaviconSource.BeginInit();
            if (MyTab.ScreenShot != null && File.Exists(MyNMC.MyAppData + @"\screenshot\" + MyTab.ScreenShot))
            {
                this.MyImageSource.UriSource = new Uri(MyNMC.MyAppData + @"\screenshot\" + MyTab.ScreenShot);
            }
            else
            {
                this.MyImageSource.UriSource = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Image\\firefox.png");
            }
            if (MyTab.Favicon != null && File.Exists(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png"))
            {
                this.MyFaviconSource.UriSource = new Uri(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png");
            }
            else
            {
                this.MyFaviconSource.UriSource = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Icon\\firefox.png");
            }
            this.MyImageSource.EndInit();
            this.MyFaviconSource.EndInit();
            this.MyTitle.Text = myTab.Title ?? "";
            this.MyImage.Source = this.MyImageSource;
            this.MyFavicon.Source = this.MyFaviconSource;
            this.MyImage.Height = (this.Width - 2 * MyData.MyBorderThickness - 2 * MyData.MyBorderPadding) * MyImageSource.PixelHeight / MyImageSource.PixelWidth;
        }
    }
    public class TabWithImage2Data
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

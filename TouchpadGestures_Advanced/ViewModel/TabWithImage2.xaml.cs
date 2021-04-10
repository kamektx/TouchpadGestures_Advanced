using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// TabWithImage.xaml の相互作用ロジック
    /// </summary>
    public partial class TabWithImage2 : TabCommon
    {
        public TabWithImage2Data MyData;
        public BitmapImage MyImageSource;
        public BitmapImage MyFaviconSource;
        static int DecodePixelWidth = 300;
        public TabWithImage2(NMC_Manager myNMC, SendingObject.MyWindow.MyTab myTab, StackPanel sp, StackPanel wrapperSP, ForBrowser forBrowser, int rowIndex, int columnIndex, int tabIndex, int columnsIndex)
        : base(myNMC, myTab, sp, wrapperSP, forBrowser, rowIndex, columnIndex, tabIndex, columnsIndex)
        {
            this.MyData = new TabWithImage2Data();
            this.DataContext = this.MyData;
            InitializeComponent();
            this.Width = sp.Width;
            this.MyImageSource = BitmapImageExtension.MyInit(MyTab.ScreenShot != null ? new Uri(MyNMC.MyAppData + @"\screenshot\" + MyTab.ScreenShot) : null, DefaultImageSourceUri, DecodePixelWidth);
            this.MyFaviconSource = BitmapImageExtension.MyInit(MyTab.Favicon != null ? new Uri(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png") : null, DefaultFaviconSourceUri);

            this.MyTitle.Text = myTab.Title ?? "";
            this.MyImage.Source = this.MyImageSource;
            this.MyFavicon.Source = this.MyFaviconSource;
            this.MyImage.Height = (this.Width - 2 * MyData.MyBorderThickness - 2 * MyData.MyBorderPadding) * MyImageSource.PixelHeight / MyImageSource.PixelWidth;
        }
    }
    public class TabWithImage2Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public int MyBorderThickness { get; set; } = 2;
        public int MyBorderPadding { get; set; } = 5;
        public int FaviconGridWidthAndHeight { get; set; } = 28;
        public int FaviconWidthAndHeight { get; set; } = 24;
    }
}

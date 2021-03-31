using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// TabWithImage.xaml の相互作用ロジック
    /// </summary>
    public partial class TabSmall : TabCommon
    {
        public TabSmallData MyData;
        public BitmapImage MyFaviconSource;
        public TabSmall(NMC_Manager myNMC, SendingObject.MyWindow.MyTab myTab, StackPanel sp, StackPanel wrapperSP, ForBrowser forBrowser, int rowIndex, int columnIndex, int tabIndex, int columnsIndex)
        :base(myNMC, myTab, sp, wrapperSP, forBrowser, rowIndex, columnIndex, tabIndex, columnsIndex)
        {
            this.MyData = new TabSmallData();
            this.DataContext = this.MyData;
            InitializeComponent();
            if (MyTab.IsActive)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4a4d8eff"));
            }
            this.Width = sp.Width;
            this.MyFaviconSource = BitmapImageExtension.MyInit(MyTab.Favicon != null ? new Uri(MyNMC.MyAppData + @"\favicon\png\" + MyTab.Favicon + @".png") : null, DefaultFaviconSourceUri);
            this.MyFavicon.Source = this.MyFaviconSource;
            this.MyTitle.Text = myTab.Title ?? "";
        }
    }
    public class TabSmallData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public int MyBorderThickness { get; set; } = 2;
        public int MyBorderPadding { get; set; } = 5;
        public int FaviconGridWidth { get; set; } = 28;
        public int FaviconGridHeight { get; set; } = 24;
        public int FaviconWidthAndHeight { get; set; } = 24;
    }
}

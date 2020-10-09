using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    /// <summary>
    /// ForBrowser.xaml の相互作用ロジック
    /// </summary>
    public partial class ForBrowser : OverlayWindow
    {
        public static readonly int MaxColumns = 20;
        public static readonly int ItemBigMinWidth = 220;
        public static readonly int ItemBigMaxWidth = 320;
        public static readonly int ItemSmallDefaultWidth = 300;
        public NMC_Manager MyNMC;
        public List<StackPanel> SPs;
        public TabWithImage2Data MyTabWithImage2Data;
        public void Refresh()
        {
            SendingObject s = MyNMC.SendingObject;
            WrapperSP.Children.Clear();
            if (s.ActiveWindowID.HasValue)
            {
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#c5000000"));
                SPs.Clear();
                var w = s.Windows[s.ActiveWindowID.Value];
                foreach (var columnSetting in s.Arrangements.Down.Column)
                {
                    switch (columnSetting.Size)
                    {
                        case "big":
                            {
                                List<int> target;
                                switch (columnSetting.Type)
                                {
                                    case "RecentTabs":
                                        target = w.RecentTabs;
                                        break;
                                    case "TabsInOrder":
                                        target = w.TabsInOrder;
                                        break;
                                    default:
                                        throw new Exception("ColumnSetting.Type is invalid.");
                                }
                                if (target.Count == 0 || columnSetting.MaxColumns == 0)
                                {
                                    continue;
                                }
                                int maxColumns = columnSetting.MaxColumns < 0 ? MaxColumns : columnSetting.MaxColumns;
                                int maxRows = App.MaxRowsOfTabWithImage;
                                int columns = 0;
                                int rows = 0;
                                for (int i = 1; i <= maxRows; i++)
                                {
                                    if (target.Count <= i * (i - 1))
                                    {
                                        columns = i - 1;
                                        rows = i;
                                        break;
                                    }
                                    else if (maxColumns < i)
                                    {
                                        columns = i - 1;
                                        rows = Math.Min((int)Math.Ceiling((double)target.Count / columns), maxRows);
                                        break;
                                    }
                                    else if (target.Count <= i * i)
                                    {
                                        columns = i;
                                        rows = i;
                                        break;
                                    }
                                    if (i == maxRows)
                                    {
                                        columns = Math.Min((int)Math.Ceiling((double)target.Count / maxRows), maxColumns);
                                        rows = maxRows;
                                    }
                                }
                                var sampleImageSource = new BitmapImage();
                                sampleImageSource.BeginInit();
                                if (w.LastCapturedTab.HasValue && w.Tabs[w.LastCapturedTab.Value]?.ScreenShot != null && File.Exists(MyNMC.MyAppData + @"\screenshot\" + w.Tabs[w.LastCapturedTab.Value].ScreenShot))
                                {
                                    sampleImageSource.UriSource = new Uri(MyNMC.MyAppData + @"\screenshot\" + w.Tabs[w.LastCapturedTab.Value].ScreenShot);
                                }
                                else
                                {
                                    sampleImageSource.UriSource = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Image\\dummy_16x9.jpg");
                                }
                                sampleImageSource.EndInit();
                                int sampleWidth = ItemBigMaxWidth;
                                double sampleActualHeight = (sampleWidth - MyTabWithImage2Data.MyBorderThickness * 2 - MyTabWithImage2Data.MyBorderPadding * 2) * sampleImageSource.PixelHeight / sampleImageSource.PixelWidth + MyTabWithImage2Data.FaviconGridWidthAndHeight + MyTabWithImage2Data.MyBorderThickness * 2 + MyTabWithImage2Data.MyBorderPadding * 2;

                                int itemMaxHeight = App.ForBrowserMaxHeight / rows;
                                int calculatedWidth = ItemBigMaxWidth;
                                if (sampleActualHeight > itemMaxHeight)
                                {
                                    calculatedWidth = (int)(ItemBigMaxWidth * itemMaxHeight / sampleActualHeight);
                                    if (calculatedWidth < ItemBigMinWidth)
                                        calculatedWidth = ItemBigMinWidth;
                                }

                                int tabCount = 0;
                                for (int i = 0; i < columns; i++)
                                {
                                    int spIndex = SPs.Count;
                                    var mySP = new StackPanel();
                                    mySP.Width = calculatedWidth;
                                    var columnLabel = new TextBlock();
                                    columnLabel.Style = (Style)this.Resources["MyLabel"];
                                    columnLabel.Text = columnSetting.Type + ":";
                                    mySP.Children.Add(columnLabel);
                                    SPs.Add(mySP);
                                    for (int j = 0; j < rows; j++)
                                    {
                                        if (tabCount < target.Count)
                                        {
                                            var item = new TabWithImage2(this.MyNMC, w.Tabs[target[tabCount]], mySP);
                                            item.MaxHeight = itemMaxHeight;
                                            mySP.Children.Add(item);
                                            tabCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    WrapperSP.Children.Add(mySP);
                                }
                                break;
                            }
                        case "small":
                            {
                                List<int> target;
                                switch (columnSetting.Type)
                                {
                                    case "RecentTabs":
                                        target = w.RecentTabs;
                                        break;
                                    case "TabsInOrder":
                                        target = w.TabsInOrder;
                                        break;
                                    default:
                                        throw new Exception("ColumnSetting.Type is invalid.");
                                }
                                if (target.Count == 0 || columnSetting.MaxColumns == 0)
                                {
                                    continue;
                                }
                                int maxColumns = columnSetting.MaxColumns < 0 ? MaxColumns : columnSetting.MaxColumns;
                                double sampleActualHeight = MyTabWithImage2Data.FaviconGridWidthAndHeight + MyTabWithImage2Data.MyBorderThickness + MyTabWithImage2Data.MyBorderPadding;
                                int rows = (int)Math.Floor(App.ForBrowserMaxHeight / sampleActualHeight);
                                int columns = (int)Math.Ceiling((double)target.Count / rows);
                                if (columns > maxColumns)
                                {
                                    columns = maxColumns;
                                }
                                int calculatedWidth = ItemSmallDefaultWidth;
                                int tabCount = 0;
                                for (int i = 0; i < columns; i++)
                                {
                                    int spIndex = SPs.Count;
                                    var mySP = new StackPanel();
                                    mySP.Width = calculatedWidth;
                                    var columnLabel = new TextBlock();
                                    columnLabel.Style = (Style)this.Resources["MyLabel"];
                                    columnLabel.Text = columnSetting.Type + ":";
                                    mySP.Children.Add(columnLabel);
                                    SPs.Add(mySP);
                                    for (int j = 0; j < rows; j++)
                                    {
                                        if (tabCount < target.Count)
                                        {
                                            var item = new TabSmall(this.MyNMC, w.Tabs[target[tabCount]], mySP);
                                            mySP.Children.Add(item);
                                            tabCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    WrapperSP.Children.Add(mySP);
                                }
                                break;
                            }
                    }
                }
            }
            else
            {
                Background = Brushes.Transparent;
            }

        }
        public ForBrowser(NMC_Manager nMC_Magager) : base()
        {
            Visibility = Visibility.Visible;
            SPs = new List<StackPanel>();
            MyNMC = nMC_Magager;
            MyTabWithImage2Data = new TabWithImage2Data();
            InitializeComponent();
        }
    }
    public class ForBrowserData : INotifyPropertyChanged
    {
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

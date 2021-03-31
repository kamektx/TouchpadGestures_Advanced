using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    public enum TabSize
    {
        big, small
    }
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
        public List<Dictionary<int, TabCommon>> ColumnsIndexVsTabIndexVsTabCommon;
        public List<Dictionary<int, TabCommon>> ColumnIndexVsRowIndexVsTabCommon;
        public TabWithImage2Data MyTabWithImage2Data { get; set; }
        public TabSmallData MyTabSmallData { get; set; }
        public ForBrowserData MyData { get; set; }
        public Direction MyDirection;
        public List<double> ColumnIndexVsHorizontalBoundary;
        public List<List<double>> ColumnIndexVsRowIndexVsVerticalBoundary;
        public List<int> ColumnsIndexVsActiveTabIndex;
        public List<TabSize> ColumnIndexVsTabSize;

        public void Refresh()
        {
            MyNMC.MySemaphore.Wait();
            MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(0, 0);
            var s = MyNMC.SendingObject;
            WrapperSP.Children.Clear();
            if (MyNMC.IsActive)
            {
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#c5000000"));
                SPs.Clear();
                ColumnsIndexVsTabIndexVsTabCommon.Clear();
                ColumnIndexVsRowIndexVsTabCommon.Clear();
                ColumnsIndexVsActiveTabIndex.Clear();
                ColumnIndexVsTabSize.Clear();
                var w = s.Windows[s.ActiveWindowID.Value];
                var arr = s.Arrangements[MyDirection];
                int columnIndexTemp = 0;
                foreach (var columnSetting in arr.Column)
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
                    ColumnsIndexVsActiveTabIndex.Add(-1);
                    ColumnsIndexVsTabIndexVsTabCommon.Add(new Dictionary<int, TabCommon>());
                    switch (columnSetting.Size)
                    {
                        case "big":
                            {
                                int maxColumns = columnSetting.MaxColumns < 0 ? MaxColumns : columnSetting.MaxColumns;
                                int maxRows = Status.MaxRowsOfTabWithImage;
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
                                var sampleImageSource = BitmapImageExtension.MyInit((w.LastCapturedTab.HasValue && w.Tabs[w.LastCapturedTab.Value]?.ScreenShot != null) ? new Uri(MyNMC.MyAppData + @"\screenshot\" + w.Tabs[w.LastCapturedTab.Value].ScreenShot) : null, TabCommon.StaticImageSourceUri);

                                int sampleWidth = ItemBigMaxWidth;
                                double sampleActualHeight = (sampleWidth - MyTabWithImage2Data.MyBorderThickness * 2 - MyTabWithImage2Data.MyBorderPadding * 2) * sampleImageSource.PixelHeight / sampleImageSource.PixelWidth + MyTabWithImage2Data.FaviconGridWidthAndHeight + MyTabWithImage2Data.MyBorderThickness * 2 + MyTabWithImage2Data.MyBorderPadding * 2;

                                int itemMaxHeight = Status.ForBrowserMaxHeight / rows;
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
                                    ColumnIndexVsRowIndexVsTabCommon.Add(new Dictionary<int, TabCommon>());
                                    ColumnIndexVsTabSize.Add(TabSize.big);
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
                                            var columnsIndex = ColumnsIndexVsActiveTabIndex.Count - 1;
                                            var columnIndex = columnIndexTemp + i;
                                            var item = new TabWithImage2(this.MyNMC, w.Tabs[target[tabCount]], mySP, WrapperSP, this, j, columnIndex, tabCount, columnsIndex);
                                            if (arr.StartPosition.Column == columnsIndex && arr.StartPosition.Index == tabCount)
                                            {
                                                MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(columnIndex, j);
                                            }
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
                                columnIndexTemp += columns;
                                break;
                            }
                        case "small":
                            {
                                int maxColumns = columnSetting.MaxColumns < 0 ? MaxColumns : columnSetting.MaxColumns;
                                double sampleActualHeight = MyTabSmallData.FaviconGridHeight * 2 + MyTabSmallData.MyBorderThickness * 2 + MyTabSmallData.MyBorderPadding * 2;
                                int rows = (int)Math.Floor(Status.ForBrowserMaxHeight / sampleActualHeight);
                                int columns = (int)Math.Ceiling((double)target.Count / rows);
                                if (columns > maxColumns)
                                {
                                    columns = maxColumns;
                                }
                                int calculatedWidth = ItemSmallDefaultWidth;
                                int tabCount = 0;
                                for (int i = 0; i < columns; i++)
                                {
                                    ColumnIndexVsRowIndexVsTabCommon.Add(new Dictionary<int, TabCommon>());
                                    ColumnIndexVsTabSize.Add(TabSize.small);
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
                                            var columnsIndex = ColumnsIndexVsActiveTabIndex.Count - 1;
                                            var columnIndex = columnIndexTemp + i;
                                            var item = new TabSmall(this.MyNMC, w.Tabs[target[tabCount]], mySP, WrapperSP, this, j, columnIndex, tabCount, columnsIndex);
                                            if (arr.StartPosition.Column == columnsIndex && arr.StartPosition.Index == tabCount)
                                            {
                                                MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(columnIndex, j);
                                            }
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
                                columnIndexTemp += columns;
                                break;
                            }
                    }
                }
            }
            else
            {
                Background = Brushes.Transparent;
            }
            MyNMC.MySemaphore.Release();
        }

        public void MakeVisible()
        {
            Visibility = Visibility.Visible;
            ColumnIndexVsHorizontalBoundary.Clear();
            ColumnIndexVsRowIndexVsVerticalBoundary.Clear();
            this.Dispatcher.Invoke(() =>
            {
                {
                    ColumnIndexVsHorizontalBoundary.Add(0);
                    int i = 0;
                    foreach (var item in SPs)
                    {
                        ColumnIndexVsHorizontalBoundary.Add(ColumnIndexVsHorizontalBoundary[i] + item.ActualWidth);
                        i++;
                    }
                }
                {
                    int i = 0;
                    foreach (var rowIndexVsTabCommon in ColumnIndexVsRowIndexVsTabCommon)
                    {
                        ColumnIndexVsRowIndexVsVerticalBoundary.Add(new List<double> { 0 });
                        int j = 0;
                        foreach (var tabCommon in rowIndexVsTabCommon.Values)
                        {
                            ColumnIndexVsRowIndexVsVerticalBoundary[i].Add(ColumnIndexVsRowIndexVsVerticalBoundary[i][j] + tabCommon.ActualHeight);
                            j++;
                        }
                        i++;
                    }
                }
            });
        }

        public void MakeHidden()
        {
            Visibility = Visibility.Hidden;
        }

        public ForBrowser(NMC_Manager nMC_Magager, Direction direction) : base()
        {
            Visibility = Visibility.Hidden;
            MyDirection = direction;
            SPs = new List<StackPanel>();
            ColumnIndexVsHorizontalBoundary = new List<double>();
            ColumnIndexVsRowIndexVsVerticalBoundary = new List<List<double>>();
            ColumnsIndexVsTabIndexVsTabCommon = new List<Dictionary<int, TabCommon>>();
            ColumnIndexVsRowIndexVsTabCommon = new List<Dictionary<int, TabCommon>>();
            ColumnsIndexVsActiveTabIndex = new List<int>();
            ColumnIndexVsTabSize = new List<TabSize>();
            MyNMC = nMC_Magager;
            MyTabWithImage2Data = new TabWithImage2Data();
            MyTabSmallData = new TabSmallData();
            MyData = new ForBrowserData();
            MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(1, 0);
            InitializeComponent();
        }
    }
    public class ForBrowserData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private KeyValuePair<int, int> _ColumnIndexAndRowIndexOfSelectedTab;

        public KeyValuePair<int, int> ColumnIndexAndRowIndexOfSelectedTab
        {
            get => _ColumnIndexAndRowIndexOfSelectedTab;
            set
            {
                if (_ColumnIndexAndRowIndexOfSelectedTab.Equals(value)) return;
                _ColumnIndexAndRowIndexOfSelectedTab = value;
                RaisePropertyChanged();
            }
        }
    }
}

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
using System.Windows.Threading;

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
        public static readonly int ItemSmallDefaultWidth = 290;
        //MaxRowsOfTabWithImage(Max size of TabWithImage2) is determined in "Status" class.
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
        public bool isOverFlow = false;

        public void Refresh()
        {
            RefreshDirectionSpecified(MyDirection);
        }

        public void RefreshDirectionSpecified(Direction dir)
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
                var arr = s.Arrangements[dir];
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
                                double sampleActualHeight = MyTabSmallData.FaviconGridHeight + MyTabSmallData.MyBorderThickness * 2 + MyTabSmallData.MyBorderPadding * 2;
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
            MyNMC.MySemaphore.Release();
        }

        public void MakeVisible()
        {
            //Visibility = Visibility.Visible;
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
                {
                    this.Top = (Status.PrimaryScreenHeight - this.ActualHeight) / 2.0;
                    double leftTemp;
                    if (this.ActualWidth < Status.ForBrowserMaxWidth)
                    {
                        leftTemp = (Status.PrimaryScreenWidth - this.ActualWidth) / 2.0;
                        isOverFlow = false;
                    }
                    else
                    {
                        leftTemp = Status.MinimumHorizontalPadding;
                        isOverFlow = true;
                    }
                    this.Left = leftTemp;
                }
            });
            Opacity = 1.0;
        }

        public void MakeHidden()
        {
            #if DEBUG
            Opacity = 0.1;
            #endif
            #if RELEASE
            Opacity = 0.0;
            #endif
            //Visibility = Visibility.Hidden;
        }
        public void OverFlowHandling(int columnIndex)
        {
            if (columnIndex == ColumnIndexVsRowIndexVsTabCommon.Count - 1)
            {
                this.Left = Status.PrimaryScreenWidth - Status.MinimumHorizontalPadding - this.ActualWidth;
                return;
            }
            if (columnIndex == 0)
            {
                this.Left = Status.MinimumHorizontalPadding;
                return;
            }
            if (this.Left + MyData.WindowPadding + ColumnIndexVsHorizontalBoundary[columnIndex + 1] > Status.PrimaryScreenWidth - Status.WidthForRemainingColumns)
            {
                this.Left = Status.PrimaryScreenWidth - Status.WidthForRemainingColumns - MyData.WindowPadding - ColumnIndexVsHorizontalBoundary[columnIndex + 1];
                return;
            }
            if (this.Left + MyData.WindowPadding + ColumnIndexVsHorizontalBoundary[columnIndex] < Status.WidthForRemainingColumns)
            {
                this.Left = Status.WidthForRemainingColumns - MyData.WindowPadding - ColumnIndexVsHorizontalBoundary[columnIndex];
                return;
            }
        }

        public ForBrowser(NMC_Manager nMC_Magager, Direction direction) : base()
        {
            Visibility = Visibility.Visible;
            MakeHidden();
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
            MyData = new ForBrowserData(this);
            MyData.ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(1, 0);
            DataContext = MyData;
            InitializeComponent();
        }
    }
    public class ForBrowserData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int WindowPadding { get; set; } = 20;
        private ForBrowser MyForBrowser;

        private KeyValuePair<int, int> _ColumnIndexAndRowIndexOfSelectedTab = new KeyValuePair<int, int>(0,0);

        public KeyValuePair<int, int> ColumnIndexAndRowIndexOfSelectedTab
        {
            get
            {
                return _ColumnIndexAndRowIndexOfSelectedTab;
            }
            set
            {
                if (_ColumnIndexAndRowIndexOfSelectedTab.Equals(value)) return;
                _ColumnIndexAndRowIndexOfSelectedTab = value;

                if (MyForBrowser.isOverFlow)
                {
                    Action<int> overFlowHandling = MyForBrowser.OverFlowHandling;
                    MyForBrowser.Dispatcher.BeginInvoke(overFlowHandling, _ColumnIndexAndRowIndexOfSelectedTab.Key);
                }

                RaisePropertyChanged();
            }
        }

        public ForBrowserData(ForBrowser forBrowser)
        {
            MyForBrowser = forBrowser;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    public class TabCommon : UserControl
    {
        public SendingObject.MyWindow.MyTab MyTab;
        public NMC_Manager MyNMC;
        public StackPanel MySP;
        public StackPanel MyWrapperSP;
        public ForBrowser ForBrowserWindow;
        public int RowIndex;
        public int ColumnIndex;
        public int MyTabIndex;
        public int ColumnsIndex;
        protected Uri DefaultImageSourceUri = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Image\\firefox.png");
        public static Uri StaticImageSourceUri = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Image\\firefox.png");
        protected Uri DefaultFaviconSourceUri = new Uri("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\Icon\\firefox.png");
        public TabCommon(NMC_Manager myNMC, SendingObject.MyWindow.MyTab myTab, StackPanel sp, StackPanel wrapperSP, ForBrowser forBrowser, int rowIndex, int columnIndex, int tabIndex, int columnsIndex)
        {
            this.MyNMC = myNMC;
            this.MyTab = myTab;
            this.MySP = sp;
            this.MyWrapperSP = wrapperSP;
            this.ForBrowserWindow = forBrowser;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.MyTabIndex = tabIndex;
            this.ColumnsIndex = columnsIndex;

            if (MyTab.IsActive)
            {
                ForBrowserWindow.ColumnsIndexVsActiveTabIndex[ColumnsIndex] = MyTabIndex;
            }
            ForBrowserWindow.ColumnsIndexVsTabIndexVsTabCommon[ColumnsIndex][MyTabIndex] = this;
            ForBrowserWindow.ColumnIndexVsRowIndexVsTabCommon[ColumnIndex][RowIndex] = this;
        }
        public TabCommon() { }
    }
}

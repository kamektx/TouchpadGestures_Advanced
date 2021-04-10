using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace TouchpadGestures_Advanced
{
    static class BitmapImageExtension
    {
        public static BitmapImage MyInit(Uri preferred, Uri alternative, int decodePixelWidth = 0, int retry = 2)
        {
            BitmapImage bi = new BitmapImage();
            if (preferred != null && File.Exists(preferred.OriginalString))
            {
                bool isInited = false;
                for (int i = retry; i > 0; i--)
                {
                    try
                    {
                        bi = new BitmapImage();
                        bi.BeginInit();
                        if (decodePixelWidth > 0) bi.DecodePixelWidth = decodePixelWidth;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.UriSource = preferred;
                        bi.EndInit();
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    isInited = true;
                    break;
                }
                if (!isInited)
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    if (decodePixelWidth > 0) bi.DecodePixelWidth = decodePixelWidth;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = alternative;
                    bi.EndInit();
                }
            }
            else
            {
                bi = new BitmapImage();
                bi.BeginInit();
                if (decodePixelWidth > 0) bi.DecodePixelWidth = decodePixelWidth;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = alternative;
                bi.EndInit();
            }
            return bi;
        }
    }
}

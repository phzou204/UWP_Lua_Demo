using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ToolsLib
{
    public class Utility
    {
        /// <summary>
        /// 从字节数组获取BitmapImage
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<BitmapImage> GetBitmapImageAsync(byte[] bytes)
        {
            BitmapImage bitmapImage = null;
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    IRandomAccessStream randomAccessStream = await ToolsLib.StreamConvert.ConvertAsync(ms);
                    bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(randomAccessStream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Get BitmapImage error!:" + e.Message);
                bitmapImage = null;
            }            
            return bitmapImage;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ToolsLib
{
    public class StreamConvert
    {
        /// <summary>
        /// MemoryStream转IRandomAccessStream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<IRandomAccessStream> ConvertAsync(MemoryStream stream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(stream.ToArray()));
            task.Start();
            await task;
            await dw.StoreAsync();
            var success = await outputStream.FlushAsync();
            return randomAccessStream;
        }
    }
}

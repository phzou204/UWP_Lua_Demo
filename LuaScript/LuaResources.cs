using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;

namespace RYTong.LuaScript
{
    /// <summary>
    /// 暂时无用
    /// </summary>
    public class LuaResources
    {
        private LuaResources() { }

        /// <summary>
        /// LuaResources.getResources(name,type)
        /// </summary>
        static int getResources( )
        {
            //string fileName = Lua.Lua_tostring( 1).ToString();
            //string fileType = Lua.Lua_tostring( 2).ToString();

            //if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileType))
            //{
            //    switch (fileType)
            //    {
            //        case "text":
            //            string content = ReadFileContent(fileName);
            //            if (string.IsNullOrEmpty(content))
            //            {
            //                return 0;
            //            }
            //            else
            //            {
            //                Lua.Lua_pushstring(L,  content);
            //                return 1;
            //            }

            //        case "image":
            //            var bi = ReadImageFile(fileName);
            //            Lua.Lua_pushlightuserdata(L, bi);
            //            return 1;

            //        case "stream":
            //            var stream = ReadImageFile(fileName);
            //            Lua.Lua_pushlightuserdata(L, stream);
            //            return 1;
            //    }
            //}

            return 0;
        }

        //private static Lua.luaL_Reg[] luaResources_libs = { new Lua.luaL_Reg("getResources", getResources) };

        //public static int luaopen_resourcesLibs( )
        //{
        //    Lua.luaL_register(lua, "resources", luaResources_libs);
        //    return 1;
        //}

        #region DB - Save / Read Stream File

        public static void SaveImageFile(Stream imageStream, string fileName)
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorage.FileExists(fileName))
                    isolatedStorage.DeleteFile(fileName);

                IsolatedStorageFileStream fileStream = isolatedStorage.CreateFile(fileName);
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(imageStream);

                WriteableBitmap wb = new WriteableBitmap(bitmap);
                wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                fileStream.Close();
            }
        }

        public static BitmapImage ReadImageFile(string fileName)
        {
            BitmapImage bi = new BitmapImage();

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    bi.SetSource(fileStream);
                }
            }

            return bi;
        }

        public static void SaveStreamFile(Stream stream, string fileName)
        {
            using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(fileName))
                    myIsolatedStorage.DeleteFile(fileName);

                using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(fileName, FileMode.Create, myIsolatedStorage))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        long length = stream.Length;
                        byte[] buffer = new byte[32];
                        int readCount = 0;
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            // read file in chunks in order to reduce memory consumption and increase performance
                            while (readCount < length)
                            {
                                int actual = reader.Read(buffer, 0, buffer.Length);
                                readCount += actual;
                                writer.Write(buffer, 0, actual);
                            }
                        }
                    }
                }
            }
        }

        public static MediaElement ReadMediaElementFile(string fileName)
        {
            MediaElement mediaElement = new MediaElement();
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    mediaElement.SetSource(fileStream);
                }
            }

            return mediaElement;
        }

        public static string ReadFileContent(string fileName)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(fileName))
                {
                    IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion
    }
}

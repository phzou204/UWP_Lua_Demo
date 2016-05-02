//
//  LuaFile
//  RYTong
//
//  Created by wu.dong on 5/16/12.
//  Copyright 2012 RYTong. All rights reserved.
//

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
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    /// <summary>
    /// 读取文件的优先级顺序为：插件资源 > 离线下载资源 > file：write写出的资源 > 安装包资源。 
    /// </summary>
    public class LuaFile
    {
        private LuaFile() { }

        public static void SetCplusDelegates()
        {
            LuaFileDelegates.write = new writeDel(write);
            LuaFileDelegates.read = new readDel(read);
            LuaFileDelegates.remove = new removeDel(remove);
            LuaFileDelegates.isExist = new isExistDel(isExist);
            LuaFileDelegates.readH5 = new readH5Del(readH5);
        }

        static int write(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            String fileName = Lua.Lua_tostring(L, 2).ToString();
            Object data = null;
            if (Lua.Lua_isstring(L, 3))
            {
                data = Lua.Lua_tostring(L, 3);
            }
            else if (Lua.Lua_isuserdata(L, 3))
            {
                data = Lua.Lua_touserdata(L,3);
            }
            if (!string.IsNullOrEmpty(fileName) && data != null)
            {
                RYTFile.WriteFile(fileName, data, null);
            }

            return 0;
        }

        /// <summary>
        /// 读取文件的优先级顺序为：插件资源 > 离线下载资源 > file：write写出的资源 > 安装包资源。 
        /// </summary>
        static int read(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            String name = Lua.Lua_tostring(L, 2).ToString();
            String type = Lua.Lua_tostring(L, 3).ToString();

            var result = RYTFile.ReadFileByType(name, type);
            if (result != null)
            {
                if (type.Equals("text", StringComparison.CurrentCultureIgnoreCase))
                {
                    Lua.Lua_pushstring(L, result.ToString());
                }
                else
                {
                    Lua.Lua_pushlightuserdata(L,result);
                }
                return 1;
            }

            Lua.Lua_pushnil(L);
            return 1;
        }

        static int remove(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            String fileName = Lua.Lua_tostring(L, 2).ToString();
            RYTFile.Remove(fileName, null, null);
            return 0;
        }

        /// <summary>
        /// 测文件的优先级顺序为：插件资源 > 离线下载资源 > file：write写出的资源 > 安装包资源。 
        /// </summary>
        static int isExist(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushboolean(L, 0);
                return 0;
            }

            var fileName = Lua.Lua_tostring(L, 2).ToString();
            if (RYTFile.IsFileExist(fileName))
            {
                Lua.Lua_pushboolean(L, 1);
            }
            else
            {
                Lua.Lua_pushboolean(L, 0);
            }
            return 1;
        }


        private static int readH5(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            String name = Lua.Lua_tostring(L, 2).ToString();
            String type = Lua.Lua_tostring(L, 3).ToString();

            var result = RYTFile.ReadH5FileByType(name, type);
            if (result != null)
            {
                if (type.Equals("text", StringComparison.CurrentCultureIgnoreCase))
                {
                    Lua.Lua_pushstring(L, result.ToString());
                }
                else
                {
                    Lua.Lua_pushlightuserdata(L,result);
                }
                return 1;
            }

            Lua.Lua_pushnil(L);
            return 1;
        }
    }
}

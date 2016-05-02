//
//  LuaScreen
//  RYTong
//
//  Created by wu.dong on 2/22/12.
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
using System.Reflection;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaScreen
    {
        private LuaScreen() { }

        public static void SetCplus_Delegates()
        {
            LuaScreenDelegates.height = new heightDel(height);
            LuaScreenDelegates.width = new widthDel(width);
            LuaScreenDelegates.clientHeight = new clientHeightDel(clientHeight);
            LuaScreenDelegates.clientWidth = new clientWidthDel(clientWidth);
            LuaScreenDelegates.dpi = new dpiDel(dpi);
        }

        static int width(int L)
        {
            Lua.Lua_pushnumber(L, RYTong.FunctionLib.RYTScreen.ScreenWidth);

            return 1;
        }

        static int height(int L)
        {
            Lua.Lua_pushnumber(L, RYTong.FunctionLib.RYTScreen.ScreenHeight);

            return 1;
        }

        static int clientWidth(int L)
        {
            Lua.Lua_pushnumber(L, RYTong.FunctionLib.RYTScreen.ScreenWidth);

            return 1;
        }

        static int clientHeight(int L)
        {

            Lua.Lua_pushnumber(L, RYTong.FunctionLib.RYTScreen.ScreenClientHeight());

            return 1;
        }

        static int dpi(int L)
        {
            Lua.Lua_pushnumber(L, RYTong.FunctionLib.RYTScreen.ScreenDpi());

            return 1;
        }
    }
}

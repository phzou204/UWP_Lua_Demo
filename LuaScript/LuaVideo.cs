//
//  LuaVideo
//  RYTong
//
//  Created by wu.dong on 5/16/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaVideo
    {
        //private static Object detailV_ { get { return LuaManager.RootLuaManager.DetailV_; } }
        private LuaVideo() { }

        public static void SetCplus_Delegates()
        {
            LuaVideoDelegates.load = new Video_loadDel(load);
            LuaVideoDelegates.play = new Video_playDel(play);
            LuaVideoDelegates.pause = new Video_pauseDel(pause);
            LuaVideoDelegates.resume = new Video_resumeDel(resume);
            LuaVideoDelegates.stop = new Video_stopDel(stop);
            LuaVideoDelegates.dispose = new Video_disposeDel(dispose);
            LuaVideoDelegates.getVolume = new Video_getVolumeDel(getVolume);
            LuaVideoDelegates.getMinVolume = new Video_getMinVolumeDel(getMinVolume);
            LuaVideoDelegates.getMaxVolume = new Video_getMaxVolumeDel(getMaxVolume);
            LuaVideoDelegates.setVolume = new Video_setVolumeDel(setVolume);
        }

        static int load(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            if (Lua.Lua_isstring(L, -1))
            {
                var fileName = Lua.Lua_tostring(L, -1).ToString();
                object page = LuaManager.GetLuaManager(L).DetailV_;
                var info = RYTVideo.Load(fileName, string.Empty, null, page);
                Lua.Lua_pushlightuserdata(L,info);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int play(int L)
        {

            #region table Frame Size
            RYTFrame frame = null;

            // 特殊的参数判断
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.NFuction, LConst.NTable))
                return 0;

            if (Lua.Lua_gettop(L) > 3 && Lua.Lua_istable(L, 4))
            {
                frame = new RYTFrame();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, 4) != 0)
                {
                    var key = Lua.Lua_tostring(L, -2).ToString();
                    var value = Lua.Lua_tonumber(L, -1);

                    if (key.Equals("x", StringComparison.CurrentCultureIgnoreCase))
                    {
                        frame.Left = value * LuaManager.WidthScale;
                    }
                    else if (key.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                    {
                        frame.Top = value * LuaManager.HeightScale;
                    }
                    else if (key.Equals("width", StringComparison.CurrentCultureIgnoreCase))
                    {
                        frame.Width = value * LuaManager.WidthScale;
                    }
                    else if (key.Equals("height", StringComparison.CurrentCultureIgnoreCase))
                    {
                        frame.Height = value * LuaManager.HeightScale;
                    }

                    Lua.Lua_pop(L, 1);
                }
            }

            #endregion

            var callbackId = -1;

            RYTVideoInfo info = null;
            int count = Lua.Lua_gettop(L);
            if (count == 2)
            {
                info = Lua.Lua_touserdata(L,0) as RYTVideoInfo;
            }
            else
            {
                info = Lua.Lua_touserdata(L,2) as RYTVideoInfo;

                callbackId = LuaManager.GetFunctionIDIndex(L);
            }

            // no loop now
            //int loop = Lua.Lua_tointeger(L, 3);
            bool bLoop = false; // = loop == 1 ? true : false;

            if (info != null)
            {
                Action action = () =>
                    {
                        if (callbackId != -1)
                        {
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunction(callbackId , false);
                        }
                    };

                RYTVideo.Play(info, string.Empty, frame, bLoop, action);
            }

            return 0;
        }

        static int pause(int L)
        {
            RYTVideo.Pause();
            return 0;
        }

        static int resume(int L)
        {
            RYTVideo.Resume();

            return 0;
        }

        static int stop(int L)
        {
            RYTVideo.Stop();

            return 0;
        }

        static int dispose(int L)
        {
            RYTVideo.Dispose();

            return 0;
        }

        static int getVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTVideo.Volumn);

            return 1;
        }

        static int setVolume(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Integer))
                return 0;

            double vol = Lua.Lua_tonumber(L, 3);
            RYTVideo.Volumn = vol;

            return 0;
        }

        static int getMaxVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTVideo.MaxVolumn);

            return 1;
        }

        static int getMinVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTVideo.MinVolumn);

            return 1;
        }
    }
}

//
//  LuaAudio
//  RYTong
//
//  Created by wu.dong on 5/15/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using System.Reflection;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Windows.Resources;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaAudio
    {
        //private static Object detailV_ { get { return LuaManager.RootLuaManager.DetailV_; } }

        private LuaAudio() { }

        public static void SetCplus_Delegates()
        {
            LuaAudioDelegates.load = new loadDel(load);
            LuaAudioDelegates.play = new Audio_playDel(play);
            LuaAudioDelegates.pause = new pauseDel(pause);
            LuaAudioDelegates.stop = new Audio_stopDel(stop);
            LuaAudioDelegates.resume = new resumeDel(resume);
            LuaAudioDelegates.dispose = new disposeDel(dispose);
            LuaAudioDelegates.getVolume = new getVolumeDel(getVolume);
            LuaAudioDelegates.setVolume = new setVolumeDel(setVolume);
            LuaAudioDelegates.getMinVolume = new getMinVolumeDel(getMinVolume);
            LuaAudioDelegates.getMaxVolume = new getMaxVolumeDel(getMaxVolume);
        }

        static int load(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            var uri = Lua.Lua_tostring(L, -1);
            if (!string.IsNullOrEmpty(uri))
            {
                object page = LuaManager.GetLuaManager(L).DetailV_;
                var info = RYTAudio.Load(uri, string.Empty, page, null);
                Lua.Lua_pushlightuserdata(L,info);
                return 1;
            }
            Lua.Lua_pushnil(L);
            return 0;
        }

        static int play(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData,LConst.Integer,LConst.NFuction))
                return 0;
            
            var callbackId = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            var objectParam = Lua.Lua_touserdata(L,2);
            int numberOfLoops = Lua.Lua_tointeger(L, 3);

            var info = Lua.Lua_touserdata(L,2) as RYTAudioInfo;
            if (info != null)
            {
                Action playCompletedAction = () =>
                {
                    if (callbackId != -1)
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunction(callbackId, info.LoopCurrentTimes == 0 ? true : false);
                    }
                };

                RYTAudio.Play(string.Empty, info, numberOfLoops, playCompletedAction);
            }

            return 0;
        }

        static int pause(int L)
        {
            RYTAudio.Pause();
            return 0;
        }

        static int stop(int L)
        {
            RYTAudio.Stop();
            return 0;
        }

        static int resume(int L)
        {
            RYTAudio.Resume();
            return 0;
        }

        static int dispose(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;
            
            var info = Lua.Lua_touserdata(L,2) as RYTAudioInfo;
            RYTAudio.Dispose(info);
            return 0;
        }

        static int getVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTAudio.Volumn);

            return 1;
        }

        static int setVolume(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NUserData, LConst.Number))
                return 0;
            
            double vol = Lua.Lua_tonumber(L, 3);
            RYTAudio.Volumn = vol;

            return 0;
        }

        static int getMaxVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTAudio.MaxVolumn);

            return 1;
        }

        static int getMinVolume(int L)
        {
            Lua.Lua_pushnumber(L, RYTAudio.MinVolumn);

            return 1;
        }
    }
}

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
using System.Windows.Threading;
using RYTLuaCplusLib;
using System.Diagnostics;

namespace RYTong.LuaScript
{
    public class LuaTimer
    {
        private LuaTimer() { }

        public static void SetCplus_Delegates()
        {
            LuaTimerDelegates.startTimer = new startTimerDel(startTimer);
            LuaTimerDelegates.stopTimer = new stopTimerDel(stopTimer);
        }

        static int startTimer(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Number, LConst.Unknown, LConst.Function, LConst.NNumber))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            double? delay = null;
            double interval = Lua.Lua_tonumber(L, 2);
            var bTimerRepeat = Lua.Lua_isnumber(L, 3) ? (Lua.Lua_tointeger(L, 3) == 0 ? false : true) : Lua.Lua_toboolean(L, 3);
            var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(interval) };

            if (Lua.Lua_gettop(L) > 4)
                delay = Lua.Lua_tonumber(L, 5);

            var callbackId = LuaManager.GetFunctionIDIndex(L); //Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (callbackId != -1)
            {
                // 可能的延迟操作
                Action mayDelayActon = delegate
                {
                    if (bTimerRepeat)
                    {
                        timer.Tick += (s, e) =>
                        {
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunction(callbackId, false);
                        };
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                LuaManager.GetLuaManager(L).ExecuteCallBackFunction(callbackId, false);
                            });
                        timer.Start();
                    }
                    else
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunction(callbackId, true);
                    }
                };

                if (delay.HasValue)
                {
                    // 延迟启动计时器
                    DispatcherTimer delayTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(delay.Value) };
                    delayTimer.Tick += delegate
                    {
                        delayTimer.Stop();
                        mayDelayActon();
                    };
                    delayTimer.Start();
                }
                else
                {
                    mayDelayActon(); // 直接启动计时器
                }

            }

            Lua.Lua_pushlightuserdata(L,timer);

            return 1;
        }

        static int stopTimer(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            try
            {
                if (Lua.Lua_islightuserdata(L, 2))
                {
                    DispatcherTimer timer = (DispatcherTimer)Lua.Lua_touserdata(L,2);
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                }
            }
            catch
            {
                Debug.WriteLine("stopTimer error ..");
            }

            return 1;
        }
    }
}

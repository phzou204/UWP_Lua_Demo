//
//  LuaHttp
//  RYTong
//
//  Created by wu.dong on 3/25/12.
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
using System.Collections.Generic;
using System.Reflection;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaHttp
    {
        private static int listenerCallBackFunctionID = -1;
        private static int luaState = 0;
        //private static Object detailV_ { get { return LuaManager.RootLuaManager.DetailV_; } }

        private LuaHttp() { }

        public static void SetCplus_Delegates()
        {
            LuaHttpDelegates.postSyn = new postSynDel(postSyn);
            LuaHttpDelegates.postAsyn = new postAsynDel(postAsyn);
            LuaHttpDelegates.setListener = new HTTP_setListenerDel(setListener);
            LuaHttpDelegates.abort = new abortDel(abort);
        }

        /// <summary>
        /// 同步网络请求
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int postSyn(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table, LConst.String, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            int num = Lua.Lua_gettop(L);
            if (num == 4)
            {
                Dictionary<String, String> headers = new Dictionary<string, string>();
                bool isTable = Lua.Lua_istable(L, 2);
                if (isTable)
                {
                    Lua.Lua_pushnil(L);
                    while (Lua.Lua_next(L, 2) != 0)
                    {
                        String value = Lua.Lua_tostring(L, -1).ToString();
                        String key = Lua.Lua_tostring(L, -2).ToString();
                        headers[key] = value;
                        Lua.Lua_pop(L, 1);
                    }
                }

                string url = Lua.Lua_tostring(L, 3).ToString();
                string body = string.Empty;
                if (Lua.Lua_isstring(L, 4))
                {
                    body = Lua.Lua_tostring(L, 4).ToString();
                }
                object page = LuaManager.GetLuaManager(L).DetailV_;
                MethodInfo methods = page.GetType().GetMethod("postSyn");
                Object[] pars = new Object[] { headers, url, body };
                String result = (String)methods.Invoke(page, pars);
                Lua.Lua_pushstring(L, result);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int postAsyn(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NTable, LConst.String, LConst.NString, LConst.NFuction, LConst.NTable))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            int num = Lua.Lua_gettop(L);
            if (num >= 5)
            {
                Dictionary<String, String> headers = new Dictionary<string, string>();
                bool isTable = Lua.Lua_istable(L, 2);
                if (isTable)
                {
                    Lua.Lua_pushnil(L);
                    while (Lua.Lua_next(L, 2) != 0)
                    {
                        String value = Lua.Lua_tostring(L, -1).ToString();
                        String key = Lua.Lua_tostring(L, -2).ToString();
                        headers[key] = value;
                        Lua.Lua_pop(L, 1);
                    }
                }

                String url = Lua.Lua_tostring(L, 3).ToString();
                String body = string.Empty;
                if (Lua.Lua_isstring(L, 4))
                {
                    body = Lua.Lua_tostring(L, 4).ToString();
                }
                int callbackParams = Lua._LUA_REFNIL;
                if (num == 6)
                    callbackParams = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);// 将Lua函数需要的参数存储在REGISTRY中
                int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                object page = LuaManager.GetLuaManager(L).DetailV_;
                MethodInfo methods = page.GetType().GetMethod("postAsyn");
                Object[] pars = new Object[] { headers, url, body, callbackF, callbackParams };
                var obj = methods.Invoke(page, pars);
                if (obj == null)
                {
                    Lua.Lua_pushnil(L);
                    return 1;
                }
                else
                {
                    Lua.Lua_pushlightuserdata(L,obj);
                    return 1;
                }
            }

            return 1;
        }

        static int setListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            var obj = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (obj != null && callbackF != 0)
            {
                object requestObj = null;
                var objStr = obj.ToString();
                if (objStr.Contains("RYTFormAction"))
                {
                    var pi = obj.GetType().GetProperty("HttpRequest");
                    if (pi != null)
                    {
                        requestObj = pi.GetValue(obj, null);
                    }
                }

                Assembly assembly = Assembly.Load("RYTong.TLSLib");
                var classType_ = assembly.GetType("RYTong.TLSLib.HttpRequest");

                // Register event by reflection:
                EventInfo successEventInfo = classType_.GetEvent("OnSuccess", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo localSuccessMethod = typeof(LuaHttp).GetMethod("OnSuccess", BindingFlags.NonPublic | BindingFlags.Static);
                Delegate successDelegate = Delegate.CreateDelegate(successEventInfo.EventHandlerType, null, localSuccessMethod);
                if (requestObj != null)
                {
                    successEventInfo.AddEventHandler(requestObj, successDelegate);
                }
                else
                {
                    successEventInfo.AddEventHandler(obj, successDelegate);
                }

                EventInfo failedEventInfo = classType_.GetEvent("OnFailed", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo localFailedMethod = typeof(LuaHttp).GetMethod("OnFailed", BindingFlags.NonPublic | BindingFlags.Static);
                Delegate failedDelegate = Delegate.CreateDelegate(failedEventInfo.EventHandlerType, null, localFailedMethod);
                if (requestObj != null)
                {
                    failedEventInfo.AddEventHandler(requestObj, failedDelegate);
                }
                else
                {
                    failedEventInfo.AddEventHandler(obj, failedDelegate);
                }

                listenerCallBackFunctionID = callbackF;
                luaState = L;
            }

            return 0;
        }

        static void OnSuccess(string result, byte[] temp, int responseCode, WebHeaderCollection reponseHeaders)
        {
            if (listenerCallBackFunctionID != -1)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("progress", 100);
                dict.Add("responseCode", responseCode);
                dict.Add("isReachable", true);
                if (luaState != 0)
                    LuaManager.GetLuaManager(luaState).ExecuteCallBackFunctionWithTableParam(listenerCallBackFunctionID, dict);
                listenerCallBackFunctionID = -1;
                luaState = 0;
            }
        }

        static void OnFailed(string error, WebExceptionStatus status)
        {
            if (listenerCallBackFunctionID != -1)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("progress", 0);
                dict.Add("responseCode", 404);
                dict.Add("isReachable", false);
                if (luaState != 0)
                    LuaManager.GetLuaManager(luaState).ExecuteCallBackFunctionWithTableParam(listenerCallBackFunctionID, dict);
                listenerCallBackFunctionID = -1;
                luaState = 0;
            }
        }

        static int abort(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table))
                return 0;

            bool isTable = Lua.Lua_istable(L, 2);
            if (isTable)
            {
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    var request = Lua.Lua_touserdata(L,-1);
                    if (request != null)
                    {
                        var mi = request.GetType().GetMethod("Abort");
                        if (mi != null)
                        {
                            mi.Invoke(request, null);
                        }
                    }
                    Lua.Lua_pop(L, 1);
                }
            }

            //var obj = Lua.Lua_touserdata(L, 2);
            //if (obj != null)
            //{
            //    var mi = obj.GetType().GetMethod("Abort");
            //    if (mi != null)
            //    {
            //        mi.Invoke(obj, null);
            //    }
            //}

            return 0;
        }
    }
}

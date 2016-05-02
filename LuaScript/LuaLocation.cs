//
//  LuaLocation
//  RYTong
//
//  Created by wu.dong on 2/24/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using System.Reflection;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaLocation
    {
        private LuaLocation() { }

        public static void SetCplus_Delegates()
        {
            LuaLocationDelegates.reload = new reloadDel(reload);
            LuaLocationDelegates.replace = new replaceDel(replace);
        }

        static int reload(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NBoolean))
                return 0;

            bool value = Lua.Lua_toboolean(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo reload = page.GetType().GetMethod("reload");
            Object[] pars = new Object[] { value };
            reload.Invoke(page, pars);
            return 0;
        }

        /// <summary>
        /// location:replace(content)
        /// location:replace(content, transitionType)
        /// location:replace(content, callback, params)
        /// location:replace(content, transitionType, callback,params) // string, int, function, table
        /// </summary>
        /// <returns></returns>
        static int replace(int L)
        {
            int top = Lua.Lua_gettop(L);
            string str = string.Empty;
            int transitionType = -1;
            int callbackF = -1;
            int parmsId = -1;
            Action callbackAction = null;
            if (top == 5)
            {
                if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Integer, LConst.Function, LConst.Table))
                    return 0;
                str = Lua.Lua_tostring(L, 2);
                if (Lua.Lua_istable(L, -1))
                    parmsId = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                else
                    Lua.Lua_pop(L, 1);

                if (Lua.Lua_isfunction(L, -1))
                    callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                else
                    Lua.Lua_pop(L, 1);

                transitionType = Lua.Lua_tointeger(L, -1);
            }
            else if (top == 4)
            {
                if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Function, LConst.Table))
                    return 0;
                str = Lua.Lua_tostring(L, 2);

                if (Lua.Lua_istable(L, -1))
                    parmsId = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                else
                    Lua.Lua_pop(L, 1);

                if (Lua.Lua_isfunction(L, -1))
                    callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                else
                    Lua.Lua_pop(L, 1);
            }
            else if (top == 3)
            {
                if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Integer))
                    return 0;
                str = Lua.Lua_tostring(L, 2);
                if (!string.IsNullOrEmpty(str))
                {
                    if (Lua.Lua_isnumber(L, 3))
                    {
                        transitionType = Lua.Lua_tointeger(L, 3);
                    }
                }
            }
            else if (top == 2)
            {
                if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                    return 0;
                str = Lua.Lua_tostring(L, 2);
            }

            if (!string.IsNullOrEmpty(str))
            {
                if (callbackF != -1)
                {
                    callbackAction = delegate
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, parmsId == -1 ? null : new LuaCValue(parmsId));
                    };
                }
                object page = LuaManager.GetLuaManager(L).DetailV_;
                MethodInfo replace = page.GetType().GetMethod("replace");
                Object[] pars = new Object[] { str, transitionType, callbackAction };
                replace.Invoke(page, pars);
            }
            else
            {
                LogLib.RYTLog.ShowMessage("replace()调用出错,报文内容为空.", LogLib.RYTLog.Const.LuaPortError);
            }
            return 0;
        }
    }
}

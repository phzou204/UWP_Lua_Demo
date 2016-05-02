//
//  LuaUtility
//  RYTong
//
//  Created by wu.dong on 2/24/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using RYTLuaCplusLib;
using System.Diagnostics;

namespace RYTong.LuaScript
{
    public class LuaUtility
    {        
        public static Func<string, string, bool> ExtendDelagate;
        private LuaUtility() { }

        public static void SetCplus_Delegates()
        {
            LuaUtilityDelegates.length = new lengthDel(length);
            LuaUtilityDelegates.Trim = new TrimDel(trim);
            LuaUtilityDelegates.base64Encode = new base64EncodeDel(base64Encode);
            LuaUtilityDelegates.base64Decode = new base64DecodeDel(base64Decode);
            LuaUtilityDelegates.escapeURI = new escapeURIDel(escapeURI);
            LuaUtilityDelegates.unescapeURI = new unescapeURIDel(unescapeURI);
            LuaUtilityDelegates.Tls = new TlsDel(tls);
            LuaUtilityDelegates.passwordEncryption = new passwordEncryptionDel(passwordEncryption);
            LuaUtilityDelegates.appverify = new appverifyDel(appverify);
            LuaUtilityDelegates.extend = new extendDel(extend);

        }
        

        static int length(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushinteger(L, 0);
                return 1;
            }

            int result = 0;
            var str = Lua.Lua_tostring(L, 2).ToString();
            if (!string.IsNullOrEmpty(str))
            {
                result = str.Length;
                Lua.Lua_pushinteger(L, result);
            }
            else
            {
                Lua.Lua_pushinteger(L, 0);
            }

            return 1;
        }

        static int trim(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            String str = Lua.Lua_tostring(L, 2).ToString();
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                Lua.Lua_pushstring(L, str);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int base64Encode(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            string str = Lua.Lua_tostring(L, 2);
            if (!string.IsNullOrEmpty(str))
            {
                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(str);
                string baseStr = Convert.ToBase64String(data);
                Lua.Lua_pushstring(L, baseStr);
            }
            else
            {
                Lua.Lua_pushstring(L, string.Empty);
            }

            return 1;
        }

        static int base64Decode(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            string str = Lua.Lua_tostring(L, 2);
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    var bytes = Convert.FromBase64String(str);
                    var r = System.Text.UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    Lua.Lua_pushstring(L, r);
                }
                catch
                {
                    Debug.WriteLine("base64Decode error.");
                    Lua.Lua_pushstring(L, string.Empty);
                }
            }
            else
            {
                Lua.Lua_pushstring(L, string.Empty);
            }

            return 1;
        }

        static int escapeURI(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            string str = Lua.Lua_tostring(L, 2).ToString();
            if (str != null)
            {
                //string uriStr = HttpUtility.UrlEncode(str);
                string uriStr = ToolsLib.URIEncode.escapeURIComponent(str);
                Lua.Lua_pushstring(L, uriStr);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int unescapeURI(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            string str = Lua.Lua_tostring(L, 2).ToString();
            if (str != null)
            {
                //string uriStr = HttpUtility.UrlDecode(str);
                string uriStr = ToolsLib.URIEncode.unescapeURIComponent(str);
                Lua.Lua_pushstring(L, uriStr);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int tls(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NTable, LConst.NFuction))
                return 0;

            Action callbackAction = null;
            if (Lua.Lua_isfunction(L, -1))
            {
                var callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                callbackAction = () =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, null);
                };
            }

            Dictionary<String, String> parameters = null;
            bool isTable = Lua.Lua_istable(L, 2);
            if (isTable)
            {
                parameters = new Dictionary<String, String>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, 2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    parameters[key] = value;
                    Lua.Lua_pop(L, 1);
                }
            }
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo tls = page.GetType().GetMethod("startTLS");
            //Object[] pars = new Object[] { parameters };
            tls.Invoke(page, new object[] { parameters, callbackAction });

            return 0;
        }

        static int passwordEncryption(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table, LConst.Table, LConst.Function))
                return 0;

            Dictionary<String, object> parameters = null;
            bool isTable = Lua.Lua_istable(L, 2);
            if (isTable)
            {
                parameters = new Dictionary<String, object>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, 2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    parameters[key] = value;
                    Lua.Lua_pop(L, 1);
                }
            }
            Dictionary<String, String> modes = null;
            isTable = Lua.Lua_istable(L, 3);
            if (isTable)
            {
                modes = new Dictionary<String, String>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, 3) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    modes[key] = value;
                    Lua.Lua_pop(L, 1);
                }
            }
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo pe = page.GetType().GetMethod("passwordEncryption");
            Object[] pars = new Object[] { parameters, modes, callbackF };
            pe.Invoke(page, pars);

            return 0;
        }

        static int appverify(int L)
        {
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (callbackF != -1)
            {
                object page = LuaManager.GetLuaManager(L).DetailV_;
                MethodInfo tls = page.GetType().GetMethod("appVerify");
                Object[] pars = new Object[] { callbackF };
                tls.Invoke(page, new object[] { null });
            }
            return 0;
        }

        static int extend(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            string arg1 = Lua.Lua_tostring(L, 2);
            string arg2 = Lua.Lua_tostring(L, 3);

            if (ExtendDelagate != null)
            {
                bool re = ExtendDelagate(arg1, arg2);
                Lua.Lua_pushboolean(L, re ? 1: 0);
            }
            else
            {
                Lua.Lua_pushboolean(L, 0);
            }

            return 1;
        }

    }
}

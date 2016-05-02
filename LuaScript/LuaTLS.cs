using System;
using System.Reflection;
using System.Collections.Generic;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaTLS
    {
        private LuaTLS()
        { }
        public static void SetCplus_Delegates()
        {
            LuaTLSDelegates.connect = new connectDel(connect);
        }
        /// <summary>
        /// tls:connect 方法的实现；
        /// </summary>
        /// <returns></returns>
        private static int connect(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function, LConst.NTable))
                return 0;

            Dictionary<String, String> parameters = null;
            int top = Lua.Lua_gettop(L);
            if (top > 2)
            {
                bool isTable = Lua.Lua_istable(L, 3);
                if (isTable)
                {
                    parameters = new Dictionary<String, String>();
                    Lua.Lua_pushnil(L);
                    while (Lua.Lua_next(L, -2) != 0)
                    {
                        String value = Lua.Lua_tostring(L, -1).ToString();
                        String key = Lua.Lua_tostring(L, -2).ToString();
                        parameters[key] = value;
                        Lua.Lua_pop(L, 1);
                    }
                    Lua.Lua_pop(L, 1);
                }
            }
            int callbackF = -1;
            if (Lua.Lua_isfunction(L, -1))
                callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            Action<string, int> callbackAction = null;
            if (callbackF != -1)
            {
                callbackAction = (content, update) =>
                {
                    Dictionary<string, object> parameter_Dic = new Dictionary<string, object>();
                    parameter_Dic.Add("content", content);
                    parameter_Dic.Add("update", update);
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithTableParam(callbackF, parameter_Dic, true);
                };
            }
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("V1startTLS");
            if (mi != null)
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { parameters, callbackAction });
            return 0;
        }

    }
}

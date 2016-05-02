using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaKV
    {

        public LuaKV(){ }

        public static void SetCplusDelegates()
        {
            LuaKVDelegates.putD = new putDel(put);
            LuaKVDelegates.getD = new getDel(get);
            LuaKVDelegates.delD = new delDel(del);
        }

        static int put(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.LuaData))
                return 0;
            string key = Lua.Lua_tostring(L, 2);
            object value; 
            if (Lua.Lua_isstring(L, 3))
            {
                value = Lua.Lua_tostring(L, 3);
            }
            else if (Lua.Lua_isbool(L, 3))
            {
                value = Lua.Lua_toboolean(L, 3);
            }
            else if (Lua.Lua_isnumber(L, 3))
            {
                value = Lua.Lua_tointeger(L, 3);
            }
            else
            {
                List<KeyValuePair<string,string>> kvList = new List<KeyValuePair<string, string>>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String tvalue = Lua.Lua_tostring(L, -1).ToString();
                    String tkey = Lua.Lua_tostring(L, -2).ToString();
                    kvList.Add(new KeyValuePair<string, string>(tkey, tvalue));
                    Lua.Lua_pop(L, 1);
                }
                value = kvList;
            }
            bool result = false; // kvy 值为空不添加
            if (!string.IsNullOrEmpty(key))
            {
                result = RYTDatabase.PutKVData(key, value);
            }
            if (result)
            {
                Lua.Lua_pushboolean(L, 1);
            }
            else
            {
                Lua.Lua_pushboolean(L, 0);
            }
            return 1;
        }

        static int get(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            string key = Lua.Lua_tostring(L, 2);
            var value = RYTDatabase.GetKVData(key);
            if (value != null)
            {
                if (value is string)
                {
                    Lua.Lua_pushstring(L, value.ToString());
                }
                else if (value is double)
                {
                    Lua.Lua_pushnumber(L, (double)value);
                }
                else if (value is Boolean)
                {
                    int result = (int)value;
                    Lua.Lua_pushboolean(L, result);
                }
                else
                {
                    List<KeyValuePair<string, string>> list = value as List<KeyValuePair<string, string>>;
                    
                    if (list != null && list.Count > 0)
                    {
                        Lua.Lua_newtable(L);
                        for (int i = 0; i < list.Count; i++)
                        {
                            LuaManager.PushValueByType(L, list[i].Key);
                            LuaManager.PushValueByType(L, list[i].Value);
                            Lua.Lua_rawset(L, -3);
                        }
                    }
                }
            }
            else
            {
                Lua.Lua_pushnil(L);
            }
            return 1;
        }

        static int del(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                return 0;
            }
            string key = Lua.Lua_tostring(L, 2);
            bool result = RYTDatabase.DelKVData(key);
            if (result)
            {
                Lua.Lua_pushboolean(L, 1);
            }
            else
            {
                Lua.Lua_pushboolean(L, 0);
            }
            return 1;
        }
    }
}

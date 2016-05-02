//
//  LuaJson
//  RYTong
//
//  Created by wu.dong on 5/18/12.
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaJson
    {
        private LuaJson() { }

        public static void SetCplus_Delegates()
        {
            LuaJsonDelegates.jsonFromObject = new jsonFromObjectDel(jsonFromObject);
            LuaJsonDelegates.objectFromJSON = new objectFromJSONDel(objectFromJSON);
        }

        static int objectFromJSON(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            String jsonStr = Lua.Lua_tostring(L, 2);
            try
            {
                JObject jo = JObject.Parse(jsonStr);
                parseJsonToTable(L, jo);

                return 1;
            }
            catch (Exception e)
            {
                if (LuaManager.ExceptionHandleAction != null)
                {
                    string message = string.Format("{0}\n↘\n{1}", e.Message, jsonStr);
                    LuaManager.ExceptionHandleAction(null, message, LogLib.RYTLog.Const.JsonError);
                }
                Lua.Lua_pushnil(L);
                return 1;
            }
        }

        private static void parseJsonToTable(int L, JToken token)
        {
            if (token != null)
            {
                if (token is JProperty)
                {
                    JProperty jp = token as JProperty;
                    Lua.Lua_pushstring(L, jp.Name);//key
                    parseJsonToTable(L, jp.Value);//value
                    Lua.Lua_rawset(L, -3);
                }
                else if (token is JObject)
                {
                    Lua.Lua_newtable(L);
                    JObject jo = token as JObject;
                    var s = jo.Children();
                    foreach (var item in s)
                    {
                        parseJsonToTable(L, item);
                    }
                }
                else if (token is JArray)
                {
                    Lua.Lua_newtable(L);
                    JArray ja = token as JArray;
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Lua.Lua_pushnumber(L, i + 1); //key
                        parseJsonToTable(L, ja[i]); //value
                        Lua.Lua_rawset(L, -3);
                    }
                }
                else if (token is JValue)
                {
                    JValue jv = token as JValue;
                    if (jv.Value is String)
                    {
                        Lua.Lua_pushstring(L, jv.Value as String);
                    }
                    else if (jv.Value is double)
                    {
                        Lua.Lua_pushnumber(L, (double)jv.Value);
                    }
                    else if (jv.Value is int)
                    {
                        Lua.Lua_pushnumber(L, (int)jv.Value);
                    }
                    else if (jv.Value is long)
                    {
                        Lua.Lua_pushnumber(L, (long)jv.Value);
                    }
                    else
                    {
                        Lua.Lua_pushnil(L);
                    }
                }
                else
                {
                    Lua.Lua_pushnil(L);
                }
            }
            else
            {
                Lua.Lua_pushnil(L);
            }
        }

        static int jsonFromObject(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            String jsonStr = parseTableToJsonString(L);
            if (jsonStr != null)
            {
                Lua.Lua_pushstring(L, jsonStr);

            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        private static String parseTableToJsonString(int L)
        {
            String result = null;
            Boolean isTable = Lua.Lua_istable(L, -1);
            if (isTable)
            {
                result = "{";
                Lua.Lua_pushnil(L);
                bool maybeArray = true;
                List<string> valueList = new List<string>();
                int index = 1;
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String key = parseValueString(L, -2, true);

                    if (maybeArray && !(Lua.Lua_type(L, -2) == Lua._LUA_TNUMBER && key.Equals(index.ToString())))
                        maybeArray = false;
                    
                    String value = "";
                    bool isValueTable = Lua.Lua_istable(L, -1);
                    if (isValueTable)
                    {
                        value = parseTableToJsonString(L);
                    }
                    else
                    {
                        value = parseValueString(L, -1);
                        valueList.Add(value);
                    }
                    result += key + ":" + value + ",";
                    Lua.Lua_pop(L, 1);

                    index++;
                }
                result = result.TrimEnd(new char[] { ',' });
                result += "}";

                if (maybeArray)
                {
                    // 重装成json数组
                    result = "[";
                    foreach (var v in valueList)
                    {
                        result += v + ",";
                    }
                    result = result.TrimEnd(',') + "]";
                }
            }

            return result;
        }

        private static String parseValueString(int L, int index, bool bKey = false)
        {
            if (Lua.Lua_isnil(L, index))
            {
                return "";
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TNUMBER)
            {
                double value = Lua.Lua_tonumber(L, index);
                if (bKey)
                    return "\"" + Convert.ToString(value) + "\"";
                return Convert.ToString(value);
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TSTRING)
            {
                String value = Lua.Lua_tostring(L, index).ToString();
                return "\"" + value + "\"";
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TBOOLEAN)
            {
                return Lua.Lua_toboolean(L, index).ToString();
            }

            return "";
        }
    }
}

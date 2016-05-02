//
//  LuaDocument
//  RYTong
//
//  Created by wu.dong on 2/23/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaDocument
    {
        // 汉字 TF8
        //private static Object detailV_ { get { return LuaManager.GetLuaManager(L).DetailV_; } }

        private LuaDocument() { }

        public static void SetCplusDelegates()
        {
            LuaDocumentDelegates.getElementsByName = new getElementsByNameDel(getElementsByName);
            LuaDocumentDelegates.getElementsByTagName = new getElementsByTagNameDel(getElementsByTagName);
            LuaDocumentDelegates.getElementsByProperty = new getElementsByPropertyDel(getElementsByProperty);
            LuaDocumentDelegates.getElementsByClassName = new getElementsByClassNameDel(getElementsByClassName);
            LuaDocumentDelegates.getElementById = new getElementByIdDel(getElementById);
            LuaDocumentDelegates.createElement = new createElementDel(createElement);
            LuaDocumentDelegates.getAllEventElements = new getAllEventElementsDel(getAllEventElements);
            LuaDocumentDelegates.getEventElementsByName = new getEventElementsByNameDel(getEventElementsByName);
            LuaDocumentDelegates.getEventElementsByListener = new getEventElementsByListenerDel(getEventElementsByListener);
            LuaDocumentDelegates.setOnClickListener = new setOnClickListenerDel(setOnClickListener);
            LuaDocumentDelegates.setOnFocusListener = new setOnFocusListenerDel(setOnFocusListener);
            LuaDocumentDelegates.setOnBlurListener = new setOnBlurListenerDel(setOnBlurListener);
            LuaDocumentDelegates.setOnChangeListener = new setOnChangeListenerDel(setOnChangeListener);
            LuaDocumentDelegates.removeOnClickListener = new removeOnClickListenerDel(removeOnClickListener);
            LuaDocumentDelegates.removeOnFocusListener = new removeOnFocusListenerDel(removeOnFocusListener);
            LuaDocumentDelegates.removeOnBlurListener = new removeOnBlurListenerDel(removeOnBlurListener);
            LuaDocumentDelegates.removeOnChangeListener = new removeOnChangeListenerDel(removeOnChangeListener);
        }

        private static int removeOnChangeListener(int L)
        {
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("removeOnChangeListener");
            if (method != null)
                method.Invoke(page, null);
            return 0;
        }

        private static int removeOnBlurListener(int L)
        {
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("removeOnBlurListener");
            if (method != null)
                method.Invoke(page, null);
            return 0;
        }

        private static int removeOnFocusListener(int L)
        {
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("removeOnFocusListener");
            if (method != null)
                method.Invoke(page, null);
            return 0;
        }

        private static int setOnBlurListener(int L)
        {
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
                return 0;
            Action<object, string, string> callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("setOnBlurListener");
            if (method != null)
                method.Invoke(page, new Object[] { callbackAction });
            return 0;
        }

        private static int setOnChangeListener(int L)
        {
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
                return 0;
            Action<object, string, string> callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("setOnChangeListener");
            if (method != null)
                method.Invoke(page, new Object[] { callbackAction });
            return 0;
        }

        private static int setOnFocusListener(int L)
        {
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
            { return 0; }
            Action<object, string, string> callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("setOnFocusListener");
            if (method != null)
                method.Invoke(page, new Object[] { callbackAction });
            return 0;
        }

        private static int removeOnClickListener(int L)
        {
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("removeOnClickListener");
            if (method != null)
                method.Invoke(page, null);
            return 0;
        }

        private static int setOnClickListener(int L)
        {
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == -1)
                return 0;
            Action<object, string, string> callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            object page = LuaManager.GetLuaManager(L).DetailV_;
            MethodInfo method = page.GetType().GetMethod("setOnClickListener");
            if (method != null)
                method.Invoke(page, new Object[] { callbackAction });
            return 0;
        }

        static int getElementsByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String name = Lua.Lua_tostring(L, 2);
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(name) || page == null)
                return 0;
            MethodInfo methods = page.GetType().GetMethod("getElementsByName");
            Object[] pars = new Object[] { name };
            var invokeResult = methods.Invoke(page, pars);
            var list = invokeResult as IList;
            Lua.Lua_newtable(L);
            for (int i = 0; i < list.Count; i++)
            {
                Lua.Lua_pushnumber(L, i + 1);
                Lua.Lua_pushlightuserdata(L, list[i]);
                Lua.LuaL_getmetatable(L, "elementFunction");
                Lua.Lua_setmetatable(L, -2);
                Lua.Lua_rawset(L, -3);
            }        
            return 1;
        }

        static int getElementsByTagName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String name = Lua.Lua_tostring(L, 2).ToString();
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(name) || page == null)
                return 0;
            MethodInfo methods = page.GetType().GetMethod("getElementsByTagName");
            Object[] pars = new Object[] { name };
            var invokeResult = methods.Invoke(page, pars);
            var list = invokeResult as IList;
            Lua.Lua_newtable(L);
            for (int i = 0; i < list.Count; i++)
            {
                Lua.Lua_pushnumber(L, i + 1);
                Lua.Lua_pushlightuserdata(L,list[i]);
                Lua.LuaL_getmetatable(L, "elementFunction");
                Lua.Lua_setmetatable(L, -2);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }

        static int getElementsByProperty(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table))
                return 0;
            bool isTable = Lua.Lua_istable(L, -1);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (isTable && page != null)
            {
                Dictionary<String, String> dic = new Dictionary<string, string>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    dic[key] = value;
                    Lua.Lua_pop(L, 1);
                }
                MethodInfo methods = page.GetType().GetMethod("getElementsByProperty");
                Object[] pars = new Object[] { dic };
                List<Object> temp = (List<Object>)methods.Invoke(page, pars);
                Lua.Lua_newtable(L);
                if (temp.Count == 0)
                {
                    //string keyValue = dic[dic.Keys.GetEnumerator().Current];
                    //string message = string.Format("Control name='{0}' NOT found!", keyValue);
                    //MessageBox.Show("getElementsByProperty return null");
                    //-return 0;
                    Lua.Lua_rawset(L, -1);
                    return 1;
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    Lua.Lua_pushnumber(L, i + 1);
                    Lua.Lua_pushlightuserdata(L,temp[i]);
                    Lua.LuaL_getmetatable(L, "elementFunction");
                    Lua.Lua_setmetatable(L, -2);
                    Lua.Lua_rawset(L, -3);
                }
            }
            return 1;
        }

        static int getElementsByClassName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String name = Lua.Lua_tostring(L, 2).ToString();
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(name) || page == null)
                Lua.Lua_newtable(L);
            else
            {
                MethodInfo methods = page.GetType().GetMethod("getElementsByClassName");
                Object[] pars = new Object[] { name };
                var invokeResult = methods.Invoke(page, pars);
                var list = invokeResult as IList;
                Lua.Lua_newtable(L);
                for (int i = 0; i < list.Count; i++)
                {
                    Lua.Lua_pushnumber(L, i + 1);
                    Lua.Lua_pushlightuserdata(L,list[i]);
                    Lua.LuaL_getmetatable(L, "elementFunction");
                    Lua.Lua_setmetatable(L, -2);
                    Lua.Lua_rawset(L, -3);
                }
            }
            return 1;
        }

        private static int getElementById(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String id = Lua.Lua_tostring(L, 2).ToString();
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(id) || page == null)
                Lua.Lua_pushnil(L);
            else
            {
                MethodInfo methods = page.GetType().GetMethod("getElementById");
                Object[] pars = new Object[] { id };
                var invokeResult = methods.Invoke(page, pars);
                if (invokeResult != null)
                {
                    Lua.Lua_pushlightuserdata(L,invokeResult);
                    Lua.LuaL_getmetatable(L, "elementFunction");
                    Lua.Lua_setmetatable(L, -2);
                }
                else
                    return 0;                
            }
            return 1;
        }

        static int createElement(int L)
        {
            var top = Lua.Lua_gettop(L);
            string[] arrayList = new string[top - 1];
            for (int i = 0; i < arrayList.Length; i++)
            {
                if (i == 1)
                {
                    arrayList[i] = LConst.Table;
                }
                else
                {
                    arrayList[i] = LConst.String;
                }
            }
            if(!LuaCommon.CheckAndShowArgsError(L, arrayList))
                return 0;
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (top > 1 && page != null)
            {
                string tagName = Lua.Lua_tostring(L, 2);
                bool isTable = Lua.Lua_istable(L, 3);
                List<KeyValuePair<string, string>> kvList = null;
                if (top > 2 && isTable)
                {
                    kvList = new List<KeyValuePair<string, string>>();
                    Lua.Lua_pushnil(L);
                    while (Lua.Lua_next(L, -2) != 0)
                    {
                        String value = Lua.Lua_tostring(L, -1).ToString();
                        String key = Lua.Lua_tostring(L, -2).ToString();
                        kvList.Add(new KeyValuePair<string, string>(key, value));
                        Lua.Lua_pop(L, 1);
                    }
                }
                var method = page.GetType().GetMethod("createElement");
                if (method != null)
                {
                    var result = method.Invoke(page, new object[] { tagName, kvList});
                    if (result != null)
                    {
                        Lua.Lua_pushlightuserdata(L,result);
                        return 1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// get elements which has event(onclick,onfocus,onblur,onchange) attribute.
        /// </summary>
        /// <returns></returns>
        static int getAllEventElements(int L) //added by zou.penghui
        {
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (page == null)
                return 0;
            MethodInfo methods = page.GetType().GetMethod("getAllEventElements");            
            var invokeResult = methods.Invoke(page, null);
            var list = invokeResult as IList;
            Lua.Lua_newtable(L);
            for (int i = 0; i < list.Count; i++)
            {
                Lua.Lua_pushnumber(L, i);
                Lua.Lua_pushlightuserdata(L,list[i]);
                Lua.LuaL_getmetatable(L, "elementFunction");
                Lua.Lua_setmetatable(L, -2);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }
        
        static int getEventElementsByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String name = Lua.Lua_tostring(L, 2);
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(name) || page == null)
            {
                return 0;
            }
            MethodInfo methods = page.GetType().GetMethod("getEventElementsByName");
            var invokeResult = methods.Invoke(page, new object[] { name });
            var list = invokeResult as IList;
            Lua.Lua_newtable(L);
            for (int i = 0; i < list.Count; i++)
            {
                Lua.Lua_pushnumber(L, i);
                Lua.Lua_pushlightuserdata(L,list[i]);
                Lua.LuaL_getmetatable(L, "elementFunction");
                Lua.Lua_setmetatable(L, -2);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }

        static int getEventElementsByListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String listener = Lua.Lua_tostring(L, 2);
            Lua.Lua_pop(L, 2);
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (string.IsNullOrEmpty(listener) || page == null)
            {
                return 0;
            }
            MethodInfo methods = page.GetType().GetMethod("getEventElementsByListener");
            var invokeResult = methods.Invoke(page, new object[] { listener });
            var list = invokeResult as IList;
            Lua.Lua_newtable(L);
            for (int i = 0; i < list.Count; i++)
            {
                Lua.Lua_pushnumber(L, i);
                Lua.Lua_pushlightuserdata(L,list[i]);
                Lua.LuaL_getmetatable(L, "elementFunction");
                Lua.Lua_setmetatable(L, -2);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }
    }
}

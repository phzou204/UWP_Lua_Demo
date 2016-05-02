//
//  LuaHistory
//  RYTong
//
//  Created by wu.dong on 2/24/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System.Collections.Generic;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaHistory
    {
        public static void SetCplus_Delegates()
        {
            LuaHistoryDelegates.add = new history_AddDel(add);
            LuaHistoryDelegates.history_Get = new history_GetDel(get);
            LuaHistoryDelegates.clear = new history_ClearDel(clear);
            LuaHistoryDelegates.length = new history_LengthDel(length);
        }

        /// <summary>
        /// -1：  上个页面或者刚添加的页面
        /// 0：   栈顶,即当前页,最后一个添加的
        /// 正数：1则表示第一个显示的页面，即为页面缓存栈底，从栈底到栈顶依次为1，2，3
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int get(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Integer))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            int number = Lua.Lua_tointeger(L, 2);
            string html = LuaManager.GetLuaManager(L).GetHistoryHtml(number);
            Lua.Lua_pushstring(L, html);
            return 1;
        }

        static int add(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            string html = Lua.Lua_tostring(L, 2);
            if (!string.IsNullOrEmpty(html))
            {
                html = html.Trim();
                LuaManager.GetLuaManager(L).HistoryHtml.Add(html);
            }
            return 0;
        }

        /// <summary>
        ///  history:clear(N); 是指删除最后第N个添加到history的页面（N>0)
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int clear(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NNumber))
                return 0;
            List<string> historyHtml = LuaManager.GetLuaManager(L).HistoryHtml;
            int index = -1;
            var count = Lua.Lua_gettop(L);
            if (count == 2 && !Lua.Lua_isnil(L, 2))
                index = (int)Lua.Lua_tonumber(L, 2);
            if (index > 0 && historyHtml.Count >= index)
                historyHtml.RemoveRange(historyHtml.Count - index, index);
            else
                historyHtml.Clear();
            //var mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("Clear");
            //if (mi != null)
            //{
            //    mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { index });
            //}
            return 0;
        }

        static int length(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }                
            Lua.Lua_pushstring(L, LuaManager.GetLuaManager(L).HistoryHtml.Count.ToString());
            return 1;
        }
    }
}

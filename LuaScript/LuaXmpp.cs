using RYTLuaCplusLib;
using RYTong.FunctionLib;
using RYTong.FunctionLib.RYTXmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RYTong.LuaScript
{
    public class LuaXmpp
    {
        private LuaXmpp() { }

        public static void SetCplus_Delegates()
        {
            LuaXmppDelegates.startService = new startServiceDel(startService);
            LuaXmppDelegates.stopService = new stopServiceDel(stopService);
            LuaXmppDelegates.setPubSubListener = new setPubSubListenerDel(setPubSubListener);
            LuaXmppDelegates.setSubEnabled = new setSubEnabledDel(setSubEnabled);
            LuaXmppDelegates.getMoreMsg = new getMoreMsgDel(getMoreMsg);
            LuaXmppDelegates.getNewestMsg = new getNewestMsgDel(getNewestMsg);
            LuaXmppDelegates.isFinished = new isFinishedDel(isFinished);
            LuaXmppDelegates.clearOldMsg = new clearOldMsgDel(clearOldMsg);
        }

        static int startService(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            var username = Lua.Lua_tostring(L, 2);
            var password = Lua.Lua_tostring(L, 3);

            RYTXmppClient.Instanse.startService(username, password);

            return 0;
        }

        private static int stopService(int L)
        {
            var result = RYTXmppClient.Instanse.stopService();

            Lua.Lua_pushboolean(L, result ? 1 : 0);

            return 1;
        }
        /// <summary>
        /// 设置用户查看消息(单指推送消息)后的监听方法。
        /// 1.listener(required)：监听方法。此监听方法在定义时必须有两个参数，node_id和item_id，类型均为string。
        /// </summary>
        /// <returns></returns>
        private static int setPubSubListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            Action<string, string> cbAction = (node_id, item_id) => {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, new object[] { node_id, item_id });
            };

            RYTXmppClient.Instanse.setPubSubListener(cbAction);

            return 0;
        }

        //订阅或取消订阅节点。
        //第一个参数为表示是否订阅节点的table，key为节点id，value为0或1，0为取消订阅，1为订阅。
        //第二个参数为处理结果的回调函数，客户端程序在订阅或取消订阅完成后调用该函数。
        //回调函数包含两个参数，nodeId（类型为string）和flag（类型为整型），flag为0表示操作失败，为1表示操作成功。
        private static int setSubEnabled(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table, LConst.Function))
                return 0;

            List<KeyValuePair<string, int>> subEnabledList = new List<KeyValuePair<string, int>>();

            bool isTable = Lua.Lua_istable(L, 2);
            if (isTable)
            {
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, 2) != 0)
                {
                    var value = Lua.Lua_tointeger(L, -1);
                    var key = Lua.Lua_tostring(L, -2);
                    subEnabledList.Add(new KeyValuePair<string, int>(key, value));
                    Lua.Lua_pop(L, 1);
                }
            } 
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            RYTXmppClient.Instanse.setSubEnabled(subEnabledList, (arg) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, arg.Key, arg.Value);
                });

            return 0;
        }

        //获取消息后调用的回调函数，第一个参数为消息数组，每个元素为一个table，包含标题（title）、简介(summary)、类型（item_type）、
        //内容（content）和时间（time）五组键值对。第二个参数为last_index，用以获取下一页消息。获取消息失败时，两个参数为nil
        private static int getMoreMsg(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NString, LConst.Function))
                return 0;
            var nodeId = Lua.Lua_tostring(L, 2);
            string lastItemId = ""; //= Lua.Lua_tostring(L, 2);
            bool isnil = Lua.Lua_isnil(L, 3);
            if (!isnil)
            {
                lastItemId = Lua.Lua_tostring(L, 3);
            }
            var callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            Action<Dictionary<string,Dictionary<string,object>>, string> cbAction = (dic, lastIndex) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dic, lastIndex);
            };

            RYTXmppClient.Instanse.GetMoreMsg(nodeId, lastItemId, cbAction);
            return 0;
        }

        private static int getNewestMsg(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            string nodeId = Lua.Lua_tostring(L, 2);
            RYTXmppClient.Instanse.GetNewestMsg(nodeId);

            return 0;
        }

        /// <summary>
        /// 判断节点、订阅关系和发布项数据是否同步完成。
        /// 1.type (required): Number，数据类型。1为节点数据，2为订阅关系数据，3为发布项数据。
        /// 2.nodeId (optional): 当type为3时，须通过该参数指定发布项所属的节点。
        /// </summary>
        /// <returns></returns>
        private static int isFinished(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Number, LConst.NString))
            {
                return 0;
            }
            int type = (int)Lua.Lua_tonumber(L, 2);
            string nodeid = string.Empty;
            if (type == 3)
            {
                nodeid = Lua.Lua_tostring(L, 3);
            }

            var result = RYTXmppClient.Instanse.isFinished(type, nodeid);
            //Deployment.Current.Dispatcher.BeginInvoke(() => { Lua.Lua_pushboolean(L, result ? 1 : 0); });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        private static int clearOldMsg(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                return 0;
            }

            string nodeid = Lua.Lua_tostring(L, 2);

            var result = RYTXmppClient.Instanse.ClearOldMsg(nodeid);

            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }
    }
}

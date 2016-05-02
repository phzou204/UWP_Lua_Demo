using RYTLuaCplusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RYTong.LuaScript
{
    public class LuaCamera
    {
        private LuaCamera() { }

        public static void SetCplus_Delegates()
        {
            LuaCameraDelegates.open = new open_Del(open);
        }

        static int open(int L)
        {
            if(!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == -1)
                return 0;

            Action<string> succeedAction = (result)=>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, result);
                };
            Action<string> failedAction = (result)=>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, null);
                };
            FunctionLib.RYTCamera.Open(succeedAction, failedAction);
            
            return 0;
        }
    }
}

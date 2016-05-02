using RYTLuaCplusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RYTong.LuaScript
{
    class LuaEmp
    {
        //private static Object detailV_ { get { return LuaManager.RootLuaManager.DetailV_; } }

        private LuaEmp(){   }

        public static void SetCplusDelegates()
        {
            LuaEmpDelegates.tableLoading = new tableLoadingDel(tableLoading);
        }

        //emp:tableLoading(table_acc_ctrl[1],table_info);
        private static int tableLoading(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.String)) 
                return 0;

            var ctrl = Lua.Lua_touserdata(L,2);
            String content = Lua.Lua_tostring(L, 3).ToString();
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (page != null && content != null)
            {
                MethodInfo methods = page.GetType().GetMethod("TableLoading");
                Object[] pars = new Object[] {ctrl, content };
                methods.Invoke(page, pars);
            }

            return 0;
        }

    }
}

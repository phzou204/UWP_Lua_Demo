using RYTLuaCplusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Management.Deployment;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaCorp
    {
        private LuaCorp() { }

        public static void SetCplus_Delegates()
        {
            LuaCorpDelegates.install = new installDel(install);
            LuaCorpDelegates.isInstalled = new isInstalledDel(isInstalled);
            LuaCorpDelegates.launch = new launchDel(launch);
        }

        static int install(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Function))
                return 0;

            string url = Lua.Lua_tostring(L, 2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            string fullUrlAddress = url;
            var mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CombineUrl4Install");
            if (mi != null)
            {
                var result = mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { url });
                fullUrlAddress = result != null ? result.ToString() : url;
            }

            try
            {
                var AsyncOperation = InstallationManager.AddPackageAsync("企业App", new Uri(fullUrlAddress));

                if (callbackF > 0)
                {
                    Action<int, int> callBackAction = (progress, status) =>
                        {
                            Dictionary<string, object> callbackParams = new Dictionary<string, object>();
                            callbackParams.Add("url", url);
                            callbackParams.Add("progress", progress);
                            callbackParams.Add("status", status);

                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithTableParam(callbackF, callbackParams);
                        };

                    AsyncOperation.Progress = (result, progress) =>
                        {
                            LogLib.RYTLog.Log(fullUrlAddress + ",Status:" + result.Status.ToString() + "," + progress);
                            callBackAction((int)progress, 0);
                        };
                    AsyncOperation.Completed = (info, state) =>
                        {
                            LogLib.RYTLog.Log(fullUrlAddress + ",Status:" + state.ToString() + "," + 100);
                            if (state == AsyncStatus.Completed)
                            {
                                callBackAction(100, 1);
                            }
                            else
                            {
                                callBackAction(100, 2);
                            }
                        };
                }
            }
            catch (Exception e)
            {
                LogLib.RYTLog.ShowMessage(e.Message, "企业App安装异常");
            }

            return 0;
        }

        static int isInstalled(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            string appId = Lua.Lua_tostring(L, 2).Trim();
            var cropApps = InstallationManager.FindPackagesForCurrentPublisher();
            var result = cropApps.Any(a => a.Id.ProductId.TrimStart('{').TrimEnd('}').Equals(appId, StringComparison.CurrentCultureIgnoreCase));
            Lua.Lua_pushboolean(L, result ? 1 : 0);

            return 1;
        }

        static int launch(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            string appId = Lua.Lua_tostring(L, 2).Trim();
            var cropApps = InstallationManager.FindPackagesForCurrentPublisher();
            var app = cropApps.Where(a => a.Id.ProductId.TrimStart('{').TrimEnd('}').Equals(appId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (app != null)
            {
                app.Launch(string.Empty);
            }
            else
            {
                string error = string.Concat("appId：", appId);
                LogLib.RYTLog.ShowMessage(error, "企业App启动失败");
            }

            return 0;
        }
    }
}

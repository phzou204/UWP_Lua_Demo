using System;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using LuaRTLib;

namespace LuaScript
{
    public class LuaManager
    {       
        public int L { get; private set; }//luaState指针
        public int OnBackspaceListener { get; set; }//物理返回键注册的lua回调方法
        /// <summary>
        /// 失效的LuaManager,每次创建新LuaManager都会检查此属性是否为Null,若为Null则清理此LuaManager并关闭与其绑定的Lua状态机
        /// </summary>
        public static LuaManager InvalidLuaManager { get; set; }        
        private static Dictionary<int, LuaManager> LuaManagerDic = new Dictionary<int, LuaManager>();
        /// <summary>
        /// 获取LuaManager实例
        /// </summary>
        /// <param name="luaState">LuaState指针强转int类型的值</param>
        /// <returns></returns>
        public static LuaManager GetLuaManager(int luaState)
        {
            if(LuaManagerDic.ContainsKey(luaState))
            {
                return LuaManagerDic[luaState];
            }
            return null;
        }
        public Object DetailV_ { get; set; }
        public static object syncObj = new object();
        public static string Offline_Version="3.0";
        /// <summary>
        /// 错误处理，arg1：Execetion对象，arg2：错误内容，arg3：错误标题。
        /// </summary>
        public static Action<Exception, string, string> ExceptionHandleAction = null;
        public static bool MustVerify = true; // 强制校验每个接口方法的参数（个数&类型）！
        public static bool MustCheckPass = true; // 强制校验后一定通过.
        static bool isRegisterLuaDelegate = false;//lua代理方法只注册一次，此bool用于标识控制
        public LuaManager()
        {
            RemoveInvalidLuaManager();
            InitLua();
        }
        private static LuaManager _rootLuaManager = null;
        public static LuaManager RootLuaManager
        {
            get
            {
                if (_rootLuaManager == null)
                    _rootLuaManager = new LuaManager();
                return _rootLuaManager;
            }
        }
        private void InitLua()
        {            
            L = Lua.openLua(Offline_Version);
            LuaManagerDic[L] = this;
            if (!isRegisterLuaDelegate)
            {
                SetCplusDelegates();
                isRegisterLuaDelegate = true;
            }
        }
        private void SetCplusDelegates()
        {
            //LuaUtility.SetCplus_Delegates();
            LuaWindow.SetCplus_Delegates();
            //LuaAccelerometer.SetCplus_Delegates();
            //LuaAudio.SetCplus_Delegates();
            //LuaVideo.SetCplus_Delegates();
            //LuaDataBase.SetCplusDelegates();
            //LuaDocument.SetCplusDelegates();
            //LuaElement.SetCplusDelegates();
            //LuaEmp.SetCplusDelegates();
            //LuaFile.SetCplusDelegates();
            //LuaGesture.SetCplus_Delegates();
            //LuaGps.SetCplus_Delegates();
            //LuaHistory.SetCplus_Delegates();
            LuaHttp.SetCplus_Delegates();
            //LuaJson.SetCplus_Delegates();
            //LuaKV.SetCplusDelegates();
            //LuaLocation.SetCplus_Delegates();
            //LuaOffline.SetCplus_Delegates();
            //LuaScreen.SetCplus_Delegates();
            //LuaSystem.SetCplus_Delegates();
            //LuaTimer.SetCplus_Delegates();
            //LuaTransition.SetCplus_Delegates();
            //LuaTrack.SetCplus_Delegates();
            //LuaAnimation.SetCplus_Delegates();
            //LuaCamera.SetCplus_Delegates();
            //LuaCorp.SetCplus_Delegates();
            //LuaXmpp.SetCplus_Delegates();
            //LuaTLS.SetCplus_Delegates();
        }
        public void RestartLuaState()
        {
            Lua.closeLua(L);
            this.InitLua();
        }
        public void Clear()
        {
            this.DetailV_ = null;
            this.L = 0;
            Lua.closeLua(L);
        }         
        public string DoSLTParaser(string page)
        {
            return Lua.doSLTParaser(L, page);
        }
        public void LoadLuaScript(String luaStr)
        {
            lock (syncObj)
            {
                //LoadLuaStringAction(luaStr);
                //System.Threading.ThreadPool.QueueUserWorkItem(c => LoadLuaStringAction(luaStr), luaStr);
                luaStr = ReplaceLuaString(luaStr);

                /*
                int status = Lua.LuaL_loadbuffer(luaStr);

                string error = null;
                if (status == 0)
                {
                    int result = Lua.Lua_resume(L,0);
                    if (result != 0)
                    {
                        error = Lua.Lua_tostring(L, -1);
                        Lua.Lua_pop(L, 1);
                    }
                }
                else
                {
                    error = Lua.Lua_tostring(L, -1);
                    Lua.Lua_pop(L, 1);
                }
                */
                int status = Lua.LuaL_dostring(L, luaStr);
                string error = null;
                if (status != 0)
                {
                    error = Lua.Lua_tostring(L, -1);
                    Lua.Lua_pop(L, 1);
                }
                if (error != null && ExceptionHandleAction != null)
                {
                    error = string.Concat("@Load :\n", error);
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaError);
                }
            }
        }           
        private string ReplaceLuaString(string luaStr)
        {
            return Regex.Replace(luaStr, @"\.\.\.\s*\)\r\n", "...) arg = {...}; arg.n = #arg; ");
        }
        /// <summary>
        /// 执行Lua Function
        /// </summary>
        /// <param name="function"></param>
        public void PerformLuaFunction(String function)
        {
            lock (syncObj)
            {
                int status = Lua.LuaL_dostring(this.L, function);
                string error = null;
                if (status != 0)
                {
                    error = Lua.Lua_tostring(L, -1);
                    Lua.Lua_pop(L, 1);
                }
                if (error != null && ExceptionHandleAction != null)
                {
                    error = string.Concat("@",function,"\n", error);
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaError);
                }                
            }
        }
        public List<String> CallLuaFunction(Object method, String[] param, int resultNum)
        {
            lock (syncObj)
            {
                //Lua.lua_resume(lua_s, 0);
                // push function 
                if (method is String)
                {
                    Lua.Lua_getglobal(L, method as String);
                }
                else if (Lua.IsLuaCFunction(method))
                {
                    Lua.Lua_pushcfunction(method);
                }
                else if (method is int)
                {
                    Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, (int)method);
                }
                else
                {
                    //MessageBox.Show("无效的Lua Method。", "提示", MessageBoxButton.OK);
                }
                // push paramaters.
                int count = param.Length;
                for (int i = 0; i < count; i++)
                {
                    if (param[i].StartsWith("\"") || param[i].StartsWith("\'"))
                    {
                        var p = param[i].Trim(new char[] { '\"', '\'' });
                        Lua.Lua_pushstring(L, p);
                    }
                    else
                    {
                        double number = 0;
                        if (double.TryParse(param[i], out number))
                        {
                            Lua.Lua_pushnumber(L, number);
                        }
                        else
                        {
                            Lua.Lua_pushstring(L, param[i]);
                        }
                    }
                }

                int r = Lua.Lua_pcall(L, count, resultNum, 0);
                if (r != 0 && ExceptionHandleAction != null)
                {
                    string error = string.Concat("@", method, " :\n", Lua.Lua_tostring(L, -1));
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
                }

                //pop paramaters.
                List<String> result = new List<string>();
                for (int i = 0; i < resultNum; i++)
                {
                    String data = Lua.Lua_tostring(L, -1).ToString();
                    Lua.Lua_pop(L, 1);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
                return result;
            }
        }
        public void CallLuaFunction(String method)
        {
            lock (syncObj)
            {
                //Lua.lua_resume(lua_s, 0);
                Lua.Lua_getglobal(L, method);
                int status = Lua.Lua_pcall(L, 0, 0, 0);
                if (status != 0)
                {
                    String error = Lua.Lua_tostring(L, -1);
                    Lua.Lua_pop(L, 1);
                    if (ExceptionHandleAction != null)
                    {
                        error = string.Concat("@", method, " :\n", error);
                        //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaPortError);
                    }
                }
            }
        }
        public void CallPostAsynFunction(int callback, int callbackParams, String resultBody, int responseCode, WebHeaderCollection headers)
        {
            lock (syncObj)
            {
                try
                {
                    //Lua.lua_resume(lua_s, 0);
                    // push function 
                    Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callback);
                    Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callback);
                    // push paramaters.
                    if (callbackParams != Lua._LUA_REFNIL)
                    {
                        Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackParams);
                        Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callbackParams);
                    }
                    else
                    {
                        Lua.Lua_newtable(L);
                    }

#if true
                    if (resultBody.Contains("NotFound"))
                    {
                            
                    }
#endif

                    Lua.Lua_pushstring(L, "responseBody");
                    Lua.Lua_pushstring(L, resultBody);
                    Lua.Lua_rawset(L, -3);

                    Lua.Lua_pushstring(L, "responseCode");
                    Lua.Lua_pushnumber(L, responseCode);
                    Lua.Lua_rawset(L, -3);

                    if (headers != null && headers.Count > 0)
                    {
                        Lua.Lua_pushstring(L, "responseHeader");
                        Lua.Lua_newtable(L);

                        foreach (var key in headers.AllKeys)
                        {
                            Lua.Lua_pushstring(L, key);
                            Lua.Lua_pushstring(L, headers[key]);
                            Lua.Lua_rawset(L, -3);
                        }

                        Lua.Lua_rawset(L, -3);
                    }

                    int r = Lua.Lua_pcall(L, 1, 0, 0);
                    if (r != 0 && ExceptionHandleAction != null)
                    {
                        string error = Lua.Lua_tostring(L, -1);
                        //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaPostCallbackError);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Lua callback Exception:"+e.Message);
                }
            }
        }
        public void ExecuteCallBackFunction(int callbackF, bool unref = true)
        {
            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //   {
            //lock (syncObj)
            {
                Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);
                if (unref)
                {
                    Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callbackF);
                }

                //Lua.lua_pushnil(lua_s);
                int result = Lua.Lua_pcall(L, 0, 0, 0);
                if (result != 0 && ExceptionHandleAction != null)
                {
                    string error = Lua.Lua_tostring(L, -1);
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
                }
            }
            // });
        }
        public void ExecuteCallBackFunctionWithParam(int callbackF, object param, bool unRef = true)
        {
            lock (syncObj)
            {
                Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);
                if (unRef)
                    Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callbackF);
                PushValueByType(L, param);
                int result = Lua.Lua_pcall(L, 1, 0, 0);
                if (result != 0 && ExceptionHandleAction != null)
                {
                    string error = Lua.Lua_tostring(L, -1);
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
                }
            }
        }
        public void ExecuteCallBackFunctionWithParamInMainThreadAsync(int callbackF, object param, bool unRef = true)
        {
            if (callbackF == -1)
                return;
            //Windows.Management.Deployment.Dispatcher.BeginInvoke(() =>
            //    {
            //        lock (syncObj)
            //        {
            //            Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);
            //            if (unRef)
            //                Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callbackF);
            //            PushValueByType(L, param);
            //            int result = Lua.Lua_pcall(L, 1, 0, 0);
            //            if (result != 0 && ExceptionHandleAction != null)
            //            {
            //                string error = Lua.Lua_tostring(L, -1);
            //                ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
            //            }
            //        }
            //    });
        }
        public void ExecuteCallBackFunctionWithTableParam(int callbackF, Dictionary<string, object> paramDict, bool unRef = false)
        {
            if (callbackF == -1)
                return;
            //Windows.Management.Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    lock (syncObj)
            //    {
            //        Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);
            //        if (unRef)
            //            Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, callbackF);
            //        if (paramDict != null)
            //        {
            //            Lua.Lua_newtable(L);
            //            foreach (var key in paramDict.Keys)
            //            {
            //                Lua.Lua_pushstring(L, key);
            //                PushValueByType(L, paramDict[key]);
            //                Lua.Lua_rawset(L, -3);
            //            }
            //        }
            //        int result = Lua.Lua_pcall(L, 1, 0, 0);
            //        if (result != 0 && ExceptionHandleAction != null)
            //        {
            //            string error = Lua.Lua_tostring(L, -1);
            //            ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
            //        }
            //    }
            //});
        }      
        public void ExecuteCallBackFunctionWithAnyParams(int callbackF, params object[] parmList)
        {
            if (callbackF == -1)
                return;
            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    lock (syncObj)
            //    {
            //        Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);

            //        if (parmList != null && parmList.Length > 0)
            //        {
            //            foreach (var parm in parmList)
            //            {
            //                PushValueByType(L, parm);
            //            }
            //        }
            //        int result = Lua.Lua_pcall(L, parmList == null ? 0 : parmList.Length, 0, 0);
            //        if (result != 0 && ExceptionHandleAction != null)
            //        {
            //            string error = Lua.Lua_tostring(L, -1);
            //            ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
            //        }
            //    }
            //});
        }   
        public void ExecuteCallBackFunctionWithAnyParamsSync(int callbackF, params object[] parmList)
        {
            if (callbackF == -1)
                return;

            lock (syncObj)
            {
                Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, callbackF);

                if (parmList != null && parmList.Length > 0)
                {
                    foreach (var parm in parmList)
                    {
                        PushValueByType(L, parm);
                    }
                }

                int result = Lua.Lua_pcall(L, parmList == null ? 0 : parmList.Length, 0, 0);
                if (result != 0 && ExceptionHandleAction != null)
                {
                    string error = Lua.Lua_tostring(L, -1);
                    //ExceptionHandleAction(null, error, LogLib.RYTLog.Const.LuaCallbackError);
                }
            }
        }
        public void ReleaseRef(int index)
        {
            Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, index);
        }
        public static void PushValueByType(int L, object value)
        {
            if (value == null)
            {
                Lua.Lua_pushnil(L);
            }
            else if (value is float)
            {
                Lua.Lua_pushnumber(L, (float)value);
            }
            else if (value is double)
            {
                Lua.Lua_pushnumber(L, (double)value);
            }
            else if (value is string)
            {
                Lua.Lua_pushstring(L, (string)value);
            }
            else if (value is bool)
            {
                if ((bool)value == true)
                {
                    Lua.Lua_pushboolean(L, 1);
                }
                else
                {
                    Lua.Lua_pushboolean(L, 0);
                }
            }
            else if (value is int)
            {
                Lua.Lua_pushinteger(L, (int)value);
            }
            else if (value is long)
            {
                Lua.Lua_pushnumber(L, (long)value);
            }
            
            else if (value is Dictionary<string,object>)
            {
                Lua.Lua_newtable(L);
                var paramDict = value as Dictionary<string, object>;
                foreach (var key in paramDict.Keys)
                {
                    Lua.Lua_pushstring(L, key);
                    PushValueByType(L, paramDict[key]);
                    Lua.Lua_rawset(L, -3);
                }
            }
            else if (value is Dictionary<string, Dictionary<string, object>>)
            {
                var paramDict = value as Dictionary<string, Dictionary<string, object>>;
                if (value != null && paramDict.Count > 0)
                {
                    Lua.Lua_newtable(L);
                    for (int i = 0; i < paramDict.Count; i++)
                    {
                        Lua.Lua_pushnumber(L, i + 1);

                        Lua.Lua_newtable(L);
                        
                        foreach (var kv in paramDict[(i + 1).ToString()])
                        {
                            LuaManager.PushValueByType(L, kv.Key);
                            LuaManager.PushValueByType(L, kv.Value);

                            Lua.Lua_rawset(L, -3);
                        }

                        Lua.Lua_rawset(L, -3);
                    }
                }
            }
            else if (value is Dictionary<string, Dictionary<string, Dictionary<string, object>>>)
            {
                var paramDict = value as Dictionary<string, Dictionary<string, Dictionary<string, object>>>;
                if (value != null && paramDict.Count > 0)
                {
                    Lua.Lua_newtable(L);
                    for (int i = 0; i < paramDict.Count; i++)
                    {
                        foreach (var key in paramDict.Keys)
                        {
                            Lua.Lua_pushstring(L, key);
                            PushValueByType(L, paramDict[key]);
                            Lua.Lua_rawset(L, -3);
                        }
                    }
                }                     
            }
            else if (value is List<object>)
            {                
                Lua.Lua_newtable(L);
                List<object> list = value as List<object>;
                for (int i = 1; i <= list.Count; i++)
                {
                    Lua.Lua_pushnumber(L, i);
                    PushValueByType(L, list[i - 1]);
                    Lua.Lua_rawset(L, -3);
                }                
            }
            //else if (value is LuaCValue)
            //{
            //    Lua.Lua_rawgeti(L, Lua._LUA_REGISTRYINDEX, (value as LuaCValue).ValueIndex);
            //}
            else
            {
                Lua.Lua_pushlightuserdata(L, value);
            }
        }
        public static int GetFunctionIDIndex(int L)
        {
            var callbackId = -1;

            while (!Lua.Lua_isfunction(L, -1))
            {
                Lua.Lua_pop(L, 1);
            }

            if (Lua.Lua_isfunction(L, -1))
            {
                callbackId = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            }

            return callbackId;
        }
        public static double ConvertDegreesToRadians(double degrees)
        {
            return degrees * 180 / Math.PI;
        }             
        public static void RemoveInvalidLuaManager()
        {          
            if (InvalidLuaManager == null)
                return;
            if (LuaManagerDic.ContainsKey(InvalidLuaManager.L))
                LuaManagerDic.Remove(InvalidLuaManager.L);
            InvalidLuaManager.Clear();
            InvalidLuaManager = null; 
        }     
    }
}

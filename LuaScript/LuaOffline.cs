//
//  LuaOffline
//  RYTong
//
//  Created by zou.penghui on 2016/1/15
//  Copyright 2016 RYTong. All rights reserved.
//

using System;
using System.Reflection;
using System.Collections.Generic;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaOffline
    {
        private LuaOffline() { }

        private const string APP_NAME = "appname";
        private const string INFO = "info";

        public static void SetCplus_Delegates()
        {
            if (LuaManager.Offline_Version == "0")
            {
                LuaOfflineDelegates.downfile = new downfileDel(downfile);
                LuaOfflineDelegates.download = new downloadDel(download);
                LuaOfflineDelegates.optDownloadJson = new optDownloadJsonDel(optDownloadJson);
                LuaOfflineDelegates.optDownloadJsonInLocal = new optDownloadJsonInLocalDel(optDownloadJsonInLocal);
                LuaOfflineDelegates.optDownloadDescInLocal = new optDownloadDescInLocalDel(optDownloadDescInLocal);
                LuaOfflineDelegates.checkOfflineFile = new checkOfflineFileDel(checkOfflineFile);
                LuaOfflineDelegates.getServerDesc = new getServerDescDel(getServerDesc);
                LuaOfflineDelegates.remove = new removeDel2(remove);
                LuaOfflineDelegates.mustDownload = new mustDownloadDel(mustDownload);
            }
            else
            {
                if (LuaManager.Offline_Version.CompareTo("3.0") >= 0)
                {
                    LuaOfflineDelegates.checkOfflineFileWithLocal = new checkOfflineFileWithLocalDel(checkOfflineFileWithLocal);//3.0版本接口用于区分checkOfflineFile
                    LuaOfflineDelegates.setResReadAppName = new setResReadAppNameDel(setResReadAppName);
                }
                if (LuaManager.Offline_Version.CompareTo("2.1") >= 0)
                {
                    LuaOfflineDelegates.checkOfflineFileWithLocalH5 = new checkOfflineFileWithLocalH5Del(checkOfflineFileWithLocalH5);
                    LuaOfflineDelegates.checkOfflineFileWithServerH5 = new checkOfflineFileWithServerH5Del(checkOfflineFileWithServerH5);
                }
                if(LuaManager.Offline_Version.CompareTo("2.0") >= 0)
                {                    
                    LuaOfflineDelegates.checkOfflineFileWithServer = new checkOfflineFileWithServerDel(checkOfflineFileWithServer);
                }
                if (LuaManager.Offline_Version.CompareTo("1") >= 0)
                {                                       
                    LuaOfflineDelegates.update_hash = new update_hashDel(update_hash);
                    LuaOfflineDelegates.update_desc = new update_descDel(update_desc);
                    LuaOfflineDelegates.update_resource = new update_resourceDel(update_resource);
                    LuaOfflineDelegates.downOptionalFile = new downOptionalFileDel(downOptionalFile);
                    LuaOfflineDelegates.getOptInfoInServer = new getOptInfoInServerDel(getOptInfoInServer);
                    LuaOfflineDelegates.getOptInfoInLocal = new getOptInfoInLocalDel(getOptInfoInLocal);
                    LuaOfflineDelegates.checkOfflineFile = new checkOfflineFileDel(checkOfflineFileV1);
                    LuaOfflineDelegates.removeOptionalFile = new removeOptionalFileDel(removeOptionalFile);
                    LuaOfflineDelegates.commentOfFile = new commentOfFileDel(commentOfFile); 
                }
            }                     
            // 用于测试的接口
            LuaOfflineDelegates.getDownloadList = new getDownloadListDel(getDownloadList);
            LuaOfflineDelegates.removeOfflineFile = new removeOfflineFileDel(removeOfflineFile);
            LuaOfflineDelegates.version = new versionDel(version);
        }

        #region Offline Version 0
        /// <summary>
        ///	发起离线资源的升级、下载、删除功能。
        /// </summary>
        static int download(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            Action<string, string> action = null;

            if (callbackF != -1)
            {
                action = (result, mustUpdate) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, result, mustUpdate);
                };
            }

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("OfflineDownload", new Type[] { typeof(Action<string, string>) });
            if (mi != null)
            {
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { action });
            }
            
            return 0;
        }

        /// <summary>
        /// offline:downfile(src, callback)
        /// 表示当前资源的地址（EWP返回给客户端的描述信息中的“path”字段）
        /// </summary>
        /// <param name="lua">callback(optional) Function. 监听方法。此监听方法在定义时必须有一个参数，类型为Boolean， 表示当前离线资源是否下载成功。</param>
        /// <returns></returns>
        static int downfile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String, LConst.Function))
                return 0;

            var fileName = Lua.Lua_tostring(L, 2);
            var relatedPath = Lua.Lua_tostring(L, 3);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            Action<bool> action = null;

            if (callbackF != -1)
            {
                action = (result) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, result);
                };
            }

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("OfflineDownloadFile", new Type[] { typeof(string), typeof(string), typeof(Action<bool>) });
            if (mi != null)
            {
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, relatedPath, action });
            }

            return 0;
        }

        /// <summary>
        /// 返回所有可选下载的离线资源的列表（JSON）。
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int optDownloadJson(int L)
        {
            string json = string.Empty;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOfflineServerOptsJson");
            if (mi != null)
            {
                json = (string)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, null);
            }

            Lua.Lua_pushstring(L, json);
            return 1;
        }

        /// <summary>
        /// 返回本地已经下载过的可选插件 client_opt.json
        /// </summary>
        /// <returns></returns>
        static int optDownloadJsonInLocal(int L)
        {
            string json = string.Empty;

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOfflineClientOptsJson");
            if (mi != null)
            {
                json = (string)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, null);
            }

            Lua.Lua_pushstring(L, json);
            return 1;
        }

        /// <summary>
        /// 返回本地已经下载过的可选插件 client_opt.json{name:{path,rev}}
        /// </summary>
        /// <returns></returns>
        static int optDownloadDescInLocal(int L)
        {
            string json = string.Empty;

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOfflineClientOptDesc");
            if (mi != null)
            {
                json = (string)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, null);
            }

            Lua.Lua_pushstring(L, json);
            return 1;
        }

        /// <summary>
        /// 检查必选资源与描述是否一致，可选资源 与服务器最新描述是否一致
        /// </summary>
        /// <returns></returns>
        static int checkOfflineFile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            var fileName = Lua.Lua_tostring(L, 2);
            var relatedPath = Lua.Lua_tostring(L, 3);

            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileAvailabel");
            if (mi != null)
            {
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, relatedPath });
            }

            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        /// <summary>
        /// 获取完整的服务器server.desc
        /// </summary>
        /// <returns></returns>
        static int getServerDesc(int L)
        {
            string json = string.Empty;

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOfflineServerDescJson");
            if (mi != null)
            {
                json = (string)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, null);
            }

            Lua.Lua_pushstring(L, json);
            return 1;
        }

        /// <summary>
        /// 删除插件资源
        /// </summary>
        /// <returns></returns>
        static int remove(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            var fileName = Lua.Lua_tostring(L, 2);
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("RemoveClientOfflineFile");
            if (mi != null)
            {
                var result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName });
                Lua.Lua_pushboolean(L, result ? 1 : 0);
            }
            else
            {
                Lua.Lua_pushboolean(L, 0);
            }

            return 1;
        }

        /// <summary>
        /// 发起离线资源下载
        /// </summary>
        /// <returns></returns>
        static int mustDownload(int L)
        {
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("MustDownloadOffline");
            if (mi != null)
            {
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, null);
            }

            return 0;
        }

        #endregion                                          

        /// <summary>
        ///  检查必选资源与描述是否一致，可选资源与服务器最新描述是否一致
        /// </summary>
        /// <returns></returns>
        private static int checkOfflineFileV1(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            var fileName = Lua.Lua_tostring(L, 2);

            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileV1",new Type[]{typeof(string),typeof(string)});
            if (mi != null)
            {
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName ,""});
            }

            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }     
    
        private static int checkOfflineFileWithLocalH5(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String,LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 3)
                appName = Lua.Lua_tostring(L, 3);
            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileWithLocalH5", new Type[] { typeof(string), typeof(string) });
            if (mi != null)
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        private static int checkOfflineFileWithServerH5(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String,LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 3)
                appName = Lua.Lua_tostring(L, 3);
            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileWithServerH5", new Type[] { typeof(string), typeof(string) });
            if (mi != null)
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }     

        private static int update_hash(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function, LConst.NTable))
                return 0;
            int callbackF = -1;                
            Dictionary<string, string> dic = null;
            bool isTable = Lua.Lua_istable(L, -1);
            if (isTable)
            {
                dic = new Dictionary<string, string>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();

                    dic[key] = value;
                    Lua.Lua_pop(L, 1);
                }
                Lua.Lua_pop(L, 1);
            }

            if (Lua.Lua_isfunction(L, -1))
                callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);    

            Action<int> callBack = null;

            if (callbackF != -1)
            {
                callBack = (update) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, update);
                };
            }

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("OfflineUpdate_Hash", new Type[] { typeof(Action<int>), typeof(Dictionary<string, string>) });
            if (mi != null)
            {
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { callBack, dic });
            }

            return 0;
        }

        private static int update_desc(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function, LConst.NTable))
                return 0;

            int callbackF = -1;
            Dictionary<string, string> dic = null;            
            if (Lua.Lua_istable(L, -1))
            {
                dic = new Dictionary<string, string>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();

                    dic[key] = value;
                    Lua.Lua_pop(L, 1);
                }
                Lua.Lua_pop(L, 1);
            }

            if (Lua.Lua_isfunction(L, -1))
                callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);    
            Action<int> callBack = null;
            if (callbackF != -1)
            {
                callBack = (mustUpdate) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, mustUpdate);
                };
            }

            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("OfflineUpdate_Desc", new Type[] { typeof(Action<int>), typeof(Dictionary<string, string>) });
            if (mi != null)
            {
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { callBack, dic });
            }
            return 0;
        }

        private static int update_resource(int L)
        {          
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NTable))
                return 0;

            Action<int, int> processCallBack = null;
            Action<List<object>> finishCallBack = null;
            Dictionary<string, string> parameter_Dictionary = null;            
            if (Lua.Lua_gettop(L) == 2 && Lua.Lua_istable(L, 2))
            {
                parameter_Dictionary = new Dictionary<string, string>();
                Dictionary<string, int> callBack_Dictionary = new Dictionary<string, int>();                
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    if (Lua.Lua_isfunction(L, -1))//参数是回调方法时
                    {
                        int value = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                        String key = Lua.Lua_tostring(L, -1).ToString();
                        callBack_Dictionary[key] = value;                        
                    }                  
                    else
                    {
                        string value = Lua.Lua_tostring(L, -1).ToString();
                        String key = Lua.Lua_tostring(L, -2).ToString();
                        parameter_Dictionary[key] = value;
                        Lua.Lua_pop(L, 1);
                    }                                       
                }
                if (callBack_Dictionary.ContainsKey("processCallback"))
                {
                    processCallBack = (downNum, totalNum) =>
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callBack_Dictionary["processCallback"], downNum, totalNum);
                    };
                }
                if (callBack_Dictionary.ContainsKey("finishedCallback"))
                {
                    finishCallBack = (failedList) =>
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParamInMainThreadAsync(callBack_Dictionary["finishedCallback"], failedList);                        
                    };
                }                               
            }
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("Update_resource", new Type[] { typeof(Action<int, int>), typeof(Action<List<object>>), typeof(Dictionary<string, string>) });
            if (mi != null)
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { processCallBack, finishCallBack, parameter_Dictionary });
            return 0;
        }

        private static int downOptionalFile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NFuction, LConst.NTable))
                return 0;

            int callbackF = -1;
            string fileName = "";
            Action<bool> callBack = null;
            Dictionary<string, string> parameter_Dictionary = new Dictionary<string, string>();            
            if (Lua.Lua_istable(L, -1))
            {
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    parameter_Dictionary[key] = value;
                    Lua.Lua_pop(L, 1);
                }
                Lua.Lua_pop(L, 1);
            }
            if (Lua.Lua_isfunction(L, -1))
                callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);             
            if (Lua.Lua_isstring(L, -1))
                fileName = Lua.Lua_tostring(L, -1);           
            if (callbackF != -1)
            {
                callBack = (result) =>
                {
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, result);
                };
            }
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("DownOptionalFile", new Type[] { typeof(string), typeof(Action<bool>), typeof(Dictionary<string, string>) });
            if (mi != null)
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, callBack, parameter_Dictionary });
            return 0;
        }

        private static int getOptInfoInServer(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NString))
                return 0;

            string appName = "";
            if (Lua.Lua_gettop(L) == 2)
                appName = Lua.Lua_tostring(L, -1);
            Dictionary<string, string> result_Dictionary = new Dictionary<string, string>();
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOptInfoInServer", new Type[] { typeof(string) });
            if (mi != null)
                result_Dictionary = (Dictionary<string, string>)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { appName });
            Lua.Lua_newtable(L);
            foreach (var item in result_Dictionary)
            {
                Lua.Lua_pushstring(L, item.Key);
                Lua.Lua_pushstring(L, item.Value);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }

        private static int getOptInfoInLocal(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NString))
                return 0;

            string appName = "";
            if (Lua.Lua_gettop(L) == 2)
                appName = Lua.Lua_tostring(L, -1);
            Dictionary<string, string> result_Dictionary = new Dictionary<string, string>();
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetOptInfoInLocal", new Type[] { typeof(string) });
            if (mi != null)
                result_Dictionary = (Dictionary<string, string>)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { appName });

            Lua.Lua_newtable(L);
            foreach (var item in result_Dictionary)
            {
                Lua.Lua_pushstring(L, item.Key);
                Lua.Lua_pushstring(L, item.Value);
                Lua.Lua_rawset(L, -3);
            }
            return 1;
        }

        private static int removeOptionalFile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 3)
                appName = Lua.Lua_tostring(L, 3);
            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("RemoveOptionalFile", new Type[] { typeof(string),typeof(string) });
            if (mi != null)
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        private static int commentOfFile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            var fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 3)
                appName = Lua.Lua_tostring(L, 3);
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetCommentOfFile", new Type[] { typeof(string),typeof(string) });
            if (mi != null)
            {
                string result = (string)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
                if (!string.IsNullOrEmpty(result))
                    Lua.Lua_pushstring(L, result);
                else
                    Lua.Lua_pushnil(L);
            }
            else
                Lua.Lua_pushnil(L);
            return 1;
        }
        /// <summary>
        ///离线3.0版本的checkOfflineFile
        /// </summary>
        /// <returns></returns>
        private static int checkOfflineFileWithLocal(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            var fileName = Lua.Lua_tostring(L, 2);
            string appName = Lua.Lua_tostring(L, 3);

            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileWithLocal", new Type[] { typeof(string) });
            if (mi != null)
            {
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            }

            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        private static int checkOfflineFileWithServer(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 3)
                appName = Lua.Lua_tostring(L, 3);
            bool result = false;
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("CheckOfflineFileWithServer", new Type[] { typeof(string), typeof(string) });
            if (mi != null)
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }

        private static int setResReadAppName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string appName = Lua.Lua_tostring(L, 2);
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("SetResReadAppName", new Type[] { typeof(string) });
            if (mi != null)
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { appName });
            return 0;
        }       

        #region 用于测试的接口，不对外开放

        private static int getDownloadList(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NString))
                return 0;
            string appName = "";
            if (Lua.Lua_gettop(L) == 2)
                appName = Lua.Lua_tostring(L, 2);
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetDownloadList", new Type[] { typeof(string) });
            if (mi != null)
                mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { appName });
            return 0;
        }

        /// <summary>
        /// 删除指定的文件，用于删除离线下载和插件资源，需要删除资源文件及相应的描述信息，只用于删除必选资源。
        /// 注意：通过此接口删除必选资源后，需要将本地的必选资源hash值清掉，否则后续测试时不会去服务器请求更新。
        /// </summary>
        /// <returns></returns>
        private static int removeOfflineFile(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String,LConst.NString))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            string fileName = Lua.Lua_tostring(L, 2);
            string appName = "";
            if (Lua.Lua_gettop(L) == 2)
                appName = Lua.Lua_tostring(L, 2);
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("RemoveOfflineFile", new Type[] { typeof(string), typeof(string) });
            bool result = false;
            if (mi != null)
                result = (bool)mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { fileName, appName });
            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 1;
        }                
        private static int version(int L)
        {
            double v = double.Parse(LuaManager.Offline_Version);
            Lua.Lua_pushnumber(L, v);
            return 1;
        }
        #endregion
        
    }
}

//
//  LuaDataBase
//  RYTong
//
//  Created by wu.dong on 2/23/12.
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
using System.IO.IsolatedStorage;
using SQLiteClient;
using System.Collections.Generic;
using RYTong.FunctionLib;
using RYTLuaCplusLib;
using System.Diagnostics;

namespace RYTong.LuaScript
{
    public class LuaDataBase
    {
        private LuaDataBase() { }

        public static void SetCplusDelegates()
        {
            LuaDataBaseDelegates.addData = new addDataDel(addData);
            LuaDataBaseDelegates.updateData = new updateDataDel(updateDate);
            LuaDataBaseDelegates.getData = new getDataDel(getData);
            LuaDataBaseDelegates.deleteData = new deleteDataDel(deleteData);
            LuaDataBaseDelegates.open = new openDBDel(open);
            LuaDataBaseDelegates.exec = new execDel(exec);
            LuaDataBaseDelegates.close = new closeDBDel(close);
        }

        #region IsolatedStorageSettings

        static int addData(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;
            
            String key = Lua.Lua_tostring(L, 2);
            String value = string.Empty;
            if (!Lua.Lua_isnil(L, 3))
            {
                value = Lua.Lua_tostring(L, 3);
            }

            bool result = RYTDatabase.AddData(key, value, null, null);
            Lua.Lua_pushboolean(L, result ? 1 : 0);

            return 0;
        }


        /// <summary>
        /// 更新键值，如果不存在则添加，此接口已在5.2.48_B 以后废弃
        /// </summary>
        /// <returns></returns>
        static int updateDate(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            string key = Lua.Lua_tostring(L, 2);
            string value = Lua.Lua_tostring(L, 3);

            RYTDatabase.UpdateData(key, value,null,null);

            return 0;
        }

        static int getData(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            
            String key = Lua.Lua_tostring(L, 2).ToString();

            var value = RYTDatabase.GetData(key, null, null);
            if (value != null)
            {
                Lua.Lua_pushstring(L, value);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int deleteData(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            
            String key = Lua.Lua_tostring(L, 2).ToString();

            bool result = RYTDatabase.DeleteData(key, null);

            Lua.Lua_pushboolean(L, result ? 1 : 0);
            return 0;
        }

        #endregion

        #region SQLLite

        // 数据库名必须加.sql
        static int open(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            String dbName = Lua.Lua_tostring(L, 2).ToString();
            
            if (!string.IsNullOrEmpty(dbName))
            {
                SQLiteConnection db = RYTDatabase.OpenDB(dbName);                
                Lua.Lua_pushpermanentuserdata(L, db);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }

            return 1;
        }

        static int exec(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.String))
                return 0;
            SQLiteConnection db = Lua.Lua_topermanentuserdata(L, 2) as SQLiteConnection;
            String sql = Lua.Lua_tostring(L, 3).Trim();

            try
            {
                if (db != null && sql != null)
                {
                    Action<List<List<KeyValuePair<string, object>>>> action = (list) =>
                        {
                            if (sql.StartsWith("select", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Lua.Lua_newtable(L);
                                if (list != null && list.Count > 0)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        Lua.Lua_pushnumber(L, i + 1);

                                        Lua.Lua_newtable(L);
                                        foreach (var kv in list[i])
                                        {
                                            LuaManager.PushValueByType(L, kv.Key);
                                            LuaManager.PushValueByType(L, kv.Value);

                                            Lua.Lua_rawset(L, -3);
                                        }

                                        Lua.Lua_rawset(L, -3);
                                    }
                                }
                            }
                            else
                            {
                                Lua.Lua_pushnil(L);
                            }
                        };

                    RYTDatabase.ExecuteSQL(db.Database, sql, action, null, null);

                    //SQLiteCommand cmd = db.CreateCommand(sql);

                    //if (sql.StartsWith("select", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    List<List<KeyValuePair<string, object>>> list = cmd.ExecuteQueryAndReturnTable();

                    //    if (list.Count > 0)
                    //    {
                    //        Lua.Lua_newtable(L);
                    //        for (int i = 0; i < list.Count; i++)
                    //        {
                    //            Lua.Lua_pushnumber(L,  i + 1);

                    //            Lua.Lua_newtable(L);
                    //            foreach (var kv in list[i])
                    //            {
                    //                LuaManager.PushValueByType(L, lua, kv.Key);
                    //                LuaManager.PushValueByType(L, lua, kv.Value);

                    //                Lua.Lua_rawset(L,  -3);
                    //            }

                    //            Lua.Lua_rawset(L,  -3);
                    //        }
                    //        return 1;
                    //    }
                    //}
                    //else
                    //{
                    //    int result = cmd.ExecuteNonQuery();
                    //    Lua.Lua_pushnil(L);
                    //}
                }
                else
                {
                    Lua.Lua_pushnil(L);
                }
            }
            catch (Exception e)
            {
                if (sql.StartsWith("select", StringComparison.CurrentCultureIgnoreCase))
                {
                    Lua.Lua_newtable(L);
                }
                else
                {
                    Lua.Lua_pushnil(L);
                }
                Debug.WriteLine("DB Exception: " + e.Message);
                LuaCommon.ShowError(null,e.Message,"DB Exception:" );
            }

            return 1;
        }

        static int close(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            try
            {
                SQLiteConnection db = Lua.Lua_topermanentuserdata(L, 2) as SQLiteConnection;
                RYTDatabase.Close(db.Database);
            }
            catch
            { }
            return 0;
        }

        #endregion
    }
}

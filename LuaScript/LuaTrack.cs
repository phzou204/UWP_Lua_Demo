using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RYTong.FunctionLib;
using System.Data.Linq;
using System.Net;
using RYTLuaCplusLib;
using System.Reflection;

namespace RYTong.LuaScript
{
    public class LuaTrack
    {
        private LuaTrack() { }

        public static void SetCplus_Delegates()
        {
            LuaTrackDelegates.setUserId = new setUserIdDel(setUserId);
            LuaTrackDelegates.setAge = new setAgeDel(setAge);
            LuaTrackDelegates.setBirthday = new setBirthdayDel(setBirthday);
            LuaTrackDelegates.setGender = new setGenderDel(setGender);
            LuaTrackDelegates.setLocation = new setLocationDel(setLocation);
            LuaTrackDelegates.endSession = new endSessionDel(endSession);
            LuaTrackDelegates.logEvent = new logEventDel(logEvent);            
            LuaTrackDelegates.endTimedEvent = new endTimedEventDel(endTimedEvent);
            LuaTrackDelegates.logError = new logErrorDel(logError);
            LuaTrackDelegates.logPurchase = new logPurchaseDel(logPurchase);
            LuaTrackDelegates.logPageView = new logPageViewDel(logPageView);
            LuaTrackDelegates.sendReport = new sendReportDel(sendReport);
            LuaTrackDelegates.getPageId = new getPageIdDel(getPageId);
            LuaTrackDelegates.setPageId = new setPageIdDel(setPageId);
            LuaTrackDelegates.getLoadStartTime = new getLoadStartTimeDel(getLoadStartTime);
            LuaTrackDelegates.getLoadCompletedTime = new getLoadCompletedTimeDel(getLoadCompletedTime);
            LuaTrackDelegates.getLastPageId = new getLastPageIdDel(getLastPageId);
            LuaTrackDelegates.getLastLoadStartTime = new getLastLoadStartTimeDel(getLastLoadStartTime);
            LuaTrackDelegates.getLastLoadCompletedTime = new getLastLoadCompletedTimeDel(getLastLoadCompletedTime);

        }

        //public static RYTTrackLib.RYTTrack RYTTrackInstanse
        //{
        //    get
        //    {
        //        return RYTTrackLib.RYTTrack.Instanse;
        //    }
        //}
        
        private static Type _TrackType;
        private static Type TrackType
        {
            get
            {
                if (_TrackType == null)
                {
                    System.Reflection.Assembly trackLibAssembly = System.Reflection.Assembly.Load("RYTong.RYTTrackLib");
                    _TrackType = trackLibAssembly.GetType("RYTong.RYTTrackLib.RYTTrack");
                    return _TrackType;
                }
                else
                {
                    return _TrackType;
                }
            }
        }
        private static object _RYTTrackInstanse;
        private static object RYTTrackInstanse
        {
            get
            {
                if (_RYTTrackInstanse == null)
                {
                    System.Reflection.PropertyInfo propertyInfo = TrackType.GetProperty("Instanse");
                    _RYTTrackInstanse = propertyInfo.GetValue(null, null);
                    return _RYTTrackInstanse;
                }
                else
                {
                    return _RYTTrackInstanse;
                }
                
            }
        }

        static int setUserId(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            string userId = Lua.Lua_tostring(L, 2);
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action SetuserId_Action = delegate() { TrackType.GetMethod("SetuserId").Invoke(RYTTrackInstanse, new object[] { userId }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { SetuserId_Action });
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.SetuserId(userId); });
            return 0;
        }

        static int setAge(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Integer))
                return 0;

            int age = Lua.Lua_tointeger(L, 2);
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.SetAge(age); });

            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action SetAge_Action = delegate() { TrackType.GetMethod("SetAge").Invoke(RYTTrackInstanse, new object[] { age }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { SetAge_Action });
            return 0;
        }

        static int setBirthday(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            string dateTime = Lua.Lua_tostring(L, 2);
            DateTime day;
            if (DateTime.TryParse(dateTime, out day))
            {
                //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.SetBirthday(day); });

                System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
                Action SetBirthday_Action = delegate() { TrackType.GetMethod("SetBirthday").Invoke(RYTTrackInstanse, new object[] { day }); };
                AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { SetBirthday_Action });
            }

            return 0;
        }

        static int setGender(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Integer))
                return 0;

            int gender = Lua.Lua_tointeger(L, 2);
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.SetGender(gender); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action SetGender_Action = delegate() { TrackType.GetMethod("SetGender").Invoke(RYTTrackInstanse, new object[] { gender }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { SetGender_Action });
            return 0;
        }

        static int setLocation(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NNumber, LConst.NNumber, LConst.NNumber))
                return 0;

            double latitude = double.NaN;
            double longitude = double.NaN;
            double accuracy = double.NaN;
            if (Lua.Lua_gettop(L) > 2)
            {
                latitude = Lua.Lua_tonumber(L, 2);
                longitude = Lua.Lua_tonumber(L, 3);
            }
            if (Lua.Lua_gettop(L) > 3)
            {
                Lua.Lua_tonumber(L, 4);
            }

            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.SetLocation(latitude, longitude, accuracy); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action SetLocation_Action = delegate() { TrackType.GetMethod("SetLocation").Invoke(RYTTrackInstanse, new object[] { latitude, longitude, accuracy }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { SetLocation_Action });
            return 0;
        }

        static int endSession(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NBoolean))
                return 0;
            bool immediately = false;

            if (Lua.Lua_gettop(L) > 1)
                immediately = Lua.Lua_toboolean(L, 2);

            //RYTTrackInstanse.EndSession(immediately);
            System.Reflection.MethodInfo EndSession_MethodInfo = TrackType.GetMethod("EndSession");           
            EndSession_MethodInfo.Invoke(RYTTrackInstanse, new object[] { immediately });
            return 0;
        }

        static int logEvent(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NTable, LConst.NBoolean))
                return 0;

            string eventName = Lua.Lua_tostring(L, 2).ToString();
            List<KeyValuePair<string, string>> eventParameters = null;
            bool isTimedEvent = false;

            if (Lua.Lua_gettop(L) > 2)
            {
                eventParameters = ParseTableToList(L, 3);
            }
            if (Lua.Lua_gettop(L) > 3)
            {
                if (Lua.Lua_toboolean(L, 4))
                {
                    isTimedEvent = true;
                }
            }
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.LogEvent(eventName, eventParameters, isTimedEvent); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action LogEvent_Action = delegate() { TrackType.GetMethod("LogEvent").Invoke(RYTTrackInstanse, new object[] { eventName, eventParameters, isTimedEvent }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { LogEvent_Action });
            return 0;
        }

        static int endTimedEvent(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.NTable))
                return 0;

            string eventName = Lua.Lua_tostring(L, 2).ToString();
            List<KeyValuePair<string, string>> parameters = null;
            if (Lua.Lua_gettop(L) > 2)
            {
                parameters = ParseTableToList(L, 3);
            }
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.EndTimedEvent(eventName, parameters); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action EndTimedEvent_Action = delegate() { TrackType.GetMethod("EndTimedEvent").Invoke(RYTTrackInstanse, new object[] { eventName, parameters }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { EndTimedEvent_Action });
            return 0;
        }

        static int logError(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String, LConst.Integer))
                return 0;

            string name = Lua.Lua_tostring(L, 2).ToString();
            string message = string.Empty;
            int lineNumber = 0;
            if (Lua.Lua_gettop(L) > 2)
            {
                message = Lua.Lua_tostring(L, 3).ToString();
            }
            if (Lua.Lua_gettop(L) > 3)
            {
                lineNumber = Lua.Lua_tointeger(L, 4);
            }
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.LogError(name, message, lineNumber); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action LogError_Action = delegate() { TrackType.GetMethod("LogError").Invoke(RYTTrackInstanse, new object[] { name, message, lineNumber }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { LogError_Action });
            return 0;
        }

        static int logPurchase(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Number, LConst.NString, LConst.NTable))
                return 0;

            string name = Lua.Lua_tostring(L, 2).ToString();
            double cost = Lua.Lua_tonumber(L, 3);
            string currency = string.Empty;
            List<KeyValuePair<string, string>> parameters = null;
            if (Lua.Lua_gettop(L) > 3)
            {
                currency = Lua.Lua_tostring(L, 4).ToString();
            }
            if (Lua.Lua_gettop(L) > 4)
            {
                parameters = ParseTableToList(L, 5);
            }
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.LogPurchase(name, cost, currency, parameters); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action LogPurchase_Action = delegate() { TrackType.GetMethod("LogPurchase").Invoke(RYTTrackInstanse, new object[] { name, cost, currency, parameters }); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { LogPurchase_Action });
            return 0;
        }

        static int logPageView(int L)
        {
            //RYTTrackInstanse.AddTask(() => { RYTTrackInstanse.LogPageView(); });
            System.Reflection.MethodInfo AddTask_MethodInfo = TrackType.GetMethod("AddTask");
            Action LogPageView_Action = delegate() { TrackType.GetMethod("LogPageView").Invoke(RYTTrackInstanse,null); };
            AddTask_MethodInfo.Invoke(RYTTrackInstanse, new object[] { LogPageView_Action });
            return 0;
        }

        static int sendReport(int L)
        {
            //RYTTrackInstanse.sendReport();
            System.Reflection.MethodInfo sendReport_MethodInfo = TrackType.GetMethod("sendReport");
            sendReport_MethodInfo.Invoke(RYTTrackInstanse, null);
            return 0;
        }

        static int getPageId(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetPageId");
            string pageId = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, pageId);
            return 1;
        }
        static int setPageId(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String pageId = Lua.Lua_tostring(L, 2);
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("SetPageId");
            method.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { pageId });
            return 0;
        }
        static int getLoadStartTime(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetLoadStartTime");
            string loadStartTime = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, loadStartTime);
            return 1;
        }
        static int getLoadCompletedTime(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetLoadCompletedTime");
            string loadCompletedTime = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, loadCompletedTime);
            return 1;
        }
        static int getLastPageId(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetLastPageId");
            string lastPageId = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, lastPageId);
            return 1;
        }
        static int getLastLoadStartTime(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetLastLoadStartTime");
            string lastLoadStartTime = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, lastLoadStartTime);
            return 1;
        }
        static int getLastLoadCompletedTime(int L)
        {
            MethodInfo method = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetLastLoadCompletedTime");
            string lastLoadCompletedTime = method.Invoke(LuaManager.GetLuaManager(L).DetailV_, null) as string;
            Lua.Lua_pushstring(L, lastLoadCompletedTime);
            return 1;
        }

        private static List<KeyValuePair<string, string>> ParseTableToList(int L, int index)
        {
            bool bTable = Lua.Lua_istable(L, index);
            List<KeyValuePair<string, string>> keyValueList = null;
            if (bTable)
            {
                keyValueList = new List<KeyValuePair<string, string>>();
                int loopIndex = index < 0 ? index - 1 : index;
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, loopIndex) != 0)
                {
                    string key = parseValueString(L, -2);
                    string value = parseValueString(L, -1);

                    keyValueList.Add(new KeyValuePair<string, string>(key, value));
                    Lua.Lua_pop(L, 1);
                }
            }

            return keyValueList;
        }

        private static String parseValueString(int L, int index)
        {
            if (Lua.Lua_isnil(L, index))
            {
                return "";
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TNUMBER)
            {
                double value = Lua.Lua_tonumber(L, index);

                return Convert.ToString(value);
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TSTRING)
            {
                String value = Lua.Lua_tostring(L, index).ToString();
                return value;
            }
            else if (Lua.Lua_type(L, index) == Lua._LUA_TBOOLEAN)
            {
                return Lua.Lua_toboolean(L, index).ToString();
            }

            return "";
        }
    }
}

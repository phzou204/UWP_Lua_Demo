//
//  LuaSystem
//  RYTong
//
//  Created by wu.dong on 5/14/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaSystem
    {
        //private static Object detailV_ { get { return LuaManager.RootLuaManager.DetailV_; } }

        private LuaSystem() { }

        public static void SetCplus_Delegates()
        {
            LuaSystemDelegates.getInfo = new getInfoDel(getInfo);
            LuaSystemDelegates.openURL = new openURLDel(openURL);
        }

        /// <summary>
        /// “deviceID”：设备唯一ID，例：UDID，Mac地址，IMEI，IMSI等
        /// “name”：手机机主名称，没有则返回nil
        /// “model”：手机类型，例：iPhone，iPad，Nexus One等
        /// “environment”：程序运行环境，simulator/device
        /// “playform”：程序运行平台，Windows/iOS/Android
        /// “version”：手机操作系统版本号
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int getInfo(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            var param = Lua.Lua_tostring(L, 2).ToString();

            if (param.Equals("playform", StringComparison.CurrentCultureIgnoreCase) || param.Equals("platform", StringComparison.CurrentCultureIgnoreCase))
            {
                //Lua.Lua_pushstring(L,  "windows phone"); 
                Lua.Lua_pushstring(L, RYTDevice.OSPlatform);
                return 1;
            }
            else if (param.Equals("model", StringComparison.CurrentCultureIgnoreCase))
            {
                Lua.Lua_pushstring(L, RYTDevice.DeviceManufacturer + " " + RYTDevice.DeviceName);
                return 1;
            }
            else if (param.Equals("version", StringComparison.CurrentCultureIgnoreCase))
            {
                Lua.Lua_pushstring(L, RYTDevice.OSVersion);
                return 1;
            }
            else if (param.Equals("deviceID", StringComparison.CurrentCultureIgnoreCase))
            {
                Lua.Lua_pushstring(L, RYTDevice.DeviceUniqueId);
                return 1;
            }
            else if (param.Equals("name", StringComparison.CurrentCultureIgnoreCase))
            {
                Lua.Lua_pushstring(L, "unknown");
                //Lua.Lua_pushstring(L, RYTDevice.DeviceUserName);
                return 1;
            }
            else if (param.Equals("dpi", StringComparison.CurrentCultureIgnoreCase))
            {
                Lua.Lua_pushstring(L, "1");
                return 1;
            }
            else if (param.Equals("environment", StringComparison.CurrentCultureIgnoreCase))
            {
                var value = RYTDevice.DeviceName;
                if (value.Equals("XDeviceEmulator"))
                {
                    value = "simulator";
                }
                else
                {
                    value = "device";
                }

                Lua.Lua_pushstring(L, value);
                return 1;
            }

            return 0;
        }

        static int openURL(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            if (Lua.Lua_isstring(L, 2))
            {
                String url = Lua.Lua_tostring(L, 2).ToString();
                if (url.StartsWith("tel:"))
                {
                    String number = url.Substring("tel:".Length);
                    RYTTasks.StartPhoneCallTask(string.Empty, number);
                }
                else if (url.StartsWith("mailto:"))
                {
                    String to = url.Substring("mailto:".Length);
                    RYTTasks.StartEmailTask(string.Empty, to);
                }
                else if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    RYTTasks.StartWebBrowserTask(url);
                }
            }

            return 0;
        } 
    }
}

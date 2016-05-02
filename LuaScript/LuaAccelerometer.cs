using System;
using System.Windows;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using RYTong.FunctionLib;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaAccelerometer
    {
        //static RYTAccelerometer RYTAccelerometer;

        private LuaAccelerometer() { }

        //static double accelerometerInterval = -1;

        public static void SetCplus_Delegates()
        {
            LuaAccelerometerDelegates.startAccelerometer = new startAccelerometerDel(startAccelerometer);
            LuaAccelerometerDelegates.setListener = new setListenerDel(setListener);
            LuaAccelerometerDelegates.stopAccelerometer = new stopAccelerometerDel(stopAccelerometer);
            LuaAccelerometerDelegates.setAccelerometerInterval = new setAccelerometerIntervalDel(setAccelerometerInterval);
        }

        static int setListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            //if (RYTAccelerometer == null)
            //{
            //    RYTAccelerometer = new RYTAccelerometer();
            //}

            Action<Vector3> action = (vector) =>
            {
                Dictionary<String, object> dict = new Dictionary<string, object>();
                dict.Add("accelerometerX", vector.X);
                dict.Add("accelerometerY", vector.Y);
                dict.Add("accelerometerZ", vector.Z);
                
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithTableParam(callbackF, dict);
            };

            RYTAccelerometer.SetLisenter(action);

            return 0;
        }

        // 无用方法
        static int startAccelerometer(int L)
        {
            if (!Accelerometer.IsSupported)
            {
                MessageBox.Show("该手机不支持测震仪");
                return 0;
            }

            RYTAccelerometer.StartAccelerometer(-1, null, null);

            return 0;
        }

        // 无用方法
        static int setAccelerometerInterval(int L)
        {
            var accelerometerInterval = Lua.Lua_tonumber(L, 2);
            RYTAccelerometer.SetInterval(accelerometerInterval);

            return 0;
        }

        static int stopAccelerometer(int L)
        {
            //if (RYTAccelerometer != null)
            //{
                RYTAccelerometer.StopAccelerometer();
            //}

            return 0;
        }
    }
}

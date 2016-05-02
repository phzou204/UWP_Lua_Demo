//
//  LuaGps
//  RYTong
//
//  Created by wang.ping on 05/22/12.
//  Copyright 2012 RYTong. All rights reserved.
//

using System;
using System.Windows;
using RYTLuaCplusLib;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using System.Linq;

namespace RYTong.LuaScript
{
    public class LuaGps
    {
        private LuaGps() { }

        private static List<KeyValuePair<Geolocator, int>> _GeoChangedRegistry = null;

        public static void SetCplus_Delegates()
        {
            LuaGpsDelegates.startUpdateLocation = new startUpdateLocationDel(startUpdateLocation);
            LuaGpsDelegates.stopUpdateLocation = new stopUpdateLocationDel(stopUpdateLocation);
            LuaGpsDelegates.dispose = new Gps_disposeDel(dispose);
            LuaGpsDelegates.setListener = new Gps_setListenerDel(setListener);
        }

        static int startUpdateLocation(int L)
        {
            Geolocator watcher = new Windows.Devices.Geolocation.Geolocator();
            //System.Device.Location.GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            //watcher.Start();
            watcher.DesiredAccuracy = PositionAccuracy.Default;
            watcher.DesiredAccuracyInMeters = 10;
            watcher.MovementThreshold = 10;
            watcher.ReportInterval = 10;

            Lua.Lua_pushlightuserdata(L,watcher);

            return 1;
        }

        static int stopUpdateLocation(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            Windows.Devices.Geolocation.Geolocator watcher = (Windows.Devices.Geolocation.Geolocator)Lua.Lua_touserdata(L,-1);
            if (watcher != null && _GeoChangedRegistry != null)
            {
                _GeoChangedRegistry.RemoveAll(k => k.Key.Equals(watcher));
                watcher.PositionChanged -= GeoChangedCallback4Lua;

                //watcher.Stop();
            }

            return 0;
        }

        static int dispose(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            Geolocator watcher = (Geolocator)Lua.Lua_touserdata(L,-1);
            if (watcher != null && _GeoChangedRegistry != null)
            {
                _GeoChangedRegistry.RemoveAll(k => k.Key.Equals(watcher));
                watcher.PositionChanged -= GeoChangedCallback4Lua;
                watcher = null;
            }

            return 0;
        }

        static int setListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 1;

            Geolocator watcher = (Geolocator)Lua.Lua_touserdata(L,2);
            if (watcher != null)
            {
                try
                {
                    if (_GeoChangedRegistry == null)
                        _GeoChangedRegistry = new List<KeyValuePair<Geolocator, int>>();

                    int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                    if (!_GeoChangedRegistry.Any(k => k.Key.Equals(watcher)))
                        watcher.PositionChanged += GeoChangedCallback4Lua;
                    _GeoChangedRegistry.Add(new KeyValuePair<Geolocator, int>(watcher, callbackF));

                    //watcher.Start();
                }
                catch 
                {
                    LuaCommon.ShowError(null, "setListener failed @gps", "GPS");
                }
            }
            
            return 1;
        }

        private static void GeoChangedCallback4Lua(Geolocator sender, PositionChangedEventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Dictionary<string, object> parmDic = new Dictionary<string, object>();
                    parmDic.Add("latitude", Convert.ToString(args.Position.Coordinate.Latitude));
                    parmDic.Add("longitude", Convert.ToString(args.Position.Coordinate.Longitude));
                    if (_GeoChangedRegistry != null)
                    {
                        var queryList = _GeoChangedRegistry.Where(K => K.Key.Equals(sender));
                        foreach (var reg in queryList)
                        {
                            if (reg.Value != Lua._LUA_REFNIL)
                                LuaManager.RootLuaManager.ExecuteCallBackFunctionWithAnyParams(reg.Value, parmDic);
                        }
                    }
                });
        }

    }
}

using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Threading;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{
    public class LuaGesture
    {
        private static List<GestureEventRegisterInfo> EventsList = new List<GestureEventRegisterInfo>();

        private LuaGesture() { }

        public static void SetCplus_Delegates()
        {
            LuaGestureDelegates.setScaleListener = new setScaleListenerDel(setScaleListener);
            LuaGestureDelegates.setRotationListener = new setRotationListenerDel(setRotationListener);
            LuaGestureDelegates.setDragListener = new setDragListenerDel(setDragListener);
            LuaGestureDelegates.setSwipeListener = new setSwipeListenerDel(setSwipeListener);
            LuaGestureDelegates.setTapListener = new setTapListenerDel(setTapListener);
            LuaGestureDelegates.setLongTapListener = new setLongTapListenerDel(setLongTapListener);
        }

        static void ClearLastUselessEvents(FrameworkElement fe, string eventName)
        {
            var lisenter = GestureService.GetGestureListener(fe as FrameworkElement);
            List<GestureEventRegisterInfo> uselessEventsList_ = new List<GestureEventRegisterInfo>();
            foreach (var e in EventsList)
            {
                if (e.FE == fe as FrameworkElement)
                {
                    if (e.EventsName.Equals(eventName))
                    {
                        if (e.EventsName.Equals("setTapListener") && e.TapGestureEventHandler != null)
                        {
                            fe.Tap -= e.TapGestureEventHandler;
                            uselessEventsList_.Add(e);
                        }
                        else if (e.EventsName.Equals("setDoubleTapListener") && e.TapGestureEventHandler != null)
                        {
                            fe.DoubleTap -= e.TapGestureEventHandler;
                            uselessEventsList_.Add(e);

                        }
                        else if (e.EventsName.Equals("setLongTapListener") && e.GestureEventHandler != null)
                        {
                            lisenter.Hold -= e.GestureEventHandler;
                            uselessEventsList_.Add(e);
                        }
                        else if (e.EventsName.Equals("setSwipeListener") && e.FlickGestureEventHandler != null)
                        {
                            lisenter.Flick -= e.FlickGestureEventHandler;
                            uselessEventsList_.Add(e);
                        }
                        else if (e.EventsName.Equals("setDragListener") && e.DragDeltaGestureEventHandler != null)
                        {
                            fe.ManipulationDelta -= e.DragDeltaGestureEventHandler;
                            uselessEventsList_.Add(e);
                        }
                        else if (e.EventsName.Equals("setRotationListener") && e.RotationDeltaEventHandler != null)
                        {
                            fe.ManipulationDelta -= e.RotationDeltaEventHandler;
                            uselessEventsList_.Add(e);
                        }
                        else if (e.EventsName.Equals("setScaleListener") && e.ScaleDeltaEventHandler != null)
                        {
                            fe.ManipulationDelta -= e.ScaleDeltaEventHandler;
                            uselessEventsList_.Add(e);
                        }
                    }
                }
            }
            foreach (var e in uselessEventsList_)
            {
                EventsList.Remove(e);
            }
        }

        static int setScaleListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            var control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                double initialScale = 1;
                bool isPinch = false;
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);

                if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setScaleListener")))
                {
                    ClearLastUselessEvents(fe as FrameworkElement, "setScaleListener");
                }

                EventHandler<ManipulationDeltaEventArgs> scaleEventHandler = null;

                (fe as FrameworkElement).ManipulationDelta += scaleEventHandler = (sender, e) =>
                {
                    isPinch = e.PinchManipulation != null;
                    if (isPinch)
                    {
                        var scale = initialScale * e.PinchManipulation.CumulativeScale;
                        LogLib.RYTLog.Log("scale:" + scale);
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("scale", scale);
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                    }
                };

                //return 0;

                //var lisenter = GestureService.GetGestureListener(fe as FrameworkElement);
                //EventHandler<PinchGestureEventArgs> PinchCompletedEventHandler = null;
                //lisenter.PinchCompleted += PinchCompletedEventHandler = (s, e) =>
                //{
                //    var scale = initialScale * e.DistanceRatio;
                //    Dictionary<string, object> dict = new Dictionary<string, object>();
                //    dict.Add("scale", scale);

                //    LuaManager.Instance.ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                //};

                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setScaleListener", ScaleDeltaEventHandler = scaleEventHandler });

            }
            return 0;
        }

        /// <summary>
        /// 为视图设置当前旋转的[弧度值]监听方法。
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int setRotationListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            var control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);
                bool isPinch = false;
                //double initialAngle = 0;
               
                if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setRotationListener")))
                {
                    ClearLastUselessEvents(fe as FrameworkElement, "setRotationListener");
                }

                EventHandler<ManipulationDeltaEventArgs> rotationEventHandler = null;

                (fe as FrameworkElement).ManipulationDelta += rotationEventHandler = (sender, e) =>
                {
                    bool oldIsPinch = isPinch;
                    isPinch = e.PinchManipulation != null;

                    if (oldIsPinch && isPinch)
                    {
                        double angleDelta = GetAngle(e.PinchManipulation.Current) - GetAngle(e.PinchManipulation.Original);

                        //var value = (initialAngle += angleDelta);
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("rotation", angleDelta);
                        LogLib.RYTLog.Log("rotation----------------------------------:" + angleDelta.ToString());
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                    }
                };
                //LuaManager.GetLuaManager(L).
                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setRotationListener", RotationDeltaEventHandler = rotationEventHandler });
            }
            return 0;
        }

        static int setDragListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;
            var control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);
                bool isPinch = false;

                if (fe is FrameworkElement)
                {
                    if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setDragListener")))
                    {
                        ClearLastUselessEvents(fe as FrameworkElement, "setDragListener");
                    }

                    EventHandler<ManipulationDeltaEventArgs> dragDeltaEventHandler = null;

                    (fe as FrameworkElement).ManipulationDelta += dragDeltaEventHandler = (sender, e) =>
                        {
                            bool oldIsPinch = isPinch;
                            isPinch = e.PinchManipulation != null;

                            if (oldIsPinch == isPinch)
                            {
                                Dictionary<string, object> dict = new Dictionary<string, object>();
                                //dict.Add("x", (Math.Ceiling(e.DeltaManipulation.Translation.X / LuaManager.WidthScale)));
                                //dict.Add("y", (Math.Ceiling(e.DeltaManipulation.Translation.Y / LuaManager.HeightScale)));
                                dict.Add("x", e.DeltaManipulation.Translation.X / LuaManager.WidthScale);
                                dict.Add("y", e.DeltaManipulation.Translation.Y / LuaManager.HeightScale);
                                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                            }
                        };
                    EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setDragListener", DragDeltaGestureEventHandler = dragDeltaEventHandler });
                }
            }

            return 0;
        }

        static int setSwipeListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            Object control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);

                if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setSwipeListener")))
                {
                    ClearLastUselessEvents(fe as FrameworkElement, "setSwipeListener");
                }

                var lisenter = GestureService.GetGestureListener(fe as FrameworkElement);
                EventHandler<Microsoft.Phone.Controls.FlickGestureEventArgs> FlickGestureHandler = null;
                lisenter.Flick += FlickGestureHandler = (s, e) =>
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    string value = string.Empty;
                    if (e.Direction == Orientation.Horizontal)
                    {
                        if (e.HorizontalVelocity > 0)
                        {
                            value = "right";
                        }
                        else
                        {
                            value = "left";
                        }
                    }
                    else if (e.Direction == Orientation.Vertical)
                    {
                        if (e.VerticalVelocity > 0)
                        {
                            value = "down";
                        }
                        else
                        {
                            value = "up";
                        }
                    }
                    dict.Add("direction", value);

                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                };

                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setSwipeListener", FlickGestureEventHandler = FlickGestureHandler });
            }
            return 0;
        }

        static int setTapListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            var control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);

                if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setTapListener")))
                {
                    ClearLastUselessEvents(fe as FrameworkElement, "setTapListener");
                }

                EventHandler<System.Windows.Input.GestureEventArgs> tapGestureHandler = null;
                DispatcherTimer tapTimer = null;

                (fe as FrameworkElement).Tap += tapGestureHandler = (s, e) =>
                {
                    e.Handled = true;
                    tapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                    tapTimer.Tick += (sender, args) =>
                        {
                            (sender as DispatcherTimer).Stop();
                            Point p = e.GetPosition(null);
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict.Add("tag", 1);
                            dict.Add("x", p.X / LuaManager.WidthScale);
                            dict.Add("y", p.Y / LuaManager.HeightScale);
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                        };
                    tapTimer.Start();
                };
                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setTapListener", TapGestureEventHandler = tapGestureHandler });

                (fe as FrameworkElement).DoubleTap += tapGestureHandler = (s, e) =>
                {
                    e.Handled = true;

                    if (tapTimer != null && tapTimer.IsEnabled)
                    {
                        tapTimer.Stop();
                    }

                    Point p = e.GetPosition(null);
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("tag", 2);
                    dict.Add("x", p.X / LuaManager.WidthScale);
                    dict.Add("y", p.Y / LuaManager.HeightScale);
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);
                };
                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setDoubleTapListener", TapGestureEventHandler = tapGestureHandler });
            }
            return 0;
        }

        static int setLongTapListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            var control = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (control != null && callbackF != 0)
            {
                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);
                var lisenter = GestureService.GetGestureListener(fe as FrameworkElement);
                if (EventsList.Any(c => (c.FE == fe as FrameworkElement) && c.EventsName.Equals("setLongTapListener")))
                {
                    ClearLastUselessEvents(fe as FrameworkElement, "setLongTapListener");
                }

                EventHandler<System.Windows.Input.GestureEventArgs> holdGestureHandler = null;
                (fe as FrameworkElement).Hold += holdGestureHandler = (s, e) =>
                {
                    e.Handled = true;

                    Point p = e.GetPosition(null);
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("x", p.X / LuaManager.WidthScale);
                    dict.Add("y", p.Y / LuaManager.HeightScale);
                    LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, dict, control);

                };
                EventsList.Add(new GestureEventRegisterInfo() { FE = fe as FrameworkElement, EventsName = "setLongTapListener", LongTapGestureEventHandler = holdGestureHandler });

            }
            return 0;
        }

        /// <summary>
        /// 清除事件注册记录,防止内存泄露。
        /// </summary>
        public static void ClearUselessEventInfo()
        {
            EventsList.Clear();
        }

        private static double GetAngle(PinchContactPoints points)
        {
            Point directionVector = new Point(points.SecondaryContact.X - points.PrimaryContact.X, points.SecondaryContact.Y - points.PrimaryContact.Y);
            return GetAngle(directionVector.X, directionVector.Y);
        }

        private static double GetAngle(double x, double y)
        {
            double angle = Math.Atan2(y, x);

            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            return angle;// *180 / Math.PI;
        }
    }

    public class GestureEventRegisterInfo
    {
        public FrameworkElement FE { get; set; }
        public string EventsName { get; set; }
        public GestureType Type { get; set; }
        public List<EventHandler<EventArgs>> HanlderList { get; private set; }
        public GestureEventRegisterInfo()
        {
            HanlderList = new List<EventHandler<EventArgs>>();
        }

        public EventHandler<Microsoft.Phone.Controls.GestureEventArgs> GestureEventHandler { get; set; }
        public EventHandler<System.Windows.Input.GestureEventArgs> LongTapGestureEventHandler { get; set; }
        public EventHandler<System.Windows.Input.GestureEventArgs> TapGestureEventHandler { get; set; }
        public EventHandler<FlickGestureEventArgs> FlickGestureEventHandler { get; set; }
        public EventHandler<ManipulationDeltaEventArgs> DragDeltaGestureEventHandler { get; set; }
        public EventHandler<ManipulationDeltaEventArgs> ScaleDeltaEventHandler { get; set; }
        public EventHandler<ManipulationDeltaEventArgs> RotationDeltaEventHandler { get; set; }
    }

    public enum GestureType
    {
        Drag,
        LongTap,
        Tap,
        Rotation,
        Scale,
        Swip
    }

}

//
//  LuaElement
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;
using System.Collections;
using RYTLuaCplusLib;
using RYTong.LogLib;
using System.Diagnostics;
using RYTong.LuaScript.Helpers;
using System.Linq;
namespace RYTong.LuaScript
{
    public class LuaElement
    {
        private static List<TouchEventRegisterInfo> touchEventList = new List<TouchEventRegisterInfo>();
        private LuaElement() { }

        public static void SetCplusDelegates()
        {
            LuaElementDelegates.setAttribute = new setAttributeDel(setAttribute);
            LuaElementDelegates.getAttribute = new getAttributeDel(getAttribute);
            LuaElementDelegates.getPropertyByName = new getPropertyByNameDel(getPropertyByName);
            LuaElementDelegates.setPropertyByName = new setPropertyByNameDel(setPropertyByName);
            LuaElementDelegates.getStyleByName = new getStyleByNameDel(getStyleByName);
            LuaElementDelegates.setStyleByName = new setStyleByNameDel(setStyleByName);
            LuaElementDelegates.setInnerHTML = new setInnerHTMLDel(setInnerHTML);
            LuaElementDelegates.getParent = new getParentDel(getParent);
            LuaElementDelegates.getChildren = new getChildrenDel(getChildren);
            LuaElementDelegates.removeChild = new removeChildDel(removeChild);
            LuaElementDelegates.showLoading = new showLoadingDel(showLoading);
            LuaElementDelegates.stopLoading = new stopLoadingDel(stopLoading);
            LuaElementDelegates.submit = new submitDel(submit);
            LuaElementDelegates.setMatrix = new setMatrixDel(setMatrix);
            LuaElementDelegates.getMatrix = new getMatrixDel(getMatrix);
            LuaElementDelegates.setMapType = new setMapTypeDel(setMapType);
            LuaElementDelegates.setMapScrollEnabled = new setMapScrollEnabledDel(setMapScrollEnabled);
            LuaElementDelegates.setMapZoomEnabled = new setMapZoomEnabledDel(setMapZoomEnabled);
            LuaElementDelegates.addAnnotation = new addAnnotationDel(addAnnotation);
            LuaElementDelegates.getAllAnnotations = new getAllAnnotationsDel(getAllAnnotations);
            LuaElementDelegates.getUserLocation = new getUserLocationDel(getUserLocation);
            LuaElementDelegates.setCenter = new setCenterDel(setCenter);
            LuaElementDelegates.appendChild = new appendChildDel(appendChild);
            LuaElementDelegates.insertBefore = new insertBeforeDel(insertBefore);
            LuaElementDelegates.gc = new gcDel(__gc);
            LuaElementDelegates.setOnClickListener = new setOnClickListenerDel(setOnClickListener);
            LuaElementDelegates.setOnFocusListener = new setOnFocusListenerDel(setOnFocusListener);
            LuaElementDelegates.setOnBlurListener = new setOnBlurListenerDel(setOnBlurListener);
            LuaElementDelegates.setOnChangeListener = new setOnChangeListenerDel(setOnChangeListener);
            LuaElementDelegates.addEventListener = new addEventListenerDel(addEventListener);
            LuaElementDelegates.removeEventListener = new removeEventListenerDel(removeEventListener);
        }

        static int setAttribute(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String name = Lua.Lua_tostring(L, 2).ToString();
            String value = Lua.Lua_tostring(L, 3).ToString();

            var setResult = RYTKeyMapping.Instance.DoReflection(name, tag, value, ReflectionType.Set);
            if (setResult == null || setResult.ToString().Equals("true"))
            {
                RYTLog.ShowMessage(string.Format("在Control：[{0}]上设置属性为：[{1}]的值：[{2}]失败！", tag.ToString(), name, value),
                                    "LuaElement:setAttribute()");
            }


            return 0;
        }

        static int getAttribute(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String arrtName = Lua.Lua_tostring(L, 2).ToString();

            var value = RYTKeyMapping.Instance.DoReflection(arrtName, tag, null, ReflectionType.Get);
            if (value != null)
            {
                Lua.Lua_pushstring(L, value.ToString());
                return 1;
            }
            else
            {
                Lua.Lua_pushstring(L, string.Empty);
                //MessageBox.Show(string.Format("未在Control：[{0}]上取到属性为：[{1}]的值", tag.ToString(), arrtName),"LuaElement:getAttribute()", MessageBoxButton.OK);
            }


            return 1;
        }

        static int getPropertyByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }
            
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String arrtName = Lua.Lua_tostring(L, 2).ToString();

            if (string.IsNullOrEmpty(arrtName))
            {
                RYTLog.ShowMessage("getPropertyByName()参数不能为空！");
                Lua.Lua_pushnil(L);
                return 0;
            }

            object resultValue = null;
            try
            {
                resultValue = RYTKeyMapping.Instance.DoReflection(arrtName, tag, null, ReflectionType.Get);
            }
            catch (Exception e)
            {
                Debug.WriteLine("RYTKeyMapping.Instance.DoReflection() error .." + e.Message ?? "");
            }
            if (resultValue != null)
            {
                Lua.Lua_pushstring(L, resultValue.ToString());
                return 1;
            }
            else
            {
                Lua.Lua_pushstring(L, string.Empty);
            }

            return 1;
        }

        static int setPropertyByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String arrtName = Lua.Lua_tostring(L, 2).ToString();
            String value = string.Empty;

            value = Lua.Lua_tostring(L, 3);

            var setResult = RYTKeyMapping.Instance.DoReflection(arrtName, tag, value, ReflectionType.Set);
            if (setResult == null || setResult.ToString().Equals("true"))
            {
                LogLib.RYTLog.ShowMessage(string.Format("在Control：[{0}]上设置属性为：[{1}]的值：[{2}]失败！", tag.ToString(), arrtName, value),
                                "LuaElement:setPropertyByName()");
            }

            return 0;
        }

        static int getStyleByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
            {
                Lua.Lua_pushnil(L);
                return 1;
            }

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String arrtName = Lua.Lua_tostring(L, 2).ToString();
            Object style = null;
            FieldInfo tagCss = tag.GetType().GetField("CurrentCSSStyle_");
            if (tagCss != null)
            {
                style = tagCss.GetValue(tag);
                if (style != null)
                {
                    MethodInfo methods = style.GetType().GetMethod("getStyleByName");
                    Object[] pars = new Object[] { arrtName };
                    var obj = methods.Invoke(style, pars);
                    if (obj != null)
                    {
                        Lua.Lua_pushstring(L, obj.ToString());
                        return 1;
                    }
                }
            }

            var fe = LuaTransition.GetRYTControlView(tag);
            var str = GetDefaultStyle(fe, arrtName);
            Lua.Lua_pushstring(L, str);

            return 1;
        }

        private static string GetDefaultStyle(FrameworkElement fe, string styleName)
        {
            if (styleName.Equals("color"))
            {
                var pi = fe.GetType().GetProperty("Foreground");
                if (pi != null)
                {
                    var brush = pi.GetValue(fe, null) as SolidColorBrush;
                    return brush.Color.ToString();
                }
            }
            else if (styleName.Equals("left"))
            {
                return ((double)fe.GetValue(Canvas.LeftProperty) / LuaManager.WidthScale).ToString();
            }
            else if (styleName.Equals("top"))
            {
                return ((double)fe.GetValue(Canvas.TopProperty) / LuaManager.HeightScale).ToString();
            }
            else if (styleName.Equals("right"))
            {
                return "0";
            }
            else if (styleName.Equals("bottom"))
            {
                return "0";
            }
            else if (styleName.Equals("width"))
            {
                return fe.Width.ToString();
            }
            else if (styleName.Equals("height"))
            {
                return fe.Height.ToString();
            }
            else if (styleName.Equals("font-size"))
            {
                var pi = fe.GetType().GetProperty("FontSize");
                if (pi != null)
                {
                    return pi.GetValue(fe, null).ToString();
                }
            }
            else if (styleName.Equals("font-weight"))
            {
                var pi = fe.GetType().GetProperty("FontWeight");
                if (pi != null)
                {
                    return pi.GetValue(fe, null).ToString();
                }
            }
            else if (styleName.Equals("font-family"))
            {
                var pi = fe.GetType().GetProperty("FontFamily");
                if (pi != null)
                {
                    return pi.GetValue(fe, null).ToString();
                }
            }
            else if (styleName.Equals("display"))
            {
                return fe.Visibility == Visibility.Visible ? "block" : "none";
            }
            else if (styleName.Equals("position"))
            {
                return "static";
            }
            else if (styleName.Equals("filter"))
            {
                return "";
            }
            else if (styleName.Equals("text-align"))
            {
                return fe.HorizontalAlignment.ToString();
            }

            return string.Empty;
        }

        static int setStyleByName(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.String))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String attriName = Lua.Lua_tostring(L, 2).ToString();
            String value = Lua.Lua_tostring(L, 3).ToString();
            Object style = null;
            FieldInfo fi = tag.GetType().GetField("CurrentCSSStyle_");
            if (fi != null)
            {
                style = fi.GetValue(tag);
                if (style != null)
                {
                    MethodInfo methods = style.GetType().GetMethod("setStyleByName");
                    Object[] pars = new Object[] { attriName, value };
                    methods.Invoke(style, pars);
                }
                else
                {
                    MessageBox.Show("setStyleByName: 当前控件没有应用样式，无法设置样式");
                }
            }

            return 0;
        }

        static int setInnerHTML(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            String content = Lua.Lua_tostring(L, 2).ToString();
            if (tag != null && content != null)
            {
                MethodInfo methods = tag.GetType().GetMethod("setInnerHTML_new");
                Object[] pars = new Object[] { content };
                methods.Invoke(tag, pars);
            }

            return 0;
        }

        static int getParent(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            Object parent = null;
            FieldInfo field = tag.GetType().GetField("Parent_");
            if (field != null)
            {
                parent = field.GetValue(tag);
            }
            Lua.Lua_pushlightuserdata(L,parent);

            return 1;
        }

        static int getChildren(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            //FieldInfo field = tag.GetType().GetField("ChildrenElements_");
            MethodInfo mi = LuaManager.GetLuaManager(L).DetailV_.GetType().GetMethod("GetControlChildrenForLua");
            if (mi != null)
            {
                var list = mi.Invoke(LuaManager.GetLuaManager(L).DetailV_, new object[] { tag }) as IList;
                Lua.Lua_newtable(L);
                for (int i = 0; i < list.Count; i++)
                {
                    Lua.Lua_pushnumber(L, i + 1);
                    Lua.Lua_pushlightuserdata(L,list[i]);
                    Lua.LuaL_getmetatable(L, "elementFunction");
                    Lua.Lua_setmetatable(L, -2);
                    Lua.Lua_rawset(L, -3);
                }
            }

            return 1;
        }

        static int removeChild(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            Object control = Lua.Lua_touserdata(L,1);
            Object child = Lua.Lua_touserdata(L,2);

            bool bSucceed = false;
            if (control != null && child != null)
            {
                MethodInfo method = control.GetType().GetMethod("removeChild");
                if (method != null)
                {
                    var r = method.Invoke(control, new object[] { child });
                    bSucceed = (bool)r;
                }
            }

            Lua.Lua_pushboolean(L, bSucceed ? 1 : 0);
            return 1;
        }

        static int appendChild(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            Object control = Lua.Lua_touserdata(L,1);
            Object child = Lua.Lua_touserdata(L,2);

            bool bSucceed = false;
            if (child != null)
            {
                MethodInfo method = control.GetType().GetMethod("appendChild");
                if (method != null)
                {
                    var result = method.Invoke(control, new object[] { child });
                    bSucceed = (bool)result;
                }
            }

            Lua.Lua_pushboolean(L, bSucceed ? 1 : 0);
            return 1;
        }

        static int insertBefore(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.NUserData))
                return 0;

            object control = Lua.Lua_touserdata(L,1);
            object child = Lua.Lua_touserdata(L,2);
            object flagControl = null;
            if (Lua.Lua_isuserdata(L, 3))
                flagControl = Lua.Lua_touserdata(L,3);

            bool bSucceed = false;
            if (control != null && child != null)
            {
                MethodInfo method = control.GetType().GetMethod("insertBefore");
                if (method != null)
                {
                    var r = method.Invoke(control, new object[] { child, flagControl });
                    bSucceed = (bool)r;
                }
            }

            Lua.Lua_pushboolean(L, bSucceed ? 1 : 0);
            return 1;
        }

        static int showLoading(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            if (tag != null)
            {
                MethodInfo methods = tag.GetType().GetMethod("showLoading");
                if (methods != null)
                {
                    methods.Invoke(tag, null);
                }
            }

            return 0;
        }

        static int stopLoading(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            if (tag != null)
            {
                MethodInfo methods = tag.GetType().GetMethod("stopLoading");
                if (methods != null)
                {
                    methods.Invoke(tag, null);
                }
            }

            return 0;
        }

        static int submit(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            if (tag != null)
            {
                MethodInfo methods = tag.GetType().GetMethod("submit");
                if (methods != null)
                {   //only RYTFormControl haves submit.
                    methods.Invoke(tag, null);
                }
            }

            return 0;
        }

        static int setMatrix(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            bool isTable = Lua.Lua_istable(L, -1);
            if (isTable && tag != null)
            {
                var fe = LuaTransition.GetRYTControlView(tag);
                CompositeTransform tranF = fe.RenderTransform as CompositeTransform;
                if(tranF == null)
                {
                   fe.RenderTransform = tranF =  new CompositeTransform();
                }
                tranF.CenterX = fe.Width / 2;
                tranF.CenterY = fe.Height /2;

                Matrix mValue = new Matrix();
                Dictionary<String, String> dic = new Dictionary<string, string>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String value = Lua.Lua_tostring(L, -1).ToString();
                    String key = Lua.Lua_tostring(L, -2).ToString();
                    dic[key] = value;
                    Lua.Lua_pop(L, 1);

                    if (key.Equals("m11", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.M11 = double.Parse(value);
                        tranF.ScaleX = mValue.M11;
                    }
                    else if (key.Equals("m12", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.M12 = double.Parse(value);
                        tranF.SkewY = LuaManager.ConvertDegreesToRadians(Math.Atan(mValue.M12));
                    }
                    else if (key.Equals("m21", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.M21 = double.Parse(value);
                        tranF.SkewX = LuaManager.ConvertDegreesToRadians(Math.Atan(mValue.M21));
                    }
                    else if (key.Equals("m22", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.M22 = double.Parse(value);
                        tranF.ScaleY = mValue.M22;
                    }
                    else if (key.Equals("m31", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.OffsetX = double.Parse(value);
                        tranF.TranslateX = mValue.OffsetX * LuaManager.WidthScale;
                    }
                    else if (key.Equals("m32", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mValue.OffsetY = double.Parse(value) ;
                        tranF.TranslateY = mValue.OffsetY * LuaManager.HeightScale;
                    }
                }

                MethodInfo methods = tag.GetType().GetMethod("SetMatrix");
                methods.Invoke(tag, new object[] { mValue });
            }

            return 0;
        }

        static int getMatrix(int L)
        {
            Object tag = (Object)Lua.Lua_touserdata(L,1);
            if (tag != null)
            {
                MethodInfo methods = tag.GetType().GetMethod("GetMatrix");
                if (methods != null)
                {
                    Matrix m = (Matrix)methods.Invoke(tag, null);

                    Lua.Lua_newtable(L);

                    Lua.Lua_pushstring(L, "m11");
                    Lua.Lua_pushnumber(L, m.M11);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m12");
                    Lua.Lua_pushnumber(L, m.M12);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m13");
                    Lua.Lua_pushnumber(L, 0);
                    Lua.Lua_rawset(L, -3);

                    Lua.Lua_pushstring(L, "m21");
                    Lua.Lua_pushnumber(L, m.M21);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m22");
                    Lua.Lua_pushnumber(L, m.M22);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m23");
                    Lua.Lua_pushnumber(L, 0);
                    Lua.Lua_rawset(L, -3);

                    Lua.Lua_pushstring(L, "m31");
                    Lua.Lua_pushnumber(L, m.OffsetX);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m32");
                    Lua.Lua_pushnumber(L, m.OffsetY);
                    Lua.Lua_rawset(L, -3);
                    Lua.Lua_pushstring(L, "m33");
                    Lua.Lua_pushnumber(L, 1);
                    Lua.Lua_rawset(L, -3);

                    return 1;
                }
            }

            Lua.Lua_pushnil(L);
            return 1;
        }

        #region Map

        static int setMapType(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;

            object rytMapControl = Lua.Lua_touserdata(L,1);
            string mapTypeValue = Lua.Lua_tostring(L, 2).ToString();

            if (rytMapControl != null)
            {
                var mi = rytMapControl.GetType().GetMethod("SetMapType", new Type[] { typeof(string) });
                if (mi != null)
                {
                    mi.Invoke(rytMapControl, new object[] { mapTypeValue });
                }
            }

            return 0;
        }

        static int setMapScrollEnabled(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Boolean))
                return 0;

            object rytMapControl = Lua.Lua_touserdata(L,1);
            bool value = Lua.Lua_toboolean(L, 2);

            if (rytMapControl != null)
            {
                var mi = rytMapControl.GetType().GetMethod("SetMapScrollEnabled", new Type[] { typeof(bool) });
                if (mi != null)
                {
                    mi.Invoke(rytMapControl, new object[] { value });
                }
            }

            return 0;
        }

        static int setMapZoomEnabled(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Boolean))
                return 0;

            object rytMapControl = Lua.Lua_touserdata(L,1);
            bool value = Lua.Lua_toboolean(L, 2);

            if (rytMapControl != null)
            {
                var mi = rytMapControl.GetType().GetMethod("SetMapZoomEnabled", new Type[] { typeof(bool) });
                if (mi != null)
                {
                    mi.Invoke(rytMapControl, new object[] { value });
                }
            }

            return 0;
        }

        static int addAnnotation(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Number, LConst.Number, LConst.String, LConst.String))
                return 0;

            object rytMapControl = Lua.Lua_touserdata(L,1);
            double latitude = Lua.Lua_tonumber(L, 2);
            double longitude = Lua.Lua_tonumber(L, 3);
            string title = Lua.Lua_tostring(L, 4).ToString();
            string content = Lua.Lua_tostring(L, 5).ToString();

            MethodInfo mi = rytMapControl.GetType().GetMethod("SetPushpinAndView", new Type[] { typeof(double), typeof(double), typeof(string), typeof(string) });
            if (mi != null)
            {
                object[] param = new object[] { latitude, longitude, title, content };
                mi.Invoke(rytMapControl, param);
            }

            return 0;
        }

        static int getUserLocation(int L)
        {
            object rytMapControl = Lua.Lua_touserdata(L,1);

            MethodInfo mi = rytMapControl.GetType().GetMethod("ReturnLastViewPoint");
            object[] result = mi.Invoke(rytMapControl, null) as object[];
            if (result != null && result.Length == 4)
            {
                Lua.Lua_pushnumber(L, (double)result[0]);
                Lua.Lua_pushnumber(L, (double)result[1]);
                String title = result[2] as String;
                if (title != null)
                {
                    Lua.Lua_pushstring(L, title);
                }
                else
                {
                    Lua.Lua_pushnil(L);
                }
                String content = result[3] as String;
                if (content != null)
                {
                    Lua.Lua_pushstring(L, content);
                }
                else
                {
                    Lua.Lua_pushnil(L);
                }
            }

            return 4;
        }

        static int setCenter(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Number, LConst.Number))
                return 0;

            object rytMapControl = Lua.Lua_touserdata(L,1);
            double latitude = Lua.Lua_tonumber(L, 2);
            double longitude = Lua.Lua_tonumber(L, 3);

            MethodInfo mi = rytMapControl.GetType().GetMethod("setMapCenter");
            if (mi != null)
            {
                object[] param = new object[] { latitude, longitude };
                mi.Invoke(rytMapControl, param);
            }

            return 0;
        }

        /// <summary>
        /// 返回当前地图上所有坐标点信息。（仅地图控件可调用）
        /// </summary>
        /// <param name="lua"></param>
        /// <returns>包含地图上所有坐标点信息的数组。
        /// 数组中每个元素又是一个table类型数组，
        /// 每个数组可包含四个元素：
        /// {lat, lon, title, subtitle}，
        /// lat：Number-纬度（required），
        /// lon：Number-经度（required），
        /// title：String-坐标点标题（optional），
        /// subtitle：String-坐标点子标题（optional）。
        /// </returns>
        static int getAllAnnotations(int L)
        {
            object rytMapControl = Lua.Lua_touserdata(L,1);
            MethodInfo mi = rytMapControl.GetType().GetMethod("GetAllPushpins");
            if (mi != null)
            {
                IList<Pushpin> list = mi.Invoke(rytMapControl, null) as IList<Pushpin>;
                if (list != null)
                {
                    Lua.Lua_newtable(L);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Pushpin p = list[i];
                        Lua.Lua_pushnumber(L, i + 1);

                        Lua.Lua_newtable(L);
                        Lua.Lua_pushstring(L, "lat");
                        Lua.Lua_pushnumber(L, p.Location.Latitude);
                        Lua.Lua_rawset(L, -3);
                        Lua.Lua_pushstring(L, "lon");
                        Lua.Lua_pushnumber(L, p.Location.Longitude);
                        Lua.Lua_rawset(L, -3);
                        if (p.Content != null && !string.IsNullOrEmpty(p.Content as string))
                        {
                            var tempArray = (p.Content as string).Split(new string[] { "\n" }, StringSplitOptions.None);
                            if (tempArray.Length == 2)
                            {
                                Lua.Lua_pushstring(L, "title");
                                Lua.Lua_pushstring(L, tempArray[0]);
                                Lua.Lua_rawset(L, -3);
                                Lua.Lua_pushstring(L, "subtitle");
                                Lua.Lua_pushstring(L, tempArray[1]);
                                Lua.Lua_rawset(L, -3);
                            }
                            else
                            {
                                Lua.Lua_pushstring(L, "title");
                                Lua.Lua_pushstring(L, tempArray[0]);
                                Lua.Lua_rawset(L, -3);
                            }
                        }
                        Lua.Lua_rawset(L, -3);
                    }
                }
            }

            return 1;
        }

        static int __gc(int L)
        {


            return 0;
        }

        #endregion

        #region Track Event listener. added by zou.penghui 
        static int setOnClickListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            Action<object, string, string> callbackAction = null;
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
            {
                return 0;
            }
            callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            if (tag != null)
            {
                MethodInfo method = tag.GetType().GetMethod("setOnClickListener");
                if (method != null)
                {
                    method.Invoke(tag, new object[] { callbackAction, tag });
                }
            }
            return 0;
        }
        static int setOnFocusListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            Action<object, string, string> callbackAction = null;
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
            {
                return 0;
            }
            callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            if (tag != null)
            {
                MethodInfo method = tag.GetType().GetMethod("setOnFocusListener");
                if (method != null)
                {
                    method.Invoke(tag, new object[] { callbackAction, tag });
                }
            }
            return 0;
        }
        static int setOnBlurListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            Action<object, string, string> callbackAction = null;
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
            {
                return 0;
            }
            callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            if (tag != null)
            {
                MethodInfo method = tag.GetType().GetMethod("setOnBlurListener");
                if (method != null)
                {
                    method.Invoke(tag, new object[] { callbackAction, tag });
                }
            }
            return 0;
        }
        static int setOnChangeListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            Object tag = (Object)Lua.Lua_touserdata(L,1);
            Action<object, string, string> callbackAction = null;
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF == 0)
            {
                return 0;
            }
            callbackAction = (control, name, value) =>
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParamsSync(callbackF, control, name, value);
            };
            if (tag != null)
            {
                MethodInfo method = tag.GetType().GetMethod("setOnChangeListener");
                if (method != null)
                {
                    method.Invoke(tag, new object[] { callbackAction, tag });
                }
            }
            return 0;
        } 
        #endregion
        static int removeEventListener(int L)
        {           
            Object control = (Object)Lua.Lua_touserdata(L,1);
            string eventName = Lua.Lua_tostring(L, 2);
            var pi = control.GetType().GetProperty("View_");
            var fe = pi.GetValue(control, null);
            var touchEventRegisterInfo = touchEventList.FirstOrDefault(c => (c.FE == fe as FrameworkElement));
            if (touchEventRegisterInfo !=null)
            {
                if (eventName == "touchstart")
                {
                    if(touchEventRegisterInfo.TouchStartedEventHandler!=null)
                    {
                        (fe as FrameworkElement).ManipulationStarted -= touchEventRegisterInfo.TouchStartedEventHandler;
                    }
                }
                else if (eventName == "touchmove")
                {
                    if (touchEventRegisterInfo.TouchDeltaEventHandler != null)
                    {
                        (fe as FrameworkElement).ManipulationDelta -= touchEventRegisterInfo.TouchDeltaEventHandler;
                    }
                }
                else if (eventName == "touchend")
                {
                    if (touchEventRegisterInfo.TouchCompletedEventHandler != null)
                    {
                        (fe as FrameworkElement).ManipulationCompleted -= touchEventRegisterInfo.TouchCompletedEventHandler;
                    }
                }
                else if (eventName == "touchcancel")
                {
                    
                }
            }            
            return 0;
        }
        static int addEventListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            Object control = (Object)Lua.Lua_touserdata(L,1);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            string eventName = Lua.Lua_tostring(L, 2);
            if(control !=null && callbackF!=0)
            {

                var pi = control.GetType().GetProperty("View_");
                var fe = pi.GetValue(control, null);
                bool isPinch = false;
                if (fe is FrameworkElement)
                {                    

                    if (eventName.Equals("touchmove"))
                    {
                        EventHandler<ManipulationDeltaEventArgs> touchDeltaEventHandler = null;
                        TouchEventRegisterInfo touchInfo = null;
                        Dictionary<string, object> touchDict = null;
                        if (!touchEventList.Any(c => (c.FE == fe as FrameworkElement)))
                        {
                            touchInfo = new TouchEventRegisterInfo();
                            touchEventList.Add(touchInfo);
                            touchInfo.FE = fe as FrameworkElement;
                            touchDict = touchInfo.TouchDict;
                        }
                        else
                        {
                            touchInfo = touchEventList.FirstOrDefault(c => (c.FE == fe as FrameworkElement));
                            touchDict = touchInfo.TouchDict;
                        }
                        
                        (fe as FrameworkElement).ManipulationDelta += touchDeltaEventHandler = (sender, e) =>
                        {
                            bool oldIsPinch = isPinch;
                            isPinch = e.PinchManipulation != null;

                            if (oldIsPinch == isPinch)
                            {
                                double currentOffsetY = 0;
                                double page_OffsetY = 0;
                                object obj = e.OriginalSource;
                                Point OriginPoint = e.ManipulationOrigin;
                                Point currentPoint = new Point(0, 0);
                                Page currentPage = LuaCommon.FindCurrentPage(sender as FrameworkElement);
                                FrameworkElement container = e.ManipulationContainer as FrameworkElement;
                                if (container != null && container is ScrollViewer)
                                {
                                    ScrollViewer sv = container as ScrollViewer;
                                    currentOffsetY = sv.VerticalOffset;
                                }
                                if (currentPage != null)
                                {
                                    GeneralTransform generalTransform1 = currentPage.TransformToVisual(sender as UIElement);
                                    currentPoint = generalTransform1.Transform(new Point(0, 0));
                                    ScrollViewer sv = LuaCommon.FindVisualChild<ScrollViewer>(currentPage);
                                    page_OffsetY = sv.VerticalOffset;
                                }
                                touchDict["target"] = control;
                                touchDict["viewX"] = OriginPoint.X;
                                touchDict["viewY"] = OriginPoint.Y;                               
                                touchDict["contentX"] = OriginPoint.X;
                                touchDict["contentY"] = OriginPoint.Y + currentOffsetY;
                                touchDict["clientX"] = OriginPoint.X - currentPoint.X;
                                touchDict["clientY"] = OriginPoint.Y - currentPoint.Y;
                                touchDict["pageX"] = OriginPoint.X - currentPoint.X;
                                touchDict["pageY"] = OriginPoint.Y - currentPoint.Y + page_OffsetY;
                                RYTTouchEvent.TouchesDict["1"] = touchDict;
                                RYTTouchEvent.TargetTouchesDict["1"] = touchDict;
                                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, RYTTouchEvent.TouchEventDict);                                
                            }
                        };
                        touchInfo.TouchDeltaEventHandler = touchDeltaEventHandler;
                    }
                    else if (eventName.Equals("touchstart"))
                    {
                        EventHandler<ManipulationStartedEventArgs> touchStartedEventHandler = null;                        
                        TouchEventRegisterInfo touchInfo = null;
                        Dictionary<string, object> touchDict = null;
                        if (!touchEventList.Any(c => (c.FE == fe as FrameworkElement)))
                        {
                            touchInfo = new TouchEventRegisterInfo();
                            touchEventList.Add(touchInfo);
                            touchInfo.FE = fe as FrameworkElement;
                            touchDict = touchInfo.TouchDict;
                        }
                        else
                        {
                            touchInfo = touchEventList.FirstOrDefault(c => (c.FE == fe as FrameworkElement));
                            touchDict = touchInfo.TouchDict;
                        }
                        
                        (fe as FrameworkElement).ManipulationStarted += touchStartedEventHandler = (sender, e) =>
                        {
                            double currentOffsetY=0;
                            double page_OffsetY=0;
                            object obj = e.OriginalSource;
                            Point OriginPoint = e.ManipulationOrigin;
                            Point currentPoint = new Point(0, 0);
                            Page currentPage = LuaCommon.FindCurrentPage(sender as FrameworkElement);
                            FrameworkElement container = e.ManipulationContainer as FrameworkElement;
                            if (container != null && container is ScrollViewer)
                            {
                                ScrollViewer sv = container as ScrollViewer;
                                currentOffsetY = sv.VerticalOffset;
                            }                            
                            if(currentPage!=null)
                            {                                
                                GeneralTransform generalTransform1 = currentPage.TransformToVisual(sender as UIElement);
                                currentPoint = generalTransform1.Transform(new Point(0, 0));                              
                                ScrollViewer sv = LuaCommon.FindVisualChild<ScrollViewer>(currentPage);
                                page_OffsetY = sv.VerticalOffset;
                            }                            
                            touchDict["target"] = control;
                            touchDict["viewX"] = OriginPoint.X / LuaManager.WidthScale;
                            touchDict["viewY"] = OriginPoint.Y / LuaManager.HeightScale;
                            touchDict["contentX"] = OriginPoint.X / LuaManager.WidthScale;
                            touchDict["contentY"] = (OriginPoint.Y + currentOffsetY)/LuaManager.HeightScale;
                            touchDict["clientX"] = (OriginPoint.X - currentPoint.X) / LuaManager.WidthScale;
                            touchDict["clientY"] = (OriginPoint.Y - currentPoint.Y) / LuaManager.HeightScale;
                            touchDict["pageX"] = (OriginPoint.X - currentPoint.X) / LuaManager.WidthScale;
                            touchDict["pageY"] = (OriginPoint.Y - currentPoint.Y + page_OffsetY) / LuaManager.HeightScale;
                            RYTTouchEvent.TouchesDict["1"] = touchDict;
                            RYTTouchEvent.TargetTouchesDict["1"] = touchDict;
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, RYTTouchEvent.TouchEventDict);
                        };
                        touchInfo.TouchStartedEventHandler = touchStartedEventHandler;
                    }
                    else if (eventName.Equals("touchend"))
                    {
                        EventHandler<ManipulationCompletedEventArgs> dragCompletedEventHandler = null;
                        TouchEventRegisterInfo touchInfo = null;
                        Dictionary<string, object> touchDict = null;
                        if (!touchEventList.Any(c => (c.FE == fe as FrameworkElement)))
                        {
                            touchInfo = new TouchEventRegisterInfo();
                            touchEventList.Add(touchInfo);
                            touchInfo.FE = fe as FrameworkElement;
                            touchDict = touchInfo.TouchDict;
                        }
                        else
                        {
                            touchInfo = touchEventList.FirstOrDefault(c => (c.FE == fe as FrameworkElement));
                            touchDict = touchInfo.TouchDict;
                        }
                        (fe as FrameworkElement).ManipulationCompleted += dragCompletedEventHandler = (sender, e) =>
                        {
                            double currentOffsetY = 0;
                            double page_OffsetY = 0;
                            object obj = e.OriginalSource;
                            Point OriginPoint = e.ManipulationOrigin;
                            Point currentPoint = new Point(0, 0);
                            Page currentPage = LuaCommon.FindCurrentPage(sender as FrameworkElement);
                            FrameworkElement container = e.ManipulationContainer as FrameworkElement;
                            if (container != null && container is ScrollViewer)
                            {
                                ScrollViewer sv = container as ScrollViewer;
                                currentOffsetY = sv.VerticalOffset;
                            }
                            if (currentPage != null)
                            {
                                GeneralTransform generalTransform1 = currentPage.TransformToVisual(sender as UIElement);
                                currentPoint = generalTransform1.Transform(new Point(0, 0));
                                ScrollViewer sv = LuaCommon.FindVisualChild<ScrollViewer>(currentPage);
                                page_OffsetY = sv.VerticalOffset;
                            }
                            touchDict["target"] = control;
                            touchDict["viewX"] = OriginPoint.X / LuaManager.WidthScale;
                            touchDict["viewY"] = OriginPoint.Y / LuaManager.HeightScale;
                            touchDict["contentX"] = OriginPoint.X / LuaManager.WidthScale;
                            touchDict["contentY"] = (OriginPoint.Y + currentOffsetY) / LuaManager.HeightScale;
                            touchDict["clientX"] = (OriginPoint.X - currentPoint.X) / LuaManager.WidthScale;
                            touchDict["clientY"] = (OriginPoint.Y - currentPoint.Y) / LuaManager.HeightScale;
                            touchDict["pageX"] = (OriginPoint.X - currentPoint.X) / LuaManager.WidthScale;
                            touchDict["pageY"] = (OriginPoint.Y - currentPoint.Y + page_OffsetY) / LuaManager.HeightScale;
                            RYTTouchEvent.TouchesDict["1"] = touchDict;
                            RYTTouchEvent.TargetTouchesDict["1"] = touchDict;
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, RYTTouchEvent.TouchEventDict);                           
                        };
                    }
                    else if (eventName.Equals("touchcancel"))
                    {

                    }                   
                }                
            }
            
            
            return 0;
        } 
    }
    public class TouchEventRegisterInfo
    {
        public FrameworkElement FE { get; set; }
        private Dictionary<string, object> touchDict = new Dictionary<string, object>();
        public Dictionary<string, object> TouchDict { get { return touchDict; } }
        public TouchEventRegisterInfo()
        {            
            touchDict.Add("identifier", 0);
            touchDict.Add("target", FE);
            touchDict.Add("clientX", 0);
            touchDict.Add("clientY", 0);
            touchDict.Add("pageX", 0);
            touchDict.Add("pageY", 0);
            touchDict.Add("viewX", 0);
            touchDict.Add("viewY", 0);
            touchDict.Add("contentX", 0);
            touchDict.Add("contentY", 0);
            touchDict.Add("tapCount", 0);
        }        
        public EventHandler<ManipulationStartedEventArgs> TouchStartedEventHandler { get; set; }
        public EventHandler<ManipulationDeltaEventArgs> TouchDeltaEventHandler { get; set; }
        public EventHandler<ManipulationCompletedEventArgs> TouchCompletedEventHandler { get; set; }

    }
}

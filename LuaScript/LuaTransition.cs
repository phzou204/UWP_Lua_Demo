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
using System.Collections.Generic;
using System.Windows.Markup;
using System.Diagnostics;
using RYTLuaCplusLib;

namespace RYTong.LuaScript
{

    public class LuaTransition
    {
        static Dictionary<FrameworkElement, List<AnimationWrapperClass>> FromAnimationsDict = new Dictionary<FrameworkElement, List<AnimationWrapperClass>>();
        static Dictionary<FrameworkElement, RepeatBehavior> RepeatInfoDic = new Dictionary<FrameworkElement, RepeatBehavior>();
        static Dictionary<FrameworkElement, Dictionary<TranType, List<Storyboard>>> AllFeStoryBoard = new Dictionary<FrameworkElement, Dictionary<TranType, List<Storyboard>>>();

        public static void ClearTranInfoList()
        {
            FromAnimationsDict.Clear();
            RepeatInfoDic.Clear();
            AllFeStoryBoard.Clear();
        }

        public static void ClearStoryboard(bool isKeepAlphaState=false)
        {
            if(isKeepAlphaState)//when reload ,keep opacity
            {
                foreach (FrameworkElement fe in FromAnimationsDict.Keys)
                {
                    foreach (AnimationWrapperClass wrapper in FromAnimationsDict[fe])
                    {
                        if (wrapper.PropertyName == "alpha")
                        {
                            #region keep alpha state when css has value then use it 
                            object RYTControl = fe.Tag;
                            if (RYTControl == null)
                            {
                                double opacityVal = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                                fe.Opacity = opacityVal;
                                continue;
                            }

                            System.Reflection.FieldInfo css_FieldInfo = fe.Tag.GetType().GetField("CurrentCSSStyle_");
                            object css = css_FieldInfo.GetValue(RYTControl);
                            if (css == null)
                            {
                                double opacityVal = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                                fe.Opacity = opacityVal;
                                continue;
                            }

                            System.Reflection.FieldInfo filters_FieldInfo = css_FieldInfo.FieldType.GetField("filterInfo_");
                            object filterInfo = filters_FieldInfo.GetValue(css);
                            if (filterInfo == null)
                            {
                                double opacityVal = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                                fe.Opacity = opacityVal;
                                continue;
                            }

                            System.Reflection.PropertyInfo alpha_PropertyInfo = filters_FieldInfo.FieldType.GetProperty("IsHaveAlpha");
                            object isHaveAlpha = alpha_PropertyInfo.GetValue(filterInfo);
                            if (isHaveAlpha == null)
                            {
                                double opacityVal = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                                fe.Opacity = opacityVal;
                                continue;
                            }
                            bool flag = (bool)isHaveAlpha;

                            if (flag)
                            {
                                System.Reflection.PropertyInfo opacity_PropertyInfo = filters_FieldInfo.FieldType.GetProperty("Opcity4Control");
                                object opacity = opacity_PropertyInfo.GetValue(filterInfo);
                                double opacityVal = (double)opacity;
                                fe.Opacity = opacityVal;
                            }
                            else
                            {
                                double opacityVal = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                                fe.Opacity = opacityVal;
                            } 
                            #endregion                            
                        }
                    }
                }
                foreach (FrameworkElement fe in AllFeStoryBoard.Keys)
                {
                    foreach (TranType tt in AllFeStoryBoard[fe].Keys)
                    {                        
                        foreach (Storyboard sb in AllFeStoryBoard[fe][tt])
                        {
                            sb.Stop();
                        }

                    }
                }

                //foreach (FrameworkElement fe in FromAnimationsDict.Keys)
                //{
                //    foreach (AnimationWrapperClass wrapper in FromAnimationsDict[fe])
                //    {
                //        if (wrapper.PropertyName == "alpha")
                //        {
                //            double opacity = (double)fe.GetValue(FrameworkElement.OpacityProperty);
                //            fe.Opacity = opacity;
                //        }
                //    }
                //}
                //foreach (FrameworkElement fe in AllFeStoryBoard.Keys)
                //{
                //    foreach (TranType tt in AllFeStoryBoard[fe].Keys)
                //    {
                //        if (tt == TranType.alpha)
                //        {
                //            continue;
                //        }
                //        foreach (Storyboard sb in AllFeStoryBoard[fe][tt])
                //        {
                //            sb.Stop();
                //        }

                //    }
                //}

            }
            else 
            {
                foreach (FrameworkElement fe in AllFeStoryBoard.Keys)
                {
                    foreach (TranType tt in AllFeStoryBoard[fe].Keys)
                    {
                        foreach (Storyboard sb in AllFeStoryBoard[fe][tt])
                        {
                            sb.Stop();
                        }
                    }
                }
            }
            
        }      
        private LuaTransition() { }

        public static void SetCplus_Delegates()
        {
            LuaTransitionDelegates.from = new fromDel(from);
            LuaTransitionDelegates.to = new toDel(to);
            LuaTransitionDelegates.setCurve = new Transition_setCurveDel(setCurve);
            LuaTransitionDelegates.setRepeatCount = new Transition_setRepeatCountDel(setRepeatCount);
            LuaTransitionDelegates.translate = new translateDel(translate);
            LuaTransitionDelegates.translateX = new translateXDel(translateX);
            LuaTransitionDelegates.translateY = new translateYDel(translateY);
            LuaTransitionDelegates.scale = new scaleDel(scale);
            LuaTransitionDelegates.scaleX = new scaleXDel(scaleX);
            LuaTransitionDelegates.scaleY = new scaleYDel(scaleY);
            LuaTransitionDelegates.alpha = new alphaDel(alpha);
            LuaTransitionDelegates.rotate = new rotateDel(rotate);
            LuaTransitionDelegates.skew = new skewDel(skew);
            LuaTransitionDelegates.skewX = new skewXDel(skewX);
            LuaTransitionDelegates.skewY = new skewYDel(skewY);
            LuaTransitionDelegates.matrix = new matrixDel(matrix);
            LuaTransitionDelegates.setStartListener = new Transition_setStartListenerDel(setStartListener);
            LuaTransitionDelegates.setStopListener = new Transition_setStopListenerDel(setStopListener);
            LuaTransitionDelegates.pageTransition = new pageTransitionDel(pageTransition);
        }

        enum TranType { to, setCurve, translate, translateX, translateY, scale, scaleX, scaleY, alpha, rotate, skew, skewX, skewY, matrix }

        #region from, to, curve

        static bool CkeckFromToKey(string key)
        {
            string[] keys = new string[] { "x", "y", "width", "height", "alpha" };
            return keys.Any(k => k == key);
        }

        static int from(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Table))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var fe = GetRYTControlView(obj);
            double yReduceHeight = 0;
            CheckIsParentBodyControl(obj, out yReduceHeight);

            #region read table data
            bool isTable = Lua.Lua_istable(L, -1);
            Dictionary<string, object> tableDict = null;
            if (isTable)
            {
                tableDict = new Dictionary<string, object>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -2) != 0)
                {
                    String key = Lua.Lua_tostring(L, -2).ToString();

                    bool isValueTable = Lua.Lua_istable(L, -1);
                    if (isValueTable) // matrix table
                    {
                        Dictionary<string, double> valueDict = new Dictionary<string, double>();
                        Lua.Lua_pushnil(L);
                        while (Lua.Lua_next(L, -2) != 0)
                        {
                            String key1 = Lua.Lua_tostring(L, -2).ToString();
                            var value = Lua.Lua_tonumber(L, -1);
                            Lua.Lua_pop(L, 1);

                            valueDict.Add(key1, DoubleNumberIncrease(fe, key1, value, yReduceHeight));
                        }

                        tableDict.Add(key, valueDict);
                    }
                    else
                    {
                        if (!CkeckFromToKey(key))
                        {
                            return 0;
                        }
                        var value = Lua.Lua_tonumber(L, -1);
                        tableDict.Add(key, DoubleNumberIncrease(fe, key, value, yReduceHeight));
                    }

                    Lua.Lua_pop(L, 1);
                }
            }
            #endregion

            if (fe != null)
            {
                if (tableDict != null && tableDict.Count > 0)
                {
                    CreateDoubleAnimations(fe, tableDict, AnimationValueType.From);
                }
            }

            return 0;
        }

        static int to(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Table, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            double seconds = Lua.Lua_tonumber(L, 4);
            Dictionary<string, object> tableDict = null;
            var fe = GetRYTControlView(obj);
            double yReduceHeight = 0;
            CheckIsParentBodyControl(obj, out yReduceHeight);

            #region read table data
            bool isTable = Lua.Lua_istable(L, -2);
            if (isTable)
            {
                tableDict = new Dictionary<string, object>();
                Lua.Lua_pushnil(L);
                while (Lua.Lua_next(L, -3) != 0)
                {
                    String key = Lua.Lua_tostring(L, -2).ToString();

                    bool isValueTable = Lua.Lua_istable(L, -1);
                    if (isValueTable) // matrix table
                    {
                        Dictionary<string, double> valueDict = new Dictionary<string, double>();
                        Lua.Lua_pushnil(L);
                        while (Lua.Lua_next(L, -2) != 0)
                        {
                            String key1 = Lua.Lua_tostring(L, -2).ToString();
                            var value = Lua.Lua_tonumber(L, -1);
                            Lua.Lua_pop(L, 1);

                            valueDict.Add(key1, DoubleNumberIncrease(fe, key1, value, yReduceHeight));
                        }

                        tableDict.Add(key, valueDict);
                    }
                    else
                    {
                        if (!CkeckFromToKey(key))
                        {
                            return 0;
                        }
                        var value = Lua.Lua_tonumber(L, -1);
                        tableDict.Add(key, DoubleNumberIncrease(fe, key, value, yReduceHeight));
                    }

                    Lua.Lua_pop(L, 1);
                }
            }
            #endregion

            if (fe != null)
            {
                if (tableDict != null && tableDict.Count > 0) //matrix
                {
                    Storyboard sb = new Storyboard();
                    var proDict = tableDict as Dictionary<string, object>;
                    if (proDict != null)
                    {
                        CreateDoubleAnimations(fe, proDict, AnimationValueType.To, FromAnimationsDict.ContainsKey(fe) ? FromAnimationsDict[fe] : null);

                        foreach (var key in tableDict.Keys)
                        {
                            var wrapper = FromAnimationsDict[fe].FirstOrDefault(c => c.PropertyName.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                            if (wrapper == null)
                            {
                                Debug.WriteLine("LuaTransition to , wraper is null .");
                                continue;
                            }
                            if (key.Equals("matrix", StringComparison.CurrentCultureIgnoreCase))
                            {
                                foreach (var mInfo in wrapper.MatrixInfoList)
                                {
                                    if (!mInfo.DA.To.HasValue)
                                    {
                                        mInfo.DA.To = mInfo.DA.From.Value;
                                    }
                                    if (mInfo.PropertyName.Equals("m31", StringComparison.CurrentCultureIgnoreCase))
                                    { 
                                        var _x = FromAnimationsDict[fe].FirstOrDefault(c => c.PropertyName.Equals("x", StringComparison.CurrentCultureIgnoreCase));
                                        if (_x != null)
                                        {
                                            sb.Children.Remove(_x.DA);
                                        }
                                    }
                                    else if (mInfo.PropertyName.Equals("m32", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        var _y = FromAnimationsDict[fe].FirstOrDefault(c => c.PropertyName.Equals("y", StringComparison.CurrentCultureIgnoreCase));
                                        if (_y != null)
                                        {
                                            sb.Children.Remove(_y.DA);
                                        }
                                    }
                                    SetMatrixStoryboard(fe, mInfo.PropertyName, seconds, mInfo, sb);
                                }

                            }
                            else
                            {
                                wrapper.DA.Duration = TimeSpan.FromSeconds(seconds);
                                sb.Children.Add(wrapper.DA);
                            }
                        }

                        SetStoryboardRepeatBehavior(fe, sb);
                        SetStoryboardCurve(fe, null, sb);
                        RegisterCallBackFunctions(L, fe, sb);
                        TryBeginStoryBoard(fe, sb, TranType.to);
                    }
                }
            }

            return 0;
        }

        private static double DoubleNumberIncrease(FrameworkElement fe, string key, double value, double yReduceHieght)
        {
            if (key.Equals("x", StringComparison.CurrentCultureIgnoreCase))
            {
                double x = value * LuaManager.WidthScale;

                if (fe != null)
                {
                    var left = (double)fe.GetValue(Canvas.LeftProperty);
                    x -= left;
                }
                return x;
            }
            else if (key.Equals("width", StringComparison.CurrentCultureIgnoreCase))
            {
                return value * LuaManager.WidthScale;
            }
            else if (key.Equals("y", StringComparison.CurrentCultureIgnoreCase))
            {
                double y = value * LuaManager.HeightScale;

                if (fe != null)
                {
                    var top = (double)fe.GetValue(Canvas.TopProperty);
                    if (yReduceHieght > 0)
                        top += yReduceHieght;
                    y -= top;
                }
                return y;
            }
            else if (key.Equals("height", StringComparison.CurrentCultureIgnoreCase))
            {
                return value * LuaManager.HeightScale;
            }

            return value;
        }

        /// <summary>
        /// curve：动画展示速度效果
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int setCurve(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            int num = (int)Lua.Lua_tonumber(L, 3);
            CurveType cType = (CurveType)num;

            if (obj != null)
            {
                var fe = GetRYTControlView(obj);
                if (FromAnimationsDict.ContainsKey(fe))
                {
                    foreach (var info in FromAnimationsDict[fe])
                    {
                        if (info.DA == null)
                        {
                            info.CurveType = cType;
                        }
                        else
                        {
                            if (info.MatrixInfoList != null)
                            {
                                info.MatrixInfoList.ForEach(c => SetCurveTypeToDA(c.DA, cType));
                            }
                            else
                            {
                                SetCurveTypeToDA(info.DA, cType);
                            }
                        }
                    }
                }
                else
                {
                    List<AnimationWrapperClass> listAnimation = new List<AnimationWrapperClass>() { new AnimationWrapperClass{CurveType=cType,Sender=obj}};
                    FromAnimationsDict.Add(fe, listAnimation);
                }
            }

            return 0;
        }

        #endregion

        #region setRepeatCount

        static int setRepeatCount(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var count = Lua.Lua_tonumber(L, 3);

            if (obj != null)
            {
                RepeatBehavior rb = new RepeatBehavior(1);
                if (count == -1)
                {
                    rb = RepeatBehavior.Forever;
                }
                else if (count > 0)
                {
                    rb = new RepeatBehavior(count);
                }
                else
                {
                    LuaCommon.ShowError(null, "传入参数值[" + count + "]不合法", "setRepeatCount");
                }

                var fe = GetRYTControlView(obj);
                if (RepeatInfoDic.ContainsKey(fe))
                {
                    RepeatInfoDic[fe] = rb;
                }
                else
                {
                    RepeatInfoDic.Add(fe, rb);
                }
            }

            return 0;
        }

        #endregion

        #region translate

        static int translate(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.NNumber, LConst.NNumber, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var x = Lua.Lua_tonumber(L, 3);
            var y = Lua.Lua_tonumber(L, 4);
            var duration = Lua.Lua_tonumber(L, 5);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var XfromValue = (double)comTF.GetValue(CompositeTransform.TranslateXProperty);
                    var YfromValue = (double)comTF.GetValue(CompositeTransform.TranslateYProperty);

                    x = XfromValue + x * LuaManager.WidthScale;
                    y = YfromValue + y * LuaManager.HeightScale;

                    var daX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateXProperty, duration, null, x, sb);
                    var daY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateYProperty, duration, null, y, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateX", comTF.TranslateX);
                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateY", comTF.TranslateY);

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, null, sb);
                    RegisterCallBackFunctions(L, fe, sb);
                    //sb.Begin();
                    TryBeginStoryBoard(fe, sb, TranType.translate);
                }
            }

            return 0;
        }

        static int translateX(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var x = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var XfromValue = (double)comTF.GetValue(CompositeTransform.TranslateXProperty);

                    x = XfromValue + x * LuaManager.WidthScale;
                    var daX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateXProperty, duration, null, x, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateX", comTF.TranslateX);

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, daX, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.translateX);
                }
            }

            return 0;
        }

        static int translateY(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var y = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();
                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var YfromValue = (double)comTF.GetValue(CompositeTransform.TranslateYProperty);
                    y = YfromValue + y * LuaManager.HeightScale;
                    var daY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateYProperty, duration, null, y, sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateY", comTF.TranslateY);
                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, daY, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.translateY);
                }
            }

            return 0;
        }

        #endregion

        #region scale

        public const double HeightScale = 2.7;

        static int scale(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var x = Lua.Lua_tonumber(L, 3);
            var y = Lua.Lua_tonumber(L, 4);
            var duration = Lua.Lua_tonumber(L, 5);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var XfromValue = (double)comTF.GetValue(CompositeTransform.ScaleXProperty);
                    var YfromValue = (double)comTF.GetValue(CompositeTransform.ScaleYProperty);

                    var scaleX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleXProperty, duration, null, XfromValue * x, sb);
                    var scaleY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleYProperty, duration, null, YfromValue * y, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleX", comTF.ScaleX);
                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleY", comTF.ScaleY);

                    if (fe.Height > 0 & fe.Width > 0)
                    {
                        comTF.CenterX = fe.Width / 2;
                        comTF.CenterY = fe.Height / 2;
                    }

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, null, sb);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.scale);
                }
            }

            return 0;
        }

        static int scaleX(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var x = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var fromValue = (double)comTF.GetValue(CompositeTransform.ScaleXProperty);

                    var scaleX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleXProperty, duration, null, fromValue * x, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleX", comTF.ScaleX);

                    if (fe.Height > 0 & fe.Width > 0)
                    {
                        comTF.CenterX = fe.Width / 2;
                        comTF.CenterY = fe.Height / 2;
                    }

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, scaleX, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.scaleX);
                }
            }

            return 0;
        }

        static int scaleY(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var y = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    var fromValue = (double)comTF.GetValue(CompositeTransform.ScaleYProperty);

                    var scaleY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleYProperty, duration, null, fromValue * y, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleY", comTF.ScaleY);

                    if (fe.Height > 0 & fe.Width > 0)
                    {
                        comTF.CenterX = fe.Width / 2;
                        comTF.CenterY = fe.Height / 2;
                    }

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, scaleY, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.scaleY);
                }
            }

            return 0;
        }

        #endregion

        #region alpha

        static int alpha(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var alpha = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                if (alpha >= 0 && alpha <= 1)
                {
                    FrameworkElement fe = GetRYTControlView(obj);
                    if (fe != null)
                    {
                        Storyboard sb = new Storyboard();

                        DoubleAnimation alphaDa = new DoubleAnimation();
                        alphaDa.From = fe.Opacity;
                        alphaDa.To = alpha;
                        alphaDa.Duration = TimeSpan.FromSeconds(duration);
                        Storyboard.SetTarget(alphaDa, fe);
                        Storyboard.SetTargetProperty(alphaDa, new PropertyPath(FrameworkElement.OpacityProperty));
                        sb.Children.Add(alphaDa);

                        SetRYTControlRestoreAnimationKeyValue(obj, "Opacity", fe.Opacity);

                        SetStoryboardRepeatBehavior(fe, sb);
                        SetStoryboardCurve(fe, alphaDa, null);
                        RegisterCallBackFunctions(L, fe, sb);
                        //sb.Begin();
                        TryBeginStoryBoard(fe, sb, TranType.alpha);
                    }
                }
                else
                {
                    LuaCommon.ShowError(null, "alpha值不正确", "alpha动画");
                }
            }

            return 0;
        }

        #endregion

        #region rotate

        static object syncObj = new object();

        static int rotate(int L)
        {
            lock (syncObj)
            {
                try
                {
                    if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.String, LConst.Number))
                        return 0;

                    object obj = Lua.Lua_touserdata(L,2);
                    var angle = Lua.Lua_tonumber(L, 3);
                    var axis = Lua.Lua_tostring(L, 4).ToString();
                    var duration = Lua.Lua_tonumber(L, 5);

                    if (obj != null)
                    {
                        var fe = GetRYTControlView(obj);
                        if (fe != null)
                        {
                            PlaneProjection plane = null;
                            CompositeTransform rotateTF = null;
                            if (fe.Projection != null && fe.Projection is PlaneProjection)
                            {
                                plane = fe.Projection as PlaneProjection;
                            }
                            else
                            {
                                plane = new PlaneProjection();
                                fe.Projection = plane;
                            }

                            Storyboard sb = new Storyboard();

                            DoubleAnimation da = new DoubleAnimation();
                            da.Duration = TimeSpan.FromSeconds(duration);
                            Storyboard.SetTarget(da, plane);

                            if (axis.Equals("x", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Storyboard.SetTargetProperty(da, new PropertyPath(PlaneProjection.RotationXProperty));

                                da.To = plane.RotationX - angle;
                                SetRYTControlRestoreAnimationKeyValue(obj, "RotationX", plane.RotationX);
                            }
                            else if (axis.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Storyboard.SetTargetProperty(da, new PropertyPath(PlaneProjection.RotationYProperty));
                                da.To = plane.RotationY - angle;
                                SetRYTControlRestoreAnimationKeyValue(obj, "RotationY", plane.RotationY);
                            }
                            else if (axis.Equals("z", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (fe.RenderTransform != null && fe.RenderTransform is CompositeTransform)
                                {
                                    rotateTF = fe.RenderTransform as CompositeTransform;
                                }
                                else
                                {
                                    rotateTF = new CompositeTransform();
                                    fe.RenderTransform = rotateTF;
                                }
                                rotateTF.CenterX = fe.ActualWidth / 2;
                                rotateTF.CenterY = fe.ActualHeight / 2;

                                Storyboard.SetTargetProperty(da, new PropertyPath(CompositeTransform.RotationProperty));
                                Storyboard.SetTarget(da, rotateTF);

                                //在空间中旋转对象的[角度]
                                //da.From = plane.RotationZ;
                                //da.To = rotateTF.Angle + angle;

                                //da.To = angle;
                                da.To = rotateTF.Rotation + angle;
                                Debug.WriteLine("angle:" + angle);
                                SetRYTControlRestoreAnimationKeyValue(obj, "RotationZ", rotateTF.Rotation);
                            }

                            sb.Children.Add(da);

                            SetStoryboardRepeatBehavior(fe, sb);
                            SetStoryboardCurve(fe, null, sb);
                            RegisterCallBackFunctions(L, fe, sb);
                            //sb.Begin();
                            TryBeginStoryBoard(fe, sb, TranType.rotate);
                        }
                    }

                    return 0;
                }
                catch (Exception)
                {
                    Debug.WriteLine("rotate error");
                    return 0;
                }
            }
        }

        #endregion

        #region skew

        static int skew(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var xangle = Lua.Lua_tonumber(L, 3);
            var yangle = Lua.Lua_tonumber(L, 4);
            var duration = Lua.Lua_tonumber(L, 5);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    if (fe.ActualHeight != 0 && fe.ActualWidth != 0)
                    {
                        comTF.CenterX = fe.ActualWidth / 2;
                        comTF.CenterY = fe.ActualHeight / 2;
                    }

                    var XfromValue = (double)comTF.GetValue(CompositeTransform.SkewXProperty);
                    var YfromValue = (double)comTF.GetValue(CompositeTransform.SkewYProperty);

                    var xToValue = XfromValue + xangle;
                    var yToValue = YfromValue + yangle;

                    // 2015/09/06 经检测未出现问题因此注释掉（#11385），change by du.yanjie
                    //xToValue = ConvertSkewValue(xToValue, 5, true);
                    //yToValue = ConvertSkewValue(yToValue, 5, true);

                    var daX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewXProperty, duration, XfromValue, xToValue, sb);
                    var daY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewYProperty, duration, YfromValue, yToValue, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewX", comTF.SkewX);
                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewY", comTF.SkewY);

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, null, sb);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.skew);
                }
            }

            return 0;
        }

        static double ConvertSkewValue(double value, double reduce = 0 , bool bSkewXY = false)
        {
            var limit = 90 - reduce;
            if (value >= limit && value < 90)
            {
                return limit;
            }
            else if (value <= -limit && value > -90)
            {
                return -limit;
            }
            else if (bSkewXY && Math.Abs(value) == 90)
            {
                return value > 0 ? limit : -limit;
            }

            return value;
        }

        static int skewX(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var xangle = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    if (fe.ActualHeight != 0 && fe.ActualWidth != 0)
                    {
                        comTF.CenterX = fe.ActualWidth / 2;
                        comTF.CenterY = fe.ActualHeight / 2;
                    }
                    var XfromValue = (double)comTF.GetValue(CompositeTransform.SkewXProperty);
                    var xToValue = XfromValue + xangle;

                    //xToValue = ConvertSkewValue(xToValue, 5);

                    var daX = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewXProperty, duration, XfromValue, xToValue, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewX", comTF.SkewX);

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, daX, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.skewX);
                }
            }

            return 0;
        }

        static int skewY(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var yangle = Lua.Lua_tonumber(L, 3);
            var duration = Lua.Lua_tonumber(L, 4);

            if (obj != null)
            {
                FrameworkElement fe = GetRYTControlView(obj);
                if (fe != null)
                {
                    Storyboard sb = new Storyboard();

                    CompositeTransform comTF = GetCompositeTransform(fe);
                    if (fe.ActualHeight != 0 && fe.ActualWidth != 0)
                    {
                        comTF.CenterX = fe.ActualWidth / 2;
                        comTF.CenterY = fe.ActualHeight / 2;
                    }
                    var YfromValue = (double)comTF.GetValue(CompositeTransform.SkewYProperty);

                    var yToValue = YfromValue + yangle;
                    //yToValue = ConvertSkewValue(yToValue, 5);

                    var daY = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewYProperty, duration, YfromValue, yToValue, sb);

                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewY", comTF.SkewY);

                    SetStoryboardRepeatBehavior(fe, sb);
                    SetStoryboardCurve(fe, daY, null);
                    RegisterCallBackFunctions(L, fe, sb);
                    TryBeginStoryBoard(fe, sb, TranType.skewY);
                }
            }

            return 0;
        }

        #endregion

        #region matrix

        static int matrix(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Table, LConst.Number))
                return 0;

            #region Read matrix table
            object obj = Lua.Lua_touserdata(L,2);
            var duration = Lua.Lua_tonumber(L, 4);

            bool isTable = Lua.Lua_istable(L, -2);
            if (obj == null || !isTable)
            {
                return 0;
            }

            Dictionary<string, double> valueDict = new Dictionary<string, double>();

            Lua.Lua_pushnil(L);
            while (Lua.Lua_next(L, -3) != 0)
            {
                string key = Lua.Lua_tostring(L, -2).ToString();

                bool isValueTable = Lua.Lua_istable(L, -1);
                if (isValueTable)
                {
                    Lua.Lua_pushnil(L);
                    while (Lua.Lua_next(L, -2) != 0)
                    {
                        string key1 = Lua.Lua_tostring(L, -2).ToString();
                        var value = Lua.Lua_tonumber(L, -1);
                        Lua.Lua_pop(L, 1);

                        valueDict.Add(key1, value);
                    }
                }
                else
                {
                    var value = Lua.Lua_tonumber(L, -1);
                    valueDict.Add(key, value);
                }

                Lua.Lua_pop(L, 1);
            }

            var fe = GetRYTControlView(obj);
            if (fe == null || valueDict == null || valueDict.Count == 0)
            {
                return 0;
            }
            #endregion

            Storyboard sb = new Storyboard();
            bool bEventRegister = false;

            CompositeTransform comTF = fe.RenderTransform as CompositeTransform;

            foreach (var key in valueDict.Keys)
            {
                if (comTF == null)
                {
                    comTF = new CompositeTransform();
                    fe.RenderTransform = comTF;
                }

                var value = valueDict[key];
                DoubleAnimation da = null;

                if (key.Equals("m11", StringComparison.CurrentCultureIgnoreCase)) // x scale
                {
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleXProperty, duration, null, value, sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleX", comTF.ScaleX);
                }
                else if (key.Equals("m12", StringComparison.CurrentCultureIgnoreCase)) // y skew
                {
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewYProperty, duration, null, LuaManager.ConvertDegreesToRadians(Math.Atan(value)), sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewY", comTF.SkewY);
                }
                else if (key.Equals("m21", StringComparison.CurrentCultureIgnoreCase)) // x skew
                {
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewXProperty, duration, null, LuaManager.ConvertDegreesToRadians(Math.Atan(value)), sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "SkewX", comTF.SkewX);
                }
                else if (key.Equals("m22", StringComparison.CurrentCultureIgnoreCase)) // y scale
                {
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleYProperty, duration, null, value, sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "ScaleY", comTF.ScaleY);
                }
                else if (key.Equals("m31", StringComparison.CurrentCultureIgnoreCase)) // x translation
                {
                    value *= LuaManager.WidthScale;
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateXProperty, duration, null, value, sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateX", comTF.TranslateX);
                }
                else if (key.Equals("m32", StringComparison.CurrentCultureIgnoreCase)) //y translation
                {
                    value *= LuaManager.HeightScale;
                    da = CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateYProperty, duration, null, value, sb);
                    SetRYTControlRestoreAnimationKeyValue(obj, "TranslateY", comTF.TranslateY);
                }

                if (da != null && !bEventRegister)
                {
                    bEventRegister = true;
                    SetStoryboardRepeatBehavior(fe, sb);
                    RegisterCallBackFunctions(L, fe, sb);
                }
            }

            TryBeginStoryBoard(fe, sb, TranType.matrix);

            return 0;
        }

        #endregion

        #region Set Listener

        /// <summary>
        /// 在动画开始前调用该监听方法
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int setStartListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (obj != null && callbackF != 0)
            {
                var fe = GetRYTControlView(obj);
                if (FromAnimationsDict.ContainsKey(fe))
                {
                    var list = FromAnimationsDict[fe];
                    list.ForEach(c => { c.StartFunctionId = callbackF; c.Sender = obj; });
                }
                else
                {
                    FromAnimationsDict.Add(fe, new List<AnimationWrapperClass>() { new AnimationWrapperClass() { StartFunctionId = callbackF, Sender = obj } });
                }
            }

            return 0;
        }

        /// <summary>
        /// 在动画结束时调用该监听方法
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int setStopListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);

            if (obj != null && callbackF != 0)
            {
                var fe = GetRYTControlView(obj);
                if (FromAnimationsDict.ContainsKey(fe))
                {
                    var list = FromAnimationsDict[fe];
                    list.ForEach(c => { c.StopFunctionId = callbackF; c.Sender = obj; });
                }
                else
                {
                    FromAnimationsDict.Add(fe, new List<AnimationWrapperClass>() { new AnimationWrapperClass() { StopFunctionId = callbackF, Sender = obj } });
                }
            }

            return 0;
        }

        #endregion

        #region pageTransition

        static int pageTransition(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.UserData, LConst.UserData))
                return 0;

            var page1 = Lua.Lua_touserdata(L,2);
            var page2 = Lua.Lua_touserdata(L,3);

            var animationStyle = Lua.Lua_touserdata(L,4);

            return 0;
        }

        #endregion

        #region Help Methods

        static CompositeTransform GetCompositeTransform(FrameworkElement fe)
        {
            CompositeTransform comTF = null;
            if (!(fe.RenderTransform is CompositeTransform))
            {
                comTF = new CompositeTransform();
                fe.RenderTransform = comTF;
            }
            else
            {
                comTF = fe.RenderTransform as CompositeTransform;
            }

            return comTF;
        }

        public static void SetRYTControlRestoreAnimationKeyValue(object rytControl, string key, double value)
        {
            var mi = rytControl.GetType().GetMethod("SetAimationRestoreValue");
            if (mi != null)
            {
                mi.Invoke(rytControl, new object[] { key, value });
            }
        }

        static List<AnimationWrapperClass> CreateDoubleAnimations(FrameworkElement fe, Dictionary<string, object> propDict, AnimationValueType valueType, List<AnimationWrapperClass> fromAnimations = null, Storyboard sb = null)
        {
            List<AnimationWrapperClass> result = null;

            if (valueType == AnimationValueType.From)
            {
                result = new List<AnimationWrapperClass>();
            }

            CompositeTransform comTF = null;
            if (!(fe.RenderTransform is CompositeTransform))
            {
                comTF = new CompositeTransform();
                fe.RenderTransform = comTF;
            }
            else
            {
                comTF = fe.RenderTransform as CompositeTransform;
            }

            foreach (var key in propDict.Keys)
            {
                AnimationWrapperClass wrapper = null;

                if (fromAnimations == null)
                {
                    wrapper = new AnimationWrapperClass();
                    wrapper.DA = new DoubleAnimation();
                }
                else
                {
                    var query = fromAnimations.SingleOrDefault(c => !string.IsNullOrEmpty(c.PropertyName) && c.PropertyName.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                    if (query != null)
                    {
                        wrapper = query;
                    }
                }

                if (wrapper == null)
                {
                    return null;
                }
                wrapper.PropertyName = key;

                if (valueType == AnimationValueType.From)
                {
                    if (key.Equals("matrix"))
                    {
                        wrapper.MatrixInfoList = new List<MatrixInfo>();
                        var dict = propDict[key] as Dictionary<string, double>;
                        foreach (var vKey in dict.Keys)
                        {
                            var info = new MatrixInfo();
                            info.PropertyName = vKey;
                            info.DA = new DoubleAnimation();
                            info.DA.From = dict[vKey];

                            wrapper.MatrixInfoList.Add(info);
                        }
                    }
                    else if(key.Equals("alpha"))
                    {
                        var value = (double)propDict[key];
                        if(value >=0 && value <=1)
                        {
                            wrapper.DA.From = value;
                        }
                    }
                    else
                    {
                        wrapper.DA.From = (double)propDict[key];
                    }
                }
                else if (valueType == AnimationValueType.To)
                {
                    if (key.Equals("matrix"))
                    {
                        var dict = propDict[key] as Dictionary<string, double>;
                        foreach (var vKey in dict.Keys)
                        {
                            var query = wrapper.MatrixInfoList.FirstOrDefault(c => c.PropertyName.Equals(vKey, StringComparison.CurrentCultureIgnoreCase));
                            if (query != null)
                            {
                                query.DA.To = dict[vKey];
                            }
                        }
                    }
                    else if (key.Equals("alpha"))
                    {
                        var value = (double)propDict[key];
                        if (value >= 0 && value <= 1)
                        {
                            wrapper.DA.To = value;
                        }
                    }
                    else
                    {
                        wrapper.DA.To = (double)propDict[key];
                    }
                }

                switch (key)
                {
                    case "x":
                        Storyboard.SetTarget(wrapper.DA, comTF);                      
                        Storyboard.SetTargetProperty(wrapper.DA, new PropertyPath(CompositeTransform.TranslateXProperty));
                        break;
                    case "y":
                        Storyboard.SetTarget(wrapper.DA, comTF);
                        Storyboard.SetTargetProperty(wrapper.DA, new PropertyPath(CompositeTransform.TranslateYProperty));
                        break;
                    case "width":
                        Storyboard.SetTarget(wrapper.DA, fe);
                        Storyboard.SetTargetProperty(wrapper.DA, new PropertyPath(FrameworkElement.WidthProperty));
                        break;
                    case "height":
                        Storyboard.SetTarget(wrapper.DA, fe);
                        Storyboard.SetTargetProperty(wrapper.DA, new PropertyPath(FrameworkElement.HeightProperty));
                        break;
                    case "alpha":
                        Storyboard.SetTarget(wrapper.DA, fe);
                        Storyboard.SetTargetProperty(wrapper.DA, new PropertyPath(FrameworkElement.OpacityProperty));
                        break;
                    default:
                        break;
                }

                if (result != null)
                {
                    result.Add(wrapper);
                }
            }

            if (valueType == AnimationValueType.From)
            {
                if (!FromAnimationsDict.ContainsKey(fe))
                {
                    FromAnimationsDict.Add(fe, result);
                }
                else
                {
                    if (FromAnimationsDict[fe].Count > 0)
                    {
                        result[0].StartFunctionId = FromAnimationsDict[fe][0].StopFunctionId;
                        result[0].StopFunctionId = FromAnimationsDict[fe][0].StopFunctionId;
                        result[0].Sender = FromAnimationsDict[fe][0].Sender;
                    }
                    FromAnimationsDict[fe] = result;
                }
            }

            return result;
        }

        /// <summary>
        ///  m11 ——X轴缩放
        ///  m12 ——Y轴上倾斜
        ///  m21 ——X轴上倾斜
        ///  m22——Y轴缩放
        ///  offsetX ——X轴上的位移
        ///  offsetY ——Y轴上的位移
        /// </summary>
        static void SetMatrixStoryboard(FrameworkElement fe, string propertyName, double duration, MatrixInfo mInfo, Storyboard sb)
        {
            if (propertyName.Equals("m11", StringComparison.CurrentCultureIgnoreCase)) // x scale
            {
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleXProperty, duration, mInfo.DA.From.Value, mInfo.DA.To.Value, sb, mInfo.DA);
            }
            else if (propertyName.Equals("m12", StringComparison.CurrentCultureIgnoreCase)) // y skew
            {
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewYProperty, duration, LuaManager.ConvertDegreesToRadians(Math.Atan(mInfo.DA.From.Value)), LuaManager.ConvertDegreesToRadians(Math.Atan(mInfo.DA.To.Value)), sb, mInfo.DA);
            }
            else if (propertyName.Equals("m21", StringComparison.CurrentCultureIgnoreCase)) // x skew
            {
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.SkewXProperty, duration, LuaManager.ConvertDegreesToRadians(Math.Atan(mInfo.DA.From.Value)), LuaManager.ConvertDegreesToRadians(Math.Atan(mInfo.DA.To.Value)), sb, mInfo.DA);
            }
            else if (propertyName.Equals("m22", StringComparison.CurrentCultureIgnoreCase)) // y scale
            {
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.ScaleYProperty, duration, mInfo.DA.From.Value, mInfo.DA.To.Value, sb, mInfo.DA);
            }
            else if (propertyName.Equals("m31", StringComparison.CurrentCultureIgnoreCase)) // x translation
            {
                mInfo.DA.From *= LuaManager.WidthScale;
                mInfo.DA.To *= LuaManager.WidthScale;
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateXProperty, duration, mInfo.DA.From.Value, mInfo.DA.To.Value, sb, mInfo.DA);
            }
            else if (propertyName.Equals("m32", StringComparison.CurrentCultureIgnoreCase)) //y translation
            {
                mInfo.DA.From *= LuaManager.HeightScale;
                mInfo.DA.To *= LuaManager.HeightScale;
                CreateDoubleAnimationBasedOnComTF(fe, CompositeTransform.TranslateYProperty, duration, mInfo.DA.From.Value, mInfo.DA.To.Value, sb, mInfo.DA);
            }
        }

        internal static DoubleAnimation CreateDoubleAnimationBasedOnComTF(FrameworkElement fe, DependencyProperty dp, double duration, double? from, double toValue, Storyboard sb = null, DoubleAnimation da = null)
        {
            CompositeTransform comTF = null;
            if (fe.RenderTransform == null || !(fe.RenderTransform is CompositeTransform))
            {
                comTF = new CompositeTransform();
                fe.RenderTransform = comTF;
            }
            else
            {
                comTF = fe.RenderTransform as CompositeTransform;
            }

            if (fe.Width > 0 && fe.Height > 0)
            {
                comTF.CenterX = fe.Width / 2;
                comTF.CenterY = fe.Height / 2;
            }
            else if (fe.ActualHeight != 0 && fe.ActualWidth != 0)
            {
                comTF.CenterX = fe.ActualWidth / 2;
                comTF.CenterY = fe.ActualHeight / 2;
            }

            if (da == null)
            {
                da = new DoubleAnimation();
            }

            if (from.HasValue)
            {
                da.From = from.Value;
            }

            da.To = toValue;

            da.Duration = TimeSpan.FromSeconds(duration);
            Storyboard.SetTarget(da, comTF);
            Storyboard.SetTargetProperty(da, new PropertyPath(dp));

            if (sb != null)
            {
                sb.Children.Add(da);
            }

            return da;
        }

        internal static FrameworkElement GetRYTControlView(object obj)
        {
            var pi = obj.GetType().GetProperty("View_");
            var fe = pi.GetValue(obj, null) as FrameworkElement;

            return fe;
        }

        internal static bool CheckIsParentBodyControl(object obj, out double reduceHeight)
        {
            bool result = false;
            double height = 0;
            var fieldInfo = obj.GetType().GetField("Parent_");
            if (fieldInfo != null)
            {
                var parent = fieldInfo.GetValue(obj);
                if (parent.GetType().Name.Equals("RYTBodyControl"))
                {
                    var topSpField = parent.GetType().GetProperty("SpTopPositionFixedPanel_");
                    if (topSpField != null)
                    {
                        var topSp = topSpField.GetValue(parent);
                        height = (topSp as FrameworkElement).Height;
                        result = true;
                    }
                }
            }
            reduceHeight = height;
            return result;
        }

        internal static void SetStoryboardRepeatBehavior(FrameworkElement fe, Storyboard sb)
        {
            if (RepeatInfoDic.ContainsKey(fe))
            {
                sb.RepeatBehavior = RepeatInfoDic[fe];
            }
        }

        internal static void SetCurveTypeToDA(DoubleAnimation da, CurveType value)
        {
            switch (value)
            {
                case CurveType.Linear:
                    break;
                case CurveType.EaseIn:
                    da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseIn };
                    break;
                case CurveType.EaseOut:
                    da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut };
                    break;
                case CurveType.EaseInOut:
                    da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseInOut };
                    break;
                default:
                    break;
            }
        }

        static void SetStoryboardCurve(FrameworkElement fe, DoubleAnimation da, Storyboard sb)
        {
            if (FromAnimationsDict.ContainsKey(fe))
            {
                if (da != null)
                {
                    foreach (var info in FromAnimationsDict[fe])
                    {
                        SetCurveTypeToDA(da, info.CurveType);
                        break;
                    }
                }
                else if (sb != null)
                {
                    foreach (var info in FromAnimationsDict[fe])
                    {
                        foreach (var timeline in sb.Children)
                        {
                            if (timeline is DoubleAnimation)
                            {
                                SetCurveTypeToDA(timeline as DoubleAnimation, info.CurveType);
                            }
                        }
                        break;
                    }
                }
            }
        }

        static void RegisterCallBackFunctions(int L, FrameworkElement fe, Storyboard sb)
        {
            if (FromAnimationsDict.ContainsKey(fe))
            {
                var startQuery = FromAnimationsDict[fe].FirstOrDefault(c => c.StartFunctionId != 0);
                if (startQuery != null)
                {
                    if (startQuery.StartFunctionId != 0)
                    {
                        var callbackF = startQuery.StartFunctionId;
                        startQuery.StartFunctionId = 0;
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbackF, startQuery.Sender); // 此回调lua方法中有可能修改StartFunctionId.
                    }
                }

                if (sb != null)
                {
                    var endQuery = FromAnimationsDict[fe].FirstOrDefault(c => c.StopFunctionId != 0);
                    if (endQuery != null)
                    {
                        sb.Completed += (s, e) =>
                        {
                            if (endQuery.StopFunctionId != 0)
                            {
                                var callbakcF = endQuery.StopFunctionId;
                                endQuery.StopFunctionId = 0;
                                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(callbakcF, endQuery.Sender);// 此回调lua方法中有可能修改StopFunctionId.
                            }
                        };
                    }
                }
            }
        }

        //internal static CurveType ConvertAndSetCurveType(DoubleAnimation da, string value)
        //{
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        return CurveType.Linear;
        //    }
        //    else if (value.Equals("ease-in", StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseIn };
        //        return CurveType.EaseIn;
        //    }
        //    else if (value.Equals("ease-out", StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut };
        //        return CurveType.EaseOut;
        //    }
        //    else if (value.Equals("ease-inout", StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        da.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseInOut };
        //        return CurveType.EaseInOut;
        //    }

        //    return CurveType.Linear;
        //}

        /// <summary>
        /// 根据模版相关需要，添加屏蔽同一控件同种动画的二次执行[比较另类，建议日后删除]。
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        static void TryBeginStoryBoard(FrameworkElement fe, Storyboard sb, TranType type)
        {
            try
            {
                Dictionary<TranType, List<Storyboard>> typeDic = null;
                List<Storyboard> sbList = null;
                AllFeStoryBoard.TryGetValue(fe, out typeDic);
                if (typeDic != null)
                {
                    typeDic.TryGetValue(type, out sbList);
                    if (sbList != null)
                    {
                        var active = sbList.Any(c => c.GetCurrentState() == ClockState.Active);
                        if (active)
                        {
                            return;
                        }
                    }
                    else
                    {
                        sbList = new List<Storyboard>();
                        typeDic.Add(type, sbList);
                    }
                }
                else
                {
                    sbList = new List<Storyboard>();
                    typeDic = new Dictionary<TranType, List<Storyboard>>();
                    typeDic.Add(type, sbList);
                    AllFeStoryBoard.Add(fe, typeDic);
                }
                sbList.Add(sb);

                sb.Begin();
            }
            catch (Exception e)
            {
                LuaCommon.ShowError(e, "", "动画执行失败");
            }
        }

        #endregion
    }

    #region Helper class & enum

    class AnimationWrapperClass
    {
        public object Sender { get; set; }

        public DoubleAnimation DA { get; set; }
        public string PropertyName { get; set; }

        public List<MatrixInfo> MatrixInfoList { get; set; }

        public int StartFunctionId { get; set; }
        public int StopFunctionId { get; set; }

        public CurveType CurveType { get; set; }

        public RepeatBehavior RepeatBehavior { get; set; }
    }

    class MatrixInfo
    {
        public string PropertyName { get; set; }

        public DoubleAnimation DA { get; set; }
    }

    enum AnimationValueType
    {
        From,
        To
    }

    internal enum CurveType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    /// <summary>
    /// 尚无用处
    /// </summary>
    public class MatrixFrameCreator
    {
        /**
        <Storyboard x:Name="Storyboard1" RepeatBehavior="Forever">
            <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="(UIElement.RenderTransform).(MatrixTransform.Matrix)" Storyboard.TargetName="button">
                <DiscreteObjectKeyFrame KeyTime="0" Value="1.5 0 0 1.5 -50 0" />
                <DiscreteObjectKeyFrame KeyTime="0:0:1" Value="1.5 0 0 2.5 -80 0" />
                <DiscreteObjectKeyFrame KeyTime="0:0:2" Value="1.5 1 0 1.5 -80 0" />
            </ObjectAnimationUsingKeyFrames>
        </Storyboard>
        **/

        private readonly Matrix From;
        private readonly Matrix To;
        private readonly TimeSpan Duration;

        private TimeSpan rate = TimeSpan.FromMilliseconds(1000 / 30); // 帧间毫秒数

        public MatrixFrameCreator(Matrix _from, Matrix _to, TimeSpan _duration)
        {
            this.From = _from;
            this.To = _to;
            this.Duration = _duration;
        }

        public void CreateFrames(ObjectKeyFrameCollection _collection)
        {
            if (_collection == null)
                return;

            var count = Duration.TotalMilliseconds / rate.TotalMilliseconds;
            var _pM11 = (To.M11 - From.M11) / count;
            var _pM12 = (To.M12 - From.M12) / count;
            var _pM21 = (To.M21 - From.M21) / count;
            var _pM22 = (To.M22 - From.M22) / count;
            var _pM31 = (To.OffsetX - From.OffsetX) / count;
            var _pM32 = (To.OffsetY - From.OffsetY) / count;

            TimeSpan time = TimeSpan.Zero;
            Matrix lastMatrix = From;
            _collection.Add(new DiscreteObjectKeyFrame { KeyTime = time, Value = From });
            while (time < Duration)
            {
                DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame();
                frame.KeyTime = time;

                Matrix _matrix = new Matrix();
                _matrix.M11 = lastMatrix.M11 + _pM11;
                _matrix.M12 = lastMatrix.M12 + _pM12;
                _matrix.M21 = lastMatrix.M21 + _pM21;
                _matrix.M22 = lastMatrix.M22 + _pM22;
                _matrix.OffsetX = lastMatrix.OffsetX + _pM31;
                _matrix.OffsetY = lastMatrix.OffsetY + _pM32;
                lastMatrix = _matrix;
                _collection.Add(new DiscreteObjectKeyFrame { KeyTime = time, Value = _matrix });

                time = time.Add(rate);
            }
            _collection.Add(new DiscreteObjectKeyFrame { KeyTime = Duration, Value = To });
        }

    }

    #endregion
}

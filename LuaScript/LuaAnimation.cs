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
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using RYTong.FunctionLib;
using RYTLuaCplusLib;
using System.Diagnostics;
using System.Linq;

namespace RYTong.LuaScript
{
    public class LuaAnimation
    {
        private LuaAnimation() { }

        static List<FrameAnimationInfo> registeAnimaInfoList = new List<FrameAnimationInfo>();

        public static void SetCplus_Delegates()
        {
            LuaAnimationDelegates.newFrames = new newFramesDel(newFrames);
            LuaAnimationDelegates.New = new NewDel(New);
            LuaAnimationDelegates.setFrame = new setFrameDel(setFrame);
            LuaAnimationDelegates.setDuration = new setDurationDel(setDuration);
            LuaAnimationDelegates.setCurve = new setCurveDel(setCurve);
            LuaAnimationDelegates.setRepeatCount = new setRepeatCountDel(setRepeatCount);
            LuaAnimationDelegates.add = new addDel(add);
            LuaAnimationDelegates.play = new playDel(play);
            LuaAnimationDelegates.stop = new stopDel(stop);
            LuaAnimationDelegates.setStartListener = new setStartListenerDel(setStartListener);
            LuaAnimationDelegates.setStopListener = new setStopListenerDel(setStopListener);
        }

        /// <summary>
        /// table对象，每一帧帧图片名称，播放时将按照提供的图片顺序播放。
        /// </summary>
        /// <param name="lua"></param>
        /// <returns>返回新创建的动画对象</returns>
        static int newFrames(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Table))
                return 0;

            List<string> imgSourceList = new List<string>();
            Lua.Lua_pushnil(L);
            while (Lua.Lua_next(L, -2) != 0)
            {
                string value = Lua.Lua_tostring(L, -1).ToString();
                Lua.Lua_pop(L, 1);
                imgSourceList.Add(value);
            }

            FrameAnimationInfo info = new FrameAnimationInfo();
            info.ImageList = imgSourceList;
            Lua.Lua_pushlightuserdata(L,info);

            return 1;
        }

        /// <summary>
        /// animation:new(function) function方法，该方法内为需要动画显示的目标设置
        /// </summary>
        /// <param name="lua"></param>
        /// <returns>返回新创建的动画对象</returns>
        static int New(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Function))
                return 0;

            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            //LuaManager.Instance.ExecuteCallBackFunction(callbackF);

            if (callbackF != -1)
            {
                AnimationNewInfo info = new AnimationNewInfo();
                info.FunctionID = callbackF;
                Lua.Lua_pushlightuserdata(L,info);
            }
            else
            {
                Lua.Lua_pushnil(L);
            }
            return 1;
        }

        /// <summary>
        /// 设置动画显示区域
        /// animation:setFrame(object,frame) object：动画对象frame：为table类型，{x, y, width, height}
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        static int setFrame(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Table))
                return 0;

            RYTFrame frame = new RYTFrame();
            Lua.Lua_pushnil(L);
            //int index = 0;
            while (Lua.Lua_next(L, -2) != 0)
            {
                var key = Lua.Lua_tostring(L, -2).ToString();
                var value = Lua.Lua_tonumber(L, -1);
                Lua.Lua_pop(L, 1);

                if (key.Equals("x", StringComparison.CurrentCultureIgnoreCase))
                {
                    frame.Left = value * LuaManager.WidthScale;
                }
                else if (key.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    frame.Top = value * LuaManager.HeightScale;
                }
                else if (key.Equals("width", StringComparison.CurrentCultureIgnoreCase))
                {
                    frame.Width = value * LuaManager.WidthScale;
                }
                else if (key.Equals("height", StringComparison.CurrentCultureIgnoreCase))
                {
                    frame.Height = value * LuaManager.HeightScale;
                }
            }

            object obj = Lua.Lua_touserdata(L,2);
            if (obj is FrameAnimationInfo)
            {
                (obj as FrameAnimationInfo).SetFrame(frame);
            }
            else
            {
            }

            return 0;
        }

        static int setDuration(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Number))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var seconds = Lua.Lua_tonumber(L, 3);

            if (obj is FrameAnimationInfo)
            {
                (obj as FrameAnimationInfo).SetDuration(seconds);
            }
            else if (obj != null && obj is Storyboard)
            {
                var sb = obj as Storyboard;
                //sb.Duration = TimeSpan.FromSeconds(seconds);
                foreach (var timeline in sb.Children)
                {
                    (timeline as DoubleAnimation).Duration = TimeSpan.FromSeconds(seconds);
                }
            }

            //Lua.Lua_pushlightuserdata(L, obj);

            return 0;
        }

        static int setCurve(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.String))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            string curve = Lua.Lua_tostring(L, 3).ToString();
            if (obj != null && obj is Storyboard)
            {
                var sb = obj as Storyboard;
                foreach (var timeline in sb.Children)
                {
                    //LuaTransition.ConvertAndSetCurveType(timeline as DoubleAnimation, curve);
                }
            }

            return 0;
        }

        static int setRepeatCount(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Integer))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            var repeatCount = Lua.Lua_tointeger(L, 3);
            if (obj != null)
            {
                if (obj is Storyboard)
                {
                    var sb = obj as Storyboard;
                    sb.RepeatBehavior = new RepeatBehavior(repeatCount);
                    //if (repeatCount > 1)
                    //{
                    //    sb.RepeatBehavior = new RepeatBehavior(repeatCount);
                    //}
                }
                else if (obj is FrameAnimationInfo)
                {

                    if (repeatCount == -1)
                    {
                        //    (obj as FrameAnimationInfo).Loop = true;
                        (obj as FrameAnimationInfo).SB.RepeatBehavior = RepeatBehavior.Forever;
                    }
                    else if (repeatCount > 0)
                    {
                        (obj as FrameAnimationInfo).SB.RepeatBehavior = new RepeatBehavior(repeatCount);
                    }
                }
            }

            //Lua.Lua_pushlightuserdata(L, obj);

            return 0;
        }

        static int add(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.UserData))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            object view = Lua.Lua_touserdata(L,3);

            if (obj == null || view == null)
            {
                return 0;
            }

            if (obj is FrameAnimationInfo)
            {
                var fe = LuaTransition.GetRYTControlView(view);
                (obj as FrameAnimationInfo).SetTargetControl(fe);
            }
            else if (obj != null && obj is Storyboard && view != null && view is FrameworkElement)
            {
                var sb = obj as Storyboard;
                var fe = view as FrameworkElement;
                foreach (var timeline in sb.Children)
                {
                    Storyboard.SetTarget(timeline, fe.RenderTransform);
                }
            }

            return 0;
        }

        static int play(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            if (obj != null)
            {
                if (obj is Storyboard)
                {
                    var sb = obj as Storyboard;
                    sb.Begin();
                }
                else if (obj is FrameAnimationInfo)
                {
                    var fInfo = obj as FrameAnimationInfo;
                    fInfo.Play(L);
                    registeAnimaInfoList.Add(fInfo);
                }
                else if (obj is AnimationNewInfo)
                {
                    var aInfo = obj as AnimationNewInfo;
                    if (aInfo.StartFunctionId != -1)
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(aInfo.StartFunctionId, obj);
                    }

                    if (aInfo.FunctionID != -1)
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(aInfo.FunctionID, obj);
                    }
                }
            }

            return 0;
        }

        static int stop(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            if (obj != null)
            {
                if (obj is Storyboard)
                {
                    var sb = obj as Storyboard;
                    sb.Stop();
                }
                else if (obj is FrameAnimationInfo)
                {
                    var fInfo = obj as FrameAnimationInfo;
                    fInfo.Stop(L);
                }
            }

            return 0;
        }

        #region Set Listener

        static int setStartListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF != 0)
            {
                if (obj is FrameAnimationInfo)
                {
                    (obj as FrameAnimationInfo).StartFunctionId = callbackF;
                }
                else if (obj is AnimationNewInfo)
                {
                    (obj as AnimationNewInfo).StartFunctionId = callbackF;
                }
                else
                {
                }
            }

            return 0;
        }

        static int setStopListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Function))
                return 0;

            object obj = Lua.Lua_touserdata(L,2);
            int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callbackF != 0)
            {
                if (obj is FrameAnimationInfo)
                {
                    (obj as FrameAnimationInfo).StopFunctionId = callbackF;
                }
                else if (obj is AnimationNewInfo)
                {
                    (obj as AnimationNewInfo).EndFunctionId = callbackF;
                }
                else
                {
                }
            }

            return 0;
        }

        #endregion

        public static void TryToDisposeUselessAnimation()
        {
            registeAnimaInfoList.ForEach(a => a.CheckIfDispose());
            registeAnimaInfoList.Clear();
        }
    }

    class FrameAnimationInfo
    {
        private FrameworkElement fe;
        private double seconds;
        private Image img;

        private List<BitmapImage> imageBrushList;
        public List<string> ImageList { get; set; }
        public object Sender { get; set; }

        private Storyboard _SB = null;
        public Storyboard SB
        {
            get
            {
                if (_SB == null)
                    _SB = new Storyboard();
                return _SB;
            }
        }
        public int StartFunctionId { get; set; }
        public int StopFunctionId { get; set; }
        public RYTFrame Frame { get; set; }
        public bool Loop { get; set; }
        public int RepeatCount { get; set; }

        public event EventHandler AnimationCompleted;

        public void SetDuration(double value)
        {
            seconds = value;
        }

        public void SetFrame(RYTFrame value)
        {
            if (SB != null)
            {
                /* 奇怪的逻辑，被注销。by hu.pengtao
                if (StopFunctionId != 0)
                {
                    LuaManager.Instance.ExecuteCallBackFunctionWithParam(StopFunctionId,this);//
                    StopFunctionId = 0;
                }
                */

                SB.Stop();
                SB.Children.Clear();
            }

            Dispose();

            Frame = value;
            img = new Image() { Height = value.Height, Width = value.Width };
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Stretch = Stretch.Fill;
            img.Margin = new Thickness(value.Left, value.Top, 0, 0);
        }

        private Action removePanelAction;

        public void SetTargetControl(FrameworkElement _fe)
        {
            bool bAddImgToPage = true;

            fe = _fe;

            //if (fe is Panel && img != null)
            //{
            //    (fe as Panel).Children.Add(img);
            //    removePanelAction = () => { (fe as Panel).Children.Remove(img); };
            //}
            //else if (fe is ContentControl)
            //{
            //    var temp = (fe as ContentControl).Content;
            //    (fe as ContentControl).Content = img;
            //    removePanelAction = () => { (fe as ContentControl).Content = temp; };
            //}
            //else if (fe is Border)
            //{
            //    var child = (fe as Border).Child;
            //    if (child != null && child is Panel)
            //    {
            //        (child as Panel).Children.Add(img);
            //        removePanelAction = () => { (child as Panel).Children.Remove(img); };
            //    }
            //    else if (child != null && child is ContentControl)
            //    {
            //        var temp = (child as ContentControl).Content;
            //        (child as ContentControl).Content = img;
            //        removePanelAction = () => { (child as ContentControl).Content = temp; };
            //    }

            //}
            //else if (fe is Image)
            //{
            //    bAddImgToPage = false;

            //    var mi = LuaManager.Instance.DetailV_.GetType().GetMethod("AddImageSourceDict");
            //    if (mi != null)
            //    {
            //        mi.Invoke(LuaManager.Instance.DetailV_, new object[] { (fe as Image), (fe as Image).Source });
            //    }

            //    img = fe as Image;
            //    if (Frame != null)
            //    {
            //        img.Height = Frame.Height;
            //        img.Width = Frame.Width;
            //        img.VerticalAlignment = VerticalAlignment.Top;
            //        img.HorizontalAlignment = HorizontalAlignment.Left;

            //        // 有异议，暂时去掉 by wang.ping 0914
            //        //img.Margin = new Thickness(Frame.Left, Frame.Top, 0, 0);
            //    }

            //    removePanelAction = () => { fe = null; };
            //}

            //if (bAddImgToPage)
            //{
            //    var mi = LuaManager.Instance.DetailV_.GetType().GetMethod("AddAnimationImage");
            //    if (mi != null)
            //    {
            //        mi.Invoke(LuaManager.Instance.DetailV_, new object[] { img });
            //    }
            //}

            if (fe.Parent != null && img != null)
            {
                Grid aniGrid = new Grid { Width = fe.Width, Height = fe.Height };
                aniGrid.Children.Add(img);
                if (fe.Parent is Canvas)
                {
                    Canvas.SetLeft(aniGrid, Canvas.GetLeft(fe));
                    Canvas.SetTop(aniGrid, Canvas.GetTop(fe));
                    (fe.Parent as Canvas).Children.Add(aniGrid);
                    removePanelAction = () => { (fe.Parent as Canvas).Children.Remove(aniGrid); aniGrid.Children.Clear(); img.Source = null; img = null; };
                }
                else if (fe.Parent is Grid)
                {
                    Grid.SetRow(aniGrid, Grid.GetRow(fe));
                    Grid.SetColumn(aniGrid, Grid.GetColumn(fe));
                    aniGrid.Margin = fe.Margin;
                    (fe.Parent as Grid).Children.Add(aniGrid);
                    removePanelAction = () => { (fe.Parent as Grid).Children.Remove(aniGrid); aniGrid.Children.Clear(); img.Source = null; img = null; };
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "Unknwon Error @ Animation Panel.");
                }
            }
        }

        public void Play(int L)
        {
            if (ImageList == null || ImageList.Count == 0)
            {
                return;
            }

            ObjectAnimationUsingKeyFrames da = new ObjectAnimationUsingKeyFrames();
            var cellSpan = seconds / ImageList.Count;
            double flagKeyTime = 0;
            imageBrushList = new List<BitmapImage>();
            foreach (var source in ImageList)
            {
                var bmp = FunctionLib.RYTFile.ReadFileByType(source, "image") as BitmapImage;
                if (bmp != null)
                {
                    imageBrushList.Add(bmp);

                    DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame();
                    keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(flagKeyTime));
                    flagKeyTime += cellSpan;

                    keyFrame.Value = bmp;
                    da.KeyFrames.Add(keyFrame);
                }
            }

            da.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = imageBrushList.LastOrDefault(), KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(flagKeyTime)) });

            if (imageBrushList.Count == 0)
            {
                return;
            }

            Storyboard.SetTarget(da, img);
            Storyboard.SetTargetProperty(da, new PropertyPath(Image.SourceProperty));

            SB.Children.Add(da);

            SB.Completed += (s, e) =>
                {
                    if (AnimationCompleted != null)
                    {
                        AnimationCompleted(this, null);
                    }

                    var page = LuaCommon.FindCurrentPage(img);
                    if (page != LuaManager.GetLuaManager(L).DetailV_)
                    {
                        RYTong.LogLib.RYTLog.Log("animation stoped，auto dispose it ..");
                        Dispose();
                        return;
                    }

                    Dispose();

                    if (StopFunctionId != 0)
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(StopFunctionId, this);
                    }
                };

            if (StartFunctionId != 0)
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(StartFunctionId, this);
            }

            SB.Begin();
        }

        public bool CheckIfDispose()
        {
            if (this.SB != null)
            {
                RYTong.LogLib.RYTLog.Log("animation stoped，auto dispose it ..");
                SB.Stop();
                SB.Children.Clear();
                Dispose();
                return true;
            }

            return false;
        }

        public void Play_old(int L)
        {
            if (ImageList == null || ImageList.Count == 0)
            {
                return;
            }

            int index = 0;

            imageBrushList = new List<BitmapImage>();
            foreach (var source in ImageList)
            {
                var bmp = FunctionLib.RYTFile.ReadFileByType(source, "image") as BitmapImage;
                if (bmp != null)
                {
                    imageBrushList.Add(bmp);
                }
            }

            if (imageBrushList.Count == 0)
            {
                return;
            }

            //SB = new Storyboard();

            ObjectAnimationUsingKeyFrames da = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame();
            keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds / ImageList.Count));
            var bitmap = imageBrushList[index++];
            keyFrame.Value = bitmap;
            da.KeyFrames.Add(keyFrame);

            Storyboard.SetTarget(da, img);
            Storyboard.SetTargetProperty(da, new PropertyPath(Image.SourceProperty));

            SB.Children.Add(da);

            if (ImageList.Count == 1)
            {
                img.Source = bitmap;
            }

            int repeatCountIndex = 1;
            SB.Completed += (s, e) =>
            {
                try
                {
                    img.TransformToVisual(null);
                }
                catch
                {
                    RYTong.LogLib.RYTLog.Log("检测到未stop的animation动画，自动释放ing ..");
                    Dispose();
                    return;
                }
                if (index >= ImageList.Count)
                {
                    if (Loop)
                    {
                        index = 0;
                    }
                    else if (RepeatCount > 0 && RepeatCount > repeatCountIndex)
                    {
                        index = 0;
                        repeatCountIndex++;
                    }
                    else
                    {
                        Dispose();
                        if (AnimationCompleted != null)
                        {
                            AnimationCompleted(this, null);
                        }

                        if (StopFunctionId != 0)
                        {
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(StopFunctionId, this);//
                        }

                        return;
                    }
                }
                ////??? problem
                if (index < imageBrushList.Count)
                    keyFrame.Value = imageBrushList[index++];
                SB.Begin();
            };

            if (StartFunctionId != 0)
            {
                LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(StartFunctionId, this);//
            }

            SB.Begin();
        }

        public void Stop(int L)
        {
            if (SB != null)
            {
                if (SB.GetCurrentState() != ClockState.Stopped)
                {
                    SB.Stop();
                    if (StopFunctionId != 0)
                    {
                        LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithParam(StopFunctionId, this);//
                        //StopFunctionId = 0;
                    }
                }
            }
            Dispose();
        }

        private void Dispose()
        {
            if (img != null && removePanelAction != null)
            {
                try
                {
                    removePanelAction();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("removePanelAction error.");
                }
            }
        }
    }

    class AnimationNewInfo
    {
        public AnimationNewInfo()
        {
            StartFunctionId = -1;
            EndFunctionId = -1;
        }

        public Storyboard Storyboard { get; set; }

        public int StartFunctionId { get; set; }

        public int EndFunctionId { get; set; }

        public int FunctionID { get; set; }
    }
}

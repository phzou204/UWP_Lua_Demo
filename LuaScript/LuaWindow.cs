
using System;
using System.Reflection;
using LuaRTLib;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LuaScript
{
    public class LuaWindow
    {
        private LuaWindow() { }
        public static void SetCplus_Delegates()
        {
            //LuaWindowDelegates.alertForTest = new alertForTestDel(
            //    (str) =>
            //    RYTLog.ShowMessage(str));
            LuaWindowDelegates.alertForTest = new alertForTestDel(
                (L, str) =>
                System.Diagnostics.Debug.WriteLine(str));
            LuaWindowDelegates.alert = new alertDel(alert);
            LuaWindowDelegates.close = new closeDel(close);
            LuaWindowDelegates.open = new openDel(open);
            LuaWindowDelegates.showControl = new showControlDel(showControl);
            LuaWindowDelegates.showContent = new showContentDel(showContent);
            LuaWindowDelegates.hide = new hideDel(hide);
            LuaWindowDelegates.setPhysicalkeyListener = new setPhysicalkeyListenerDel(setPhysicalkeyListener);
            LuaWindowDelegates.setOnPhysKeyListener = new setOnPhysKeyListenerDel(setOnPhysKeyListener);
            LuaWindowDelegates.closeKeyboard = new closeKeyboardDel(closeKeyboard);
            LuaWindowDelegates.supportStatusBarInXML = new supportStatusBarInXMLDel(supportStatusBarInXML);
        }

        static int alert(int L)
        {
            int count = Lua.Lua_gettop(L);
            if (LuaManager.ExceptionHandleAction != null)
            {
                string[] parms = new string[count - 1];
                for (int i = 0; i < parms.Length; i++)
                    parms[i] = LConst.String;
                if (count >= 4)
                    parms[parms.Length - 1] = LConst.Function;
                if (!LuaCommon.CheckAndShowArgsError(L, parms))
                    return 0;
            }
            String messagebody = Lua.Lua_tostring(L, 2).ToString();
            if (count == 2)
            {
                /*Page page = LuaManager.GetLuaManager(L).DetailV_ as Page;
                if(page !=null)
                {
                    page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        new MessageDialog(messagebody).ShowAsync();
                    });
                }*/
                //Thread.CurrentThread.ManagedThreadId

                var res = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {                    
                    var x = new MessageDialog(messagebody).ShowAsync();
                });

                //CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { });
                //    System.ServiceModel.Dispatcher.DispatchOperation(() =>
                //{
                //    var x = new MessageDialog(messagebody).ShowAsync();
                //});

                //Deployment.Current.Dispatcher.BeginInvoke(() =>
                //{
                //MessageBox.Show(messagebody, "提示:", MessageBoxButton.OK);
                //});
            }
            else if (count > 2)
            {
                Action<int> callbackAction = null;
                if (Lua.Lua_isfunction(L, -1))
                {
                    int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                    callbackAction = (result) =>
                        {
                            LuaManager.GetLuaManager(L).ExecuteCallBackFunctionWithAnyParams(callbackF, result);
                        };
                }
                List<string> btnList = new List<string>();
                for (int i = 3; i <= Lua.Lua_gettop(L); i++)
                {
                    btnList.Add(Lua.Lua_tostring(L, i));
                }
                object page = LuaManager.GetLuaManager(L).DetailV_;
                if (page != null)
                {
                    var mInfo = page.GetType().GetMethod("alert");
                    mInfo.Invoke(page, new object[] { callbackAction, messagebody, btnList });
                }
            }
            return 0;
            /*
            if (count == 2 || count == 3)
            {
                MessageBox.Show(messagebody, "提示:", MessageBoxButton.OK);
            }
            else if (count == 4)
            {
                var btnName = Lua.Lua_tostring(L, 3).ToString();
                int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                if (callbackF > 0)
                {
                    MessageBox.Show(messagebody, "提示:", MessageBoxButton.OK);
                    LuaManager.Instance.ExecuteCallBackFunctionWithParam(callbackF, 0);
                    //Lua.luaL_unref(lua, Lua._LUA_REGISTRYINDEX, callbackF);
                }
                else
                {
                    MessageBox.Show(messagebody, "提示:", MessageBoxButton.OK);
                }
            }
            else if (count > 4)
            {
                int callbackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                if (callbackF > 0)
                {
                    var result = MessageBox.Show(messagebody, "提示:", MessageBoxButton.OKCancel);
                    LuaManager.Instance.ExecuteCallBackFunctionWithParam(callbackF, ConvertMessageBoxResult(result));
                    //Lua.luaL_unref(lua, Lua._LUA_REGISTRYINDEX, callbackF);
                }
                else
                {
                    MessageBox.Show(messagebody, "提示:", MessageBoxButton.OKCancel);
                }
            }

            return 0;
             * */
        }

        //static int ConvertMessageBoxResult(MessageBoxResult result)
        //{
        //    switch (result)
        //    {
        //        case MessageBoxResult.OK:
        //        case MessageBoxResult.Yes:
        //            return 0;
        //        case MessageBoxResult.Cancel:
        //        case MessageBoxResult.No:
        //            return 1;
        //        default:
        //            return 2;
        //    }
        //}

        static int close(int L)
        {
            //Windows.Management.Deployment.Current.Dispatcher.BeginInvoke(() =>
            //    {
            //        throw new QuitException();
            //    });
            return 0;
        }

        static int open(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String))
                return 0;
            String str = Lua.Lua_tostring(L, 2).ToString().Trim();
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (str.StartsWith("file://"))
            {
                //string filePath = str.Substring(7, str.Length - 7);
                //if (filePath.IndexOf('.') == -1)
                //{
                //    string error = string.Format("window:open调用错误，{0}文件类型丢失！", filePath);
                //    LuaCommon.ShowError(null, error, RYTLog.Const.LuaPortError);
                //    return 0;
                //}

                //if (FunctionLib.RYTFile.IsFileExist(filePath))
                //{
                //    FunctionLib.RYTTasks.LaunchFileAsync(filePath);
                //}
                //else
                //{
                //    string error = string.Format("window:open调用错误，{0}附件不存在！", filePath);
                //    LuaCommon.ShowError(null, error, RYTLog.Const.LuaPortError);
                //}
            }
            else if (str != null && page != null)
            {
                MethodInfo methods = page.GetType().GetMethod("open");
                Object[] pars = new Object[] { str };
                methods.Invoke(page, pars);
            }
            return 0;
        }

        static int showControl(int L)
        {
            try
            {
                if (!LuaCommon.CheckAndShowArgsError(L, LConst.UserData, LConst.Integer, LConst.NInteger, LConst.NString))
                    return 0;

                Object control = Lua.Lua_touserdata(L,2);
                int tagId = Lua.Lua_tointeger(L, 3);
                int transitionType = -1;
                bool isMode = true;
                int top = Lua.Lua_gettop(L);
                if (top >= 4)
                {
                    transitionType = Lua.Lua_tointeger(L, 4);
                }
                if (top >= 5)
                {
                    string mode = Lua.Lua_tostring(L, 5);
                    if (!string.IsNullOrEmpty(mode) && mode.Equals("false"))
                    {
                        isMode = false;
                    }
                }
                object page = LuaManager.GetLuaManager(L).DetailV_;
                MethodInfo methods = page.GetType().GetMethod("showControl");
                if (methods != null)
                {
                    Object[] pars = new Object[] { control, tagId, transitionType, isMode };
                    methods.Invoke(page, pars);
                }              
            }
            catch
            {
            }

            return 0;
        }

        static int showContent(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Integer, LConst.NInteger))
                return 0;

            string content = Lua.Lua_tostring(L, 2).ToString();
            var tagId = Lua.Lua_tointeger(L, 3);
            int transitionType = -1;
            if (Lua.Lua_gettop(L) == 4)
            {
                transitionType = Lua.Lua_tointeger(L, 4);
            }
            object page = LuaManager.GetLuaManager(L).DetailV_;
            var mi = page.GetType().GetMethod("showContent", new Type[] { typeof(string), typeof(int), typeof(int) });
            if (mi != null)
            {
                Object[] pars = new Object[] { content, tagId, transitionType };
                mi.Invoke(page, pars);
            }
            return 0;
        }

        static int hide(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.Integer, LConst.NInteger))
                return 0;

            int tagId = Lua.Lua_tointeger(L, 2);
            int transitionType = -1;
            if (Lua.Lua_gettop(L) == 3)
            {
                transitionType = Lua.Lua_tointeger(L, 3);
            }
            object page = LuaManager.GetLuaManager(L).DetailV_;
            if (page != null)
            {
                MethodInfo methods = page.GetType().GetMethod("hide");
                Object[] pars = new Object[] { tagId, transitionType };
                methods.Invoke(page, pars);
            }
            return 0;
        }

        static int setPhysicalkeyListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.String, LConst.Function))
                return 0;
            if (L != LuaManager.RootLuaManager.L)//物理键注册lua方法的行为只对根任务（RootLuaManager）有效，子任务中不注册物理键返回函数
                return 0;
            string key = Lua.Lua_tostring(L, 2).ToString();
            if (!string.IsNullOrEmpty(key) && key.Equals("backspace", StringComparison.CurrentCultureIgnoreCase))
            {
                object page = LuaManager.GetLuaManager(L).DetailV_;
                PropertyInfo pi = page.GetType().GetProperty("BackspaceListener");
                int oldListener = (int)pi.GetValue(page);
                if (oldListener != 0)
                {
                    Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, oldListener);
                    pi.SetValue(page, 0, null);
                }                    
                int callBackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
                if (callBackF != 0)
                    pi.SetValue(page, callBackF, null);
            }
            return 0;
        }
        static int setOnPhysKeyListener(int L)
        {
            if (!LuaCommon.CheckAndShowArgsError(L, LConst.NFuction))
                return 0;
            if (L != LuaManager.RootLuaManager.L)//物理键注册lua方法的行为只对根任务（RootLuaManager）有效，子任务中不注册物理键返回函数
                return 0;
            int oldListener = LuaManager.RootLuaManager.OnBackspaceListener;
            if (Lua.Lua_isnil(L, 2))
            {
                if (oldListener == 0)
                    return 0;
                Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, oldListener);
                LuaManager.RootLuaManager.OnBackspaceListener = 0;
                return 0;
            }
            if (oldListener != 0)
            {
                Lua.LuaL_unref(L, Lua._LUA_REGISTRYINDEX, oldListener);
                LuaManager.RootLuaManager.OnBackspaceListener = 0;
            }                
            int callBackF = Lua.LuaL_ref(L, Lua._LUA_REGISTRYINDEX);
            if (callBackF != 0)
                LuaManager.RootLuaManager.OnBackspaceListener = callBackF;
            return 0;
        }

        static int closeKeyboard(int L)
        {
            try
            {
                object page = LuaManager.GetLuaManager(L).DetailV_;
                if (page != null)
                {
                    var method = page.GetType().GetMethod("closeKeyboard");
                    if (method != null)
                    {
                        method.Invoke(page, null);
                    }
                }
            }
            catch
            {
            }
            return 0;
        }

        static int supportStatusBarInXML(int L)
        {
            Lua.Lua_pushboolean(L, 0);
            return 1;
        }
    }
}

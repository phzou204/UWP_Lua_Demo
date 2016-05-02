using LuaRTLib;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LuaScript
{
    public static class LuaCommon
    {
        public static bool CheckAndShowArgsError(int L, params string[] typeArray)
        {
            bool bCheckPassed = true;

            if (!LuaManager.MustVerify)
                return bCheckPassed;

            #region 打印方法调用方法名

            ////从调用堆栈中获取调用当前方法的方法名.
            //StackTrace st = new StackTrace(false);
            //var packName = st.GetFrame(1).GetMethod().DeclaringType.Name;
            //if (packName.StartsWith("Lua"))
            //    packName = packName.Substring(3, packName.Length - 3).ToLower();
            //var method = st.GetFrame(1).GetMethod().Name;
            //// - 控制台调试输出当前执行的库名+方法名.
            //RYTLog.Log(string.Format("{0}:{1}", packName, method), true);

            #endregion

            int top = Lua.Lua_gettop(L);

            #region 参数个数验证

            // 验证参数个数
            var argsCount = typeArray.Length; // 传入需验证的参数个数
            int canBeNullCount = 0;
            for (int argIndex = argsCount - 1; argIndex >= 0; argIndex--)
            {
                if (typeArray[argIndex].Contains("|"))
                    canBeNullCount++;
                else
                    break;
            }
            int minArgsCount = argsCount - canBeNullCount;//支持最小参数个数
            int maxArgsCount = argsCount;//支持最大参数个数
            if ((top - 1) < minArgsCount || (top - 1) > maxArgsCount)
            {
                string countStr = minArgsCount == maxArgsCount ? minArgsCount.ToString() : minArgsCount.ToString() + "-" + maxArgsCount.ToString();
                //string message = string.Format("调用方法参数个数可能不正确，所传类型个数为{0}，支持个数为{1},方法名为{2} : {3}", top - 1, countStr, packName, method);
                //ShowError(null, message, RYTLog.Const.LuaPortError);
            }

            #endregion

            for (int i = 0; i < typeArray.Length; i++)
            {
                bool bCrrect = true;
                int luaIndex = i + 2;

                if (luaIndex > top)
                    break;
                switch (typeArray[i])
                {
                    case (LConst.String):
                        bCrrect = Lua.Lua_isstring(L, luaIndex);
                        break;
                    case (LConst.NString):
                        bCrrect = Lua.Lua_isstring(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.Table):
                        bCrrect = Lua.Lua_istable(L, luaIndex);
                        break;
                    case (LConst.NTable):
                        bCrrect = Lua.Lua_istable(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.Number):
                        bCrrect = Lua.Lua_isnumber(L, luaIndex);
                        break;
                    case (LConst.NNumber):
                        bCrrect = Lua.Lua_isnumber(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.Integer):
                        bCrrect = Lua.Lua_isnumber(L, luaIndex);
                        break;
                    case (LConst.NInteger):
                        bCrrect = Lua.Lua_isnumber(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.UserData):
                        bCrrect = Lua.Lua_isuserdata(L, luaIndex);
                        break;
                    case(LConst.NUserData):
                        bCrrect = Lua.Lua_isuserdata(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.Boolean):
                        bCrrect = Lua.Lua_isbool(L, luaIndex);
                        break;
                    case (LConst.NBoolean):
                        bCrrect = Lua.Lua_isbool(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.Function):
                        bCrrect = Lua.Lua_isfunction(L, luaIndex);
                        break;
                    case (LConst.NFuction):
                        bCrrect = Lua.Lua_isfunction(L, luaIndex) || Lua.Lua_isnil(L, luaIndex);
                        break;
                    case (LConst.LuaData):
                        bCrrect = Lua.Lua_isnumber(L, luaIndex) || Lua.Lua_isbool(L, luaIndex) || Lua.Lua_isstring(L, luaIndex) || Lua.Lua_istable(L, luaIndex);
                        break;
                }

                if (!bCrrect)
                {
                    bCheckPassed = false;
                    var errorType = LuaCommon.GetType(L, luaIndex);
                    //string message = string.Format("调用方法参数{0}不正确，所传类型为{1}，应该为{2},方法名为{3} : {4}", luaIndex - 1, errorType, typeArray[i], packName, method);
                    //ShowError(null, message, RYTLog.Const.LuaPortError);
                }
            }

            return LuaManager.MustCheckPass || bCheckPassed;
        }

        public static string GetType(int L, int index)
        {
            int intType = Lua.Lua_type(L, index);
            switch (intType)
            {
                case (0):
                    return LConst.Nil;
                case (1):
                    return LConst.Boolean;
                case (2):
                    return LConst.UserData;
                case (3):
                    return LConst.Number;
                case (4):
                    return LConst.String;
                case (5):
                    return LConst.Table;
                case (6):
                    return LConst.Function;
                case (7):
                    return LConst.UserData;
                default:
                    return string.Empty;
            }
        }

        public static void ShowError(Exception e, string content, string title)
        {
            if (LuaManager.ExceptionHandleAction != null)
            {
                LuaManager.ExceptionHandleAction(e, content, title);
            }
        }

        public static Page FindCurrentPage(FrameworkElement control)
        {
            var temp = control as FrameworkElement;
            while (temp.Parent != null && !(temp.Parent is Page))
            {
                temp = temp.Parent as FrameworkElement;
            }

            if (temp.Parent != null)
            {
                return temp.Parent as Page;
            }
            return null;
        }

        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T t = default(T);

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    t = child as T;
                    break;
                }
                else
                {
                    var t1 = FindVisualChild<T>(child);
                    if (t1 != null)
                    {
                        t = t1;
                        break;
                    }
                }
            }

            return t;
        }
    }

    public static class LConst
    {
        public const string String = "string";
        public const string NString = "nil | string";

        public const string Number = "number";
        public const string NNumber = "nil | number";

        public const string Table = "table";
        public const string NTable = "nil | table";

        public const string UserData = "userdata";
        public const string NUserData = "nil | userdata";

        public const string Integer = "integer";
        public const string NInteger = "nil | integer";

        public const string Boolean = "boolean";
        public const string NBoolean = "nil | boolean";

        public const string Function = "function";
        public const string NFuction = "nil | function";

        public const string LuaData = "luadata";

        public const string Nil = "nil";
        public const string Unknown = "|";
    }

    public class QuitException : Exception { }

    /// <summary>
    /// Lua全局表索引辅助类，帮助传递Lua各种引用类型。
    /// </summary>
    public class LuaCValue
    {
        public int ValueIndex = -1;

        public LuaCValue(int id)
        {
            ValueIndex = id;
        }

        public LuaCValue()
        {
        }

    }
}

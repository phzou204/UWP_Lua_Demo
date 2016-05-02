#pragma once

#include "pch.h"
#include <collection.h>
#include <algorithm>

extern "C"
{
#include <string.h>
#include <lualib.h>
#include <lauxlib.h>
}

using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation::Collections;

namespace LuaRTLib
{
	public delegate int CFunction();

	public ref class Lua sealed
	{

	public:
		static property int _LUA_REFNIL{ int get(){ return LUA_REFNIL;}} 
		static property int _LUA_REGISTRYINDEX{ int get(){ return LUA_REGISTRYINDEX;}} 
		static property int _LUA_TNUMBER{ int get(){ return LUA_TNUMBER;}} 
		static property int _LUA_TSTRING{ int get(){ return LUA_TSTRING;}} 
		static property int _LUA_TBOOLEAN{ int get(){ return LUA_TBOOLEAN;}} 
		static int openLua(String^ offline_version);
		static void closeLua(int L);
		//static int GlobalLuaState();
		//static void Lua_setmetatable(String^ method);
		static void Lua_setmetatable(int L, int index);
		static void LuaL_newmetatable(int L, String^ str);
		static void LuaL_getmetatable(int L, String^ str);
		static void Lua_getglobal(int L, String^ method);
		static void Lua_getfield(int L, int index, String^ key);
		static void Lua_rawgeti(int L, int index, int functionID);
		static void Lua_rawset(int L, int index);
		static int LuaL_ref(int L, int t);
		static void LuaL_unref(int L, int index,int functionID);
		static int Lua_pcall(int L, int nargs, int nresults, int errfunc);
		static String^ Lua_tostring(int L, int index);
		static bool Lua_toboolean(int L, int index);
		static void Lua_pop(int L, int index);
		static void Lua_newtable(int L);
		static void Lua_pushvalue(int L, int value);
		static void Lua_pushstring(int L, String^ str);
		static void Lua_pushnumber(int L, double number);
		static void Lua_pushboolean(int L, int value);
		static void Lua_pushinteger(int L, int value);
		static void Lua_pushnil(int L);
		static void Lua_pushlightuserdata(int L, Object^ obj);
		static void Lua_pushpermanentuserdata(int L, Object^ obj);
		static void Lua_newuserdata(int L, int size);
		static bool Lua_isfunction(int L, int index);
		static bool Lua_isuserdata(int L, int index);
		static bool Lua_islightuserdata(int L, int index);
		static bool Lua_isstring(int L, int index);
		static bool Lua_istable(int L, int index);
		static bool Lua_isnil(int L, int index);
		static bool Lua_isnumber(int L, int index);
		static bool Lua_isbool(int L, int index);
		static int Lua_next(int L, int index);
		static void Lua_setfield(int L, int index, String^ str);
		static int Lua_type(int L, int index);
		static int LuaL_loadbuffer(int L, String^ str);
		static int LuaL_dostring(int L, String^ str);
		static void Fprintf(String^ erorFormat, String^ str);
		static int Lua_resume(int L, int narg);
		static bool IsLuaCFunction(Object^ obj);
		static void Lua_pushcfunction(Object^ obj);
		static int Lua_gettop(int L);
		static Object^ Lua_touserdata(int L, int index);
		static Object^ Lua_topermanentuserdata(int L, int index);
		//static void ClearUserdata();
		static int Lua_tointeger(int L, int index);
		static double Lua_tonumber(int L, int index);
		static String^ loadLuaString(int L, String ^luaString);
		static String^ callLuaString(int L, String ^luaString);
		static void setlua_getglobal(int L, String ^luaString);
		static String^ doLuaPCall(int L);
		static String^ doLuaCallBack(int L, int callBackF,bool unRef);
		static String^ doLuaCallBackWithParam(int L, int callBackF,Object^ param, bool unRef);
		//static void doHttpPostCallBack(int callBackF,int callbackParam,String^ resultBody,int responseCode,IMap<String^,String^>^ headers);
		static void parserValueByType(int L, Object^ param);
		static String^ doSLTParaser(int L, String^ pageContent);
		static void UpdateLuaState(int newlua);
	private:
		static int setmetatableEx(lua_State *lua_s);
	};
}


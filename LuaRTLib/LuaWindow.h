#pragma once
#include "pch.h"

extern "C"
{
#include <string.h>
#include <lualib.h>
#include <lauxlib.h>
}

namespace LuaRTLib
{
	class LuaWindow
	{
	public:

		static int registerLib(lua_State *lua);

		static int alert(lua_State *lua);
		static int close(lua_State *lua);
		static int open(lua_State *lua);
		static int showControl(lua_State *lua);
		static int showContent(lua_State *lua);
		static int hide(lua_State *lua);
		static int setPhysicalkeyListener(lua_State *lua);
		static int setOnPhysKeyListener(lua_State *lua);
		static int closeKeyboard(lua_State *lua);
		static int supportStatusBarInXML(lua_State *lua);
	};
}

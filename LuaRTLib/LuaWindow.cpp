#include "pch.h"
#include "LuaWindow.h"
#include "LuaWindowDelegates.h"

extern "C"
{
#include <string.h>
#include <lualib.h>
#include <lauxlib.h>
}

namespace LuaRTLib
{
	int LuaWindow::alert(lua_State *lua)
	{
		return LuaWindowDelegates::alert(reinterpret_cast<int>(lua));
	}

	int LuaWindow::close(lua_State *lua)
	{
		return LuaWindowDelegates::close(reinterpret_cast<int>(lua));
	}

	int LuaWindow::open(lua_State *lua)
	{
		return LuaWindowDelegates::open(reinterpret_cast<int>(lua));
	}

	int LuaWindow::showControl(lua_State *lua)
	{
		return LuaWindowDelegates::showControl(reinterpret_cast<int>(lua));
	}

	int LuaWindow::showContent(lua_State *lua)
	{
		return LuaWindowDelegates::showContent(reinterpret_cast<int>(lua));
	}

	int LuaWindow::hide(lua_State *lua)
	{
		return LuaWindowDelegates::hide(reinterpret_cast<int>(lua));
	}

	int LuaWindow::setPhysicalkeyListener(lua_State *lua)
	{
		return LuaWindowDelegates::setPhysicalkeyListener(reinterpret_cast<int>(lua));
	}
	int LuaWindow::setOnPhysKeyListener(lua_State *lua)
	{
		return LuaWindowDelegates::setOnPhysKeyListener(reinterpret_cast<int>(lua));
	}
	int LuaWindow::closeKeyboard(lua_State *lua)
	{
		return LuaWindowDelegates::closeKeyboard(reinterpret_cast<int>(lua));
	}
	int LuaWindow::supportStatusBarInXML(lua_State *lua)
	{
		return LuaWindowDelegates::supportStatusBarInXML(reinterpret_cast<int>(lua));
	}

	const struct luaL_Reg libs[] =
	{
		{"alert",LuaWindow::alert},
		{"close",LuaWindow::close},
		{"open",LuaWindow::open},
		{"showContent",LuaWindow::showContent},
		{"showControl",LuaWindow::showControl},
		{"hide",LuaWindow::hide},
		{"setPhysicalkeyListener",LuaWindow::setPhysicalkeyListener},
		{ "setOnPhysKeyListener", LuaWindow::setOnPhysKeyListener },
		{ "closeKeyboard",LuaWindow::closeKeyboard },
		{"supportStatusBarInXML",LuaWindow::supportStatusBarInXML},
		{NULL,NULL}
	};

	int LuaWindow::registerLib(lua_State *lua)
	{
		luaL_register(lua, "window", libs);
		return 1;
	}
}
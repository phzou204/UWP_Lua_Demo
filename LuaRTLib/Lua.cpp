#include "pch.h"
#include "RC_Helpers.h"
#include <collection.h>
#include <algorithm>

using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation::Collections;

namespace LuaRTLib                               
{
	static Map<int, Object^>^ permanentDataMap = ref new Map<int, Object^>();
	int Lua::openLua(String^ offline_version)
	{
		lua_State *lua = lua_open();
		luaopen_base(lua);
		luaopen_math(lua);
		luaopen_os(lua);
		luaopen_table(lua);
		luaopen_string(lua);
		luaopen_debug(lua);

		//LuaAccelerometer::registerLib(lua);
		//LuaAnimation::registerLib(lua);
		//LuaAudio::registerLib(lua);
		//LuaDataBase::registerLib(lua);
		//LuaDocument::registerLib(lua);
		//LuaElement::registerLib(lua);
		//LuaEmp::registerLib(lua);
		//LuaFile::registerLib(lua);
		//LuaGesture::registerLib(lua);
		//LuaGps::registerLib(lua);
		//LuaHistory::registerLib(lua);
		//LuaHttp::registerLib(lua);
		//LuaJson::registerLib(lua);
		//LuaKV::registerLib(lua);
		//LuaLocation::registerLib(lua);
		//LuaOffline::registerLib(lua, offline_version);
		//LuaScreen::registerLib(lua);
		//LuaSystem::registerLib(lua);
		//LuaTimer::registerLib(lua);
		//LuaTransition::registerLib(lua);
		//luaUtility::registerLib(lua);
		LuaWindow::registerLib(lua);
		//LuaVideo::registerLib(lua);
		//LuaTrack::registerLib(lua);
		//LuaCorp::registerLib(lua);
		//LuaCamera::registerLib(lua);
		//LuaXmpp::registerLib(lua);
		//LuaTLS::registerLib(lua);

		//设置全局函数
		lua_pushcfunction(lua, setmetatableEx);
		lua_setglobal(lua, "setmetatable");
		return reinterpret_cast<int>(lua);
	}
	int Lua::setmetatableEx(lua_State *lua_s)
	{
		//lua_setmetatable(lua,-2);
		lua_setmetatable(lua_s, -2);
		return 0;
	}

	void Lua::closeLua(int L)
	{
		lua_State *luaState= reinterpret_cast<lua_State*>(L);
		if(luaState !=nullptr)
		{
			lua_close(luaState);
		}
	}

	/*void Lua::Lua_setmetatable(String^ method)
	{
	lua_getglobal(lua,RC_Helpers::ConvertStringToChar(method));
	}*/

	void Lua::Lua_setmetatable(int L, int index)
	{
		lua_setmetatable(reinterpret_cast<lua_State*>(L),index);
	}

	void Lua::LuaL_newmetatable(int L, String^ str)
	{
		luaL_newmetatable(reinterpret_cast<lua_State*>(L), RC_Helpers::ConvertStringToChar(str));
	}

	void Lua::LuaL_getmetatable(int L, String^ str)
	{
		luaL_getmetatable(reinterpret_cast<lua_State*>(L), RC_Helpers::ConvertStringToChar(str));
	}

	void Lua::Lua_rawgeti(int L, int index, int functionID)
	{
		lua_rawgeti(reinterpret_cast<lua_State*>(L), index, functionID);
	}

	void Lua::Lua_getfield(int L, int index, String^ key)
	{
		lua_getfield(reinterpret_cast<lua_State*>(L), index, RC_Helpers::ConvertStringToChar(key));
	}

	void Lua::Lua_rawset(int L, int index)
	{
		lua_rawset(reinterpret_cast<lua_State*>(L), index);
	}

	void Lua::LuaL_unref(int L, int index, int functionID)
	{
		luaL_unref(reinterpret_cast<lua_State*>(L), index, functionID);
	}

	int Lua::LuaL_ref(int L, int t)
	{
		return luaL_ref(reinterpret_cast<lua_State*>(L), t);
	}

	int Lua::Lua_pcall(int L, int nargs, int nresults, int errfunc)
	{
		return lua_pcall(reinterpret_cast<lua_State*>(L), nargs, nresults, errfunc);
	}

	String^ Lua::Lua_tostring(int L,int index)
	{
		if (!Lua_isnil(L, index))
		{
			const char *ch = lua_tostring(reinterpret_cast<lua_State*>(L), index);
			return RC_Helpers::ConvertCharToString(ch);
		}
		else
		{
			return "";
		}
	}

	bool Lua::Lua_toboolean(int L, int index)
	{
		int i = lua_toboolean(reinterpret_cast<lua_State*>(L), index);
		if(i == 1)
		{
			return true;
		}
		return false;
	}

	void Lua::Lua_pop(int L, int index)
	{
		lua_pop(reinterpret_cast<lua_State*>(L), index);
	}

	void Lua::Lua_newtable(int L)
	{
		lua_newtable(reinterpret_cast<lua_State*>(L));
	}

	void Lua::Lua_pushvalue(int L, int value)
	{
		lua_pushvalue(reinterpret_cast<lua_State*>(L), value);
	}

	void Lua::Lua_pushstring(int L, String^ str)
	{
		const char *ch = RC_Helpers::ConvertStringToChar(str);
		lua_pushstring(reinterpret_cast<lua_State*>(L), ch);
	}

	void Lua::Lua_pushnumber(int L, double number)
	{
		lua_pushnumber(reinterpret_cast<lua_State*>(L), number);
	}

	void Lua::Lua_pushboolean(int L, int value)
	{
		lua_pushboolean(reinterpret_cast<lua_State*>(L), value);
	}

	void Lua::Lua_pushinteger(int L, int value)
	{
		lua_pushinteger(reinterpret_cast<lua_State*>(L), value);
	}

	void Lua::Lua_pushlightuserdata(int L, Object^ obj)
	{
		void * star = (void *) obj;			
		lua_pushlightuserdata(reinterpret_cast<lua_State*>(L), star);
	}

	// 添加长久保存的方法， 用于 db 等对象。
	void Lua::Lua_pushpermanentuserdata(int L, Object^ obj)
	{
		void * star = (void *)obj;
		int testKey = (int)star;
		permanentDataMap->Insert(testKey, obj);
		lua_pushlightuserdata(reinterpret_cast<lua_State*>(L), star);
	}

	void Lua::Lua_newuserdata(int L, int size)
	{
		lua_newuserdata(reinterpret_cast<lua_State*>(L), size);
	}

	void Lua::Lua_pushnil(int L)
	{
		lua_pushnil(reinterpret_cast<lua_State*>(L));
	}

	bool Lua::Lua_isfunction(int L, int index)
	{
		return lua_isfunction(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_isuserdata(int L, int index)
	{
		return lua_isuserdata(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_islightuserdata(int L, int index)
	{
		return lua_islightuserdata(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_isstring(int L, int index)
	{
		return lua_isstring(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_istable(int L, int index)
	{
		return lua_istable(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_isnil(int L, int index)
	{
		return lua_isnil(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_isnumber(int L, int index)
	{
		return lua_isnumber(reinterpret_cast<lua_State*>(L), index);
	}

	bool Lua::Lua_isbool(int L, int index)
	{
		return lua_isboolean(reinterpret_cast<lua_State*>(L), index);
	}

	int Lua::Lua_next(int L, int index)
	{
		return lua_next(reinterpret_cast<lua_State*>(L), index);
	}

	void Lua::Lua_setfield(int L, int index, String^ str)
	{
		lua_setfield(reinterpret_cast<lua_State*>(L), index, RC_Helpers::ConvertStringToChar(str));
	}

	int Lua::Lua_type(int L, int index)
	{
		return lua_type(reinterpret_cast<lua_State*>(L), index);
	}

	int Lua::LuaL_loadbuffer(int L, String^ str)
	{
		const char * ch = RC_Helpers::ConvertStringToChar(str);
		//String^ newStr= RC_Helpers::ConvertCharToString(ch);
		//LuaWindowDelegates::alertForTest(newStr);
		return luaL_loadbuffer(reinterpret_cast<lua_State*>(L),ch,strlen(ch),"string");
	}

	int Lua::LuaL_dostring(int L, String^ str)
	{
		const char * ch = RC_Helpers::ConvertStringToChar(str);
		return luaL_dostring(reinterpret_cast<lua_State*>(L), ch);
	}

	void Lua::Fprintf(String^ erorFormat, String^ str)
	{
		fprintf(stderr,RC_Helpers::ConvertStringToChar( erorFormat),str);
	}

	int Lua::Lua_resume(int L, int narg)
	{
		return lua_resume(reinterpret_cast<lua_State*>(L), narg);
	}

	void Lua::Lua_getglobal(int L, String ^str)
	{
		char *ch = RC_Helpers::ConvertStringToChar(str);
		lua_getglobal(reinterpret_cast<lua_State*>(L), ch);
	}

	void Lua::setlua_getglobal(int L, String ^str)
	{
		char *ch = RC_Helpers::ConvertStringToChar(str);
		lua_setglobal(reinterpret_cast<lua_State*>(L), ch);
	}

	bool Lua::IsLuaCFunction(Object^ obj)
	{
		lua_CFunction c = (lua_CFunction)obj;
		if(c != nullptr)
		{
			return true;
		}

		return false;
	}

	void Lua::Lua_pushcfunction(Object^ obj)
	{
		/*lua_CFunction c = (lua_CFunction)func;
		if(c != nullptr)
		{
		lua_pushcfunction(lua,c);
		}*/
	}

	int Lua::Lua_gettop(int L)
	{
		return lua_gettop(reinterpret_cast<lua_State*>(L));
	}

	Object^ Lua::Lua_touserdata(int L, int index)
	{
		void * star = lua_touserdata(reinterpret_cast<lua_State*>(L), index);
		//int key = (int)star;		
		//LuaWindowDelegates::alertForTest(key.ToString());
		//LuaWindowDelegates::alertForTest(key.ToString());
		//LuaWindowDelegates::alertForTest(userDataMap->Size.ToString());

		//bool b = userDataMap->HasKey(key);
		//LuaWindowDelegates::alertForTest(b.ToString());

		//Object^ obj = userDataMap->Lookup(key);
		//userDataMap->Remove(key); // 有时会调用多次，不能删除，但是会占用内存。TBD
		Object^ obj = reinterpret_cast<Object^>(star);
		//Object^ obj = (Object^)star;
		return obj;
	}

	Object^ Lua::Lua_topermanentuserdata(int L, int index)
	{
		void * star = lua_touserdata(reinterpret_cast<lua_State*>(L), index);
		int key = (int)star;
		Object^ obj = permanentDataMap->Lookup(key);		
		return obj;
	}

	//void Lua::ClearUserdata()//改造后，已废弃
	//{
	//	//LuaWindowDelegates::alertForTest("c");
	//	userDataMap->Clear();
	//}

	int Lua::Lua_tointeger(int L, int index)
	{
		return lua_tointeger(reinterpret_cast<lua_State*>(L),index);
	}

	double Lua::Lua_tonumber(int L, int index)
	{
		return lua_tonumber(reinterpret_cast<lua_State*>(L), index);
	}

	String^ Lua::loadLuaString(int L, Platform::String ^str)
	{
		String ^error = nullptr;
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		if (luaState == nullptr)
		{
			return error;
		}

		int len = str->Length();
		char *ch = RC_Helpers::ConvertStringToChar(str);

		int result = luaL_dostring(luaState,ch);
		//luaL_loadbuffer(lua,"local a = nil; local b =a..a;",len,"string");
		if(result!=0)
		{
			error = RC_Helpers::ConvertCharToString(lua_tostring(luaState,-1));
			lua_pop(luaState,1);
		}
		lua_resume(luaState,0);

		return error;
	}

	String^ Lua::callLuaString(int L, Platform::String ^str)
	{
		String ^error = nullptr;
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		if(luaState ==nullptr)
		{
			return error;
		}

		int len = str->Length();
		char *ch = RC_Helpers::ConvertStringToChar(str);

		int result = luaL_dostring(luaState,ch);
		//luaL_loadbuffer(lua,"local a = nil; local b =a..a;",len,"string");
		if(result!=0)
		{
			error = RC_Helpers::ConvertCharToString(lua_tostring(luaState,-1));
			lua_pop(luaState,1);
		}
		lua_resume(luaState,0);

		return error;
	}

	String^ Lua::doLuaPCall(int L)
	{
		String ^error = nullptr;
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		int result = lua_pcall(luaState,0,0,0);
		if(result!=0)
		{
			error = RC_Helpers::ConvertCharToString(lua_tostring(luaState,-1));
			lua_pop(luaState,1);
		}

		return error;
	}

	String^ Lua::doLuaCallBack(int L, int callbackF,bool unRef)
	{
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		lua_rawgeti(luaState,LUA_REGISTRYINDEX,callbackF);
		if(unRef)
		{
			lua_unref(luaState,callbackF);
		}

		return doLuaPCall(L);
	}

	String^ Lua::doLuaCallBackWithParam(int L, int callbackF, Object^ param, bool unRef)
	{
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		lua_rawgeti(luaState,LUA_REGISTRYINDEX,callbackF);
		if(unRef)
		{
			lua_unref(luaState,callbackF);
		}

		parserValueByType(L, param);

		String ^error = nullptr;

		int result = lua_pcall(luaState,1,0,0);
		if(result!=0)
		{
			error = RC_Helpers::ConvertCharToString(lua_tostring(luaState,-1));
			lua_pop(luaState,1);
		}

		return error;
	}

	void Lua::parserValueByType(int L, Object^ param)
	{
		lua_State *luaState = reinterpret_cast<lua_State*>(L);
		Type^ pType = param->GetType();

		if(pType->FullName == float::typeid->FullName)
		{
			lua_pushnumber(luaState,(float)param);
		}
		else if(pType->FullName == double::typeid->FullName)
		{
			lua_pushnumber(luaState,(float)param);
		}
		else if(pType->FullName == String::typeid->FullName)
		{
			String^ str = param->ToString();
			lua_pushstring(luaState,RC_Helpers::ConvertStringToChar(str));
		}
		else if(pType->FullName == bool::typeid->FullName)
		{
			if((bool) param == true)
			{
				lua_pushboolean(luaState,1);
			}
			else
			{
				lua_pushboolean(luaState,0);
			}
		}
		else if(pType->FullName == int::typeid->FullName)
		{
			lua_pushinteger(luaState,(int)param);
		}
		else if(pType->FullName == long::typeid->FullName)
		{
			lua_pushnumber(luaState,(long)param);
		}
		else
		{
			lua_pushlightuserdata(luaState,(void *)param);
		}
	}

	String^ Lua::doSLTParaser(int L, String^ pageContent)
	{
		lua_State *luaState = reinterpret_cast<lua_State*>(L);

		lua_getglobal(luaState, "ert_slt_ad_2015");
		if (lua_istable(luaState, -1))
		{
			lua_getfield(luaState, -1, "load_render_string");
			if (lua_isfunction(luaState, -1))
			{
				const char * pageContentChars =  RC_Helpers::ConvertStringToChar(pageContent);
				lua_pushstring(luaState, pageContentChars);
				int oldlua = reinterpret_cast<int>(luaState);
				int result = lua_pcall(luaState, 1, 1, 0);
				luaState = reinterpret_cast<lua_State*>(oldlua);
				if (result == 0)
				{
					const char * parsedContentChars = lua_tostring(luaState, -1);
					String^ parsedContent = RC_Helpers::ConvertCharToString(parsedContentChars);
					//delete[]parsedContentChars;
					return parsedContent;
				}
				else
				{
					const char * errorResult = lua_tostring(luaState, -1);
					lua_pop(luaState, 1);
					delete[]errorResult;
					return "Error";
				}
			}
			else
			{
				return "Error";
			}
		}
		else
		{
			return "Error";
		}
		return pageContent;
	}

	void Lua::UpdateLuaState(int newlua)//
	{
		lua_State *lua = reinterpret_cast<lua_State*>(newlua);
	}

	/*void Lua::doHttpPostCallBack(int callBackF,int callbackParam,String^ resultBody,int responseCode,IMap<String^,String^>^ headers)
	{

	}*/
}
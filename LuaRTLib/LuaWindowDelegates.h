#pragma once

namespace LuaRTLib
{
	public delegate int alertDel(int L);
	public delegate void alertForTestDel(int L, String^ str);
	public delegate int closeDel(int L);
	public delegate int openDel(int L);
	public delegate int showControlDel(int L);
	public delegate int showContentDel(int L);
	public delegate int hideDel(int L);
	public delegate int setPhysicalkeyListenerDel(int L);
	public delegate int setOnPhysKeyListenerDel(int L);
	public delegate int closeKeyboardDel(int L);
	public delegate int supportStatusBarInXMLDel(int L);


	public ref class LuaWindowDelegates sealed
	{

	public:

		property static alertDel^ alert;
		property static alertForTestDel^ alertForTest;

		property static closeDel^ close;

		property static openDel^ open;

		property static showControlDel^ showControl;

		property static showContentDel^ showContent;

		property static hideDel^ hide;

		property static setPhysicalkeyListenerDel^ setPhysicalkeyListener;

		property static setOnPhysKeyListenerDel^ setOnPhysKeyListener;

		property static closeKeyboardDel^ closeKeyboard;

		property static supportStatusBarInXMLDel^ supportStatusBarInXML;
	};
}
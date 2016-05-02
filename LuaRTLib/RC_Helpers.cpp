#include "pch.h"
#include "RC_Helpers.h"
#include "string.h"
#include <stdlib.h>

using namespace Windows::Foundation;

namespace LuaRTLib
{
	char* RC_Helpers::ConvertStringToChar(Platform::String^ str)
	{ 
		/*const wchar_t *W = str->Data();
		int Size = wcslen( W );
		char *CString= new char[Size + 1];
		CString[ Size ] = 0;
		for(int y = 0; y < Size;  y++)
		{
		CString[y] = (char)W[y];
		}
		return CString;*/

		//const wchar_t *data = str->Data();
		//size_t origsize = wcslen(data)+1;
		//char * buffer = (char*) malloc(origsize);
		////setlocale(LC_ALL,"zh-CN");
		//wcharTochar(data,buffer,origsize);
		//return buffer;

		setlocale(LC_CTYPE, "zh-CN");
		const wchar_t *data = str->Data();
		size_t origsize = 2 * wcslen(data)+100;
		//LuaWindowDelegates::alertForTest(origsize.ToString());
		size_t convertedChars = 0;
		char * buffer = (char*) malloc(origsize);
		//setlocale(LC_ALL,"zh-CN");
		wcstombs_s(&convertedChars,buffer,origsize,data,_TRUNCATE);
		return buffer;
	}

	String^ RC_Helpers::ConvertCharToString(const char *ch)
	{
		size_t len = 2 * strlen(ch) + 1;
		size_t converted = 0;
		wchar_t *WStr;
		WStr=(wchar_t*)malloc(len*sizeof(wchar_t));
		setlocale(LC_CTYPE, "zh-CN");
		mbstowcs_s(&converted, WStr, len, ch, _TRUNCATE);
		String^ str = ref new String(WStr);
		return str;

		//return ConvertCharToCxString(ch);

		//size_t len = strlen(ch) + 1;
		//size_t converted = 0;
		//wchar_t *WStr;
		//WStr=(wchar_t*)malloc(len*sizeof(wchar_t));
		////WStr=(wchar_t*)malloc(len*sizeof(wchar_t));
		//charTowchar(ch,WStr,len);
		//return ref new String(WStr);
	}

	wchar_t* RC_Helpers::ConvertCharToWchar_t(const char* stringToConvert)
	{
		size_t origsize =strlen(stringToConvert)+1;
		size_t convertedChars = 0;
		auto wcstring = (wchar_t*)malloc(origsize*sizeof(wchar_t));
		mbstowcs_s(&convertedChars,wcstring,origsize,stringToConvert,_TRUNCATE);
		return wcstring;
	}

	String^ RC_Helpers::ConvertCharToCxString(const char* stringToConvert)
	{
		auto wcharStr = RC_Helpers::ConvertCharToWchar_t(stringToConvert);
		auto cxStr = ref new String(wcharStr);
		free(wcharStr);
		return cxStr;
	}

	void RC_Helpers::charTowchar(const char *chr, wchar_t *wchar, int size)  
	{     
		MultiByteToWideChar(CP_ACP, 0, chr, -1, wchar, sizeof(wchar)); 
		//MultiByteToWideChar(CP_ACP, 0, chr, strlen(chr)+1, wchar, size/sizeof(wchar[0]) );  
	}

	void RC_Helpers::wcharTochar(const wchar_t *wchar, char *chr, int length)  
	{  
		//WideCharToMultiByte(CP_OEMCP, NULL, wchar, -1,  chr, length, NULL, FALSE );  
		WideCharToMultiByte(CP_ACP, 0, wchar, -1, chr, length, NULL, NULL);
	}
}
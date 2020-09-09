#include "pch.h"
#include <shlobj.h>

using namespace std;
int main()
{
    string temp;
    WCHAR path1[MAX_PATH];
    SHGetFolderPathW(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, path1);
    cout << utf8_encode(wstring(path1)) << endl;
    cin >> temp;
}
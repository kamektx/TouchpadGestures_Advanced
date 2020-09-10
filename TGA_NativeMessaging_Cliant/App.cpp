#include "pch.h"
#include "App.h"
#include "MyRegistry.h"
#include "Base32.h"
#include <shlobj.h>

using namespace std;

App app = App();

string App::GenerateKey() {
    size_t const bytes = 4;
    random_device rnd;
    auto randomUI = vector<unsigned int>();
    for (size_t i = 0; i < bytes; i++)
    {
        randomUI.push_back(rnd());
    }
    string str = Base32::encode(randomUI);
    return str;
}
void App::SetID(int id) {
    ID = id;
    PID = GetCurrentProcessId();
    registry.SetValue(registry.TGA_NMC, "NMC" + to_string(ID) + "_Key", Key);
    registry.SetValue(registry.TGA_NMC, "NMC" + to_string(ID) + "_PID", PID);
    registry.SetValue(registry.TGA_NMC, "NMC_Running", registry.NMC_Running + (1 << ID));
}
void App::SetAppData() {
    WCHAR path1[MAX_PATH];
    SHGetFolderPathW(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, path1);
    string LocalAppDataPath = utf8_encode(wstring(path1));
    TGA_AppData = LocalAppDataPath + "\\TouchpadGestures_Advanced";
    filesystem::create_directory(filesystem::path(TGA_AppData));
    NMC_AppData = TGA_AppData + "\\NMC";
    filesystem::create_directory(filesystem::path(NMC_AppData));
    MyAppData = NMC_AppData + "\\" + Key;
    filesystem::create_directory(filesystem::path(MyAppData));
    filesystem::create_directory(filesystem::path(MyAppData + "\\screenshot"));
    filesystem::create_directory(filesystem::path(MyAppData + "\\favicon"));
    filesystem::create_directory(filesystem::path(MyAppData + "\\favicon\\raw"));
    filesystem::create_directory(filesystem::path(MyAppData + "\\favicon\\png"));
}
App::App() {
    Key = GenerateKey();
    SetAppData();
}
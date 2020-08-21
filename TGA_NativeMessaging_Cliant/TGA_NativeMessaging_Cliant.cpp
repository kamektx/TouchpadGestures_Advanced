#include "pch.h"
#include "MyRegistry.h"
#include "App.h"
#include "Events.h"

using namespace std;
int main()
{
    HANDLE TGA_init = CreateEvent(NULL, false, false, L"TouchpadGestures_Advanced_init");
    STARTUPINFO tStartupInfo = { sizeof(tStartupInfo) };
    PROCESS_INFORMATION tProcessInfomation = { 0 };
    WCHAR AppPath[] = L"C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\bin\\Debug\\netcoreapp3.1\\TouchpadGestures_Advanced.exe";
    WCHAR CommandLine[] = L"";

    BOOL bResult = CreateProcessW(
        AppPath,
        CommandLine,
        NULL,
        NULL,
        FALSE,
        0,
        NULL,
        NULL,
        &tStartupInfo,
        &tProcessInfomation
    );
    if (TGA_init != NULL) {
        WaitForSingleObject(TGA_init, INFINITE);
        CloseHandle(TGA_init);
    }
    else {
        throw exception();
    }

    for (unsigned int i = 0; i < registry.NMC_RunningMax; i++)
    {
        unsigned int bit = (unsigned int)pow(2, i);
        if ((bit & registry.NMC_Running) == 0) {
            app.SetID(i);
            break;
        }
    }
    string unko;
    cin >> unko;
    cout << unko << endl;
    CloseHandle(tProcessInfomation.hProcess);
    CloseHandle(tProcessInfomation.hThread);
    return 0;
}
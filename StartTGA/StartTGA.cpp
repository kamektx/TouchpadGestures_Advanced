#include <iostream>
#include <windows.h>

std::string utf8_encode(const std::wstring& wstr)
{
    if (wstr.empty()) return std::string();
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
    std::string strTo(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
    return strTo;
}

std::wstring utf8_decode(const std::string& str)
{
    if (str.empty()) return std::wstring();
    int size_needed = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
    std::wstring wstrTo(size_needed, 0);
    MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], size_needed);
    return wstrTo;
}

using namespace std;

int main(int argc, char* argv[])
{
    if (argc <= 1) return -1;
    STARTUPINFO tStartupInfo;
    PROCESS_INFORMATION tProcessInfomation;
    std::wstring AppPath;
    std::wstring CommandLine;
    WCHAR* CommandLineWCHAR;
    tStartupInfo = { sizeof(tStartupInfo) };
    tProcessInfomation = { 0 };
    AppPath = utf8_decode(string(argv[1]));
    CommandLine = L"";
    CommandLineWCHAR = (WCHAR*)calloc(CommandLine.length() + 1, sizeof(WCHAR));
    wcscpy_s(CommandLineWCHAR, CommandLine.length() + 1, CommandLine.c_str());

    BOOL bResult = CreateProcessW(
        AppPath.c_str(),
        CommandLineWCHAR,
        NULL,
        NULL,
        FALSE,
        CREATE_BREAKAWAY_FROM_JOB,
        NULL,
        NULL,
        &tStartupInfo,
        &tProcessInfomation
    );
    free(CommandLineWCHAR);
    CloseHandle(tProcessInfomation.hProcess);
    CloseHandle(tProcessInfomation.hThread);
    return 0;
}

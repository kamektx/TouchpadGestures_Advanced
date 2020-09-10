#include "pch.h"
#include "MyProcess.h"

using namespace std;

MyProcess::MyProcess(string appPath, string commandLine) {
    tStartupInfo = { sizeof(tStartupInfo) };
    tProcessInfomation = { 0 };
    AppPath = utf8_decode(appPath);
    CommandLine = utf8_decode(commandLine);
    CommandLineWCHAR = (WCHAR*)calloc(CommandLine.length() + 1, sizeof(WCHAR));
    wcscpy_s(CommandLineWCHAR, CommandLine.length() + 1, CommandLine.c_str());
    //char_traits<WCHAR>::copy(CommandLineWCHAR, CommandLine.c_str(), CommandLine.length() + 1);

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
}

MyProcess::~MyProcess() {
    free(CommandLineWCHAR);
    CloseHandle(tProcessInfomation.hProcess);
    CloseHandle(tProcessInfomation.hThread);
}

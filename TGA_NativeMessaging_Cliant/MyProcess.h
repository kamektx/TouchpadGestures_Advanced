#pragma once
#include "pch.h"
class MyProcess
{
public:
    STARTUPINFO tStartupInfo;
    PROCESS_INFORMATION tProcessInfomation;
    std::wstring AppPath;
    std::wstring CommandLine;
    WCHAR* CommandLineWCHAR;
    MyProcess(std::string appPath, std::string commandLine = "");
    ~MyProcess();
};
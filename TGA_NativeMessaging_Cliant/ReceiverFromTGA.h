#pragma once
#include "pch.h"

class ReceiverFromTGA
{
public:
    ReceiverFromTGA();

private:
    std::wstring MakeFileForReceiving();
    FileWatch<std::wstring> FileWatchHandle;
    std::wstring FilePath;

};


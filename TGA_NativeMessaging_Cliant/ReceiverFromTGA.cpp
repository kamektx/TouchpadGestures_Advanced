#include "pch.h"
#include "ReceiverFromTGA.h"

using namespace std;
using namespace filewatch;

ReceiverFromTGA::ReceiverFromTGA()
{
    FilePath = MakeFileForReceiving();
	filewatch::FileWatch<std::wstring> FileWatchHandle(
		FilePath,
		[](const std::wstring& path, const filewatch::Event change_type) {
			std::wcout << path << L"\n";
		}
	);
}

std::wstring ReceiverFromTGA::MakeFileForReceiving()
{
    return std::wstring();
}

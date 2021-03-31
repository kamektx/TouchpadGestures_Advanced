#include "pch.h"
#include "Events.h"

using namespace std;

Events events = Events();

Events::Events()
{
    TGA_Init = MyCreateEvent("TouchpadGestures_Advanced_Init");
    NMC_Changed = MyCreateEvent("TouchpadGestures_Advanced_NMC_Changed");
    NMC_Created = MyCreateEvent("TouchpadGestures_Advanced_NMC_Created");
}

Events::~Events() {
    CloseHandle(TGA_Init);
    CloseHandle(NMC_Changed);
    CloseHandle(NMC_Created);
}

HANDLE Events::MyCreateEvent(string name, bool manualReset)
{
    wstring name_w = utf8_decode(name);
    return CreateEventW(NULL, manualReset, false, name_w.c_str());
}



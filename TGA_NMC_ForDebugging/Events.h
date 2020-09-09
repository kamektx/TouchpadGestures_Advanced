#pragma once
#include "pch.h"
using namespace std;
class Events
{
public:
    Events();
    ~Events();
    HANDLE MyCreateEvent(string name, bool manualReset = false);
    HANDLE TGA_Init;
    HANDLE NMC_Changed;
};
extern Events events;

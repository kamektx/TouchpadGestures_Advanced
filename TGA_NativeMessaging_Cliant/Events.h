#pragma once
#include "pch.h"

class Events
{
public:
    Events();
    ~Events();
    HANDLE MyCreateEvent(std::string name, bool manualReset = false);
    HANDLE TGA_Init;
    HANDLE NMC_Changed;
    HANDLE NMC_Created;
};
extern Events events;

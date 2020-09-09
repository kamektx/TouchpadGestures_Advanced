#pragma once
#include "pch.h"

class App {
public:
    std::string TGA_AppData;
    std::string NMC_AppData;
    std::string MyAppData;
    int ID = -1;
    std::string Key;
    unsigned int PID = 0;
    std::string GenerateKey();
    void SetID(int id);
    void SetAppData();
    App();
};
extern App app;
#pragma once
#include "pch.h"
using namespace std;
class App {
public:
    int ID = -1;
    string Key;
    unsigned int PID = 0;
    string GenerateKey();
    void SetID(int id);
    App();
};
extern App app;
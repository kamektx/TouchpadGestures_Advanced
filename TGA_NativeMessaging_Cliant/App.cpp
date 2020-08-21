#include "pch.h"
#include "App.h"
#include "MyRegistry.h"
#include "Base32.h"
App app = App();

string App::GenerateKey() {
    size_t const bytes = 8;
    random_device rnd;
    auto randomUI = vector<unsigned int>();
    for (size_t i = 0; i < bytes; i++)
    {
        randomUI.push_back(rnd());
    }
    string str = Base32::encode(randomUI);
    return str;
}
void App::SetID(int id) {
    ID = id;
    PID = GetCurrentProcessId();
    registry.SetValue(registry.TGA_NMC, "NMC" + to_string(ID) + "_Key", Key);
    registry.SetValue(registry.TGA_NMC, "NMC" + to_string(ID) + "_PID", PID);
}
App::App() {
    Key = GenerateKey();
}
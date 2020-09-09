#pragma once
#include "pch.h"
class MyRegistry {
public:
    HKEY TGA;
    HKEY TGA_NMC;
    unsigned int NMC_Running;
    unsigned int NMC_RunningMax;
    MyRegistry();
    ~MyRegistry();
    HKEY Open(std::string subKey);
    void GetValue(HKEY hkey, std::string value, unsigned int* data);
    void GetValue(HKEY hkey, std::string value, std::string data);
    void SetValue(HKEY hkey, std::string value, unsigned int data);
    void SetValue(HKEY hkey, std::string value, std::string data);
private:
    class RegistryError
        : public std::runtime_error
    {
    public:
        RegistryError(const char* message, LONG errorCode);
        LONG ErrorCode() const noexcept;
    private:
        LONG m_errorCode;
    };
};
extern MyRegistry registry;
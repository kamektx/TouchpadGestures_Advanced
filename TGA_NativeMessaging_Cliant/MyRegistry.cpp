#include "pch.h"
#include "MyRegistry.h"
MyRegistry registry = MyRegistry();

MyRegistry::MyRegistry() {
    TGA = Open("SOFTWARE\\TouchpadGestures_Advanced");
    TGA_NMC = Open("SOFTWARE\\TouchpadGestures_Advanced\\NativeMessaging_Cliant");
    GetValue(TGA_NMC, "NMC_Running", &NMC_Running);
    GetValue(TGA_NMC, "NMC_RunningMax", &NMC_RunningMax);
}

MyRegistry::~MyRegistry() {
    RegCloseKey(TGA);
    RegCloseKey(TGA_NMC);
}

HKEY MyRegistry::Open(string subKey) {
    HKEY temp;
    auto subKey_w = wstring(subKey.begin(), subKey.end());
    LSTATUS result = RegOpenKeyExW(
        HKEY_CURRENT_USER,
        (LPCWSTR)subKey_w.c_str(),
        0,
        KEY_READ | KEY_WRITE,
        &temp);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("The registry subkey is not found", result);
    }
    return temp;
}

void MyRegistry::GetValue(HKEY hkey, string value, unsigned int* data) {
    LSTATUS result;
    auto value_w = wstring(value.begin(), value.end());
    DWORD size = sizeof(unsigned int);
    result = RegGetValueW(hkey, NULL, value_w.c_str(), RRF_RT_REG_DWORD, 0, data, &size);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("The registry value is not valid.", result);
    }
}

void MyRegistry::GetValue(HKEY hkey, string value, string data) {
    LSTATUS result;
    auto value_w = wstring(value.begin(), value.end());
    DWORD size{};
    result = RegGetValueW(hkey, NULL, value_w.c_str(), RRF_RT_REG_SZ, 0, NULL, &size);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("The registry value is not valid.", result);
    }
    wstring data_w;
    data_w.resize(size / sizeof(wchar_t) - 1);
    result = RegGetValueW(hkey, NULL, value_w.c_str(), RRF_RT_REG_SZ, 0, &data_w[0], &size);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("The registry value is not valid.", result);
    }
    data = utf8_encode(data_w);
}

void MyRegistry::SetValue(HKEY hkey, string value, unsigned int data) {
    LSTATUS result;
    auto value_w = wstring(value.begin(), value.end());
    unsigned int data2 = data;
    DWORD size = sizeof(unsigned int);
    result = RegSetValueExW(hkey, value_w.c_str(), 0, REG_DWORD, (LPBYTE)&data2, size);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("Couldn't set the registry value.", result);
    }
}

void MyRegistry::SetValue(HKEY hkey, string value, string data) {
    LSTATUS result;
    auto value_w = wstring(value.begin(), value.end());
    auto data_w = wstring(data.begin(), data.end());
    DWORD size = data_w.size() * sizeof(wchar_t) + 1;
    result = RegSetValueExW(hkey, value_w.c_str(), 0, REG_SZ, (LPBYTE)data_w.c_str(), size);
    if (result != ERROR_SUCCESS) {
        throw RegistryError("Couldn't set the registry value.", result);
    }
}

MyRegistry::RegistryError::RegistryError(const char* message, LONG errorCode)
    : std::runtime_error{ message }
    , m_errorCode{ errorCode }
{}

LONG MyRegistry::RegistryError::ErrorCode() const noexcept
{
    return m_errorCode;
}
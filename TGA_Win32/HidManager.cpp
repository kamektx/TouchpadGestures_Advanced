#include "pch.h"
DLLEXPORT int __stdcall HidManager(WPARAM wParam, LPARAM lParam, char* r_json, unsigned int length);

using namespace nlohmann;
using namespace std;

DLLEXPORT int __stdcall HidManager(WPARAM wParam, LPARAM lParam, char* rjson, unsigned int length)
{
    json ljson;
    ljson["LinkCollection"] = json::array();
    UINT dwSize;

    GetRawInputData((HRAWINPUT)lParam, RID_INPUT, NULL, &dwSize,
        sizeof(RAWINPUTHEADER));
    LPBYTE lpb = new BYTE[dwSize];
    if (lpb == NULL)
    {
        return 0;
    }

    if (GetRawInputData((HRAWINPUT)lParam, RID_INPUT, lpb, &dwSize, sizeof(RAWINPUTHEADER)) != dwSize)
    {
        OutputDebugString(TEXT("GetRawInputData does not return correct size !\n"));
    }

    RAWINPUT* raw = (RAWINPUT*)lpb;

    if (raw->header.dwType == RIM_TYPEHID)
    {
        GetRawInputDeviceInfo(raw->header.hDevice, RIDI_PREPARSEDDATA, NULL, &dwSize);
        HANDLE preparsedData_handle = GetProcessHeap();
        PHIDP_PREPARSED_DATA preparsedData = (PHIDP_PREPARSED_DATA)HeapAlloc(preparsedData_handle, 0, dwSize);
        GetRawInputDeviceInfo(raw->header.hDevice, RIDI_PREPARSEDDATA, preparsedData, &dwSize);

        HIDP_CAPS caps;
        HidP_GetCaps(preparsedData, &caps);
        USHORT valueCapsLength = caps.NumberInputValueCaps;
        //USHORT buttonCapsLength = caps.NumberInputButtonCaps;
        HANDLE valueCaps_handle = GetProcessHeap();
        PHIDP_VALUE_CAPS valueCaps = (PHIDP_VALUE_CAPS)HeapAlloc(valueCaps_handle, 0, valueCapsLength * sizeof(HIDP_VALUE_CAPS));
        //HANDLE buttonCaps_handle = GetProcessHeap();
        //PHIDP_BUTTON_CAPS buttonCaps = (PHIDP_BUTTON_CAPS)HeapAlloc(buttonCaps_handle, 0, buttonCapsLength * sizeof(HIDP_BUTTON_CAPS));

        HidP_GetValueCaps(HidP_Input, valueCaps, &valueCapsLength, preparsedData);
        //HidP_GetButtonCaps(HidP_Input, buttonCaps, &buttonCapsLength, preparsedData);
        for (int i = 0; i < valueCapsLength; i++) {
            ULONG value;
            USHORT valuelength = valueCaps[i].BitSize * valueCaps[i].ReportCount;
            HidP_GetUsageValue
            (
                HidP_Input,
                valueCaps[i].UsagePage,
                valueCaps[i].LinkCollection,
                valueCaps[i].NotRange.Usage,
                &value,
                preparsedData,
                (PCHAR)raw->data.hid.bRawData,
                raw->data.hid.dwSizeHid
            );

            if (valueCaps[i].LinkCollection == 0) {
                if (valueCaps[i].UsagePage == 0x0d && valueCaps[i].NotRange.Usage == 0x56) {
                    ljson["ScanTime"] = value;
                }
                else if (valueCaps[i].UsagePage == 0x0d && valueCaps[i].NotRange.Usage == 0x54) {
                    ljson["ContactCount"] = value;
                }
            }
            else if (valueCaps[i].LinkCollection >= 1 && valueCaps[i].LinkCollection <= 5) {
                if (valueCaps[i].UsagePage == 0x0d && valueCaps[i].NotRange.Usage == 0x51) {
                    ljson["LinkCollection"][valueCaps[i].LinkCollection - 1]["ContactID"] = value;
                }
                else if (valueCaps[i].UsagePage == 0x01 && valueCaps[i].NotRange.Usage == 0x30) {
                    ljson["LinkCollection"][valueCaps[i].LinkCollection - 1]["X"] = value;
                }
                else if (valueCaps[i].UsagePage == 0x01 && valueCaps[i].NotRange.Usage == 0x31) {
                    ljson["LinkCollection"][valueCaps[i].LinkCollection - 1]["Y"] = value;
                }
            }
        }
        for (USHORT linkCollection = 1; linkCollection <= 5; linkCollection++)
        {
            ULONG usageLength = 9;
            USAGE_AND_PAGE usage_and_page[9];
            HidP_GetUsagesEx(HidP_Input, linkCollection, usage_and_page, &usageLength, preparsedData, (PCHAR)raw->data.hid.bRawData, raw->data.hid.dwSizeHid);
            if (linkCollection <= ljson["ContactCount"].get<unsigned long>()) {
                ljson["LinkCollection"][linkCollection - 1]["Tip"] = "off";
            }
            else {
                ljson["LinkCollection"][linkCollection - 1]["Tip"] = "no";
            }
            for (size_t i = 0; i < usageLength; i++)
            {
                if (usage_and_page[i].UsagePage == 0x0d && usage_and_page[i].Usage == 0x42) {
                    ljson["LinkCollection"][linkCollection - 1]["Tip"] = "on";
                    break;
                }
            }
        }
        strcpy_s(rjson, length, ljson.dump().c_str());
        HeapFree(preparsedData_handle, 0, preparsedData);
        HeapFree(valueCaps_handle, 0, valueCaps);
    }

    delete[] lpb;
    return 0;
}
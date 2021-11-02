#include "pch.h"
DLLEXPORT int __stdcall HidManager(WPARAM wParam, LPARAM lParam, char* r_json, unsigned int length);
DLLEXPORT int __stdcall HidInit(WPARAM wParam, LPARAM lParam, char* r_json, unsigned int length);

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
        OutputDebugString(TEXT("GetRawInputData does not return correct size!\n"));
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
        USHORT inputValueCapsLength = caps.NumberInputValueCaps;
        HANDLE inputValueCaps_handle = GetProcessHeap();
        PHIDP_VALUE_CAPS inputValueCaps = (PHIDP_VALUE_CAPS)HeapAlloc(inputValueCaps_handle, 0, inputValueCapsLength * sizeof(HIDP_VALUE_CAPS));

        HidP_GetValueCaps(HidP_Input, inputValueCaps, &inputValueCapsLength, preparsedData);

        bool doesLinkCollectionAbove1Exist = false;
        int dwCount = raw->data.hid.dwCount;

        for (int dataCount = 0; dataCount < dwCount; dataCount++)
        {
            for (int i = 0; i < inputValueCapsLength; i++) {
                ULONG value;
                USHORT valuelength = inputValueCaps[i].BitSize * inputValueCaps[i].ReportCount;
                NTSTATUS status = HidP_GetUsageValue
                (
                    HidP_Input,
                    inputValueCaps[i].UsagePage,
                    inputValueCaps[i].LinkCollection,
                    inputValueCaps[i].NotRange.Usage,
                    &value,
                    preparsedData,
                    (PCHAR)(raw->data.hid.bRawData + dataCount * raw->data.hid.dwSizeHid),
                    raw->data.hid.dwSizeHid
                );

                //if (status != HIDP_STATUS_SUCCESS) {
                //    OutputDebugStringW(L"HidP_GetUsageValue failed.");
                //}

                if (inputValueCaps[i].LinkCollection == 0) {
                    if (inputValueCaps[i].UsagePage == 0x0d && inputValueCaps[i].NotRange.Usage == 0x56) {
                        ljson["ScanTime"] = value;
                    }
                    else if (inputValueCaps[i].UsagePage == 0x0d && inputValueCaps[i].NotRange.Usage == 0x54) {
                        ljson["ContactCount"] = value;
                    }
                }
                else if (dwCount > 1) {
                    if (inputValueCaps[i].UsagePage == 0x0d && inputValueCaps[i].NotRange.Usage == 0x51) {
                        ljson["LinkCollection"][dataCount]["ContactID"] = value;
                    }
                    else if (inputValueCaps[i].UsagePage == 0x01 && inputValueCaps[i].NotRange.Usage == 0x30) {
                        ljson["LinkCollection"][dataCount]["X"] = value;
                    }
                    else if (inputValueCaps[i].UsagePage == 0x01 && inputValueCaps[i].NotRange.Usage == 0x31) {
                        ljson["LinkCollection"][dataCount]["Y"] = value;
                    }
                }
                else if (inputValueCaps[i].LinkCollection >= 1 && inputValueCaps[i].LinkCollection <= 5) {
                    if (inputValueCaps[i].UsagePage == 0x0d && inputValueCaps[i].NotRange.Usage == 0x51) {
                        ljson["LinkCollection"][inputValueCaps[i].LinkCollection - 1]["ContactID"] = value;
                    }
                    else if (inputValueCaps[i].UsagePage == 0x01 && inputValueCaps[i].NotRange.Usage == 0x30) {
                        ljson["LinkCollection"][inputValueCaps[i].LinkCollection - 1]["X"] = value;
                    }
                    else if (inputValueCaps[i].UsagePage == 0x01 && inputValueCaps[i].NotRange.Usage == 0x31) {
                        ljson["LinkCollection"][inputValueCaps[i].LinkCollection - 1]["Y"] = value;
                    }
                }
                if (inputValueCaps[i].LinkCollection > 1) {
                    doesLinkCollectionAbove1Exist = true;
                }
            }
        }

        int contactCount = ljson["ContactCount"].get<unsigned long>();
        bool isTandCType = false;
        if (contactCount == 0 || (contactCount > 1 && !doesLinkCollectionAbove1Exist)) {
            ljson["isTandCType"] = true;
            isTandCType = true;
        }
        else {
            ljson["isTandCType"] = false;
        }

        if (dwCount > 1) {
            for (int dataCount = 0; dataCount < dwCount; dataCount++)
            {
                ULONG usageLength = 9;
                USAGE_AND_PAGE usage_and_page[9] = { sizeof(usage_and_page) };
                HidP_GetUsagesEx(
                    HidP_Input,
                    1,
                    usage_and_page,
                    &usageLength,
                    preparsedData,
                    (PCHAR)(raw->data.hid.bRawData + dataCount * raw->data.hid.dwSizeHid),
                    raw->data.hid.dwSizeHid
                );
                ljson["LinkCollection"][dataCount]["Tip"] = "off";
                ljson["LinkCollection"][dataCount]["IsFinger"] = false;

                for (size_t i = 0; i < usageLength; i++)
                {
                    if (usage_and_page[i].UsagePage == 0x0d && usage_and_page[i].Usage == 0x42) {
                        ljson["LinkCollection"][dataCount]["Tip"] = "on";

                    }
                    else if (usage_and_page[i].UsagePage == 0x0d && usage_and_page[i].Usage == 0x47) {
                        ljson["LinkCollection"][dataCount]["IsFinger"] = true;
                    }
                }
            }
            for (int dataCount = dwCount; dataCount < 5; dataCount++)
            {
                ljson["LinkCollection"][dataCount]["Tip"] = "no";
                ljson["LinkCollection"][dataCount]["IsFinger"] = false;
            }
        }
        else {
            for (USHORT linkCollection = 1; linkCollection <= 5; linkCollection++)
            {
                ULONG usageLength = 9;
                USAGE_AND_PAGE usage_and_page[9] = { sizeof(usage_and_page) };
                if (linkCollection > ljson["ContactCount"].get<unsigned long>() || (linkCollection != 1 && isTandCType)) {
                    ljson["LinkCollection"][linkCollection - 1]["Tip"] = "no";
                    ljson["LinkCollection"][linkCollection - 1]["IsFinger"] = false;
                    continue;
                }
                ljson["LinkCollection"][linkCollection - 1]["Tip"] = "off";
                ljson["LinkCollection"][linkCollection - 1]["IsFinger"] = false;
                HidP_GetUsagesEx(HidP_Input, linkCollection, usage_and_page, &usageLength, preparsedData, (PCHAR)raw->data.hid.bRawData, raw->data.hid.dwSizeHid);
                for (size_t i = 0; i < usageLength; i++)
                {
                    if (usage_and_page[i].UsagePage == 0x0d && usage_and_page[i].Usage == 0x42) {
                        ljson["LinkCollection"][linkCollection - 1]["Tip"] = "on";

                    }
                    else if (usage_and_page[i].UsagePage == 0x0d && usage_and_page[i].Usage == 0x47) {
                        ljson["LinkCollection"][linkCollection - 1]["IsFinger"] = true;
                    }
                }
            }
        }


        strcpy_s(rjson, length, ljson.dump().c_str());

        //if (raw->data.hid.dwCount != 1) {
        //    wostringstream ss;
        //    ss << L"data.hid.dwCount is " << raw->data.hid.dwCount << endl;
        //    OutputDebugStringW(ss.str().c_str());
        //}

        HeapFree(preparsedData_handle, 0, preparsedData);
        HeapFree(inputValueCaps_handle, 0, inputValueCaps);
    }

    delete[] lpb;

    return 0;
}

DLLEXPORT int __stdcall HidInit(WPARAM wParam, LPARAM lParam, char* rjson, unsigned int length)
{
    json ljson;

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
        HANDLE valueCaps_handle = GetProcessHeap();
        PHIDP_VALUE_CAPS valueCaps = (PHIDP_VALUE_CAPS)HeapAlloc(valueCaps_handle, 0, valueCapsLength * sizeof(HIDP_VALUE_CAPS));
        HidP_GetValueCaps(HidP_Input, valueCaps, &valueCapsLength, preparsedData);
        for (int i = 0; i < valueCapsLength; i++) {
            auto theCaps = valueCaps[i];
            if (theCaps.LinkCollection == 1 && theCaps.UsagePage == 0x01 && theCaps.NotRange.Usage == 0x30) {
                ljson["XLogicalMax"] = theCaps.LogicalMax;
            }
            else if (theCaps.LinkCollection == 1 && theCaps.UsagePage == 0x01 && theCaps.NotRange.Usage == 0x31) {
                ljson["YLogicalMax"] = theCaps.LogicalMax;
            }
            else {
                continue;
            }
        }
        strcpy_s(rjson, length, ljson.dump().c_str());
        HeapFree(preparsedData_handle, 0, preparsedData);
        HeapFree(valueCaps_handle, 0, valueCaps);
    }

    delete[] lpb;
    return 0;
}
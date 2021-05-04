/*
* This program runs only "Release" build
* because of ImageMagick's dll.
*
* Put the all contents of "ImageMagickDll" directory (exclude sub directories)
* to the directry "Release" which contains exe file.
*/

#include "pch.h"
#include "MyRegistry.h"
#include "App.h"
#include "Events.h"
#include "MyFunc.h"
#include "MyProcess.h"

#include "Magick++.h"
#include "curl/curl.h"

using namespace std;
using namespace nlohmann;
using namespace Magick;

size_t callbackCurlWriteFile(void* ptr, size_t size, size_t nmemb, FILE* stream)
{
    size_t written = fwrite(ptr, size, nmemb, stream);
    return written;
}

size_t callbackCurlWriteString(char* ptr, size_t size, size_t nmemb, string* stream)
{
    size_t dataLength = size * nmemb;
    stream->append(ptr, dataLength);
    return dataLength;
}

bool CaptureWindow(HWND hwnd, const std::function<void(const void* data, int width, int height)>& callback)
{
    RECT rect{};
    ::GetWindowRect(hwnd, &rect);
    int width = rect.right - rect.left;
    int height = rect.bottom - rect.top;

    BITMAPINFO info{};
    info.bmiHeader.biSize = sizeof(info.bmiHeader);
    info.bmiHeader.biWidth = width;
    info.bmiHeader.biHeight = height;
    info.bmiHeader.biPlanes = 1;
    info.bmiHeader.biBitCount = 32;
    info.bmiHeader.biCompression = BI_RGB;
    info.bmiHeader.biSizeImage = width * height * 4;

    bool ret = false;
    HDC hscreen = ::GetDC(hwnd);
    HDC hdc = ::CreateCompatibleDC(hscreen);
    void* data = nullptr;
    if (HBITMAP hbmp = ::CreateDIBSection(hdc, &info, DIB_RGB_COLORS, &data, NULL, NULL)) {
        ::SelectObject(hdc, hbmp);
        ::PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT);
        callback(data, width, height);
        ::DeleteObject(hbmp);
        ret = true;
    }
    ::DeleteDC(hdc);
    ::ReleaseDC(hwnd, hscreen);
    return ret;
}

void convertFavicon(string& data, string name, string format) {
    Image favicon;
    if (format == "svg") {
        favicon.size("64x64");
        favicon.backgroundColor("none");
        favicon.read(app.MyAppData + "\\favicon\\raw\\" + name + "." + format);
    }
    else if (format == "ico") {
        int numberOfImages = data.at(4);
        int maxWidth = 0;
        int maxWidthIndex = 0;
        for (int i = 0; i < numberOfImages; i++)
        {
            int width = data.at(6 + 16 * i);
            if (width == 0) width = 256;
            if (width > maxWidth) {
                maxWidth = width;
                maxWidthIndex = i;
            }
        } // Get an image which has the biggest width in the ico file.
        favicon.read(app.MyAppData + "\\favicon\\raw\\" + name + "." + format + "[" + to_string(maxWidthIndex) + "]");
        favicon.backgroundColor("none");
        favicon.resize("64x64");
    }
    else {
        favicon.read(app.MyAppData + "\\favicon\\raw\\" + name + "." + format);
        favicon.backgroundColor("none");
        favicon.resize("64x64");
    }
    favicon.write(app.MyAppData + "\\favicon\\png\\" + name + ".png");
    filesystem::remove(app.MyAppData + "\\favicon\\raw\\" + name + "." + format);
}

int main(int argc, char* argv[])
{
    ofstream logfile(app.MyAppData + "\\log2.txt", ios::app);
    InitializeMagick(argv[0]);
    curl_global_init(CURL_GLOBAL_WIN32 | CURL_GLOBAL_SSL);

    try {
        MyProcess TGA(app.TGA_AppData + "\\bin\\TGA\\TouchpadGestures_Advanced.exe", "");

        if (events.TGA_Init != NULL) {
            WaitForSingleObject(events.TGA_Init, INFINITE);
        }
        else {
            throw exception();
        }

        for (unsigned int i = 0; i < registry.NMC_RunningMax; i++)
        {
            unsigned int bit = (1 << i);
            if ((bit & registry.NMC_Running) == 0) {
                app.SetID(i);
                break;
            }
        }
        {
            ofstream file(app.MyAppData + "\\sending_object.json");
            file << "@" << endl;
            file.close();
        }

        _setmode(_fileno(stdin), _O_BINARY);

        string fileName = app.MyAppData + "\\for_receiving.json";
        {
            ofstream fileForReceiving(fileName);
            fileForReceiving << endl;
            fileForReceiving.close();
        }
        chrono::system_clock::time_point  lastTime = chrono::system_clock::now();
        auto receiveCommandsAndSendCommands = [&]() {
            if (chrono::duration_cast<std::chrono::milliseconds>(chrono::system_clock::now() - lastTime).count() < 60) return;
            this_thread::sleep_for(chrono::milliseconds(5));
            ifstream file(fileName);
            if (!file) throw exception("The File for receiving commands from TGA is broken.");
            mySend(string(std::istreambuf_iterator<char>(file),
                std::istreambuf_iterator<char>()));
            lastTime = chrono::system_clock::now();
        };

        filewatch::FileWatch<wstring> handleForReceiving(utf8_decode(fileName),
            [&](const wstring& path, const filewatch::Event change_type) {
                switch (change_type)
                {
                case filewatch::Event::modified:
                    receiveCommandsAndSendCommands();
                    break;
                default:
                    break;
                };
            });

        SetEvent(events.NMC_Created);

        int count = 0;
        int countTemp = 0;

        auto thread2 = thread([&countTemp] {
            while (true) {
                int maxloop = 3000;
                this_thread::sleep_for(chrono::milliseconds(5000));
                if (countTemp > maxloop) {
                    cerr << "Infinite loop." << endl;
                    throw exception("Infinite loop.");
                }
                countTemp = 0;
            }
            return;
            });

        while (true) {
            countTemp++;
            BYTE buff[4];
            unsigned int length = 0;
            int dataSizeLength = fread(buff, sizeof(char), 4, stdin);
            if (dataSizeLength != 4) {
                cerr << "dataSizeLength length:" << dataSizeLength << endl;
                int err;
                if ((err = ferror(stdin)))
                {
                    fprintf(stderr, "An error occured while reading file, err code: %d\n", err);
                    clearerr(stdin);
                }
                if (feof(stdin))
                {
                    fprintf(stderr, "Unexpectedly encountered EOF while reading file\n");
                }
                break;
            }
            for (int i = 0; i < 4; i++)
            {
                length += buff[i] * (1 << (8 * i));
            }
            char* buff2 = (char*)calloc(length + 1, sizeof(char));
            if (buff2 == NULL) {
                return 0;
            }
            this_thread::sleep_for(chrono::milliseconds(2));
            unsigned int readSize = fread(buff2, sizeof(char), length, stdin);
            buff2[length] = '\0';

            json json0 = json::parse(buff2);
            json json1 = json0.at("Content");
            string jsonType = json1.at("Type").get<string>();



            if (jsonType == "ScreenShot")
            {
                string data = json1.at("Data").get<string>();
                string format;
                string result = base64Decode(data, format);
                ofstream file(app.MyAppData + "\\screenshot\\" + json1.at("FileName").get<string>(), ios::binary);
                file.write(result.c_str(), result.length());
                file.close();
            }
            else if (jsonType == "Favicon")
            {
                string data = json1.at("Data").get<string>();
                string format;
                string name = json1.at("Name").get<string>();
                string result = base64Decode(data, format);
                ofstream file(app.MyAppData + "\\favicon\\raw\\" + name + "." + format, ios::binary);
                file.write(result.c_str(), result.length());
                file.close();

                convertFavicon(result, name, format);

            }
            else if (jsonType == "FaviconUrl") {
                auto curlAsync = async(launch::async, [json1]
                    {
                        //cerr << "FaviconUrl processing..." << endl;
                        string url = json1.at("Url").get<string>();
                        string format;
                        string name = json1.at("Name").get<string>();
                        CURL* curl;
                        CURLcode ret;
                        string chunk;
                        curl = curl_easy_init();
                        //cerr << "Curl init is OK." << endl;
                        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, callbackCurlWriteString);
                        curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
                        //curl_easy_setopt(curl, CURLOPT_VERBOSE, 1L);
                        curl_easy_setopt(curl, CURLOPT_TIMEOUT_MS, 2000);
                        curl_easy_setopt(curl, CURLOPT_URL, url);
                        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &chunk);
                        //cerr << "Curl setopt is OK" << endl;
                        ret = curl_easy_perform(curl);
                        //cerr << "Curl perform is OK." << endl;
                        curl_easy_cleanup(curl);
                        //cerr << "Curl cleanup is OK." << endl;

                        if (ret != CURLE_OK) {
                            cerr << "Curl Error. ErrorCode is " << ret << endl;
                            return -1;
                        }
                        //cerr << "Curl OK" << endl;

                        if (chunk.starts_with("\xFF\xD8")) {
                            format = "jpg";
                        }
                        else if (chunk.starts_with("\x89\x50\x4e\x47"))
                        {
                            format = "png";
                        }
                        else if (chunk.starts_with("\x47\x49\x46\x38"))
                        {
                            format = "gif";
                        }
                        else if (chunk.starts_with("\x00\x00\x01\x00") || chunk.starts_with("\x00\x00\x02\x00"))
                        {
                            format = "ico";
                        }
                        else
                        {
                            format = "svg";
                        }

                        ofstream file(app.MyAppData + "\\favicon\\raw\\" + name + "." + format, ios::binary);
                        file.write(chunk.c_str(), chunk.length());
                        file.close();

                        convertFavicon(chunk, name, format);
                        //cerr << "FaviconUrl Process is over." << endl;
                        return 0;
                    }
                );
            }
            else if (jsonType == "SendingObject") {
                ofstream file(app.MyAppData + "\\sending_object.json", ios::binary);
                file << json1.dump();
                file.close();
            }
            else if (jsonType == "CannotCaptureScreenShot") {
                wstring title = utf8_decode(json1.at("Title").get<string>());
                wstring seachString = title.length() > 30 ? wstring(title, 0, 30) : title;
                string fileName = json1.at("FileName").get<string>();
                auto checkTheWindowText = [seachString](HWND hwnd) -> bool {
                    constexpr int nMaxCount = 260;
                    WCHAR windowTextWC[nMaxCount];
                    GetWindowTextW(hwnd, windowTextWC, nMaxCount);
                    wstring windowText(windowTextWC);
                    if (windowText.find(seachString) != wstring::npos)
                    {
                        return true;
                    }
                    return false;
                };
                HWND true_hwnd = NULL;
                // first, check the foreground window...
                HWND hwnd_now;
                hwnd_now = GetForegroundWindow();
                if (checkTheWindowText(hwnd_now))
                {
                    true_hwnd = hwnd_now;
                }
                else {
                    // Check every window...
                    hwnd_now = GetTopWindow(NULL);
                    while (hwnd_now != NULL) {
                        if (checkTheWindowText(hwnd_now))
                        {
                            true_hwnd = hwnd_now;
                            break;
                        }
                        hwnd_now = GetWindow(hwnd_now, GW_HWNDNEXT);
                    }
                }
                if (true_hwnd != NULL) {
                    CaptureWindow(true_hwnd, [fileName](const void* data, int width, int height) {
                        Image screenshot;
                        screenshot.read(width, height, "BGRA", CharPixel, data);
                        screenshot.resize("1000x1000");
                        screenshot.flip();
                        screenshot.write(app.MyAppData + "\\screenshot\\" + fileName);
                        });
                }
            }
            count++;
            json forSending;
            forSending["Command"] = "Received";
            forSending["ReceivedIndex"] = json0.at("SendingIndex").get<int>();
            mySend(forSending.dump());
            ::free(buff2);
        }
    }
    catch (exception e) {
        cerr << e.what() << endl;
    }
    logfile.close();
    this_thread::sleep_for(chrono::milliseconds(20 * 1000));
    return 0;
}

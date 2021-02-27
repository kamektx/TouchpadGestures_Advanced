﻿/*
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

using namespace std;
using namespace nlohmann;
using namespace Magick;

int main(int argc, char* argv[])
{
    ofstream logfile(app.MyAppData + "\\log2.txt", ios::app);
    InitializeMagick(argv[0]);

    try {
        MyProcess TGA("C:\\Users\\TakumiK\\source\\repos\\TouchpadGestures_Advanced\\TouchpadGestures_Advanced\\bin\\Release\\netcoreapp3.1\\TouchpadGestures_Advanced.exe", "");

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
            file << "未" << endl;
            file.close();
        }

        string str;
        stringstream ss;
        int count = 0;
        int countTemp = 0;
        auto thread2 = thread([&] {
            while (true) {
                int maxloop = 100;
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
            fread(buff2, sizeof(char), length, stdin);
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
                logfile << "format: " << format << endl;
                logfile << flush;
                ofstream file(app.MyAppData + "\\favicon\\raw\\" + name + "." + format, ios::binary);
                file.write(result.c_str(), result.length());
                file.close();
                Image favicon;
                if (format == "svg") {
                    favicon.size("64x64");
                    favicon.backgroundColor("none");
                    favicon.read(app.MyAppData + "\\favicon\\raw\\" + name + "." + format);
                }
                else if (format == "ico") {
                    int numberOfImages = result.at(4);
                    int maxWidth = 0;
                    int maxWidthIndex = 0;
                    for (int i = 0; i < numberOfImages; i++)
                    {
                        int width = result.at(6 + 16 * i);
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
            else if (jsonType == "SendingObject") {
                ofstream file(app.MyAppData + "\\sending_object.json", ios::binary);
                file << json1.dump();
                file.close();
            }
            count++;
            json forSending;
            forSending["ReceivedIndex"] = json0.at("SendingIndex").get<int>();
            mySend(forSending.dump());
        }
    }
    catch (exception e) {
        cerr << e.what() << endl;
        throw e;
    }
    return 0;
}

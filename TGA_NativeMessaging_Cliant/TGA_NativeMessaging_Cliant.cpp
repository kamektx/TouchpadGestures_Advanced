#include "pch.h"
#include "MyRegistry.h"
#include "App.h"
#include "Events.h"
#include "MyFunc.h"
#include "MyProcess.h"


using namespace std;
using namespace nlohmann;

int main()
{
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

    string str;
    stringstream ss;
    int count = 0;
    int countTemp = 0;
    //auto thread1 = thread([&] {
    //    while (true) {
    //        this_thread::sleep_for(chrono::milliseconds(5000));
    //        ofstream file("log.txt");
    //        file << count << "\n";
    //        file << ss.str();
    //        file.close();
    //    }
    //    return;
    //    }
    //);
    auto thread2 = thread([&] {
        while (true) {
            int maxloop = 100;
            this_thread::sleep_for(chrono::milliseconds(5000));
            if (countTemp > maxloop) {
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
        int result = fread(buff, sizeof(char), 4, stdin);
        if (result != 4) {
            cin.clear();
            cin.ignore(std::numeric_limits<std::streamsize>::max());
            continue;
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
        json json1 = json::parse(buff2);
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
            if (format == "svg") {
                MyProcess ImageMagick(app.TGA_AppData + "\\GraphicsMagick\\gm.exe", "convert -size 64x64 \"" + app.MyAppData + "\\favicon\\raw\\" + name + "." + format + "\" \"" + app.MyAppData + "\\favicon\\png\\" + name + ".png\"");
            }
            else {
                MyProcess ImageMagick(app.TGA_AppData + "\\GraphicsMagick\\gm.exe", "convert -resize 64x64 \"" + app.MyAppData + "\\favicon\\raw\\" + name + "." + format + "\" \"" + app.MyAppData + "\\favicon\\png\\" + name + ".png\"");
            }
        }
        else if (jsonType == "SendingObject") {
            ofstream file(app.MyAppData + "\\sending_object.json", ios::binary);
            file.write(buff2, length);
            file.close();
        }
        count++;
        mySend(u8"pong");
    }
    return 0;
}

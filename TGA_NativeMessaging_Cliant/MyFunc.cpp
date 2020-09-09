#include "pch.h"
#include "MyFunc.h"

using namespace std;
void mySend(string utf8string)
{
    string outString = "\"" + utf8string + "\"";
    int dataLength = outString.length();
    BYTE bytes[4];
    for (int i = 0; i < 4; i++)
    {
        bytes[i] = (dataLength >> (8 * i)) & 0xff;
    }
    fwrite(bytes, sizeof(BYTE), 4, stdout);
    fwrite(outString.c_str(), sizeof(char), dataLength, stdout);
    cout << flush;
}
std::string base64Decode(const std::string& in) {
    std::string out;
    std::vector<int> T(256, -1);
    for (int i = 0; i < 64; i++)
    {
        T["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[i]] = i;
    }

    unsigned int val = 0;
    int valb = -8;
    int counter = 0;
    for (unsigned char c : in) {
        if (counter < 23) {
            counter++;
            continue;
        }
        if (T[c] == -1) break;
        val = (val << 6) + T[c];
        valb += 6;
        if (valb >= 0) {
            out.push_back(unsigned char((val >> valb) & 0xFF));
            valb -= 8;
        }
    }
    return out;
}
#include "pch.h"
#include "Base32.h"

using namespace std;
std::string Base32::encode(std::vector<unsigned int> const& vec){
    const char EncodingTable[] = {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
        'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
        'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
        'Y', 'Z', '2', '3', '4', '5', '6', '7'
    };
    stringstream ss;
    for (const auto& value : vec)
    {
        ss << std::bitset<32>(value);
    }
    string str = ss.str();
    string result;
    size_t index = 0;
    while (index < str.size()) {
        int a = stoi(string(str, index, 5), nullptr, 2);
        index += 5;
        a <<= (str.size() < index) ? index - str.size() : 0;
        if (a < 32) {
            result.push_back(EncodingTable[a]);
        }
        else {
            throw exception("Base32 encode error.");
        }

    }
    return result;
}
#ifndef PCH_H
#define PCH_H

#define _USE_MATH_DEFINES
#define NOMINMAX

#include <windows.h>
#include <strsafe.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>

#include <iostream>
#include <fstream>
#include <iomanip>
#include <iterator>
#include <vector>
//#include <algorithm>
//#include <numeric>
#include <cmath>
#include <cfloat>
//#include <queue>
//#include <stack>
#include <string>
#include <map>
#include <functional>
#include <stdexcept>
#include <future>
#include <random>
#include <bitset>
#include <sstream>
#include <filesystem>

#include "json.hpp"

template <typename T>
std::ostream& operator<< (std::ostream& out, const std::vector<std::vector<T>>& v) {
    if (!v.empty()) {
        for (std::vector<T> v2 : v) {
            std::copy(v2.begin(), v2.end(), std::ostream_iterator<T>(out, ", "));
            out << "\b\b \n";
        }
    }
    return out;
}
template <typename T>
std::ostream& operator<< (std::ostream& out, const std::vector<T>& v) {
    if (!v.empty()) {
        std::copy(v.begin(), v.end(), std::ostream_iterator<T>(out, ", "));
        out << "\b\b ";
    }
    return out;
}

std::string utf8_encode(const std::wstring& wstr);
std::wstring utf8_decode(const std::string& str);
#endif //PCH_H
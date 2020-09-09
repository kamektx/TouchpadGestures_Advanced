#pragma once
#include "pch.h"
class Base32 {
public:
    static std::string encode(std::vector<unsigned int> const& vec);
};
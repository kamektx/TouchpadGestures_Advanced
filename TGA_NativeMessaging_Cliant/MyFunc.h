#pragma once
#include "pch.h"

void mySend(std::string utf8string, std::mutex& mutex);

std::string base64Decode(const std::string& in, std::string& formatOut);
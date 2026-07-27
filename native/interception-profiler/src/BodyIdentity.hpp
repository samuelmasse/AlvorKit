#pragma once

#include <array>
#include <cstddef>
#include <cstdint>

std::array<uint8_t, 32> ComputeBodyIdentity(const uint8_t* bytes, size_t size);

#include "BodyIdentity.hpp"

#include <array>
#include <cstddef>
#include <cstdint>
#include <cstring>

namespace {
constexpr std::array<uint32_t, 64> round_constants{
    0x428a2f98u, 0x71374491u, 0xb5c0fbcfu, 0xe9b5dba5u, 0x3956c25bu, 0x59f111f1u, 0x923f82a4u, 0xab1c5ed5u,
    0xd807aa98u, 0x12835b01u, 0x243185beu, 0x550c7dc3u, 0x72be5d74u, 0x80deb1feu, 0x9bdc06a7u, 0xc19bf174u,
    0xe49b69c1u, 0xefbe4786u, 0x0fc19dc6u, 0x240ca1ccu, 0x2de92c6fu, 0x4a7484aau, 0x5cb0a9dcu, 0x76f988dau,
    0x983e5152u, 0xa831c66du, 0xb00327c8u, 0xbf597fc7u, 0xc6e00bf3u, 0xd5a79147u, 0x06ca6351u, 0x14292967u,
    0x27b70a85u, 0x2e1b2138u, 0x4d2c6dfcu, 0x53380d13u, 0x650a7354u, 0x766a0abbu, 0x81c2c92eu, 0x92722c85u,
    0xa2bfe8a1u, 0xa81a664bu, 0xc24b8b70u, 0xc76c51a3u, 0xd192e819u, 0xd6990624u, 0xf40e3585u, 0x106aa070u,
    0x19a4c116u, 0x1e376c08u, 0x2748774cu, 0x34b0bcb5u, 0x391c0cb3u, 0x4ed8aa4au, 0x5b9cca4fu, 0x682e6ff3u,
    0x748f82eeu, 0x78a5636fu, 0x84c87814u, 0x8cc70208u, 0x90befffau, 0xa4506cebu, 0xbef9a3f7u, 0xc67178f2u};

uint32_t RotateRight(uint32_t value, uint32_t count) {
  return (value >> count) | (value << (32u - count));
}

uint32_t ReadBigEndian(const uint8_t* bytes) {
  return (static_cast<uint32_t>(bytes[0]) << 24u) | (static_cast<uint32_t>(bytes[1]) << 16u) |
         (static_cast<uint32_t>(bytes[2]) << 8u) | static_cast<uint32_t>(bytes[3]);
}

void WriteBigEndian(uint32_t value, uint8_t* bytes) {
  bytes[0] = static_cast<uint8_t>(value >> 24u);
  bytes[1] = static_cast<uint8_t>(value >> 16u);
  bytes[2] = static_cast<uint8_t>(value >> 8u);
  bytes[3] = static_cast<uint8_t>(value);
}

void Transform(const uint8_t* block, std::array<uint32_t, 8>* state) {
  std::array<uint32_t, 64> words{};
  for (size_t index = 0; index < 16; ++index)
    words[index] = ReadBigEndian(block + index * 4u);
  for (size_t index = 16; index < words.size(); ++index) {
    const uint32_t left =
        RotateRight(words[index - 15], 7u) ^ RotateRight(words[index - 15], 18u) ^ (words[index - 15] >> 3u);
    const uint32_t right =
        RotateRight(words[index - 2], 17u) ^ RotateRight(words[index - 2], 19u) ^ (words[index - 2] >> 10u);
    words[index] = words[index - 16] + left + words[index - 7] + right;
  }

  uint32_t a = (*state)[0];
  uint32_t b = (*state)[1];
  uint32_t c = (*state)[2];
  uint32_t d = (*state)[3];
  uint32_t e = (*state)[4];
  uint32_t f = (*state)[5];
  uint32_t g = (*state)[6];
  uint32_t h = (*state)[7];
  for (size_t index = 0; index < words.size(); ++index) {
    const uint32_t choice = (e & f) ^ (~e & g);
    const uint32_t majority = (a & b) ^ (a & c) ^ (b & c);
    const uint32_t sum0 = RotateRight(a, 2u) ^ RotateRight(a, 13u) ^ RotateRight(a, 22u);
    const uint32_t sum1 = RotateRight(e, 6u) ^ RotateRight(e, 11u) ^ RotateRight(e, 25u);
    const uint32_t first = h + sum1 + choice + round_constants[index] + words[index];
    const uint32_t second = sum0 + majority;
    h = g;
    g = f;
    f = e;
    e = d + first;
    d = c;
    c = b;
    b = a;
    a = first + second;
  }

  (*state)[0] += a;
  (*state)[1] += b;
  (*state)[2] += c;
  (*state)[3] += d;
  (*state)[4] += e;
  (*state)[5] += f;
  (*state)[6] += g;
  (*state)[7] += h;
}
} // namespace

std::array<uint8_t, 32> ComputeBodyIdentity(const uint8_t* bytes, size_t size) {
  std::array<uint32_t, 8> state{0x6a09e667u, 0xbb67ae85u, 0x3c6ef372u, 0xa54ff53au,
                                0x510e527fu, 0x9b05688cu, 0x1f83d9abu, 0x5be0cd19u};
  const size_t complete_size = size - size % 64u;
  for (size_t offset = 0; offset < complete_size; offset += 64u)
    Transform(bytes + offset, &state);

  std::array<uint8_t, 128> tail{};
  const size_t remaining = size - complete_size;
  if (remaining != 0)
    std::memcpy(tail.data(), bytes + complete_size, remaining);
  tail[remaining] = 0x80u;
  const size_t padded_size = remaining < 56u ? 64u : 128u;
  const uint64_t bit_count = static_cast<uint64_t>(size) * 8u;
  for (size_t index = 0; index < 8; ++index) {
    tail[padded_size - 1u - index] = static_cast<uint8_t>(bit_count >> (index * 8u));
  }
  for (size_t offset = 0; offset < padded_size; offset += 64u)
    Transform(tail.data() + offset, &state);

  std::array<uint8_t, 32> result{};
  for (size_t index = 0; index < state.size(); ++index)
    WriteBigEndian(state[index], result.data() + index * 4u);
  return result;
}

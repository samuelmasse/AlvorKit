#ifndef _WIN32

#include <cstring>

#include <minipal/guid.h>

extern "C" bool minipal_guid_equals(const GUID* left, const GUID* right) {
  return std::memcmp(left, right, sizeof(GUID)) == 0;
}

#endif

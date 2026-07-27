#include "InterceptionIdentity.hpp"

#include <cstring>

namespace {
bool EqualGuid(const alvorkit_guid_v2& left, const alvorkit_guid_v2& right) {
  return std::memcmp(&left, &right, sizeof(left)) == 0;
}
} // namespace

GUID ToGuid(const alvorkit_guid_v2& value) {
  GUID result{};
  result.Data1 = value.data1;
  result.Data2 = value.data2;
  result.Data3 = value.data3;
  std::memcpy(result.Data4, value.data4, sizeof(result.Data4));
  return result;
}

bool EqualTarget(const alvorkit_interception_target_v2& left, const alvorkit_interception_target_v2& right) {
  return EqualGuid(left.module_mvid, right.module_mvid) && left.method_token == right.method_token &&
         left.signature_hash == right.signature_hash;
}

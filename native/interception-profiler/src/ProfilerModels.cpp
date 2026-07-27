#include "ProfilerModels.hpp"

#include <cstddef>

size_t RuntimeMethodKeyHash::operator()(const RuntimeMethodKey& key) const noexcept {
  const auto module = static_cast<size_t>(key.module_id);
  const auto method = static_cast<size_t>(key.method_id);
  const auto epoch = static_cast<size_t>(key.module_epoch);
  const auto combined = module ^ (method + 0x9e3779b9u + (module << 6u) + (module >> 2u));
  return combined ^ (epoch + 0x9e3779b9u + (combined << 6u) + (combined >> 2u));
}

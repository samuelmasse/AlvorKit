#include "ModuleCatalog.hpp"

#include <cstdint>

ModuleCatalog::ModuleCatalog(ICorProfilerInfo10* info) : info_(info), metadata_tokens_(info) {}

void ModuleCatalog::ReadAllowlist() {
  allowlist_.ReadEnvironment();
}

bool ModuleCatalog::IsAllowed(ModuleID module_id) const {
  return allowlist_.Allows(info_, module_id);
}

HRESULT ModuleCatalog::Register(ModuleID module_id) {
  std::lock_guard metadata_lock(metadata_gate_);
  std::lock_guard lock(mutex_);
  if (module_epochs_.contains(module_id))
    return S_FALSE;
  ++next_module_epoch_;
  if (next_module_epoch_ == 0)
    ++next_module_epoch_;
  module_epochs_[module_id] = next_module_epoch_;
  return S_OK;
}

HRESULT ModuleCatalog::Catalog(ModuleID module_id) {
  std::lock_guard metadata_lock(metadata_gate_);
  return CatalogModule(module_id);
}

void ModuleCatalog::Unload(ModuleID module_id) {
  std::lock_guard metadata_lock(metadata_gate_);
  std::lock_guard lock(mutex_);
  module_epochs_.erase(module_id);
  for (auto iterator = methods_.begin(); iterator != methods_.end();) {
    if (iterator->first.module_id == module_id)
      iterator = methods_.erase(iterator);
    else
      ++iterator;
  }
  metadata_tokens_.RemoveModule(module_id);
}

bool ModuleCatalog::IsCurrent(const RuntimeMethodKey& method) const {
  std::lock_guard lock(mutex_);
  const auto epoch = module_epochs_.find(method.module_id);
  return epoch != module_epochs_.end() && epoch->second == method.module_epoch;
}

bool ModuleCatalog::TryGetMethodMetadata(const RuntimeMethodKey& method, MethodMetadata* metadata) const {
  std::lock_guard lock(mutex_);
  const auto value = methods_.find(method);
  if (value == methods_.end())
    return false;
  *metadata = value->second;
  return true;
}

HRESULT ModuleCatalog::ResolveRelocation(const RuntimeMethodKey& method,
                                         const alvorkit_interception_relocation_v3& relocation,
                                         const std::vector<uint8_t>& metadata, mdToken* token) {
  std::lock_guard metadata_lock(metadata_gate_);
  {
    std::lock_guard lock(mutex_);
    const auto epoch = module_epochs_.find(method.module_id);
    if (epoch == module_epochs_.end() || epoch->second != method.module_epoch) {
      return CORPROF_E_DATAINCOMPLETE;
    }
  }
  return metadata_tokens_.Resolve(method, relocation, metadata, token);
}

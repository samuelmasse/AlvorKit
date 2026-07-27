#pragma once

#include <cstdint>

#include "ProfilerModels.hpp"

class MetadataTokenCache {
public:
  explicit MetadataTokenCache(ICorProfilerInfo10* info) : info_(info) {}

  // ModuleCatalog serializes resolution and unload with its metadata gate.
  HRESULT Resolve(const RuntimeMethodKey& method, const alvorkit_interception_relocation_v3& relocation,
                  const std::vector<uint8_t>& metadata, mdToken* token);

  void RemoveModule(ModuleID module_id);

private:
  ICorProfilerInfo10* info_;
  std::vector<MetadataTokenRecord> records_;
};

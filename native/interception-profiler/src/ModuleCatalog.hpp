#pragma once

#include <cstdint>
#include <mutex>
#include <unordered_map>

#include "MetadataTokenCache.hpp"
#include "ModuleAllowlist.hpp"
#include "ProfilerModels.hpp"

class ModuleCatalog {
public:
  explicit ModuleCatalog(ICorProfilerInfo10* info);

  void ReadAllowlist();
  bool IsAllowed(ModuleID module_id) const;
  HRESULT Register(ModuleID module_id);
  HRESULT Catalog(ModuleID module_id);
  void Unload(ModuleID module_id);

  HRESULT ResolveTarget(const alvorkit_interception_target_v2& target, RuntimeMethodKey* method);
  bool TryGetMethod(FunctionID function_id, RuntimeMethodKey* method);
  bool TryGetMethod(ModuleID module_id, mdMethodDef method_id, RuntimeMethodKey* method) const;
  bool IsCurrent(const RuntimeMethodKey& method) const;
  bool TryGetMethodMetadata(const RuntimeMethodKey& method, MethodMetadata* metadata) const;
  HRESULT ResolveRelocation(const RuntimeMethodKey& method, const alvorkit_interception_relocation_v3& relocation,
                            const std::vector<uint8_t>& metadata, mdToken* token);

private:
  HRESULT CatalogModule(ModuleID module_id);
  HRESULT FindModule(const GUID& mvid, ModuleID* module_id);
  HRESULT GetSignatureHash(ModuleID module_id, mdMethodDef method_id, uint64_t* signature_hash);

  ICorProfilerInfo10* info_;
  ModuleAllowlist allowlist_;
  MetadataTokenCache metadata_tokens_;
  // Serializes metadata emission, cataloging, and module unload.
  mutable std::mutex metadata_gate_;
  // Protects module epochs and exact-dispatch method metadata.
  mutable std::mutex mutex_;
  std::unordered_map<RuntimeMethodKey, MethodMetadata, RuntimeMethodKeyHash> methods_;
  std::unordered_map<ModuleID, uint64_t> module_epochs_;
  uint64_t next_module_epoch_ = 0;
};

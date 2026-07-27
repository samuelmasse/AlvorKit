#pragma once

#include <cstdint>
#include <memory>
#include <mutex>
#include <unordered_map>

#include "CompletionStore.hpp"
#include "ProfilerModels.hpp"

struct PendingRejitSnapshot {
  uint64_t request_id = 0;
  alvorkit_interception_operation_v2 operation = ALVORKIT_INTERCEPTION_OPERATION_NONE;
  std::shared_ptr<const PreparedRejit> prepared;
};

struct RejitStateSnapshot {
  uint32_t active_patches = 0;
  uint32_t retained_completions = 0;
  uint64_t last_request_id = 0;
};

class RejitState {
public:
  HRESULT Publish(const ProfilerCommand& command);

  HRESULT ValidateGenerationInstall(const RuntimeMethodKey& method, const ProfilerCommand& command) const;
  HRESULT BeginInstall(const RuntimeMethodKey& method, const ProfilerCommand& command,
                       std::shared_ptr<const PreparedRejit> prepared);
  HRESULT BeginRemove(const ProfilerCommand& command, RuntimeMethodKey* method);

  void ModuleUnloaded(ModuleID module_id);
  void CompilationStarted(const RuntimeMethodKey& method, ReJITID rejit_id);
  bool TryGetParameters(const RuntimeMethodKey& method, PendingRejitSnapshot* snapshot);
  void CompilationFinished(const RuntimeMethodKey& method, ReJITID rejit_id, HRESULT status);
  void RejitError(const RuntimeMethodKey& method, HRESULT status);

  void RecordRelocation(uint64_t request_id, uint32_t index, mdToken token, HRESULT status);
  void FailGeneration(uint64_t request_id, HRESULT status, alvorkit_interception_failure_stage_v3 stage,
                      uint32_t relocation_index = UINT32_MAX);
  void Fail(uint64_t request_id, HRESULT status);

  HRESULT GetCompletion(uint64_t request_id, alvorkit_interception_completion_v2* completion) const;
  HRESULT GetGenerationCompletion(uint64_t request_id,
                                  alvorkit_interception_generation_completion_v3* completion) const;
  HRESULT GetRelocationResult(uint64_t request_id, uint32_t relocation_index,
                              alvorkit_interception_relocation_result_v3* result) const;
  RejitStateSnapshot GetSnapshot() const;

private:
  void FailLocked(uint64_t request_id, HRESULT status);
  void ClearPending(ProfilerPatch* patch);

  // Owns patch and completion transitions as one atomic state domain.
  mutable std::mutex mutex_;
  std::unordered_map<RuntimeMethodKey, ProfilerPatch, RuntimeMethodKeyHash> patches_;
  std::unordered_map<uint64_t, RuntimeMethodKey> methods_by_patch_id_;
  CompletionStore completions_;
};

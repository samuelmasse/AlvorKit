#include "ProfilerRuntime.hpp"

#include <cstdint>

HRESULT ProfilerRuntime::GetCapabilities(alvorkit_interception_capabilities_v2* capabilities) {
  if (capabilities == nullptr)
    return E_INVALIDARG;

  *capabilities = {};
  capabilities->size = sizeof(*capabilities);
  capabilities->abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
  capabilities->flags =
      ALVORKIT_INTERCEPTION_CAPABILITY_REJIT | ALVORKIT_INTERCEPTION_CAPABILITY_REJIT_INLINERS |
      ALVORKIT_INTERCEPTION_CAPABILITY_REVERT | ALVORKIT_INTERCEPTION_CAPABILITY_RAW_IL |
      ALVORKIT_INTERCEPTION_CAPABILITY_MULTIPLE_PATCHES | ALVORKIT_INTERCEPTION_CAPABILITY_SIGNATURE_VALIDATION |
      ALVORKIT_INTERCEPTION_CAPABILITY_EXACT_DISPATCH | ALVORKIT_INTERCEPTION_CAPABILITY_METHOD_GENERATIONS |
      ALVORKIT_INTERCEPTION_CAPABILITY_LATE_METADATA | ALVORKIT_INTERCEPTION_CAPABILITY_IL_MAP |
      ALVORKIT_INTERCEPTION_CAPABILITY_BODY_IDENTITY | ALVORKIT_INTERCEPTION_CAPABILITY_LOADED_BODY;
  capabilities->maximum_il_body_bytes = ALVORKIT_INTERCEPTION_MAX_IL_BODY_BYTES;
  capabilities->maximum_metadata_bytes = ALVORKIT_INTERCEPTION_MAX_METADATA_BYTES;
  capabilities->maximum_relocations = ALVORKIT_INTERCEPTION_MAX_RELOCATIONS;
  capabilities->maximum_il_map_entries = ALVORKIT_INTERCEPTION_MAX_IL_MAP_ENTRIES;
  capabilities->maximum_pending_requests = ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS;
  capabilities->maximum_active_patches = ALVORKIT_INTERCEPTION_MAX_ACTIVE_PATCHES;
  return S_OK;
}

HRESULT ProfilerRuntime::GetProfilerState(alvorkit_interception_profiler_state_v2* state) {
  if (state == nullptr)
    return E_INVALIDARG;

  std::lock_guard lock(queue_mutex_);
  *state = {};
  state->size = sizeof(*state);
  state->abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
  state->ready = ready_ ? 1u : 0u;
  state->stopping = stopping_ ? 1u : 0u;
  state->pending_requests = static_cast<uint32_t>(commands_.size());
  const RejitStateSnapshot rejit = rejit_.GetSnapshot();
  state->active_patches = rejit.active_patches;
  state->retained_completions = rejit.retained_completions;
  state->last_request_id = rejit.last_request_id;
  return S_OK;
}

HRESULT ProfilerRuntime::GetCompletion(uint64_t request_id, alvorkit_interception_completion_v2* completion) {
  if (request_id == 0 || completion == nullptr)
    return E_INVALIDARG;
  return rejit_.GetCompletion(request_id, completion);
}

HRESULT ProfilerRuntime::GetGenerationCompletion(uint64_t request_id,
                                                 alvorkit_interception_generation_completion_v3* completion) {
  if (request_id == 0 || completion == nullptr)
    return E_INVALIDARG;
  return rejit_.GetGenerationCompletion(request_id, completion);
}

HRESULT ProfilerRuntime::GetRelocationResult(uint64_t request_id, uint32_t relocation_index,
                                             alvorkit_interception_relocation_result_v3* result) {
  if (request_id == 0 || result == nullptr)
    return E_INVALIDARG;
  return rejit_.GetRelocationResult(request_id, relocation_index, result);
}

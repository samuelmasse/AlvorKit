#include "InterceptionProfiler.hpp"

#include <cstdint>

HRESULT InterceptionProfiler::GetCapabilities(alvorkit_interception_capabilities_v2* capabilities) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->GetCapabilities(capabilities);
}

HRESULT InterceptionProfiler::GetProfilerState(alvorkit_interception_profiler_state_v2* state) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->GetProfilerState(state);
}

HRESULT InterceptionProfiler::GetLoadedMethodBody(const alvorkit_interception_target_v2* target, uint8_t* body,
                                                  uint32_t body_capacity, uint32_t* body_size,
                                                  alvorkit_interception_body_identity_v3* identity) {
  return runtime_ == nullptr ? E_UNEXPECTED
                             : runtime_->GetLoadedMethodBody(target, body, body_capacity, body_size, identity);
}

HRESULT InterceptionProfiler::EnqueueInstall(const alvorkit_interception_install_v2* request, const uint8_t* il_body,
                                             uint32_t il_body_size) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->EnqueueInstall(request, il_body, il_body_size);
}

HRESULT InterceptionProfiler::EnqueueInstallDispatch(const alvorkit_interception_install_dispatch_v2* request) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->EnqueueInstallDispatch(request);
}

HRESULT InterceptionProfiler::EnqueueGeneration(const alvorkit_interception_generation_v3* request,
                                                const uint8_t* il_body, uint32_t il_body_size,
                                                const alvorkit_interception_relocation_v3* relocations,
                                                uint32_t relocation_count, const uint8_t* metadata,
                                                uint32_t metadata_size, const alvorkit_interception_il_map_v3* il_map,
                                                uint32_t il_map_count) {
  return runtime_ == nullptr
             ? E_UNEXPECTED
             : runtime_->EnqueueGeneration(request, il_body, il_body_size, relocations, relocation_count, metadata,
                                           metadata_size, il_map, il_map_count);
}

HRESULT InterceptionProfiler::EnqueueRemove(const alvorkit_interception_remove_v2* request) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->EnqueueRemove(request);
}

HRESULT InterceptionProfiler::GetCompletion(uint64_t request_id, alvorkit_interception_completion_v2* completion) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->GetCompletion(request_id, completion);
}

HRESULT InterceptionProfiler::GetGenerationCompletion(uint64_t request_id,
                                                      alvorkit_interception_generation_completion_v3* completion) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->GetGenerationCompletion(request_id, completion);
}

HRESULT InterceptionProfiler::GetRelocationResult(uint64_t request_id, uint32_t relocation_index,
                                                  alvorkit_interception_relocation_result_v3* result) {
  return runtime_ == nullptr ? E_UNEXPECTED : runtime_->GetRelocationResult(request_id, relocation_index, result);
}

#include "RejitState.hpp"

#include <cstdint>

HRESULT RejitState::Publish(const ProfilerCommand& command) {
  std::lock_guard lock(mutex_);
  if (completions_.Contains(command.request_id))
    return HRESULT_FROM_WIN32(ERROR_ALREADY_EXISTS);
  return completions_.Publish(command);
}

void RejitState::RecordRelocation(uint64_t request_id, uint32_t index, mdToken token, HRESULT status) {
  std::lock_guard lock(mutex_);
  auto* results = completions_.FindRelocations(request_id);
  if (results != nullptr && index < results->size()) {
    (*results)[index].metadata_token = token;
    (*results)[index].hresult = status;
  }
  auto* completion = completions_.FindGeneration(request_id);
  if (completion != nullptr && SUCCEEDED(status))
    ++completion->applied_relocations;
}

void RejitState::FailGeneration(uint64_t request_id, HRESULT status, alvorkit_interception_failure_stage_v3 stage,
                                uint32_t relocation_index) {
  std::lock_guard lock(mutex_);
  auto* generation = completions_.FindGeneration(request_id);
  if (generation != nullptr) {
    generation->failure_stage = stage;
    generation->failure_relocation_index = relocation_index;
  }
  FailLocked(request_id, status);
}

void RejitState::Fail(uint64_t request_id, HRESULT status) {
  std::lock_guard lock(mutex_);
  FailLocked(request_id, status);
}

void RejitState::FailLocked(uint64_t request_id, HRESULT status) {
  completions_.Fail(request_id, status);
  for (auto iterator = patches_.begin(); iterator != patches_.end(); ++iterator) {
    ProfilerPatch& patch = iterator->second;
    if (patch.pending_request_id != request_id)
      continue;

    ClearPending(&patch);
    if (!patch.active) {
      methods_by_patch_id_.erase(patch.patch_id);
      patches_.erase(iterator);
    }
    break;
  }
}

void RejitState::ClearPending(ProfilerPatch* patch) {
  patch->pending_request_id = 0;
  patch->pending_operation = ALVORKIT_INTERCEPTION_OPERATION_NONE;
  patch->pending_generation_id = 0;
  patch->pending_prior_generation_id = 0;
  patch->pending_rejit.reset();
}

HRESULT RejitState::GetCompletion(uint64_t request_id, alvorkit_interception_completion_v2* completion) const {
  std::lock_guard lock(mutex_);
  return completions_.Get(request_id, completion);
}

HRESULT RejitState::GetGenerationCompletion(uint64_t request_id,
                                            alvorkit_interception_generation_completion_v3* completion) const {
  std::lock_guard lock(mutex_);
  return completions_.GetGeneration(request_id, completion);
}

HRESULT RejitState::GetRelocationResult(uint64_t request_id, uint32_t relocation_index,
                                        alvorkit_interception_relocation_result_v3* result) const {
  std::lock_guard lock(mutex_);
  return completions_.GetRelocation(request_id, relocation_index, result);
}

RejitStateSnapshot RejitState::GetSnapshot() const {
  std::lock_guard lock(mutex_);
  return {static_cast<uint32_t>(patches_.size()), static_cast<uint32_t>(completions_.Count()),
          completions_.LastRequestId()};
}

#include "RejitState.hpp"

#include <cstdint>

void RejitState::ModuleUnloaded(ModuleID module_id) {
  std::lock_guard lock(mutex_);
  for (auto iterator = patches_.begin(); iterator != patches_.end();) {
    if (iterator->first.module_id != module_id) {
      ++iterator;
      continue;
    }

    ProfilerPatch& patch = iterator->second;
    if (patch.pending_request_id != 0) {
      auto* generation = completions_.FindGeneration(patch.pending_request_id);
      if (generation != nullptr) {
        generation->failure_stage = ALVORKIT_INTERCEPTION_FAILURE_REJIT;
      }
      completions_.Fail(patch.pending_request_id, CORPROF_E_DATAINCOMPLETE);
    }
    methods_by_patch_id_.erase(patch.patch_id);
    iterator = patches_.erase(iterator);
  }
}

void RejitState::CompilationStarted(const RuntimeMethodKey& method, ReJITID rejit_id) {
  std::lock_guard lock(mutex_);
  const auto patch = patches_.find(method);
  if (patch == patches_.end() || patch->second.pending_request_id == 0) {
    return;
  }

  const uint64_t request_id = patch->second.pending_request_id;
  auto* completion = completions_.Find(request_id);
  if (completion != nullptr) {
    ++completion->rejit_started_callbacks;
    completion->state = patch->second.pending_operation == ALVORKIT_INTERCEPTION_OPERATION_REMOVE
                            ? ALVORKIT_INTERCEPTION_STATE_REMOVING
                            : ALVORKIT_INTERCEPTION_STATE_APPLYING;
  }
  auto* generation = completions_.FindGeneration(request_id);
  if (generation != nullptr) {
    generation->state = ALVORKIT_INTERCEPTION_STATE_APPLYING;
    generation->target_rejit_id = rejit_id;
  }
}

bool RejitState::TryGetParameters(const RuntimeMethodKey& method, PendingRejitSnapshot* snapshot) {
  std::lock_guard lock(mutex_);
  const auto patch = patches_.find(method);
  if (patch == patches_.end() || patch->second.pending_request_id == 0) {
    return false;
  }

  snapshot->request_id = patch->second.pending_request_id;
  snapshot->operation = patch->second.pending_operation;
  snapshot->prepared = patch->second.pending_rejit;
  auto* completion = completions_.Find(snapshot->request_id);
  if (completion != nullptr) {
    ++completion->parameter_callbacks;
    if (snapshot->operation != ALVORKIT_INTERCEPTION_OPERATION_REMOVE) {
      completion->state = ALVORKIT_INTERCEPTION_STATE_APPLYING;
    }
  }
  return true;
}

void RejitState::CompilationFinished(const RuntimeMethodKey& method, ReJITID rejit_id, HRESULT status) {
  std::lock_guard lock(mutex_);
  auto patch = patches_.find(method);
  if (patch == patches_.end() || patch->second.pending_request_id == 0) {
    return;
  }

  ProfilerPatch& value = patch->second;
  const uint64_t request_id = value.pending_request_id;
  auto* completion = completions_.Find(request_id);
  if (completion == nullptr)
    return;

  ++completion->rejit_finished_callbacks;
  completion->hresult = status;
  completions_.RecordElapsed(request_id);
  if (FAILED(status)) {
    auto* generation = completions_.FindGeneration(request_id);
    if (generation != nullptr) {
      generation->failure_stage = ALVORKIT_INTERCEPTION_FAILURE_REJIT;
      generation->target_rejit_id = rejit_id;
    }
    completions_.Fail(request_id, status);
    ClearPending(&value);
    if (!value.active) {
      methods_by_patch_id_.erase(value.patch_id);
      patches_.erase(patch);
    }
    return;
  }

  if (value.pending_operation == ALVORKIT_INTERCEPTION_OPERATION_REMOVE) {
    completion->state = ALVORKIT_INTERCEPTION_STATE_REMOVED;
    methods_by_patch_id_.erase(value.patch_id);
    patches_.erase(patch);
    return;
  }

  value.active_generation_id = value.pending_generation_id;
  value.active = true;
  ClearPending(&value);
  completion->state = ALVORKIT_INTERCEPTION_STATE_ACTIVE;
  auto* generation = completions_.FindGeneration(request_id);
  if (generation != nullptr) {
    generation->state = ALVORKIT_INTERCEPTION_STATE_ACTIVE;
    generation->hresult = S_OK;
    generation->applied_il_map_entries = generation->requested_il_map_entries;
    generation->target_rejit_id = rejit_id;
  }
}

void RejitState::RejitError(const RuntimeMethodKey& method, HRESULT status) {
  std::lock_guard lock(mutex_);
  auto patch = patches_.find(method);
  if (patch == patches_.end() || patch->second.pending_request_id == 0) {
    return;
  }

  const uint64_t request_id = patch->second.pending_request_id;
  auto* completion = completions_.Find(request_id);
  if (completion != nullptr) {
    ++completion->rejit_error_callbacks;
    completion->hresult = status;
  }
  auto* generation = completions_.FindGeneration(request_id);
  if (generation != nullptr) {
    generation->failure_stage = ALVORKIT_INTERCEPTION_FAILURE_REJIT;
  }
  completions_.Fail(request_id, status);
  ClearPending(&patch->second);
  if (!patch->second.active) {
    methods_by_patch_id_.erase(patch->second.patch_id);
    patches_.erase(patch);
  }
}

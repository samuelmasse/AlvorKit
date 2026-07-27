#include "RejitState.hpp"

#include "InterceptionIdentity.hpp"

#include <utility>

HRESULT RejitState::ValidateGenerationInstall(const RuntimeMethodKey& method, const ProfilerCommand& command) const {
  std::lock_guard lock(mutex_);
  const auto existing_id = methods_by_patch_id_.find(command.patch_id);
  if (existing_id != methods_by_patch_id_.end() && !(existing_id->second == method)) {
    return E_ACCESSDENIED;
  }

  const auto existing_patch = patches_.find(method);
  if (existing_patch == patches_.end()) {
    return command.prior_generation_id == 0 ? S_OK : HRESULT_FROM_WIN32(ERROR_REVISION_MISMATCH);
  }

  const ProfilerPatch& patch = existing_patch->second;
  if (patch.patch_id != command.patch_id || !EqualTarget(patch.target, command.target)) {
    return E_ACCESSDENIED;
  }
  if (patch.pending_request_id != 0)
    return HRESULT_FROM_WIN32(ERROR_BUSY);
  return patch.active_generation_id == command.prior_generation_id ? S_OK : HRESULT_FROM_WIN32(ERROR_REVISION_MISMATCH);
}

HRESULT RejitState::BeginInstall(const RuntimeMethodKey& method, const ProfilerCommand& command,
                                 std::shared_ptr<const PreparedRejit> prepared) {
  std::lock_guard lock(mutex_);
  auto existing_id = methods_by_patch_id_.find(command.patch_id);
  if (existing_id != methods_by_patch_id_.end() && !(existing_id->second == method)) {
    FailLocked(command.request_id, E_ACCESSDENIED);
    return E_ACCESSDENIED;
  }

  auto patch = patches_.find(method);
  if (patch == patches_.end()) {
    if (patches_.size() >= ALVORKIT_INTERCEPTION_MAX_ACTIVE_PATCHES) {
      const HRESULT status = HRESULT_FROM_WIN32(68u); // ERROR_TOO_MANY_NAMES
      FailLocked(command.request_id, status);
      return status;
    }

    ProfilerPatch value;
    value.patch_id = command.patch_id;
    value.target = command.target;
    value.method = method;
    patch = patches_.emplace(method, std::move(value)).first;
    try {
      methods_by_patch_id_.emplace(command.patch_id, method);
    } catch (...) {
      patches_.erase(patch);
      throw;
    }
  } else if (patch->second.patch_id != command.patch_id || !EqualTarget(patch->second.target, command.target)) {
    FailLocked(command.request_id, E_ACCESSDENIED);
    return E_ACCESSDENIED;
  }

  ProfilerPatch& value = patch->second;
  if (command.body_kind == ProfilerBodyKind::Generation && value.active_generation_id != command.prior_generation_id) {
    const HRESULT status = HRESULT_FROM_WIN32(ERROR_REVISION_MISMATCH);
    auto* generation = completions_.FindGeneration(command.request_id);
    if (generation != nullptr)
      generation->failure_stage = ALVORKIT_INTERCEPTION_FAILURE_VALIDATION;
    FailLocked(command.request_id, status);
    return status;
  }
  if (value.pending_request_id != 0) {
    const HRESULT status = HRESULT_FROM_WIN32(ERROR_BUSY);
    FailLocked(command.request_id, status);
    return status;
  }

  value.pending_request_id = command.request_id;
  value.pending_operation =
      value.active ? ALVORKIT_INTERCEPTION_OPERATION_REPLACE : ALVORKIT_INTERCEPTION_OPERATION_INSTALL;
  value.pending_generation_id = command.generation_id;
  value.pending_prior_generation_id = command.prior_generation_id;
  value.pending_rejit = std::move(prepared);

  auto* completion = completions_.Find(command.request_id);
  if (completion != nullptr) {
    completion->operation = value.pending_operation;
    completion->state = ALVORKIT_INTERCEPTION_STATE_REQUESTED;
  }
  auto* generation = completions_.FindGeneration(command.request_id);
  if (generation != nullptr)
    generation->state = ALVORKIT_INTERCEPTION_STATE_REQUESTED;
  return S_OK;
}

HRESULT RejitState::BeginRemove(const ProfilerCommand& command, RuntimeMethodKey* method) {
  std::lock_guard lock(mutex_);
  const auto patch_method = methods_by_patch_id_.find(command.patch_id);
  if (patch_method == methods_by_patch_id_.end()) {
    const HRESULT status = HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
    FailLocked(command.request_id, status);
    return status;
  }

  *method = patch_method->second;
  auto patch = patches_.find(*method);
  if (patch == patches_.end() || patch->second.pending_request_id != 0 ||
      !EqualTarget(patch->second.target, command.target)) {
    const HRESULT status =
        patch == patches_.end() ? HRESULT_FROM_WIN32(ERROR_NOT_FOUND) : HRESULT_FROM_WIN32(ERROR_BUSY);
    FailLocked(command.request_id, status);
    return status;
  }

  patch->second.pending_request_id = command.request_id;
  patch->second.pending_operation = ALVORKIT_INTERCEPTION_OPERATION_REMOVE;
  auto* completion = completions_.Find(command.request_id);
  if (completion != nullptr)
    completion->state = ALVORKIT_INTERCEPTION_STATE_REMOVING;
  return S_OK;
}

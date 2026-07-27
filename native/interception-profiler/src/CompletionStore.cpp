#include "CompletionStore.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <new>
#include <utility>

namespace {
constexpr size_t maximum_retained_completions = 2048;

bool IsTerminal(int32_t state) {
  return state == ALVORKIT_INTERCEPTION_STATE_ACTIVE || state == ALVORKIT_INTERCEPTION_STATE_REMOVED ||
         state == ALVORKIT_INTERCEPTION_STATE_FAILED;
}
} // namespace

bool CompletionStore::Contains(uint64_t request_id) const {
  return completions_.contains(request_id);
}

HRESULT CompletionStore::Publish(const ProfilerCommand& command) {
  try {
    alvorkit_interception_completion_v2 completion{};
    completion.size = sizeof(completion);
    completion.abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
    completion.request_id = command.request_id;
    completion.patch_id = command.patch_id;
    completion.operation = command.operation;
    completion.state = ALVORKIT_INTERCEPTION_STATE_QUEUED;
    completion.hresult = S_OK;
    completion.patch_flags = command.patch_flags;
    completion.target = command.target;
    completions_.emplace(command.request_id, completion);

    if (command.body_kind == ProfilerBodyKind::Generation) {
      alvorkit_interception_generation_completion_v3 generation{};
      generation.size = sizeof(generation);
      generation.abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
      generation.request_id = command.request_id;
      generation.patch_id = command.patch_id;
      generation.generation_id = command.generation_id;
      generation.prior_generation_id = command.prior_generation_id;
      generation.state = ALVORKIT_INTERCEPTION_STATE_QUEUED;
      generation.hresult = S_OK;
      generation.failure_relocation_index = UINT32_MAX;
      generation.requested_relocations = static_cast<uint32_t>(command.relocations.size());
      generation.requested_il_map_entries = static_cast<uint32_t>(command.il_map.size());
      generation_completions_.emplace(command.request_id, generation);

      std::vector<alvorkit_interception_relocation_result_v3> results;
      results.reserve(command.relocations.size());
      for (uint32_t index = 0; index < command.relocations.size(); ++index) {
        alvorkit_interception_relocation_result_v3 result{};
        result.size = sizeof(result);
        result.abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
        result.request_id = command.request_id;
        result.generation_id = command.generation_id;
        result.relocation_index = index;
        result.kind = command.relocations[index].kind;
        result.hresult = E_PENDING;
        results.push_back(result);
      }
      relocation_results_.emplace(command.request_id, std::move(results));
    }

    started_.emplace(command.request_id, ProfilerClock::now());
    order_.push_back(command.request_id);
    last_request_id_ = command.request_id;
    Trim();
    return S_OK;
  } catch (const std::bad_alloc&) {
    completions_.erase(command.request_id);
    generation_completions_.erase(command.request_id);
    relocation_results_.erase(command.request_id);
    started_.erase(command.request_id);
    std::erase(order_, command.request_id);
    return E_OUTOFMEMORY;
  } catch (...) {
    completions_.erase(command.request_id);
    generation_completions_.erase(command.request_id);
    relocation_results_.erase(command.request_id);
    started_.erase(command.request_id);
    std::erase(order_, command.request_id);
    return E_FAIL;
  }
}

alvorkit_interception_completion_v2* CompletionStore::Find(uint64_t request_id) {
  const auto value = completions_.find(request_id);
  return value == completions_.end() ? nullptr : &value->second;
}

alvorkit_interception_generation_completion_v3* CompletionStore::FindGeneration(uint64_t request_id) {
  const auto value = generation_completions_.find(request_id);
  return value == generation_completions_.end() ? nullptr : &value->second;
}

std::vector<alvorkit_interception_relocation_result_v3>* CompletionStore::FindRelocations(uint64_t request_id) {
  const auto value = relocation_results_.find(request_id);
  return value == relocation_results_.end() ? nullptr : &value->second;
}

HRESULT CompletionStore::Get(uint64_t request_id, alvorkit_interception_completion_v2* completion) const {
  const auto value = completions_.find(request_id);
  if (value == completions_.end())
    return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
  *completion = value->second;
  return S_OK;
}

HRESULT CompletionStore::GetGeneration(uint64_t request_id,
                                       alvorkit_interception_generation_completion_v3* completion) const {
  const auto value = generation_completions_.find(request_id);
  if (value == generation_completions_.end())
    return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
  *completion = value->second;
  return S_OK;
}

HRESULT CompletionStore::GetRelocation(uint64_t request_id, uint32_t relocation_index,
                                       alvorkit_interception_relocation_result_v3* result) const {
  const auto values = relocation_results_.find(request_id);
  if (values == relocation_results_.end() || relocation_index >= values->second.size()) {
    return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
  }
  *result = values->second[relocation_index];
  return S_OK;
}

void CompletionStore::RecordElapsed(uint64_t request_id) {
  auto* completion = Find(request_id);
  const auto started = started_.find(request_id);
  if (completion == nullptr || started == started_.end())
    return;
  completion->elapsed_microseconds = static_cast<uint64_t>(
      std::chrono::duration_cast<std::chrono::microseconds>(ProfilerClock::now() - started->second).count());
}

void CompletionStore::Fail(uint64_t request_id, HRESULT status, alvorkit_interception_failure_stage_v3 stage,
                           uint32_t relocation_index) {
  auto* completion = Find(request_id);
  if (completion == nullptr || IsTerminal(completion->state))
    return;
  completion->state = ALVORKIT_INTERCEPTION_STATE_FAILED;
  completion->hresult = status;
  auto* generation = FindGeneration(request_id);
  if (generation != nullptr) {
    generation->state = ALVORKIT_INTERCEPTION_STATE_FAILED;
    generation->hresult = status;
    if (generation->failure_stage == ALVORKIT_INTERCEPTION_FAILURE_NONE) {
      generation->failure_stage = stage;
      generation->failure_relocation_index = relocation_index;
    }
  }
  RecordElapsed(request_id);
}

size_t CompletionStore::Count() const {
  return completions_.size();
}

uint64_t CompletionStore::LastRequestId() const {
  return last_request_id_;
}

void CompletionStore::Trim() {
  while (order_.size() > maximum_retained_completions) {
    const uint64_t request_id = order_.front();
    const auto completion = completions_.find(request_id);
    if (completion != completions_.end() && !IsTerminal(completion->second.state)) {
      break;
    }

    order_.pop_front();
    completions_.erase(request_id);
    generation_completions_.erase(request_id);
    relocation_results_.erase(request_id);
    started_.erase(request_id);
  }
}

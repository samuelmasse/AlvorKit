#pragma once

#include <chrono>
#include <cstddef>
#include <cstdint>
#include <deque>
#include <unordered_map>
#include <vector>

#include "ProfilerModels.hpp"

class CompletionStore {
public:
  bool Contains(uint64_t request_id) const;
  HRESULT Publish(const ProfilerCommand& command);

  alvorkit_interception_completion_v2* Find(uint64_t request_id);
  alvorkit_interception_generation_completion_v3* FindGeneration(uint64_t request_id);
  std::vector<alvorkit_interception_relocation_result_v3>* FindRelocations(uint64_t request_id);

  HRESULT Get(uint64_t request_id, alvorkit_interception_completion_v2* completion) const;
  HRESULT GetGeneration(uint64_t request_id, alvorkit_interception_generation_completion_v3* completion) const;
  HRESULT GetRelocation(uint64_t request_id, uint32_t relocation_index,
                        alvorkit_interception_relocation_result_v3* result) const;

  void RecordElapsed(uint64_t request_id);
  void Fail(uint64_t request_id, HRESULT status,
            alvorkit_interception_failure_stage_v3 stage = ALVORKIT_INTERCEPTION_FAILURE_REJIT,
            uint32_t relocation_index = UINT32_MAX);

  size_t Count() const;
  uint64_t LastRequestId() const;

private:
  void Trim();

  std::unordered_map<uint64_t, alvorkit_interception_completion_v2> completions_;
  std::unordered_map<uint64_t, alvorkit_interception_generation_completion_v3> generation_completions_;
  std::unordered_map<uint64_t, std::vector<alvorkit_interception_relocation_result_v3>> relocation_results_;
  std::unordered_map<uint64_t, ProfilerClock::time_point> started_;
  std::deque<uint64_t> order_;
  uint64_t last_request_id_ = 0;
};

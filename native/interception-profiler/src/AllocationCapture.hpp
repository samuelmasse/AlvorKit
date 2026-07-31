#pragma once

#include <atomic>
#include <cstdint>
#include <mutex>
#include <vector>

#include "CoreClrHeaders.hpp"
#include "alvorkit_interception_profiler.h"

class AllocationCapture {
public:
  AllocationCapture(ICorProfilerInfo10* info, bool enabled);

  bool IsEnabled() const;
  void ObjectAllocated(ClassID class_id) noexcept;

  HRESULT Begin(const alvorkit_interception_allocation_capture_v3* request);
  HRESULT End(alvorkit_interception_allocation_summary_v3* summary);
  HRESULT GetSample(uint32_t sample_index, alvorkit_interception_allocation_sample_v3* sample,
                    alvorkit_interception_allocation_frame_v3* frames, uint32_t frame_capacity);
  HRESULT ResolveFrame(const alvorkit_interception_allocation_frame_v3* frame,
                       alvorkit_interception_resolved_frame_v3* resolved);

private:
  struct SampleSlot {
    uint64_t allocation_ordinal = 0;
    uint64_t class_id = 0;
    uint32_t frame_count = 0;
    HRESULT stack_status = S_OK;
  };

  struct StackContext {
    alvorkit_interception_allocation_frame_v3* frames;
    uint32_t capacity;
    uint32_t count;
  };

  static HRESULT STDMETHODCALLTYPE RecordStackFrame(FunctionID function_id, UINT_PTR instruction_pointer,
                                                    COR_PRF_FRAME_INFO frame_info, ULONG32 context_size, BYTE context[],
                                                    void* client_data);
  void RecordSample(uint64_t allocation_ordinal, ClassID class_id) noexcept;

  static constexpr uint64_t ActiveBit = uint64_t{1} << 63;
  static constexpr uint64_t CountMask = ~ActiveBit;

  ICorProfilerInfo10* info_;
  bool enabled_;
  std::mutex operation_mutex_;
  std::vector<SampleSlot> samples_;
  std::vector<alvorkit_interception_allocation_frame_v3> frames_;
  std::atomic<uint64_t> capture_state_{0};
  std::atomic<uint64_t> completed_allocations_{0};
  std::atomic<uint64_t> reserved_samples_{0};
  std::atomic<uint64_t> dropped_samples_{0};
  std::atomic<uint64_t> failed_stack_walks_{0};
  uint64_t captured_allocations_ = 0;
  uint32_t sample_interval_ = 0;
  uint32_t maximum_frames_per_sample_ = 0;
  bool result_available_ = false;
};

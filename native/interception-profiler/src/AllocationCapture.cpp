#include "AllocationCapture.hpp"

#include <algorithm>
#include <new>
#include <thread>

namespace {
constexpr HRESULT kInsufficientBuffer = static_cast<HRESULT>(0x8007007aL);
}

AllocationCapture::AllocationCapture(ICorProfilerInfo10* info, bool enabled) : info_(info), enabled_(enabled) {}

bool AllocationCapture::IsEnabled() const {
  return enabled_;
}

HRESULT AllocationCapture::Begin(const alvorkit_interception_allocation_capture_v3* request) {
  if (!enabled_)
    return E_NOTIMPL;
  if (request == nullptr || request->size != sizeof(*request) ||
      request->abi_version != ALVORKIT_INTERCEPTION_ABI_VERSION || request->sample_interval == 0 ||
      request->maximum_samples > ALVORKIT_INTERCEPTION_MAX_ALLOCATION_SAMPLES ||
      request->maximum_frames_per_sample > ALVORKIT_INTERCEPTION_MAX_ALLOCATION_FRAMES ||
      (request->maximum_samples != 0 && request->maximum_frames_per_sample == 0)) {
    return E_INVALIDARG;
  }

  std::lock_guard lock(operation_mutex_);
  if ((capture_state_.load(std::memory_order_acquire) & ActiveBit) != 0)
    return E_UNEXPECTED;

  try {
    samples_.assign(request->maximum_samples, {});
    frames_.assign(static_cast<size_t>(request->maximum_samples) * request->maximum_frames_per_sample, {});
  } catch (const std::bad_alloc&) {
    samples_.clear();
    frames_.clear();
    return E_OUTOFMEMORY;
  } catch (...) {
    samples_.clear();
    frames_.clear();
    return E_FAIL;
  }

  sample_interval_ = request->sample_interval;
  maximum_frames_per_sample_ = request->maximum_frames_per_sample;
  captured_allocations_ = 0;
  result_available_ = false;
  completed_allocations_.store(0, std::memory_order_relaxed);
  reserved_samples_.store(0, std::memory_order_relaxed);
  dropped_samples_.store(0, std::memory_order_relaxed);
  failed_stack_walks_.store(0, std::memory_order_relaxed);
  capture_state_.store(ActiveBit, std::memory_order_release);
  return S_OK;
}

HRESULT AllocationCapture::End(alvorkit_interception_allocation_summary_v3* summary) {
  if (summary == nullptr)
    return E_INVALIDARG;

  std::lock_guard lock(operation_mutex_);
  const uint64_t prior = capture_state_.fetch_and(CountMask, std::memory_order_acq_rel);
  if ((prior & ActiveBit) == 0)
    return E_UNEXPECTED;

  captured_allocations_ = prior & CountMask;
  while (completed_allocations_.load(std::memory_order_acquire) != captured_allocations_)
    std::this_thread::yield();

  result_available_ = true;
  *summary = {};
  summary->size = sizeof(*summary);
  summary->abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
  summary->total_object_allocations = captured_allocations_;
  summary->sampled_object_allocations =
      std::min<uint64_t>(reserved_samples_.load(std::memory_order_relaxed), static_cast<uint64_t>(samples_.size()));
  summary->dropped_samples = dropped_samples_.load(std::memory_order_relaxed);
  summary->failed_stack_walks = failed_stack_walks_.load(std::memory_order_relaxed);
  summary->sample_interval = sample_interval_;
  summary->maximum_frames_per_sample = maximum_frames_per_sample_;
  return S_OK;
}

HRESULT AllocationCapture::GetSample(uint32_t sample_index, alvorkit_interception_allocation_sample_v3* sample,
                                     alvorkit_interception_allocation_frame_v3* frames, uint32_t frame_capacity) {
  if (sample == nullptr)
    return E_INVALIDARG;

  std::lock_guard lock(operation_mutex_);
  const uint64_t sample_count =
      std::min<uint64_t>(reserved_samples_.load(std::memory_order_relaxed), static_cast<uint64_t>(samples_.size()));
  if (!result_available_ || sample_index >= sample_count)
    return E_INVALIDARG;

  const SampleSlot& slot = samples_[sample_index];
  *sample = {};
  sample->size = sizeof(*sample);
  sample->abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
  sample->allocation_ordinal = slot.allocation_ordinal;
  sample->class_id = slot.class_id;
  sample->frame_count = slot.frame_count;
  sample->stack_hresult = slot.stack_status;
  if (frame_capacity < slot.frame_count || (slot.frame_count != 0 && frames == nullptr))
    return kInsufficientBuffer;

  if (slot.frame_count != 0) {
    const size_t frame_offset = static_cast<size_t>(sample_index) * maximum_frames_per_sample_;
    std::copy_n(frames_.data() + frame_offset, slot.frame_count, frames);
  }
  return S_OK;
}

void AllocationCapture::ObjectAllocated(ClassID class_id) noexcept {
  uint64_t state = capture_state_.load(std::memory_order_acquire);
  while ((state & ActiveBit) != 0) {
    if ((state & CountMask) == CountMask)
      return;
    if (!capture_state_.compare_exchange_weak(state, state + 1, std::memory_order_acq_rel, std::memory_order_acquire)) {
      continue;
    }

    const uint64_t ordinal = (state & CountMask) + 1;
    if (!samples_.empty() && (ordinal - 1) % sample_interval_ == 0)
      RecordSample(ordinal, class_id);
    completed_allocations_.fetch_add(1, std::memory_order_acq_rel);
    return;
  }
}

void AllocationCapture::RecordSample(uint64_t allocation_ordinal, ClassID class_id) noexcept {
  const uint64_t index = reserved_samples_.fetch_add(1, std::memory_order_relaxed);
  if (index >= samples_.size()) {
    dropped_samples_.fetch_add(1, std::memory_order_relaxed);
    return;
  }

  SampleSlot& sample = samples_[static_cast<size_t>(index)];
  sample.allocation_ordinal = allocation_ordinal;
  sample.class_id = static_cast<uint64_t>(class_id);
  StackContext context{frames_.data() + static_cast<size_t>(index) * maximum_frames_per_sample_,
                       maximum_frames_per_sample_, 0};
  sample.stack_status = info_->DoStackSnapshot(0, RecordStackFrame, COR_PRF_SNAPSHOT_DEFAULT, &context, nullptr, 0);
  sample.frame_count = context.count;
  if (FAILED(sample.stack_status))
    failed_stack_walks_.fetch_add(1, std::memory_order_relaxed);
}

HRESULT STDMETHODCALLTYPE AllocationCapture::RecordStackFrame(FunctionID function_id, UINT_PTR instruction_pointer,
                                                              COR_PRF_FRAME_INFO, ULONG32, BYTE[], void* client_data) {
  auto* context = static_cast<StackContext*>(client_data);
  if (context == nullptr || function_id == 0 || context->count >= context->capacity)
    return S_OK;

  context->frames[context->count++] = {static_cast<uint64_t>(function_id), static_cast<uint64_t>(instruction_pointer)};
  return S_OK;
}

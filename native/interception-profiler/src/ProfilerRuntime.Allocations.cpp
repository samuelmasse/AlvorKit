#include "ProfilerRuntime.hpp"

#include <new>

namespace {
constexpr HRESULT kCommandQueueFull = HRESULT_FROM_WIN32(56u);
}

HRESULT ProfilerRuntime::ResolveAllocationFrame(const alvorkit_interception_allocation_frame_v3* frame,
                                                alvorkit_interception_resolved_frame_v3* resolved) {
  if (frame == nullptr || resolved == nullptr)
    return E_INVALIDARG;

  AllocationFrameResolveRequest request;
  request.frame = *frame;
  {
    std::unique_lock lock(queue_mutex_);
    if (stopping_ || !ready_)
      return E_UNEXPECTED;
    if (frame_requests_.size() >= ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS)
      return kCommandQueueFull;
    try {
      frame_requests_.push_back(&request);
    } catch (const std::bad_alloc&) {
      return E_OUTOFMEMORY;
    } catch (...) {
      return E_FAIL;
    }
    changed_.notify_one();
    frame_completed_.wait(lock, [&request, this] { return request.completed || stopping_; });
    if (!request.completed)
      return E_UNEXPECTED;
  }

  if (SUCCEEDED(request.status))
    *resolved = request.resolved;
  return request.status;
}

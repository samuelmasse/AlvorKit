#include "ProfilerRuntime.hpp"

#include <new>

ProfilerRuntime::ProfilerRuntime(ICorProfilerInfo10* info, bool allocation_capture_enabled)
    : info_(info), allocation_capture_(info, allocation_capture_enabled), modules_(info) {}

ProfilerRuntime::~ProfilerRuntime() {
  Stop();
  info_->Release();
}

HRESULT ProfilerRuntime::Start() {
  try {
    modules_.ReadAllowlist();
    {
      std::lock_guard lock(queue_mutex_);
      ready_ = true;
    }
    worker_ = std::thread(&ProfilerRuntime::WorkerMain, this);
  } catch (const std::bad_alloc&) {
    std::lock_guard lock(queue_mutex_);
    ready_ = false;
    return E_OUTOFMEMORY;
  } catch (...) {
    std::lock_guard lock(queue_mutex_);
    ready_ = false;
    return E_FAIL;
  }
  return S_OK;
}

void ProfilerRuntime::Stop() {
  {
    std::lock_guard lock(queue_mutex_);
    if (stopping_)
      return;
    stopping_ = true;
    ready_ = false;
    changed_.notify_all();
    body_completed_.notify_all();
    frame_completed_.notify_all();
  }
  if (worker_.joinable())
    worker_.join();
}

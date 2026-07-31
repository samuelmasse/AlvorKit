#include "InterceptionProfiler.hpp"

#include <cstdlib>
#include <mutex>
#include <new>
#include <string_view>

namespace {
std::mutex instance_mutex;
InterceptionProfiler* instance = nullptr;

bool AllocationCaptureEnabled() {
#ifdef _WIN32
  wchar_t* configured = nullptr;
  size_t configured_size = 0;
  if (_wdupenv_s(&configured, &configured_size, L"ALVORKIT_INTERCEPTION_ALLOCATION_PROFILING") != 0 ||
      configured == nullptr) {
    return false;
  }
  const std::wstring_view value(configured);
  const bool enabled = value == L"1" || value == L"true" || value == L"TRUE";
  std::free(configured);
  return enabled;
#else
  const char* configured = std::getenv("ALVORKIT_INTERCEPTION_ALLOCATION_PROFILING");
  if (configured == nullptr)
    return false;
  const std::string_view value(configured);
  return value == "1" || value == "true" || value == "TRUE";
#endif
}

template <typename Action> HRESULT GuardCallback(Action action) noexcept {
  try {
    return action();
  } catch (const std::bad_alloc&) {
    return E_OUTOFMEMORY;
  } catch (...) {
    return E_FAIL;
  }
}
} // namespace

const GUID kAlvorKitInterceptionProfilerClsid = {
    0x3840acf7, 0x5af1, 0x49ea, {0xbf, 0x94, 0x5f, 0x70, 0x86, 0xc5, 0x7f, 0x57}};

InterceptionProfiler::~InterceptionProfiler() {
  runtime_.reset();
  DetachInstance();
}

InterceptionProfiler* InterceptionProfiler::AcquireInstance() {
  std::lock_guard lock(instance_mutex);
  if (instance != nullptr)
    instance->AddRef();
  return instance;
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::Initialize(IUnknown* unknown) {
  ICorProfilerInfo10* info = nullptr;
  HRESULT status = unknown->QueryInterface(__uuidof(ICorProfilerInfo10), reinterpret_cast<void**>(&info));
  if (FAILED(status))
    return status;

  const bool allocation_capture_enabled = AllocationCaptureEnabled();
  DWORD events = static_cast<DWORD>(COR_PRF_MONITOR_MODULE_LOADS | COR_PRF_MONITOR_JIT_COMPILATION |
                                    COR_PRF_ENABLE_REJIT | COR_PRF_DISABLE_ALL_NGEN_IMAGES);
  if (allocation_capture_enabled) {
    events |= static_cast<DWORD>(COR_PRF_ENABLE_OBJECT_ALLOCATED | COR_PRF_MONITOR_OBJECT_ALLOCATED |
                                 COR_PRF_ENABLE_STACK_SNAPSHOT);
  }
  status = info->SetEventMask2(events, 0);
  if (FAILED(status)) {
    info->Release();
    return status;
  }

  runtime_.reset(new (std::nothrow) ProfilerRuntime(info, allocation_capture_enabled));
  if (runtime_ == nullptr) {
    info->Release();
    return E_OUTOFMEMORY;
  }
  status = runtime_->Start();
  if (FAILED(status)) {
    runtime_.reset();
    return status;
  }

  std::lock_guard lock(instance_mutex);
  instance = this;
  return S_OK;
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::Shutdown() {
  DetachInstance();
  runtime_.reset();
  return S_OK;
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ModuleLoadFinished(ModuleID module_id, HRESULT status) {
  return GuardCallback([this, module_id, status] {
    return runtime_ == nullptr ? S_OK : runtime_->ModuleLoadFinished(module_id, status);
  });
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ModuleUnloadStarted(ModuleID module_id) {
  return GuardCallback(
      [this, module_id] { return runtime_ == nullptr ? S_OK : runtime_->ModuleUnloadStarted(module_id); });
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ObjectAllocated(ObjectID, ClassID class_id) {
  if (runtime_ != nullptr)
    runtime_->ObjectAllocated(class_id);
  return S_OK;
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ReJITCompilationStarted(FunctionID function_id, ReJITID rejit_id,
                                                                        BOOL) {
  return GuardCallback([this, function_id, rejit_id] {
    return runtime_ == nullptr ? S_OK : runtime_->ReJITCompilationStarted(function_id, rejit_id);
  });
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::GetReJITParameters(ModuleID module_id, mdMethodDef method_id,
                                                                   ICorProfilerFunctionControl* control) {
  return GuardCallback([this, module_id, method_id, control] {
    return runtime_ == nullptr ? S_OK : runtime_->GetReJITParameters(module_id, method_id, control);
  });
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ReJITCompilationFinished(FunctionID function_id, ReJITID rejit_id,
                                                                         HRESULT status, BOOL) {
  return GuardCallback([this, function_id, rejit_id, status] {
    return runtime_ == nullptr ? S_OK : runtime_->ReJITCompilationFinished(function_id, rejit_id, status);
  });
}

HRESULT STDMETHODCALLTYPE InterceptionProfiler::ReJITError(ModuleID module_id, mdMethodDef method_id, FunctionID,
                                                           HRESULT status) {
  return GuardCallback([this, module_id, method_id, status] {
    return runtime_ == nullptr ? S_OK : runtime_->ReJITError(module_id, method_id, status);
  });
}

void InterceptionProfiler::DetachInstance() {
  std::lock_guard lock(instance_mutex);
  if (instance == this)
    instance = nullptr;
}

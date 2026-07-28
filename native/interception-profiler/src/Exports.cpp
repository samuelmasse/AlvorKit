#include "InterceptionClassFactory.hpp"
#include "InterceptionProfiler.hpp"
#include "alvorkit_interception_profiler.h"

#include <cstdint>
#include <new>

#ifdef _WIN32
#include <objbase.h>
#include <windows.h>
#define ALVORKIT_PROFILER_EXPORT STDAPI
#else
#define ALVORKIT_PROFILER_EXPORT extern "C" __attribute__((visibility("default"))) HRESULT STDMETHODCALLTYPE
#endif

namespace {
class ProfilerReference {
public:
  ProfilerReference() : value_(InterceptionProfiler::AcquireInstance()) {}
  ~ProfilerReference() {
    if (value_ != nullptr)
      value_->Release();
  }

  InterceptionProfiler* Get() const {
    return value_;
  }

private:
  InterceptionProfiler* value_;
};

template <typename Action> int32_t InvokeProfiler(Action action) noexcept {
  try {
    ProfilerReference profiler;
    if (profiler.Get() == nullptr)
      return E_UNEXPECTED;
    return action(profiler.Get());
  } catch (const std::bad_alloc&) {
    return E_OUTOFMEMORY;
  } catch (...) {
    return E_FAIL;
  }
}
} // namespace

ALVORKIT_PROFILER_EXPORT DllGetClassObject(REFCLSID class_id, REFIID interface_id, void** value) {
  if (value == nullptr)
    return E_POINTER;
  *value = nullptr;
  if (class_id != kAlvorKitInterceptionProfilerClsid)
    return CLASS_E_CLASSNOTAVAILABLE;

  InterceptionClassFactory* factory = new (std::nothrow) InterceptionClassFactory();
  if (factory == nullptr)
    return E_OUTOFMEMORY;
  const HRESULT status = factory->QueryInterface(interface_id, value);
  if (FAILED(status))
    delete factory;
  return status;
}

ALVORKIT_PROFILER_EXPORT DllCanUnloadNow() {
  return S_FALSE;
}

extern "C" ALVORKIT_INTERCEPTION_API uint32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_abi_version() {
  return ALVORKIT_INTERCEPTION_ABI_VERSION;
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_capabilities(alvorkit_interception_capabilities_v2* capabilities) {
  return InvokeProfiler(
      [capabilities](InterceptionProfiler* profiler) { return profiler->GetCapabilities(capabilities); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_profiler_state(alvorkit_interception_profiler_state_v2* state) {
  return InvokeProfiler([state](InterceptionProfiler* profiler) { return profiler->GetProfilerState(state); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_loaded_method_body(
    const alvorkit_interception_target_v2* target, uint8_t* body, uint32_t body_capacity, uint32_t* body_size,
    alvorkit_interception_body_identity_v3* identity) {
  return InvokeProfiler([=](InterceptionProfiler* profiler) {
    return profiler->GetLoadedMethodBody(target, body, body_capacity, body_size, identity);
  });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_enqueue_install(
    const alvorkit_interception_install_v2* request, const uint8_t* il_body, uint32_t il_body_size) {
  return InvokeProfiler(
      [=](InterceptionProfiler* profiler) { return profiler->EnqueueInstall(request, il_body, il_body_size); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_enqueue_install_dispatch(const alvorkit_interception_install_dispatch_v2* request) {
  return InvokeProfiler(
      [request](InterceptionProfiler* profiler) { return profiler->EnqueueInstallDispatch(request); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_enqueue_generation(
    const alvorkit_interception_generation_v3* request, const uint8_t* il_body, uint32_t il_body_size,
    const alvorkit_interception_relocation_v3* relocations, uint32_t relocation_count, const uint8_t* metadata,
    uint32_t metadata_size, const alvorkit_interception_il_map_v3* il_map, uint32_t il_map_count) {
  return InvokeProfiler([=](InterceptionProfiler* profiler) {
    return profiler->EnqueueGeneration(request, il_body, il_body_size, relocations, relocation_count, metadata,
                                       metadata_size, il_map, il_map_count);
  });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_enqueue_remove(const alvorkit_interception_remove_v2* request) {
  return InvokeProfiler([request](InterceptionProfiler* profiler) { return profiler->EnqueueRemove(request); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_completion(uint64_t request_id, alvorkit_interception_completion_v2* completion) {
  return InvokeProfiler(
      [=](InterceptionProfiler* profiler) { return profiler->GetCompletion(request_id, completion); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_generation_completion(
    uint64_t request_id, alvorkit_interception_generation_completion_v3* completion) {
  return InvokeProfiler(
      [=](InterceptionProfiler* profiler) { return profiler->GetGenerationCompletion(request_id, completion); });
}

extern "C" ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_relocation_result(
    uint64_t request_id, uint32_t relocation_index, alvorkit_interception_relocation_result_v3* result) {
  return InvokeProfiler([=](InterceptionProfiler* profiler) {
    return profiler->GetRelocationResult(request_id, relocation_index, result);
  });
}

#ifdef _WIN32
BOOL WINAPI DllMain(HINSTANCE instance_handle, DWORD reason, LPVOID) {
  if (reason == DLL_PROCESS_ATTACH)
    DisableThreadLibraryCalls(instance_handle);
  return TRUE;
}
#endif

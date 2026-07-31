#pragma once

#include <cstdint>
#include <memory>

#include "ProfilerCallbackBase.hpp"
#include "ProfilerRuntime.hpp"

extern const GUID kAlvorKitInterceptionProfilerClsid;

class InterceptionProfiler final : public ProfilerCallbackBase {
public:
  InterceptionProfiler() = default;
  ~InterceptionProfiler() override;

  static InterceptionProfiler* AcquireInstance();

  HRESULT STDMETHODCALLTYPE Initialize(IUnknown* unknown) override;
  HRESULT STDMETHODCALLTYPE Shutdown() override;
  HRESULT STDMETHODCALLTYPE ModuleLoadFinished(ModuleID module_id, HRESULT status) override;
  HRESULT STDMETHODCALLTYPE ModuleUnloadStarted(ModuleID module_id) override;
  HRESULT STDMETHODCALLTYPE ObjectAllocated(ObjectID object_id, ClassID class_id) override;
  HRESULT STDMETHODCALLTYPE ReJITCompilationStarted(FunctionID function_id, ReJITID rejit_id,
                                                    BOOL safe_to_block) override;
  HRESULT STDMETHODCALLTYPE GetReJITParameters(ModuleID module_id, mdMethodDef method_id,
                                               ICorProfilerFunctionControl* control) override;
  HRESULT STDMETHODCALLTYPE ReJITCompilationFinished(FunctionID function_id, ReJITID rejit_id, HRESULT status,
                                                     BOOL safe_to_block) override;
  HRESULT STDMETHODCALLTYPE ReJITError(ModuleID module_id, mdMethodDef method_id, FunctionID function_id,
                                       HRESULT status) override;

  HRESULT GetCapabilities(alvorkit_interception_capabilities_v2* capabilities);
  HRESULT GetProfilerState(alvorkit_interception_profiler_state_v2* state);
  HRESULT GetLoadedMethodBody(const alvorkit_interception_target_v2* target, uint8_t* body, uint32_t body_capacity,
                              uint32_t* body_size, alvorkit_interception_body_identity_v3* identity);
  HRESULT EnqueueInstall(const alvorkit_interception_install_v2* request, const uint8_t* il_body,
                         uint32_t il_body_size);
  HRESULT EnqueueInstallDispatch(const alvorkit_interception_install_dispatch_v2* request);
  HRESULT EnqueueGeneration(const alvorkit_interception_generation_v3* request, const uint8_t* il_body,
                            uint32_t il_body_size, const alvorkit_interception_relocation_v3* relocations,
                            uint32_t relocation_count, const uint8_t* metadata, uint32_t metadata_size,
                            const alvorkit_interception_il_map_v3* il_map, uint32_t il_map_count);
  HRESULT EnqueueRemove(const alvorkit_interception_remove_v2* request);
  HRESULT GetCompletion(uint64_t request_id, alvorkit_interception_completion_v2* completion);
  HRESULT GetGenerationCompletion(uint64_t request_id, alvorkit_interception_generation_completion_v3* completion);
  HRESULT GetRelocationResult(uint64_t request_id, uint32_t relocation_index,
                              alvorkit_interception_relocation_result_v3* result);
  HRESULT BeginAllocationCapture(const alvorkit_interception_allocation_capture_v3* request);
  HRESULT EndAllocationCapture(alvorkit_interception_allocation_summary_v3* summary);
  HRESULT GetAllocationSample(uint32_t sample_index, alvorkit_interception_allocation_sample_v3* sample,
                              alvorkit_interception_allocation_frame_v3* frames, uint32_t frame_capacity);
  HRESULT ResolveAllocationFrame(const alvorkit_interception_allocation_frame_v3* frame,
                                 alvorkit_interception_resolved_frame_v3* resolved);

private:
  void DetachInstance();

  std::unique_ptr<ProfilerRuntime> runtime_;
};

#pragma once

#include <condition_variable>
#include <cstdint>
#include <deque>
#include <mutex>
#include <thread>

#include "AllocationCapture.hpp"
#include "ModuleCatalog.hpp"
#include "RejitState.hpp"

class ProfilerRuntime {
public:
  ProfilerRuntime(ICorProfilerInfo10* info, bool allocation_capture_enabled);
  ~ProfilerRuntime();

  HRESULT Start();
  void Stop();

  HRESULT ModuleLoadFinished(ModuleID module_id, HRESULT status);
  HRESULT ModuleUnloadStarted(ModuleID module_id);
  HRESULT ReJITCompilationStarted(FunctionID function_id, ReJITID rejit_id);
  HRESULT GetReJITParameters(ModuleID module_id, mdMethodDef method_id, ICorProfilerFunctionControl* control);
  HRESULT ReJITCompilationFinished(FunctionID function_id, ReJITID rejit_id, HRESULT status);
  HRESULT ReJITError(ModuleID module_id, mdMethodDef method_id, HRESULT status);
  void ObjectAllocated(ClassID class_id) noexcept;

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
  HRESULT Enqueue(ProfilerCommand command);
  void WorkerMain();
  void ProcessBodyRead(BodyReadRequest* request);
  void ProcessAllocationFrameResolve(AllocationFrameResolveRequest* request);
  void ProcessInstall(ProfilerCommand command);
  void ProcessRemove(const ProfilerCommand& command);
  HRESULT PrepareExactDispatch(const RuntimeMethodKey& method, const ProfilerCommand& command,
                               std::vector<uint8_t>* body);
  HRESULT PrepareGeneration(const RuntimeMethodKey& method, ProfilerCommand* command);
  HRESULT ValidateBaseline(const RuntimeMethodKey& method, const alvorkit_interception_body_identity_v3& expected);

  ICorProfilerInfo10* info_;
  AllocationCapture allocation_capture_;
  ModuleCatalog modules_;
  RejitState rejit_;
  std::thread worker_;
  // Protects lifecycle and queues; it may precede, but never follow, RejitState.
  std::mutex queue_mutex_;
  std::condition_variable changed_;
  std::condition_variable body_completed_;
  std::condition_variable frame_completed_;
  bool stopping_ = false;
  bool ready_ = false;
  std::deque<ProfilerCommand> commands_;
  // Callers own these requests and wait until completion or shutdown.
  std::deque<BodyReadRequest*> body_requests_;
  std::deque<AllocationFrameResolveRequest*> frame_requests_;
  std::deque<ModuleID> catalog_requests_;
};

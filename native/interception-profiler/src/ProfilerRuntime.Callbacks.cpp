#include "ProfilerRuntime.hpp"

#include <cstring>

HRESULT ProfilerRuntime::ModuleLoadFinished(ModuleID module_id, HRESULT status) {
  if (FAILED(status) || !modules_.IsAllowed(module_id))
    return S_OK;

  status = modules_.Register(module_id);
  if (status == S_FALSE)
    return S_OK;
  if (FAILED(status))
    return status;
  {
    std::lock_guard lock(queue_mutex_);
    if (stopping_)
      return S_OK;
    if (catalog_requests_.size() >= ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS) {
      return HRESULT_FROM_WIN32(56u); // ERROR_TOO_MANY_CMDS
    }
    catalog_requests_.push_back(module_id);
  }
  changed_.notify_one();
  return S_OK;
}

HRESULT ProfilerRuntime::ModuleUnloadStarted(ModuleID module_id) {
  modules_.Unload(module_id);
  rejit_.ModuleUnloaded(module_id);
  return S_OK;
}

HRESULT ProfilerRuntime::ReJITCompilationStarted(FunctionID function_id, ReJITID rejit_id) {
  RuntimeMethodKey method;
  if (modules_.TryGetMethod(function_id, &method))
    rejit_.CompilationStarted(method, rejit_id);
  return S_OK;
}

HRESULT ProfilerRuntime::GetReJITParameters(ModuleID module_id, mdMethodDef method_id,
                                            ICorProfilerFunctionControl* control) {
  RuntimeMethodKey method;
  if (!modules_.TryGetMethod(module_id, method_id, &method))
    return S_OK;

  PendingRejitSnapshot snapshot;
  if (!rejit_.TryGetParameters(method, &snapshot))
    return S_OK;
  if (snapshot.operation == ALVORKIT_INTERCEPTION_OPERATION_REMOVE)
    return S_OK;
  if (snapshot.prepared == nullptr || snapshot.prepared->body.empty()) {
    rejit_.Fail(snapshot.request_id, E_UNEXPECTED);
    return E_UNEXPECTED;
  }

  IMethodMalloc* allocator = nullptr;
  HRESULT status = info_->GetILFunctionBodyAllocator(module_id, &allocator);
  if (FAILED(status)) {
    rejit_.Fail(snapshot.request_id, status);
    return status;
  }

  const PreparedRejit& prepared = *snapshot.prepared;
  BYTE* replacement = static_cast<BYTE*>(allocator->Alloc(static_cast<ULONG>(prepared.body.size())));
  if (replacement == nullptr) {
    allocator->Release();
    rejit_.Fail(snapshot.request_id, E_OUTOFMEMORY);
    return E_OUTOFMEMORY;
  }

  std::memcpy(replacement, prepared.body.data(), prepared.body.size());
  status = control->SetILFunctionBody(static_cast<ULONG>(prepared.body.size()), replacement);
  if (SUCCEEDED(status) && !prepared.il_map.empty()) {
    status = control->SetILInstrumentedCodeMap(static_cast<ULONG>(prepared.il_map.size()),
                                               const_cast<COR_IL_MAP*>(prepared.il_map.data()));
    if (FAILED(status)) {
      rejit_.FailGeneration(snapshot.request_id, status, ALVORKIT_INTERCEPTION_FAILURE_IL_MAP);
    }
  }
  if (SUCCEEDED(status) && (prepared.flags & ALVORKIT_INTERCEPTION_PATCH_FLAG_DISABLE_INLINING) != 0) {
    status = control->SetCodegenFlags(COR_PRF_CODEGEN_DISABLE_INLINING);
  }
  allocator->Release();
  if (FAILED(status))
    rejit_.Fail(snapshot.request_id, status);
  return status;
}

HRESULT ProfilerRuntime::ReJITCompilationFinished(FunctionID function_id, ReJITID rejit_id, HRESULT status) {
  RuntimeMethodKey method;
  if (modules_.TryGetMethod(function_id, &method))
    rejit_.CompilationFinished(method, rejit_id, status);
  return S_OK;
}

HRESULT ProfilerRuntime::ReJITError(ModuleID module_id, mdMethodDef method_id, HRESULT status) {
  RuntimeMethodKey method;
  if (modules_.TryGetMethod(module_id, method_id, &method))
    rejit_.RejitError(method, status);
  return S_OK;
}

#include "ProfilerCallbackBase.hpp"

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::InitializeForAttach(IUnknown*, void*, UINT) {
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ProfilerAttachComplete() {
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ProfilerDetachSucceeded() {
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ReJITCompilationStarted(FunctionID, ReJITID, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::GetReJITParameters(ModuleID, mdMethodDef,
                                                                   ICorProfilerFunctionControl*) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ReJITCompilationFinished(FunctionID, ReJITID, HRESULT, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ReJITError(ModuleID, mdMethodDef, FunctionID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::MovedReferences2(ULONG, ObjectID[], ObjectID[], SIZE_T[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::SurvivingReferences2(ULONG, ObjectID[], SIZE_T[]) {
  return S_OK;
}

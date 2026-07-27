#include "ProfilerCallbackBase.hpp"

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::MovedReferences(ULONG, ObjectID[], ObjectID[], ULONG[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ObjectAllocated(ObjectID, ClassID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ObjectsAllocatedByClass(ULONG, ClassID[], ULONG[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ObjectReferences(ObjectID, ClassID, ULONG, ObjectID[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RootReferences(ULONG, ObjectID[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionThrown(ObjectID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionSearchFunctionEnter(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionSearchFunctionLeave() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionSearchFilterEnter(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionSearchFilterLeave() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionSearchCatcherFound(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionOSHandlerEnter(UINT_PTR) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionOSHandlerLeave(UINT_PTR) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionUnwindFunctionEnter(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionUnwindFunctionLeave() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionUnwindFinallyEnter(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionUnwindFinallyLeave() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionCatcherEnter(FunctionID, ObjectID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionCatcherLeave() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::COMClassicVTableCreated(ClassID, REFGUID, void*, ULONG) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::COMClassicVTableDestroyed(ClassID, REFGUID, void*) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionCLRCatcherFound() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ExceptionCLRCatcherExecute() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ThreadNameChanged(ThreadID, ULONG, WCHAR[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::GarbageCollectionStarted(int, BOOL[], COR_PRF_GC_REASON) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::SurvivingReferences(ULONG, ObjectID[], ULONG[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::GarbageCollectionFinished() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::FinalizeableObjectQueued(DWORD, ObjectID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RootReferences2(ULONG, ObjectID[], COR_PRF_GC_ROOT_KIND[],
                                                                COR_PRF_GC_ROOT_FLAGS[], UINT_PTR[]) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::HandleCreated(GCHandleID, ObjectID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::HandleDestroyed(GCHandleID) {
  return S_OK;
}

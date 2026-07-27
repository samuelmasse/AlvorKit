#include "ProfilerCallbackBase.hpp"

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::Initialize(IUnknown*) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::Shutdown() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AppDomainCreationStarted(AppDomainID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AppDomainCreationFinished(AppDomainID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AppDomainShutdownStarted(AppDomainID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AppDomainShutdownFinished(AppDomainID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AssemblyLoadStarted(AssemblyID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AssemblyLoadFinished(AssemblyID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AssemblyUnloadStarted(AssemblyID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::AssemblyUnloadFinished(AssemblyID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ModuleLoadStarted(ModuleID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ModuleLoadFinished(ModuleID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ModuleUnloadStarted(ModuleID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ModuleUnloadFinished(ModuleID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ModuleAttachedToAssembly(ModuleID, AssemblyID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ClassLoadStarted(ClassID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ClassLoadFinished(ClassID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ClassUnloadStarted(ClassID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ClassUnloadFinished(ClassID, HRESULT) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::FunctionUnloadStarted(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITCompilationStarted(FunctionID, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITCompilationFinished(FunctionID, HRESULT, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITCachedFunctionSearchStarted(FunctionID, BOOL*) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITCachedFunctionSearchFinished(FunctionID, COR_PRF_JIT_CACHE) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITFunctionPitched(FunctionID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::JITInlining(FunctionID, FunctionID, BOOL*) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ThreadCreated(ThreadID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ThreadDestroyed(ThreadID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ThreadAssignedToOSThread(ThreadID, DWORD) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingClientInvocationStarted() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingClientSendingMessage(GUID*, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingClientReceivingReply(GUID*, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingClientInvocationFinished() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingServerReceivingMessage(GUID*, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingServerInvocationStarted() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingServerInvocationReturned() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RemotingServerSendingReply(GUID*, BOOL) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::UnmanagedToManagedTransition(FunctionID, COR_PRF_TRANSITION_REASON) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::ManagedToUnmanagedTransition(FunctionID, COR_PRF_TRANSITION_REASON) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeSuspendFinished() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeSuspendAborted() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeResumeStarted() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeResumeFinished() {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeThreadSuspended(ThreadID) {
  return S_OK;
}

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::RuntimeThreadResumed(ThreadID) {
  return S_OK;
}

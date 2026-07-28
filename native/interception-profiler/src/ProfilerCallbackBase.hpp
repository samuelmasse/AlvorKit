#pragma once

#include <atomic>

#include "CoreClrHeaders.hpp"

class ProfilerCallbackBase : public ICorProfilerCallback4 {
public:
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** value) override;
  ULONG STDMETHODCALLTYPE AddRef() override;
  ULONG STDMETHODCALLTYPE Release() override;

  HRESULT STDMETHODCALLTYPE Initialize(IUnknown*) override;
  HRESULT STDMETHODCALLTYPE Shutdown() override;
  HRESULT STDMETHODCALLTYPE AppDomainCreationStarted(AppDomainID) override;
  HRESULT STDMETHODCALLTYPE AppDomainCreationFinished(AppDomainID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted(AppDomainID) override;
  HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished(AppDomainID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE AssemblyLoadStarted(AssemblyID) override;
  HRESULT STDMETHODCALLTYPE AssemblyLoadFinished(AssemblyID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted(AssemblyID) override;
  HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished(AssemblyID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE ModuleLoadStarted(ModuleID) override;
  HRESULT STDMETHODCALLTYPE ModuleLoadFinished(ModuleID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE ModuleUnloadStarted(ModuleID) override;
  HRESULT STDMETHODCALLTYPE ModuleUnloadFinished(ModuleID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly(ModuleID, AssemblyID) override;
  HRESULT STDMETHODCALLTYPE ClassLoadStarted(ClassID) override;
  HRESULT STDMETHODCALLTYPE ClassLoadFinished(ClassID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE ClassUnloadStarted(ClassID) override;
  HRESULT STDMETHODCALLTYPE ClassUnloadFinished(ClassID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE FunctionUnloadStarted(FunctionID) override;
  HRESULT STDMETHODCALLTYPE JITCompilationStarted(FunctionID, BOOL) override;
  HRESULT STDMETHODCALLTYPE JITCompilationFinished(FunctionID, HRESULT, BOOL) override;
  HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted(FunctionID, BOOL*) override;
  HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished(FunctionID, COR_PRF_JIT_CACHE) override;
  HRESULT STDMETHODCALLTYPE JITFunctionPitched(FunctionID) override;
  HRESULT STDMETHODCALLTYPE JITInlining(FunctionID, FunctionID, BOOL*) override;
  HRESULT STDMETHODCALLTYPE ThreadCreated(ThreadID) override;
  HRESULT STDMETHODCALLTYPE ThreadDestroyed(ThreadID) override;
  HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(ThreadID, DWORD) override;
  HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted() override;
  HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage(GUID*, BOOL) override;
  HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply(GUID*, BOOL) override;
  HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished() override;
  HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage(GUID*, BOOL) override;
  HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted() override;
  HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned() override;
  HRESULT STDMETHODCALLTYPE RemotingServerSendingReply(GUID*, BOOL) override;
  HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition(FunctionID, COR_PRF_TRANSITION_REASON) override;
  HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition(FunctionID, COR_PRF_TRANSITION_REASON) override;
  HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON) override;
  HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished() override;
  HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted() override;
  HRESULT STDMETHODCALLTYPE RuntimeResumeStarted() override;
  HRESULT STDMETHODCALLTYPE RuntimeResumeFinished() override;
  HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended(ThreadID) override;
  HRESULT STDMETHODCALLTYPE RuntimeThreadResumed(ThreadID) override;

  HRESULT STDMETHODCALLTYPE MovedReferences(ULONG, ObjectID[], ObjectID[], ULONG[]) override;
  HRESULT STDMETHODCALLTYPE ObjectAllocated(ObjectID, ClassID) override;
  HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass(ULONG, ClassID[], ULONG[]) override;
  HRESULT STDMETHODCALLTYPE ObjectReferences(ObjectID, ClassID, ULONG, ObjectID[]) override;
  HRESULT STDMETHODCALLTYPE RootReferences(ULONG, ObjectID[]) override;
  HRESULT STDMETHODCALLTYPE ExceptionThrown(ObjectID) override;
  HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter(FunctionID) override;
  HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave() override;
  HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter(FunctionID) override;
  HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave() override;
  HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound(FunctionID) override;
  HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter(UINT_PTR) override;
  HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave(UINT_PTR) override;
  HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter(FunctionID) override;
  HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave() override;
  HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter(FunctionID) override;
  HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave() override;
  HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter(FunctionID, ObjectID) override;
  HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave() override;
  HRESULT STDMETHODCALLTYPE COMClassicVTableCreated(ClassID, REFGUID, void*, ULONG) override;
  HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed(ClassID, REFGUID, void*) override;
  HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound() override;
  HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute() override;
  HRESULT STDMETHODCALLTYPE ThreadNameChanged(ThreadID, ULONG, WCHAR[]) override;
  HRESULT STDMETHODCALLTYPE GarbageCollectionStarted(int, BOOL[], COR_PRF_GC_REASON) override;
  HRESULT STDMETHODCALLTYPE SurvivingReferences(ULONG, ObjectID[], ULONG[]) override;
  HRESULT STDMETHODCALLTYPE GarbageCollectionFinished() override;
  HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued(DWORD, ObjectID) override;
  HRESULT STDMETHODCALLTYPE RootReferences2(ULONG, ObjectID[], COR_PRF_GC_ROOT_KIND[], COR_PRF_GC_ROOT_FLAGS[],
                                            UINT_PTR[]) override;
  HRESULT STDMETHODCALLTYPE HandleCreated(GCHandleID, ObjectID) override;
  HRESULT STDMETHODCALLTYPE HandleDestroyed(GCHandleID) override;

  HRESULT STDMETHODCALLTYPE InitializeForAttach(IUnknown*, void*, UINT) override;
  HRESULT STDMETHODCALLTYPE ProfilerAttachComplete() override;
  HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded() override;
  HRESULT STDMETHODCALLTYPE ReJITCompilationStarted(FunctionID, ReJITID, BOOL) override;
  HRESULT STDMETHODCALLTYPE GetReJITParameters(ModuleID, mdMethodDef, ICorProfilerFunctionControl*) override;
  HRESULT STDMETHODCALLTYPE ReJITCompilationFinished(FunctionID, ReJITID, HRESULT, BOOL) override;
  HRESULT STDMETHODCALLTYPE ReJITError(ModuleID, mdMethodDef, FunctionID, HRESULT) override;
  HRESULT STDMETHODCALLTYPE MovedReferences2(ULONG, ObjectID[], ObjectID[], SIZE_T[]) override;
  HRESULT STDMETHODCALLTYPE SurvivingReferences2(ULONG, ObjectID[], SIZE_T[]) override;

protected:
  virtual ~ProfilerCallbackBase() = default;

private:
  std::atomic<ULONG> reference_count_{0};
};

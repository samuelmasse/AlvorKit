#include "ProfilerCallbackBase.hpp"

HRESULT STDMETHODCALLTYPE ProfilerCallbackBase::QueryInterface(REFIID iid, void** value) {
  if (value == nullptr) {
    return E_POINTER;
  }

  if (iid == IID_IUnknown || iid == __uuidof(ICorProfilerCallback) || iid == __uuidof(ICorProfilerCallback2) ||
      iid == __uuidof(ICorProfilerCallback3) || iid == __uuidof(ICorProfilerCallback4)) {
    *value = static_cast<ICorProfilerCallback4*>(this);
    AddRef();
    return S_OK;
  }

  *value = nullptr;
  return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE ProfilerCallbackBase::AddRef() {
  return ++reference_count_;
}

ULONG STDMETHODCALLTYPE ProfilerCallbackBase::Release() {
  ULONG count = --reference_count_;
  if (count == 0) {
    delete this;
  }

  return count;
}

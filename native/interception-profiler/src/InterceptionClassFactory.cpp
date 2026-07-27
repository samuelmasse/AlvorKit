#include "InterceptionClassFactory.hpp"
#include "InterceptionProfiler.hpp"

#include <new>

HRESULT STDMETHODCALLTYPE InterceptionClassFactory::QueryInterface(REFIID iid, void** value) {
  if (value == nullptr) {
    return E_POINTER;
  }

  if (iid == IID_IUnknown || iid == IID_IClassFactory) {
    *value = static_cast<IClassFactory*>(this);
    AddRef();
    return S_OK;
  }

  *value = nullptr;
  return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE InterceptionClassFactory::AddRef() {
  return ++reference_count_;
}

ULONG STDMETHODCALLTYPE InterceptionClassFactory::Release() {
  ULONG count = --reference_count_;
  if (count == 0) {
    delete this;
  }

  return count;
}

HRESULT STDMETHODCALLTYPE InterceptionClassFactory::CreateInstance(IUnknown* outer, REFIID iid, void** value) {
  if (value == nullptr) {
    return E_POINTER;
  }
  *value = nullptr;

  if (outer != nullptr) {
    return CLASS_E_NOAGGREGATION;
  }

  InterceptionProfiler* profiler = new (std::nothrow) InterceptionProfiler();
  if (profiler == nullptr) {
    *value = nullptr;
    return E_OUTOFMEMORY;
  }
  HRESULT status = profiler->QueryInterface(iid, value);
  if (FAILED(status)) {
    delete profiler;
  }

  return status;
}

HRESULT STDMETHODCALLTYPE InterceptionClassFactory::LockServer(BOOL) {
  return S_OK;
}

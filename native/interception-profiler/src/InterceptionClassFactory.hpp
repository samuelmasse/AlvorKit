#pragma once

#include <atomic>
#include <unknwn.h>

class InterceptionClassFactory final : public IClassFactory {
public:
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** value) override;
  ULONG STDMETHODCALLTYPE AddRef() override;
  ULONG STDMETHODCALLTYPE Release() override;
  HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer, REFIID iid, void** value) override;
  HRESULT STDMETHODCALLTYPE LockServer(BOOL lock) override;

private:
  std::atomic<ULONG> reference_count_{0};
};

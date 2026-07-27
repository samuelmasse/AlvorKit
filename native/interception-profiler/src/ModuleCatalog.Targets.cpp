#include "ModuleCatalog.hpp"

#include "InterceptionIdentity.hpp"
#include "MethodSignature.hpp"

#include <cstdint>
#include <iterator>

HRESULT ModuleCatalog::ResolveTarget(const alvorkit_interception_target_v2& target, RuntimeMethodKey* method) {
  ModuleID module_id = 0;
  HRESULT status = FindModule(ToGuid(target.module_mvid), &module_id);
  if (FAILED(status))
    return status;
  if (!IsAllowed(module_id))
    return E_ACCESSDENIED;

  uint64_t signature_hash = 0;
  status = GetSignatureHash(module_id, static_cast<mdMethodDef>(target.method_token), &signature_hash);
  if (FAILED(status))
    return status;
  if (signature_hash != target.signature_hash)
    return COR_E_BADIMAGEFORMAT;

  std::lock_guard lock(mutex_);
  const auto epoch = module_epochs_.find(module_id);
  if (epoch == module_epochs_.end())
    return CORPROF_E_DATAINCOMPLETE;
  *method = {module_id, static_cast<mdMethodDef>(target.method_token), epoch->second};
  return S_OK;
}

HRESULT ModuleCatalog::FindModule(const GUID& mvid, ModuleID* module_id) {
  ICorProfilerModuleEnum* modules = nullptr;
  HRESULT status = info_->EnumModules(&modules);
  if (FAILED(status))
    return status;

  ModuleID values[32]{};
  ULONG fetched = 0;
  while (SUCCEEDED(status = modules->Next(static_cast<ULONG>(std::size(values)), values, &fetched)) && fetched != 0) {
    for (ULONG index = 0; index < fetched; ++index) {
      IMetaDataImport* metadata = nullptr;
      const HRESULT metadata_status =
          info_->GetModuleMetaData(values[index], ofRead, IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&metadata));
      if (FAILED(metadata_status))
        continue;

      GUID candidate{};
      const HRESULT scope_status = metadata->GetScopeProps(nullptr, 0, nullptr, &candidate);
      metadata->Release();
      if (SUCCEEDED(scope_status) && IsEqualGUID(candidate, mvid)) {
        *module_id = values[index];
        modules->Release();
        return S_OK;
      }
    }
  }

  modules->Release();
  return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
}

HRESULT ModuleCatalog::GetSignatureHash(ModuleID module_id, mdMethodDef method_id, uint64_t* signature_hash) {
  IMetaDataImport* metadata = nullptr;
  HRESULT status =
      info_->GetModuleMetaData(module_id, ofRead, IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&metadata));
  if (FAILED(status))
    return status;

  PCCOR_SIGNATURE signature = nullptr;
  ULONG signature_size = 0;
  status = metadata->GetMethodProps(method_id, nullptr, nullptr, 0, nullptr, nullptr, &signature, &signature_size,
                                    nullptr, nullptr);
  if (SUCCEEDED(status)) {
    *signature_hash = ComputeSignatureHash(signature, signature_size);
  }
  metadata->Release();
  return status;
}

bool ModuleCatalog::TryGetMethod(FunctionID function_id, RuntimeMethodKey* method) {
  ClassID class_id = 0;
  if (FAILED(info_->GetFunctionInfo(function_id, &class_id, &method->module_id, &method->method_id))) {
    return false;
  }

  std::lock_guard lock(mutex_);
  const auto epoch = module_epochs_.find(method->module_id);
  if (epoch == module_epochs_.end())
    return false;
  method->module_epoch = epoch->second;
  return true;
}

bool ModuleCatalog::TryGetMethod(ModuleID module_id, mdMethodDef method_id, RuntimeMethodKey* method) const {
  std::lock_guard lock(mutex_);
  const auto epoch = module_epochs_.find(module_id);
  if (epoch == module_epochs_.end())
    return false;
  *method = {module_id, method_id, epoch->second};
  return true;
}

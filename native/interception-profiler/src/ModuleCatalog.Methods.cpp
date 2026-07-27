#include "ModuleCatalog.hpp"

#include "MethodSignature.hpp"

#include <cstdint>
#include <iterator>
#include <utility>

HRESULT ModuleCatalog::CatalogModule(ModuleID module_id) {
  uint64_t module_epoch = 0;
  {
    std::lock_guard lock(mutex_);
    const auto epoch = module_epochs_.find(module_id);
    if (epoch == module_epochs_.end())
      return CORPROF_E_DATAINCOMPLETE;
    module_epoch = epoch->second;
  }

  IMetaDataImport* metadata = nullptr;
  HRESULT status = info_->GetModuleMetaData(module_id, ofRead | ofWrite, IID_IMetaDataImport,
                                            reinterpret_cast<IUnknown**>(&metadata));
  if (FAILED(status))
    return status;

  IMetaDataEmit* emitter = nullptr;
  status = metadata->QueryInterface(IID_IMetaDataEmit, reinterpret_cast<void**>(&emitter));
  if (FAILED(status)) {
    metadata->Release();
    return status;
  }

  const COR_SIGNATURE resolver_signature[] = {IMAGE_CEE_CS_CALLCONV_DEFAULT, 2, ELEMENT_TYPE_I, ELEMENT_TYPE_U8,
                                              ELEMENT_TYPE_OBJECT};
  mdSignature resolver_token = mdSignatureNil;
  status =
      emitter->GetTokenFromSig(resolver_signature, static_cast<ULONG>(std::size(resolver_signature)), &resolver_token);
  if (FAILED(status)) {
    emitter->Release();
    metadata->Release();
    return status;
  }

  std::vector<std::pair<RuntimeMethodKey, MethodMetadata>> catalog;
  HCORENUM type_enum = nullptr;
  mdTypeDef types[64]{};
  ULONG type_count = 0;
  while (SUCCEEDED(status =
                       metadata->EnumTypeDefs(&type_enum, types, static_cast<ULONG>(std::size(types)), &type_count)) &&
         type_count != 0) {
    for (ULONG type_index = 0; type_index < type_count; ++type_index) {
      HCORENUM method_enum = nullptr;
      mdMethodDef methods[128]{};
      ULONG method_count = 0;
      HRESULT method_status = S_OK;
      while (SUCCEEDED(method_status = metadata->EnumMethods(&method_enum, types[type_index], methods,
                                                             static_cast<ULONG>(std::size(methods)), &method_count)) &&
             method_count != 0) {
        for (ULONG method_index = 0; method_index < method_count; ++method_index) {
          DWORD attributes = 0;
          DWORD implementation = 0;
          PCCOR_SIGNATURE signature = nullptr;
          ULONG signature_size = 0;
          method_status = metadata->GetMethodProps(methods[method_index], nullptr, nullptr, 0, nullptr, &attributes,
                                                   &signature, &signature_size, nullptr, &implementation);
          if (FAILED(method_status) || signature == nullptr || signature_size < 2 || IsMdAbstract(attributes) ||
              IsMdPinvokeImpl(attributes) || !IsMiIL(implementation) ||
              (signature[0] & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0) {
            continue;
          }

          ULONG parameter_count = 0;
          if (CorSigUncompressData(signature + 1, &parameter_count) == static_cast<ULONG>(-1)) {
            continue;
          }

          const bool is_static = IsMdStatic(attributes) != 0;
          std::vector<COR_SIGNATURE> call_signature;
          method_status =
              BuildExactCallSignature(signature, signature_size, types[type_index], is_static, &call_signature);
          if (FAILED(method_status))
            continue;

          mdSignature call_token = mdSignatureNil;
          method_status =
              emitter->GetTokenFromSig(call_signature.data(), static_cast<ULONG>(call_signature.size()), &call_token);
          if (FAILED(method_status))
            continue;

          MethodMetadata value;
          value.call_signature = call_token;
          value.resolver_signature = resolver_token;
          value.parameter_count = parameter_count;
          value.is_static = is_static;
          catalog.emplace_back(RuntimeMethodKey{module_id, methods[method_index], module_epoch}, value);
        }
      }
      if (method_enum != nullptr)
        metadata->CloseEnum(method_enum);
    }
  }

  if (type_enum != nullptr)
    metadata->CloseEnum(type_enum);
  emitter->Release();
  metadata->Release();
  if (FAILED(status))
    return status;

  std::lock_guard lock(mutex_);
  for (const auto& item : catalog)
    methods_[item.first] = item.second;
  return S_OK;
}

#include "AllocationCapture.hpp"

#include <cstdint>
#include <cstring>
#include <limits>
#include <vector>

namespace {
bool TryGetNativeOffset(ICorProfilerInfo10* info, FunctionID function_id, ReJITID rejit_id,
                        uint64_t instruction_pointer, uint32_t* native_offset) {
  ULONG32 code_info_count = 0;
  HRESULT status = info->GetCodeInfo3(function_id, rejit_id, 0, &code_info_count, nullptr);
  if (FAILED(status) || code_info_count == 0)
    return false;

  std::vector<COR_PRF_CODE_INFO> code_infos(code_info_count);
  status = info->GetCodeInfo3(function_id, rejit_id, code_info_count, &code_info_count, code_infos.data());
  if (FAILED(status))
    return false;

  uint64_t prior_size = 0;
  for (const COR_PRF_CODE_INFO& code_info : code_infos) {
    const uint64_t start = static_cast<uint64_t>(code_info.startAddress);
    const uint64_t end = start + code_info.size;
    if (instruction_pointer >= start && instruction_pointer < end) {
      const uint64_t offset = prior_size + instruction_pointer - start;
      if (offset > std::numeric_limits<uint32_t>::max())
        return false;
      *native_offset = static_cast<uint32_t>(offset);
      return true;
    }
    prior_size += code_info.size;
  }
  return false;
}

bool TryGetIlOffset(ICorProfilerInfo10* info, FunctionID function_id, ReJITID rejit_id, uint32_t native_offset,
                    uint32_t* il_offset) {
  ULONG32 map_count = 0;
  HRESULT status = info->GetILToNativeMapping2(function_id, rejit_id, 0, &map_count, nullptr);
  if (FAILED(status) || map_count == 0)
    return false;

  std::vector<COR_DEBUG_IL_TO_NATIVE_MAP> mappings(map_count);
  status = info->GetILToNativeMapping2(function_id, rejit_id, map_count, &map_count, mappings.data());
  if (FAILED(status))
    return false;

  for (const COR_DEBUG_IL_TO_NATIVE_MAP& mapping : mappings) {
    if (mapping.ilOffset > static_cast<ULONG32>(std::numeric_limits<int32_t>::max()))
      continue;
    if (native_offset >= mapping.nativeStartOffset && native_offset < mapping.nativeEndOffset) {
      *il_offset = mapping.ilOffset;
      return true;
    }
  }
  return false;
}

HRESULT GetMethodIdentity(ICorProfilerInfo10* info, FunctionID function_id, ModuleID* module_id,
                          mdToken* method_token) {
  ClassID class_id = 0;
  ULONG32 type_argument_count = 0;
  return info->GetFunctionInfo2(function_id, 0, &class_id, module_id, method_token, 0, &type_argument_count, nullptr);
}

HRESULT GetModuleMvid(ICorProfilerInfo10* info, ModuleID module_id, alvorkit_guid_v2* module_mvid) {
  IMetaDataImport* metadata = nullptr;
  HRESULT status =
      info->GetModuleMetaData(module_id, ofRead, IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&metadata));
  if (FAILED(status))
    return status;

  GUID mvid{};
  status = metadata->GetScopeProps(nullptr, 0, nullptr, &mvid);
  metadata->Release();
  if (FAILED(status))
    return status;

  module_mvid->data1 = mvid.Data1;
  module_mvid->data2 = mvid.Data2;
  module_mvid->data3 = mvid.Data3;
  std::memcpy(module_mvid->data4, mvid.Data4, sizeof(module_mvid->data4));
  return S_OK;
}
} // namespace

HRESULT AllocationCapture::ResolveFrame(const alvorkit_interception_allocation_frame_v3* frame,
                                        alvorkit_interception_resolved_frame_v3* resolved) {
  if (!enabled_)
    return E_NOTIMPL;
  if (frame == nullptr || resolved == nullptr || frame->instruction_pointer == 0)
    return E_INVALIDARG;

  FunctionID function_id = static_cast<FunctionID>(frame->function_id);
  FunctionID ip_function_id = 0;
  ReJITID rejit_id = 0;
  HRESULT status =
      info_->GetFunctionFromIP3(reinterpret_cast<LPCBYTE>(frame->instruction_pointer), &ip_function_id, &rejit_id);
  const bool code_version_resolved = SUCCEEDED(status);
  if (code_version_resolved)
    function_id = ip_function_id;

  ModuleID module_id = 0;
  mdToken method_token = mdTokenNil;
  status = GetMethodIdentity(info_, function_id, &module_id, &method_token);
  if (FAILED(status))
    return status;

  *resolved = {};
  resolved->size = sizeof(*resolved);
  resolved->abi_version = ALVORKIT_INTERCEPTION_ABI_VERSION;
  status = GetModuleMvid(info_, module_id, &resolved->module_mvid);
  if (FAILED(status))
    return status;
  resolved->method_token = static_cast<int32_t>(method_token);

  uint32_t native_offset = 0;
  uint32_t il_offset = 0;
  if (code_version_resolved &&
      TryGetNativeOffset(info_, function_id, rejit_id, frame->instruction_pointer, &native_offset) &&
      TryGetIlOffset(info_, function_id, rejit_id, native_offset, &il_offset)) {
    resolved->il_offset = il_offset;
    resolved->has_il_offset = 1;
  }
  return S_OK;
}

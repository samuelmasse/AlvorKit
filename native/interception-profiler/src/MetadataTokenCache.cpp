#include "MetadataTokenCache.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <string>
#include <utility>

namespace {
bool DecodeUtf8(const uint8_t* bytes, size_t size, std::basic_string<WCHAR>* result) {
  result->clear();
  for (size_t index = 0; index < size;) {
    uint32_t value = 0;
    size_t trailing = 0;
    const uint8_t first = bytes[index++];
    if (first < 0x80u) {
      value = first;
    } else if ((first & 0xE0u) == 0xC0u) {
      value = first & 0x1Fu;
      trailing = 1;
    } else if ((first & 0xF0u) == 0xE0u) {
      value = first & 0x0Fu;
      trailing = 2;
    } else if ((first & 0xF8u) == 0xF0u) {
      value = first & 0x07u;
      trailing = 3;
    } else {
      return false;
    }
    if (trailing > size - index)
      return false;
    for (size_t count = 0; count < trailing; ++count) {
      const uint8_t next = bytes[index++];
      if ((next & 0xC0u) != 0x80u)
        return false;
      value = (value << 6u) | (next & 0x3Fu);
    }
    if (value == 0 || value > 0x10FFFFu || (value >= 0xD800u && value <= 0xDFFFu) || (trailing == 1 && value < 0x80u) ||
        (trailing == 2 && value < 0x800u) || (trailing == 3 && value < 0x10000u)) {
      return false;
    }
    if (value <= 0xFFFFu) {
      result->push_back(static_cast<WCHAR>(value));
    } else {
      value -= 0x10000u;
      result->push_back(static_cast<WCHAR>(0xD800u + (value >> 10u)));
      result->push_back(static_cast<WCHAR>(0xDC00u + (value & 0x3FFu)));
    }
  }
  return !result->empty();
}
} // namespace

HRESULT MetadataTokenCache::Resolve(const RuntimeMethodKey& method,
                                    const alvorkit_interception_relocation_v3& relocation,
                                    const std::vector<uint8_t>& metadata, mdToken* token) {
  const auto signature_begin = metadata.begin() + relocation.signature_offset;
  std::vector<uint8_t> signature(signature_begin, signature_begin + relocation.signature_size);
  std::vector<uint8_t> name;
  if (relocation.name_size != 0) {
    const auto name_begin = metadata.begin() + relocation.name_offset;
    name.assign(name_begin, name_begin + relocation.name_size);
  }

  const auto cached = std::find_if(records_.begin(), records_.end(), [&](const MetadataTokenRecord& record) {
    return record.module_id == method.module_id && record.module_epoch == method.module_epoch &&
           record.kind == relocation.kind && record.parent_token == static_cast<mdToken>(relocation.parent_token) &&
           record.name == name && record.signature == signature;
  });
  if (cached != records_.end()) {
    *token = cached->token;
    return S_OK;
  }

  IMetaDataEmit* emitter = nullptr;
  HRESULT status = info_->GetModuleMetaData(method.module_id, ofRead | ofWrite, IID_IMetaDataEmit,
                                            reinterpret_cast<IUnknown**>(&emitter));
  if (FAILED(status))
    return status;

  mdToken created = mdTokenNil;
  switch (relocation.kind) {
  case ALVORKIT_INTERCEPTION_RELOCATION_STANDALONE_SIGNATURE:
    status = emitter->GetTokenFromSig(signature.data(), static_cast<ULONG>(signature.size()),
                                      reinterpret_cast<mdSignature*>(&created));
    break;
  case ALVORKIT_INTERCEPTION_RELOCATION_TYPE_SPEC:
    status = emitter->GetTokenFromTypeSpec(signature.data(), static_cast<ULONG>(signature.size()),
                                           reinterpret_cast<mdTypeSpec*>(&created));
    break;
  case ALVORKIT_INTERCEPTION_RELOCATION_MEMBER_REF: {
    std::basic_string<WCHAR> decoded_name;
    if (!DecodeUtf8(name.data(), name.size(), &decoded_name)) {
      status = COR_E_BADIMAGEFORMAT;
      break;
    }
    status = emitter->DefineMemberRef(relocation.parent_token, decoded_name.c_str(), signature.data(),
                                      static_cast<ULONG>(signature.size()), reinterpret_cast<mdMemberRef*>(&created));
    break;
  }
  case ALVORKIT_INTERCEPTION_RELOCATION_METHOD_SPEC: {
    IMetaDataEmit2* emitter2 = nullptr;
    status = emitter->QueryInterface(IID_IMetaDataEmit2, reinterpret_cast<void**>(&emitter2));
    if (SUCCEEDED(status)) {
      status =
          emitter2->DefineMethodSpec(relocation.parent_token, signature.data(), static_cast<ULONG>(signature.size()),
                                     reinterpret_cast<mdMethodSpec*>(&created));
      emitter2->Release();
    }
    break;
  }
  default:
    status = E_INVALIDARG;
    break;
  }
  emitter->Release();
  if (FAILED(status))
    return status;

  MetadataTokenRecord record;
  record.module_id = method.module_id;
  record.module_epoch = method.module_epoch;
  record.kind = relocation.kind;
  record.parent_token = static_cast<mdToken>(relocation.parent_token);
  record.name = std::move(name);
  record.signature = std::move(signature);
  record.token = created;
  records_.push_back(std::move(record));
  *token = created;
  return S_OK;
}

void MetadataTokenCache::RemoveModule(ModuleID module_id) {
  std::erase_if(records_, [module_id](const MetadataTokenRecord& record) { return record.module_id == module_id; });
}

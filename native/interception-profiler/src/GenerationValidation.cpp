#include "GenerationValidation.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>

namespace {
bool IsBlobRangeValid(uint32_t offset, uint32_t size, size_t available) {
  return size != 0 && offset <= available && size <= available - offset;
}

bool IsRelocationKind(uint32_t kind) {
  return kind >= ALVORKIT_INTERCEPTION_RELOCATION_STANDALONE_SIGNATURE &&
         kind <= ALVORKIT_INTERCEPTION_RELOCATION_METHOD_SPEC;
}

bool IsParentTokenValid(uint32_t kind, mdToken token) {
  const uint32_t token_type = TypeFromToken(token);
  if (kind == ALVORKIT_INTERCEPTION_RELOCATION_MEMBER_REF) {
    return token_type == mdtTypeDef || token_type == mdtTypeRef || token_type == mdtTypeSpec ||
           token_type == mdtModuleRef || token_type == mdtMethodDef;
  }
  if (kind == ALVORKIT_INTERCEPTION_RELOCATION_METHOD_SPEC) {
    return token_type == mdtMethodDef || token_type == mdtMemberRef;
  }
  return token == mdTokenNil;
}
} // namespace

HRESULT ValidateGenerationRelocations(const std::vector<alvorkit_interception_relocation_v3>& relocations,
                                      const std::vector<uint8_t>& metadata, const std::vector<uint8_t>& body,
                                      uint32_t* failure_index) {
  std::vector<uint32_t> offsets;
  offsets.reserve(relocations.size());
  for (uint32_t index = 0; index < relocations.size(); ++index) {
    const auto& relocation = relocations[index];
    const bool name_required = relocation.kind == ALVORKIT_INTERCEPTION_RELOCATION_MEMBER_REF;
    const bool valid =
        relocation.reserved == 0 && IsRelocationKind(relocation.kind) && relocation.body_offset <= body.size() &&
        body.size() - relocation.body_offset >= 4u &&
        IsBlobRangeValid(relocation.signature_offset, relocation.signature_size, metadata.size()) &&
        IsParentTokenValid(relocation.kind, relocation.parent_token) &&
        (name_required == IsBlobRangeValid(relocation.name_offset, relocation.name_size, metadata.size())) &&
        body[relocation.body_offset] == 0 && body[relocation.body_offset + 1u] == 0 &&
        body[relocation.body_offset + 2u] == 0 && body[relocation.body_offset + 3u] == 0;
    if (!valid) {
      *failure_index = index;
      return E_INVALIDARG;
    }
    offsets.push_back(relocation.body_offset);
  }

  std::sort(offsets.begin(), offsets.end());
  if (std::adjacent_find(offsets.begin(), offsets.end()) != offsets.end()) {
    *failure_index = UINT32_MAX;
    return E_INVALIDARG;
  }
  return S_OK;
}

HRESULT ValidateGenerationIlMap(const std::vector<alvorkit_interception_il_map_v3>& il_map, uint32_t old_code_size,
                                uint32_t new_code_size) {
  uint32_t prior_old = 0;
  uint32_t prior_new = 0;
  bool first = true;
  for (const auto& entry : il_map) {
    const bool valid = entry.reserved == 0 && entry.accurate <= 1 && entry.old_offset <= old_code_size &&
                       entry.new_offset <= new_code_size &&
                       (first || (entry.old_offset > prior_old && entry.new_offset >= prior_new));
    if (!valid)
      return E_INVALIDARG;

    first = false;
    prior_old = entry.old_offset;
    prior_new = entry.new_offset;
  }
  return S_OK;
}

#include "MethodBody.hpp"

#include <cstddef>
#include <cstdint>

namespace {
uint16_t ReadUInt16(const BYTE* source) {
  return static_cast<uint16_t>(source[0] | (static_cast<uint16_t>(source[1]) << 8u));
}

uint32_t ReadUInt32(const BYTE* source) {
  return static_cast<uint32_t>(source[0] | (static_cast<uint32_t>(source[1]) << 8u) |
                               (static_cast<uint32_t>(source[2]) << 16u) | (static_cast<uint32_t>(source[3]) << 24u));
}
} // namespace

bool IsMethodBody(const uint8_t* body, uint32_t size) {
  if (body == nullptr || size == 0)
    return false;

  const uint8_t format = body[0] & CorILMethod_FormatMask;
  if (format == CorILMethod_TinyFormat || format == CorILMethod_TinyFormat1) {
    const uint32_t code_size = body[0] >> 2u;
    return size == code_size + 1u;
  }

  if (format != CorILMethod_FatFormat || size < sizeof(IMAGE_COR_ILMETHOD_FAT)) {
    return false;
  }

  const auto* header = reinterpret_cast<const IMAGE_COR_ILMETHOD_FAT*>(body);
  const uint32_t header_size = header->Size * sizeof(uint32_t);
  return header_size >= sizeof(IMAGE_COR_ILMETHOD_FAT) && header_size <= size && header->CodeSize <= size - header_size;
}

bool TryGetMethodCodeSize(const uint8_t* body, uint32_t size, uint32_t* code_size) {
  if (!IsMethodBody(body, size))
    return false;

  const uint8_t format = body[0] & CorILMethod_FormatMask;
  if (format == CorILMethod_TinyFormat || format == CorILMethod_TinyFormat1) {
    *code_size = body[0] >> 2u;
    return true;
  }

  const auto* header = reinterpret_cast<const IMAGE_COR_ILMETHOD_FAT*>(body);
  *code_size = header->CodeSize;
  return true;
}

HRESULT ParseMethodBody(const BYTE* body, uint32_t size, ParsedMethodBody* result) {
  if (body == nullptr || size == 0 || result == nullptr)
    return COR_E_BADIMAGEFORMAT;

  *result = {};
  const uint8_t format = body[0] & CorILMethod_FormatMask;
  if (format == CorILMethod_TinyFormat || format == CorILMethod_TinyFormat1) {
    result->code_size = body[0] >> 2u;
    result->code = body + 1;
    return result->code_size + 1u <= size ? S_OK : COR_E_BADIMAGEFORMAT;
  }
  if (format != CorILMethod_FatFormat || size < sizeof(IMAGE_COR_ILMETHOD_FAT)) {
    return COR_E_BADIMAGEFORMAT;
  }

  const auto* header = reinterpret_cast<const IMAGE_COR_ILMETHOD_FAT*>(body);
  const uint32_t header_size = header->Size * sizeof(uint32_t);
  if (header_size < sizeof(IMAGE_COR_ILMETHOD_FAT) || header_size > size || header->CodeSize > size - header_size) {
    return COR_E_BADIMAGEFORMAT;
  }

  result->code = body + header_size;
  result->code_size = header->CodeSize;
  result->maximum_stack = header->MaxStack;
  result->local_signature = header->LocalVarSigTok;
  result->initialize_locals = (header->Flags & CorILMethod_InitLocals) != 0;
  if ((header->Flags & CorILMethod_MoreSects) == 0)
    return S_OK;

  size_t section_offset = (static_cast<size_t>(header_size) + result->code_size + 3u) & ~static_cast<size_t>(3u);
  bool more_sections = true;
  while (more_sections) {
    if (section_offset + 4u > size)
      return COR_E_BADIMAGEFORMAT;

    const BYTE* section = body + section_offset;
    const uint8_t kind = section[0];
    more_sections = (kind & CorILMethod_Sect_MoreSects) != 0;
    if ((kind & CorILMethod_Sect_KindMask) != CorILMethod_Sect_EHTable) {
      return COR_E_NOTSUPPORTED;
    }

    const bool fat = (kind & CorILMethod_Sect_FatFormat) != 0;
    const uint32_t data_size = fat ? static_cast<uint32_t>(section[1] | (static_cast<uint32_t>(section[2]) << 8u) |
                                                           (static_cast<uint32_t>(section[3]) << 16u))
                                   : section[1];
    const uint32_t clause_size = fat ? 24u : 12u;
    if (data_size < 4u || (data_size - 4u) % clause_size != 0u || section_offset + data_size > size) {
      return COR_E_BADIMAGEFORMAT;
    }

    for (uint32_t offset = 4u; offset < data_size; offset += clause_size) {
      const BYTE* clause = section + offset;
      MethodExceptionClause value{};
      value.flags = fat ? ReadUInt32(clause) : ReadUInt16(clause);
      value.try_offset = fat ? ReadUInt32(clause + 4u) : ReadUInt16(clause + 2u);
      value.try_length = fat ? ReadUInt32(clause + 8u) : clause[4];
      value.handler_offset = fat ? ReadUInt32(clause + 12u) : ReadUInt16(clause + 5u);
      value.handler_length = fat ? ReadUInt32(clause + 16u) : clause[7];
      value.class_token_or_filter_offset = fat ? ReadUInt32(clause + 20u) : ReadUInt32(clause + 8u);
      result->exception_clauses.push_back(value);
    }
    section_offset = (section_offset + data_size + 3u) & ~static_cast<size_t>(3u);
  }
  return S_OK;
}

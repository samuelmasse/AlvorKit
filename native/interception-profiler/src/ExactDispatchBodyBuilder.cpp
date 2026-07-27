#include "ExactDispatchBodyBuilder.hpp"

#include "MethodBody.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <limits>

namespace {
class PrefixBuilder {
public:
  explicit PrefixBuilder(std::vector<uint8_t>* bytes) : bytes_(bytes) {}

  void Byte(uint8_t value) {
    bytes_->push_back(value);
  }

  void UInt32(uint32_t value) {
    for (uint32_t index = 0; index < sizeof(value); ++index)
      Byte(static_cast<uint8_t>(value >> (index * 8u)));
  }

  void UInt64(uint64_t value) {
    for (uint32_t index = 0; index < sizeof(value); ++index)
      Byte(static_cast<uint8_t>(value >> (index * 8u)));
  }

  void LoadArgument(uint32_t index) {
    if (index <= 3) {
      Byte(static_cast<uint8_t>(0x02u + index));
    } else if (index <= std::numeric_limits<uint8_t>::max()) {
      Byte(0x0E);
      Byte(static_cast<uint8_t>(index));
    } else {
      Byte(0xFE);
      Byte(0x09);
      Byte(static_cast<uint8_t>(index));
      Byte(static_cast<uint8_t>(index >> 8u));
    }
  }

private:
  std::vector<uint8_t>* bytes_;
};

void WriteUInt32(std::vector<uint8_t>* body, size_t offset, uint32_t value) {
  for (uint32_t index = 0; index < sizeof(value); ++index) {
    (*body)[offset + index] = static_cast<uint8_t>(value >> (index * 8u));
  }
}
} // namespace

HRESULT BuildExactDispatchBody(const BYTE* original_body, uint32_t original_body_size, const MethodMetadata& metadata,
                               uint64_t slot_id, uint64_t resolver_pointer, std::vector<uint8_t>* body) {
  ParsedMethodBody original;
  HRESULT status = ParseMethodBody(original_body, original_body_size, &original);
  if (FAILED(status))
    return status;

  const uint32_t argument_count = metadata.parameter_count + (metadata.is_static ? 0u : 1u);
  std::vector<uint8_t> prefix;
  prefix.reserve(static_cast<size_t>(argument_count) * 4u + 64u);
  PrefixBuilder emit(&prefix);
  for (uint32_t index = 0; index < argument_count; ++index)
    emit.LoadArgument(index);

  emit.Byte(0x21);
  emit.UInt64(slot_id);
  if (metadata.is_static)
    emit.Byte(0x14);
  else
    emit.LoadArgument(0);
  emit.Byte(0x21);
  emit.UInt64(resolver_pointer);
  emit.Byte(0xD3);
  emit.Byte(0x29);
  emit.UInt32(metadata.resolver_signature);
  emit.Byte(0x25);
  emit.Byte(0x2C);
  emit.Byte(0x06);
  emit.Byte(0x29);
  emit.UInt32(metadata.call_signature);
  emit.Byte(0x2A);
  emit.Byte(0x26);
  for (uint32_t index = 0; index < argument_count; ++index)
    emit.Byte(0x26);

  const uint64_t combined_code_size = static_cast<uint64_t>(prefix.size()) + original.code_size;
  const uint64_t required_stack = static_cast<uint64_t>(argument_count) + 3u;
  if (combined_code_size > std::numeric_limits<uint32_t>::max() ||
      required_stack > std::numeric_limits<uint16_t>::max()) {
    return COR_E_OVERFLOW;
  }

  constexpr uint32_t fat_header_size = 12;
  const size_t aligned_code_size =
      (fat_header_size + static_cast<size_t>(combined_code_size) + 3u) & ~static_cast<size_t>(3u);
  const uint64_t exception_section_size =
      original.exception_clauses.empty() ? 0u : 4u + static_cast<uint64_t>(original.exception_clauses.size()) * 24u;
  if (exception_section_size > 0x00FFFFFFu ||
      aligned_code_size > std::numeric_limits<size_t>::max() - exception_section_size) {
    return COR_E_OVERFLOW;
  }

  body->assign(aligned_code_size + static_cast<size_t>(exception_section_size), 0);
  const uint16_t flags =
      static_cast<uint16_t>(CorILMethod_FatFormat | (!original.exception_clauses.empty() ? CorILMethod_MoreSects : 0) |
                            (original.initialize_locals ? CorILMethod_InitLocals : 0));
  const uint16_t flags_and_size = static_cast<uint16_t>((3u << 12u) | flags);
  const uint16_t maximum_stack = std::max(original.maximum_stack, static_cast<uint16_t>(required_stack));

  (*body)[0] = static_cast<uint8_t>(flags_and_size);
  (*body)[1] = static_cast<uint8_t>(flags_and_size >> 8u);
  (*body)[2] = static_cast<uint8_t>(maximum_stack);
  (*body)[3] = static_cast<uint8_t>(maximum_stack >> 8u);
  WriteUInt32(body, 4, static_cast<uint32_t>(combined_code_size));
  WriteUInt32(body, 8, original.local_signature);
  std::copy(prefix.begin(), prefix.end(), body->begin() + fat_header_size);
  std::copy(original.code, original.code + original.code_size, body->begin() + fat_header_size + prefix.size());
  if (original.exception_clauses.empty())
    return S_OK;

  const size_t section_offset = aligned_code_size;
  const uint32_t data_size = static_cast<uint32_t>(exception_section_size);
  (*body)[section_offset] = static_cast<uint8_t>(CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat);
  (*body)[section_offset + 1u] = static_cast<uint8_t>(data_size);
  (*body)[section_offset + 2u] = static_cast<uint8_t>(data_size >> 8u);
  (*body)[section_offset + 3u] = static_cast<uint8_t>(data_size >> 16u);

  const uint32_t prefix_size = static_cast<uint32_t>(prefix.size());
  size_t destination = section_offset + 4u;
  for (const MethodExceptionClause& source : original.exception_clauses) {
    const bool is_filter = (source.flags & COR_ILEXCEPTION_CLAUSE_FILTER) != 0;
    const uint32_t filter_or_token =
        is_filter ? source.class_token_or_filter_offset + prefix_size : source.class_token_or_filter_offset;
    WriteUInt32(body, destination, source.flags);
    WriteUInt32(body, destination + 4u, source.try_offset + prefix_size);
    WriteUInt32(body, destination + 8u, source.try_length);
    WriteUInt32(body, destination + 12u, source.handler_offset + prefix_size);
    WriteUInt32(body, destination + 16u, source.handler_length);
    WriteUInt32(body, destination + 20u, filter_or_token);
    destination += 24u;
  }
  return S_OK;
}

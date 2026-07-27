#include "MethodSignature.hpp"

#include "SignatureTypeReader.hpp"

#include <cstdint>
#include <limits>

namespace {
constexpr uint64_t fnv_offset_basis = 14695981039346656037ull;
constexpr uint64_t fnv_prime = 1099511628211ull;
} // namespace

HRESULT BuildExactCallSignature(PCCOR_SIGNATURE signature, ULONG signature_size, mdTypeDef declaring_type,
                                bool is_static, std::vector<COR_SIGNATURE>* result) {
  if (signature == nullptr || signature_size < 2)
    return COR_E_BADIMAGEFORMAT;
  if (is_static) {
    result->assign(signature, signature + signature_size);
    return S_OK;
  }

  const BYTE calling_convention = signature[0];
  if ((calling_convention & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0)
    return COR_E_NOTSUPPORTED;

  const COR_SIGNATURE* end = signature + signature_size;
  const COR_SIGNATURE* cursor = signature + 1;
  ULONG parameter_count = 0;
  const ULONG count_size = CorSigUncompressData(cursor, &parameter_count);
  if (count_size == static_cast<ULONG>(-1) || count_size > static_cast<ULONG>(end - cursor) ||
      parameter_count == std::numeric_limits<ULONG>::max()) {
    return COR_E_BADIMAGEFORMAT;
  }
  cursor += count_size;
  const COR_SIGNATURE* return_start = cursor;
  if (!SkipSignatureType(&cursor, end))
    return COR_E_BADIMAGEFORMAT;
  const COR_SIGNATURE* return_end = cursor;

  result->clear();
  result->reserve(signature_size + 8u);
  result->push_back(static_cast<COR_SIGNATURE>(calling_convention &
                                               ~(IMAGE_CEE_CS_CALLCONV_HASTHIS | IMAGE_CEE_CS_CALLCONV_EXPLICITTHIS)));
  COR_SIGNATURE compressed[8]{};
  ULONG compressed_size = CorSigCompressData(parameter_count + 1u, compressed);
  result->insert(result->end(), compressed, compressed + compressed_size);
  result->insert(result->end(), return_start, return_end);
  result->push_back(ELEMENT_TYPE_CLASS);
  compressed_size = CorSigCompressToken(declaring_type, compressed);
  result->insert(result->end(), compressed, compressed + compressed_size);
  result->insert(result->end(), return_end, end);
  return S_OK;
}

uint64_t ComputeSignatureHash(PCCOR_SIGNATURE signature, ULONG signature_size) {
  uint64_t hash = fnv_offset_basis;
  for (ULONG index = 0; index < signature_size; ++index) {
    hash ^= signature[index];
    hash *= fnv_prime;
  }
  return hash;
}

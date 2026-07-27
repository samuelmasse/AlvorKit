#include "SignatureTypeReader.hpp"

namespace {
bool SkipCompressedData(const COR_SIGNATURE** cursor, const COR_SIGNATURE* end) {
  if (*cursor >= end)
    return false;

  ULONG value = 0;
  const ULONG size = CorSigUncompressData(*cursor, &value);
  if (size == static_cast<ULONG>(-1) || size > static_cast<ULONG>(end - *cursor)) {
    return false;
  }

  *cursor += size;
  return true;
}

bool SkipCompressedSignedData(const COR_SIGNATURE** cursor, const COR_SIGNATURE* end) {
  if (*cursor >= end)
    return false;

  int value = 0;
  const ULONG size = CorSigUncompressSignedInt(*cursor, &value);
  if (size == static_cast<ULONG>(-1) || size > static_cast<ULONG>(end - *cursor)) {
    return false;
  }

  *cursor += size;
  return true;
}

bool SkipMethodSignature(const COR_SIGNATURE** cursor, const COR_SIGNATURE* end) {
  if (*cursor >= end)
    return false;

  const BYTE calling_convention = *(*cursor)++;
  if ((calling_convention & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0 && !SkipCompressedData(cursor, end)) {
    return false;
  }

  const COR_SIGNATURE* count_position = *cursor;
  ULONG parameter_count = 0;
  const ULONG count_size = CorSigUncompressData(count_position, &parameter_count);
  if (count_size == static_cast<ULONG>(-1) || count_size > static_cast<ULONG>(end - count_position)) {
    return false;
  }
  *cursor += count_size;
  if (!SkipSignatureType(cursor, end))
    return false;

  for (ULONG index = 0; index < parameter_count; ++index) {
    if (*cursor < end && **cursor == ELEMENT_TYPE_SENTINEL)
      ++*cursor;
    if (!SkipSignatureType(cursor, end))
      return false;
  }

  return true;
}
} // namespace

bool SkipSignatureType(const COR_SIGNATURE** cursor, const COR_SIGNATURE* end) {
  while (*cursor < end) {
    const CorElementType prefix = static_cast<CorElementType>(**cursor);
    if (prefix == ELEMENT_TYPE_CMOD_REQD || prefix == ELEMENT_TYPE_CMOD_OPT) {
      ++*cursor;
      if (!SkipCompressedData(cursor, end))
        return false;
      continue;
    }
    if (prefix == ELEMENT_TYPE_PINNED || prefix == ELEMENT_TYPE_SENTINEL) {
      ++*cursor;
      continue;
    }
    break;
  }
  if (*cursor >= end)
    return false;

  const CorElementType type = static_cast<CorElementType>(*(*cursor)++);
  switch (type) {
  case ELEMENT_TYPE_VOID:
  case ELEMENT_TYPE_BOOLEAN:
  case ELEMENT_TYPE_CHAR:
  case ELEMENT_TYPE_I1:
  case ELEMENT_TYPE_U1:
  case ELEMENT_TYPE_I2:
  case ELEMENT_TYPE_U2:
  case ELEMENT_TYPE_I4:
  case ELEMENT_TYPE_U4:
  case ELEMENT_TYPE_I8:
  case ELEMENT_TYPE_U8:
  case ELEMENT_TYPE_R4:
  case ELEMENT_TYPE_R8:
  case ELEMENT_TYPE_STRING:
  case ELEMENT_TYPE_TYPEDBYREF:
  case ELEMENT_TYPE_I:
  case ELEMENT_TYPE_U:
  case ELEMENT_TYPE_OBJECT:
    return true;
  case ELEMENT_TYPE_CLASS:
  case ELEMENT_TYPE_VALUETYPE:
  case ELEMENT_TYPE_VAR:
  case ELEMENT_TYPE_MVAR:
    return SkipCompressedData(cursor, end);
  case ELEMENT_TYPE_PTR:
  case ELEMENT_TYPE_BYREF:
  case ELEMENT_TYPE_SZARRAY:
    return SkipSignatureType(cursor, end);
  case ELEMENT_TYPE_GENERICINST: {
    if (*cursor >= end || (**cursor != ELEMENT_TYPE_CLASS && **cursor != ELEMENT_TYPE_VALUETYPE)) {
      return false;
    }
    ++*cursor;
    if (!SkipCompressedData(cursor, end))
      return false;

    const COR_SIGNATURE* count_position = *cursor;
    ULONG argument_count = 0;
    const ULONG count_size = CorSigUncompressData(count_position, &argument_count);
    if (count_size == static_cast<ULONG>(-1) || count_size > static_cast<ULONG>(end - count_position)) {
      return false;
    }
    *cursor += count_size;
    for (ULONG index = 0; index < argument_count; ++index) {
      if (!SkipSignatureType(cursor, end))
        return false;
    }
    return true;
  }
  case ELEMENT_TYPE_ARRAY: {
    if (!SkipSignatureType(cursor, end) || !SkipCompressedData(cursor, end)) {
      return false;
    }

    const COR_SIGNATURE* count_position = *cursor;
    ULONG size_count = 0;
    const ULONG count_size = CorSigUncompressData(count_position, &size_count);
    if (count_size == static_cast<ULONG>(-1) || count_size > static_cast<ULONG>(end - count_position)) {
      return false;
    }
    *cursor += count_size;
    for (ULONG index = 0; index < size_count; ++index) {
      if (!SkipCompressedData(cursor, end))
        return false;
    }

    count_position = *cursor;
    ULONG lower_bound_count = 0;
    const ULONG lower_bound_count_size = CorSigUncompressData(count_position, &lower_bound_count);
    if (lower_bound_count_size == static_cast<ULONG>(-1) ||
        lower_bound_count_size > static_cast<ULONG>(end - count_position)) {
      return false;
    }
    *cursor += lower_bound_count_size;
    for (ULONG index = 0; index < lower_bound_count; ++index) {
      if (!SkipCompressedSignedData(cursor, end))
        return false;
    }
    return true;
  }
  case ELEMENT_TYPE_FNPTR:
    return SkipMethodSignature(cursor, end);
  default:
    return false;
  }
}

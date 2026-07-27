#pragma once

#include <cstdint>
#include <vector>

#include "cor.h"

HRESULT BuildExactCallSignature(PCCOR_SIGNATURE signature, ULONG signature_size, mdTypeDef declaring_type,
                                bool is_static, std::vector<COR_SIGNATURE>* result);

uint64_t ComputeSignatureHash(PCCOR_SIGNATURE signature, ULONG signature_size);

#pragma once

#include <cstdint>
#include <vector>

#include "ProfilerModels.hpp"

HRESULT BuildExactDispatchBody(const BYTE* original_body, uint32_t original_body_size, const MethodMetadata& metadata,
                               uint64_t slot_id, uint64_t resolver_pointer, std::vector<uint8_t>* body);

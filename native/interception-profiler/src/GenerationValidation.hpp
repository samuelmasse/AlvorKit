#pragma once

#include <cstdint>
#include <vector>

#include "alvorkit_interception_profiler.h"
#include "cor.h"

HRESULT ValidateGenerationRelocations(const std::vector<alvorkit_interception_relocation_v3>& relocations,
                                      const std::vector<uint8_t>& metadata, const std::vector<uint8_t>& body,
                                      uint32_t* failure_index);

HRESULT ValidateGenerationIlMap(const std::vector<alvorkit_interception_il_map_v3>& il_map, uint32_t old_code_size,
                                uint32_t new_code_size);

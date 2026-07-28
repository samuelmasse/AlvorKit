#pragma once

#include <cstdint>
#include <vector>

#include "CoreClrHeaders.hpp"

struct MethodExceptionClause {
  uint32_t flags = 0;
  uint32_t try_offset = 0;
  uint32_t try_length = 0;
  uint32_t handler_offset = 0;
  uint32_t handler_length = 0;
  uint32_t class_token_or_filter_offset = 0;
};

struct ParsedMethodBody {
  const BYTE* code = nullptr;
  uint32_t code_size = 0;
  uint16_t maximum_stack = 8;
  uint32_t local_signature = 0;
  bool initialize_locals = false;
  std::vector<MethodExceptionClause> exception_clauses;
};

bool IsMethodBody(const uint8_t* body, uint32_t size);

bool TryGetMethodCodeSize(const uint8_t* body, uint32_t size, uint32_t* code_size);

HRESULT ParseMethodBody(const BYTE* body, uint32_t size, ParsedMethodBody* result);

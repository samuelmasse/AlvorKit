#pragma once

#include <chrono>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <vector>

#include "alvorkit_interception_profiler.h"
#include "cor.h"
#include "corprof.h"

struct RuntimeMethodKey {
  ModuleID module_id = 0;
  mdMethodDef method_id = mdMethodDefNil;
  uint64_t module_epoch = 0;

  bool operator==(const RuntimeMethodKey&) const = default;
};

struct RuntimeMethodKeyHash {
  size_t operator()(const RuntimeMethodKey& key) const noexcept;
};

enum class ProfilerBodyKind { Raw, ExactDispatch, Generation };

struct ProfilerCommand {
  alvorkit_interception_operation_v2 operation = ALVORKIT_INTERCEPTION_OPERATION_NONE;
  ProfilerBodyKind body_kind = ProfilerBodyKind::Raw;
  uint64_t request_id = 0;
  uint64_t patch_id = 0;
  alvorkit_interception_target_v2 target{};
  uint32_t patch_flags = 0;
  uint64_t slot_id = 0;
  uint64_t resolver_pointer = 0;
  uint64_t generation_id = 0;
  uint64_t prior_generation_id = 0;
  alvorkit_interception_body_identity_v3 baseline_body_identity{};
  std::vector<uint8_t> il_body;
  std::vector<alvorkit_interception_relocation_v3> relocations;
  std::vector<uint8_t> metadata;
  std::vector<alvorkit_interception_il_map_v3> il_map;
};

struct PreparedRejit {
  std::vector<uint8_t> body;
  std::vector<COR_IL_MAP> il_map;
  uint32_t flags = 0;
};

struct ProfilerPatch {
  uint64_t patch_id = 0;
  alvorkit_interception_target_v2 target{};
  RuntimeMethodKey method{};
  bool active = false;
  uint64_t active_generation_id = 0;
  uint64_t pending_request_id = 0;
  alvorkit_interception_operation_v2 pending_operation = ALVORKIT_INTERCEPTION_OPERATION_NONE;
  uint64_t pending_generation_id = 0;
  uint64_t pending_prior_generation_id = 0;
  std::shared_ptr<const PreparedRejit> pending_rejit;
};

struct MetadataTokenRecord {
  ModuleID module_id = 0;
  uint64_t module_epoch = 0;
  uint32_t kind = ALVORKIT_INTERCEPTION_RELOCATION_NONE;
  mdToken parent_token = mdTokenNil;
  std::vector<uint8_t> name;
  std::vector<uint8_t> signature;
  mdToken token = mdTokenNil;
};

struct BodyReadRequest {
  alvorkit_interception_target_v2 target{};
  HRESULT status = E_PENDING;
  std::vector<uint8_t> body;
  alvorkit_interception_body_identity_v3 identity{};
  bool completed = false;
};

struct MethodMetadata {
  mdSignature call_signature = mdSignatureNil;
  mdSignature resolver_signature = mdSignatureNil;
  uint32_t parameter_count = 0;
  bool is_static = false;
};

using ProfilerClock = std::chrono::steady_clock;

#pragma once

#include <stdint.h>

#if defined(_WIN32)
#define ALVORKIT_INTERCEPTION_CALL __cdecl
#else
#define ALVORKIT_INTERCEPTION_CALL
#endif

#if defined(ALVORKIT_INTERCEPTION_PROFILER_BUILD) && !defined(_WIN32)
#define ALVORKIT_INTERCEPTION_API __attribute__((visibility("default")))
#else
#define ALVORKIT_INTERCEPTION_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

enum {
  ALVORKIT_INTERCEPTION_ABI_VERSION = 3,
  ALVORKIT_INTERCEPTION_MAX_IL_BODY_BYTES = 1024 * 1024,
  ALVORKIT_INTERCEPTION_MAX_METADATA_BYTES = 1024 * 1024,
  ALVORKIT_INTERCEPTION_MAX_RELOCATIONS = 4096,
  ALVORKIT_INTERCEPTION_MAX_IL_MAP_ENTRIES = 65536,
  ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS = 256,
  ALVORKIT_INTERCEPTION_MAX_ACTIVE_PATCHES = 4096,
  ALVORKIT_INTERCEPTION_MAX_ALLOCATION_SAMPLES = 65536,
  ALVORKIT_INTERCEPTION_MAX_ALLOCATION_FRAMES = 128
};

typedef enum alvorkit_interception_capability_v2 {
  ALVORKIT_INTERCEPTION_CAPABILITY_NONE = 0,
  ALVORKIT_INTERCEPTION_CAPABILITY_REJIT = 1 << 0,
  ALVORKIT_INTERCEPTION_CAPABILITY_REJIT_INLINERS = 1 << 1,
  ALVORKIT_INTERCEPTION_CAPABILITY_REVERT = 1 << 2,
  ALVORKIT_INTERCEPTION_CAPABILITY_RAW_IL = 1 << 3,
  ALVORKIT_INTERCEPTION_CAPABILITY_MULTIPLE_PATCHES = 1 << 4,
  ALVORKIT_INTERCEPTION_CAPABILITY_SIGNATURE_VALIDATION = 1 << 5,
  ALVORKIT_INTERCEPTION_CAPABILITY_EXACT_DISPATCH = 1 << 6,
  ALVORKIT_INTERCEPTION_CAPABILITY_METHOD_GENERATIONS = 1 << 7,
  ALVORKIT_INTERCEPTION_CAPABILITY_LATE_METADATA = 1 << 8,
  ALVORKIT_INTERCEPTION_CAPABILITY_IL_MAP = 1 << 9,
  ALVORKIT_INTERCEPTION_CAPABILITY_BODY_IDENTITY = 1 << 10,
  ALVORKIT_INTERCEPTION_CAPABILITY_LOADED_BODY = 1 << 11,
  ALVORKIT_INTERCEPTION_CAPABILITY_ALLOCATION_CAPTURE = 1 << 12
} alvorkit_interception_capability_v2;

typedef enum alvorkit_interception_operation_v2 {
  ALVORKIT_INTERCEPTION_OPERATION_NONE = 0,
  ALVORKIT_INTERCEPTION_OPERATION_INSTALL = 1,
  ALVORKIT_INTERCEPTION_OPERATION_REPLACE = 2,
  ALVORKIT_INTERCEPTION_OPERATION_REMOVE = 3
} alvorkit_interception_operation_v2;

typedef enum alvorkit_interception_state_v2 {
  ALVORKIT_INTERCEPTION_STATE_UNAVAILABLE = 0,
  ALVORKIT_INTERCEPTION_STATE_IDLE = 1,
  ALVORKIT_INTERCEPTION_STATE_QUEUED = 2,
  ALVORKIT_INTERCEPTION_STATE_REQUESTED = 3,
  ALVORKIT_INTERCEPTION_STATE_APPLYING = 4,
  ALVORKIT_INTERCEPTION_STATE_ACTIVE = 5,
  ALVORKIT_INTERCEPTION_STATE_REMOVING = 6,
  ALVORKIT_INTERCEPTION_STATE_REMOVED = 7,
  ALVORKIT_INTERCEPTION_STATE_FAILED = 8
} alvorkit_interception_state_v2;

typedef enum alvorkit_interception_patch_flag_v2 {
  ALVORKIT_INTERCEPTION_PATCH_FLAG_NONE = 0,
  ALVORKIT_INTERCEPTION_PATCH_FLAG_DISABLE_INLINING = 1 << 0
} alvorkit_interception_patch_flag_v2;

typedef enum alvorkit_interception_relocation_kind_v3 {
  ALVORKIT_INTERCEPTION_RELOCATION_NONE = 0,
  ALVORKIT_INTERCEPTION_RELOCATION_STANDALONE_SIGNATURE = 1,
  ALVORKIT_INTERCEPTION_RELOCATION_TYPE_SPEC = 2,
  ALVORKIT_INTERCEPTION_RELOCATION_MEMBER_REF = 3,
  ALVORKIT_INTERCEPTION_RELOCATION_METHOD_SPEC = 4
} alvorkit_interception_relocation_kind_v3;

typedef enum alvorkit_interception_failure_stage_v3 {
  ALVORKIT_INTERCEPTION_FAILURE_NONE = 0,
  ALVORKIT_INTERCEPTION_FAILURE_VALIDATION = 1,
  ALVORKIT_INTERCEPTION_FAILURE_TARGET = 2,
  ALVORKIT_INTERCEPTION_FAILURE_BASELINE = 3,
  ALVORKIT_INTERCEPTION_FAILURE_METADATA = 4,
  ALVORKIT_INTERCEPTION_FAILURE_IL_MAP = 5,
  ALVORKIT_INTERCEPTION_FAILURE_REJIT = 6
} alvorkit_interception_failure_stage_v3;

typedef struct alvorkit_guid_v2 {
  uint32_t data1;
  uint16_t data2;
  uint16_t data3;
  uint8_t data4[8];
} alvorkit_guid_v2;

typedef struct alvorkit_interception_target_v2 {
  alvorkit_guid_v2 module_mvid;
  int32_t method_token;
  uint32_t reserved;
  uint64_t signature_hash;
} alvorkit_interception_target_v2;

typedef struct alvorkit_interception_body_identity_v3 {
  uint8_t sha256[32];
} alvorkit_interception_body_identity_v3;

typedef struct alvorkit_interception_capabilities_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t flags;
  uint32_t maximum_il_body_bytes;
  uint32_t maximum_metadata_bytes;
  uint32_t maximum_relocations;
  uint32_t maximum_il_map_entries;
  uint32_t maximum_pending_requests;
  uint32_t maximum_active_patches;
  uint32_t reserved;
} alvorkit_interception_capabilities_v2;

typedef struct alvorkit_interception_install_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  alvorkit_interception_target_v2 target;
  uint32_t patch_flags;
  uint32_t il_body_size;
} alvorkit_interception_install_v2;

typedef struct alvorkit_interception_remove_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  alvorkit_interception_target_v2 target;
} alvorkit_interception_remove_v2;

typedef struct alvorkit_interception_install_dispatch_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  alvorkit_interception_target_v2 target;
  uint32_t patch_flags;
  uint32_t reserved;
  uint64_t slot_id;
  uint64_t resolver_pointer;
} alvorkit_interception_install_dispatch_v2;

typedef struct alvorkit_interception_generation_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  alvorkit_interception_target_v2 target;
  uint32_t patch_flags;
  uint32_t il_body_size;
  uint64_t generation_id;
  uint64_t prior_generation_id;
  alvorkit_interception_body_identity_v3 baseline_body_identity;
  uint32_t relocation_count;
  uint32_t metadata_size;
  uint32_t il_map_count;
  uint32_t reserved;
} alvorkit_interception_generation_v3;

typedef struct alvorkit_interception_relocation_v3 {
  uint32_t kind;
  uint32_t body_offset;
  int32_t parent_token;
  uint32_t signature_offset;
  uint32_t signature_size;
  uint32_t name_offset;
  uint32_t name_size;
  uint32_t reserved;
} alvorkit_interception_relocation_v3;

typedef struct alvorkit_interception_il_map_v3 {
  uint32_t old_offset;
  uint32_t new_offset;
  uint32_t accurate;
  uint32_t reserved;
} alvorkit_interception_il_map_v3;

typedef struct alvorkit_interception_relocation_result_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t generation_id;
  uint32_t relocation_index;
  uint32_t kind;
  int32_t metadata_token;
  int32_t hresult;
} alvorkit_interception_relocation_result_v3;

typedef struct alvorkit_interception_generation_completion_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  uint64_t generation_id;
  uint64_t prior_generation_id;
  int32_t state;
  int32_t hresult;
  uint32_t failure_stage;
  uint32_t failure_relocation_index;
  uint32_t requested_relocations;
  uint32_t applied_relocations;
  uint32_t requested_il_map_entries;
  uint32_t applied_il_map_entries;
  uint64_t target_rejit_id;
} alvorkit_interception_generation_completion_v3;

typedef struct alvorkit_interception_completion_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t request_id;
  uint64_t patch_id;
  int32_t operation;
  int32_t state;
  int32_t hresult;
  uint32_t patch_flags;
  alvorkit_interception_target_v2 target;
  uint32_t rejit_started_callbacks;
  uint32_t parameter_callbacks;
  uint32_t rejit_finished_callbacks;
  uint32_t rejit_error_callbacks;
  uint64_t elapsed_microseconds;
} alvorkit_interception_completion_v2;

typedef struct alvorkit_interception_profiler_state_v2 {
  uint32_t size;
  uint32_t abi_version;
  uint32_t ready;
  uint32_t stopping;
  uint32_t pending_requests;
  uint32_t active_patches;
  uint32_t retained_completions;
  uint32_t reserved;
  uint64_t last_request_id;
} alvorkit_interception_profiler_state_v2;

typedef struct alvorkit_interception_allocation_capture_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint32_t sample_interval;
  uint32_t maximum_samples;
  uint32_t maximum_frames_per_sample;
  uint32_t reserved;
} alvorkit_interception_allocation_capture_v3;

typedef struct alvorkit_interception_allocation_summary_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t total_object_allocations;
  uint64_t sampled_object_allocations;
  uint64_t dropped_samples;
  uint64_t failed_stack_walks;
  uint32_t sample_interval;
  uint32_t maximum_frames_per_sample;
} alvorkit_interception_allocation_summary_v3;

typedef struct alvorkit_interception_allocation_sample_v3 {
  uint32_t size;
  uint32_t abi_version;
  uint64_t allocation_ordinal;
  uint64_t class_id;
  uint32_t frame_count;
  int32_t stack_hresult;
} alvorkit_interception_allocation_sample_v3;

typedef struct alvorkit_interception_allocation_frame_v3 {
  uint64_t function_id;
  uint64_t instruction_pointer;
} alvorkit_interception_allocation_frame_v3;

typedef struct alvorkit_interception_resolved_frame_v3 {
  uint32_t size;
  uint32_t abi_version;
  alvorkit_guid_v2 module_mvid;
  int32_t method_token;
  uint32_t il_offset;
  uint32_t has_il_offset;
  uint32_t reserved;
} alvorkit_interception_resolved_frame_v3;

ALVORKIT_INTERCEPTION_API uint32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_abi_version(void);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_capabilities(alvorkit_interception_capabilities_v2* capabilities);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_profiler_state(alvorkit_interception_profiler_state_v2* state);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_loaded_method_body(
    const alvorkit_interception_target_v2* target, uint8_t* body, uint32_t body_capacity, uint32_t* body_size,
    alvorkit_interception_body_identity_v3* identity);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_enqueue_install(
    const alvorkit_interception_install_v2* request, const uint8_t* il_body, uint32_t il_body_size);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_enqueue_install_dispatch(const alvorkit_interception_install_dispatch_v2* request);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_enqueue_generation(
    const alvorkit_interception_generation_v3* request, const uint8_t* il_body, uint32_t il_body_size,
    const alvorkit_interception_relocation_v3* relocations, uint32_t relocation_count, const uint8_t* metadata,
    uint32_t metadata_size, const alvorkit_interception_il_map_v3* il_map, uint32_t il_map_count);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_enqueue_remove(const alvorkit_interception_remove_v2* request);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_get_completion(uint64_t request_id, alvorkit_interception_completion_v2* completion);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_generation_completion(
    uint64_t request_id, alvorkit_interception_generation_completion_v3* completion);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_relocation_result(
    uint64_t request_id, uint32_t relocation_index, alvorkit_interception_relocation_result_v3* result);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_begin_allocation_capture(
    const alvorkit_interception_allocation_capture_v3* request);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL
alvorkit_interception_end_allocation_capture(alvorkit_interception_allocation_summary_v3* summary);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_get_allocation_sample(
    uint32_t sample_index, alvorkit_interception_allocation_sample_v3* sample,
    alvorkit_interception_allocation_frame_v3* frames, uint32_t frame_capacity);

ALVORKIT_INTERCEPTION_API int32_t ALVORKIT_INTERCEPTION_CALL alvorkit_interception_resolve_allocation_frame(
    const alvorkit_interception_allocation_frame_v3* frame,
    alvorkit_interception_resolved_frame_v3* resolved);

#ifdef __cplusplus
}
#endif

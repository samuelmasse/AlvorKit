#include "ProfilerRuntime.hpp"

#include "MethodBody.hpp"

#include <cstdint>
#include <new>
#include <utility>

HRESULT ProfilerRuntime::EnqueueGeneration(const alvorkit_interception_generation_v3* request, const uint8_t* il_body,
                                           uint32_t il_body_size,
                                           const alvorkit_interception_relocation_v3* relocations,
                                           uint32_t relocation_count, const uint8_t* metadata, uint32_t metadata_size,
                                           const alvorkit_interception_il_map_v3* il_map, uint32_t il_map_count) {
  if (request == nullptr || request->size < sizeof(alvorkit_interception_generation_v3) ||
      request->abi_version != ALVORKIT_INTERCEPTION_ABI_VERSION || request->request_id == 0 || request->patch_id == 0 ||
      request->generation_id == 0 || request->il_body_size != il_body_size ||
      request->relocation_count != relocation_count || request->metadata_size != metadata_size ||
      request->il_map_count != il_map_count || il_body_size > ALVORKIT_INTERCEPTION_MAX_IL_BODY_BYTES ||
      metadata_size > ALVORKIT_INTERCEPTION_MAX_METADATA_BYTES ||
      relocation_count > ALVORKIT_INTERCEPTION_MAX_RELOCATIONS ||
      il_map_count > ALVORKIT_INTERCEPTION_MAX_IL_MAP_ENTRIES ||
      TypeFromToken(request->target.method_token) != mdtMethodDef || !IsMethodBody(il_body, il_body_size) ||
      (relocation_count != 0 && relocations == nullptr) || (metadata_size != 0 && metadata == nullptr) ||
      (il_map_count != 0 && il_map == nullptr) ||
      (request->patch_flags & ~static_cast<uint32_t>(ALVORKIT_INTERCEPTION_PATCH_FLAG_DISABLE_INLINING)) != 0) {
    return E_INVALIDARG;
  }

  try {
    ProfilerCommand command;
    command.operation = ALVORKIT_INTERCEPTION_OPERATION_INSTALL;
    command.body_kind = ProfilerBodyKind::Generation;
    command.request_id = request->request_id;
    command.patch_id = request->patch_id;
    command.target = request->target;
    command.patch_flags = request->patch_flags;
    command.generation_id = request->generation_id;
    command.prior_generation_id = request->prior_generation_id;
    command.baseline_body_identity = request->baseline_body_identity;
    command.il_body.assign(il_body, il_body + il_body_size);
    if (relocation_count != 0) {
      command.relocations.assign(relocations, relocations + relocation_count);
    }
    if (metadata_size != 0) {
      command.metadata.assign(metadata, metadata + metadata_size);
    }
    if (il_map_count != 0)
      command.il_map.assign(il_map, il_map + il_map_count);
    return Enqueue(std::move(command));
  } catch (const std::bad_alloc&) {
    return E_OUTOFMEMORY;
  } catch (...) {
    return E_FAIL;
  }
}

#include "ProfilerRuntime.hpp"

#include "BodyIdentity.hpp"
#include "GenerationValidation.hpp"
#include "MethodBody.hpp"

#include <algorithm>
#include <cstdint>

HRESULT ProfilerRuntime::ValidateBaseline(const RuntimeMethodKey& method,
                                          const alvorkit_interception_body_identity_v3& expected) {
  LPCBYTE body = nullptr;
  ULONG body_size = 0;
  HRESULT status = info_->GetILFunctionBody(method.module_id, method.method_id, &body, &body_size);
  if (FAILED(status))
    return status;
  if (!IsMethodBody(body, body_size))
    return COR_E_BADIMAGEFORMAT;

  const auto actual = ComputeBodyIdentity(body, body_size);
  return std::equal(actual.begin(), actual.end(), expected.sha256) ? S_OK : kRevisionMismatchStatus;
}

HRESULT ProfilerRuntime::PrepareGeneration(const RuntimeMethodKey& method, ProfilerCommand* command) {
  HRESULT status = ValidateBaseline(method, command->baseline_body_identity);
  if (FAILED(status)) {
    rejit_.FailGeneration(command->request_id, status, ALVORKIT_INTERCEPTION_FAILURE_BASELINE);
    return status;
  }

  uint32_t failure_index = UINT32_MAX;
  status = ValidateGenerationRelocations(command->relocations, command->metadata, command->il_body, &failure_index);
  if (FAILED(status)) {
    rejit_.FailGeneration(command->request_id, status, ALVORKIT_INTERCEPTION_FAILURE_VALIDATION, failure_index);
    return status;
  }

  for (uint32_t index = 0; index < command->relocations.size(); ++index) {
    mdToken token = mdTokenNil;
    status = modules_.ResolveRelocation(method, command->relocations[index], command->metadata, &token);
    rejit_.RecordRelocation(command->request_id, index, token, status);
    if (FAILED(status)) {
      rejit_.FailGeneration(command->request_id, status, ALVORKIT_INTERCEPTION_FAILURE_METADATA, index);
      return status;
    }

    const uint32_t offset = command->relocations[index].body_offset;
    for (uint32_t byte = 0; byte < 4; ++byte) {
      command->il_body[offset + byte] = static_cast<uint8_t>(static_cast<uint32_t>(token) >> (byte * 8u));
    }
  }

  LPCBYTE baseline = nullptr;
  ULONG baseline_size = 0;
  status = info_->GetILFunctionBody(method.module_id, method.method_id, &baseline, &baseline_size);
  uint32_t old_code_size = 0;
  uint32_t new_code_size = 0;
  if (FAILED(status) || !TryGetMethodCodeSize(baseline, baseline_size, &old_code_size) ||
      !TryGetMethodCodeSize(command->il_body.data(), static_cast<uint32_t>(command->il_body.size()), &new_code_size)) {
    const HRESULT failure = FAILED(status) ? status : COR_E_BADIMAGEFORMAT;
    rejit_.FailGeneration(command->request_id, failure, ALVORKIT_INTERCEPTION_FAILURE_IL_MAP);
    return failure;
  }

  status = ValidateGenerationIlMap(command->il_map, old_code_size, new_code_size);
  if (FAILED(status)) {
    rejit_.FailGeneration(command->request_id, status, ALVORKIT_INTERCEPTION_FAILURE_IL_MAP);
  }
  return status;
}

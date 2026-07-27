#include "ProfilerRuntime.hpp"

#include "ExactDispatchBodyBuilder.hpp"

#include <cstdint>
#include <memory>
#include <utility>

HRESULT ProfilerRuntime::PrepareExactDispatch(const RuntimeMethodKey& method, const ProfilerCommand& command,
                                              std::vector<uint8_t>* body) {
  MethodMetadata metadata;
  if (!modules_.TryGetMethodMetadata(method, &metadata))
    return CORPROF_E_NOT_YET_AVAILABLE;

  LPCBYTE original_body = nullptr;
  ULONG original_body_size = 0;
  HRESULT status = info_->GetILFunctionBody(method.module_id, method.method_id, &original_body, &original_body_size);
  if (FAILED(status))
    return status;

  return BuildExactDispatchBody(original_body, original_body_size, metadata, command.slot_id, command.resolver_pointer,
                                body);
}

void ProfilerRuntime::ProcessInstall(ProfilerCommand command) {
  RuntimeMethodKey method;
  HRESULT status = modules_.ResolveTarget(command.target, &method);
  if (FAILED(status)) {
    if (command.body_kind == ProfilerBodyKind::Generation) {
      rejit_.FailGeneration(command.request_id, status, ALVORKIT_INTERCEPTION_FAILURE_TARGET);
    } else {
      rejit_.Fail(command.request_id, status);
    }
    return;
  }

  if (command.body_kind == ProfilerBodyKind::Generation) {
    status = rejit_.ValidateGenerationInstall(method, command);
    if (FAILED(status)) {
      rejit_.FailGeneration(command.request_id, status, ALVORKIT_INTERCEPTION_FAILURE_VALIDATION);
      return;
    }
    status = PrepareGeneration(method, &command);
    if (FAILED(status))
      return;
  } else if (command.body_kind == ProfilerBodyKind::ExactDispatch) {
    status = PrepareExactDispatch(method, command, &command.il_body);
    if (FAILED(status)) {
      rejit_.Fail(command.request_id, status);
      return;
    }
  }

  if (!modules_.IsCurrent(method)) {
    if (command.body_kind == ProfilerBodyKind::Generation) {
      rejit_.FailGeneration(command.request_id, CORPROF_E_DATAINCOMPLETE, ALVORKIT_INTERCEPTION_FAILURE_TARGET);
    } else {
      rejit_.Fail(command.request_id, CORPROF_E_DATAINCOMPLETE);
    }
    return;
  }

  auto prepared = std::make_shared<PreparedRejit>();
  prepared->body = std::move(command.il_body);
  prepared->flags = command.patch_flags;
  prepared->il_map.reserve(command.il_map.size());
  for (const auto& entry : command.il_map) {
    COR_IL_MAP mapped{};
    mapped.oldOffset = entry.old_offset;
    mapped.newOffset = entry.new_offset;
    mapped.fAccurate = entry.accurate != 0 ? TRUE : FALSE;
    prepared->il_map.push_back(mapped);
  }

  status = rejit_.BeginInstall(method, command, std::move(prepared));
  if (FAILED(status))
    return;

  ModuleID module_id = method.module_id;
  mdMethodDef method_id = method.method_id;
  status = info_->RequestReJITWithInliners(COR_PRF_REJIT_BLOCK_INLINING, 1, &module_id, &method_id);
  if (FAILED(status))
    rejit_.Fail(command.request_id, status);
}

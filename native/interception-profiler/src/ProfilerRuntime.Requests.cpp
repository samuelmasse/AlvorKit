#include "ProfilerRuntime.hpp"

#include "MethodBody.hpp"

#include <cstdint>
#include <new>
#include <utility>

namespace {
constexpr HRESULT error_command_queue_full = HRESULT_FROM_WIN32(56u); // ERROR_TOO_MANY_CMDS

bool ArePatchFlagsValid(uint32_t flags) {
  return (flags & ~static_cast<uint32_t>(ALVORKIT_INTERCEPTION_PATCH_FLAG_DISABLE_INLINING)) == 0;
}
} // namespace

HRESULT ProfilerRuntime::Enqueue(ProfilerCommand command) {
  std::lock_guard lock(queue_mutex_);
  if (stopping_ || !ready_)
    return E_UNEXPECTED;
  if (commands_.size() >= ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS) {
    return error_command_queue_full;
  }

  try {
    commands_.push_back(std::move(command));
    const HRESULT status = rejit_.Publish(commands_.back());
    if (FAILED(status)) {
      commands_.pop_back();
      return status;
    }
  } catch (const std::bad_alloc&) {
    return E_OUTOFMEMORY;
  } catch (...) {
    return E_FAIL;
  }

  changed_.notify_one();
  return S_OK;
}

HRESULT ProfilerRuntime::EnqueueInstall(const alvorkit_interception_install_v2* request, const uint8_t* il_body,
                                        uint32_t il_body_size) {
  if (request == nullptr || request->size < sizeof(alvorkit_interception_install_v2) ||
      request->abi_version != ALVORKIT_INTERCEPTION_ABI_VERSION || request->request_id == 0 || request->patch_id == 0 ||
      request->il_body_size != il_body_size || il_body_size > ALVORKIT_INTERCEPTION_MAX_IL_BODY_BYTES ||
      TypeFromToken(request->target.method_token) != mdtMethodDef || !IsMethodBody(il_body, il_body_size) ||
      !ArePatchFlagsValid(request->patch_flags)) {
    return E_INVALIDARG;
  }

  try {
    ProfilerCommand command;
    command.operation = ALVORKIT_INTERCEPTION_OPERATION_INSTALL;
    command.request_id = request->request_id;
    command.patch_id = request->patch_id;
    command.target = request->target;
    command.patch_flags = request->patch_flags;
    command.il_body.assign(il_body, il_body + il_body_size);
    return Enqueue(std::move(command));
  } catch (const std::bad_alloc&) {
    return E_OUTOFMEMORY;
  } catch (...) {
    return E_FAIL;
  }
}

HRESULT ProfilerRuntime::EnqueueInstallDispatch(const alvorkit_interception_install_dispatch_v2* request) {
  if (request == nullptr || request->size < sizeof(alvorkit_interception_install_dispatch_v2) ||
      request->abi_version != ALVORKIT_INTERCEPTION_ABI_VERSION || request->request_id == 0 || request->patch_id == 0 ||
      request->slot_id == 0 || request->resolver_pointer == 0 ||
      TypeFromToken(request->target.method_token) != mdtMethodDef || !ArePatchFlagsValid(request->patch_flags)) {
    return E_INVALIDARG;
  }

  ProfilerCommand command;
  command.operation = ALVORKIT_INTERCEPTION_OPERATION_INSTALL;
  command.body_kind = ProfilerBodyKind::ExactDispatch;
  command.request_id = request->request_id;
  command.patch_id = request->patch_id;
  command.target = request->target;
  command.patch_flags = request->patch_flags;
  command.slot_id = request->slot_id;
  command.resolver_pointer = request->resolver_pointer;
  return Enqueue(std::move(command));
}

HRESULT ProfilerRuntime::EnqueueRemove(const alvorkit_interception_remove_v2* request) {
  if (request == nullptr || request->size < sizeof(alvorkit_interception_remove_v2) ||
      request->abi_version != ALVORKIT_INTERCEPTION_ABI_VERSION || request->request_id == 0 || request->patch_id == 0 ||
      TypeFromToken(request->target.method_token) != mdtMethodDef) {
    return E_INVALIDARG;
  }

  ProfilerCommand command;
  command.operation = ALVORKIT_INTERCEPTION_OPERATION_REMOVE;
  command.request_id = request->request_id;
  command.patch_id = request->patch_id;
  command.target = request->target;
  return Enqueue(std::move(command));
}

#include "ProfilerRuntime.hpp"

void ProfilerRuntime::ProcessRemove(const ProfilerCommand& command) {
  RuntimeMethodKey method;
  HRESULT status = rejit_.BeginRemove(command, &method);
  if (FAILED(status))
    return;

  ModuleID module_id = method.module_id;
  mdMethodDef method_id = method.method_id;
  HRESULT method_status = E_PENDING;
  status = info_->RequestRevert(1, &module_id, &method_id, &method_status);
  if (FAILED(status)) {
    rejit_.Fail(command.request_id, status);
    return;
  }
  if (FAILED(method_status)) {
    rejit_.Fail(command.request_id, method_status);
    return;
  }

  status = info_->RequestReJITWithInliners(COR_PRF_REJIT_BLOCK_INLINING, 1, &module_id, &method_id);
  if (FAILED(status))
    rejit_.Fail(command.request_id, status);
}

#include "ProfilerRuntime.hpp"

#include "BodyIdentity.hpp"
#include "MethodBody.hpp"

#include <algorithm>
#include <cstdint>
#include <new>
#include <utility>

void ProfilerRuntime::WorkerMain() {
  const HRESULT initialize_status = info_->InitializeCurrentThread();
  if (FAILED(initialize_status)) {
    std::lock_guard lock(queue_mutex_);
    ready_ = false;
    while (!commands_.empty()) {
      rejit_.Fail(commands_.front().request_id, initialize_status);
      commands_.pop_front();
    }
    return;
  }

  for (;;) {
    ProfilerCommand command;
    BodyReadRequest* body_request = nullptr;
    AllocationFrameResolveRequest* frame_request = nullptr;
    ModuleID catalog_module = 0;
    bool has_command = false;
    {
      std::unique_lock lock(queue_mutex_);
      changed_.wait(lock, [this] {
        return stopping_ || !catalog_requests_.empty() || !body_requests_.empty() || !frame_requests_.empty() ||
               !commands_.empty();
      });
      if (stopping_)
        return;

      if (!catalog_requests_.empty()) {
        catalog_module = catalog_requests_.front();
        catalog_requests_.pop_front();
      } else if (!body_requests_.empty()) {
        body_request = body_requests_.front();
        body_requests_.pop_front();
      } else if (!frame_requests_.empty()) {
        frame_request = frame_requests_.front();
        frame_requests_.pop_front();
      } else {
        command = std::move(commands_.front());
        commands_.pop_front();
        has_command = true;
      }
    }

    if (catalog_module != 0) {
      try {
        (void)modules_.Catalog(catalog_module);
      } catch (...) {
      }
      continue;
    }
    if (frame_request != nullptr) {
      try {
        ProcessAllocationFrameResolve(frame_request);
      } catch (...) {
        std::lock_guard lock(queue_mutex_);
        frame_request->status = E_FAIL;
        frame_request->completed = true;
        frame_completed_.notify_all();
      }
      continue;
    }
    if (body_request != nullptr) {
      try {
        ProcessBodyRead(body_request);
      } catch (const std::bad_alloc&) {
        std::lock_guard lock(queue_mutex_);
        body_request->status = E_OUTOFMEMORY;
        body_request->completed = true;
        body_completed_.notify_all();
      } catch (...) {
        std::lock_guard lock(queue_mutex_);
        body_request->status = E_FAIL;
        body_request->completed = true;
        body_completed_.notify_all();
      }
      continue;
    }
    if (!has_command)
      continue;

    try {
      if (command.operation == ALVORKIT_INTERCEPTION_OPERATION_INSTALL) {
        ProcessInstall(std::move(command));
      } else if (command.operation == ALVORKIT_INTERCEPTION_OPERATION_REMOVE) {
        ProcessRemove(command);
      }
    } catch (const std::bad_alloc&) {
      rejit_.Fail(command.request_id, E_OUTOFMEMORY);
    } catch (...) {
      rejit_.Fail(command.request_id, E_FAIL);
    }
  }
}

void ProfilerRuntime::ProcessAllocationFrameResolve(AllocationFrameResolveRequest* request) {
  alvorkit_interception_resolved_frame_v3 resolved{};
  const HRESULT status = allocation_capture_.ResolveFrame(&request->frame, &resolved);
  {
    std::lock_guard lock(queue_mutex_);
    request->status = status;
    request->resolved = resolved;
    request->completed = true;
  }
  frame_completed_.notify_all();
}

void ProfilerRuntime::ProcessBodyRead(BodyReadRequest* request) {
  RuntimeMethodKey method;
  HRESULT status = modules_.ResolveTarget(request->target, &method);
  LPCBYTE loaded_body = nullptr;
  ULONG loaded_size = 0;
  if (SUCCEEDED(status)) {
    status = info_->GetILFunctionBody(method.module_id, method.method_id, &loaded_body, &loaded_size);
  }
  if (SUCCEEDED(status) && !IsMethodBody(loaded_body, loaded_size)) {
    status = COR_E_BADIMAGEFORMAT;
  }

  std::vector<uint8_t> body;
  alvorkit_interception_body_identity_v3 identity{};
  if (SUCCEEDED(status)) {
    body.assign(loaded_body, loaded_body + loaded_size);
    const auto digest = ComputeBodyIdentity(loaded_body, loaded_size);
    std::copy(digest.begin(), digest.end(), identity.sha256);
  }

  {
    std::lock_guard lock(queue_mutex_);
    request->status = status;
    request->body = std::move(body);
    request->identity = identity;
    request->completed = true;
  }
  body_completed_.notify_all();
}

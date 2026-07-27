#include "ProfilerRuntime.hpp"

#include <algorithm>
#include <cstdint>
#include <new>
#include <utility>

namespace {
constexpr HRESULT error_command_queue_full = HRESULT_FROM_WIN32(56u); // ERROR_TOO_MANY_CMDS
}

HRESULT ProfilerRuntime::GetLoadedMethodBody(const alvorkit_interception_target_v2* target, uint8_t* body,
                                             uint32_t body_capacity, uint32_t* body_size,
                                             alvorkit_interception_body_identity_v3* identity) {
  if (target == nullptr || body_size == nullptr || identity == nullptr || (body == nullptr && body_capacity != 0) ||
      TypeFromToken(target->method_token) != mdtMethodDef) {
    return E_INVALIDARG;
  }

  BodyReadRequest request;
  request.target = *target;
  {
    std::unique_lock lock(queue_mutex_);
    if (stopping_ || !ready_)
      return E_UNEXPECTED;
    if (body_requests_.size() >= ALVORKIT_INTERCEPTION_MAX_PENDING_REQUESTS) {
      return error_command_queue_full;
    }
    try {
      body_requests_.push_back(&request);
    } catch (const std::bad_alloc&) {
      return E_OUTOFMEMORY;
    } catch (...) {
      return E_FAIL;
    }
    changed_.notify_one();
    body_completed_.wait(lock, [&request, this] { return request.completed || stopping_; });
    if (!request.completed)
      return E_UNEXPECTED;
  }
  if (FAILED(request.status))
    return request.status;

  *body_size = static_cast<uint32_t>(request.body.size());
  *identity = request.identity;
  if (body == nullptr)
    return S_OK;
  if (body_capacity < request.body.size())
    return HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);

  std::copy(request.body.begin(), request.body.end(), body);
  return S_OK;
}

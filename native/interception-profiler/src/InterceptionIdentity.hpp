#pragma once

#include "alvorkit_interception_profiler.h"
#include "cor.h"

GUID ToGuid(const alvorkit_guid_v2& value);

bool EqualTarget(const alvorkit_interception_target_v2& left, const alvorkit_interception_target_v2& right);

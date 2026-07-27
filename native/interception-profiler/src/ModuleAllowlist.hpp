#pragma once

#include <string>
#include <vector>

#include "cor.h"
#include "corprof.h"

class ModuleAllowlist {
public:
  void ReadEnvironment();

  bool Allows(ICorProfilerInfo10* info, ModuleID module_id) const;

private:
  std::vector<std::basic_string<WCHAR>> modules_;
};

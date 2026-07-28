#pragma once

#include <string>
#include <vector>

#include "CoreClrHeaders.hpp"

class ModuleAllowlist {
public:
  void ReadEnvironment();

  bool Allows(ICorProfilerInfo10* info, ModuleID module_id) const;

private:
  std::vector<std::basic_string<WCHAR>> modules_;
};

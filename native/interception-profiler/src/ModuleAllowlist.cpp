#include "ModuleAllowlist.hpp"

#include <cstddef>
#include <cstdlib>

namespace {
using ModuleString = std::basic_string<WCHAR>;

WCHAR FoldModuleCharacter(WCHAR value) {
  if (value >= static_cast<WCHAR>('A') && value <= static_cast<WCHAR>('Z')) {
    return static_cast<WCHAR>(value + static_cast<WCHAR>('a' - 'A'));
  }
  return value;
}

bool EqualModuleName(const ModuleString& left, const ModuleString& right) {
  if (left.size() != right.size())
    return false;

  for (size_t index = 0; index < left.size(); ++index) {
    if (FoldModuleCharacter(left[index]) != FoldModuleCharacter(right[index])) {
      return false;
    }
  }
  return true;
}
} // namespace

void ModuleAllowlist::ReadEnvironment() {
  modules_.clear();
  wchar_t* configured = nullptr;
  size_t configured_size = 0;
  if (_wdupenv_s(&configured, &configured_size, L"ALVORKIT_INTERCEPTION_MODULES") != 0 || configured == nullptr) {
    return;
  }

  ModuleString value(configured);
  std::free(configured);

  size_t offset = 0;
  while (offset <= value.size()) {
    const size_t separator = value.find(static_cast<WCHAR>(';'), offset);
    const size_t end = separator == ModuleString::npos ? value.size() : separator;
    ModuleString item = value.substr(offset, end - offset);
    const WCHAR whitespace[] = {static_cast<WCHAR>(' '), static_cast<WCHAR>('\t'), static_cast<WCHAR>('\r'),
                                static_cast<WCHAR>('\n'), static_cast<WCHAR>(0)};
    const size_t first = item.find_first_not_of(whitespace);
    const size_t last = item.find_last_not_of(whitespace);
    if (first != ModuleString::npos) {
      modules_.push_back(item.substr(first, last - first + 1));
    }
    if (separator == ModuleString::npos)
      break;
    offset = separator + 1;
  }
}

bool ModuleAllowlist::Allows(ICorProfilerInfo10* info, ModuleID module_id) const {
  if (modules_.empty())
    return false;

  ULONG name_length = 0;
  LPCBYTE base_address = nullptr;
  AssemblyID assembly_id = 0;
  DWORD flags = 0;
  HRESULT status = info->GetModuleInfo2(module_id, &base_address, 0, &name_length, nullptr, &assembly_id, &flags);
  if (FAILED(status) || name_length == 0)
    return false;

  ModuleString path(name_length, static_cast<WCHAR>(0));
  status = info->GetModuleInfo2(module_id, &base_address, name_length, &name_length, path.data(), &assembly_id, &flags);
  if (FAILED(status))
    return false;
  if (!path.empty() && path.back() == static_cast<WCHAR>(0))
    path.pop_back();

  const WCHAR separators[] = {static_cast<WCHAR>('\\'), static_cast<WCHAR>('/'), static_cast<WCHAR>(0)};
  const size_t slash = path.find_last_of(separators);
  ModuleString module_name = slash == ModuleString::npos ? path : path.substr(slash + 1);
  const size_t extension = module_name.find_last_of(static_cast<WCHAR>('.'));
  if (extension != ModuleString::npos)
    module_name.resize(extension);

  for (const ModuleString& allowed : modules_) {
    const bool wildcard = allowed.size() == 1 && allowed.front() == static_cast<WCHAR>('*');
    if (wildcard || EqualModuleName(allowed, module_name))
      return true;
  }
  return false;
}

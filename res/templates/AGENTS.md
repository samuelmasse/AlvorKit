# Template Instructions

These instructions apply to templates under this directory.

Use `RepositoryTemplates` from `AlvorKit.Script.Workspace` for repository-file
templates in script tools. Prefer an area-scoped `RepositoryTemplateSet`, such
as `RepositoryTemplates.ForArea(typeof(MyEmitter), "maths")`, so call sites pass
short template names instead of full `res/templates/...` paths.

Keep source-generator templates that must be packaged into analyzers on their
embedded-resource path. Align behavior with the script renderer when practical,
but do not add script-tool dependencies to analyzer projects.

Follow `README.md` for naming, fragment, and generated-output review rules.
Generated-output review for template changes is a Commit Mode gate; in Working
Mode, generate or inspect output only when it helps iteration.

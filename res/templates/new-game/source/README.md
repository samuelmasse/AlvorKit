# Alvor Starter

Alvor Starter is an AlvorKit game scaffold. It references the sibling
`../AlvorKit` checkout directly and starts with:

- a low-level OpenGL color triangle,
- direct `RootSprites` sprite-batch drawing,
- a Blend UI menu with a click counter.

The project split follows `../AlvorKit/docs/ProjectSplitModel.md`:

- `AlvorStarter.App`: pure app scope and app state,
- `AlvorStarter.App.Frontend`: GL, sprite drawing, and client presentation,
- `AlvorStarter.Menus`: menu UI, menu styling, and state glue,
- `AlvorStarter`: executable startup and engine loop ownership.

Keep the split real in `.csproj` files. Pure packages should not reference UI,
OpenGL, windowing, audio, menu, or frontend packages. Frontend packages should
use `AlvorKit.Engine`, not `AlvorKit.Engine.Loop`; loop ownership belongs in the
executable or menu/composition layer.

Run it from the repository root:

```powershell
dotnet run --project src\AlvorStarter
```

For visual automation, read `../AlvorKit/docs/AlvorSense.md` and run:

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- start --id AlvorStarter --project src\AlvorStarter --workdir .
```

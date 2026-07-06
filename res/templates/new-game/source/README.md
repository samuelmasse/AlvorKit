# Alvor Starter

Alvor Starter is an AlvorKit game scaffold. It references the sibling
`../AlvorKit` checkout directly and starts with:

- a low-level OpenGL color triangle,
- direct `RootSprites` sprite-batch drawing and text,
- a Blend UI menu with a click counter.

The project split follows the small game shape used by AlvorPong:

- `AlvorStarter.App`: app scope, style, app-wide services, and starter rendering,
- `AlvorStarter.Menus`: menu UI and state glue,
- `AlvorStarter`: executable startup.

Run it from the repository root:

```powershell
dotnet run --project src\AlvorStarter
```

For visual automation, read `../AlvorKit/docs/AlvorSense.md` and run:

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- start --id AlvorStarter --project src\AlvorStarter --workdir .
```

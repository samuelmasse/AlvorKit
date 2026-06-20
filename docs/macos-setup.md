# macOS setup

This guide bootstraps a fresh macOS machine for building and running AlvorKit.
It assumes an Apple Silicon Mac. On Intel Macs, Homebrew normally installs under
`/usr/local` instead of `/opt/homebrew`; adjust paths accordingly.

## 1. Install Apple Command Line Tools

Install Apple's compiler and Git toolchain:

```sh
softwareupdate --list
softwareupdate --install "Command Line Tools for Xcode <version-from-list>"
```

If `softwareupdate --list` does not show Command Line Tools, start the
interactive installer instead:

```sh
xcode-select --install
```

Verify:

```sh
git --version
xcode-select -p
```

`xcode-select -p` should print `/Library/Developer/CommandLineTools`.

## 2. Install Homebrew

Install Homebrew from the official installer:

```sh
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Add Homebrew to zsh login shells:

```sh
echo 'eval "$(/opt/homebrew/bin/brew shellenv zsh)"' >> ~/.zprofile
eval "$(/opt/homebrew/bin/brew shellenv zsh)"
```

Verify:

```sh
brew --version
```

## 3. Install required developer tools

Install the .NET SDK and Node.js:

```sh
brew install dotnet node
```

Add the .NET root expected by some tools:

```sh
echo 'export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"' >> ~/.zprofile
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
```

Verify:

```sh
dotnet --version
node --version
npm --version
```

AlvorKit currently targets `net10.0`, so `dotnet --version` must be a .NET 10
SDK. Node is required by the repository linter, which shells out to `npx`.

## 4. Optional: install GitHub CLI

This is not required if the repository is already cloned, but it is the cleanest
way to authenticate a fresh machine for private GitHub repositories:

```sh
brew install gh
gh auth login --hostname github.com --git-protocol ssh --web
```

If the machine has no SSH key yet, allow `gh` to generate and add one, or create
one explicitly:

```sh
mkdir -p ~/.ssh
chmod 700 ~/.ssh
ssh-keygen -t ed25519 -C "github" -f ~/.ssh/id_ed25519_github
gh ssh-key add ~/.ssh/id_ed25519_github.pub --title "$(hostname)-github"
```

Verify SSH access:

```sh
ssh -T git@github.com
```

## 5. Clone the repository

Clone with SSH:

```sh
mkdir -p ~/repos
git clone git@github.com:<owner>/AlvorKit.git ~/repos/AlvorKit
cd ~/repos/AlvorKit
```

Set a Git identity if this is a new machine:

```sh
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```

## 6. Generate local bindings

The checked-in source can use published binding packages, but active development
often needs generated bindings under `out/bindgen`. Generate all local bindings:

```sh
dotnet run --project scripts/AlvorKit.Script.Bindgen -- all
```

This downloads upstream native sources and emits binding projects under
`out/bindgen`. Projects automatically use each exact local generated binding
project when it exists and otherwise fall back to the pinned package. The bindgen
C header parser uses ClangSharp runtime NuGet packages, so a separate Homebrew
LLVM install should not be necessary.

## 7. Build

Build the whole solution:

```sh
dotnet build AlvorKit.slnx
```

A successful build ends with:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## 8. Run smoke tests

Run the simple OpenGL demo:

```sh
dotnet run --project demos/AlvorKit.OpenGL.Demo.HelloTriangle/AlvorKit.OpenGL.Demo.HelloTriangle.csproj
```

Run the combined FreeType, OpenGL, and MiniAudio demo:

```sh
dotnet run --project demos/AlvorKit.Demo/AlvorKit.Demo.csproj
```

On macOS, demos that use GLSL `#version 330 core` must request a modern OpenGL
core context before creating the GLFW window:

```csharp
glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
glfw.WindowHint(GlfwWindowHint.OpenGLForwardCompat, true);
```

Without those hints, GLFW may create the default macOS OpenGL 2.1 context, which
cannot compile GLSL 330 shaders.

## 9. Run lint after changes

Use scoped linting for files you edited:

```sh
dotnet run --project scripts/AlvorKit.Script.Lint -- --include "path/to/file"
```

For example:

```sh
dotnet run --project scripts/AlvorKit.Script.Lint -- --include "demos/AlvorKit.Demo/Program.cs"
```

The linter may download npm tools through `npx` on first use.

## Troubleshooting

If `dotnet` is not found in a new terminal, confirm Homebrew and `DOTNET_ROOT`
are present in `~/.zprofile`, then open a new login shell or run:

```sh
eval "$(/opt/homebrew/bin/brew shellenv zsh)"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
```

If bindgen fails with `Unable to load shared library 'libclang'`, restore the
bindgen project and verify the macOS runtime packages are referenced:

```sh
dotnet restore scripts/AlvorKit.Script.Bindgen.CHeaders/AlvorKit.Script.Bindgen.CHeaders.csproj
```

For Apple Silicon, the project should reference:

```xml
<PackageReference Include="libclang.runtime.osx-arm64" Version="21.1.8" />
<PackageReference Include="libClangSharp.runtime.osx-arm64" Version="21.1.8.2" />
```

If an OpenGL demo reports `OpenGL 2.1` and then fails to compile `#version 330`
shaders, add the GLFW 3.3 core-profile hints before `CreateWindow`.

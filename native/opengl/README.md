# AlvorKit.OpenGL

OpenGL bindings generated from the Khronos [OpenGL registry](https://github.com/KhronosGroup/OpenGL-Registry) (gl.xml): the 4.6 core profile command surface with typed enum groups.

Unlike the other AlvorKit native libraries there is no `.Native` runtimes package - the OpenGL implementation ships with the platform's graphics driver. `GlBackend.Load` resolves every entry point at runtime through a proc loader such as `Glfw.GetProcAddress`, after the GL context is current.

`TAG` pins the registry commit the bindings are generated from. The package version is the bound GL version with an AlvorKit packaging revision (`4.6.N`).

# OpenGL maths overloads

`AlvorKit.OpenGL.Maths` adds allocation-free OpenGL extension methods for
AlvorKit vectors, matrices, quaternions, intervals, and spatial values. The
generated `AlvorKit.OpenGL` API remains available for calls that are naturally
scalar or need exact control over the native arguments.

The extensions use the `AlvorKit.OpenGL` namespace and accept `Gl`, so they work
with both a raw `Gl` instance and `GlLayer`:

```csharp
using AlvorKit.Maths;
using AlvorKit.OpenGL;

gl.Viewport(canvas.Size);
gl.ClearColor(new Vec4(0.08f, 0.12f, 0.18f, 1));
gl.DepthRange(new Intervald(0, 1));
```

Add `AlvorKit.OpenGL.Maths` directly when using the raw OpenGL package. Projects
that reference `AlvorKit.OpenGL.Layer` receive the maths package transitively.
Calls made through a `GlLayer` receiver still pass through its virtual
validation, state tracking, and backend dispatch.

## Shader values

Vector and quaternion overloads are available for current-program and
direct-program uniforms. Single matrices use an `in` parameter, and matrix
arrays use `ReadOnlySpan<T>`:

```csharp
Vec3 lightDirection = (0.2f, -1, 0.4f);
gl.ProgramUniform3f(program, lightLocation, lightDirection);

Mat4 modelViewProjection = projection * view * model;
gl.ProgramUniformMatrix4fv(program, matrixLocation, in modelViewProjection);

ReadOnlySpan<Mat4> jointMatrices = animation.JointMatrices;
gl.UniformMatrix4fv(jointLocation, jointMatrices);
```

The matrix overloads cover every 2-to-4-column by 2-to-4-row float and double
matrix shape. AlvorKit matrices are column-major, so the overloads always pass
`transpose: false`. They reinterpret the existing matrix storage without a
temporary buffer. The number of matrices comes from the matrix span length,
not its scalar length.

`Quat` and `Quatd` upload their contiguous `(X, Y, Z, W)` components through
the four-component uniform calls. GLSL has no quaternion type; these overloads
do not normalize, reorder, or otherwise convert the value.

## Regions, images, and framebuffer operations

Origins and extents remain distinct. Signed `Vec2i` or `Vec3i` values describe
origins, while unsigned `Vec2u` or `Vec3u` values describe sizes:

```csharp
Vec2i destination = (atlasX, atlasY);
Vec2u glyphSize = glyph.Size;
gl.TexSubImage2D(
    GlTextureTarget.Texture2D,
    level: 0,
    destination,
    glyphSize,
    GlPixelFormat.Red,
    GlPixelType.UnsignedByte,
    glyph.Pixels);

gl.RenderbufferStorage(
    GlRenderbufferTarget.Renderbuffer,
    GlInternalFormat.DepthComponent24,
    framebufferSize);

gl.ReadPixels(
    framebufferSize,
    GlPixelFormat.Rgba,
    GlPixelType.UnsignedByte,
    pixels);
```

OpenGL represents these extents with signed `GLsizei` values. A component above
`int.MaxValue` throws `OverflowException` during checked conversion before
calling the backend; the extensions never clamp or wrap it. The zero-origin
`Viewport`, `Scissor`, and `ReadPixels` overloads are conveniences for the
common full-target case.

`Viewport(Vec2u)` intentionally accepts a size, not AlvorKit's `Viewport`
camera/projection value. There is no `Viewport(Viewport)` or `Box2i` overload,
because those domain types do not represent the exact `(origin, extent)`
contract of `glViewport`.

Packed `ScissorArrayv` values use `Vec4i` in `(x, y, width, height)` order and
are forwarded unchanged; OpenGL determines whether signed values are valid.
Blit overloads support either explicit `Vec4i` endpoint vectors or
origin-plus-size pairs. A size may exceed `int.MaxValue` when adding it to a
negative origin still produces a signed endpoint. An unrepresentable calculated
endpoint throws `OverflowException`; explicit endpoint forms are forwarded
exactly, including reversed regions.

## Vertex attributes

Generic vertex declaration overloads derive component count and OpenGL storage
type from the maths vector:

```csharp
gl.VertexAttribPointer<Vec3>(positionIndex, false, Vertex.Size, 0);
gl.VertexAttribPointer<Vec2>(textureIndex, false, Vertex.Size, 6 * sizeof(float));

gl.VertexAttribIPointer<Vec4u8>(colorIndex, Vertex.Size, colorOffset);
gl.VertexAttribLPointer<Vec3d>(positionIndex, Vertex.Size, positionOffset);
```

The normal pointer/format family supports two-, three-, and four-component
vectors of `float`, `double`, `Half`, signed and unsigned 8-bit integers,
signed and unsigned 16-bit integers, and signed and unsigned 32-bit integers.
The integer family supports those integer vectors, and the long family supports
double vectors. Unsupported types throw `NotSupportedException` before the raw
OpenGL call.

The same type mapping is available for `VertexAttribFormat`,
`VertexArrayAttribFormat`, and their integer and long variants. Constant vertex
values accept float, double, signed-int, and unsigned-int vectors. The bridge
does not infer normalized or packed formats because those are shader and data
layout policy; callers continue to select them explicitly through the raw API.

## Other supported shapes and raw calls

The package also provides maths forms for color masks, blend colors, viewport
and depth-range arrays, texture allocation and copies, image binding, compute
dispatch, and direct-state-access texture/framebuffer operations. Existing
buffer upload calls already accept unmanaged maths structs as span payloads, so
the bridge does not duplicate them.

Raw instance methods remain the escape hatch for scalar dimensions, one-value
uniforms, explicit matrix transposition, packed vertex formats, enum-dependent
vector arities, or any OpenGL operation without an exact maths shape. C# gives
instance methods precedence over extension methods, so adding the package does
not change how existing raw calls bind.

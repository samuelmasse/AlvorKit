# Engine package review

Reviewed on November 19, 2026. Items 1–5, 7, and 9 are complete. Items 6, 8,
and 11 are deferred. Items 10 and 12 are dropped. The numbers match the
original review.

1. **Complete — script dispatch.** Disposing a UI surface can unregister a
   script during a callback. An active pass skips removed registrations and
   finishes in order. Additions join the next outermost pass. Dispatch depth
   is decremented on normal return. Tests cover mutation, nesting, and
   allocation-free steady-state traversal.
   [Script registry](../src/AlvorKit.Engine.States/RootScripts.cs)

2. **Complete — shutdown handlers.** `RootEngine.Unload` unregisters all three
   process-wide handlers before tearing down the window. Process callbacks
   use the shutdown state to end window access. The old monitor query and its
   catch-all are gone. An unhandled exception remains fatal.
   [Engine unload](../src/AlvorKit.Engine.Loop/RootEngine.cs) ·
   [Failure semantics](AgentRules/GameCodeDesign.md#failure-semantics)

   GLFW restricts monitor work-area queries to the main thread and permits
   getting and setting the window-close flag from any thread.
   [Shutdown callbacks](../src/AlvorKit.Engine.Loop/RootShutdown.cs) ·
   [GLFW backend](../src/AlvorKit.Windowing.Glfw/GlfwWindowSizes.cs) ·
   [GLFW monitor reference](https://www.glfw.org/docs/latest/group__monitor.html) ·
   [GLFW window reference](https://www.glfw.org/docs/latest/group__window.html)

3. **Complete — frame timing.** The first tick's last, average, and maximum
   agree. Every completed tick contributes to the rolling statistics, including
   ticks that share a monotonic timestamp. The window samples the trailing
   interval on its owner's thread when read. A one-second idle interval
   reports zero ticks. The frame counter still reports the lifetime total.
   [Tick measurement](../src/AlvorKit.Engine.Frontend/Metrics/TickMetric.cs) ·
   [Window publication](../src/AlvorKit.Engine.Frontend/Metrics/TickMetricWindow.cs) ·
   [Current assertions](../tests/AlvorKit.Engine.Test/TickMetricTest.cs)

4. **Complete — texture-cube controls.** The demo resolves `Controls.toml`
   through its injected `RootFiles` before calling the TOML loader. It can
   launch from the repository root with its assets in the output directory.
   [Texture-cube demo](../demos/AlvorKit.Engine.Demo.TextureCube/Program.cs)

5. **Complete — quad buffer capacity.** An eight-vertex request reuses an
   eight-vertex buffer. The test covers zero, exact capacity, and growth.
   The engine and Graphics2D buffers retain their respective GL lifetimes.
   [Engine buffer](../src/AlvorKit.Engine.Frontend/Buffers/RootQuadIndexBuffer.cs) ·
   [Graphics2D buffer](../src/AlvorKit.Graphics2D/Quad/QuadIndexBuffer.cs)

6. **Defer the GL split until it removes a dependency.** SHROOM's ASCII and
   cube rendering libraries both reference `Engine.Frontend`. They use
   `RenderProgram<T>`, `IVertex`, and `SharedVertexBuffer` with a caller-supplied
   `GlLayer`. Check whether an existing GL package can own the shared types.
   Removing the engine reference from both libraries is the acceptance test.
   A new package and a wholesale move of vertex layouts are premature.
   [Render program](../src/AlvorKit.Engine.Frontend/Rendering/RenderProgram.cs) ·
   [Shared vertex buffer](../src/AlvorKit.Engine.Frontend/Buffers/SharedVertexBuffer.cs)

7. **Complete — multi-turn camera rotation.** Vector modulo wraps all three
   rotation components, including positive and negative multi-turn deltas.
   `ComputeVectors` documents the yaw and pitch used for view directions.
   [Camera](../src/AlvorKit.Engine.Frontend/Geometry/Camera3D.cs)

8. **Defer PNG row-order work.** Craftdig decodes a window icon and flips
   the rows back to the order its window host expects. The icon is correct.
   If that startup copy matters, fix the decode order at the icon boundary.
   There is no demonstrated need to move `ImageData` and the decoder into a new package.
   [PNG decoder](../src/AlvorKit.Engine.Frontend/RootPngs.cs)

9. **Complete — game-facing noise.** Desktop and headless hosts share
   `FnGraph` with child scopes. The host constructs the backend at the
   composition boundary. If raw `Fn` injection is deliberately removed from
   that surface later, the change is one registration and its binding assertion.
   The dedicated binding verifier can construct the backend directly.
   [Root host](../src/AlvorKit.Engine.Root.Host/RootHost.cs) ·
   [Binding tests](../tests/AlvorKit.Engine.Test/RootLoopBindingTest.cs)

10. **Drop the deferred-deletion move.** Craftdig texture finalizers enqueue
    GL deletion and the render thread drains the queue. That handoff has a
    concrete owner and purpose. Rename `RootBinEmptyer` when doing substantive
    work there if the name is still in the way.
    [Deletion queue](../src/AlvorKit.Engine.Frontend/GlBin.cs)

11. **Defer the LiveCode draw optimization.** LiveCode already pumps through
    `window.Dispatch` and unsubscribes during unload. It also inherits an empty
    draw callback that the renderer wraps in sprite-batch setup and teardown.
    If profiling warrants removing that cost, use the existing dispatch and
    unload hooks. A new script hierarchy is unnecessary.
    [LiveCode script](../src/AlvorKit.Engine.LiveCode/RootLiveCodeScript.cs) ·
    [Sprite passes](../src/AlvorKit.Engine.Loop/RootGraphics2D.cs)

12. **Drop the standalone rename pass.** Renaming `Engine.Loop`, `RootMetrics`,
    and `RootRoboto` would churn callers without changing ownership or behavior.
    Revisit individual names when a change to their responsibility warrants it.

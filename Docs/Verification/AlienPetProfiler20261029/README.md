# Alien Pet — native OPPO Profiler capture, 29 October 2026

## What was measured

Actual Android-player binary captures were loaded into **Unity Profiler** and
**Profile Analyzer 1.4.0**, not simulated Editor frame times. All five captures
were pulled into the Analyzer, analyzed and saved as `.pdata`. The GUI remains
open with the level-53 capture showing the long-frame spikes. There are **2,618 recorded frames** across levels
2, 7, 23, 53 and 65, approximately 95 seconds of captured gameplay.

- OPPO CPH2591, Android 15, Mali-G52 MC2, Vulkan, 720 × 1612.
- Unity 6000.3.19f1, ARM64 IL2CPP Development; Deep Profile and Script Debugging off.
- APK SHA-256: `5fefc616cc6975382134152ac710f850235151f09e6f3dd0426a1ac4b13f6e0c`.
- `NewGraphic`, HEAD `afc640dc`, including the existing uncommitted branding/test work.
- Normal 32-particle body, 120 Hz physics, approved Glass C art and render settings.
- Target frame rate 60. Full render scale, 4× MSAA, 2048 main shadow map, one cascade;
  no HDR or post-processing. No quality or gameplay optimization was applied here.
- Opt-in author replay issues real InputSystem touches and preserves saved progress.
  Five seconds of scene settling precede each capture. No screenshot/video capture
  runs during profiling. Each trace is at most 25 seconds; it is not the entire solve.
- Sampled battery temperature 38–40 °C, Android thermal status 1–2, charging via USB.
  This is a warm-device diagnostic, not a controlled cold-device comparison.

## Findings

Mean inclusive milliseconds per **recorded frame**, including capture boundaries:

| Level | Frames | Frame mean / p95 | Body mesh | Scripted gameplay | PhysX | Graph rebuild |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 2 | 264 | 27.43 / 41.86 | 3.99 | 2.01 | 5.63 | none in window |
| 7 | 738 | 33.89 / 45.85 | 4.65 | 3.57 | 6.95 | 0.13 |
| 23 | 378 | 33.88 / 45.61 | **9.27** | 3.61 | 4.94 | 0.21 |
| 53 | 583 | 42.89 / 56.38 | 8.24 | **11.12** | 6.00 | **3.05** |
| 65 | 655 | 38.21 / 48.44 | 5.73 | **8.79** | 7.03 | 0.53 |

These are diagnostic Development timings, **not a new Release FPS result**. The
previous 30.1 FPS full-campaign result and these short traces use different builds,
sampling and temperatures; they are not an optimization before/after comparison.

### 1. Expensive graph edges cause visible stalls

Level 53 (N43) rebuilt its graph nine times in eight frames:

- Mean **197.74 ms per build**, maximum **206.50 ms**.
- Edge construction/validation accounts for **98.33%** of graph time, averaging
  194.43 ms per build. Nodes average 2.33 ms, snapshot 0.65 ms, transform sync 0.012 ms.
- Frame index 305 has two builds totaling **392.23 ms**. The whole frame lasts
  **446.90 ms** and contains 12 fixed ticks. This is a concrete long-frame cause.
- Six builds occur inside `COghe.Gameplay.Step`; three occur outside it, in the
  update/input path. Do not add all graph time to the gameplay column above.
- Route search itself averages only 0.047 ms per recorded frame in this trace.
  The existing graph cache is useful; each remaining cache miss is too expensive.

### 2. Body construction consumes a large continuous CPU budget

Level 23 spends **9.27 ms/frame** in `COghe.Skin.Rebuild` (maximum call 15.27 ms).
For a 60 FPS target, the complete frame has only 16.67 ms. The current marker does
not separate field evaluation, polygonization, surface constraints and mesh upload;
that breakdown is the next measurement, not an already proven sub-cause.

### 3. Scripted gameplay needs a narrower breakdown before changing physics

Gameplay self time per fixed tick is 0.83 ms at level 23, 1.87 ms at 53 and
1.82 ms at 65. PhysX time per tick is respectively 1.21, 1.24 and 1.53 ms.
The increase at level 53 is mainly in scripted work, not a comparable increase in
PhysX per tick. At lower FPS, more 120 Hz ticks accumulate within each frame.
This evidence does not justify reducing physics frequency, solver quality or CCD.

### 4. Rendering waits are not GPU measurements

At level 53 the render thread spends 26.11 ms/frame in a semaphore wait under
`Gfx.WaitForGfxCommandsFromMainThread`: it is waiting for main-thread commands.
Main-thread waiting for render/present is only 0.31 ms/frame. This supports a
CPU-first investigation for that window.

The render pipeline still consumes about 6.12 ms/frame of main-thread CPU time.
`GfxDeviceVK.Present` is about 11.04 ms/frame on the render thread, but present
includes synchronization/pacing; it is not a shader execution timer. Threads and
parent/child markers overlap and must not be added as independent costs.

All GPU frame fields are blank and report
`NotSupportedByGraphicsAPI, NotSupportedWithVulkan`. Unity's GPU Profiler does not
support Android Vulkan: [official Unity GPU profiling documentation](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/profile-rendering/profiler-gpu).
Unavailable GPU time is not zero GPU work. No GPU bottleneck has been established.

## Measurement limits and replay outcomes

The replay uses the developer HUD. At level 53 its `COgheDayLabPresentation.OnGUI`
costs about 1.66 ms/frame inclusive; this code returns when the normal Product UI
exists. `Profiler.FlushMemoryCounters` also costs about 0.72 ms/frame. Do not
subtract these averages and claim a predicted Product FPS: pacing and workload
change, and Product has its own UI and saved creature style.

Levels 2, 23, 53 and 65 completed their normal authored solve. Level 7's replay
stopped at “Cross to receiving plinth”; its trace is diagnostic work, not a passed
solve. It passed the previous Release sweep. The Development capture/no-screenshot
route has different timing; this run alone does not establish a gameplay regression.
The error collection contains no Unity Error/Exception/Assert. The replay monitor
was restarted after a host-side attempt to pull `run.json` before it existed;
the native replay and binary capture continued uninterrupted. Health timestamps
therefore contain a monitor restart, not a continuous elapsed clock.

After capture, the normal Release APK was restored without clearing app data:
SHA-256 `c715f25e611b4716186c1de15a66ff3ba3402068921fddba0f0e3f615e88e0a9`.

## Evidence and repeatability

- `summary.json`: compact native metrics, graph event frames and raw-file hashes.
- `measurements.csv`: table above at full precision.
- Local ignored evidence: `Artifacts/AlienPetProfiler-2026-10-29/native-capture/`.
  Each level has a `.raw`; `analysis/level-XX/` has `.pdata`, frame/thread/marker CSVs
  and an analysis summary. Preserve `.raw` as well as `.pdata` for Profiler timelines.
- `Artifacts/AlienPetProfiler-2026-10-29/editor-profiler.log` confirms all five
  native captures were successfully processed by Profile Analyzer.
- Runtime markers are inactive without profiling; capture is opt-in and requires
  a Development build. No Deep Profile, gameplay, art or quality change was needed.

Build the separate diagnostic APK:

```sh
COGHE_BENCHMARK=1 COGHE_PROFILE=1 bash Tools/build-venom-android.sh --spatial
```

Run `coghe_view_proof` with `coghe_spatial=true`, a level subset, and
`coghe_profiler_capture=true` / `coghe_profiler_seconds=25`. Pull the resulting
`files/view-proof/<timestamp>/level-XX.raw` files after `COGHE_PROFILE_END`.
Set `COGHE_PROFILE_DIRECTORY` before starting Unity and use
`GravityBox.Editor.COgheProfilerAnalysis.LoadAndAnalyzeDirectory` as the
execute method, or the corresponding **Gravity Box → COghe → Profiler** menu.
Do not pass `-quit`: the Analyzer initializes through the normal Editor loop.

See [the optimization plan](../../../PLANS/ALIEN_PET_MOBILE_PERFORMANCE_PLAN.md).

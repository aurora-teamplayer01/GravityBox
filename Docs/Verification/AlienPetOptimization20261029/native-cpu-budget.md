# Retained native capture: CPU budget and remaining bottlenecks

Generated 2026-10-29T09:02:09.146573+00:00. Scope: canonical 15 before + 15 retained after native captures only. No Unity/ADB/runtime action. Balanced Release results are separate and were pending during this analysis.

The evidence is consistent with saved main-thread work becoming partly additional waits; it does not demonstrate 60 FPS or isolate a GPU bottleneck. Baseline native captures were hotter (39–41°C/status 2) than retained captures (36–38°C/status 1–2), so delta magnitudes are not code-only effects.

The table below uses frame-weighted totals across three captures per side. Units are milliseconds per captured frame; the first three columns form an additive partition apart from the small uninstrumented residual. All captured frames are included.

| Level | Main frame elapsed B→F | Explicit wait/sync SELF B→F | Remaining attributed elapsed B→F | Skin inclusive B→F | Gameplay Step inclusive B→F | Physics.Simulate inclusive B→F |
|---|---:|---:|---:|---:|---:|---:|
| 23 | 34.55→33.45 | 3.59→6.89 | 30.84→26.44 | 9.94→6.78 | 3.80→3.48 | 5.14→5.03 |
| 53 | 43.95→40.70 | 2.95→6.13 | 40.89→34.46 | 8.76→6.15 | 11.82→10.08 | 6.22→5.59 |
| 65 | 38.23→37.23 | 4.35→7.12 | 33.76→30.00 | 5.56→4.62 | 8.60→7.61 | 6.95→6.33 |

Main-thread self totals miss only 0.099–0.129 ms/frame across individual captures; all 30 exports report zero invalid/negative-self samples. The remaining elapsed column includes profiler/driver/managed/native execution outside explicit waits; it is not guaranteed hardware CPU busy. Profiler.FlushMemoryCounters alone is roughly 0.60–0.73 ms/frame in the retained captures.

Wait accounting uses SELF only for: `WaitForTargetFPS`, `Semaphore.WaitForSignal`, `WaitForJobGroupID`, `WaitForRenderJobs`, `Idle`, `TimeUpdate.WaitForLastPresentationAndUpdateTime`, `Gfx.WaitForPresentOnGfxThread`, `WaitForCompleted`. `WaitForJobGroupID` inclusive can contain child work; `Gfx.WaitForPresentOnGfxThread` has near-zero self while its semaphore child holds most of the wait. Their inclusive values must not be summed.

At levels 23/53/65, pooled skin decreases 3.16/2.61/0.94 ms while explicit waits increase 3.30/3.18/2.77 ms. Whole-frame elapsed decreases only 1.10/3.24/1.00 ms. This association supports partial conversion to waits, without proving that each saved skin millisecond caused a wait.

| Level | Main FinishFrameRendering inclusive B→F | Main WaitForTargetFPS inclusive B→F | Render-thread GfxDeviceVK.Present B→F | Render-thread Semaphore.WaitForSignal B→F |
|---|---:|---:|---:|---:|
| 23 | 7.13→7.24 | 2.26→4.96 | 13.52→16.31 | 16.00→12.22 |
| 53 | 7.17→6.83 | 2.00→4.91 | 9.62→14.37 | 28.61→21.01 |
| 65 | 6.98→6.48 | 2.96→5.63 | 14.43→16.45 | 18.32→15.87 |

This rendering table is context, not another additive budget: FinishFrameRendering is inclusive of rendering/submission and some waits; the Render Thread runs in parallel. GfxDeviceVK.Present is nested under Gfx.PresentFrame. Present/queue waits are not GPU execution timestamps. The CSV export drops sample starts/parent links, so it cannot reconstruct the exact cross-thread critical path.

The retained three-run medians still show 29.91/24.35/26.81 FPS for levels 23/53/65; p95 frame times are 48.24/53.56/51.42 ms. At observed cadence, the 120 Hz simulation averages 4.02/4.63/4.47 ticks per frame (pooled). At 60 Hz it would average 2 ticks, so do not turn today’s remainder into a hard theoretical 60 FPS bound. Source still requests 60 FPS; no hidden 30 FPS cap was found.

The justified next investigations are:

- Inspect the raw native Timeline critical path with matched-temperature runs and frame presentation timing, especially WaitForTargetFPS versus render-thread Present/queue synchronization. Collect supported platform GPU execution/clock data before deciding whether a GPU bottleneck exists. Keep graphics/presentation settings fixed.
- Separate recurrent script/physics cost from fixed-update catch-up: retained Step/Physics means are 3.48/5.03 ms at 23, 10.08/5.59 ms at 53, and 7.61/6.33 ms at 65 (observed frame cadence). Motion alone has median 5.69/5.08 ms per frame at 53/65. These are diagnostic targets, not permission to alter 120 Hz or puzzle forces.
- On 23, Polygonize remains 5.34 ms/frame median inside 6.88 ms Skin.Rebuild; field and upload are only 0.38/0.34 ms. Use the existing submarkers to inspect its longest frames before choosing a next algorithm change.
- At 53, unchanged navigation still makes 9 calls per 25-second capture: up to 202.45 ms/call and 397.44 ms accumulated in one frame when two calls coincide. Record exact rebuild reasons/order and which calls fall inside mechanism steps versus input. Do not add graph durations to Gameplay.Step totals, because they overlap; do not reintroduce the rejected cache without new evidence.

Limitations: the native player is a Development/Profiler build with profiler overhead; L53/65 are prefixes, not identical whole-route windows. Source equivalence restores original navigation but does not remove the native thermal confound. Final Release acceptance must use the separately balanced cohort.

Provenance and machine-readable per-capture/run statistics: [native-cpu-budget.json](native-cpu-budget.json). Companion [retained-profiler-review.md](retained-profiler-review.md) verifies canonical sources and the actual Profile Analyzer comparison. Exporter: `Assets/_Game/Editor/COgheProfilerAnalysis.cs:167–174,219–228`. Runtime phase boundaries: `VenomSurface.cs:99–151`, `VenomLevelController.cs:115–122`; 120 Hz contract: `PhysicsTiming.cs:7–11`.

Unity marker semantics reference: https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-markers.html

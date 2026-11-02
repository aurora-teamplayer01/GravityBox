# Alien Pet mobile performance plan — 29 October 2026

Status: **selected CPU changes implemented; final native verification in progress**.
The navigation cache experiment was removed after device measurements did not
establish a reliable benefit. The retained candidate optimizes body construction
and repeated per-tick work, keeping the original navigation algorithm and all
visual/physics quality settings. Final evidence: [optimization verification](../Docs/Verification/AlienPetOptimization20261029/README.md).
Evidence: [native OPPO Profiler + Profile Analyzer](../Docs/Verification/AlienPetProfiler20261029/README.md)
and the [65-level Release audit](../Docs/Verification/AlienPetOppo20261029/README.md).

The display's 60 Hz capability is not a guarantee of 60 game frames per second.
The target is 16.67 ms per frame with thermal headroom. Current evidence identifies
large CPU costs and discrete navigation stalls; it does not establish how much GPU
headroom exists or promise a twofold improvement from a single change.

## 1. Establish comparable Product captures

Use levels 2, 7, 23, 53 and 65, plus Home/monster animation and a reward/ability
sequence. Record build hash, graphics API, resolution, quality, saved appearance,
power state, refresh rate and temperature. Reproduce the same input/action windows
three times from a similar starting temperature. Fix the level-7 author route's
diagnostic timing before treating it as a performance regression scenario.

Capture short Development traces with the normal Product UI and separate harness
work from gameplay. Use Release, without profiler/screenshot/video capture, for
FPS acceptance. Keep author proof runs save-protected. Extend narrow markers around
graph candidate generation/visibility checks, `Matter.Step`, `Motion.Step`,
mechanisms, skin field/polygonization/constraints/upload. Avoid whole-game Deep Profile.

## 2. Remove navigation stalls first

Measured: level 53 has nine graph builds averaging 198 ms, 98.3% in edge work;
one frame contains two builds and lasts 447 ms. Existing graph caching, spatial
grid, BVH and heap search already exist; reintroducing those is not a solution.

First experiments, separately measured:

1. Record candidate pairs, BVH nodes visited, exact checks and invalidation reasons.
   Determine whether the two builds in one frame reflect two legitimately different
   mechanism states; coalesce only truly identical revisions.
2. Reuse neighbouring bucket lists per occupied cell instead of repeating all
   27 dictionary lookups for every node. Preserve deterministic edge ordering.
3. Try conservative segment-versus-expanded-bounds rejection within the existing
   navigation BVH before exact shape checks. Preserve endpoint/margin semantics.
4. If still expensive, retain static edge visibility and revalidate only edges
   affected by moved blockers, with a full-rebuild fallback. This is a later,
   higher-risk change, not the first patch.

Do not blindly loosen motion thresholds, reuse stale paths through closed doors,
or prioritize transform synchronization: it was only 0.012 ms per graph build.
Validate identical nodes/edges/routes against the reference builder, then solve,
wander, thin-clearance, wall/roof shortcut and mechanism-revision tests. Proposed
target: eliminate navigation-caused frames over 50 ms in the measured route; refine
the per-build budget after the narrow trace rather than promising it in advance.

## 3. Reduce body construction cost while preserving its appearance

Measured: level 23 body rebuild averages 9.27 ms/frame, level 53 8.24 ms/frame.
Use sub-markers to choose between field, polygonization, constraints and mesh upload.

- If empty voxel work dominates, process/reset affected tiles or cells only.
- If upload/bounds dominate, retain buffers and calculate bounds from emitted
  vertices instead of a separate scan, where equivalent.
- Consider Jobs/Burst only for a demonstrated data-parallel hot loop after the
  smaller changes; avoid allocations and synchronization that erase the benefit.

Keep the approved mesh density and shape, surface contact, ink/material, deformation,
split/merge silhouette and update cadence. Compare close-up motion on glass, tubes,
corners and the Home monster head. An initial experimental target is approximately
30% less skin CPU on the same route; this is a goal, not an expected measured gain.

## 4. Reduce per-tick gameplay work without changing the puzzle

Measured: scripted gameplay self cost per tick is about 1.87 ms on level 53 and
1.82 ms on 65, versus 0.83 ms on 23. PhysX per tick is much closer between 23/53.

Use the narrower trace to remove repeated work. Code-audit candidates include
reusing tube HashSets/route buffers, removing per-frame `ToArray`, updating Activity
strings only on state changes, caching stable component references and avoiding
unchanged joint configuration writes. GC collection time is currently small; do not
sell allocation cleanup alone as the cure for the 200–400 ms navigation stalls.

Keep **120 Hz physics, 32 particles, existing forces, solver/CCD and puzzle rules**.
Any future physics-quality change requires separate evidence and regression scope.
Test pushes/pulls, lifts, moving bridges, glass corners, slit/pipe entry, split/merge
and recovery at both 30 and 60 display FPS. Only keep measured, equivalent patches.

## 5. Inspect rendering and thermal load after CPU fixes

The render thread often waits for the main thread. GPU time is unavailable in
Unity's Android Vulkan profiler. Main-thread rendering submission still costs
roughly 5–7 ms in these diagnostic windows and deserves its own breakdown.

Inspect Frame Debugger for passes, material variants, repeated transparent layers
and unnecessary draw submission. Test changes one at a time: MSAA 4× versus 2×,
shadow map/filter settings, render scale and a cheaper equivalent slick material.
These are controlled GPU-sensitivity experiments, **not automatic reductions of
the approved Glass C art**. If available on this device, use Android system/GPU
tracing to separate GPU execution from present/pacing waits. Do not label Present
or semaphore wait durations as shader time.

Retain a change only when native Release frame times or sustained temperature improve
and the runtime image remains approved in appearance. Keep phone render scale and
Product UI legible; don't achieve a benchmark by disabling the defining visual effects.

## 6. Acceptance and reporting

- Same routes, comparable temperature/power, at least three Release measurements;
  report median/p95/p99 frame time and counts above 33/50/100 ms, not average FPS alone.
- Aim toward sustained 60 FPS (16.67 ms frame budget). If OPPO cannot sustain it at
  approved quality after CPU fixes, report that honestly and compare a stable 30 FPS
  mode versus a visually verified scalable quality mode; do not silently change target.
- Run a 20-minute warm-device soak after short captures. Compare early and late
  performance and Android thermal status, not just the first minute.
- After shared navigation/physics-adjacent changes, rerun all 65 solves and the
  affected wander/no-shortcut/recovery tests. Keep the existing unresolved cases
  18/27/48/51 separate from new regressions.
- Verify normal Product UI, rewards, ability effects, tubes and Home animation,
  then restore a Release install for the user's playtest. No save resets.
- Record each patch's before/after evidence. If it does not improve the intended
  metric, revert that experiment rather than stacking unproven optimizations.

Recommended first implementation batch: **narrow CPU markers → level-53 edge
validation → skin hot loop → per-tick gameplay cleanup**. Graphics-quality experiments
follow only when CPU evidence and Product Release measurements justify them.

## Measured implementation update — 29 October 2026

The retained candidate keeps exact body construction and per-tick cleanup changes.
The navigation candidate/cache experiment did not establish reliable Android
benefit and was rolled back. All **209 final Editor tests** pass, including all
65 campaign solves and the targeted dense-mesh/navigation equivalence tests.

The six alternating Release runs have completed (B1/F1/F2/B2/B3/F3), each passing
all five selected routes. Body timing decreases **6.39–20.32%**, while median FPS
changes only **−1.74% to +1.32%**. Level 2's median P99 worsens from 32.75 to
48.95 ms. These results support retaining lower component cost, not claiming
60 FPS or uniformly smoother frame times. Start gates match the common band;
B2/B3/F3 later drift to thermal status 2 and remain in the results.

The proposed approximately 30% skin reduction and sub-50-ms navigation-stall goals
are **not achieved**. Original navigation still produces a 379.75 ms accumulated
graph frame in one Release repeat. Native Profiler identifies edge validation as
about 98% of level-53 graph time; A* and transform synchronization are not the main
cost in that capture. GPU time remains unavailable.

The next bounded investigations are presentation/tail attribution on an unchanged
APK, rebuild-reason and sampled exact-visibility attribution, then an independently
measured exact polygonizer-dispatch experiment. Keep all graphics and physics
contracts. Do not repeat the rejected cache merely because it reduces query counts.
See the [Release audit](../Docs/Verification/AlienPetOptimization20261029/final-release-review.md)
and [native accounting](../Docs/Verification/AlienPetOptimization20261029/native-cpu-budget.md).
The native full-campaign run lasted 39.81 minutes and passed 57/65 authored touch
routes, with no captured crash signature. An immediate warm-device repeat passed
all five performance routes. Product presentation/reward/Home checks passed, and
the exact final Release APK was restored and opened on OPPO without clearing saves.

Native functional acceptance remains incomplete: full-sweep failures at
10/18/27/32/48/51/56/57 are retained. Three isolated repeats per APK reproduce
failures on both versions; level 27 also has an earlier handoff failure in 1/3
final repeats versus 0/3 baseline repeats. This is not sufficient to establish
cause or rule out regression. No speculative physics change was made; investigate
pad-load and command history before changing the puzzle. This completed CPU batch
reduces component cost but does not satisfy the overall 60 FPS/navigation-stall
goals. See the [complete verification report](../Docs/Verification/AlienPetOptimization20261029/README.md).

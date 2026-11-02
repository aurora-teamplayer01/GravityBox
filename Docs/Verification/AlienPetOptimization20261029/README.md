# Alien Pet — mobile CPU optimization, 29 October 2026

Status: the retained CPU changes, before/after measurements, native soak and
isolated rechecks are complete. The verified final Release is installed and open
on OPPO. This is a playtest hand-off with unresolved native route failures, not a
65-level acceptance or sustained-60-FPS result.

## Scope and preserved contracts

The user authorized the measured performance plan while preserving graphics and gameplay.
The work keeps 120 Hz physics, 32 particles, solver/CCD, forces, split/merge rules,
level geometry/order/IDs, shaders/materials/lighting, render resolution/quality,
mesh density, animation and mesh update cadence unchanged. No save reset is used.

Retained changes:

- The navigation cache/candidate experiments were **removed** from the shipping
  candidate. Native Release did not demonstrate a reliable FPS benefit, and small
  routes showed higher graph costs. The after cohort was also hotter (38 °C/status 2
  versus 37 °C/status 1), so this is not a controlled causal regression claim.
  The original graph algorithm is restored byte-for-byte from the baseline, keeping
  its broad Profiler markers. All experiment sources, APKs and captures are retained
  separately; they must not be mixed into the final comparison.
- Skin construction skips provably empty grid rows and reuses directed edge
  interpolation/constraint results within the same fragment. Vertex order, normals,
  indices, field values and ink interpolation are compared against the original dense
  implementation. The Monster path and visual mesh density stay unchanged.
- Tube scratch sets, stable component references, Activity strings and unchanged
  joint settings avoid repeated allocation/work. Mechanism update order and forces
  remain unchanged.

## Measurement protocol

Unity 6000.3.19f1; Profile Analyzer 1.4.0; OPPO CPH2591 / Mali-G52 MC2;
Android 15, Vulkan, native 720×1612, target 60 FPS, USB charging.

A [read-only system snapshot](display-snapshot.json), taken outside the primary
comparison during the third Home warmup, confirms an active 60 Hz display/render
mode, no reported Display/SurfaceFlinger frame-rate override and standard Game Mode
with no package intervention. The panel also advertises 90 Hz, but that mode was
not active. This single snapshot is not historical CPU/GPU-clock telemetry; no
refresh, game-mode or thermal setting was changed.

Development captures use the actual Unity binary Profiler and the installed
Profile Analyzer package, without Deep Profile. Levels 2/7/23/53/65 use the same
save-protected authored touch replay, with a 25-second capture maximum per level.
This replay uses the developer HUD, **not Product UI**. Normal Product Home is
measured separately in Release; UI, rewards and ability paths also have functional
checks. GPU timestamps are unavailable with this Android Vulkan setup; present or
semaphore waits are not reported as GPU execution time.

Release A/B uses the same current replay harness with screenshots disabled.
The matched baseline is rebuilt with the preserved pre-optimization runtime files.
The older Release APK ignored the no-screenshot option and is excluded, along with
incomplete runs, from the primary comparison. Intermediate experiments remain
separate from final before/after runs. Repeat summaries show run ranges, temperature,
thermal status and route/tap differences; unlike levels/build types are not pooled.

Frame coverage is sampled, not continuous: the shared replay pauses its timers for
two diagnostic frames **before** submitting a new touch, even with screenshots
disabled. Measurement resumes before Began/Ended and remains active for the
ordinary synchronous navigation rebuild caused by that command. Physics and
mechanisms continue during the excluded frames, so unrelated/previous-command work
can still fall outside the sample. FPS, quantiles and maximum graph timings describe
the retained active windows, not every frame of the complete route. The final
Release audit records coverage against route duration rather than hiding this gap.

Known variability: baseline Home repeats are 29.87 / 60.24 / 29.82 FPS despite the
same APK, dimensions and target rate. Existing telemetry does not establish why.
This variability must not be attributed to optimization or hidden by a median.

## Evidence

Local detailed evidence is retained under
`Artifacts/AlienPetOptimization-2026-10-29/` (ignored): immutable before/after APKs,
source snapshots/hashes, build/test logs/XML, raw native captures, `.pdata`,
per-frame/per-marker CSV exports, device health logs and comparison scripts.

The baseline Release APK SHA-256 is
`0239994b0fdaaea74d2711dda94a5199486760bcbeba9052a6afdcadb51f536d`.

## Functional tests on the retained source

**209/209 unique Editor PlayMode tests passed**: 27 targeted tests and 182
campaign/Product tests, rerun after removing the navigation experiment. This
includes all 65 main-campaign solve cases, existing wander/shortcut/recovery
coverage, lifts, rail interactions, tubes, split/merge, UI, rewards and abilities.
See `editor-tests.json` for every test name, duration and suite result.

The body tests compare the optimized mesh construction against the independent
original dense implementation, including field samples, normals, vertex/index
order, ink, fragment changes, cache-plane wrap and generation rollover. Navigation
oracles also verify the restored graph against a full scan across moving/removed
blockers and recooked geometry. Editor passes are not native touch/novice-usability
proof; those are reported separately below when complete.

## Final Release comparison protocol

The original cooler baseline and rejected navigation candidate remain separate
from the final primary comparison. The retained Release is compared in the order
**baseline / final / final / baseline / baseline / final**, with the same
screenshot-free five-level replay. Before each run, identical short warmup and
settling are followed by two device-state readings 15 seconds apart.

Calibration was completed **before collecting either measured cohort**. Repeated
warmups at the now-full battery remained around 36°C/status 1, so the common start
band is **36–38°C, Android thermal status 1, USB powered**. Earlier strict 38°C and
37°C gate attempts contain warmup/calibration only, no measured before/after runs.
Their records are retained. No thermal, clock, brightness, refresh or quality
setting was overridden. Actual start/end/whole-run observations must be checked;
matching this band does not establish identical CPU/GPU clocks or thermal headroom.

Home and the final 65-level soak follow the primary comparison. The same five-level
route is repeated once immediately after the soak, without cooling, and stays out
of the three-repeat primary groups. Saved progress/wallet are disabled only in the
opt-in fixtures; the installed Release is then returned to normal Product mode.

## Retained Release comparison — complete

All six planned runs passed their five-level routes: **30/30 native route records**.
Values below are medians of three runs per APK, not a pooled full-campaign average.
See the [independent Release audit](final-release-review.md) for each run, ranges,
frame tails, memory and source validation.

| Level | Body construction before → after, ms/sample frame | Change | FPS before → after |
|---|---:|---:|---:|
| 2 | 3.493 → 3.270 | −6.4% | 46.58 → 45.76 |
| 7 | 5.960 → 5.576 | −6.4% | 29.72 → 29.75 |
| 23 | 10.790 → 8.927 | −17.3% | 29.67 → 29.85 |
| 53 | 7.510 → 5.984 | −20.3% | 27.12 → 27.47 |
| 65 | 6.358 → 5.660 | −11.0% | 28.84 → 28.90 |

All three retained body-time means are below all three baseline means for each
level. This reduces measured component work, but **does not establish a material
overall FPS increase or 60 FPS**. Level 2 FPS is slightly lower and its median P99
increases from 32.75 to 48.95 ms; level 7 P99 improves from 65.32 to 49.08 ms.
Other frame tails and remaining navigation spikes must not be hidden by the body
timing improvement. The approximately 30% skin-reduction experiment target was not
reached in these matched Release routes.

All start gates passed; **whole-run thermal matching was not achieved**. B2, B3 and
F3 reached thermal status 2 at the end and remain included. Battery observations
were 36–38°C. The frame intervals cover 96.423–98.420% of route wall time, median
97.278%; this is not an exact rendered-frame coverage percentage. The old 30.1 FPS
campaign sweep included screenshots and is not an equivalent before measurement.

The actual [Unity Profiler / Profile Analyzer audit](retained-profiler-review.md)
also finds lower body work. Its after captures are cooler than before, and it uses
a Development player, so those FPS deltas are not Release acceptance evidence.

## Diagnostic interpretation

See [native CPU accounting](native-cpu-budget.md). Skin work decreases in the final
native captures; some of the saved time appears alongside larger explicit waits.
The native after cohort is cooler, so its deltas alone do not isolate the code's
contribution. Inclusive parents/children and separate threads are not summed.
Observed fixed-step counts per rendered frame also differ: these captures do not
prove a hard CPU ceiling at a hypothetical 60 FPS.

Navigation spikes remain because that algorithm was restored. The retained level
53 traces include a 202.45 ms graph call and up to 397.44 ms summed graph work in
one frame. The proposed sub-50-ms navigation goal is **not achieved**. GPU execution
time remains unavailable; Present/semaphore waits are not reported as GPU time.

The skin optimization trades reusable scratch memory for fewer repeated edge
calculations. Buffers keep their high-water capacity until their owning surface is
destroyed; initial growth can allocate. Reported Unity allocated-memory peaks are
not Android process PSS, GC traffic or a proof of zero allocation.

The [bounded follow-up experiments](followup-notes.md) identify what remains to be
measured. Four fixed ticks per rendered frame near 30 FPS are normal accounting
for 120 Hz physics; this alone is not runaway catch-up. At 60 FPS there would be
about two ticks per displayed frame. Do not reduce physics frequency or infer a
60 FPS result by simply halving the current per-frame physics cost.

## Product presentation and save safety

The native Product proof passed: first solve, three victory poses, simulated reward,
Home and customization. Its overlap check reports zero overlaps, full-screen
720×1612 rendering and no saved-progress writes. The proof uses a fake ad reward;
it does not establish live ad delivery. The normal Release installation/open check
is completed separately after all baseline/final replays.

Images are real OPPO captures under `product/`. The gameplay, victory and Home
captures were visually inspected. No art assets, shaders, quality settings,
resolution or scene geometry were changed by this optimization. The mesh-equivalence
suite checks construction output; it is not a claim that every rendered pixel on
all 65 levels was image-diffed.

## Native campaign soak and late repeat — complete

The continuous native run lasted **39.81 minutes**, with **57/65 authored touch
routes passed**. Failed display IDs are **10, 18, 27, 32, 48, 51, 56, 57**. These
original failures remain recorded even if subsequent isolated rechecks pass.
No crash signatures, Unity Error/Fatal lines or telemetry errors were found in
the captured current-process logs. Caught route timeouts are still failures.

Sampled gameplay totals 31.56 minutes / 56,790 frames, weighted **29.99 FPS**,
P95 **49.01 ms**, P99 **65.33 ms**. This includes waiting in failed routes and is
not a matched comparison with the historical screenshot-enabled 30.1 FPS sweep.
The battery reaches **40°C**; Android thermal status moves from 1 to 2.

The immediate late repeat passes **5/5** without cooling or extra warmup. It
restarts the app in the same way as the early repeats. Single late observations:

| Level | Early retained median FPS | Late FPS | Late P99, ms |
|---|---:|---:|---:|
| 2 | 45.76 | 42.29 | 49.02 |
| 7 | 29.75 | 29.69 | 65.34 |
| 23 | 29.85 | 29.67 | 65.33 |
| 53 | 27.47 | 26.56 | 245.00 |
| 65 | 28.90 | 28.39 | 81.68 |

These are descriptive warm-device observations, not an isolated thermal-causality
experiment or a sustained-60-FPS result. See the [independent native soak audit](native-soak-review.md)
for interval coverage, elapsed versus sampled time, memory and failed-route context.

The earlier unresolved IDs 18/27/48/51 remain separate from additional failed IDs;
27 fails at a different earlier step this time. Level 56 may be a fixture false
failure after a one-way control has already completed, but that is still an
inference.

## Isolated native rechecks and installed build — complete

Three additional functional repeats per APK used the same five levels. These are
not additional primary FPS samples. See [all observations](native-rechecks.md)
and their [machine-readable evidence](native-rechecks.json), plus the
[independent replay audit](recheck-audit.md). These functional repeats are not
thermally matched: baseline observations are 40–41°C/status 2; final observations
are 41–42°C/status 2–3. Do not use them for FPS comparisons or causal attribution.

| Level | Baseline passes | Final passes |
|---|---:|---:|
| 10 | 3/3 | 3/3 |
| 27 | 0/3 | 0/3 |
| 32 | 1/3 | 1/3 |
| 56 | 2/3 | 1/3 |
| 57 | 0/3 | 0/3 |

The original full-sweep failures are not erased by these repeats. No level failed
only in the final isolated set, but **that does not rule out a regression**:
level 27 fails at the earlier back-road handoff in 1/3 final repeats versus 0/3
baseline repeats; all other isolated attempts fail later at reunion. The original
final full sweep also has the early failure and stays outside these repeat counts.
At level 56, the rejection phase is shared, but its first error message differs.

The [level-27 source review](level27-causal-source-review.md) finds no skin-to-physics
or mesh-to-picking path; navigation is baseline-identical. At failure neither pad
has particles within its footprint, consistent with the non-retaining gate closing.
The replay asserts geometric arrival near P2 but does not verify its measured load
before the handoff. This is a testable diagnostic hypothesis, not a proven fixture
bug or proof that the optimization is harmless. Pad-load/order history is needed
to resolve it. No speculative gameplay fix was added to hide the failures.

After rechecks, the installed APK was pulled and matched against the final Release:
`26df2c98969cd38f500708e2c364e9a82dd9090bbb1407b5babdd6efe4e5fb39`.
The app was launched without benchmark extras and confirmed in the foreground.
User progress was not cleared. See [installation evidence](installed-release.json)
and [normal launch capture](product/08-final-normal-launch.png).

The user APK is `Builds/SpatialLab/Android/COghe-Spatial.apk`. No commit or push was
performed in this task. Graphics and gameplay contracts are preserved by the
implementation; the unresolved native failures limit acceptance confidence.

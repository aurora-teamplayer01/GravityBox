# Native final Release sweep and late repeat — independent audit

Generated 2026-10-29T10:17:33.099294+00:00.

Final native full sweep: **57/65 authored routes passed**, failed display IDs **[10, 18, 27, 32, 48, 51, 56, 57]**. Immediate late repeat: **5/5 passed**, failed IDs **[]**. These are completed reports with all expected records and matching pulled evidence; failed route records remain failures.

| Run | Host start–end (UTC) | Monitor min | Route min | Sample min / frames | Sample-weighted FPS | Failed IDs |
|---|---|---:|---:|---:|---:|---|
| after-release-full65 | 2026-10-29T09:32:54.843544+00:00 – 2026-10-29T10:12:43.631078+00:00 | 39.813 | 32.403 | 31.564 / 56,790 | 29.987 | [10, 18, 27, 32, 48, 51, 56, 57] |
| after-soak-1 | 2026-10-29T10:12:43.845877+00:00 – 2026-10-29T10:15:58.647368+00:00 | 3.247 | 2.424 | 2.354 / 4,092 | 28.968 | [] |

The frame aggregates include failed-route waiting periods and therefore do not represent only successful solves. Monitor time includes process launch, scene transitions, UI, and collection. Route time is the sum of harness route durations. FPS and frame tails use recorded intervals; they are not exhaustive whole-session frame counts. The late run restarts the app but follows full65 directly without cooling, thermal gate, or extra warmup.

| Run | P95 / P99 / max ms | >50 / >100 ms frames | Sample interval coverage of route time |
|---|---:|---:|---:|
| after-release-full65 | 49.008 / 65.332 / 245.134 | 759 / 26 | 97.412% |
| after-soak-1 | 49.014 / 81.675 / 277.804 | 82 / 14 | 97.130% |

| Run | Health samples | Battery °C first→last [range] | Thermal statuses (sample counts) | Live HAL CPU °C range | Observed PSS / RSS / swap-PSS max MiB |
|---|---:|---|---|---|---|
| after-release-full65 | 77 | 36.0→40.0 [36.0–40.0] | {'1': 17, '2': 60} | 50.6–64.5 | 468.75 / 592.07 / 114.66 |
| after-soak-1 | 6 | 40.0→40.0 [40.0–40.0] | {'2': 6} | 52.4–63.9 | 491.82 / 592.34 / 16.09 |

Thermal status 1 means light and 2 moderate throttling severity; telemetry does not provide core/GPU clocks. Memory maxima are periodic observations, not exhaustive peaks or allocation traffic. PSS/RSS change with swap/residency, so this is not a memory-leak or reduction claim.

Filtered current-PID/current-run logs contain **0 crash signatures** and **0 Unity Error/Fatal lines**; serialized runtime-error collections total **0**. Telemetry-error samples: **0**. This observation is limited to the captured logs; caught route timeouts remain in failed-level records.

| Failed display | Error in this run | Baseline context |
|---|---|---|
| 10 (after-release-full65) | Operate B | Historical native sweep passed with 11 taps. First four targets match to logged three-decimal precision; historical tap 4 accepted moves=1 mechanisms=1 with target (0.08,-0.28,-0.29). |
| 18 (after-release-full65) | All parts meet and fuse | Reunion projected taps consumed as selection; alternate cold start missed first Q-machine tap. |
| 27 (after-release-full65) | Across the back road onto the right shelf, clear of P2 | Prior full/retry runs failed later at reunion after 49 taps, with merge-required loss. This run did not reach the C operation or reunion, so its signature differs. |
| 32 (after-release-full65) | No visible floor command for part 1 at (0.050, -0.300, -0.240) | Historical full sweep passed with 16 taps. First 12 logged rounded targets match; both runs reject candidates 13 and 14. Historical candidate 15 at (.02,-.30,-.24) succeeds; final candidates 15–17 fail. This location previously needed nearby-point recovery. |
| 48 (after-release-full65) | All tissue through the final exit | Initial exit approach stopped on floor; clean confirmation later stalled at partition after reunion. |
| 51 (after-release-full65) | Swing to landing 1 | Target dock tap released rope to rescue floor, phase Idle, landings=0, misses=0. |
| 56 (after-release-full65) | Handle C rejected:  | The historical 18-tap failure was an already-reached floor-pad assertion; an arrival guard corrected it, and a clean retry passed with 29 taps. The current rejection occurs at a different check. |
| 57 (after-release-full65) | All parts meet and fuse | Historical sweep passed with 31 taps. Both runs accepted the first seven rounded targets. Historical reunion also had ineffective taps, but retry points near (.229,-.30,-.140) accepted movement and solved. |

Display 10 investigation: the fourth tap at B produced no accepted command, then the single-attempt pilot helper timed out after 35 seconds. The older passing sweep accepted the same target to logged precision. Subsequent pick rays do not prove original event delivery or exact rejection. Static inspection found no scene/definition/lift/route changes; cache changes do not exclude timing/pose effects. Its cause remains unconfirmed; passing Editor tests do not negate this native failure. Saved abilities/progress are disabled in this fixture, but process/input history still differs from isolated launches. See companion failure-investigation data for source locations.

Display 56 is a strong fixture false-failure candidate: earlier walking taps appear to have moved its one-way C rail to the end before the helper demanded a fresh operation. Exact AtEnd/journey state was not recorded, so this remains an inference.

A repeated authored-route failure is not proof of an impossible puzzle, a confirmed gameplay defect, or an optimization regression. No new gameplay bug is confirmed by these saved observations alone. Known failures are tracked separately from clean passes; level 65 has a prior baseline approach failure despite passing the historical full sweep.

Both runs use build GUID `63a40fbb10a74a47a9b606b67cd721b4` on OPPO CPH2591, Vulkan, 720×1612, and declare screenshot capture disabled (zero image artifacts). The final APK SHA-256 is `26df2c98969cd38f500708e2c364e9a82dd9090bbb1407b5babdd6efe4e5fb39`. This reviewer did not interact with the device. The subsequent Product UI fixture is separate evidence.

This single native sweep supports the recorded route outcomes, thermal observations, and sampled frame statistics. It does not establish novice usability, sustained 60 FPS, a new gameplay regression, or a before/after FPS improvement. The historical 30.1 FPS screenshot sweep is not a matched comparator.

References: `Docs/Verification/AlienPetOppo20261029/README.md`; `measurement-exceptions.txt`; exact per-run source paths, hashes, health ranges, failed-route metrics, errors, and timing checks are recorded in `native-soak-review.json`.

Additional investigation classifications (original sweep failures retained):

| Display | Classification |
|---|---|
| 10 | Additional native authored-replay failure; optimization causality unresolved |
| 27 | Known failed ID with a different earlier route-step failure; cause unresolved |
| 32 | Additional native authored-replay failure at a projection-sensitive floor command; causality unresolved |
| 56 | Additional failed replay; strong fixture false-failure candidate, not confirmed |
| 57 | Additional native authored-replay failure; selection/occlusion-sensitive reunion, not a confirmed merge-rule defect |

A separate baseline/final recheck series, owned by the main pipeline, may provide follow-up evidence. It must not replace, remove, or relabel these original full-sweep failures.

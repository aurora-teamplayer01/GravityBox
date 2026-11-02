# Retained final Release — independent evidence review

Generated 2026-10-29T09:24:37.534794+00:00.

Skin time decreases consistently across all five levels (median reductions 6.39–20.32%), while FPS median changes range from -1.74% to +1.32%. The retained work is a component-cost improvement; it does not establish a substantial FPS gain.

All six planned canonical runs passed 5/5: B1–F1–F2–B2–B3–F3. No prior cool baseline, navigation experiment, Home, full65 or late-soak result enters this comparison. All are non-debuggable, screenshot-free Release author replays with the same recorded OPPO/API/resolution/profile/harness metadata. APK SHA-256 and readiness data were verified against files; full hashes are in the companion JSON.

The pre-measurement calibrated policy was 36–38°C battery and Android thermal status 1. Every start gate passed two readings at least 15 seconds apart after the identical short warmup/settle policy. The prior calibration sessions contain zero measured runs and were stopped before this cohort: no measured result was selected out by those calibration changes.

**Whole-run thermal matching: not achieved.** Drifted runs were retained with explicit false flags and caveats; they were not selectively excluded. Battery/status do not establish equal CPU/core/GPU clocks or thermal headroom.

| Run | Start gate °C/status | Whole-run battery °C | Thermal statuses | In band throughout recorded samples | Warmups |
|---|---|---|---|---|---:|
| before-release-1 | 36.0/1 | [36.0, 37.0] | [1] | True | 1 |
| after-release-1 | 37.0/1 | [36.0, 37.0] | [1] | True | 1 |
| after-release-2 | 37.0/1 | [37.0, 37.0] | [1] | True | 1 |
| before-release-2 | 37.0/1 | [37.0, 38.0] | [1, 2] | False | 1 |
| before-release-3 | 37.0/1 | [37.0, 38.0] | [1, 2] | False | 1 |
| after-release-3 | 38.0/1 | [37.0, 38.0] | [1, 2] | False | 1 |

All cells below are median [min–max] of three runs. Timers are inclusive Stopwatch wall milliseconds per sampled frame; columns overlap and must not be added as CPU busy time.

| Level | Side | FPS | Skin ms | Graph ms | Max frame-accumulated graph ms |
|---|---|---|---|---|---|
| 2 | before | 46.576 [46.575–46.906] | 3.493 [3.467–3.509] | 0.000 [0.000–0.000] | 0.000 [0.000–0.000] |
| 2 | after | 45.764 [45.028–46.094] | 3.270 [3.211–3.277] | 0.000 [0.000–0.000] | 0.000 [0.000–0.000] |
| 7 | before | 29.716 [29.640–29.808] | 5.960 [5.899–6.154] | 0.197 [0.197–0.216] | 35.832 [27.323–38.099] |
| 7 | after | 29.748 [29.540–29.768] | 5.576 [5.455–5.802] | 0.176 [0.164–0.194] | 28.760 [26.789–32.319] |
| 23 | before | 29.665 [29.617–29.715] | 10.790 [10.090–10.922] | 0.259 [0.237–0.288] | 35.902 [29.559–40.771] |
| 23 | after | 29.854 [29.763–29.990] | 8.927 [8.888–9.144] | 0.313 [0.297–0.352] | 48.346 [46.581–49.994] |
| 53 | before | 27.115 [27.106–27.266] | 7.510 [7.332–7.625] | 2.552 [2.449–2.573] | 195.301 [184.905–196.974] |
| 53 | after | 27.474 [27.325–27.499] | 5.984 [5.848–5.999] | 2.160 [2.102–2.357] | 199.074 [187.867–379.746] |
| 65 | before | 28.838 [28.586–28.989] | 6.358 [5.976–6.444] | 0.732 [0.695–0.807] | 49.847 [46.121–53.164] |
| 65 | after | 28.895 [28.699–28.905] | 5.660 [5.630–5.849] | 0.730 [0.703–0.769] | 50.880 [47.878–53.893] |

| Level | Side | P95 frame ms | P99 frame ms | Unity peak allocated MiB | Gen0 collections |
|---|---|---|---|---|---|
| 2 | before | 32.690 [32.679–32.711] | 32.754 [32.702–32.895] | 94.203 [94.177–94.230] | 2.000 [2.000–2.000] |
| 2 | after | 32.684 [32.681–32.697] | 48.954 [32.696–49.002] | 94.278 [94.188–94.336] | 2.000 [2.000–2.000] |
| 7 | before | 49.007 [49.007–49.009] | 65.322 [49.105–65.338] | 95.389 [95.147–95.866] | 5.000 [5.000–5.000] |
| 7 | after | 49.008 [49.007–49.009] | 49.076 [49.065–49.092] | 95.356 [95.351–95.416] | 5.000 [5.000–5.000] |
| 23 | before | 49.011 [49.011–49.017] | 65.335 [49.096–65.355] | 95.777 [95.766–96.184] | 4.000 [3.000–4.000] |
| 23 | after | 49.007 [49.007–49.011] | 65.321 [49.087–65.331] | 95.779 [95.763–95.803] | 2.000 [2.000–3.000] |
| 53 | before | 49.019 [49.013–49.027] | 212.357 [212.321–212.374] | 98.727 [98.684–99.110] | 10.000 [9.000–10.000] |
| 53 | after | 49.020 [49.011–49.027] | 212.272 [196.079–212.329] | 98.308 [98.235–98.342] | 7.000 [7.000–8.000] |
| 65 | before | 49.017 [49.015–49.020] | 65.366 [65.360–65.412] | 98.310 [98.282–98.717] | 23.000 [23.000–23.000] |
| 65 | after | 49.017 [49.015–49.025] | 65.352 [65.341–65.354] | 98.426 [97.882–98.753] | 20.000 [20.000–20.000] |

Skin saving versus delivered FPS:

| Level | Skin median change | FPS median change | FPS repeat ranges overlap |
|---|---:|---:|---|
| 2 | -6.39% | -1.74% | False |
| 7 | -6.44% | +0.11% | True |
| 23 | -17.27% | +0.64% | False |
| 53 | -20.32% | +1.32% | False |
| 65 | -10.97% | +0.20% | True |

Whole-process memory: median [range] of each run’s maximum **observed** sample, MiB. There are six periodic memory observations per run; they do not cover every transient peak or align precisely with individual levels.

| Side | Observed PSS maximum | Observed RSS maximum | Observed swap PSS maximum |
|---|---|---|---|
| before | 521.456 [504.149–526.717] | 635.336 [617.930–652.344] | 3.337 [0.991–43.097] |
| after | 495.497 [468.793–509.592] | 607.867 [587.270–624.102] | 4.581 [0.885–46.508] |

The lower observed PSS/RSS cohort values are not a clean application-memory reduction claim: swap/residency varies substantially, and sampling is sparse. Per-level Unity allocation peaks are reported separately above.

Across all 30 level records, frame-array lengths equal declared frame counts and sampled FPS/P95/P99 recompute correctly. Sum(recorded frame intervals)/route wall time ranges **96.423–98.420%** (median 97.278%). This is interval-time coverage, not an exact percentage of all rendered frames; there is no whole-route rendered-frame counter.

The two explicit measuring-off frames are **before** each new touch. In `COgheViewProofPlayer.cs:214`, the harness pauses, yields, clears timers, and resumes measurement; Began/Ended are queued afterward at217–218. The new command’s normal synchronous graph build is therefore not intentionally hidden. Physics, mechanisms or deferred work from an earlier command can still run during a later pause and be discarded. Other proof pauses and route boundaries also omit samples. Screenshot disabling skips image work but preserves those yields. FPS/tails/maxGraph are sampled-window statistics, not whole-route exhaustive maxima.

Skin savings and FPS must be reported separately. This small one-device cohort supports descriptive component timings and frame distributions, not a causal attribution or a 60 FPS claim. Thermal drift, three repeats, level 65 tap variation and frame pacing limit inference. MaxGraph is a maximum accumulated sampled-frame timer, not the longest individual graph call. Sampled Unity peak allocation is neither allocation traffic nor RSS/PSS; GC counts cover the full route, including sampling pauses.

Source: `COgheViewProofPlayer.cs:63–68,122–131,214–218,285–289`; `COgheMobileBenchmark.cs:14–22`. Full per-run metrics, health, coverage, APK hashes and protocol checks are in `final-release-review.json`; detailed CPU/GC/ranges also in `comparison/release-cpu.md`.

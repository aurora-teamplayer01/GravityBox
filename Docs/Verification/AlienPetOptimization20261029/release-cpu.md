# Release CPU timer comparison

Release benchmark APK author replays only. Primary groups require complete passing screenshot-free runs using the common integrity checks. CPU timers are Stopwatch wall durations per harness sampled frame, not native CPU busy time. Parent/child timers can overlap; never add skin/Step/graph/route columns. maxGraphMs is the maximum per-frame accumulated graph timer, not the longest individual graph call. peakAllocatedBytes is the sampled peak of Unity Profiler.GetTotalAllocatedMemoryLong, not allocation traffic, RSS or PSS. gcCollections is the route-wide GC.CollectionCount(0) difference, not bytes. Medians/ranges summarize runs; no invented missing/zero samples.

All cells are median [minimum–maximum] across eligible runs. Timer columns are ms per sampled frame; FPS is the harness result. Missing final runs remain missing.

| Level | Phase | Runs | FPS | Skin ms | Step ms | Graph ms | Route ms |
|---|---|---:|---|---|---|---|---|
| 2 | before | 3/3 | 46.576 [46.575–46.906] | 3.493 [3.467–3.509] | 1.205 [1.196–1.237] | 0.000 [0.000–0.000] | 0.011 [0.009–0.015] |
| 2 | after | 3/3 | 45.764 [45.028–46.094] | 3.270 [3.211–3.277] | 1.273 [1.237–1.311] | 0.000 [0.000–0.000] | 0.010 [0.010–0.011] |
| 7 | before | 3/3 | 29.716 [29.640–29.808] | 5.960 [5.899–6.154] | 4.516 [4.327–4.521] | 0.197 [0.197–0.216] | 0.026 [0.023–0.045] |
| 7 | after | 3/3 | 29.748 [29.540–29.768] | 5.576 [5.455–5.802] | 4.411 [4.347–4.499] | 0.176 [0.164–0.194] | 0.034 [0.033–0.038] |
| 23 | before | 3/3 | 29.665 [29.617–29.715] | 10.790 [10.090–10.922] | 4.023 [3.817–4.044] | 0.259 [0.237–0.288] | 0.015 [0.012–0.017] |
| 23 | after | 3/3 | 29.854 [29.763–29.990] | 8.927 [8.888–9.144] | 4.383 [4.331–4.474] | 0.313 [0.297–0.352] | 0.017 [0.016–0.021] |
| 53 | before | 3/3 | 27.115 [27.106–27.266] | 7.510 [7.332–7.625] | 8.392 [8.193–8.521] | 2.552 [2.449–2.573] | 0.022 [0.019–0.022] |
| 53 | after | 3/3 | 27.474 [27.325–27.499] | 5.984 [5.848–5.999] | 8.055 [8.028–8.379] | 2.160 [2.102–2.357] | 0.024 [0.018–0.029] |
| 65 | before | 3/3 | 28.838 [28.586–28.989] | 6.358 [5.976–6.444] | 5.651 [5.368–5.945] | 0.732 [0.695–0.807] | 0.020 [0.018–0.021] |
| 65 | after | 3/3 | 28.895 [28.699–28.905] | 5.660 [5.630–5.849] | 5.545 [5.480–5.775] | 0.730 [0.703–0.769] | 0.019 [0.019–0.019] |

| Level | Phase | P95 frame ms | P99 frame ms | Max accumulated graph ms | Unity peak allocated MiB | Gen0 collections | Route taps |
|---|---|---|---|---|---|---|---|
| 2 | before | 32.690 [32.679–32.711] | 32.754 [32.702–32.895] | 0.000 [0.000–0.000] | 94.203 [94.177–94.230] | 2.000 [2.000–2.000] | 3.000 [3.000–3.000] |
| 2 | after | 32.684 [32.681–32.697] | 48.954 [32.696–49.002] | 0.000 [0.000–0.000] | 94.278 [94.188–94.336] | 2.000 [2.000–2.000] | 3.000 [3.000–3.000] |
| 7 | before | 49.007 [49.007–49.009] | 65.322 [49.105–65.338] | 35.832 [27.323–38.099] | 95.389 [95.147–95.866] | 5.000 [5.000–5.000] | 6.000 [6.000–6.000] |
| 7 | after | 49.008 [49.007–49.009] | 49.076 [49.065–49.092] | 28.760 [26.789–32.319] | 95.356 [95.351–95.416] | 5.000 [5.000–5.000] | 6.000 [6.000–6.000] |
| 23 | before | 49.011 [49.011–49.017] | 65.335 [49.096–65.355] | 35.902 [29.559–40.771] | 95.777 [95.766–96.184] | 4.000 [3.000–4.000] | 5.000 [5.000–5.000] |
| 23 | after | 49.007 [49.007–49.011] | 65.321 [49.087–65.331] | 48.346 [46.581–49.994] | 95.779 [95.763–95.803] | 2.000 [2.000–3.000] | 5.000 [5.000–5.000] |
| 53 | before | 49.019 [49.013–49.027] | 212.357 [212.321–212.374] | 195.301 [184.905–196.974] | 98.727 [98.684–99.110] | 10.000 [9.000–10.000] | 13.000 [13.000–13.000] |
| 53 | after | 49.020 [49.011–49.027] | 212.272 [196.079–212.329] | 199.074 [187.867–379.746] | 98.308 [98.235–98.342] | 7.000 [7.000–8.000] | 13.000 [13.000–13.000] |
| 65 | before | 49.017 [49.015–49.020] | 65.366 [65.360–65.412] | 49.847 [46.121–53.164] | 98.310 [98.282–98.717] | 23.000 [23.000–23.000] | 33.000 [33.000–34.000] |
| 65 | after | 49.017 [49.015–49.025] | 65.352 [65.341–65.354] | 50.880 [47.878–53.893] | 98.426 [97.882–98.753] | 20.000 [20.000–20.000] | 34.000 [33.000–34.000] |

## Comparability

- Level 2: Starting battery temperatures vary: before 36.0..37.0 C; after 37.0..38.0 C; Thermal severity differs or is missing.
- Level 7: Starting battery temperatures vary: before 36.0..37.0 C; after 37.0..38.0 C; Thermal severity differs or is missing.
- Level 23: Starting battery temperatures vary: before 36.0..37.0 C; after 37.0..38.0 C; Thermal severity differs or is missing.
- Level 53: Starting battery temperatures vary: before 36.0..37.0 C; after 37.0..38.0 C; Thermal severity differs or is missing.
- Level 65: Starting battery temperatures vary: before 36.0..37.0 C; after 37.0..38.0 C; Thermal severity differs or is missing; Route tap counts vary: [34.0, 33.0, 34.0, 34.0, 33.0, 33.0].

## Release temperatures

| Run | Eligible | Battery start / end C | Thermal statuses |
|---|---|---|---|
| after-release-1 | True | 37.0 / 37.0 | [1] |
| after-release-2 | True | 37.0 / 37.0 | [1] |
| after-release-3 | True | 38.0 / 38.0 | [1, 2] |
| before-release-1 | True | 36.0 / 37.0 | [1] |
| before-release-2 | True | 37.0 / 38.0 | [1, 2] |
| before-release-3 | True | 37.0 / 38.0 | [1] |

## Profiler (separate diagnostic cohort) temperatures

| Run | Eligible | Battery start / end C | Thermal statuses |
|---|---|---|---|
| after-profiler-1 | True | 36.0 / 37.0 | [1] |
| after-profiler-2 | True | 37.0 / 38.0 | [1, 2] |
| after-profiler-3 | True | 38.0 / 38.0 | [2] |
| before-profiler-1 | True | 39.0 / 40.0 | [2] |
| before-profiler-2 | True | 40.0 / 40.0 | [2] |
| before-profiler-3 | True | 40.0 / 41.0 | [2] |

Profiler captures are a separate dataset with Development/recording overhead. Recorded starting battery ranges: before 39.0..40.0 C; after 36.0..38.0 C. These temperatures differ; Profiler CPU deltas cannot isolate code changes from temperature/governor state. Do not pool Profiler numbers with Release CPU timers. Matching battery temperature in Release does not prove identical CPU clocks or thermal state.

## Exclusions

No discovered Release runs excluded.

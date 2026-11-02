# Alien Pet capture comparison

Profiler diagnostic windows, Release author replay and Release Home timings are separate datasets. Stages1–3 are intermediate groups and is never pooled into final runs. Primary groups exclude incomplete/failed runs and screenshot captures. Medians summarize individual runs; pooled counts include every retained frame from eligible runs. Missing runs or markers stay missing. Deltas are descriptive, not exact matched or causal estimates. Main/render-thread elapsed times include waits, are not CPU busy times, and overlap. No GPU bottleneck inference is made.

Only completed, passing, screenshot-free runs enter this table. Rejected runs remain in JSON/CSV with eligibleForPrimary=false. Percentiles use sorted[floor(n × q)]. Counts are strictly >33, >50 and >100 ms; CSV also includes >33.333 ms. Only positive finite samples enter distributions.

| Kind | Level | Phase | Runs | FPS median [min–max] | Median run mean | Median p95 | Median p99 | Pooled >33 / >50 / >100 |
|---|---:|---|---:|---:|---:|---:|---:|---|
| profiler | 2 | before | 3/3 | 41.48 [35.83–41.80] | 24.106 ms | 30.552 ms | 42.374 ms | 50 / 4 / 3 of 833 |
| profiler | 2 | stage2 | 1/3 | 38.26 [38.26–38.26] | 26.139 ms | 40.039 ms | 60.139 ms | 36 / 4 / 1 of 274 |
| profiler | 2 | stage3 | 3/3 | 43.07 [42.89–43.58] | 23.218 ms | 32.361 ms | 35.088 ms | 33 / 3 / 3 of 913 |
| profiler | 2 | after | 3/3 | 43.71 [43.20–44.31] | 22.876 ms | 33.210 ms | 36.037 ms | 54 / 3 / 2 of 922 |
| profiler | 7 | before | 3/3 | 29.50 [28.82–29.77] | 33.901 ms | 48.025 ms | 54.575 ms | 867 / 58 / 4 of 1872 |
| profiler | 7 | stage2 | 1/3 | 29.29 [29.29–29.29] | 34.138 ms | 48.958 ms | 58.598 ms | 286 / 28 / 3 of 633 |
| profiler | 7 | stage3 | 3/3 | 29.50 [29.49–29.62] | 33.895 ms | 48.391 ms | 55.877 ms | 844 / 76 / 6 of 1928 |
| profiler | 7 | after | 3/3 | 29.87 [29.62–29.88] | 33.484 ms | 48.109 ms | 55.657 ms | 848 / 78 / 5 of 1944 |
| profiler | 23 | before | 3/3 | 29.04 [28.33–29.47] | 34.434 ms | 46.154 ms | 57.529 ms | 621 / 25 / 0 of 1111 |
| profiler | 23 | stage2 | 1/3 | 29.35 [29.35–29.35] | 34.069 ms | 49.292 ms | 58.849 ms | 165 / 18 / 0 of 386 |
| profiler | 23 | stage3 | 3/3 | 29.84 [29.76–29.84] | 33.515 ms | 48.338 ms | 57.546 ms | 479 / 46 / 0 of 1142 |
| profiler | 23 | after | 3/3 | 29.91 [29.85–29.93] | 33.432 ms | 48.242 ms | 56.307 ms | 464 / 46 / 0 of 1147 |
| profiler | 53 | before | 3/3 | 22.39 [22.28–23.60] | 44.669 ms | 59.556 ms | 260.131 ms | 1522 / 148 / 35 of 1710 |
| profiler | 53 | stage2 | 1/3 | 24.19 [24.19–24.19] | 41.346 ms | 55.285 ms | 303.236 ms | 454 / 50 / 10 of 605 |
| profiler | 53 | stage3 | 3/3 | 24.34 [24.22–24.41] | 41.080 ms | 52.515 ms | 233.852 ms | 1547 / 107 / 31 of 1826 |
| profiler | 53 | after | 3/3 | 24.35 [24.30–25.06] | 41.069 ms | 53.562 ms | 228.858 ms | 1527 / 124 / 32 of 1844 |
| profiler | 65 | before | 3/3 | 26.16 [25.57–26.75] | 38.228 ms | 51.387 ms | 76.400 ms | 1503 / 125 / 1 of 1964 |
| profiler | 65 | stage2 | 1/3 | 26.88 [26.88–26.88] | 37.197 ms | 52.902 ms | 90.929 ms | 506 / 49 / 5 of 673 |
| profiler | 65 | stage3 | 3/3 | 26.63 [26.48–26.70] | 37.552 ms | 51.897 ms | 78.225 ms | 1511 / 115 / 2 of 1997 |
| profiler | 65 | after | 3/3 | 26.81 [26.72–27.06] | 37.293 ms | 51.419 ms | 71.418 ms | 1519 / 142 / 0 of 2017 |
| release | 2 | before | 3/3 | 46.58 [46.58–46.91] | 21.470 ms | 32.690 ms | 32.754 ms | 4 / 3 / 0 of 948 |
| release | 2 | after | 3/3 | 45.76 [45.03–46.09] | 21.851 ms | 32.684 ms | 48.954 ms | 9 / 3 / 0 of 931 |
| release | 7 | before | 3/3 | 29.72 [29.64–29.81] | 33.652 ms | 49.007 ms | 65.322 ms | 216 / 23 / 2 of 1845 |
| release | 7 | after | 3/3 | 29.75 [29.54–29.77] | 33.616 ms | 49.008 ms | 49.076 ms | 226 / 13 / 1 of 1853 |
| release | 23 | before | 3/3 | 29.66 [29.62–29.71] | 33.710 ms | 49.011 ms | 65.335 ms | 135 / 13 / 0 of 1054 |
| release | 23 | after | 3/3 | 29.85 [29.76–29.99] | 33.496 ms | 49.007 ms | 65.321 ms | 119 / 9 / 0 of 1036 |
| release | 53 | before | 3/3 | 27.11 [27.11–27.27] | 36.880 ms | 49.019 ms | 212.357 ms | 302 / 67 / 33 of 2444 |
| release | 53 | after | 3/3 | 27.47 [27.32–27.50] | 36.399 ms | 49.020 ms | 212.272 ms | 288 / 57 / 28 of 2480 |
| release | 65 | before | 3/3 | 28.84 [28.59–28.99] | 34.677 ms | 49.017 ms | 65.366 ms | 738 / 136 / 0 of 6060 |
| release | 65 | after | 3/3 | 28.90 [28.70–28.90] | 34.608 ms | 49.017 ms | 65.352 ms | 730 / 131 / 1 of 6182 |
| home | home | before | 3/3 | 29.87 [29.82–60.24] | 33.475 ms | 49.005 ms | 49.028 ms | 211 / 3 / 3 of 3675 |
| home | home | after | 3/3 | 29.89 [29.89–60.27] | 33.456 ms | 49.007 ms | 49.038 ms | 207 / 4 / 1 of 3680 |

## Comparison limits

- stage3 profiler level 65: Within-phase route tap counts differ: [31, 33].
- after profiler level 65: Within-phase route tap counts differ: [31, 33, 35].
- before release level 65: Within-phase route tap counts differ: [33, 34].
- after release level 65: Within-phase route tap counts differ: [33, 34].
- before home level home: Within-phase FPS range exceeds 20%; inspect per-run/cycle data. Cause is unmeasured; do not attribute this spread to the code change.
- after home level home: Within-phase FPS range exceeds 20%; inspect per-run/cycle data. Cause is unmeasured; do not attribute this spread to the code change.
- profiler level 2: mean -1.230 ms (-5.101%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Starting battery temperatures differ by >2 C (40.0 vs 37.0); diagnostic threshold only; Thermal severity differs or is unavailable; Profiler CSVs are unfiltered; frame boundaries, waits and diagnostic overhead remain; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- profiler level 7: mean -0.417 ms (-1.230%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Starting battery temperatures differ by >2 C (40.0 vs 37.0); diagnostic threshold only; Thermal severity differs or is unavailable; Profiler CSVs are unfiltered; frame boundaries, waits and diagnostic overhead remain; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- profiler level 23: mean -1.002 ms (-2.911%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Starting battery temperatures differ by >2 C (40.0 vs 37.0); diagnostic threshold only; Thermal severity differs or is unavailable; Profiler CSVs are unfiltered; frame boundaries, waits and diagnostic overhead remain; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- profiler level 53: mean -3.600 ms (-8.060%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Fixed-duration or unknown windows may cover different route progress; Starting battery temperatures differ by >2 C (40.0 vs 37.0); diagnostic threshold only; Thermal severity differs or is unavailable; Profiler CSVs are unfiltered; frame boundaries, waits and diagnostic overhead remain; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- profiler level 65: mean -0.935 ms (-2.446%). after: Within-phase route tap counts differ: [31, 33, 35]; Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Fixed-duration or unknown windows may cover different route progress; Different or unknown taps counts; Starting battery temperatures differ by >2 C (40.0 vs 37.0); diagnostic threshold only; Thermal severity differs or is unavailable; Profiler CSVs are unfiltered; frame boundaries, waits and diagnostic overhead remain; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- release level 2: mean +0.381 ms (1.775%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Thermal severity differs or is unavailable; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- release level 7: mean -0.036 ms (-0.106%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Thermal severity differs or is unavailable; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- release level 23: mean -0.214 ms (-0.634%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Thermal severity differs or is unavailable; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- release level 53: mean -0.481 ms (-1.305%). Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Thermal severity differs or is unavailable; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- release level 65: mean -0.069 ms (-0.199%). before: Within-phase route tap counts differ: [33, 34]; after: Within-phase route tap counts differ: [33, 34]; Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Different or unknown taps counts; Thermal severity differs or is unavailable; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.
- home level home: mean -0.019 ms (-0.056%). before: Within-phase FPS range exceeds 20%; inspect per-run/cycle data. Cause is unmeasured; do not attribute this spread to the code change; after: Within-phase FPS range exceeds 20%; inspect per-run/cycle data. Cause is unmeasured; do not attribute this spread to the code change; Unrecorded comparability metadata: api; Unrecorded comparability metadata: graphicProfile; Unrecorded comparability metadata: ui; Unrecorded comparability metadata: route; Unrecorded comparability metadata: style; Unrecorded comparability metadata: quality; Unrecorded comparability metadata: refreshHz; Mixed or unknown before build GUIDs; Mixed or unknown after build GUIDs; Run-wide health is not aligned to per-level captures; no causal or exact matched claim.

## Run-wide health

| Run | Battery before / after | Health battery min–max | HAL CPU min–max | Thermal statuses | PSS first / last / max MiB | RSS max MiB |
|---|---|---|---|---|---|---|
| after-profiler-1 | 36.000 / 37.000 C | 36.0–37.0 C | 54.1–62.3 C | [1] | 369.250 / 524.921 / 560.269 | 631.949 |
| after-profiler-2 | 37.000 / 38.000 C | 37.0–38.0 C | 59.6–61.8 C | [1, 2] | 359.670 / 515.388 / 515.388 | 597.828 |
| after-profiler-3 | 38.000 / 38.000 C | 38.0–38.0 C | 53.2–62.6 C | [2] | 351.835 / 571.458 / 571.458 | 691.375 |
| after-release-1 | 37.000 / 37.000 C | 36.0–37.0 C | 48.3–61.9 C | [1] | 378.928 / 485.419 / 495.497 | 607.867 |
| after-release-2 | 37.000 / 37.000 C | 37.0–37.0 C | 48.8–62.2 C | [1] | 436.467 / 426.120 / 468.793 | 587.270 |
| after-release-3 | 38.000 / 38.000 C | 37.0–38.0 C | 46.8–62.3 C | [1, 2] | 345.885 / 509.592 / 509.592 | 624.102 |
| after-release-home-1 | 37.000 / 37.000 C | 37.0–37.0 C | 44.4–57.8 C | [1] | 425.959 / 425.959 / 425.959 | 546.699 |
| after-release-home-2 | 37.000 / 37.000 C | 37.0–37.0 C | 44.4–47.6 C | [1] | 384.498 / 384.498 / 384.498 | 504.305 |
| after-release-home-3 | 37.000 / 36.000 C | 36.0–37.0 C | 43.6–47.0 C | [1] | 400.909 / 400.909 / 400.909 | 523.891 |
| before-profiler-1 | 39.000 / 40.000 C | 39.0–40.0 C | 53.1–63.8 C | [2] | 296.651 / 492.551 / 505.004 | 544.520 |
| before-profiler-2 | 40.000 / 40.000 C | 40.0–40.0 C | 58.8–64.3 C | [2] | 370.350 / 548.170 / 567.102 | 681.535 |
| before-profiler-3 | 40.000 / 41.000 C | 40.0–41.0 C | 58.4–63.5 C | [2] | 347.470 / 535.867 / 563.034 | 669.305 |
| before-release-1 | 36.000 / 37.000 C | 36.0–36.0 C | 48.3–61.7 C | [1] | 384.312 / 512.032 / 521.456 | 635.336 |
| before-release-2 | 37.000 / 38.000 C | 37.0–38.0 C | 49.4–62.2 C | [1, 2] | 370.703 / 487.945 / 504.149 | 617.930 |
| before-release-3 | 37.000 / 38.000 C | 37.0–38.0 C | 48.5–63.3 C | [1] | 445.853 / 435.014 / 526.717 | 652.344 |
| before-release-home-1 | 37.000 / 37.000 C | 37.0–37.0 C | 48.5–59.5 C | [1] | 382.293 / 382.293 / 382.293 | 485.242 |
| before-release-home-2 | 37.000 / 37.000 C | 37.0–37.0 C | 51.7–57.8 C | [1] | 413.004 / 413.004 / 413.004 | 537.336 |
| before-release-home-3 | 37.000 / 37.000 C | 37.0–37.0 C | 45.3–51.1 C | [1] | 411.363 / 411.363 / 411.363 | 518.355 |
| stage1-profiler-1 | 39.000 / 39.000 C | 39.0–39.0 C | 53.1–62.0 C | [2] | 296.102 / 496.333 / 533.881 | 611.234 |
| stage1-profiler-2 | 39.000 / 39.000 C | 39.0–39.0 C | 54.3–61.4 C | [2] | 358.062 / 533.793 / 554.253 | 663.766 |
| stage1-profiler-3 | 39.000 / 39.000 C | 39.0–39.0 C | 54.1–62.8 C | [2] | 341.849 / 489.764 / 533.554 | 652.449 |
| stage2-profiler-1 | 36.000 / 37.000 C | 36.0–37.0 C | 59.3–65.0 C | [1] | 350.860 / 481.536 / 481.536 | 449.000 |
| stage3-profiler-1 | 38.000 / 38.000 C | 38.0–38.0 C | 56.5–62.2 C | [2] | 363.016 / 551.375 / 571.497 | 632.445 |
| stage3-profiler-2 | 38.000 / 38.000 C | 38.0–38.0 C | 59.2–62.3 C | [2] | 351.261 / 550.600 / 563.286 | 681.488 |
| stage3-profiler-3 | 38.000 / 38.000 C | 38.0–39.0 C | 56.7–61.9 C | [2] | 356.749 / 536.657 / 580.418 | 691.945 |

PSS/RSS are sparse process-wide Android samples, not Unity allocation totals or per-level peaks. Missing telemetry is unknown, not zero. Memory categories must not be added together. Starting temperatures and severity are descriptive observations; this report does not establish thermal throttling.

## Integrity and coverage

| Run | Primary eligible | Completed | Screenshots | Exclusion |
|---|---|---|---:|---|
| after-profiler-1 | True | True | 0 | none |
| after-profiler-2 | True | True | 0 | none |
| after-profiler-3 | True | True | 0 | none |
| after-release-1 | True | True | 0 | none |
| after-release-2 | True | True | 0 | none |
| after-release-3 | True | True | 0 | none |
| after-release-home-1 | True | True | 0 | none |
| after-release-home-2 | True | True | 0 | none |
| after-release-home-3 | True | True | 0 | none |
| before-profiler-1 | True | True | 0 | none |
| before-profiler-2 | True | True | 0 | none |
| before-profiler-3 | True | True | 0 | none |
| before-release-1 | True | True | 0 | none |
| before-release-2 | True | True | 0 | none |
| before-release-3 | True | True | 0 | none |
| before-release-home-1 | True | True | 0 | none |
| before-release-home-2 | True | True | 0 | none |
| before-release-home-3 | True | True | 0 | none |
| stage1-profiler-1 | False | True | 0 | At least one requested author route failed or is missing |
| stage1-profiler-2 | False | False | 0 | Monitor/report incomplete or requested levels missing; At least one requested author route failed or is missing |
| stage1-profiler-3 | True | True | 0 | none |
| stage2-profiler-1 | True | True | 0 | none |
| stage3-profiler-1 | True | True | 0 | none |
| stage3-profiler-2 | True | True | 0 | none |
| stage3-profiler-3 | True | True | 0 | none |

View and realtime Home fixtures disable save persistence before scene loading in the inspected source; artifacts do not independently verify save-file checksums. Home reports do not include build GUID or graphics API. Three short Home runs do not constitute a continuous 20-minute soak. Home per-cycle distributions are in home-cycles.csv; Product UI correctness remains a separate visual/functional proof.

## Markers

Per-run and aggregated markers, calls, totals, normalized frame costs, max calls and graph frame spikes are in comparison.json, markers.csv (per run) and marker-groups.csv (aggregated). Missing stages are unavailable, never zero. Inclusive parents/children and different threads overlap; do not add their times. GPU timings may be unavailable under Vulkan; waits and Present are not shader time.

## Missing inputs and warnings

- after-release-home-1: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- after-release-home-2: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- after-release-home-3: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- before-release-home-1: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- before-release-home-2: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- before-release-home-3: Home timing.json does not record build GUID/API; verify APK provenance separately. Home cycles are measured samples, not an independent steady 20-minute soak
- stage1-profiler-1: At least one requested author route failed or is missing
- stage1-profiler-2: Monitor/report incomplete or requested levels missing
- stage1-profiler-2: At least one requested author route failed or is missing
- stage1-profiler-2: Level 2: profiler CSV export missing
- stage1-profiler-2: Level 7: profiler CSV export missing
- stage1-profiler-2: Level 23: profiler CSV export missing
- stage1-profiler-2: Level 53: profiler CSV export missing
- stage1-profiler-2: Level 65: profiler CSV export missing
- stage1-profiler-3: Level 2: profiler CSV export missing
- stage1-profiler-3: Level 7: profiler CSV export missing
- stage1-profiler-3: Level 23: profiler CSV export missing
- stage1-profiler-3: Level 53: profiler CSV export missing
- stage1-profiler-3: Level 65: profiler CSV export missing

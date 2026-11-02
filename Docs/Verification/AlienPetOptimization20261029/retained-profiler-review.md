# Retained candidate — independent native Profiler audit

Generated 2026-10-29T08:42:19.737014+00:00 from comparison generated 2026-10-29T08:38:41.033181+00:00.

All15 after captures resolve uniquely to canonical `native-after/analysis`. No rejected/navigation-experiment export is selected; the discarded candidate/cache scopes are absent. All six source runs pass5/5 and have zero image artifacts. The companion JSON records every source/export timestamp and actual device-health sample.

The current Motion and NavigationSnapshot source hashes match their pre-optimization backups: True. Lower graph timings therefore must not be presented as a retained navigation-algorithm optimization.

Values are median [min–max] across three run means. Skin and graph values are inclusive milliseconds per call. Native frame-derived FPS includes the unfiltered capture boundaries and waits; these are Development/Profiler observations, not Release FPS.

| Level | Skin before ms/call | Skin retained ms/call | FPS before | FPS retained |
|---|---|---|---|---|
| 2 | 4.366 [4.041–4.367] | 3.356 [3.198–3.557] | 41.484 [35.827–41.799] | 43.714 [43.197–44.313] |
| 7 | 4.612 [4.575–5.276] | 4.155 [3.995–4.256] | 29.498 [28.822–29.768] | 29.865 [29.625–29.879] |
| 23 | 10.002 [9.263–10.657] | 6.894 [6.585–6.920] | 29.041 [28.332–29.465] | 29.911 [29.850–29.927] |
| 53 | 8.966 [8.137–9.275] | 6.282 [5.901–6.319] | 22.387 [22.278–23.597] | 24.349 [24.298–25.056] |
| 65 | 5.704 [5.143–5.872] | 4.670 [4.502–4.699] | 26.159 [25.574–26.749] | 26.815 [26.716–27.056] |

The skin mean from every retained repeat is below every baseline repeat at each of these five levels. The temperature mismatch below prevents a code-only attribution.

| Level | Graph before ms/call | Graph retained ms/call | Calls across repeats before→retained |
|---|---|---|---|
| 2 | absent | absent | absent, not measured zero |
| 7 | 22.008 [21.621–23.271] | 17.938 [17.752–20.179] | [5, 5, 5]→[5, 5, 5] |
| 23 | 17.770 [16.097–17.823] | 16.030 [16.016–17.906] | [5, 5, 5]→[5, 5, 5] |
| 53 | 219.946 [197.578–221.594] | 198.390 [185.793–198.537] | [10, 9, 9]→[9, 9, 9] |
| 65 | 49.458 [44.220–49.763] | 45.335 [43.072–47.137] | [7, 7, 7]→[7, 7, 7] |

Large graph spikes remain. At level53 the largest individual graph call is 225.196→202.446ms and the largest accumulated graph frame is 436.759→397.439ms. The median of each run's maximum accumulated graph frame is 225.196→393.187ms, because calls can land in the same frame. Do not conflate these statistics or claim that spikes were removed.

| Run | Build GUID | Battery start→end °C | Recorded thermal statuses |
|---|---|---|---|
| after-profiler-1 | 2b746a81138342559d29b3aa95321b18 | 36.0→37.0 | [1] |
| after-profiler-2 | 2b746a81138342559d29b3aa95321b18 | 37.0→38.0 | [1, 2] |
| after-profiler-3 | 2b746a81138342559d29b3aa95321b18 | 38.0→38.0 | [2] |
| before-profiler-1 | c48b29afedde48c49088542434e318da | 39.0→40.0 | [2] |
| before-profiler-2 | c48b29afedde48c49088542434e318da | 40.0→40.0 | [2] |
| before-profiler-3 | c48b29afedde48c49088542434e318da | 40.0→41.0 | [2] |

Health is run-wide, not aligned to individual captures. Actual temperatures/severities differ between cohorts and change during the retained sequence. CPU/core/GPU clocks were not measured; even the unchanged Physics marker changes. Neither favorable nor unfavorable deltas isolate the implementation.

The actual Profile Analyzer1.4 Compare confirms run2/level53: 667 paired markers, 560→609 selected/analyzed frames, Main Thread, inclusive time, all depths, no removed-marker filter. It is the same capture cross-checked through the package, not another independent repeat.

- COghe.Skin.Rebuild: mean per present frame 8.966→6.282ms; per selected frame 8.950→6.271ms; calls 559→608, present frames 559→608.
- COghe.Navigation.BuildGraph: mean per present frame 247.439→223.189ms; per selected frame 3.535→2.932ms; calls 9→9, present frames 8→8.

Limits:

- Levels2/7/23 are complete authored routes;53/65 are25-second prefixes. Different frame pacing can shift route progress and graph grouping inside fixed windows.
- Full level65 tap counts vary: [35, 35, 35]→[33, 31, 35]. Other selected route tap counts agree. This is an author replay using the developer HUD, not Product/Home evidence.
- The same recorded OPPO/Vulkan/resolution/profile metadata is used. Source contracts preserve graphics/gameplay; metadata alone does not independently measure every rendered-quality setting.
- Inclusive parent/child timers overlap; self-time before/after is not directly comparable where new child markers were added. Do not add marker columns as CPU busy time.
- Vulkan GPU timings are unavailable. Present/semaphore waits are not GPU execution time.
- This is not a60FPS result. Final Release acceptance awaits the fresh B–F–F–B–B–F cohort. Rejected navigation and cool-baseline Release evidence must remain excluded.

Release status at generation: completion marker absent; no fresh Release conclusion yet.

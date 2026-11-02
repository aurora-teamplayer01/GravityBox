# Isolated native functional rechecks — independent audit

Generated 2026-10-29T10:58:04.214392+00:00.

All three baseline and three retained-final runs completed the exact five levels 10/27/32/56/57. All 30 level records, report counts, monitor counts, exit codes, APK hashes, report build GUIDs and screenshot-free Release metadata agree. No timing result from these functional rechecks is pooled into the primary A/B.

| Level | Baseline passes /3 | Final passes /3 | Baseline taps (runs1/2/3) | Final taps (runs1/2/3) |
|---|---:|---:|---|---|
| 10 | 3 | 3 | [11, 11, 11] | [11, 11, 11] |
| 27 | 0 | 0 | [36, 75, 49] | [11, 36, 36] |
| 32 | 1 | 1 | [16, 17, 17] | [17, 17, 16] |
| 56 | 2 | 1 | [27, 26, 26] | [26, 25, 26] |
| 57 | 0 | 0 | [63, 63, 63] | [63, 63, 63] |

After-only observed failed levels: none. After-only observed first-error assertions: [{'level': 27, 'assertions': ['Across the back road onto the right shelf, clear of P2']}, {'level': 56, 'assertions': ['Handle C rejected: Cơ quan đang thực hiện']}]. These labels describe this small cohort, not causality or statistical proof.

First assertion by run (pass is retained as pass; detailed first error lines and sources are in JSON):

| Level | Side | Run1 | Run2 | Run3 |
|---|---|---|---|---|
| 10 | before | pass | pass | pass |
| 10 | after | pass | pass | pass |
| 27 | before | All parts meet and fuse | All parts meet and fuse | All parts meet and fuse |
| 27 | after | Across the back road onto the right shelf, clear of P2 | All parts meet and fuse | All parts meet and fuse |
| 32 | before | pass | No visible floor command for part 1 at (0.050, -0.300, -0.240) | No visible floor command for part 1 at (0.050, -0.300, -0.240) |
| 32 | after | No visible floor command for part 1 at (0.050, -0.300, -0.240) | No visible floor command for part 1 at (0.050, -0.300, -0.240) | pass |
| 56 | before | pass | Handle C rejected: | pass |
| 56 | after | Handle C rejected: Cơ quan đang thực hiện | pass | Handle C rejected: Cơ quan đang thực hiện |
| 57 | before | All parts meet and fuse | All parts meet and fuse | All parts meet and fuse |
| 57 | after | All parts meet and fuse | All parts meet and fuse | All parts meet and fuse |

The original full sweep remains **57/65**, failed IDs [10, 18, 27, 32, 48, 51, 56, 57]. Isolated passes do not replace those outcomes. The earlier historical baseline full sweep used screenshots; this baseline and final use the same current screenshot-free harness. Full versus isolated runs differ in process/scene history and timing. Baseline×3 then final×3 is a functional repeat sequence, not the temperature-gated interleaved FPS experiment.

Level 27 remains distinct by route step: the original final full sweep failed at the earlier right-shelf assertion; compare the explicit recheck assertions above rather than grouping every failure by level number alone. The earlier right-shelf failure occurred in baseline 0/3 and final 1/3 isolated repeats; all other L27 rechecks reached the later reunion failure.

The historical search found two distinct original native baseline L27 records (initial and retest APKs), both later reunion failures with 49 taps. Neither contains the earlier right-shelf assertion. These older screenshot-enabled records remain separate from the matched cohort; absence in those reports does not prove the earlier failure impossible on baseline. Exact historical APK/GUID identities are in recheck-l27-history.json.

The earlier L27 failures accepted the crossing command, then ended with the back-road gate almost closed, the waiting half outside P2, and the crossing half on the floor. The route checks geometric proximity to P2, not P2 activation at handoff; no pre-crossing pad load/gate history is logged. Loss of pad contact while waiting is a falsifiable hypothesis, not an established cause: a time series of pad load/active, gate position/target, supports and order cancellation around the crossing would distinguish it from a path/adhesion interruption. Screenshot-enabled replay changes pacing and can test timing sensitivity, but cannot alone isolate a code regression.

Level 56 Handle C rejection text can vary while describing the same route step; an after-only reason string is not automatically a new failure phase. Existing one-way-C completion and projection/touch explanations remain hypotheses unless state evidence confirms them. A reproduced baseline failure supports pre-existing sensitivity; it does not prove a harmless harness-only defect.

| Run | Battery start→end °C | Recorded thermal statuses |
|---|---|---|
| before-1 | 40.0→40.0 | [2] |
| before-2 | 40.0→41.0 | [2] |
| before-3 | 41.0→41.0 | [2] |
| after-1 | 41.0→41.0 | [2, 3] |
| after-2 | 41.0→41.0 | [3] |
| after-3 | 41.0→42.0 | [3] |

Local APK signatures verify successfully and share certificate SHA-256 `25eb779fc455fa564f6821cad70117b3ca50214767f8fa1ca1a68e1d21e43ffb`. Source APK SHA-256 identities and per-run GUID checks are recorded in JSON; the exact installed APK bytes were independently checked at final restoration.

Saved-log review found 0 crash-pattern hits across all six exact-PID/epoch-filtered streams; all six reports and monitors have empty runtime-error arrays. The collector includes Unity, AndroidRuntime and libc tags, so this establishes no observed crash in the retained streams, not a complete system-wide ANR/crash audit.

Recorded final restoration at 2026-10-29T10:55:38.848064+00:00: the pulled installed APK SHA-256 equals `26df2c98969cd38f500708e2c364e9a82dd9090bbb1407b5babdd6efe4e5fb39` and the retained readiness manifest. The normal launch returned Status: ok, a running PID and foreground activity for com.gravityboxlab.venom. The final Product completion record points to this recheck state. This is verification of the saved restoration evidence, not a fresh device/UI inspection by this reviewer.

This small, sequential cohort cannot establish or rule out an optimization regression. Final rechecks also ran hotter and reached a higher thermal status than baseline, as recorded above; temperature is another uncontrolled factor, not a proven cause. Review original and repeated failures together; no outcome is erased, and no scripted route success is substituted for novice/player usability evidence.

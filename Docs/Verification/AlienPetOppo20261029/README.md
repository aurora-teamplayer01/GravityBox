# Alien Pet — OPPO campaign audit, 29 October 2026

Device: OPPO CPH2591, Android 15, Mali-G52 MC2, 720 × 1612. Built the current
`NewGraphic` working tree (HEAD `afc640dc`, including the pending Alien Pet branding).
No pull, commit, save reset or gameplay/level redesign was performed.

## Build

- Unity 6000.3.19f1, ARM64 IL2CPP Release, Google test ad units.
- Package: `com.gravityboxlab.venom`; display name: **Alien Pet: Escape Puzzle**.
- APK: `Builds/SpatialLab/Android/COghe-Spatial.apk`.
- Initial sweep APK SHA-256: `442bc3ba29bc1a2f2ef1a7c2e052208beeab9740a38fc9569ae56aeb44b88b7a`.
- Installed retest APK SHA-256: `c715f25e611b4716186c1de15a66ff3ba3402068921fddba0f0e3f615e88e0a9`.
- The second build changes only the opt-in author test harness. It adds subset
  selection, swing diagnostics, and avoids issuing a redundant walk when a part
  is already at its destination.

## Coverage and results

The full native sweep exercised all **65 main campaign levels**, using InputSystem
touch events and ordinary player physics. Completion requires all 32 particles to
escape as one body, with the normal completion state and no loss. No mechanisms,
particle positions, puzzle state or win flags are assigned by the replay.

- First sweep: **60 passed, 5 did not complete the authored route**.
- All six bosses passed: **10, 21, 32, 43, 54, 65**.
- Level **56** passed on-device after correcting the test's arrival check. This was
  a test false failure: merging had already placed the body at the requested pad;
  the extra tap selected the body instead of creating a new movement order.
- **61/65** have a verified native solve across these runs. **18, 27, 48 and 51**
  remain under investigation; a failed author route is not proof that no solution
  exists for a human player.
- No Unity Error/Exception/Assert in the full sweep's error collection, and no
  Android/native crash observed in the captured logs. Caught scenario timeouts are
  recorded separately as failed cases.
- **25/25 focused Editor PlayMode tests passed**: six level-26 lift/ceiling cases,
  plus nineteen Clever, first-play, input, reward, Mint and presentation regressions.
- The two post-campaign prototypes, F2/F3 (66–67), are present in this test build
  but are outside this campaign sweep.

### Cases requiring follow-up

| Display level | Observation | Interpretation / next check |
| --- | --- | --- |
| 18 (N18) | The two halves stop near each other; repeated projected taps are consumed as selection rather than movement. Reproduced in a clean confirmation following level 17. A separate cold-start retry also missed its first Q-machine tap. | Verify touch visibility and a clear floor route to reunion in the normal UI. Do not change the proximity-based merge rule to make the test pass. |
| 27 (G9) | Repeated reunion taps do not produce the intended movement; one half reaches the exit before merging and the normal merge-required loss appears. Reproduced in the second run. | The loss rule is functioning. Inspect selection occlusion and the route from the wall control back onto the shelf. |
| 48 (N31) | In the initial sweep, the final exit tap creates a movement order, but the body stops on the floor before escaping. A later clean run stalled at the partition after reunion, with 8 particles supported by `Partition wall` and a movement order through the door. | Check the full-body clearance through the partition as well as the climb/exit approach. A middle attempt was contaminated by a manual camera drag and is not counted as a clean reproduction. The final confirmation had no manual intervention; levels 17 and 47 passed in that same run. |
| 51 (content 26) | Tapping the target dock is handled as releasing the rope onto the rescue floor. Reproduced twice; `Landings=0`, `Misses=0`, phase returns to Idle, message `Buông dây xuống sàn cứu hộ`. | Inspect dock picking/occlusion and rescue-floor priority. The global tissue-clearance message alone does not establish a clearance/physics bug. |

## Performance

The full sweep contains **34.6 minutes executing solve routes**, including their
screenshot overhead. The retained performance samples cover **31.3 minutes /
56,480 frames**; scene loading and victory presentation are outside that sample.
Frame-weighted average: **30.1 FPS**.

| Level | Mean FPS in first sweep |
| --- | ---: |
| 53 | 25.2 |
| 33 | 26.0 |
| 38 | 26.9 |
| 43 | 27.1 |
| 10 | 28.0 |

Opening levels ran roughly 39–45 FPS; most later levels ran near 29–30 FPS. The
phone was charging over USB and warmed from approximately 32 °C to 39–41 °C over
the sweep and retries; Android reported thermal throttling severity 2 (moderate)
during the long run. These results do **not** establish sustained 60 FPS.

The harness excludes explicit screenshot-capture frames and two settling frames,
but synchronous readback and garbage collection can still affect later samples.
This is a diagnostic gameplay soak, not a clean GPU benchmark or a claim that
every spike is from a particular subsystem. Level 53 also had the largest measured
mean game-step and graph costs, so it deserves a focused performance trace.

## Normal Product UI smoke test

Using ordinary Android screen taps after restarting without proof extras:

- Alien Pet main menu and Play opened correctly.
- Level 1 was solved by tapping the visible wall exit.
- Victory kept the standard reward and rewarded-ad button visible; it remained on
  the same win screen for another 13.9 seconds between recorded screenshots.
- Tapping “Level 2” opened level 2; Pause and the restart confirmation worked.
- Restart returned to active level 2, where the app was left open.
- Test ad banners loaded. Rewarded-video completion and real purchases were not tested.

This normal smoke test legitimately completed level 1 and earned its 10 Drops.
It did not reset any save. The automated campaign proofs did not persist progress.
See `product-smoke.json` and the Product screenshots.

## Test harness changes

- Android launches can select the Spatial catalog using `coghe_spatial=true`;
  desktop-only command-line parsing previously selected the old V2 catalog.
- `coghe_view_only` accepts a comma-separated subset within first/last bounds for
  targeted repeats in the opt-in proof mode.
- `Walk` accepts an already-reached destination before attempting another tap.
- Failure diagnostics include swing phase, actor, dock, landings and release reason.

Author tests disable persistence and use the developer HUD. They do not constitute
an end-to-end test of normal progression, monetization, Home, tutorials, ability
choice, or every possible wander/recovery route. They are not novice usability
evidence. Gameplay and approved layouts were left unchanged in this task.

## Evidence

- `campaign.csv`: all 65 first-sweep outcomes and frame statistics.
- `campaign-summary.json`: compact initial native report and failure details.
- `editor-tests.json`: the 25 individual Editor results.
- `retry-summary.json` and `confirmation-summary.json`: targeted native reruns, including the noted camera-drag contamination.
- `review.html`: browsable results and native screenshots.
- Raw native reports, images, logs, thermal samples and build logs are retained
  locally in `Artifacts/AlienPetOppo-2026-10-29/` (ignored by Git). Device identifiers
  and system notification screenshots are not copied into this report.

Reproduce the opt-in Spatial route on a connected Android test build:

```sh
adb shell am force-stop com.gravityboxlab.venom
adb shell am start -W -n com.gravityboxlab.venom/com.unity3d.player.UnityPlayerGameActivity \
  --es coghe_view_proof oppo-audit --ez coghe_spatial true \
  --ei coghe_view_first 1 --ei coghe_view_last 65
```

Add `--es coghe_view_only 18,27,48,51,56` for the subset. Stop the app and launch it
without these extras to return to the normal Product UI.

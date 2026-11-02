# Follow-up after the retained skin/per-tick candidate

Read-only, 29 October 2026. Sources: canonical `native-after/analysis/after-profiler-{1,2,3}-level-*`, `comparison/markers.csv`, `review/native-cpu-budget.json`, `review/final-release-review.md`, and current runtime. No Assets edits, Unity or device operations.

Release supports a skin-cost reduction (median 6.39–20.32%), not 60 FPS. Level 2 worsened in this cohort: 46.576→45.764 FPS; median P99 32.754→48.954 ms. Preserve that observation without asserting a code-only cause. Start gates were balanced; whole-run thermal/clock equivalence was not achieved. Full-65 verification remains separate.

## Where graph cost sits

Native level 53 averages **185.79–198.54 ms/build**; Edges accounts for **182.64–195.21 ms, 98.06–98.32%**. Nodes take 2.23–2.43 ms, Snapshot 0.63–1.05 ms, captured revision 0.27–0.33 ms. Edges also accounts for 91.3–91.6% at 23 and 93.9–94.3% at 65. These child shares must not be added to graph totals. A* Search is separate and small.

`VenomCampaignMotion.cs:271–325` constructs sorted nearby-node pairs and checks exact visibility. `VenomNavigationSnapshot.cs:85–128` traverses the AABB tree, then analytic aperture/current-collider checks. Unchanged graphs already skip rebuilding through `VenomNavigationRevision.Matches`. Release level 53 still reaches **379.746 ms accumulated graph time in one sampled frame**, not one call.

## Why the navigation cache was rejected

The static N43 repeat test cut BVH visits from 2.33 million to 0.94 million. However, 38,624 of 50,228 “hits” still executed a current exact blocker check. Pure answer reuse initially covered only 12,023/61,170 queries. Cold builds, changed exact world keys, and conservative dirty bounds for mutable meshes limit reuse; dictionaries, invalidation and added slab tests introduce work.

Those are structural limits, not a measured cost attribution. Experimental native timings improved at 53 but worsened at 7/23/65; Release was mixed, with thermal confounds and diagnostic overhead. Fewer visits did not establish Android benefit. The cache was conservatively rolled back; current Motion/Snapshot match their instrumented pre-optimization backups.

## Next three bounded experiments

1. **Explain level-2 tails and presentation cadence.** Repeat unchanged baseline/final Release on the same short route, retaining all thermal drift flags and identifying sampling boundaries. Attribute the few 49 ms frames to route phase, fixed ticks, GC and scheduling/clock state. Inspect native Main/Render Timeline and presentation timestamps. Success means a repeatable explanation of those tail events. Keep graphics and physics settings fixed; Present waits alone cannot establish GPU-bound execution.

2. **Localize level-53 exact visibility.** Diagnostic-only, bounded records per actual rebuild: caller, force flag, first changed revision item, frame/tick, node/pair counts, coarse candidate-versus-visibility time. Sample a small fixed query fraction for analytic/surface-collider/prop-collider attribution. Avoid per-visit timers on the normal path and measure flag-on overhead. Determine which blocker/call sequence dominates before choosing another algorithm. Retain full-scan/full-adjacency oracles; never loosen tolerances, reuse stale routes, merge calls across differing geometry, or infer cooked-collider equality from readable mesh arrays.

3. **Test one exact polygonizer dispatch change.** Level 23 still spends median 5.34 ms/frame in Polygonize within 6.88 ms Skin.Rebuild; Field/Upload are only 0.38/0.34 ms. Candidate: form a corner bitmask during the existing threshold loop, then replace repeated tetrahedron classification (`VenomSurface.cs:353–366`) with a 16-case table. Preserve tetrahedron-local corner order, directed Edge/Triangle call order, winding and every arithmetic operation downstream. Existing empty-cell skipping and edge caches remain. Exhaust all 16 cases for all six tetrahedra; require exact vertex/normal/ink/triangle and moving/split-surface equivalence. Keep only a repeatable Polygonize/Release benefit beyond run variation with no tail or allocation regression. IL2CPP may make the gain negligible; this is unproven.

## Fixed-step interpretation and remaining target

The 120 Hz simulation averages 4.02/4.63/4.47 ticks per displayed frame at 23/53/65. Four ticks at roughly 30 FPS are normal accounting, not proof of runaway backlog; long frames can add catch-up ticks. At 60 FPS the unchanged simulation averages two ticks. Report per-tick cost alongside frame cost; neither halve physics frequency nor halve current frame cost and claim 60 FPS.

Saved skin work partly appears as more waits. GPU timestamps are unavailable. Graphics/gameplay contracts and component savings are retained; substantial FPS improvement, 60 FPS and removal of graph spikes remain **unachieved**.

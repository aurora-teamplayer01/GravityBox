# Level 27 / G9: retained-source causal review
> Source-audit snapshot written during rechecks. The complete outcome and thermal limitations are in [the final independent audit](recheck-audit.md).


Read-only against frozen final-source-sha256.json. All nine current runtime hashes match the final manifest. Exact diff saved in `frozen-runtime-diff-for-level27.patch`; scene/script inventory in `level27-source-inventory.json`. No Assets, Unity or device operation. Pending native repeats must remain separate from these source findings.

| Retained file | Relevance to the failing crossing |
|---|---|
| VenomCampaignMotion, VenomNavigationSnapshot, VenomLevelController | Byte-identical to before-source. Movement/navigation may respond to differing poses, but their algorithm and fixed-step boundaries were not changed. |
| COgheTubeNetwork | No instance in G9, and no dynamic creation path found. Its scratch sets/root cache cannot execute for this crossing. |
| COgheRailSlider | Eight instances, including Back road gate. Conditional identical joint setters are the only retained change directly affecting this scene's physical mechanism calls. Frame/prop caching does not change the identities here. |
| VenomSurface | Mesh generation/cache/markers only. No direct skin-output-to-force, route or collision-picking path found. Timing/memory effects remain possible indirect influences. |
| COgheTapRail | Activity-string cache only; no movement, hand target, force or input logic changed. |
| VenomCampaignExpansion | Evaluates the selected mechanism Activity getter once instead of twice. G9 Activity getters are read-only. |
| VenomCampaign | Adds scoped markers around the same ordered calls. No changed force, solver rate or ordering. |

## Actual gate path

G9 has eight rails/eight props, two TapRails (A/C), two tissue sensors (P1/P2), one QuantumSplitter, one LoadLatch and two ViewMechanisms. Every rail Frame points to the fixed chamber Rigidbody; components are present before initialization and no runtime frame/prop replacement is used. Thus no stale-reference path was found.

The Back road gate starts Locked=false with both endpoint latches false. Its LoadLatch has Any=true, Retain=false, Inputs=[P1,P2], no Clearance. Reset unlocks it, and Retain=false prevents the Caught/locking transition. During the crossing this gate remains Limited; it does **not** depend on releasing a previously locked brake. The changed code skips repeatedly writing its identical midpoint anchor and Limited motion. Nonzero prop gravity/cancellation forces still occur each active tick, providing the documented wake path. Private Unity joint dirty/solver side effects remain unproven, as detailed in `rail-brake-side-effects-review.md`.

Unchanged LoadLatch computes target=0.12m while either sensor is active, otherwise target=0, and supplies finite effort. A lost pad load is therefore sufficient to explain a closed gate without any setter failure.

## What the final failure state establishes

P2 is at (0.34,-0.108,0.25), with a 0.09×0.09 weighed footprint under identity transforms. Particles must have x>0.295 to count. At after-1 timeout the runner's maximum x is0.28680; in the original final full65 failure it is0.28743. In both records **zero particles occupy either P1 or P2 planar footprint**, independent of the height test. The gate is correspondingly near its closed end (0.00042/0.00035m). Calculations and all32 recorded positions are preserved in `level27-pad-timeout-check.json`.

This establishes the terminal state, not the first divergence or why the runner lost its pad. Evidence-agent review found tap9 accepted the intended crossing command; baseline/final rounded targets match. Do not call this an initial tap rejection or prove the gate was asleep while powered.

The replay (`COgheSpatialPlusScenario.cs:124–138`) walks the runner to P2, then moves the holder off P1. `Walk` accepts centre distance<0.05m to the point plus0.02m up (`COgheSpatialNextScenario.cs:122–137`). It does not explicitly assert P2.Active before or during handoff. Consequently a successful Walk-to-P2 is not a persistent-load guarantee. Whether final code makes this handoff more fragile needs repeat/state evidence.

## Skin path exclusion

`VenomSurface.Initialize` creates only MeshFilter/MeshRenderer; no skin collider. DrawnParticles is populated before the changed BuildFragment loops using unchanged code. Game TouchPoint selects physical Matter.Bodies positions, then raycasts physical surface/prop colliders. Camera bounds come from authored surfaces and explicitly exclude renderer/skin queries. No G9 mechanism overrides HasSkinConstraint (only TubeNetwork does), so edge-cache hits cannot skip a mechanism mutation there. The clipping path operates on vertex/normal references and cached surface reads. Rebuilt callbacks still execute once after each rebuild. These facts exclude a direct mesh-picking or cached-skin force path; they do not exclude changed frame scheduling, GC, input sampling or a hidden engine defect.

## Verified isolated repeat update

Direct inspection of `native-rechecks/after-2/run-latest.json` confirms that G9 passed the earlier crossing and reached the later **All parts meet and fuse** assertion (36 taps, 63.437 seconds; C position=0.05702354m). This is the later assertion recorded by all three isolated baseline runs. The two completed isolated final runs therefore have different signatures: after-1 failed the back-road crossing, while after-2 reached reunion. The original final full65 crossing failure remains a separate observation, not another isolated paired replicate. Source file hashes and exact assertions are saved in `level27-verified-repeat-signatures.json`.

This disproves a deterministic final-build failure at that crossing under the current replay. It does not establish equivalent failure probability, identify the cause, or clear the joint-setter concern. The third final isolated repeat was still pending at this review. The original pre-optimization campaign and retry reports also fail later at reunion (49 taps each); their distinct APKs and screenshot cadence are documented in `recheck-l27-history.json`, so they must not be pooled with the matched isolated repeats. No earlier crossing failure was found in the retained historical search, which cannot establish that baseline is immune. Preserve the frozen build; diagnose the pressure-pad handoff and input/pose history before changing physics based on this small sample.

## Narrow next discriminator

First finish the already planned paired isolated repeats and classify the **same early crossing signature**, not merely pass/fail. Without Assets changes, a separate screenshot-enabled paired G9 replay can expose the P2 indicator/gate at existing pre-tap captures6–9 (`coghe_no_screenshots=false`); label it diagnostic because those captures add delays. The frozen APK has no setter-toggle or live-pad-load logging flag. Existing timeout dumps can establish terminal occupancy only.

If the early signature persists only in final, the smallest implementation bisection is restoring only the two unconditional SetBrake assignments while retaining component caches and skin changes, then repeating the identical protocol. That requires a new separately hashed build and authorization; it is a causal experiment, not yet a justified fix. The present evidence identifies RailSlider as the relevant physical candidate but does not establish it as the cause.

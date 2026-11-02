# Rail brake setter side-effect review
> Source-audit snapshot written during rechecks. The complete outcome and thermal limitations are in [the final independent audit](recheck-audit.md).


Read-only review, 29 October 2026. Runtime/APK remain frozen; no Unity/device/build/test execution. The native failures at 10/27/32/56/57 are not attributed to this change; matched isolated baseline/final rechecks were pending.

**Conclusion:** no concrete rail defect found. Exact equality proves that the requested exposed anchor/motion state is unchanged, but by itself is not a proof about every hidden Unity setter side effect. Current authored rails have an independent documented wake path, and upstream PhysX 4.1 dirty handling does not show an unconditional wake or warm-start reset for these setters. The actionable gap is targeted solver-equivalence coverage, not evidence that the native failures were caused by SetBrake.

## Current path and lifetime

- `COgheRailSlider.cs:62–70` still recomputes the anchor, captures it on brake entry, writes every unequal anchor/motion value, and repairs external changes. Unlock/latch transitions write the changed xMotion; reset refreshes references, resets brakeSet, positions the body and calls SetBrake. Small authored adjustments are preserved by Vector3.Equals rather than Unity's approximate Vector3 operator.
- Every active rail tick calls nonzero upward gravity cancellation when its co-located prop exists, even when Locked or Latched (`:85`). Campaign applies nonzero downward gravity earlier (`VenomCampaign.cs:210–214`). Cancellation of their net force does not erase the fact that nonzero AddForce calls were issued. Unity 6000.3 documents that nonzero AddForce wakes an active, non-kinematic body. This makes “removing the identical setter removed the rail's only wake” unsupported for current authored rails. [Unity AddForce](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody.AddForce.html)
- Read-only YAML inventory found **699 rails in 176 authored Venom scenes**, including archives: all have a co-located VenomMovableProp, non-kinematic Body and autoConfigureConnectedAnchor=false. The builder likewise adds the rail to the prop object and adds it to campaign Props (`VenomExpansionMechanismsBuilder.cs:58–69`). Inventory: `rail-authored-inventory.json`. This is authored-scene evidence, not observation of every live object's activation state.
- FrameBody refreshes on Frame reference change or destroyed/missing cached Rigidbody. Prop/frame references refresh at initialization/reset and lazy first step. No runtime prop add/remove or Rigidbody replacement path was found. A hypothetical prop added after initialization without reset is not refreshed, but there is no current authored/runtime use of that lifecycle; do not label it the cause of these failures.

## Primary-source limits

The local Unity test log reports PhysX SDK **4.1.2**, integration 1.0.0. In upstream PhysX 4.1, D6 setMotion stores motion, requests recomputation and marks dirty; setLocalPose updates local/COM-relative pose and marks dirty. NpConstraint::markDirty only sets a dirty flag; the update path recopies the constraint constants. Explicit wakeUpActors calls appear separately for drive setters. No unconditional wake or warm-start reset was found on the examined setMotion/local-pose path. This supports the intended optimization; it is not proof of Unity's private 6000.3.19f1 native wrapper behavior. Unity's public C# bindings expose these properties as extern and cannot settle that final question.

Sources: [PhysX D6Joint](https://github.com/NVIDIAGameWorks/PhysX/blob/4.1/physx/source/physxextensions/src/ExtD6Joint.cpp), [ExtJoint local pose](https://github.com/NVIDIAGameWorks/PhysX/blob/4.1/physx/source/physxextensions/src/ExtJoint.h), [NpConstraint dirty/update](https://github.com/NVIDIAGameWorks/PhysX/blob/4.1/physx/source/physx/src/NpConstraint.cpp), [constant-block update](https://github.com/NVIDIAGameWorks/PhysX/blob/4.1/physx/source/simulationcontroller/src/ScConstraintSim.cpp), [Unity joint bindings](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Physics/ScriptBindings/Joint.bindings.cs).

## What the four targeted tests do and miss

All four in `COgheMechanismHotLoopTests` are synchronous tests and never invoke Physics.Simulate:

- `BrakeKeepsItsCaptureUntilReleasedAndRepairsChangedConfiguration`: anchor hold/release, tiny adjustment, external configuration repair and reverse latch release.
- `RailRetainsTheSameQueuedForceAndRefreshesItsFrameReference`: queued force values and reassigned carrier identity.
- `TapActivityRefreshesOnStateAndLabelChangesButReusesUnchangedText`: text cache only.
- `TubeReconciliationRevisitsTheSameGroupsAfterMergeAndNewCommands`: ownership/scratch-set reuse only.

Thus they do not establish asleep/awake equivalence, joint solver warm-start equivalence or time-series carriage trajectories. Existing `COgheMechanismExpansionTests` contain actual Physics.Simulate carriage/load/latch/rotated-frame tests, but they are not a differential comparison with the former unconditional setters; do not imply the four hot-loop tests cover them.

If matched rechecks leave a setter-specific concern, the narrow validation is an isolated pair of identical rail fixtures: current conditional SetBrake versus former unconditional SetBrake before each physics step. Compare IsSleeping, position/velocity, joint force and latch state through locked idle, explicit Sleep, unlock with zero/nonzero effort, supported load, frame rotation/translation and Retry. Include the real prop cancellation path and a prop-free fixture. No runtime WakeUp addition or rollback is justified by this read-only audit alone; wait for the pending native recheck evidence.

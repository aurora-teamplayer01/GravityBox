using System;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    /// <summary>Contracts affected by reused mechanism scratch state and cached references; no scene/save mutation.</summary>
    public sealed class COgheMechanismHotLoopTests
    {
        private const float Dt = 1f / 120;
        private readonly List<GameObject> objects = new List<GameObject>();
        private SimulationMode previousSimulation;
        private float previousTimeScale;

        [SetUp] public void Before()
        { previousSimulation = Physics.simulationMode; previousTimeScale = Time.timeScale; Physics.simulationMode = SimulationMode.Script; }
        [TearDown] public void After()
        {
            foreach (var go in objects) if (go != null) Object.DestroyImmediate(go);
            objects.Clear(); Physics.simulationMode = previousSimulation; Time.timeScale = previousTimeScale;
        }

        private GameObject Make(string name, bool active = true)
        { var go = new GameObject(name); go.SetActive(active); objects.Add(go); return go; }

        private COgheRailSlider Rail(bool prop = false)
        {
            var frame = Make("rail frame").AddComponent<Rigidbody>(); frame.isKinematic = true;
            var body = Make("rail carriage").AddComponent<Rigidbody>(); body.useGravity = false; body.mass = .05f;
            if (prop) body.gameObject.AddComponent<VenomMovableProp>().Body = body;
            var joint = body.gameObject.AddComponent<ConfigurableJoint>(); joint.connectedBody = frame;
            joint.autoConfigureConnectedAnchor = false; joint.axis = Vector3.right;
            var rail = body.gameObject.AddComponent<COgheRailSlider>();
            rail.Frame = frame.transform; rail.Body = body; rail.Joint = joint; rail.Axis = Vector3.right;
            rail.Travel = .12f; rail.InitialTravel = .03f; rail.Gravity = false;
            return rail;
        }

        [Test]
        public void BrakeKeepsItsCaptureUntilReleasedAndRepairsChangedConfiguration()
        {
            var rail = Rail(); rail.Locked = true; rail.ResetMechanism(null);
            Assert.AreEqual(ConfigurableJointMotion.Locked, rail.Joint.xMotion);
            Assert.That(rail.Joint.connectedAnchor.x, Is.EqualTo(.03f).Within(1e-7f));
            rail.Body.position = rail.Frame.TransformPoint(Vector3.right * .08f);
            rail.StepMechanism(null, Dt);
            Assert.That(rail.Joint.connectedAnchor.x, Is.EqualTo(.03f).Within(1e-7f), "A held brake must not recapture the displaced carriage.");

            rail.Locked = false; rail.StepMechanism(null, Dt);
            Assert.AreEqual(ConfigurableJointMotion.Limited, rail.Joint.xMotion);
            Assert.That(rail.Joint.connectedAnchor.x, Is.EqualTo(.06f).Within(1e-7f));
            // Less than Vector3.operator=='s tolerance: a configuration change must still reach the joint.
            rail.Start = Vector3.right * .000001f; rail.StepMechanism(null, Dt);
            Assert.AreEqual(rail.Start + rail.Axis * (rail.Travel * .5f), rail.Joint.connectedAnchor);
            rail.Joint.connectedAnchor = Vector3.one; rail.Joint.xMotion = ConfigurableJointMotion.Free;
            rail.StepMechanism(null, Dt);
            Assert.AreEqual(rail.Start + rail.Axis * (rail.Travel * .5f), rail.Joint.connectedAnchor);
            Assert.AreEqual(ConfigurableJointMotion.Limited, rail.Joint.xMotion);

            rail.Start = Vector3.zero; rail.LatchAtEnd = true; rail.Body.position = Vector3.right * rail.Travel;
            rail.StepMechanism(null, Dt); Assert.IsTrue(rail.Latched);
            rail.ApplyEffort(-Vector3.right * .04f); rail.StepMechanism(null, Dt);
            Assert.IsFalse(rail.Latched, "A deliberate reverse pull still releases the terminal catch.");
            Assert.AreEqual(ConfigurableJointMotion.Limited, rail.Joint.xMotion);
        }

        [Test]
        public void RailRetainsTheSameQueuedForceAndRefreshesItsFrameReference()
        {
            var rail = Rail(true); rail.ResetMechanism(null);
            rail.Body.linearVelocity = Vector3.right * .04f;
            rail.ApplyEffort(new Vector3(.12f, .03f, .04f)); rail.StepMechanism(null, Dt);
            Vector3 force = rail.Body.GetAccumulatedForce(Dt);
            Assert.That(force.x, Is.EqualTo(.1018f).Within(1e-6f), "Finite effort, dry resistance and damping are unchanged.");
            Assert.That(force.y, Is.EqualTo(rail.Body.mass * 9.81f).Within(1e-6f), "A campaign prop still cancels its explicit world gravity.");
            Assert.That(force.z, Is.EqualTo(0).Within(1e-6f));
            var replacement = Make("replacement frame").AddComponent<Rigidbody>(); replacement.isKinematic = true;
            rail.Frame = replacement.transform;
            Assert.AreSame(replacement, rail.FrameBody, "Reauthoring a carrier frame invalidates the component reference.");
        }

        [Test]
        public void TapActivityRefreshesOnStateAndLabelChangesButReusesUnchangedText()
        {
            var tap = Make("tap").AddComponent<COgheTapRail>(); tap.Label = "A";
            SetProperty(tap, "Phase", COgheTapRail.TaskPhase.Approaching);
            string approaching = tap.Activity;
            Assert.AreEqual("A · Đang tới", approaching); Assert.AreSame(approaching, tap.Activity);
            SetProperty(tap, "Phase", COgheTapRail.TaskPhase.Operating);
            Assert.AreEqual("A · Đang chuyển", tap.Activity);
            SetProperty(tap, "Holding", true); Assert.AreEqual("A · Đang giữ", tap.Activity);
            tap.Label = "B"; Assert.AreEqual("B · Đang giữ", tap.Activity);
            SetProperty(tap, "Holding", false);
            tap.RequiredGrip = Make("required grip").AddComponent<COgheTapRail>();
            Assert.AreEqual("B · Chờ nhả phanh", tap.Activity);
            SetProperty(tap, "Phase", COgheTapRail.TaskPhase.Idle); Assert.IsNull(tap.Activity);
        }

        [Test]
        public void TubeReconciliationRevisitsTheSameGroupsAfterMergeAndNewCommands()
        {
            // Construct only the ownership graph read by ReconcileGroups; no physical transport is bypassed in solve tests.
            var host = Make("inactive ownership fixture", false);
            var owner = host.AddComponent<VenomLevelController>(); var game = host.AddComponent<VenomCampaign>();
            var matter = host.AddComponent<CohesiveOrganism>(); var tube = host.AddComponent<COgheTubeNetwork>();
            SetProperty(owner, "Organism", matter); SetProperty(game, "Owner", owner);
            int[] groups = new int[32]; for (int i = 16; i < 32; i++) groups[i] = 16;
            SetProperty(matter, "Groups", groups);
            typeof(COgheTubeNetwork).GetField("game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tube, game);
            var travels = (Array)typeof(COgheTubeNetwork).GetField("travelByParticle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tube);
            Type travelType = travels.GetType().GetElementType();
            object first = Activator.CreateInstance(travelType, true), newer = Activator.CreateInstance(travelType, true);
            travelType.GetField("CommandId").SetValue(first, 1); travelType.GetField("CommandId").SetValue(newer, 2);
            travels.SetValue(first, 0); travels.SetValue(newer, 16);
            var reconcile = (Action)Delegate.CreateDelegate(typeof(Action), tube,
                typeof(COgheTubeNetwork).GetMethod("ReconcileGroups", BindingFlags.NonPublic | BindingFlags.Instance));
            reconcile();
            for (int i = 0; i < 32; i++) Assert.AreSame(i < 16 ? first : newer, travels.GetValue(i));
            Array.Clear(groups, 0, groups.Length); reconcile();
            for (int i = 0; i < 32; i++) Assert.AreSame(newer, travels.GetValue(i), "A merge chooses the newest command even when group 0 was processed last tick.");
            object latest = Activator.CreateInstance(travelType, true); travelType.GetField("CommandId").SetValue(latest, 3);
            travels.SetValue(latest, 7); reconcile();
            for (int i = 0; i < 32; i++) Assert.AreSame(latest, travels.GetValue(i), "A reused scratch set must not hide a subsequent command.");
        }

        private static void SetProperty(object instance, string name, object value)
            => instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true).Invoke(instance, new[] { value });
    }
}

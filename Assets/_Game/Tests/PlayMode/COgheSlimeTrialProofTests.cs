using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    /// <summary>Slime Trial proofs: solves at 60 and 30 fps (same 120 Hz physics), cancel cases, wander/recovery, portrait
    /// layout, one real Input System pass (mouse and touch, screen pixels). Launch constants come from the search
    /// (Artifacts/SlimeTrial/search-*.txt); each is (angle in degrees, power 0…1) through the player's pointer path.</summary>
    public sealed partial class COgheSlimeTrialTests
    {
        // ---- found by SlimeSearchS1/S2/S3 (see the dossiers) --------------------------------------------------------
        public static readonly Vector2[] S1Direct={new Vector2(65,.95f)};
        public static readonly Vector2[] S1MissThenFix={new Vector2(40,.55f),new Vector2(90,.7f)};
        public static readonly Vector2[] S2Solution={new Vector2(-30,.25f),new Vector2(15,.3f),new Vector2(65,.9f),new Vector2(70,.5f)};
        public static readonly Vector2[] S3WayA={new Vector2(75,.6f),new Vector2(-80,.2f)};
        public static readonly Vector2[] S3WayB={new Vector2(185,.3f),new Vector2(5,.3f),new Vector2(0,.35f)};

        private IEnumerator SolveAt(string key,string label,Vector2[] shots,int fps)
        {
            Time.captureFramerate=fps;ticksPerFrame=Mathf.RoundToInt(1/(Dt*fps));
            try
            {
                yield return Load(key);var trace=new StringBuilder();
                foreach(var v in shots)
                {
                    var o=new Outcome();yield return Shoot(new Shot(v.x,v.y),o);trace.Append($"\n  {v.x:0.#}°/{v.y:0.##} -> {State()}");
                    Assert.IsTrue(o.Taken||game.Owner.Completed,$"{key} {label}: launch {v} not taken{trace}");
                    Assert.IsFalse(game.Owner.Lost,$"{key} {label}: lost ({game.Failure}){trace}");
                    if(game.Owner.Completed)break;
                }
                yield return Ticks(.6f);
                Assert.IsTrue(game.Owner.Completed,$"{key} {label} at {fps} fps did not win{trace}");
                Assert.AreEqual(32,game.Matter.EscapedCount,"every particle went through the vent");
                Assert.IsFalse(game.Progress.Completed.Contains(game.Definition.Id),"the trial writes no campaign progress");
                File.AppendAllText($"{Out}/solves.txt",$"{DateTime.UtcNow:HH:mm} {key} {label} {fps}fps launches={slime.Launches} sim={game.Matter.SimulationTime:F1}s{trace}\n");
            }
            finally{Time.captureFramerate=0;ticksPerFrame=2;}
        }
        /// <summary>Codex review gate: a Retry first (a different launch, then THỬ LẠI through the HUD), then the full route.</summary>
        private IEnumerator SolveAfterRetry(string key,string label,Vector2[] shots,int fps)
        {
            Time.captureFramerate=fps;ticksPerFrame=Mathf.RoundToInt(1/(Dt*fps));
            try
            {
                yield return Load(key);yield return Shoot(new Shot(100,.5f));
                Assert.IsTrue(slime.PressHud(COgheSlimeControl.Layout(new Rect(0,0,width,height)).Retry.center),"THỬ LẠI");yield return Ticks(.6f);
                Assert.AreEqual(0,slime.Launches);var trace=new StringBuilder();
                foreach(var v in shots){var o=new Outcome();yield return Shoot(new Shot(v.x,v.y),o);trace.Append($"\n  {v.x:0.#}°/{v.y:0.##} -> {State()}");Assert.IsFalse(game.Owner.Lost,$"{key} {label} after retry: lost{trace}");if(game.Owner.Completed)break;}
                yield return Ticks(.6f);Assert.IsTrue(game.Owner.Completed,$"{key} {label} after retry at {fps} fps did not win{trace}");
                File.AppendAllText($"{Out}/solves.txt",$"{DateTime.UtcNow:HH:mm} {key} {label} AFTER RETRY {fps}fps launches={slime.Launches}{trace}\n");
            }
            finally{Time.captureFramerate=0;ticksPerFrame=2;}
        }
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1AfterRetry60(){yield return SolveAfterRetry("S1","one launch",S1Direct,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1AfterRetry30(){yield return SolveAfterRetry("S1","one launch",S1Direct,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS2AfterRetry60(){yield return SolveAfterRetry("S2","ram crate",S2Solution,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS2AfterRetry30(){yield return SolveAfterRetry("S2","ram crate",S2Solution,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayAAfterRetry60(){yield return SolveAfterRetry("S3","A ride the wind",S3WayA,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayAAfterRetry30(){yield return SolveAfterRetry("S3","A ride the wind",S3WayA,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayBAfterRetry60(){yield return SolveAfterRetry("S3","B stop the fan",S3WayB,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayBAfterRetry30(){yield return SolveAfterRetry("S3","B stop the fan",S3WayB,30);}

        // ---- review P2-1 / P2-2 ------------------------------------------------------------------------------------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeReleaseThenCancelBeforeTick()
        {
            yield return Load("S1");yield return Settle();Vector3 a=W(0,.10f),b=a+PullFor(60,.7f);
            foreach(var cancel in new Action[]{()=>game.SendMessage("OnApplicationFocus",false),()=>game.Owner.TogglePause()})
            {
                game.BeginPointer(S(a));game.MovePointer(S(b));game.EndPointer(S(b));   // queued for the next tick
                cancel();if(game.Owner.Paused){yield return null;game.Owner.TogglePause();}
                yield return Ticks(.5f);Assert.AreEqual(0,slime.Launches,"a release cancelled before the physics tick never launches on resume");
                Assert.Less(Speed(),.05f,"the body stayed put");
            }
        }
        [UnityTest,Timeout(900000)] public IEnumerator SlimeVentExitVsGlassTunnel()
        {
            // a fast valid entry: the strongest direct S1 shot into the vent is a win, never "out of the box"
            yield return Load("S1");yield return Shoot(new Shot(S1Direct[0].x,1f));yield return Ticks(1);
            Assert.IsFalse(game.Owner.Lost,"a fast entry through the vent is not a failure: "+State());
            for(int k=0;k<10&&!game.Owner.Completed;k++)yield return Ticks(.5f);   // bounded: at most 5 s more
            Assert.IsTrue(game.Owner.Completed,"the fast valid entry wins: "+State());
            Assert.AreEqual(32,game.Matter.EscapedCount,"every particle went through the vent");
            // the detector: a particle put through the glass right beside the hole (test-only placement, not a solution)
            yield return Load("S1");yield return Settle();
            var outlet=game.Owner.Outlet;var body=game.Matter.Bodies[0];
            body.position=outlet.TransformPoint(new Vector3(.10f,0,.04f));body.linearVelocity=Vector3.zero;body.transform.position=body.position;Physics.SyncTransforms();
            for(int k=0;k<3;k++)Tick();
            Assert.IsTrue(game.Owner.Lost,"tissue through the glass beside the vent fails the level");StringAssert.Contains("ngoài hộp",game.Failure);
        }

        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1SolveOneLaunch60(){yield return SolveAt("S1","one launch",S1Direct,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1SolveOneLaunch30(){yield return SolveAt("S1","one launch",S1Direct,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1MissThenFix60(){yield return SolveAt("S1","miss then fix",S1MissThenFix,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS1MissThenFix30(){yield return SolveAt("S1","miss then fix",S1MissThenFix,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS2Solve60(){yield return SolveAt("S2","ram crate",S2Solution,60);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS2Solve30(){yield return SolveAt("S2","ram crate",S2Solution,30);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayA60(){yield return SolveAt("S3","A ride the wind",S3WayA,60);var f=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();Assert.IsTrue(f.Running,"way A keeps the fan on");Assert.Greater(f.Carried,.2f,"way A is carried by the wind");}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayA30(){yield return SolveAt("S3","A ride the wind",S3WayA,30);var f=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();Assert.IsTrue(f.Running);Assert.Greater(f.Carried,.2f);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayB60(){yield return SolveAt("S3","B stop the fan",S3WayB,60);var f=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();Assert.IsFalse(f.Running,"way B stopped the fan");Assert.Less(f.Carried,.05f,"way B never rode the wind");}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3WayB30(){yield return SolveAt("S3","B stop the fan",S3WayB,30);var f=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();Assert.IsFalse(f.Running);Assert.Less(f.Carried,.05f);}

        // ---- the force law does not depend on the frame rate ---------------------------------------------------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeLaunchSpeedIndependentOfFrameRate()
        {
            var velocity=new Vector3[2];var after=new Vector3[2];int[] rates={60,30};
            for(int r=0;r<2;r++)
            {
                Time.captureFramerate=rates[r];ticksPerFrame=Mathf.RoundToInt(1/(Dt*rates[r]));
                try{yield return Load("S1");yield return Settle();yield return Pull(60,.7f);for(int k=0;k<2;k++)Tick();Assert.AreEqual(1,slime.Launches);velocity[r]=slime.History[0].Velocity;for(int k=0;k<30;k++)Tick();after[r]=Body;}
                finally{Time.captureFramerate=0;ticksPerFrame=2;}
            }
            Assert.Less((velocity[0]-velocity[1]).magnitude,1e-4f,$"launch velocity 60 fps {velocity[0]:F4} vs 30 fps {velocity[1]:F4}");
            Assert.AreEqual(COgheSlimeControl.SpeedFor(.7f),velocity[0].magnitude,.02f,"speed follows the power law");
            Assert.Less((after[0]-after[1]).magnitude,.02f,$"flight 0.25 s later: {after[0]:F3} vs {after[1]:F3}");
            File.AppendAllText($"{Out}/framerate.txt",$"{DateTime.UtcNow:HH:mm} v60={velocity[0]:F4} v30={velocity[1]:F4} p60={after[0]:F4} p30={after[1]:F4}\n");
        }

        // ---- cancel cases ----------------------------------------------------------------------------------------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeCancelCases()
        {
            yield return Load("S1");yield return Settle();var log=new StringBuilder();
            Vector3 a=W(0,.10f);var l=COgheSlimeControl.Layout(new Rect(0,0,width,height));
            // 1. a tiny pull (inside the dead zone) does nothing
            game.BeginPointer(S(a));Tick();game.MovePointer(S(a+Vector3.left*.012f));game.EndPointer(S(a+Vector3.left*.012f));yield return Ticks(.2f);
            Assert.AreEqual(0,slime.Launches,"tiny pull: no launch");Assert.IsFalse(slime.Pulling);log.AppendLine($"tiny pull: cancel '{slime.LastCancel}'");
            // 2. a huge pull is clamped to 100% and MaxSpeed
            game.BeginPointer(S(W(.20f,.40f)));game.MovePointer(S(W(-.25f,-.20f)));
            Assert.AreEqual(1f,slime.Power,1e-4f,"pull beyond MaxPull clamps to 100%");Assert.AreEqual(COgheSlimeControl.MaxSpeed,slime.PreviewVelocity.magnitude,1e-3f);log.AppendLine($"max pull: power {slime.Power:P0} speed {slime.PreviewVelocity.magnitude:F2}");
            // 3. pause mid-drag cancels and clears the pull
            game.Owner.TogglePause();yield return null;yield return null;
            Assert.IsFalse(slime.Pulling,"pause cancels the pull");game.EndPointer(S(W(-.25f,-.20f)));game.Owner.TogglePause();yield return Ticks(.3f);
            Assert.AreEqual(0,slime.Launches,"no launch after a paused drag");log.AppendLine($"pause mid-drag: cancel '{slime.LastCancel}'");
            // 4. release outside the play area (over the button strip) cancels
            game.BeginPointer(S(a));game.MovePointer(S(a+Vector3.down*.1f));game.MovePointer(new Vector2(width*.5f,4));game.EndPointer(new Vector2(width*.5f,4));yield return Ticks(.2f);
            Assert.AreEqual(0,slime.Launches,"release outside the play area: no launch");StringAssert.Contains("outside",slime.LastCancel);log.AppendLine($"release outside: cancel '{slime.LastCancel}'");
            // 5. a press on a HUD button never starts a pull
            game.BeginPointer(l.Pause.center);Assert.IsFalse(slime.Pulling,"a press on DỪNG is not a pull");game.EndPointer(l.Pause.center);
            // 6. app focus loss mid-drag cancels
            game.BeginPointer(S(a));game.MovePointer(S(a+new Vector3(-.08f,-.08f,0)));game.SendMessage("OnApplicationFocus",false);
            Assert.IsFalse(slime.Pulling,"focus loss cancels the pull");game.EndPointer(S(a+new Vector3(-.08f,-.08f,0)));yield return Ticks(.3f);Assert.AreEqual(0,slime.Launches);log.AppendLine($"focus loss: cancel '{slime.LastCancel}'");
            // 7. retry mid-drag cancels and resets
            game.BeginPointer(S(a));game.MovePointer(S(a+new Vector3(-.08f,-.08f,0)));Assert.IsTrue(slime.PressHud(l.Retry.center),"THỬ LẠI takes the press");
            Assert.IsFalse(slime.Pulling,"retry cancels the pull");game.EndPointer(S(a+new Vector3(-.08f,-.08f,0)));yield return Ticks(.6f);Assert.AreEqual(0,slime.Launches,"no launch after retry mid-drag");
            // 8. a release while airborne is refused (no mid-air launch)
            yield return Settle();yield return Pull(90,.8f);for(int k=0;k<12;k++)Tick();Assert.AreEqual(1,slime.Launches);Assert.IsFalse(slime.Anchored,"airborne");
            game.BeginPointer(S(a));game.MovePointer(S(a+new Vector3(.1f,-.1f,0)));game.EndPointer(S(a+new Vector3(.1f,-.1f,0)));for(int k=0;k<2;k++)Tick();
            Assert.AreEqual(1,slime.Launches,"no launch in the air");Assert.AreEqual(1,slime.Refusals,"the refusal is counted (and shown)");log.AppendLine($"airborne release refused: toast '{slime.Toast}'");
            File.WriteAllText($"{Out}/cancel-cases.txt",log.ToString());
        }

        // ---- retry restores everything ---------------------------------------------------------------------------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeRetryRestoresMechanisms()
        {
            yield return Load("S3");var fan=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();var crate=game.Owner.Apparatus.GetComponentInChildren<COgheRailSlider>();Vector3 spawn=Body;
            yield return Shoot(new Shot(S3WayB[0].x,S3WayB[0].y));Assert.IsFalse(fan.Running,"the ram stopped the fan");Assert.IsFalse(fan.Guard.activeSelf,"the shutter opened");
            game.ResetLevel();yield return Ticks(.6f);
            Assert.IsTrue(fan.Running,"Retry restarts the fan");Assert.IsTrue(fan.Guard.activeSelf,"Retry closes the shutter");Assert.AreEqual(0,crate.Position,.002f,"Retry returns the crate");
            Assert.AreEqual(0,slime.Launches);Assert.IsFalse(slime.Pulling);Assert.Less((Body-spawn).magnitude,.02f,"Retry returns the slime");
            yield return Load("S2");var laser=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeLaser>();crate=game.Owner.Apparatus.GetComponentInChildren<COgheRailSlider>();
            foreach(var v in S2Solution.Take(2))yield return Shoot(new Shot(v.x,v.y));Assert.IsFalse(laser.On,"the ram switched the laser off");
            game.ResetLevel();yield return Ticks(.6f);Assert.IsTrue(laser.On,"Retry switches the laser on");Assert.AreEqual(0,crate.Position,.002f);
            // touching the laser is a failure with a message; Retry recovers
            yield return Shoot(new Shot(0,.6f));yield return Ticks(1f);
            Assert.IsTrue(game.Owner.Lost,"a throw from the ledge into the curtain fails: "+State());StringAssert.Contains("laser",game.Failure);
            game.ResetLevel();yield return Ticks(.6f);Assert.IsFalse(game.Owner.Lost);Assert.IsTrue(slime.Anchored);
        }

        // ---- S3: the running fan's shutter closes the duct; without wind nothing clears the divider -----------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeS3NeedsOneOfTheTwoWays()
        {
            yield return Load("S3");var fan=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();var log=new StringBuilder();
            foreach(var v in new[]{new Vector2(0,.4f),new Vector2(0,.8f),new Vector2(5,1f),new Vector2(-5,.6f)})
            {
                game.ResetLevel();yield return Ticks(.6f);yield return Shoot(new Shot(v.x,v.y));
                log.AppendLine($"fan on, flat {v}: {State()}");Assert.Less(Body.x,.05f,"the shut duct stops a flat launch");
            }
            // with the fan stopped, the strongest launches from the floor and the crate top never clear the divider
            foreach(var v in new[]{new Vector2(80,1f),new Vector2(70,1f),new Vector2(60,1f),new Vector2(85,1f)})
            {
                game.ResetLevel();yield return Ticks(.6f);yield return Shoot(new Shot(S3WayB[0].x,S3WayB[0].y));Assert.IsFalse(fan.Running);
                float highest=-1;perTick=()=>highest=Mathf.Max(highest,Body.y);
                yield return Shoot(new Shot(v.x,v.y));perTick=null;
                log.AppendLine($"fan off, {v}: highest centre {highest:F3} -> {State()}");Assert.Less(highest,.38f+.03f,"no launch clears the 38 cm divider without wind");
            }
            File.WriteAllText($"{Out}/s3-needs-a-way.txt",log.ToString());
        }

        // ---- wander: random launches, no softlock without Retry, no NaN, nothing leaves the box -----------------------
        private IEnumerator Wander(string key,int seed,int launches)
        {
            yield return Load(key);var rng=new System.Random(seed);var log=new StringBuilder();int fails=0,lasers=0,stuck=0,wins=0;
            float outside=0;
            // independent oracle (not the runtime's vent predicate): once the body has settled, every particle the exit has
            // not recorded as escaped is inside the box, and nothing is NaN
            void Check(){for(int i=0;i<game.Matter.Bodies.Length;i++){var p=Local(game.Matter.Bodies[i].position);if(float.IsNaN(p.x))outside=999;
                if(game.Owner.Completed||game.Matter.Escaped[i])continue;
                if(Mathf.Abs(p.x)>.31f||p.y<-.31f||p.y>.51f||Mathf.Abs(p.z)>.11f)outside=Mathf.Max(outside,p.magnitude);}}
            ticksPerFrame=8;
            for(int i=0;i<launches;i++)
            {
                if(game.Owner.Lost||game.Owner.Completed)
                {
                    if(game.Owner.Lost){fails++;if(game.Failure!=null&&game.Failure.Contains("laser"))lasers++;else Assert.Fail($"{key} wander: lost for a reason other than the laser: {game.Failure} {State()}");}
                    else wins++;
                    game.ResetLevel();yield return Ticks(.6f);Assert.AreEqual(0,slime.Launches,"Retry clears the launches");
                }
                yield return Settle(6);
                if(!slime.Anchored&&!game.Owner.Lost&&!game.Owner.Completed){stuck++;log.AppendLine($"  {i}: not anchored after 6 s: {State()} -> Retry");game.ResetLevel();yield return Ticks(.6f);continue;}
                var s=new Shot((float)rng.NextDouble()*360,.05f+(float)rng.NextDouble()*.95f);
                yield return Shoot(s);Check();log.AppendLine($"  {i}: {s} -> {State()}");
                if(game.Owner.Lost&&game.Failure!=null&&game.Failure.Contains("ngoài hộp"))Assert.Fail($"{key} wander: failed as out of the box: {State()}");
            }
            perTick=null;ticksPerFrame=2;
            File.WriteAllText($"{Out}/wander-{key}.txt",$"{key} seed {seed}: {launches} launches, wins {wins}, failures {fails} (laser {lasers}), not anchored {stuck}, max outside {outside:F3}\n{log}");
            Assert.AreEqual(0,outside,$"{key}: a particle left the box or went NaN (see wander-{key}.txt)");
        }
        [UnityTest,Timeout(1800000)] public IEnumerator SlimeWanderS1(){yield return Wander("S1",11,40);}
        [UnityTest,Timeout(1800000)] public IEnumerator SlimeWanderS2(){yield return Wander("S2",22,40);}
        [UnityTest,Timeout(1800000)] public IEnumerator SlimeWanderS3(){yield return Wander("S3",33,40);}

        // ---- portrait layouts --------------------------------------------------------------------------------------------
        private IEnumerator LayoutCheck(string key,int w,int h)
        {
            yield return Load(key,w,h);var l=COgheSlimeControl.Layout(new Rect(0,0,w,h));
            var min=slime.RoomMin;var max=slime.RoomMax;var r=Rect.MinMaxRect(float.MaxValue,float.MaxValue,float.MinValue,float.MinValue);
            for(int i=0;i<8;i++){var c=new Vector3((i&1)==0?min.x:max.x,(i&2)==0?min.y:max.y,(i&4)==0?min.z:max.z);var p=S(game.Root.TransformPoint(c));r=Rect.MinMaxRect(Mathf.Min(r.xMin,p.x),Mathf.Min(r.yMin,p.y),Mathf.Max(r.xMax,p.x),Mathf.Max(r.yMax,p.y));}
            string info=$"{key} {w}x{h}: room {r} play {l.Play} retry {l.Retry} pause {l.Pause} title {l.Title} goal {l.Goal}";
            File.AppendAllText($"{Out}/layout.txt",info+"\n");
            Assert.IsTrue(l.Play.Contains(r.min)&&l.Play.Contains(r.max),"the room lies in the pull area: "+info);
            foreach(var ui in new[]{l.Retry,l.Pause,l.Title,l.Goal})Assert.IsFalse(ui.Overlaps(r),"UI does not cover the room: "+info);
            Assert.IsFalse(l.Retry.Overlaps(l.Pause),"buttons apart");
            Assert.IsTrue(l.Play.Contains(S(slime.Centre()))&&l.Play.Contains(S(game.Owner.Outlet.position)),"slime and door inside the pull area: "+info);
            Assert.Greater(r.width,w*.75f,"the room uses the screen width: "+info);
            Directory.CreateDirectory(Out+"/layout");Frame($"{Out}/layout/{key}-{w}x{h}.png");
        }
        [UnityTest,Timeout(900000)] public IEnumerator SlimeLayout720x1280(){foreach(var k in new[]{"S1","S2","S3"})yield return LayoutCheck(k,720,1280);}
        [UnityTest,Timeout(900000)] public IEnumerator SlimeLayout720x1612(){foreach(var k in new[]{"S1","S2","S3"})yield return LayoutCheck(k,720,1612);}

        // ---- real Input System events in screen pixels ------------------------------------------------------------------
        [UnityTest,Timeout(900000)] public IEnumerator SlimeRealPointerEvents()
        {
            yield return Load("S1");yield return Settle();var mouse=InputSystem.AddDevice<Mouse>();var touch=InputSystem.AddDevice<Touchscreen>();
            try
            {
                Vector2 a=S(W(0,.10f)),b=S(W(0,.10f)+PullFor(60,.7f));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=a});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=a,buttons=1});yield return null;Tick();Tick();
                Assert.IsTrue(slime.Pulling,"a real mouse press in the play area starts a pull");
                for(int i=1;i<=8;i++){InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(a,b,i/8f),buttons=1});yield return null;Tick();Tick();}
                Assert.AreEqual(.7f,slime.Power,.03f,"the pixel drag maps to the same power");
                InputSystem.QueueStateEvent(mouse,new MouseState{position=b});yield return null;Tick();Tick();
                Assert.AreEqual(1,slime.Launches,"a real mouse release launches");
                var v=slime.History[0].Velocity;Assert.AreEqual(60,Mathf.Atan2(v.y,v.x)*Mathf.Rad2Deg,1.5f,"direction opposite to the drag");
                yield return Ticks(1);yield return Settle();
                // a real click on THỬ LẠI resets, never launches
                var l=COgheSlimeControl.Layout(new Rect(0,0,width,height));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=l.Retry.center});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=l.Retry.center,buttons=1});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=l.Retry.center});yield return null;yield return Ticks(.6f);
                Assert.AreEqual(0,slime.Launches,"THỬ LẠI reset the level (and did not launch)");Assert.IsFalse(slime.Pulling);
                // touch: began, moved, ended
                yield return Settle();Vector2 c=S(W(0,.10f)),d=S(W(0,.10f)+PullFor(90,.5f));
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=c});yield return null;Tick();Tick();
                for(int i=1;i<=6;i++){InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=Vector2.Lerp(c,d,i/6f)});yield return null;Tick();Tick();}
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=d});yield return null;Tick();Tick();yield return null;Tick();Tick();
                Assert.AreEqual(1,slime.Launches,"a real touch drag launches");
                File.WriteAllText($"{Out}/real-input.txt",$"mouse: {a}->{b} launch {v:F3}; touch: {c}->{d} launch {slime.History[0].Velocity:F3}\n");
            }
            finally{InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(touch);}
        }
    }
}

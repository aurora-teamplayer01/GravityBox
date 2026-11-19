using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    /// <summary>
    /// Slime Trial (branch claude/slime-physics-trial, Docs/LevelDesign/COghe/SlimeTrial). Every solution goes through the
    /// player's pointer entry points (VenomCampaign.BeginPointer / MovePointer / EndPointer, the same calls touch and mouse
    /// make) with screen points of a 720×1280 render target; one test feeds real Input System mouse events. No
    /// teleporting, no forces, no win flags. Launch inputs below were found by the headless search (SlimeSearch*), not by hand.
    /// </summary>
    public sealed partial class COgheSlimeTrialTests
    {
        private VenomCampaign game;private COgheSlimeControl slime;
        private SimulationMode simulation;private bool persistence;
        private InputSettings.BackgroundBehavior background;private InputSettings.EditorInputBehaviorInPlayMode editorInput;
        private RenderTexture target;private int width=720,height=1280;
        private const float Dt=1f/120;
        private int ticksPerFrame=2;
        private Action perTick;
        public const string Out="Artifacts/SlimeTrial";

        [UnitySetUp] public IEnumerator Before()
        {
            simulation=Physics.simulationMode;persistence=VenomCampaignSave.PersistenceEnabled;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;perTick=null;ticksPerFrame=2;width=720;height=1280;
            Directory.CreateDirectory(Out);yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            Time.timeScale=1;Time.captureFramerate=0;Physics.simulationMode=simulation;VenomCampaignSave.PersistenceEnabled=persistence;
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;COgheProductUI.LayoutAreaForTests=null;
            if(game!=null&&game.Owner!=null&&game.Owner.View!=null)game.Owner.View.targetTexture=null;
            if(target!=null){target.Release();Object.Destroy(target);target=null;}
            yield return null;
        }

        // ---- harness ----------------------------------------------------------------------------------------------
        private IEnumerator Load(string key,int w=720,int h=1280)
        {
            width=w;height=h;
            void Loaded(Scene s,LoadSceneMode m){game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync("COgheSlime"+key);}finally{SceneManager.sceneLoaded-=Loaded;}
            Layout(w,h);slime=game.Slime;Assert.IsNotNull(slime,"Slime Trial levels carry COgheSlimeControl");
            COgheSlimeControl.HintDone=false;
            Assert.IsNull(game.Personality,"No personality in the trial");Assert.IsNull(game.Intelligence,"No Intelligence in the trial");Assert.IsNull(game.ProductUI,"No product UI in the trial");
            yield return Ticks(.6f);
        }
        private void Layout(int w,int h)
        {
            width=w;height=h;COgheProductUI.LayoutAreaForTests=new Rect(0,0,w,h);
            if(target!=null){game.Owner.View.targetTexture=null;target.Release();Object.Destroy(target);}
            target=new RenderTexture(w,h,24);game.Owner.View.targetTexture=target;game.CameraRig.Frame(w,h,0,true,new Rect(0,0,w,h));
        }
        private void Tick(){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);perTick?.Invoke();}
        private IEnumerator Ticks(float seconds,Func<bool> stop=null)
        {
            int n=Mathf.RoundToInt(seconds/Dt);
            for(int i=0;i<n;i++){if(stop!=null&&stop())yield break;Tick();if(i%ticksPerFrame==ticksPerFrame-1)yield return null;}
        }
        private Vector3 W(float x,float y,float z=0)=>game.Root.TransformPoint(new Vector3(x,y,z));
        private Vector3 Local(Vector3 world)=>game.Root.InverseTransformPoint(world);
        private Vector3 Body=>Local(slime.Centre());
        private Vector2 S(Vector3 world){game.CameraRig.Frame(width,height,0,true,new Rect(0,0,width,height));var p=game.Owner.View.WorldToScreenPoint(world);return new Vector2(p.x,p.y);}
        private float Speed(){float v=0;foreach(var b in game.Matter.Bodies)v+=b.linearVelocity.magnitude;return v/32;}
        /// <summary>Waits until the body is anchored and still (or the level ended).</summary>
        private IEnumerator Settle(float limit=5)
        {
            float still=0;
            for(float t=0;t<limit&&!game.Owner.Completed&&!game.Owner.Lost;t+=Dt*ticksPerFrame)
            {
                for(int k=0;k<ticksPerFrame;k++)Tick();
                still=slime.Anchored&&Speed()<.03f?still+Dt*ticksPerFrame:0;if(still>=.25f)yield break;
                yield return null;
            }
        }
        /// <summary>A launch through the pointer path: press at <paramref name="press"/> (box-local; default the room's
        /// middle, so the drag stays clear of the HUD), drag over <paramref name="frames"/> frames by the pull that gives
        /// direction <paramref name="angle"/> (degrees, 0 = right, 90 = up) and <paramref name="power"/> (0…1), release.</summary>
        public static Vector3 PullFor(float angle,float power)
        {
            float a=angle*Mathf.Deg2Rad;float length=COgheSlimeControl.DeadZone+.0005f+Mathf.Clamp01(power)*(COgheSlimeControl.MaxPull-COgheSlimeControl.DeadZone-.0005f);
            return -new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*length;
        }
        /// <summary>The drag lasts <paramref name="holdTicks"/> simulation ticks (16 = 0.13 s) whatever the frame rate: one
        /// move per rendered frame, so a 60 fps run makes twice as many moves as a 30 fps run over the same drag.</summary>
        private IEnumerator Pull(float angle,float power,int holdTicks=16,Vector3? press=null)
        {
            Vector3 a=W(press?.x??0,press?.y??.10f),b=a+game.Root.TransformDirection(PullFor(angle,power));
            game.BeginPointer(S(a));int frames=Mathf.Max(1,holdTicks/ticksPerFrame);
            for(int k=1;k<=frames;k++){for(int t=0;t<holdTicks/frames;t++)Tick();game.MovePointer(S(Vector3.Lerp(a,b,k/(float)frames)));yield return null;}
            game.EndPointer(S(b));
        }
        private IEnumerator Launch(float angle,float power,float flight=4)
        {
            yield return Settle();if(game.Owner.Completed||game.Owner.Lost)yield break;
            int before=slime.Launches;yield return Pull(angle,power);
            for(int k=0;k<2;k++)Tick();
            Assert.AreEqual(before+1,slime.Launches,$"launch {angle}°/{power:P0} was not taken (anchored={slime.Anchored}, cancel={slime.LastCancel})");
            yield return Ticks(.3f);yield return Settle(flight);
        }
        private string State()
        {
            var sb=new StringBuilder($"body={Body:F3} anchored={slime.Anchored} grips={slime.GripCount()} won={game.Owner.Completed} lost={game.Owner.Lost} failure={game.Failure} escaped={game.Matter.EscapedCount} launches={slime.Launches}");
            foreach(var r in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())sb.Append($" crate={r.Position:F3}/{r.Travel:F3}");
            foreach(var f in game.Owner.Apparatus.GetComponentsInChildren<COgheSlimeFan>())sb.Append($" fan={(f.Running?"on":"off")} carried={f.Carried:F2}s");
            return sb.ToString();
        }
        private void Frame(string path)
        {
            slime.PlaceHud(new Rect(0,0,width,height),width,height);slime.Present(game.Matter.SimulationTime);
            var cam=game.Owner.View;var previous=RenderTexture.active;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{cam.targetTexture=target;cam.Render();RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
            finally{RenderTexture.active=previous;Object.Destroy(tex);}
        }

        /// <summary>Frames (every 4th tick = 30 fps of simulated time) and a trace for a list of launches.</summary>
        private IEnumerator Probe(string key,string name,params Vector2[] shots)
        {
            yield return Load(key);string dir=$"{Out}/probe-{name}";if(Directory.Exists(dir))Directory.Delete(dir,true);Directory.CreateDirectory(dir);
            var trace=new StringBuilder();int tick=0,shot=0;
            perTick=()=>{tick++;if(tick%4==0){Frame($"{dir}/f{shot++:000}.png");if(tick%12==0)trace.AppendLine($"{tick*Dt:F2} {State()}");}};
            foreach(var v in shots){yield return Settle();yield return Pull(v.x,v.y);yield return Ticks(1.2f,()=>game.Owner.Completed||game.Owner.Lost);}
            yield return Settle(3);perTick=null;File.WriteAllText(dir+"/trace.txt",trace+"\n"+State());
        }
        [UnityTest,Explicit,Timeout(900000)] public IEnumerator SlimeProbeS3A(){yield return Probe("S3","S3A",new Vector2(75,.9f),new Vector2(0,.5f));}
        [UnityTest,Explicit,Timeout(900000)] public IEnumerator SlimeProbeS3B(){yield return Probe("S3","S3B",new Vector2(185,.3f),new Vector2(0,.6f),new Vector2(0,.5f));}
        [UnityTest,Explicit,Timeout(900000)] public IEnumerator SlimeProbeS2(){yield return Probe("S2","S2",new Vector2(-30,.15f),new Vector2(0,.6f),new Vector2(65,.9f));}

        // ---- probe: one launch in S1, frames + trace (feel check) -----------------------------------------------------
        [UnityTest,Explicit,Timeout(900000)] public IEnumerator SlimeProbeS1()
        {
            yield return Load("S1");string dir=Out+"/probe";Directory.CreateDirectory(dir);
            var trace=new StringBuilder();int tick=0,shot=0;
            perTick=()=>{tick++;if(tick%4==0){Frame($"{dir}/f{shot++:000}.png");trace.AppendLine($"{tick*Dt:F3} {Body:F3} grips={slime.GripCount()} pulling={slime.Pulling} power={slime.Power:F2}");}};
            yield return Ticks(.3f);
            yield return Pull(55,.85f,40);
            yield return Ticks(1.6f);
            yield return Pull(100,.6f,40);
            yield return Ticks(1.6f);
            perTick=null;File.WriteAllText(dir+"/trace.txt",trace.ToString()+"\n"+State());
        }
    }
}

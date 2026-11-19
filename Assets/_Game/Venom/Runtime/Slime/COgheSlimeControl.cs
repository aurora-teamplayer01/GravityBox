using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Touch=UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GravityBox.Venom
{
    /// <summary>
    /// Slime Trial (branch claude/slime-physics-trial, 19/11/2026; Docs/LevelDesign/COghe/SlimeTrial). One verb replaces the
    /// campaign's tap-to-assign: press anywhere in the play area, drag back, release, and the body is launched the opposite
    /// way (a slingshot). Same force law and feedback in every room. A launch needs the body anchored (at least two gripping
    /// particles, <see cref="VenomCampaignMotion.HasGrip"/>); the existing anchoring then sticks it wherever it lands on bare
    /// glass or floor. Input only records the pull; the launch itself is applied in the fixed simulation step, so its speed
    /// never depends on the frame rate. No DNA, no unlocks, no saves: the level's definition uses <see cref="ProgressKey"/>,
    /// which keeps Personality, Intelligence and the product UI off (their Applies switches) and VenomCampaign skips the win
    /// save. Presentation (arc, band, power, HUD) is world-space geometry under the camera so a camera capture shows it.
    /// </summary>
    [DefaultExecutionOrder(-105)]
    public sealed class COgheSlimeControl : MonoBehaviour
    {
        public const string ProgressKey="coghe.slime.trial";
        public static bool Applies(VenomCampaign game)=>game!=null&&game.Definition!=null&&game.Definition.ProgressKey==ProgressKey;

        // ---- the force law (identical in every room) -------------------------------------------------------------
        /// <summary>Pull shorter than this (metres in the launch plane) cancels: nothing happens on release.</summary>
        public const float DeadZone=.025f;
        /// <summary>Pull at which power reaches 100%; a longer pull is clamped.</summary>
        public const float MaxPull=.20f;
        /// <summary>Launch speed at the smallest and the largest power (m/s).</summary>
        public const float MinSpeed=1.0f,MaxSpeed=3.4f;
        /// <summary>The surfaces the body launched from do not grip it again for this long (it must leave them first).</summary>
        public const float DetachSeconds=.22f;
        /// <summary>After touching down, a short extra damper makes the landing squash and settle instead of skidding.</summary>
        public const float LandingSeconds=.20f,LandingBrake=14f,LandingBrakeLimit=24f;
        /// <summary>The visible charge: the side of the body facing the finger is drawn toward it (zero net force).</summary>
        public const float ChargeAccel=10f;
        /// <summary>Airborne, a mild spring keeps the body in the lane plane the room's objects live in.</summary>
        public const float LaneSpring=30f,LaneDamping=6f;
        public static float PowerFor(float pull)=>Mathf.Clamp01((pull-DeadZone)/(MaxPull-DeadZone));
        public static float SpeedFor(float power)=>Mathf.Lerp(MinSpeed,MaxSpeed,Mathf.Clamp01(power));

        // ---- authoring (written by the builder) ------------------------------------------------------------------
        public Vector3 RoomMin=new Vector3(-.30f,-.30f,-.10f),RoomMax=new Vector3(.30f,.50f,.10f);
        public float Lane;
        public string Goal="";
        public bool Hint;
        public Material Ink;
        public Font Font;

        // ---- state ---------------------------------------------------------------------------------------------
        public sealed class LaunchRecord
        {
            public float Time,Held,Power,Speed;public Vector2 ScreenStart,ScreenEnd;public Vector3 Velocity,From;
            public override string ToString()=>$"t={Time:F2}s held={Held:F2}s screen {ScreenStart:F0}->{ScreenEnd:F0} power={Power:P0} speed={Speed:F2} m/s v={Velocity:F2} from={From:F3}";
        }
        public readonly List<LaunchRecord> History=new List<LaunchRecord>();
        public event Action<LaunchRecord> Launched;
        public bool Pulling {get;private set;}
        public float Power {get;private set;}
        public Vector3 PullVector {get;private set;}
        public Vector3 PreviewVelocity {get;private set;}
        public int Launches {get;private set;}
        public int Refusals {get;private set;}
        public int Cancels {get;private set;}
        public int Landings {get;private set;}
        public bool Flying {get;private set;}
        public string LastCancel {get;private set;}
        /// <summary>The ghost-finger hint: once per app session, off after the first launch (Docs: same text for everyone).</summary>
        public static bool HintDone;
        public bool HintVisible=>Hint&&!HintDone&&game!=null&&!game.Owner.Completed&&!game.Owner.Lost;
        public string Toast {get;private set;}
        private float toastUntil;
        private VenomCampaign game;
        private Vector3 pullStart,pullNow;private Vector2 screenStart,screenNow;private float pullBeganAt;
        private Vector3? pending;private LaunchRecord pendingRecord;
        private float launchedAt=-10,landingUntil=-10;
        private readonly List<int> group=new List<int>(32);

        public void Initialize(VenomCampaign owner){game=owner;BuildPresentation();ResetState();}
        public void ResetState()
        {
            Pulling=false;pending=null;pendingRecord=null;Flying=false;launchedAt=landingUntil=-10;restingGripless=0;System.Array.Clear(viaVent,0,viaVent.Length);ventTracked=false;Power=0;PullVector=PreviewVelocity=Vector3.zero;
            Toast=null;toastUntil=0;History.Clear();Launches=Refusals=Cancels=Landings=0;LastCancel=null;
        }

        // ---- the one input API (VenomCampaign's pointer path, the HUD and the tests all come through here) --------
        public bool AllowsPointer(Vector2 screen)
        {
            var layout=Layout(Area());
            return layout.Play.Contains(screen)&&!layout.Retry.Contains(screen)&&!layout.Pause.Contains(screen);
        }
        public bool BeginPull(Vector2 screen)
        {
            if(game==null||!game.Owner.CanControl||!AllowsPointer(screen)||!ToLane(screen,out var world))return false;
            Pulling=true;pullStart=pullNow=world;screenStart=screenNow=screen;pullBeganAt=game.Matter.SimulationTime;Refresh();return true;
        }
        public void MovePull(Vector2 screen)
        {
            if(!Pulling)return;
            if(!game.Owner.CanControl){CancelPull("paused");return;}
            if(ToLane(screen,out var world)){pullNow=world;screenNow=screen;}
            Refresh();
        }
        /// <summary>Release: queues a launch for the next simulation step. False when it was cancelled or refused.</summary>
        public bool EndPull(Vector2 screen)
        {
            if(!Pulling)return false;
            if(!AllowsPointer(screen)){CancelPull("released outside the play area","Thả ngoài vùng chơi: huỷ");return false;}
            MovePull(screen);if(!Pulling)return false;
            Pulling=false;
            if(PullVector.magnitude<DeadZone){Cancels++;LastCancel="pull below the dead zone";Show("Kéo xa hơn một chút để bật");return false;}
            if(!Anchored){Refusals++;Wobble();Show("Đang bay: chạm vào bề mặt rồi mới bật được");return false;}
            pending=PreviewVelocity;
            pendingRecord=new LaunchRecord{Time=game.Matter.SimulationTime,Held=game.Matter.SimulationTime-pullBeganAt,Power=Power,Speed=PreviewVelocity.magnitude,
                ScreenStart=screenStart,ScreenEnd=screen,Velocity=PreviewVelocity,From=game.Root.InverseTransformPoint(Centre())};
            return true;
        }
        public void CancelPull(string reason,string message=null)
        {
            // a release already queued for the next physics tick is cancelled too (Codex review P2-1: release, then
            // pause/focus loss before the tick, must not launch on resume)
            if(pending.HasValue){pending=null;pendingRecord=null;Cancels++;LastCancel=reason;}
            if(!Pulling)return;
            Pulling=false;Power=0;PullVector=PreviewVelocity=Vector3.zero;Cancels++;LastCancel=reason;if(message!=null)Show(message);
        }
        private void Refresh()
        {
            Vector3 pull=Vector3.ProjectOnPlane(pullStart-pullNow,game.Root.forward);
            PullVector=Vector3.ClampMagnitude(pull,MaxPull);Power=PowerFor(pull.magnitude);
            PreviewVelocity=pull.magnitude>=DeadZone?pull.normalized*SpeedFor(Power):Vector3.zero;
        }
        private bool ToLane(Vector2 screen,out Vector3 world)
        {
            world=default;var view=game.Owner.View;if(view==null)return false;
            var ray=view.ScreenPointToRay(screen);var plane=new Plane(game.Root.forward,game.Root.TransformPoint(new Vector3(0,0,Lane)));
            if(!plane.Raycast(ray,out float d))return false;world=ray.GetPoint(d);return true;
        }

        // ---- simulation (called by VenomCampaign.Step after Motion.Step, fixed dt) -------------------------------
        public int Anchor=>game.Motion.Selected;
        public int GripCount(){CollectGroup();int n=0;foreach(int i in group)if(game.Motion.HasGrip(i))n++;return n;}
        /// <summary>May launch: at least two gripping particles, or (anti-softlock) the body has rested still on at least
        /// two contacts for <see cref="RestingLaunchSeconds"/> without grip (wedged against a slick face).</summary>
        public bool Anchored=>game!=null&&(GripCount()>=2||restingGripless>=RestingLaunchSeconds);
        public const float RestingLaunchSeconds=1f;
        private float restingGripless;
        public Vector3 Centre()=>game.Motion.Centre(Anchor);
        private void CollectGroup()
        {
            group.Clear();int g=game.Matter.Groups[Anchor];
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)if(game.Matter.Groups[i]==g&&!game.Matter.Escaped[i])group.Add(i);
        }
        public void Step(float dt)
        {
            if(game.Owner.Completed||game.Owner.Lost)return;
            float now=game.Matter.SimulationTime;TrackVent();CollectGroup();if(group.Count==0)return;
            int grips=0;Vector3 c=Vector3.zero,v=Vector3.zero;
            foreach(int i in group){if(game.Motion.HasGrip(i))grips++;c+=game.Matter.Bodies[i].position;v+=game.Matter.Bodies[i].linearVelocity;}
            c/=group.Count;v/=group.Count;
            int contacts=0;foreach(int i in group)if(game.Motion.Support(i,out _,out _,out _))contacts++;
            restingGripless=grips<2&&contacts>=2&&v.magnitude<.05f?restingGripless+dt:0;
            if(restingGripless>=RestingLaunchSeconds)grips=Mathf.Max(grips,2);
            if(float.IsNaN(c.x)||float.IsNaN(c.y)||float.IsNaN(c.z)){Debug.LogError("SLIME NaN body centre");game.Fail("Lỗi vật lý (NaN). Thử lại.");return;}
            if(pending.HasValue)
            {
                Vector3 launch=pending.Value;var record=pendingRecord;pending=null;pendingRecord=null;
                if(grips<2){Refusals++;Wobble();Show("Đang bay: chạm vào bề mặt rồi mới bật được");}
                else
                {
                    game.Motion.Cancel(Anchor);game.Motion.Detach(Anchor,DetachSeconds);
                    float z=game.Root.InverseTransformPoint(c).z-Lane;
                    launch+=game.Root.forward*Mathf.Clamp(-z*4,-.3f,.3f);
                    foreach(int i in group){var b=game.Matter.Bodies[i];b.linearVelocity=launch+(b.linearVelocity-v)*.3f;}
                    Flying=true;launchedAt=now;Launches++;HintDone=true;grips=0;
                    if(record!=null){History.Add(record);Debug.Log($"SLIME LAUNCH {game.Definition.Id} #{Launches} {record}");Launched?.Invoke(record);}
                }
            }
            // the visible charge: the finger side of the body is drawn toward the finger (no net force: it stays put)
            if(Pulling&&Power>0&&grips>=2)
            {
                Vector3 dir=PreviewVelocity.normalized;Vector3 sum=Vector3.zero;var accel=new Vector3[group.Count];
                for(int k=0;k<group.Count;k++)
                {
                    Vector3 o=game.Matter.Bodies[group[k]].position-c;float s=Mathf.Clamp01(Vector3.Dot(o,-dir)/.035f+.15f);
                    accel[k]=-dir*(Power*ChargeAccel*s);sum+=accel[k];
                }
                sum/=group.Count;
                for(int k=0;k<group.Count;k++){var b=game.Matter.Bodies[group[k]];b.AddForce((accel[k]-sum)*b.mass);}
            }
            if(Flying&&grips>=2&&now>launchedAt+DetachSeconds*.5f){Flying=false;landingUntil=now+LandingSeconds;Landings++;}
            if(now<landingUntil&&grips>=2)
            {
                var frame=game.Owner.Rotation.GetComponent<Rigidbody>();
                foreach(int i in group)
                {
                    var b=game.Matter.Bodies[i];Vector3 rel=b.linearVelocity-frame.GetPointVelocity(b.position);
                    b.AddForce(Vector3.ClampMagnitude(-rel*LandingBrake,LandingBrakeLimit)*b.mass);
                }
            }
            if(grips<2)
            {
                // airborne: keep to the lane plane (z) without touching the in-plane flight
                Vector3 local=game.Root.InverseTransformPoint(c);float vz=Vector3.Dot(v,game.Root.forward);
                Vector3 a=game.Root.forward*(-(local.z-Lane)*LaneSpring-vz*LaneDamping);
                foreach(int i in group)game.Matter.Bodies[i].AddForce(a,ForceMode.Acceleration);
            }
            // leaving the box (tunnelling through a pane included) is a failure with Retry, never a silent softlock
            foreach(int i in group)
            {
                Vector3 p=game.Root.InverseTransformPoint(game.Matter.Bodies[i].position);
                bool outside=p.x<RoomMin.x-.03f||p.y<RoomMin.y-.03f||p.z<RoomMin.z-.03f||p.x>RoomMax.x+.03f||p.y>RoomMax.y+.03f||p.z>RoomMax.z+.03f;
                // tissue leaving through the vent: a fast entry can carry a particle through the bore before the exit
                // logic records its escape (it then fails the level as "out of the box"; found by the S2 search)
                if(outside&&ExitingThroughVent(i))continue;
                if(outside)
                {Debug.LogWarning($"SLIME OUT OF BOX particle {i} at {p:F3}");game.Fail("Slime văng ra ngoài hộp. Thử lại.");return;}
            }
        }
        /// <summary>Outside the outlet's pane, on its outward side and within 25 cm of the vent: leaving by the exit.</summary>
        /// <summary>The particle crossed the vent's plane inside its aperture (Codex review P2-2: not merely "near the
        /// vent"). A particle through the glass beside the hole is not exempt and fails the level.</summary>
        public bool ExitingThroughVent(int particle)=>viaVent[particle]&&game.Owner.Outlet.InverseTransformPoint(game.Matter.Bodies[particle].position).z<.30f;
        private readonly bool[] viaVent=new bool[CohesiveOrganism.ParticleCount];
        private readonly Vector3[] ventLocal=new Vector3[CohesiveOrganism.ParticleCount];private bool ventTracked;
        private void TrackVent()
        {
            var outlet=game.Owner.Outlet;float limit=game.Owner.ApertureRadius+game.Matter.Profile.ParticleRadius;
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
            {
                Vector3 p=outlet.InverseTransformPoint(game.Matter.Bodies[i].position),old=ventLocal[i];ventLocal[i]=p;
                if(!ventTracked)continue;
                if(old.z<0&&p.z>=0){var cross=Vector3.Lerp(old,p,-old.z/(p.z-old.z));viaVent[i]=new Vector2(cross.x,cross.y).magnitude<limit;}
                else if(p.z<-.02f)viaVent[i]=false;
            }
            ventTracked=true;
        }
        private void Wobble()
        {
            // a refused release: a visible shiver of the airborne body (zero net momentum, no change of course)
            CollectGroup();Vector3 c=Centre();
            foreach(int i in group){var b=game.Matter.Bodies[i];Vector3 o=b.position-c;b.AddForce(new Vector3(-o.y,o.x,0)*6f,ForceMode.VelocityChange);}
        }
        private void Show(string text){Toast=text;toastUntil=Time.unscaledTime+1.6f;}

        // ---- layout (pixels, bottom-left origin) -----------------------------------------------------------------
        public struct HudLayout{public Rect Play,Frame,Retry,Pause,Title,Goal,Message;public float Unit;}
        /// <summary>Pixels kept free above the HUD (clip captions; 0 in play).</summary>
        public static float ReservedTop;
        public static HudLayout Layout(Rect area)
        {
            if(ReservedTop>0)area=new Rect(area.x,area.y,area.width,area.height-ReservedTop);
            float u=Mathf.Min(area.width/360f,area.height/640f);var l=new HudLayout{Unit=u};
            l.Retry=new Rect(area.xMin+14*u,area.yMin+14*u,132*u,44*u);
            l.Pause=new Rect(area.xMax-14*u-132*u,area.yMin+14*u,132*u,44*u);
            l.Title=new Rect(area.xMin+10*u,area.yMax-40*u,area.width-20*u,30*u);
            l.Goal=new Rect(area.xMin+10*u,area.yMax-66*u,area.width-20*u,22*u);
            l.Play=Rect.MinMaxRect(area.xMin,area.yMin+66*u,area.xMax,area.yMax-72*u);
            l.Frame=Rect.MinMaxRect(area.xMin+10*u,area.yMin+74*u,area.xMax-10*u,area.yMax-80*u);
            l.Message=new Rect(area.xMin+20*u,area.center.y-40*u,area.width-40*u,80*u);
            return l;
        }
        private static Rect Area()
        {
            var a=COgheProductUI.LayoutAreaForTests??Screen.safeArea;
            if(a.width<1||a.height<1)a=new Rect(0,0,Screen.width,Screen.height);return a;
        }
        /// <summary>The camera frames the room in this rect (VenomCampaignCamera.UsableRect).</summary>
        public Rect FramingRect(Rect safeArea)=>Layout(safeArea).Frame;
        /// <summary>A press on a HUD button (any state: paused and failed included). True when a button took it.</summary>
        public bool PressHud(Vector2 screen)
        {
            if(game==null)return false;var l=Layout(Area());
            if(l.Retry.Contains(screen)){CancelPull("retry");game.ResetLevel();return true;}
            if(l.Pause.Contains(screen)&&!game.Owner.Completed&&!game.Owner.Lost){CancelPull("pause");game.Owner.TogglePause();return true;}
            return false;
        }
        private void Update()
        {
            if(game==null)return;
            if(Touch.activeTouches.Count>0){foreach(var t in Touch.activeTouches)if(t.phase==UnityEngine.InputSystem.TouchPhase.Began)PressHud(t.screenPosition);}
            else if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)PressHud(Mouse.current.position.ReadValue());
        }

        // ---- presentation ------------------------------------------------------------------------------------------
        private const int Dots=18;
        private LineRenderer band,arrow,head,ring,finger;
        private Transform[] dots;private Transform landing;
        private TextMesh powerText,title,goal,message,retryText,pauseText,hintText;
        private Transform retryBox,pauseBox,messageBox,hud;private float pixelWorld=.001f,hudUnit=2;
        private static readonly Color Ready=new Color(.04f,.60f,.40f),Blocked=new Color(.86f,.22f,.16f),Faint=new Color(.08f,.30f,.36f,.9f);
        private LineRenderer Line(string name,Transform parent,float width,int points)
        {
            var go=new GameObject(name,typeof(LineRenderer));go.transform.SetParent(parent,false);var l=go.GetComponent<LineRenderer>();
            l.sharedMaterial=Ink;l.useWorldSpace=true;l.startWidth=l.endWidth=width;l.positionCount=points;l.numCapVertices=4;l.enabled=false;
            l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;l.receiveShadows=false;l.sortingOrder=50;return l;
        }
        private TextMesh Text(string name,Transform parent,TextAnchor anchor,Color color)
        {
            var go=new GameObject(name,typeof(TextMesh));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMesh>();
            t.font=Font;t.fontSize=64;t.anchor=anchor;t.alignment=TextAlignment.Center;t.color=color;t.richText=false;
            var r=go.GetComponent<MeshRenderer>();if(Font!=null)r.sharedMaterial=Font.material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.sortingOrder=60;
            return t;
        }
        private Transform Box(string name,Transform parent,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=name;DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);
            var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=Ink;var block=new MaterialPropertyBlock();block.SetColor("_Color",color);r.SetPropertyBlock(block);
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.sortingOrder=55;return go.transform;
        }
        private void BuildPresentation()
        {
            if(Ink==null)Ink=new Material(Shader.Find("Sprites/Default"));
            if(Font==null)Font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var world=new GameObject("Slime pull indicator").transform;world.SetParent(transform,false);
            band=Line("Pull band",world,.006f,2);arrow=Line("Launch arrow",world,.010f,2);head=Line("Launch arrow head",world,.010f,3);
            ring=Line("Finger ring",world,.004f,25);finger=Line("Ghost finger",world,.012f,25);
            dots=new Transform[Dots];
            for(int i=0;i<Dots;i++){var d=Box("Arc dot",world,Faint);d.localScale=Vector3.one*.009f;d.gameObject.SetActive(false);dots[i]=d;}
            landing=Box("Arc end",world,Faint);landing.localScale=Vector3.one*.022f;landing.gameObject.SetActive(false);
            powerText=Text("Power",world,TextAnchor.LowerCenter,Ready);powerText.gameObject.SetActive(false);
            hud=new GameObject("Slime HUD (camera space)").transform;hud.SetParent(game.Owner.View.transform,false);
            title=Text("Title",hud,TextAnchor.MiddleCenter,new Color(.07f,.22f,.24f));goal=Text("Goal",hud,TextAnchor.MiddleCenter,new Color(.14f,.30f,.32f));
            messageBox=Box("Message box",hud,new Color(.06f,.10f,.12f,.80f));
            message=Text("Message",hud,TextAnchor.MiddleCenter,Color.white);hintText=Text("Hint",hud,TextAnchor.MiddleCenter,new Color(.62f,.36f,.02f));
            retryBox=Box("Retry button",hud,new Color(.10f,.16f,.18f,.82f));pauseBox=Box("Pause button",hud,new Color(.10f,.16f,.18f,.82f));
            retryText=Text("Retry label",hud,TextAnchor.MiddleCenter,Color.white);pauseText=Text("Pause label",hud,TextAnchor.MiddleCenter,Color.white);
        }
        private void LateUpdate(){if(game!=null){var a=Area();PlaceHud(a,Screen.width,Screen.height);Present(Time.unscaledTime);}}
        /// <summary>Lays the camera-space HUD out for a <paramref name="width"/>×<paramref name="height"/> target (the clip
        /// recorder calls this before rendering to its own texture).</summary>
        public void PlaceHud(Rect area,int width,int height)
        {
            var cam=game.Owner.View;if(cam==null||hud==null)return;
            var l=Layout(area);float pixel=2*cam.orthographicSize/Mathf.Max(1,height);pixelWorld=pixel;hudUnit=l.Unit;float depth=cam.nearClipPlane+.02f;
            Vector3 At(Vector2 p)=>cam.transform.InverseTransformPoint(cam.ViewportToWorldPoint(new Vector3(p.x/width,p.y/height,depth)));
            void Label(TextMesh t,Rect r,float px,string text,bool show=true)
            {t.text=text;t.gameObject.SetActive(show&&!string.IsNullOrEmpty(text));t.transform.localPosition=At(r.center);t.transform.localRotation=Quaternion.identity;t.characterSize=px*pixel*10f/t.fontSize/.75f;}
            void Button(Transform box,TextMesh t,Rect r,string text){box.localPosition=At(r.center)+Vector3.forward*.001f;box.localRotation=Quaternion.identity;box.localScale=new Vector3(r.width*pixel,r.height*pixel,1);Label(t,r,17*l.Unit,text);}
            Label(title,l.Title,22*l.Unit,game.Definition.Title);
            Label(goal,l.Goal,14*l.Unit,Goal);
            string msg=game.Owner.Lost?game.Failure+"\nBấm THỬ LẠI":game.Owner.Completed?"THOÁT RỒI!":game.Owner.Paused?"TẠM DỪNG":Time.unscaledTime<toastUntil?Toast:null;
            Label(message,l.Message,(game.Owner.Lost||game.Owner.Completed?20:15)*l.Unit,msg);
            messageBox.gameObject.SetActive(message.gameObject.activeSelf);messageBox.localPosition=At(l.Message.center)+Vector3.forward*.001f;messageBox.localRotation=Quaternion.identity;messageBox.localScale=new Vector3(l.Message.width*pixel,l.Message.height*pixel,1);
            message.color=game.Owner.Lost?new Color(1f,.55f,.5f):game.Owner.Completed?Ready:Color.white;
            Label(hintText,new Rect(l.Goal.x,l.Goal.y-24*l.Unit,l.Goal.width,22*l.Unit),13*l.Unit,"Chạm, kéo lùi rồi thả: slime bật ngược lại",HintVisible);
            Button(retryBox,retryText,l.Retry,"THỬ LẠI");Button(pauseBox,pauseText,l.Pause,game.Owner.Paused?"TIẾP":"DỪNG");
            pauseBox.gameObject.SetActive(!game.Owner.Completed&&!game.Owner.Lost);pauseText.gameObject.SetActive(pauseBox.gameObject.activeSelf);
        }
        /// <summary>World-space pull feedback for the current state (also called by the clip recorder per frame).</summary>
        public void Present(float clock)
        {
            if(game==null||band==null)return;
            bool show=Pulling&&game.Owner.CanControl;Vector3 c=Centre();bool anchored=Anchored;
            Color tint=anchored?Ready:Blocked;
            band.enabled=show;ring.enabled=show;
            if(show)
            {
                band.SetPosition(0,c);band.SetPosition(1,pullNow);band.startColor=band.endColor=new Color(tint.r,tint.g,tint.b,.6f);
                Circle(ring,pullNow,.016f,new Color(tint.r,tint.g,tint.b,.8f));
            }
            bool aim=show&&PreviewVelocity.sqrMagnitude>0;
            arrow.enabled=head.enabled=aim;powerText.gameObject.SetActive(show);
            if(show)
            {
                Vector3 aside=PreviewVelocity.sqrMagnitude>0?Vector3.Cross(game.Root.forward,PreviewVelocity.normalized):Vector3.up;if(Vector3.Dot(aside,Vector3.up)<0)aside=-aside;
                Vector3 label=game.Root.InverseTransformPoint(c+aside*.075f);label.x=Mathf.Clamp(label.x,RoomMin.x+.06f,RoomMax.x-.06f);label.y=Mathf.Clamp(label.y,RoomMin.y+.03f,RoomMax.y-.03f);
                powerText.transform.position=game.Root.TransformPoint(label)+Vector3.back*.04f;powerText.transform.rotation=game.Owner.View.transform.rotation;powerText.characterSize=17*hudUnit*pixelWorld*10f/powerText.fontSize/.75f;
                powerText.text=PreviewVelocity.sqrMagnitude>0?(anchored?$"{Power*100:F0}%":"chưa bám"):"kéo xa hơn";powerText.color=tint;
            }
            if(aim)
            {
                Vector3 d=PreviewVelocity.normalized;Vector3 tip=c+d*(.045f+.09f*Power);
                arrow.SetPosition(0,c+d*.03f);arrow.SetPosition(1,tip);arrow.startColor=arrow.endColor=tint;arrow.startWidth=arrow.endWidth=.006f+.008f*Power;
                Vector3 side=Vector3.Cross(game.Root.forward,d);
                head.SetPosition(0,tip-d*.02f+side*.014f);head.SetPosition(1,tip);head.SetPosition(2,tip-d*.02f-side*.014f);head.startColor=head.endColor=tint;
            }
            int shown=0;
            if(aim&&anchored)
            {
                // ballistic preview under real gravity (the fan's wind is not drawn), ending at the first solid hit
                Vector3 p=c,v=PreviewVelocity;const float step=1/60f;float travelled=0,next=.03f;Vector3 end=p;bool hit=false;
                for(int k=0;k<60&&shown<Dots&&!hit;k++)
                {
                    Vector3 q=p+v*step;v+=Vector3.down*9.81f*step;
                    if(SolidBetween(p,q,out var at)){end=at;hit=true;q=at;}
                    travelled+=Vector3.Distance(p,q);p=q;end=p;
                    while(travelled>=next&&shown<Dots){dots[shown].position=p+Vector3.back*.03f;dots[shown].rotation=game.Owner.View.transform.rotation;dots[shown].gameObject.SetActive(true);shown++;next+=.035f;}
                }
                landing.gameObject.SetActive(hit);if(hit){landing.position=end+Vector3.back*.03f;landing.rotation=game.Owner.View.transform.rotation;}
            }
            else landing.gameObject.SetActive(false);
            for(int i=shown;i<Dots;i++)dots[i].gameObject.SetActive(false);
            // the ghost finger: presses beside the body and drags back (down-left), then the body would fly up-right
            bool hint=HintVisible&&!Pulling;finger.enabled=hint;
            if(hint)
            {
                float t=Mathf.Repeat(clock,2.2f);float drag=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.4f)/1.0f));
                Vector3 from=c+new Vector3(.10f,.16f,-.05f),to=from+new Vector3(-.09f,-.09f,0);
                Circle(finger,Vector3.Lerp(from,to,drag),.018f,new Color(.85f,.50f,.02f,t<1.6f?.95f:.3f));
            }
        }
        private readonly RaycastHit[] hits=new RaycastHit[16];
        private bool SolidBetween(Vector3 a,Vector3 b,out Vector3 point)
        {
            point=b;Vector3 d=b-a;float length=d.magnitude;if(length<1e-5f)return false;
            int n=Physics.SphereCastNonAlloc(a,.02f,d/length,hits,length,~0,QueryTriggerInteraction.Ignore);float best=float.PositiveInfinity;
            for(int k=0;k<n;k++){var h=hits[k];if(h.collider.GetComponent<VenomContact>()!=null||h.distance<=0)continue;if(h.distance<best){best=h.distance;point=a+d/length*h.distance;}}
            return best<float.PositiveInfinity;
        }
        private void Circle(LineRenderer l,Vector3 centre,float radius,Color color)
        {
            l.enabled=true;l.positionCount=25;l.startColor=l.endColor=color;var cam=game.Owner.View.transform;
            for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;l.SetPosition(i,centre+(cam.right*Mathf.Cos(a)+cam.up*Mathf.Sin(a))*radius+Vector3.back*.03f);}
        }
    }
}

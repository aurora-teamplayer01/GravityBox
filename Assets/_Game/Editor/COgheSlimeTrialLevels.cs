using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
 /// <summary>
 /// Slime Trial (Mrk 19/11/2026, Buzz thread 0a0d672f…: "Tạo 3 bài mới thử nghiệm với đề xuất của nhà phát hành"). Three
 /// glass rooms played with one verb, pull-and-release (COgheSlimeControl), on branch claude/slime-physics-trial. They stay
 /// OUT of SpatialOrder, saves and progress: own scenes, definitions (ProgressKey coghe.slime.trial) and meshes under
 /// <see cref="SlimeFolder"/>, a scene sequence of their own (S1 → S2 → S3). Design: Docs/LevelDesign/COghe/SlimeTrial.
 /// Every room is a side-on glass box 60 × 80 × 20 cm (portrait), the lane plane z = 0, bare glass (it grips) unless a
 /// visible lavender coating says otherwise. No floor tape, no soft edges (archived-level rules do not apply).
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  public const string SlimeFolder="Assets/_Game/Venom/SlimeTrial";
  static readonly Vector3 SlimeMin=new Vector3(-.30f,-.30f,-.10f),SlimeMax=new Vector3(.30f,.50f,.10f);
  sealed class SlimeLevel{public string Key,Title,Goal;public bool Hint;public Action<ExpansionContext,COgheSlimeControl> Build;}
  static SlimeLevel[] SlimeLevels()=>new[]{
   new SlimeLevel{Key="S1",Title="S1 · Nhà tù kính",Goal="Bật slime tới lỗ thông gió trên cao.",Hint=true,Build=SlimeS1},
   new SlimeLevel{Key="S2",Title="S2 · Hành lang laser",Goal="Laser chặn lối ra. Nút dưới sàn tắt laser.",Build=SlimeS2},
   new SlimeLevel{Key="S3",Title="S3 · Quạt khổng lồ",Goal="Quạt chặn ống dưới vách. Ra lỗ bên phải.",Build=SlimeS3}};
  public static string SlimeScenePath(string key)=>$"{SlimeFolder}/COgheSlime{key}.unity";

  [MenuItem("Gravity Box/COghe/Slime Trial/Generate levels")]
  public static void GenerateSlimeTrial()
  {
   PrepareCampaign30Assets();
   foreach(var d in new[]{SlimeFolder,SlimeFolder+"/Definitions",SlimeFolder+"/Meshes"})Directory.CreateDirectory(d);AssetDatabase.Refresh();
   var levels=SlimeLevels();var names=levels.Select(l=>Path.GetFileNameWithoutExtension(SlimeScenePath(l.Key))).ToArray();
   string old=authoredMeshFolder;
   try{authoredMeshFolder=SlimeFolder+"/Meshes";for(int i=0;i<levels.Length;i++)BuildSlime(levels[i],i+1,names);}
   finally{authoredMeshFolder=old;}
   var scenes=EditorBuildSettings.scenes.ToList();
   foreach(var l in levels){string p=SlimeScenePath(l.Key);if(!scenes.Any(s=>s.path==p))scenes.Add(new EditorBuildSettingsScene(p,true));}
   EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
   Debug.Log("SLIME TRIAL GENERATED "+string.Join(",",levels.Select(l=>l.Key)));
  }

  static Material SlimeMaterial(string name,Color color,bool emissive=false)
  {
   string path=SlimeFolder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
   m.shader=Shader.Find("Universal Render Pipeline/Lit");m.SetFloat("_Surface",0);m.renderQueue=2000;m.color=color;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.4f);
   if(emissive){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*2.2f);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;}
   EditorUtility.SetDirty(m);return m;
  }
  /// <summary>A flat tinted (transparent) look for glows and printed bands on the back glass.</summary>
  static Material SlimeSprite(string name,Color color)
  {
   string path=SlimeFolder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(m,path);}
   m.shader=Shader.Find("Sprites/Default");m.color=color;m.renderQueue=3100;EditorUtility.SetDirty(m);return m;
  }
  /// <summary>A quad printed on the back glass (presentation): x/y box-local rectangle.</summary>
  static Transform BackPrint(Transform parent,string name,Vector2 min,Vector2 max,Material material,float depth=.004f)
  {
   var q=MechanismVisual(parent,name,new Vector3((min.x+max.x)*.5f,(min.y+max.y)*.5f,SlimeMax.z-depth),new Vector3(max.x-min.x,max.y-min.y,1),material,PrimitiveType.Quad);
   if(parent.lossyScale!=Vector3.one){q.position=parent.root.TransformPoint(q.localPosition);}
   q.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return q;
  }
  static Material SlimeInk()
  {
   string path=SlimeFolder+"/Slime ink.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(m,path);}
   m.shader=Shader.Find("Sprites/Default");m.renderQueue=4000;EditorUtility.SetDirty(m);return m;
  }

  static void BuildSlime(SlimeLevel d,int order,string[] names)
  {
   meshSerial=(990+order)*1000;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var owner=new GameObject("COghe Slime Trial · "+d.Title).AddComponent<VenomLevelController>();
   owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
   owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
   var game=owner.gameObject.AddComponent<VenomCampaign>();string path=$"{SlimeFolder}/Definitions/Slime{d.Key}.asset";var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
   if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
   def.Id="coghe.slime."+d.Key.ToLowerInvariant();def.Order=order;def.Title=d.Title;def.Lesson=d.Goal;def.ViewOnly=false;def.CanRotate=false;def.Passive=false;def.Boss=false;
   def.ProgressKey=COgheSlimeControl.ProgressKey;def.CameraEuler=new Vector3(6,0,0);def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;def.PreferGripForExit=false;
   def.SceneSequence=names;game.Definition=def;
   owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
   var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);pivot.GetComponent<Rigidbody>().isKinematic=true;pivot.GetComponent<Rigidbody>().useGravity=false;owner.Rotation=pivot.GetComponent<BoxRotationController>();
   var c=new ExpansionContext{Number=900+order,Owner=owner,Game=game,Definition=def,Root=pivot.transform};
   var control=owner.gameObject.AddComponent<COgheSlimeControl>();
   control.RoomMin=SlimeMin;control.RoomMax=SlimeMax;control.Lane=0;control.Goal=d.Goal;control.Hint=d.Hint;control.Ink=SlimeInk();control.Font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
   d.Build(c,control);
   var floor=c.Surfaces[0];
   owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
   owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Vector3.up);Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0032f,mint);
   // A wall vent is seen edge-on from the side camera: a short mint duct outside the hole and a printed mark on the back
   // glass make it readable (presentation only).
   var exitMint=SlimeMaterial("Exit mint",new Color(.30f,.85f,.66f),true);
   var duct=MechanismVisual(c.Root,"Exit duct",c.Exit+c.Outward*.018f,new Vector3(.104f,.018f,.104f),exitMint,PrimitiveType.Cylinder);duct.localRotation=Quaternion.FromToRotation(Vector3.up,c.Outward);
   var mark=new GameObject("Exit mark",typeof(LineRenderer)).GetComponent<LineRenderer>();mark.transform.SetParent(c.Root,false);mark.useWorldSpace=false;mark.sharedMaterial=exitMint;mark.startWidth=mark.endWidth=.004f;mark.positionCount=33;
   for(int i=0;i<=32;i++){float a=Mathf.PI*.5f+i*Mathf.PI/32;float sx=c.Outward.x>0?1:-1;mark.SetPosition(i,new Vector3(c.Exit.x+sx*Mathf.Cos(a)*owner.ApertureRadius,c.Exit.y+Mathf.Sin(a)*owner.ApertureRadius,SlimeMax.z-.003f));}
   var label=new GameObject("Exit label",typeof(TextMesh)).GetComponent<TextMesh>();label.transform.SetParent(c.Root,false);label.transform.localPosition=c.Exit-c.Outward*.07f+Vector3.up*(owner.ApertureRadius+.025f)+Vector3.forward*(SlimeMax.z-.004f);
   label.text="LỐI RA";label.fontSize=64;label.characterSize=.005f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.10f,.55f,.40f);label.font=control.Font;label.GetComponent<MeshRenderer>().sharedMaterial=control.Font.material;
   game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=Enumerable.Repeat(floor.Shape,6).ToArray();foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
   var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";camera.orthographic=true;camera.nearClipPlane=.005f;camera.farClipPlane=15;camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
   Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game,$"Meshes/SlimeTrial/{d.Key}");COgheDayLabBuilder.ApplyGlassPreview(game,true);
   var ivory=SpatialMaterial("Ivory bodies",new Color(.88f,.87f,.76f));var slipMat=SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f));
   foreach(var p in c.Surfaces)if(!p.ExteriorGlass&&p!=floor&&p.GetComponent<Renderer>()!=null&&p.GetComponentInParent<VenomMovableProp>()==null)p.GetComponent<Renderer>().sharedMaterial=p.Slippery?slipMat:ivory;
   EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,SlimeScenePath(d.Key));
  }

  /// <summary>Floor and five bare glass panes; the exit vent is cut in the pane facing <see cref="ExpansionContext.Outward"/>.</summary>
  static List<VenomSurfacePatch> SlimeShell(ExpansionContext c)
  {
   Vector3 min=SlimeMin,max=SlimeMax,mid=(min+max)*.5f,size=max-min;
   Panel(c.Root,"Laboratory floor",new Vector3(mid.x,min.y,mid.z),Vector3.up,new Vector2(size.x,size.z),stone,false,Vector2.zero,0,c.Surfaces);
   var pos=new[]{new Vector3(mid.x,max.y,mid.z),new Vector3(mid.x,mid.y,min.z),new Vector3(mid.x,mid.y,max.z),new Vector3(min.x,mid.y,mid.z),new Vector3(max.x,mid.y,mid.z)};
   var normal=new[]{Vector3.down,Vector3.forward,Vector3.back,Vector3.right,Vector3.left};
   var dims=new[]{new Vector2(size.x,size.z),new Vector2(size.x,size.y),new Vector2(size.x,size.y),new Vector2(size.z,size.y),new Vector2(size.z,size.y)};
   var panes=new List<VenomSurfacePatch>();
   for(int i=0;i<5;i++)
   {
    bool hole=Vector3.Dot(normal[i],c.Outward)<-.9f;var rot=Quaternion.LookRotation(normal[i],i==0?Vector3.forward:Vector3.up);var local=Quaternion.Inverse(rot)*(c.Exit-pos[i]);
    var p=Panel(c.Root,"Outer pane "+i,pos[i],normal[i],dims[i],glass,hole,new Vector2(local.x,local.y),hole?c.Owner.ApertureRadius:0,c.Surfaces);
    p.ExteriorGlass=true;p.Selectable=true;p.Slippery=false;panes.Add(p);
   }
   return panes;   // roof, front, back, left, right
  }
  /// <summary>A visible lavender coating on a pane (box-local rectangle, its plane is the pane's): no grip there.</summary>
  static void SlimeCoating(VenomSurfacePatch pane,Vector3 a,Vector3 b)
  {
   Vector3 p=pane.transform.InverseTransformPoint(pane.transform.parent.TransformPoint(a)),q=pane.transform.InverseTransformPoint(pane.transform.parent.TransformPoint(b));
   var rect=Rect.MinMaxRect(Mathf.Min(p.x,q.x),Mathf.Min(p.y,q.y),Mathf.Max(p.x,q.x),Mathf.Max(p.y,q.y));
   var list=(pane.SlipRegions??Array.Empty<Rect>()).ToList();list.Add(rect);pane.SlipRegions=list.ToArray();
   Overlay(pane,rect,SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f)));
   // seen edge-on from the side camera: the coating is also printed as a lavender band on the back glass beside it
   var band=SlimeSprite("Coating band",new Color(.55f,.45f,.85f,.55f));var root=pane.transform.parent;
   Vector2 lo=new Vector2(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y)),hi=new Vector2(Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
   if(hi.x-lo.x<.001f){float side=lo.x<0?1:-1;lo.x=Mathf.Min(lo.x,lo.x+side*.014f);hi.x=Mathf.Max(hi.x,hi.x+side*.014f);}
   if(hi.y-lo.y<.001f){float side=lo.y<.1f?1:-1;lo.y=Mathf.Min(lo.y,lo.y+side*.014f);hi.y=Mathf.Max(hi.y,hi.y+side*.014f);}
   BackPrint(root,"Coating band (back glass)",lo,hi,band);
  }
  static void SlimeAllSlick(List<VenomSurfacePatch> faces){foreach(var f in faces)f.Slippery=true;}

  // S1 · Nhà tù kính — learn the verb. A plain glass box; the vent is high in the right wall, 45 cm over the floor: one
  // strong, well-aimed launch reaches it, or two smaller ones (stick to the wall, hop up). Every miss sticks somewhere and
  // can be corrected from there.
  static void SlimeS1(ExpansionContext c,COgheSlimeControl control)
  {
   c.Spawn=new Vector3(-.14f,-.255f,0);c.Exit=new Vector3(.30f,.20f,0);c.Outward=Vector3.right;SlimeShell(c);
  }

  // S2 · Hành lang laser — the same verb on the environment. The slime starts on a ledge on the left; a red laser curtain
  // (full height and depth) closes the right strip with the exit. A crate on a floor rail must be rammed from its left
  // side, low, onto the pad that switches the laser off: arcs from the ledge land on top of the crate (which does not move
  // it), so the player first drops to the floor beside it. The rail's end is the lip: an overshoot cannot pass it; a
  // weak hit can be repeated; a hit from the right only returns the crate to its start.
  static void SlimeS2(ExpansionContext c,COgheSlimeControl control)
  {
   c.Spawn=new Vector3(-.24f,-.055f,0);c.Exit=new Vector3(.30f,-.06f,0);c.Outward=Vector3.right;SlimeShell(c);
   NextPlinth(c,"Start ledge",new Vector3(-.24f,-.20f,0),new Vector3(.12f,.20f,.20f));   // slick sides: a drop slides to the floor (a body wedged gripless against one may still launch: COgheSlimeControl.RestingLaunchSeconds)
   var crate=SlimeCrate(c,"Crate",new Vector3(.00f,-.268f,0),Vector3.right,.06f,.06f);   // lower than the slime: a ram too high or too hard rides over it into the laser
   var pad=SlimePad(c,"Laser pad",crate,new Vector3(.06f,-.2975f,0));
   var laser=new GameObject("Laser curtain",typeof(COgheSlimeLaser)).GetComponent<COgheSlimeLaser>();laser.transform.SetParent(c.Root,false);
   laser.Pad=pad;laser.Centre=new Vector3(.17f,.10f,0);laser.HalfSize=new Vector3(.004f,.40f,.10f);
   var red=SlimeMaterial("Laser red",new Color(1f,.10f,.08f),true);var beams=new List<Renderer>();
   for(int i=0;i<7;i++){var b=MechanismVisual(c.Root,"Laser beam",new Vector3(.17f,.10f,-.075f+i*.025f),new Vector3(.005f,.40f,.005f),red,PrimitiveType.Cylinder);beams.Add(b.GetComponent<Renderer>());}
   for(int i=0;i<6;i++){var b=MechanismVisual(c.Root,"Laser beam",new Vector3(.17f,-.27f+i*.14f,0),new Vector3(.002f,.10f,.002f),red,PrimitiveType.Cylinder);b.localRotation=Quaternion.Euler(90,0,0);beams.Add(b.GetComponent<Renderer>());}
   beams.Add(BackPrint(c.Root,"Laser glow (back glass)",new Vector2(.158f,-.30f),new Vector2(.182f,.50f),SlimeSprite("Laser glow",new Color(1f,.15f,.1f,.30f))).GetComponent<Renderer>());
   laser.Beams=beams.ToArray();
   foreach(float y in new[]{-.293f,.493f})MechanismVisual(c.Root,"Laser emitter",new Vector3(.17f,y,0),new Vector3(.03f,.014f,.19f),metal);
  }

  // S3 · Quạt khổng lồ — two ways out. A slick glass divider (38 cm, too tall to clear with any launch) splits the room;
  // the exit is low in the right wall of the right chamber. The giant fan in the divider's floor duct runs: its guard
  // shutter closes the duct, and its air rises as an updraft up the divider's left face and turns right under the roof.
  // Way A, ride the wind: launch into the updraft, it lifts the slime over the divider and drops it in the right chamber.
  // Way B, stop the fan: ram the crate onto the pad by the left wall, the fan stops, the shutter opens, launch through the
  // duct. The left wall and roof are coated above 15 cm (no climbing out over the divider).
  static void SlimeS3(ExpansionContext c,COgheSlimeControl control)
  {
   c.Spawn=new Vector3(-.08f,-.255f,0);c.Exit=new Vector3(.30f,-.21f,0);c.Outward=Vector3.right;var panes=SlimeShell(c);
   SlimeCoating(panes[3],new Vector3(-.30f,-.15f,-.10f),new Vector3(-.30f,.50f,.10f));   // left wall above the crate
   SlimeCoating(panes[0],new Vector3(-.30f,.50f,-.10f),new Vector3(.30f,.50f,.10f));     // roof
   SlimeAllSlick(NextPlinth(c,"Divider",new Vector3(.06f,.095f,0),new Vector3(.02f,.57f,.20f)));   // y -.19 … .38
   var guardFaces=NextPlinth(c,"Fan guard shutter",new Vector3(.06f,-.245f,0),new Vector3(.012f,.11f,.20f));SlimeAllSlick(guardFaces);
   var guard=new GameObject("Fan guard (closed while the fan runs)").transform;guard.SetParent(c.Root,false);foreach(var f in guardFaces)f.transform.SetParent(guard,true);
   var louvre=SlimeMaterial("Fan guard grey",new Color(.42f,.47f,.50f));foreach(var f in guardFaces){var r=f.GetComponent<Renderer>();if(r!=null)r.sharedMaterial=louvre;}
   for(int i=0;i<4;i++)MechanismVisual(guard,"Guard louvre",new Vector3(.052f,-.285f+i*.028f,0),new Vector3(.004f,.006f,.19f),metal);
   var crate=SlimeCrate(c,"Crate",new Vector3(-.20f,-.263f,0),Vector3.left,.05f,.07f);
   var pad=SlimePad(c,"Fan pad",crate,new Vector3(-.25f,-.2975f,0));
   var fan=new GameObject("Giant fan",typeof(COgheSlimeFan)).GetComponent<COgheSlimeFan>();fan.transform.SetParent(c.Root,false);fan.Pad=pad;fan.Guard=guard.gameObject;
   fan.Zones=new[]{
    new COgheSlimeFan.Zone{Centre=new Vector3(.005f,.19f,0),HalfSize=new Vector3(.045f,.25f,.10f),Accel=new Vector3(0,9.81f,0),HoldVertical=true,VerticalSpeed=1.0f,Gain=10,Flow=Vector3.up},   // updraft x -.04….05, y -.06….44
    new COgheSlimeFan.Zone{Centre=new Vector3(.10f,.455f,0),HalfSize=new Vector3(.14f,.045f,.10f),Accel=new Vector3(14,9.81f,0),HoldVertical=true,VerticalSpeed=0,Gain=8,Flow=Vector3.right}};  // lid x -.04….24, y .41….50
   // the fan: a housing ring in the duct's left mouth, four blades, spinning on the x axis
   // the giant fan faces the camera from the back glass, under the chimney (a side camera sees a duct fan edge-on)
   var steel=SlimeMaterial("Fan steel",new Color(.70f,.74f,.78f));Vector3 hubAt=new Vector3(-.005f,-.17f,SlimeMax.z-.02f);
   var housing=MechanismVisual(c.Root,"Fan housing",hubAt+Vector3.forward*.008f,new Vector3(.17f,.006f,.17f),steel,PrimitiveType.Cylinder);housing.localRotation=Quaternion.Euler(90,0,0);
   var blades=new GameObject("Fan blades").transform;blades.SetParent(c.Root,false);blades.localPosition=hubAt;fan.Blades=blades;fan.SpinAxis=Vector3.forward;
   var bladeMat=SlimeMaterial("Fan blade",new Color(.20f,.55f,.70f));
   for(int i=0;i<4;i++){var b=MechanismVisual(blades,"Fan blade",Vector3.zero,new Vector3(.026f,.072f,.004f),bladeMat);b.localRotation=Quaternion.Euler(0,0,i*90);b.localPosition=b.localRotation*new Vector3(0,.040f,0);b.localRotation*=Quaternion.Euler(0,25,0);}
   MechanismVisual(blades,"Fan hub",Vector3.zero,new Vector3(.03f,.006f,.03f),steel,PrimitiveType.Cylinder).localRotation=Quaternion.Euler(90,0,0);
   var glow=SlimeSprite("Air glow",new Color(.55f,.80f,1f,.22f));
   fan.AirGlow=new[]{BackPrint(c.Root,"Updraft (back glass)",new Vector2(-.04f,-.06f),new Vector2(.05f,.44f),glow).GetComponent<Renderer>(),BackPrint(c.Root,"Lid wind (back glass)",new Vector2(-.04f,.41f),new Vector2(.24f,.50f),glow).GetComponent<Renderer>()};
   // the shutter is drawn on the back glass too (closed: grey slats over the duct; it vanishes with the guard)
   var slat=SlimeSprite("Shutter slats",new Color(.30f,.34f,.38f,.85f));
   for(int i=0;i<5;i++)BackPrint(guard,"Shutter slat (back glass)",new Vector2(.035f,-.30f+i*.022f),new Vector2(.085f,-.288f+i*.022f),slat,.003f);
   // the updraft's printed guide on the back glass: arrows up the divider and along the roof
   var air=SlimeMaterial("Wind white",new Color(.92f,.97f,1f));var streaks=new List<Transform>();var zones=new List<int>();
   for(int i=0;i<8;i++){var s=MechanismVisual(c.Root,"Wind streak",new Vector3(-.03f+(i%3)*.03f,0,-.06f+(i%4)*.04f),new Vector3(.05f,.003f,.003f),air);streaks.Add(s);zones.Add(0);}
   for(int i=0;i<6;i++){var s=MechanismVisual(c.Root,"Wind streak",new Vector3(0,.43f+(i%3)*.025f,-.06f+(i%4)*.04f),new Vector3(.05f,.003f,.003f),air);streaks.Add(s);zones.Add(1);}
   fan.Streaks=streaks.ToArray();fan.StreakZones=zones.ToArray();
  }

  /// <summary>A crate on a floor rail (no handle, no taps): only a ram along the rail moves it; the far end is a latch.</summary>
  static COgheRailSlider SlimeCrate(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,float height)
  {
   var rail=ExpansionRail(c,name,start,axis,travel,0,new Vector3(.09f,height,.12f),.08f,.06f,false,false);
   rail.LatchAtEnd=true;rail.Damping=.12f;
   var red=SlimeMaterial("Crate red",new Color(.80f,.30f,.24f));foreach(var r in rail.GetComponentsInChildren<Renderer>())r.sharedMaterial=red;
   return rail;
  }
  static COgheSlimePad SlimePad(ExpansionContext c,string name,COgheRailSlider crate,Vector3 at)
  {
   var pad=new GameObject(name,typeof(COgheSlimePad)).GetComponent<COgheSlimePad>();pad.transform.SetParent(c.Root,false);pad.transform.localPosition=at;pad.Crate=crate;
   var lamp=MechanismVisual(pad.transform,name+" cap",Vector3.zero,new Vector3(.11f,.004f,.13f),SlimeMaterial("Pad amber",new Color(.95f,.62f,.18f),true));pad.Lamp=lamp.GetComponent<Renderer>();
   return pad;
  }
 }
}

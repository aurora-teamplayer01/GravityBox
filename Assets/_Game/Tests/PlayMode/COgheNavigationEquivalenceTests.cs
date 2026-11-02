using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheNavigationEquivalenceTests
    {
        private const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        private SimulationMode previousSimulation;
        private bool previousMetrics,previousPersistence;
        private GameObject fixture;
        private VenomCampaign game;

        // Calls the unchanged exact leaf test without BVH rejection.
        // This deliberately pays for every collider/plane as an independent broad-phase reference.
        private sealed class ExactQueries
        {
            private delegate bool BlockedQuery(int item,Vector3 a,Vector3 b,Ray ray,Vector3 inverse,float length,
                Vector3 segmentMin,Vector3 segmentMax,float radius);
            private readonly object snapshot;
            private readonly Action<VenomCampaign> capture;
            private readonly Func<Vector3,Vector3,float,bool> clear;
            private readonly BlockedQuery blocked;
            private int leaves;
            public ExactQueries()
            {
                var type=typeof(VenomCampaign).Assembly.GetType("GravityBox.Venom.VenomNavigationSnapshot");
                snapshot=Activator.CreateInstance(type);
                capture=(Action<VenomCampaign>)Delegate.CreateDelegate(typeof(Action<VenomCampaign>),snapshot,type.GetMethod("Capture"));
                clear=(Func<Vector3,Vector3,float,bool>)Delegate.CreateDelegate(typeof(Func<Vector3,Vector3,float,bool>),snapshot,type.GetMethod("Clear"));
                blocked=(BlockedQuery)Delegate.CreateDelegate(typeof(BlockedQuery),snapshot,type.GetMethod("Blocked",PrivateInstance));
            }
            public void Capture(VenomCampaign owner)
            {
                capture(owner);leaves=owner.Surfaces.Length;
                foreach(var prop in owner.Props)leaves+=prop.CollisionShapes.Length;
            }
            public bool Optimized(Vector3 a,Vector3 b,float radius)=>clear(a,b,radius);
            public bool FullScan(Vector3 a,Vector3 b,float radius)
            {
                Vector3 delta=b-a;if(delta.sqrMagnitude<.000001f)return true;
                float length=delta.magnitude-.002f;var ray=new Ray(a,delta.normalized);
                Vector3 d=ray.direction,inverse=new Vector3(1/d.x,1/d.y,1/d.z);
                Vector3 min=Vector3.Min(a,b),max=Vector3.Max(a,b);
                for(int i=0;i<leaves;i++)if(blocked(i,a,b,ray,inverse,length,min,max,radius))return false;
                return true;
            }
        }

        [UnitySetUp] public IEnumerator Before()
        {
            previousSimulation=Physics.simulationMode;previousMetrics=COgheMobileMetrics.Enabled;
            previousPersistence=VenomCampaignSave.PersistenceEnabled;
            Physics.simulationMode=SimulationMode.Script;COgheMobileMetrics.Enabled=true;
            VenomCampaignSave.PersistenceEnabled=false;yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            if(fixture!=null)Object.Destroy(fixture);
            Physics.simulationMode=previousSimulation;COgheMobileMetrics.Enabled=previousMetrics;
            VenomCampaignSave.PersistenceEnabled=previousPersistence;Time.timeScale=1;yield return null;
        }

        private VenomSurfacePatch Patch(string name,Vector2 size,Vector3 position,Quaternion rotation,bool collider)
        {
            var go=new GameObject(name);go.transform.SetParent(fixture.transform,false);
            go.transform.SetPositionAndRotation(position,rotation);
            var patch=go.AddComponent<VenomSurfacePatch>();patch.Size=size;
            if(collider)
            {
                var box=go.AddComponent<BoxCollider>();box.size=new Vector3(size.x,size.y,.0003f);
                patch.Shape=box;
            }
            return patch;
        }

        [UnityTest] public IEnumerator SegmentPruningPreservesThinPanesAperturesAndRotatedMovingObstacles()
        {
            // Query fixture only: posing blockers is not used to claim a playable solve.
            fixture=new GameObject("Navigation conservative-query fixture");
            // Match the campaign's normal Initialize/OnDestroy touch-support lifetime.
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            game=fixture.AddComponent<VenomCampaign>();game.enabled=false;
            var patches=new List<VenomSurfacePatch>();
            var aperture=Patch("Analytical aperture",new Vector2(.24f,.20f),Vector3.zero,Quaternion.identity,false);
            aperture.Hole=true;aperture.HoleRadius=.04f;patches.Add(aperture);
            // This collider is offset from the patch plane, so collider bounds must also survive pruning.
            var thin=Patch("Thin offset collider",new Vector2(.13f,.11f),new Vector3(.3f,0,0),Quaternion.identity,true);
            ((BoxCollider)thin.Shape).center=new Vector3(0,0,.013f);patches.Add(thin);
            for(int i=0;i<18;i++)
                patches.Add(Patch("Angled pane "+i,new Vector2(.09f+.01f*(i%3),.075f),
                    new Vector3(-.45f+(i%6)*.18f,.25f+(i/6)*.16f,(i%2)*.11f),
                    Quaternion.Euler(13*i,31*i,7*i),true));
            // Props exercise the separate collider-only leaves, including a moving/rotating thin door.
            var propObject=new GameObject("Moving door");propObject.transform.SetParent(fixture.transform,false);
            var body=propObject.AddComponent<Rigidbody>();body.isKinematic=true;
            var shape=propObject.AddComponent<BoxCollider>();shape.size=new Vector3(.16f,.12f,.0004f);
            var prop=propObject.AddComponent<VenomMovableProp>();prop.Body=body;
            game.Surfaces=patches.ToArray();game.Props=new[]{prop};
            var queries=new ExactQueries();var random=new System.Random(5292026);
            int blocked=0,clear=0;
            for(int pose=0;pose<3;pose++)
            {
                body.position=new Vector3(.1f,.12f,-.16f)+new Vector3(.025f,0,.018f)*pose;
                body.rotation=Quaternion.Euler(17*pose,29*pose,11*pose);
                body.transform.SetPositionAndRotation(body.position,body.rotation);
                thin.transform.SetPositionAndRotation(new Vector3(.3f+.021f*pose,0,.016f*pose),Quaternion.Euler(23*pose,41*pose,0));
                aperture.NavigationHoleBlocked=pose==1;
                patches[5].Shape.enabled=pose!=2;
                patches[8].enabled=pose!=1;
                Physics.SyncTransforms();queries.Capture(game);
                Vector3 centre=aperture.transform.position,normal=aperture.Normal;
                Assert.AreEqual(pose!=1,queries.Optimized(centre-normal*.03f,centre+normal*.03f,0),"Open/closed aperture");
                // The analytical plane includes t == 1; using the shortened collider ray here would miss it.
                Vector3 rim=aperture.transform.TransformPoint(new Vector3(.08f,0,0));
                Assert.IsFalse(queries.Optimized(rim-normal*.03f,rim,0),"Endpoint on a solid pane");
                foreach(var patch in patches)
                    for(int trial=0;trial<40;trial++)
                    {
                        Vector3 local=new Vector3(((float)random.NextDouble()-.5f)*patch.Size.x*1.4f,
                            ((float)random.NextDouble()-.5f)*patch.Size.y*1.4f,0);
                        Vector3 a=patch.transform.TransformPoint(local+Vector3.back*.035f);
                        Vector3 b=patch.transform.TransformPoint(local+new Vector3((float)random.NextDouble()*.035f-.0175f,0,.035f));
                        if(trial%4==0)b=patch.transform.TransformPoint(local); // exact endpoint, including holes/rims
                        if(trial%7==0)b=a+Vector3.right*.09f; // zero ray components and coplanar cases
                        foreach(float radius in new[]{0,.006f,.01f,.014f})
                        {
                            bool expected=queries.FullScan(a,b,radius);
                            Assert.AreEqual(expected,queries.Optimized(a,b,radius),$"Forward pose={pose} pane={patch.name} trial={trial} radius={radius}");
                            Assert.AreEqual(queries.FullScan(b,a,radius),queries.Optimized(b,a,radius),$"Reverse pose={pose} pane={patch.name} trial={trial} radius={radius}");
                            if(expected)clear++;else blocked++;
                        }
                    }
                // Also exercise longer diagonal segments with sparse intervening planes.
                for(int trial=0;trial<300;trial++)
                {
                    Vector3 a=new Vector3((float)random.NextDouble()-.5f,(float)random.NextDouble()*.8f-.1f,-.2f);
                    Vector3 b=new Vector3((float)random.NextDouble()-.5f,(float)random.NextDouble()*.8f-.1f,.25f);
                    Assert.AreEqual(queries.FullScan(a,b,.006f),queries.Optimized(a,b,.006f),$"Diagonal {pose}/{trial}");
                }
            }
            Assert.Greater(clear,0);Assert.Greater(blocked,0);yield return null;
        }

        [UnityTest] public IEnumerator QueriesMatchExactAcrossMutableRemovedAndReorderedBlockers()
        {
            // Geometry-query fixture, not a playable solve. Keep the same rays across captures
            // to compare current geometry against the independent full-leaf reference.
            fixture=new GameObject("Navigation visibility reuse fixture");
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            game=fixture.AddComponent<VenomCampaign>();game.enabled=false;
            var aperture=Patch("Aperture",new Vector2(.24f,.20f),Vector3.zero,Quaternion.identity,false);
            aperture.Hole=true;aperture.HoleRadius=.04f;
            var second=Patch("Second plane",new Vector2(.12f,.12f),new Vector3(.22f,0,0),Quaternion.identity,false);
            var replacement=Patch("Replacement",new Vector2(.16f,.16f),new Vector3(.1f,0,.02f),Quaternion.Euler(0,24,0),false);
            var curved=Patch("Mutable mesh",new Vector2(.2f,.2f),new Vector3(-.3f,0,0),Quaternion.identity,false);
            // Curved surfaces use only their collider in the exact segment test.
            curved.SphereRadius=.1f;
            var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.1f,-.1f,0),new Vector3(.1f,-.1f,0),new Vector3(0,.1f,0)};
            mesh.triangles=new[]{0,1,2};mesh.RecalculateBounds();
            var meshShape=curved.gameObject.AddComponent<MeshCollider>();meshShape.sharedMesh=mesh;curved.Shape=meshShape;
            var propObject=new GameObject("Door body");propObject.transform.SetParent(fixture.transform,false);
            var body=propObject.AddComponent<Rigidbody>();body.isKinematic=true;body.position=new Vector3(.36f,0,0);
            var child=new GameObject("Child collider");child.transform.SetParent(propObject.transform,false);
            var shape=child.AddComponent<BoxCollider>();shape.size=new Vector3(.12f,.12f,.0004f);
            var sphereObject=new GameObject("Sphere collider");sphereObject.transform.SetParent(propObject.transform,false);sphereObject.transform.localPosition=new Vector3(.11f,.15f,0);
            var sphere=sphereObject.AddComponent<SphereCollider>();sphere.radius=.035f;
            var capsuleObject=new GameObject("Capsule collider");capsuleObject.transform.SetParent(propObject.transform,false);capsuleObject.transform.localPosition=new Vector3(-.81f,-.05f,0);
            var capsule=capsuleObject.AddComponent<CapsuleCollider>();capsule.radius=.025f;capsule.height=.10f;
            var prop=propObject.AddComponent<VenomMovableProp>();prop.Body=body;
            var originals=new[]{aperture,second,curved};game.Surfaces=originals;game.Props=new[]{prop};
            var queries=new ExactQueries();var rays=new List<(Vector3 a,Vector3 b)>();
            for(int x=0;x<=45;x++)for(int y=0;y<=8;y++)
                rays.Add((new Vector3(-.5f+x/45f,-.12f+y*.03f,-.055f),new Vector3(-.5f+x/45f,-.12f+y*.03f,.055f)));
            rays.Add((new Vector3(.36f,0,-.02f),new Vector3(.36f,0,.001f)));
            rays.Add((new Vector3(2,0,0),new Vector3(2.05f,0,0)));
            foreach(float x in new[]{.04f,.04f-.0000001f,.04f+.0000001f,.12000003f})
                rays.Add((new Vector3(x,0,-.055f),new Vector3(x,0,.055f)));
            rays.Add((new Vector3(.08f,0,.02f),new Vector3(.08f,0,.0300005f)));
            Action[] changes={
                ()=>{},()=>{},
                ()=>aperture.NavigationHoleBlocked=true,
                ()=>aperture.NavigationHoleBlocked=false,
                ()=>aperture.HoleCentre=new Vector2(.0000001f,0),
                ()=>aperture.HoleRadius+=.0000001f,
                ()=>aperture.Size+=new Vector2(.0000001f,0),
                ()=>aperture.transform.localPosition=new Vector3(0,0,.03f),
                ()=>aperture.transform.localPosition+=new Vector3(0,0,.000001f),
                ()=>{aperture.transform.localPosition=Vector3.zero;aperture.Size=new Vector2(.24f,.20f);aperture.HoleCentre=Vector2.zero;aperture.HoleRadius=.04f;},
                ()=>{shape.enabled=false;sphere.radius=.04f;capsule.radius=.03f;},
                ()=>{shape.enabled=true;shape.size=new Vector3(.10f,.14f,.0004f);sphere.center=new Vector3(0,-.015f,0);capsule.height=.12f;},
                ()=>{shape.center=new Vector3(.003f,0,.004f);capsule.direction=2;capsule.center=new Vector3(.01f,0,0);},
                ()=>{body.position+=new Vector3(.025f,.01f,0);body.rotation=Quaternion.Euler(9,31,7);child.transform.localScale=new Vector3(.9f,1.2f,1);},
                ()=>child.transform.localScale=new Vector3(-.9f,1.2f,1),
                ()=>{body.position=new Vector3(.36f,0,0);body.rotation=Quaternion.identity;child.transform.localScale=Vector3.one;shape.center=Vector3.zero;shape.size=new Vector3(.12f,.12f,.0004f);sphere.radius=.035f;sphere.center=Vector3.zero;capsule.radius=.025f;capsule.height=.10f;capsule.direction=1;capsule.center=Vector3.zero;},
                ()=>{mesh.triangles=new[]{2,1,0};meshShape.sharedMesh=null;meshShape.sharedMesh=mesh;},
                ()=>game.Surfaces=new[]{curved,aperture,second},
                ()=>game.Surfaces=new[]{curved,second},
                ()=>game.Surfaces=new[]{replacement,second},
                ()=>game.Surfaces=originals,
                ()=>aperture.enabled=false,
                ()=>aperture.enabled=true,
                ()=>aperture.gameObject.SetActive(false),
                ()=>{aperture.gameObject.SetActive(true);fixture.transform.rotation=Quaternion.Euler(0,7,0);},
                ()=>fixture.transform.rotation=Quaternion.identity
            };
            try
            {
                for(int pose=0;pose<changes.Length;pose++)
                {
                    changes[pose]();Physics.SyncTransforms();queries.Capture(game);
                    foreach(var ray in rays)foreach(float radius in new[]{0,.006f,.01f,.014f})
                    {
                        Assert.AreEqual(queries.FullScan(ray.a,ray.b,radius),queries.Optimized(ray.a,ray.b,radius),$"Retained forward pose={pose} radius={radius} a={ray.a}");
                        Assert.AreEqual(queries.FullScan(ray.b,ray.a,radius),queries.Optimized(ray.b,ray.a,radius),$"Retained reverse pose={pose} radius={radius} a={ray.a}");
                    }
                    if(pose==0)
                    {
                        var a=new Vector3(.36f,0,-.02f);var b=new Vector3(.36f,0,.001f);
                        Assert.IsTrue(queries.FullScan(a,b,0),"Forward collider hit is in the omitted final 2 mm");
                        Assert.IsFalse(queries.FullScan(b,a,0),"Reverse ray must not share the forward answer");
                    }
                }
            }
            finally{Object.Destroy(mesh);}
            yield return null;
        }

        [UnityTest] public IEnumerator QueriesMatchExactWhenDoorsOpenReorderOrMeshesRecook()
        {
            // Geometry fixture only. Native cooked collider state remains authoritative;
            // compare every mutation against a full scan of the current exact leaves.
            fixture=new GameObject("Current navigation geometry fixture");
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            game=fixture.AddComponent<VenomCampaign>();game.enabled=false;
            var pane=Patch("Witness door",new Vector2(.2f,.2f),Vector3.zero,Quaternion.identity,false);
            var far=Patch("Other door",new Vector2(.2f,.2f),new Vector3(.4f,0,0),Quaternion.identity,false);
            game.Surfaces=new[]{pane};game.Props=Array.Empty<VenomMovableProp>();
            var queries=new ExactQueries();Vector3 a=new Vector3(0,0,-.04f),b=new Vector3(0,0,.04f);
            bool Check(bool? expected,string label)
            {
                Physics.SyncTransforms();queries.Capture(game);bool exact=queries.FullScan(a,b,.006f);
                if(expected.HasValue)Assert.AreEqual(expected.Value,exact,label+" fixture premise");
                Assert.AreEqual(exact,queries.Optimized(a,b,.006f),label+" exact reference");
                return exact;
            }
            Check(false,"Initial door");Check(false,"Unchanged door");
            pane.transform.position=Vector3.right*.3f;
            Check(true,"Old blocker moves away");
            pane.transform.position=Vector3.zero;Check(false,"Door returns");
            pane.Hole=true;pane.HoleRadius=.04f;
            Check(true,"Aperture opens");
            pane.NavigationHoleBlocked=true;Check(false,"Aperture closes");
            pane.NavigationHoleBlocked=false;
            Check(true,"Aperture reopens");
            pane.Hole=false;game.Surfaces=new[]{pane,far};Check(false,"Two indexed surfaces");
            game.Surfaces=new[]{far,pane};
            Check(false,"Surface order changes while a current leaf still blocks");
            game.Surfaces=new[]{far};
            Check(true,"Removed tail blocker");
            // Replace a surface index with a prop collider index; the exact leaf domain changes.
            var propObject=new GameObject("Replacement prop");propObject.transform.SetParent(fixture.transform,false);
            var body=propObject.AddComponent<Rigidbody>();body.isKinematic=true;
            var box=propObject.AddComponent<BoxCollider>();box.size=new Vector3(.12f,.12f,.0004f);
            var prop=propObject.AddComponent<VenomMovableProp>();prop.Body=body;
            game.Surfaces=new[]{pane};Check(false,"Surface before prop replacement");
            game.Surfaces=Array.Empty<VenomSurfacePatch>();game.Props=new[]{prop};Check(false,"Same index now a current prop");
            body.position=Vector3.right*.4f;
            body.transform.SetPositionAndRotation(body.position,body.rotation);
            Check(true,"Prop blocker moves away");
            game.Props=Array.Empty<VenomMovableProp>();
            var curved=Patch("Mutable collider-only pane",new Vector2(.2f,.2f),Vector3.zero,Quaternion.identity,false);curved.SphereRadius=.1f;
            var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.1f,-.1f,0),new Vector3(.1f,-.1f,0),new Vector3(0,.1f,0)};
            mesh.triangles=new[]{0,1,2};mesh.RecalculateBounds();var shape=curved.gameObject.AddComponent<MeshCollider>();shape.sharedMesh=mesh;curved.Shape=shape;
            game.Surfaces=new[]{curved};a=new Vector3(0,0,.04f);b=new Vector3(0,0,-.04f);
            bool backfaces=Physics.queriesHitBackfaces;
            try
            {
                Physics.queriesHitBackfaces=false;Check(false,"Original cooked triangle");Check(false,"Unchanged cooked triangle");
                mesh.triangles=new[]{2,1,0};shape.sharedMesh=null;shape.sharedMesh=mesh;
                Check(true,"Same-bounds winding recooked");
                mesh.triangles=new[]{0,1,2};shape.sharedMesh=null;shape.sharedMesh=mesh;Check(false,"Cooked triangle restored");
                mesh.triangles=new[]{2,1,0};shape.sharedMesh=null;shape.sharedMesh=mesh;mesh.triangles=new[]{0,1,2};
                // Do not assume whether this Unity version automatically recooks the last
                // raw edit. Whatever PhysX currently reports is the reference authority.
                bool delayed=Check(null,"Raw A to B, cook B, raw A without explicit recook");
                Debug.Log($"NAV_EXACT_DEFERRED_RECOOK exactClear={delayed}");
                game.Surfaces=Array.Empty<VenomSurfacePatch>();Check(true,"All blockers removed");
            }
            finally{Physics.queriesHitBackfaces=backfaces;Object.Destroy(mesh);}
            yield return null;
        }

        private IEnumerator Load(string sceneName)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync(sceneName);}finally{SceneManager.sceneLoaded-=Loaded;}
            for(int i=0;i<30;i++){game.Owner.Step(1f/120);game.Owner.Rotation.Step(1f/120);Physics.Simulate(1f/120);}
            yield return null;
        }

        [UnityTest,Timeout(900000)] public IEnumerator N43AndN50GraphEdgesMatchExactReferenceAfterMechanismAndApertureChanges()
        {
            foreach(string scene in new[]{"COgheSpatialPlusN43","COgheSpatialPlusN50"})
            {
                yield return Load(scene);
                var moving=Array.Find(game.Surfaces,p=>p.Shape!=null&&p.Shape.attachedRigidbody!=null&&p.Shape.attachedRigidbody.transform!=game.Root);
                var hole=Array.Find(game.Surfaces,p=>p.Hole);
                Assert.NotNull(moving,"Fixture needs a moving mechanism surface");Assert.NotNull(hole,"Fixture needs an aperture");
                var boxPatch=Array.Find(game.Surfaces,p=>p.Shape is BoxCollider);Assert.NotNull(boxPatch);
                var box=(BoxCollider)boxPatch.Shape;var boxSize=box.size;var boxCentre=box.center;
                var body=moving.Shape.attachedRigidbody;var position=body.position;var rotation=body.rotation;
                var holeCentre=hole.HoleCentre;float holeRadius=hole.HoleRadius;bool holeBlocked=hole.NavigationHoleBlocked;
                bool shapeEnabled=moving.Shape.enabled,patchEnabled=moving.enabled;
                var rootRotation=game.Root.rotation;
                // Forced builds deliberately include changes smaller than the retained graph's
                // existing revision tolerance. Captured queries must match exact geometry.
                Action[] changes={
                    ()=>{},()=>{},
                    ()=>{body.position=position+new Vector3(.013f,.009f,-.007f);body.rotation=Quaternion.AngleAxis(9,Vector3.up)*rotation;},
                    ()=>body.position+=new Vector3(.000001f,0,0),
                    ()=>{body.position=position;body.rotation=rotation;},
                    ()=>hole.NavigationHoleBlocked=!holeBlocked,
                    ()=>{hole.HoleCentre+=new Vector2(.0000001f,0);hole.HoleRadius+=.0000001f;},
                    ()=>{hole.NavigationHoleBlocked=holeBlocked;hole.HoleCentre=holeCentre;hole.HoleRadius=holeRadius;},
                    ()=>moving.Shape.enabled=false,
                    ()=>{moving.Shape.enabled=shapeEnabled;box.size=boxSize+new Vector3(.000001f,0,0);box.center=boxCentre+new Vector3(0,.000001f,0);},
                    ()=>{box.size=boxSize;box.center=boxCentre;},
                    ()=>moving.enabled=false,
                    ()=>moving.enabled=patchEnabled,
                    ()=>game.Root.rotation=Quaternion.AngleAxis(7,Vector3.up)*rootRotation,
                    ()=>game.Root.rotation=rootRotation
                };
                for(int pose=0;pose<changes.Length;pose++)
                {
                    // Geometry-equivalence fixture, not a solve or a claim of legal mechanism motion.
                    changes[pose]();body.transform.SetPositionAndRotation(body.position,body.rotation);
                    Physics.SyncTransforms();game.Motion.BuildGraph(true);
                    AssertGraphMatchesExactReference(scene+"/"+pose);
                    Debug.Log($"NAV_EQUIVALENCE {scene}/{pose} full adjacency matches exact reference");
                    yield return null;
                }
            }
        }

        private void AssertGraphMatchesExactReference(string label)
        {
            var nodes=(List<Vector3>)typeof(VenomCampaignMotion).GetField("nodes",PrivateInstance).GetValue(game.Motion);
            var actual=(List<List<int>>)typeof(VenomCampaignMotion).GetField("links",PrivateInstance).GetValue(game.Motion);
            var world=(List<Vector3>)typeof(VenomCampaignMotion).GetField("worldNodes",PrivateInstance).GetValue(game.Motion);
            var queries=new ExactQueries();queries.Capture(game);
            var cells=new Dictionary<Vector3Int,List<int>>();var expected=new List<int>[nodes.Count];
            for(int i=0;i<nodes.Count;i++)
            {
                expected[i]=new List<int>();
                var cell=Cell(nodes[i]);if(!cells.TryGetValue(cell,out var bucket)){bucket=new List<int>();cells.Add(cell,bucket);}bucket.Add(i);
            }
            int checks=0;var neighbours=new List<int>();
            for(int i=0;i<nodes.Count;i++)
            {
                // Baseline candidate traversal: 27 lookups per node, then the original stable sort.
                neighbours.Clear();var cell=Cell(nodes[i]);
                for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)
                    if(cells.TryGetValue(cell+new Vector3Int(x,y,z),out var bucket))
                        foreach(int j in bucket)if(j>i&&(nodes[i]-nodes[j]).sqrMagnitude<=.10f*.10f)neighbours.Add(j);
                neighbours.Sort();checks+=neighbours.Count;
                foreach(int j in neighbours)if(queries.FullScan(world[i],world[j],.006f)){expected[i].Add(j);expected[j].Add(i);}
            }
            Assert.Greater(nodes.Count,0);
            Assert.Greater(checks,0,label+" must compare candidate pairs");
            for(int i=0;i<nodes.Count;i++)CollectionAssert.AreEqual(expected[i],actual[i],label+" node "+i+" adjacency and tie order");
        }
        private static Vector3Int Cell(Vector3 p)=>new Vector3Int(Mathf.FloorToInt(p.x/.1f),Mathf.FloorToInt(p.y/.1f),Mathf.FloorToInt(p.z/.1f));
    }
}

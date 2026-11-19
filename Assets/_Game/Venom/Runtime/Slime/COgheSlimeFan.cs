using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Slime Trial S3: a giant fan. While it runs, its guard shutter closes the duct through the divider and its
    /// air drives the wind <see cref="Zones"/>: every particle of an airborne body (fewer than two gripping particles)
    /// inside a zone gets that zone's acceleration (box-local). A zone may hold a vertical speed (an updraft carries a
    /// body up at that speed; a lid zone holds its height while it pushes it along). A stuck body is not blown: it clings.
    /// Ramming the crate onto <see cref="Pad"/> stops the fan: no wind, the shutter opens the duct.</summary>
    public sealed class COgheSlimeFan : COgheMechanism
    {
        [System.Serializable] public struct Zone{public Vector3 Centre,HalfSize,Accel;public bool HoldVertical;public float VerticalSpeed,Gain;public Vector3 Flow;}
        public COgheSlimePad Pad;
        public Zone[] Zones=System.Array.Empty<Zone>();
        public GameObject Guard;
        public Transform Blades;
        public Vector3 SpinAxis=Vector3.forward;
        /// <summary>Presentation: the visible air (glow on the back glass), shown while the blades turn.</summary>
        public Renderer[] AirGlow=System.Array.Empty<Renderer>();
        public Transform[] Streaks=System.Array.Empty<Transform>();
        public int[] StreakZones=System.Array.Empty<int>();
        public bool Running=>Pad==null||!Pad.Loaded;
        /// <summary>Simulation seconds the body has been carried by the wind since the level was reset (tests, clips).</summary>
        public float Carried {get;private set;}
        private float spin,angle,flow;
        public override void ResetMechanism(VenomCampaign game){Carried=0;if(Guard!=null)Guard.SetActive(true);}
        public override void StepMechanism(VenomCampaign game,float dt)
        {
            if(Guard!=null&&Guard.activeSelf!=Running){Guard.SetActive(Running);game.Motion.BuildGraph(true);}
            if(!Running||game.Owner.Lost||game.Owner.Completed)return;
            var root=game.Root;bool any=false;
            for(int g=0;g<CohesiveOrganism.ParticleCount;g++)
            {
                bool first=true;for(int k=0;k<g;k++)if(game.Matter.Groups[k]==game.Matter.Groups[g]){first=false;break;}
                if(!first)continue;
                int grips=0;for(int i=0;i<CohesiveOrganism.ParticleCount;i++)if(game.Matter.Groups[i]==game.Matter.Groups[g]&&game.Motion.HasGrip(i))grips++;
                if(grips>=2)continue;
                for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[g]||game.Matter.Escaped[i])continue;
                    var body=game.Matter.Bodies[i];Vector3 local=root.InverseTransformPoint(body.position);
                    foreach(var z in Zones)
                    {
                        Vector3 p=local-z.Centre;
                        if(Mathf.Abs(p.x)>z.HalfSize.x||Mathf.Abs(p.y)>z.HalfSize.y||Mathf.Abs(p.z)>z.HalfSize.z)continue;
                        Vector3 a=z.Accel;
                        if(z.HoldVertical){float vy=root.InverseTransformDirection(body.linearVelocity).y;a.y+=Mathf.Clamp((z.VerticalSpeed-vy)*z.Gain,-12,12);}
                        body.AddForce(root.TransformDirection(a),ForceMode.Acceleration);any=true;break;
                    }
                }
            }
            if(any)Carried+=dt;
        }
        private void Update()
        {
            float dt=Time.deltaTime;spin=Mathf.MoveTowards(spin,Running?1:0,dt*(Running?1.5f:.6f));
            angle+=spin*900*dt;if(Blades!=null)Blades.localRotation=Quaternion.AngleAxis(angle,SpinAxis);
            foreach(var r in AirGlow)if(r!=null)r.enabled=spin>.05f;
            flow+=spin*dt*.8f;
            for(int i=0;i<Streaks.Length;i++)
            {
                var s=Streaks[i];if(s==null)continue;int zi=i<StreakZones.Length?StreakZones[i]:0;if(zi>=Zones.Length)continue;var z=Zones[zi];
                float u=Mathf.Repeat(flow+i*.37f,1);s.gameObject.SetActive(spin>.05f);
                Vector3 axis=z.Flow.sqrMagnitude>0?z.Flow.normalized:Vector3.right;
                Vector3 extent=Vector3.Scale(axis,z.HalfSize);var p=s.localPosition;Vector3 across=p-Vector3.Project(p-z.Centre,axis)-z.Centre;
                s.localPosition=z.Centre+across+extent*(u*2-1);
                s.localRotation=Quaternion.FromToRotation(Vector3.right,axis);s.localScale=new Vector3(.06f*spin,.005f,.002f);
            }
        }
    }
}

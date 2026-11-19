using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Slime Trial S2: a full-height curtain of red beams. Any particle inside it while it is on is a failure
    /// (short message, Retry). Off while <see cref="Pad"/> is loaded.</summary>
    public sealed class COgheSlimeLaser : COgheMechanism
    {
        public COgheSlimePad Pad;
        public Vector3 Centre,HalfSize;   // box-local (the chamber root)
        public Renderer[] Beams=System.Array.Empty<Renderer>();
        public string Message="Chạm tia laser! Thử lại.";
        public bool On=>Pad==null||!Pad.Loaded;
        public override void ResetMechanism(VenomCampaign game){}
        public override void StepMechanism(VenomCampaign game,float dt)
        {
            if(!On||game.Owner.Lost||game.Owner.Completed)return;
            float r=game.Matter.Profile.ParticleRadius;var root=game.Root;
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
            {
                if(game.Matter.Escaped[i])continue;Vector3 p=root.InverseTransformPoint(game.Matter.Bodies[i].position)-Centre;
                if(Mathf.Abs(p.x)<HalfSize.x+r&&Mathf.Abs(p.y)<HalfSize.y+r&&Mathf.Abs(p.z)<HalfSize.z+r){Debug.Log($"SLIME LASER hit by particle {i}");game.Fail(Message);return;}
            }
        }
        private void LateUpdate()
        {
            bool on=On;
            for(int i=0;i<Beams.Length;i++)if(Beams[i]!=null)Beams[i].enabled=on;
        }
    }
}

using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Slime Trial: a crate on a rail that the slime rams onto a pad. "Loaded" means the crate rests at the rail's
    /// pad end (the rail's end stop is the low lip: an overshoot cannot pass it, a short hit can be repeated). The pad lamp
    /// shows the state; laser and fan read <see cref="Loaded"/>.</summary>
    public sealed class COgheSlimePad : COgheMechanism
    {
        public COgheRailSlider Crate;
        public Renderer Lamp;
        public float Tolerance=.006f;
        public bool Loaded=>Crate!=null&&Crate.Position>=Crate.Travel-Tolerance;
        private MaterialPropertyBlock block;
        public override void ResetMechanism(VenomCampaign game)=>Show();
        public override void StepMechanism(VenomCampaign game,float dt){}
        private void LateUpdate()=>Show();
        private void Show()
        {
            if(Lamp==null)return;block=block??new MaterialPropertyBlock();
            Color c=Loaded?new Color(.30f,1f,.55f):new Color(.95f,.62f,.18f);block.SetColor("_BaseColor",c);block.SetColor("_Color",c);block.SetColor("_EmissionColor",c*(Loaded?1.6f:.4f));Lamp.SetPropertyBlock(block);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    /// <summary>The headless launch search (EXPERIMENT.md / brief: "search a grid of angle × power inside one PlayMode run,
    /// reset, replay, measure, then write the found inputs into the tests"). Explicit: run on demand. Each candidate is
    /// replayed from Retry through the same pointer path as the tests and the player. Reports go to Artifacts/SlimeTrial.</summary>
    public sealed partial class COgheSlimeTrialTests
    {
        public struct Shot
        {
            public float Angle,Power;public Shot(float angle,float power){Angle=angle;Power=power;}
            public override string ToString()=>$"{Angle:0.#}°/{Power:0.##}";
        }
        private sealed class Outcome
        {
            public Shot Shot;public bool Taken,Won,Lost;public string Failure;public Vector3 Body;public float Crate=-1;public bool FanOn;public float Carried;public int Robust;
            public override string ToString()=>$"{Shot} taken={Taken} won={Won} lost={Lost}{(Lost?" ("+Failure+")":"")} body={Body:F3} crate={Crate:F3} fan={(FanOn?"on":"off")} carried={Carried:F2} robust={Robust}";
        }
        /// <summary>One launch without assertions (search, recovery): settles, pulls, flies until still or the level ends.</summary>
        private IEnumerator Shoot(Shot s,Outcome o=null,float flight=4)
        {
            yield return Settle();if(game.Owner.Completed||game.Owner.Lost){if(o!=null)o.Taken=false;yield break;}
            int before=slime.Launches;yield return Pull(s.Angle,s.Power);
            for(int k=0;k<2;k++)Tick();
            if(o!=null)o.Taken=slime.Launches==before+1;
            yield return Ticks(.3f,()=>game.Owner.Completed||game.Owner.Lost);yield return Settle(flight);
            if(game.Owner.Completed)yield return Ticks(.1f);
        }
        private void Observe(Outcome o)
        {
            o.Won=game.Owner.Completed;o.Lost=game.Owner.Lost;o.Failure=game.Failure;o.Body=Body;
            var r=game.Owner.Apparatus.GetComponentInChildren<COgheRailSlider>();if(r!=null)o.Crate=r.Position;
            var f=game.Owner.Apparatus.GetComponentInChildren<COgheSlimeFan>();if(f!=null){o.FanOn=f.Running;o.Carried=f.Carried;}
        }
        private IEnumerator Replay(IList<Shot> prefix,Shot? candidate,Outcome o)
        {
            game.ResetLevel();yield return Ticks(.6f);
            foreach(var s in prefix){var p=new Outcome();yield return Shoot(s,p);if(!p.Taken||game.Owner.Lost||game.Owner.Completed)break;}
            if(candidate.HasValue&&!game.Owner.Lost&&!game.Owner.Completed){o.Shot=candidate.Value;yield return Shoot(candidate.Value,o);}
            Observe(o);
        }
        private static IEnumerable<float> Range(float from,float to,float step){for(float x=from;x<=to+step*.01f;x+=step)yield return x;}
        /// <summary>Grid search of one stage after <paramref name="prefix"/>; returns successes ranked by robustness (successful
        /// grid neighbours) then by power (lower first: gentler). Writes the whole table.</summary>
        private IEnumerator Stage(string name,IList<Shot> prefix,float[] angles,float[] powers,Func<Outcome,bool> success,List<Outcome> ranked,StringBuilder report)
        {
            var grid=new Outcome[angles.Length,powers.Length];
            for(int a=0;a<angles.Length;a++)for(int p=0;p<powers.Length;p++){var o=new Outcome();yield return Replay(prefix,new Shot(angles[a],powers[p]),o);grid[a,p]=o;}
            report.AppendLine($"== {game.Definition.Id} stage {name} after [{string.Join(", ",prefix)}]");
            for(int a=0;a<angles.Length;a++)for(int p=0;p<powers.Length;p++)
            {
                var o=grid[a,p];if(!success(o))continue;int n=0;
                for(int da=-1;da<=1;da++)for(int dp=-1;dp<=1;dp++){int x=a+da,y=p+dp;if((da!=0||dp!=0)&&x>=0&&y>=0&&x<angles.Length&&y<powers.Length&&success(grid[x,y]))n++;}
                o.Robust=n;ranked.Add(o);
            }
            ranked.Sort((x,y)=>x.Robust!=y.Robust?y.Robust.CompareTo(x.Robust):x.Shot.Power.CompareTo(y.Shot.Power));
            for(int a=0;a<angles.Length;a++)for(int p=0;p<powers.Length;p++)report.AppendLine((success(grid[a,p])?"  OK ":"  -- ")+grid[a,p]);
            report.AppendLine($"  successes {ranked.Count}/{angles.Length*powers.Length}; best: {string.Join(" | ",ranked.Take(5))}");
            File.WriteAllText($"{Out}/search-{game.Definition.Id}.txt",report.ToString());
        }
        private static float[] F(params float[] v)=>v;

        [UnityTest,Explicit,Timeout(3600000)] public IEnumerator SlimeSearchS1()
        {
            yield return Load("S1");ticksPerFrame=16;var report=new StringBuilder();
            var direct=new List<Outcome>();
            yield return Stage("one launch",new Shot[0],Range(40,75,5).ToArray(),Range(.70f,1f,.05f).ToArray(),o=>o.Won,direct,report);
            var wall=new List<Outcome>();
            yield return Stage("to the right wall",new Shot[0],F(20,30,40),F(.45f,.55f,.65f),o=>!o.Lost&&o.Body.x>.24f&&o.Body.y>-.20f,wall,report);
            if(wall.Count>0)
            {
                var hop=new List<Outcome>();
                yield return Stage("wall to vent",new[]{wall[0].Shot},Range(85,125,5).ToArray(),Range(.3f,1f,.1f).ToArray(),o=>o.Won,hop,report);
            }
        }
        [UnityTest,Explicit,Timeout(3600000)] public IEnumerator SlimeSearchS2()
        {
            yield return Load("S2");ticksPerFrame=16;var report=new StringBuilder();
            var drop=new List<Outcome>();
            yield return Stage("ledge to the floor left of the crate",new Shot[0],Range(-70,10,10).ToArray(),Range(.05f,.4f,.05f).ToArray(),o=>!o.Lost&&o.Body.y<-.23f&&o.Body.x<-.09f&&o.Body.x>-.18f,drop,report);
            if(drop.Count==0)yield break;
            var ram=new List<Outcome>();var p1=new[]{drop[0].Shot};
            yield return Stage("ram the crate",p1,Range(-5,25,5).ToArray(),Range(.2f,1f,.1f).ToArray(),o=>!o.Lost&&o.Crate>=.054f,ram,report);
            if(ram.Count==0)yield break;
            var strip=new List<Outcome>();var p2=new[]{drop[0].Shot,ram[0].Shot};
            yield return Stage("over the crate, past the curtain",p2,Range(40,90,5).ToArray(),Range(.4f,1f,.1f).ToArray(),o=>o.Won||!o.Lost&&o.Body.x>.17f,strip,report);
            var stripExit=new List<Outcome>();
            if(strip.Count>0&&!strip[0].Won)
            {var exit=stripExit;yield return Stage("strip to the vent",new[]{drop[0].Shot,ram[0].Shot,strip[0].Shot},Range(60,120,10).ToArray(),Range(.2f,.8f,.1f).ToArray(),o=>o.Won,exit,report);}
            if(strip.Count>0)
            {
                // the chosen sequence again from a fresh scene load (the solve tests start there, the grid from Retry)
                var chosen=new List<Shot>{drop[0].Shot,ram[0].Shot,strip[0].Shot};if(!strip[0].Won&&stripExit.Count>0)chosen.Add(stripExit[0].Shot);
                for(int k=0;k<2;k++){yield return Load("S2");ticksPerFrame=16;foreach(var s in chosen){var o=new Outcome();yield return Shoot(s,o);report.AppendLine($"  fresh load #{k}: {s} -> {State()}");if(game.Owner.Lost||game.Owner.Completed)break;}}
                File.WriteAllText($"{Out}/search-{game.Definition.Id}.txt",report.ToString());
            }
            var back=new List<Outcome>();
            yield return Stage("back up onto the ledge",p2,Range(110,160,10).ToArray(),Range(.4f,1f,.15f).ToArray(),o=>!o.Lost&&o.Body.y>-.08f&&o.Body.x<-.17f,back,report);
            if(back.Count>0)
            {var throwOut=new List<Outcome>();yield return Stage("ledge throw to the vent",new[]{drop[0].Shot,ram[0].Shot,back[0].Shot},Range(-10,30,5).ToArray(),Range(.3f,.9f,.1f).ToArray(),o=>o.Won,throwOut,report);}
            var top=new List<Outcome>();
            yield return Stage("from the ledge (where do throws land?)",new Shot[0],Range(-60,40,20).ToArray(),F(.2f,.4f,.6f),o=>!o.Lost,top,report);
        }
        [UnityTest,Explicit,Timeout(3600000)] public IEnumerator SlimeSearchS3()
        {
            yield return Load("S3");ticksPerFrame=16;var report=new StringBuilder();
            var ride=new List<Outcome>();
            yield return Stage("A: into the updraft",new Shot[0],Range(50,100,5).ToArray(),Range(.5f,1f,.1f).ToArray(),o=>o.Won||!o.Lost&&o.Body.x>.08f,ride,report);
            if(ride.Count>0&&!ride[0].Won)
            {var vent=new List<Outcome>();yield return Stage("A: right chamber to the vent",new[]{ride[0].Shot},Range(-110,-20,10).ToArray(),Range(.05f,.65f,.15f).ToArray(),o=>o.Won,vent,report);}
            var ram=new List<Outcome>();
            yield return Stage("B: ram the crate",new Shot[0],Range(170,190,5).ToArray(),Range(.2f,1f,.1f).ToArray(),o=>!o.Lost&&o.Crate>=.044f&&o.Body.y<-.2f,ram,report);
            if(ram.Count==0)yield break;
            var duct=new List<Outcome>();
            yield return Stage("B: through the duct",new[]{ram[0].Shot},Range(-10,20,5).ToArray(),Range(.2f,1f,.1f).ToArray(),o=>o.Won||!o.Lost&&o.Body.x>.08f,duct,report);
            if(duct.Count>0&&!duct[0].Won)
            {var vent=new List<Outcome>();yield return Stage("B: right chamber to the vent",new[]{ram[0].Shot,duct[0].Shot},Range(-30,60,15).ToArray(),Range(.2f,.8f,.15f).ToArray(),o=>o.Won,vent,report);}
            var blocked=new List<Outcome>();
            yield return Stage("fan on: the duct is shut",new Shot[0],Range(-10,20,5).ToArray(),Range(.2f,1f,.2f).ToArray(),o=>!o.Lost&&o.Body.x>.08f&&o.Carried<.05f,blocked,report);
        }
    }
}

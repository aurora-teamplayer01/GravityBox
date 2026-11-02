using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheSpatialCampaignTests
    {
        private const BindingFlags SkinPrivate=BindingFlags.Instance|BindingFlags.NonPublic;
        private static T SkinField<T>(VenomSurface skin,string name)=>(T)typeof(VenomSurface).GetField(name,SkinPrivate).GetValue(skin);

        // Uses one already captured presentation state, without advancing life
        // animation twice. The oracle accumulates/polygonizes the original full
        // grid; production reuses its buffers from all preceding comparisons.
        private static void AssertDenseSkin(VenomSurface skin,int count,string label)
        {
            SkinField<List<Vector3>>(skin,"vertices").Clear();SkinField<List<Vector3>>(skin,"normals").Clear();
            SkinField<List<int>>(skin,"triangles").Clear();SkinField<List<Vector4>>(skin,"vertexInks").Clear();
            var stamps=SkinField<int[]>(skin,"edgeVersions");Array.Clear(stamps,0,stamps.Length);
            typeof(VenomSurface).GetField("cellVersion",SkinPrivate).SetValue(skin,0);
            typeof(VenomSurface).GetMethod("BuildFragment",SkinPrivate).Invoke(skin,new object[]{count});
            var reference=new COgheSkinDenseReference(skin);reference.Build(count);
            CollectionAssert.AreEqual(reference.Vertices,SkinField<List<Vector3>>(skin,"vertices"),label+" vertex order/positions");
            CollectionAssert.AreEqual(reference.Normals,SkinField<List<Vector3>>(skin,"normals"),label+" normals");
            CollectionAssert.AreEqual(reference.Triangles,SkinField<List<int>>(skin,"triangles"),label+" indices");
            CollectionAssert.AreEqual(reference.Inks,SkinField<List<Vector4>>(skin,"vertexInks"),label+" ink interpolation");
            int n=reference.ValidFieldLength;
            CollectionAssert.AreEqual(new ArraySegment<float>(reference.Field,0,n),new ArraySegment<float>(SkinField<float[]>(skin,"field"),0,n),label+" field");
            CollectionAssert.AreEqual(new ArraySegment<Vector3>(reference.Gradient,0,n),new ArraySegment<Vector3>(SkinField<Vector3[]>(skin,"gradient"),0,n),label+" gradients");
            if(SkinField<bool>(skin,"inking"))CollectionAssert.AreEqual(new ArraySegment<Vector4>(reference.InkField,0,n),new ArraySegment<Vector4>(SkinField<Vector4[]>(skin,"inkField"),0,n),label+" ink field");
        }

        [UnityTest] public IEnumerator SparseSkinExactlyMatchesDenseFieldAndMeshAcrossBufferReuse()
        {
            var random=new System.Random(291026);
            foreach(string scene in new[]{"COgheSpatial01","COgheSpatialPlusN15","COgheSpatialPlusN43"})
            {
                yield return LoadScene(scene);
                var skin=game.Matter.GetComponent<VenomSurface>();skin.Rebuild(false);
                var before=BodySnapshot();
                AssertDenseSkin(skin,SkinField<int>(skin,"sourceCount"),scene+" live body");
                var points=SkinField<Vector3[]>(skin,"points");var supports=SkinField<float[]>(skin,"supports");var weights=SkinField<float[]>(skin,"weights");
                var inks=SkinField<Vector4[]>(skin,"sourceInks");var centre=points[0];
                // Alternate dense, stretched, separated, empty and tiny source
                // sets so stale row ranges, gradients and ink channels are exposed.
                for(int trial=0;trial<15;trial++)
                {
                    int count=new[]{32,1,48,4,16}[trial%5];
                    bool inking=trial%3!=0;typeof(VenomSurface).GetField("inking",SkinPrivate).SetValue(skin,inking);
                    for(int i=0;i<count;i++)
                    {
                        float Next()=>(float)random.NextDouble();
                        float extent=trial%4==0?.10f:trial%4==1?.008f:.04f;
                        points[i]=centre+new Vector3((Next()-.5f)*extent,(Next()-.5f)*extent,(Next()-.5f)*extent);
                        supports[i]=.006f+Next()*.025f;
                        weights[i]=trial==7?0:(count>1&&i%7==0?0:.3f+Next()*1.2f);
                        inks[i]=new Vector4(Next(),Next(),Next(),Next())*.2f;
                    }
                    AssertDenseSkin(skin,count,scene+" sources "+trial);
                }
                CollectionAssert.AreEqual(before,BodySnapshot(),"Construction must not move physical particles");
                skin.Rebuild(false);
            }
        }

        [UnityTest] public IEnumerator DirectedSkinCacheExactlyMatchesDenseAcrossPlaneWrapAndGenerationReset()
        {
            yield return LoadScene("COgheSpatialPlusN15");
            var skin=game.Matter.GetComponent<VenomSurface>();skin.Rebuild(false);
            Assert.LessOrEqual(SkinField<VenomLifeAnimation>(skin,"life").Monster.Body,.001f);
            var before=BodySnapshot();
            var points=SkinField<Vector3[]>(skin,"points");var supports=SkinField<float[]>(skin,"supports");
            var weights=SkinField<float[]>(skin,"weights");var inks=SkinField<Vector4[]>(skin,"sourceInks");
            var centre=points[0];
            var sizes=new[]{new Vector3(.046f,.031f,.12f),new Vector3(.51f,.51f,.51f),new Vector3(.009f,.012f,.006f),
                new Vector3(.031f,.055f,.09f),new Vector3(.51f,.51f,.51f),new Vector3(.02f,.03f,.045f)};
            for(int trial=0;trial<sizes.Length;trial++)
            {
                // Separated z islands wrap the rolling cache repeatedly. Large
                // x/y extents reach the grid cap; then small fragments reuse it.
                // Reversing source weights changes edge directions/rounding.
                typeof(VenomSurface).GetField("inking",SkinPrivate).SetValue(skin,trial%2==1);
                if(trial==4)typeof(VenomSurface).GetField("ordinaryStamp",SkinPrivate).SetValue(skin,int.MaxValue-1);
                for(int i=0;i<8;i++)
                {
                    var signs=new Vector3((i&1)==0?-.5f:.5f,(i&2)==0?-.5f:.5f,(i&4)==0?-.5f:.5f);
                    points[i]=centre+Vector3.Scale(signs,sizes[trial]);supports[i]=.014f+i*.001f;
                    weights[i]=.7f+(trial%2==0?i:7-i)*.13f;
                    inks[i]=new Vector4((i+1)*.08f,(8-i)*.06f,trial*.03f,.1f);
                }
                AssertDenseSkin(skin,8,"Directed cache plane/generation reuse "+trial);
                Assert.Greater(SkinField<List<int>>(skin,"triangles").Count,0,"Reference comparison must include a surface");
            }
            CollectionAssert.AreEqual(before,BodySnapshot(),"Cached construction must not move physical particles");
            skin.Rebuild(false);
        }

        [UnityTest] public IEnumerator SparseSkinExactlyMatchesDenseMonsterAndInjectedInk()
        {
            yield return Load(1);yield return Frames(60);
            var skin=game.Matter.GetComponent<VenomSurface>();
            var previousInk=skin.ParticleInk;
            skin.ParticleInk=new Vector4[32];for(int i=0;i<32;i++)skin.ParticleInk[i]=new Vector4(i/31f,1-i/31f,0,0);
            try
            {
                game.Personality.Force(COgheAct.Monster);
                foreach(int frames in new[]{30,72,72,75})
                {
                    yield return Frames(frames);skin.Rebuild(false);
                    AssertDenseSkin(skin,SkinField<int>(skin,"sourceCount"),"Monster phase "+frames);
                    skin.Rebuild(false);
                }
            }
            finally{skin.ParticleInk=previousInk;}
        }
    }
}

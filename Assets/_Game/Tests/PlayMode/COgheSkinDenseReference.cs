using System;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Tests
{
    // Frozen dense field + polygonizer from VenomSurface immediately before the
    // 29/10/2026 occupied-row optimization. Do not make this traversal sparse:
    // it is the independent reference for numerical and vertex-order equivalence.
    // The unchanged contact projection is delegated to the live surface, whose
    // frozen source inputs/frame snapshot are identical during a comparison.
    internal sealed class COgheSkinDenseReference
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private delegate void Constraint(ref Vector3 point,ref Vector3 normal);
        private readonly Constraint constrain;
        private readonly Action<Bounds> prepareFragment;
        private readonly CohesiveOrganism organism;
        private readonly VenomLifeAnimation life;
        private readonly VenomFloorBoundary floor;
        private readonly Matrix4x4 toFloor;
        private readonly Vector3[] points;
        private readonly float[] supports,weights;
        private readonly Vector4[] sourceInks;
        private readonly bool inking,reuseMonsterEdges;
        private bool monsterEdges,allAboveFloor;
        private int sourceCount,cellVersion,monsterStamp;
        private readonly Vector3[] floorPoints=new Vector3[VenomSurface.MaxSkinSources];
        private readonly Vector3[] corners=new Vector3[8],cornerNormals=new Vector3[8];
        private readonly float[] values=new float[8];
        private readonly Vector3[] edgePoints=new Vector3[64],edgeNormals=new Vector3[64];
        private readonly int[] edgeVersions=new int[64],cornerGrid=new int[8],inside=new int[4],outside=new int[4];
        private int[] monsterEdgeStamp=Array.Empty<int>();
        private Vector3[] monsterEdgePoint=Array.Empty<Vector3>(),monsterEdgeNormal=Array.Empty<Vector3>();
        private Vector4[] monsterEdgeInk=Array.Empty<Vector4>();
        private float[] field=Array.Empty<float>();
        private Vector3[] gradient=Array.Empty<Vector3>();
        private Vector4[] inkField=Array.Empty<Vector4>();
        private readonly Vector4[] cornerInks=new Vector4[8],edgeInks=new Vector4[64];
        private readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        private readonly List<int> triangles=new List<int>();
        private readonly List<Vector4> vertexInks=new List<Vector4>();
        private readonly int[,] tetrahedra={{0,5,1,6},{0,1,2,6},{0,2,3,6},{0,3,7,6},{0,7,4,6},{0,4,5,6}};
        private readonly Vector3Int[] offsets={new Vector3Int(0,0,0),new Vector3Int(1,0,0),new Vector3Int(1,1,0),new Vector3Int(0,1,0),new Vector3Int(0,0,1),new Vector3Int(1,0,1),new Vector3Int(1,1,1),new Vector3Int(0,1,1)};
        public IReadOnlyList<Vector3> Vertices=>vertices;
        public IReadOnlyList<Vector3> Normals=>normals;
        public IReadOnlyList<int> Triangles=>triangles;
        public IReadOnlyList<Vector4> Inks=>vertexInks;
        public float[] Field=>field;
        public Vector3[] Gradient=>gradient;
        public Vector4[] InkField=>inkField;
        public int ValidFieldLength{get;private set;}
        public COgheSkinDenseReference(VenomSurface source)
        {
            T Read<T>(string name)=>(T)typeof(VenomSurface).GetField(name,Private).GetValue(source);
            organism=Read<CohesiveOrganism>("organism");life=Read<VenomLifeAnimation>("life");floor=Read<VenomFloorBoundary>("floor");
            toFloor=Read<Matrix4x4>("toFloor");inking=Read<bool>("inking");reuseMonsterEdges=Read<bool>("reuseMonsterEdges");
            points=(Vector3[])Read<Vector3[]>("points").Clone();supports=(float[])Read<float[]>("supports").Clone();weights=(float[])Read<float[]>("weights").Clone();
            sourceInks=(Vector4[])Read<Vector4[]>("sourceInks").Clone();
            constrain=(Constraint)Delegate.CreateDelegate(typeof(Constraint),source,typeof(VenomSurface).GetMethod("ConstrainFloor",Private));
            var campaign=Read<VenomLevelController>("level").Campaign;
            if(campaign!=null)prepareFragment=(Action<Bounds>)Delegate.CreateDelegate(typeof(Action<Bounds>),campaign,typeof(VenomCampaign).GetMethod("PrepareSkinFragment",Private));
        }
        private void ConstrainFloor(ref Vector3 point,ref Vector3 normal)=>constrain(ref point,ref normal);
        public void Build(int count)
        {
            sourceCount = count; allAboveFloor = true;
            for (int i=0;i<count;i++)
            {
                floorPoints[i] = toFloor.MultiplyPoint3x4(points[i]);
                allAboveFloor &= floorPoints[i].y >= floor.Top-.0005f;
            }
            float threshold=organism.Profile.SkinThreshold;
            Vector3 min=points[0]-Vector3.one*supports[0], max=points[0]+Vector3.one*supports[0];
            for(int i=1;i<count;i++){min=Vector3.Min(min,points[i]-Vector3.one*supports[i]);max=Vector3.Max(max,points[i]+Vector3.one*supports[i]);}
            Vector3 size=max-min; float cell=Mathf.Max(organism.Profile.MeshCell,Mathf.Max(size.x,size.y,size.z)/64);
            prepareFragment?.Invoke(new Bounds((min+max)*.5f,size+Vector3.one*cell*2));
            int nx=Mathf.CeilToInt(size.x/cell)+1, ny=Mathf.CeilToInt(size.y/cell)+1,nz=Mathf.CeilToInt(size.z/cell)+1;
            int length=nx*ny*nz; ValidFieldLength=length;
            monsterEdges = reuseMonsterEdges && life.Monster.Body > .001f;
            if (monsterEdges)
            {
                int required=length*7; // the seven positive grid-edge directions used by these tetrahedra
                if(monsterEdgeStamp.Length<required)
                {
                    int capacity=Mathf.NextPowerOfTwo(required);
                    monsterEdgeStamp=new int[capacity];monsterEdgePoint=new Vector3[capacity];monsterEdgeNormal=new Vector3[capacity];
                    monsterEdgeInk=Array.Empty<Vector4>();
                }
                if(inking&&monsterEdgeInk.Length<monsterEdgeStamp.Length)monsterEdgeInk=new Vector4[monsterEdgeStamp.Length];
                if(++monsterStamp==int.MaxValue){Array.Clear(monsterEdgeStamp,0,monsterEdgeStamp.Length);monsterStamp=1;}
            }
            if(field.Length<length){int capacity=Mathf.NextPowerOfTwo(length);field=new float[capacity];gradient=new Vector3[capacity];}
            Array.Clear(field,0,length);Array.Clear(gradient,0,length);
            if(inking){if(inkField.Length<field.Length)inkField=new Vector4[field.Length];Array.Clear(inkField,0,length);}
            for(int p=0;p<count;p++)
            {
                float support=supports[p],h2=support*support,weight=weights[p];
                Vector3 q=(points[p]-min)/cell;
                int reach=Mathf.CeilToInt(support/cell);
                for(int z=Mathf.Max(0,(int)q.z-reach);z<=Mathf.Min(nz-1,(int)q.z+reach);z++)
                for(int y=Mathf.Max(0,(int)q.y-reach);y<=Mathf.Min(ny-1,(int)q.y+reach);y++)
                for(int x=Mathf.Max(0,(int)q.x-reach);x<=Mathf.Min(nx-1,(int)q.x+reach);x++)
                {
                    Vector3 d=min+new Vector3(x,y,z)*cell-points[p];float s=1-d.sqrMagnitude/h2;if(s<=0)continue;
                    int index=x+nx*(y+ny*z);field[index]+=s*s*s*weight;gradient[index]+=d*(6*s*s*weight/h2);
                    if(inking)inkField[index]+=sourceInks[p]*(s*s*s*weight);
                }
            }
            for(int z=0;z<nz-1;z++)for(int y=0;y<ny-1;y++)for(int x=0;x<nx-1;x++)
            {
                int above=0;
                for(int c=0;c<8;c++)
                {
                    Vector3Int o=offsets[c];int index=x+o.x+nx*(y+o.y+ny*(z+o.z));
                    values[c]=field[index];if(values[c]>=threshold)above++;
                }
                if(above==0||above==8)continue;
                cellVersion++;
                // Empty/full cells need no corner positions or normals. Keep
                // the same field, tetrahedra and mesh density at the boundary.
                for(int c=0;c<8;c++)
                {
                    Vector3Int o=offsets[c];int index=x+o.x+nx*(y+o.y+ny*(z+o.z));
                    cornerGrid[c]=index;
                    corners[c]=min+new Vector3(x+o.x,y+o.y,z+o.z)*cell;cornerNormals[c]=gradient[index];
                    if(inking)cornerInks[c]=inkField[index];
                }
                for(int t=0;t<6;t++)
                {
                    int ni=0,no=0;
                    for(int c=0;c<4;c++){int id=tetrahedra[t,c];if(values[id]>=threshold)inside[ni++]=id;else outside[no++]=id;}
                    if(ni==0||ni==4)continue;
                    // edge keys (from*8+to) also index each crossing's cached ink
                    if(ni==1){Edge(inside[0],outside[0],out var a,out var an);Edge(inside[0],outside[1],out var b,out var bn);Edge(inside[0],outside[2],out var c,out var cn);Triangle(a,b,c,an,bn,cn,inside[0]*8+outside[0],inside[0]*8+outside[1],inside[0]*8+outside[2]);}
                    else if(ni==3){Edge(outside[0],inside[0],out var a,out var an);Edge(outside[0],inside[1],out var b,out var bn);Edge(outside[0],inside[2],out var c,out var cn);Triangle(a,b,c,an,bn,cn,outside[0]*8+inside[0],outside[0]*8+inside[1],outside[0]*8+inside[2]);}
                    else
                    {
                        Edge(inside[0],outside[0],out var a,out var an);Edge(inside[0],outside[1],out var b,out var bn);
                        Edge(inside[1],outside[0],out var c,out var cn);Edge(inside[1],outside[1],out var d,out var dn);
                        int ka=inside[0]*8+outside[0],kb=inside[0]*8+outside[1],kc=inside[1]*8+outside[0],kd=inside[1]*8+outside[1];
                        Triangle(a,b,c,an,bn,cn,ka,kb,kc);Triangle(b,d,c,bn,dn,cn,kb,kd,kc);
                    }
                }
            }
        }
        private void Edge(int a,int b,out Vector3 p,out Vector3 n)
        {
            // Adjacent tetrahedra share crossings. Compute and constrain each
            // directed edge once per cell, preserving the original arithmetic
            // order and emitted triangle/vertex order exactly.
            int key=a*8+b;
            if(edgeVersions[key]==cellVersion){p=edgePoints[key];n=edgeNormals[key];return;}
            int shared=-1;
            if(monsterEdges)
            {
                Vector3Int d=offsets[b]-offsets[a];
                int direction=Mathf.Abs(d.x)+Mathf.Abs(d.y)*2+Mathf.Abs(d.z)*4-1;
                shared=Mathf.Min(cornerGrid[a],cornerGrid[b])*7+direction;
                if(monsterEdgeStamp[shared]==monsterStamp)
                {
                    p=monsterEdgePoint[shared];n=monsterEdgeNormal[shared];
                    edgeVersions[key]=cellVersion;edgePoints[key]=p;edgeNormals[key]=n;
                    if(inking)edgeInks[key]=monsterEdgeInk[shared];
                    return;
                }
            }
            float t=(organism.Profile.SkinThreshold-values[a])/(values[b]-values[a]);p=Vector3.Lerp(corners[a],corners[b],t);n=Vector3.Lerp(cornerNormals[a],cornerNormals[b],t).normalized;
            if (life.Monster.Body > .001f)
            {
                // Correct interpolation error on the portrait head against the smooth implicit field.
                // One bounded Newton step and an analytic normal smooth the portrait without increasing mesh density.
                Vector3 edge = corners[b] - corners[a];
                for (int iteration = 0; iteration < 2; iteration++)
                {
                    float value = 0; Vector3 gradient = Vector3.zero;
                    for (int i = 0; i < sourceCount; i++)
                    {
                        if (weights[i] < .00001f) continue;
                        Vector3 d = p - points[i]; float h2 = supports[i] * supports[i], q = 1 - d.sqrMagnitude / h2;
                        if (q <= 0) continue;
                        value += q * q * q * weights[i]; gradient += d * (6 * q * q * weights[i] / h2);
                    }
                    if (gradient.sqrMagnitude > 1e-12f) n = gradient.normalized;
                    if (iteration == 1) break;
                    float slope = Vector3.Dot(gradient, edge);
                    if (Mathf.Abs(slope) < 1e-6f) break;
                    t = Mathf.Clamp01(t + (value - organism.Profile.SkinThreshold) / slope);
                    p = Vector3.Lerp(corners[a], corners[b], t);
                }
            }
            ConstrainFloor(ref p,ref n);
            edgeVersions[key]=cellVersion;edgePoints[key]=p;edgeNormals[key]=n;
            if(inking)edgeInks[key]=Vector4.Lerp(cornerInks[a],cornerInks[b],t)/Mathf.Max(1e-6f,Mathf.Lerp(values[a],values[b],t));
            if(shared>=0)
            {
                monsterEdgeStamp[shared]=monsterStamp;monsterEdgePoint[shared]=p;monsterEdgeNormal[shared]=n;
                if(inking)monsterEdgeInk[shared]=edgeInks[key];
            }
            if (life.Monster.Body > .001f)
            {
                int reverse=b*8+a;edgeVersions[reverse]=cellVersion;edgePoints[reverse]=p;edgeNormals[reverse]=n;
                if(inking)edgeInks[reverse]=edgeInks[key];
            }
        }
        private void Triangle(Vector3 a,Vector3 b,Vector3 c,Vector3 an,Vector3 bn,Vector3 cn,int ka,int kb,int kc)
        {
            if (Vector3.Cross(b-a,c-a).sqrMagnitude < 1e-18f) return;
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),an+bn+cn)<0){(b,c)=(c,b);(bn,cn)=(cn,bn);(kb,kc)=(kc,kb);}
            int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);normals.Add(an);normals.Add(bn);normals.Add(cn);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);
            if(inking){vertexInks.Add(edgeInks[ka]);vertexInks.Add(edgeInks[kb]);vertexInks.Add(edgeInks[kc]);}
        }
    }
}

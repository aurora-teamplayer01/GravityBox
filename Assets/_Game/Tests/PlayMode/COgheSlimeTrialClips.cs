using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    /// <summary>Clips for the owner (Explicit). Automated replay through the same pointer path as the tests: camera renders of
    /// a 720×1280 target, one frame every 4 physics ticks (1/120 s), i.e. real game speed at 30 fps of simulated time. The
    /// game is laid out under a 120 px strip (COgheSlimeControl.ReservedTop) that ffmpeg fills with captions; captions.tsv
    /// gives (start, end, text) per clip. Not a recording of a human player.</summary>
    public sealed partial class COgheSlimeTrialTests
    {
        private string clipDir;private int clipFrame;private readonly List<(int start,string text)> clipCaptions=new List<(int,string)>();
        private IEnumerator StartClip(string key,string name)
        {
            COgheSlimeControl.ReservedTop=120;
            yield return Load(key);ticksPerFrame=4;
            clipDir=$"{Out}/clips/{name}";if(Directory.Exists(clipDir))Directory.Delete(clipDir,true);Directory.CreateDirectory(clipDir);
            clipFrame=0;clipCaptions.Clear();int tick=0;
            perTick=()=>{if(++tick%4==0)Frame($"{clipDir}/f{clipFrame++:0000}.png");};
        }
        private void Caption(string text)=>clipCaptions.Add((clipFrame,text));
        private void EndClip(string header)
        {
            perTick=null;var sb=new StringBuilder();sb.AppendLine("# "+header);
            for(int i=0;i<clipCaptions.Count;i++){int end=i+1<clipCaptions.Count?clipCaptions[i+1].start:clipFrame;sb.AppendLine($"{clipCaptions[i].start/30f:F2}\t{end/30f:F2}\t{clipCaptions[i].text}");}
            File.WriteAllText(clipDir+"/captions.tsv",sb.ToString());
            File.WriteAllText(clipDir+"/launches.txt",$"frames {clipFrame} at 30 fps (sim dt 1/120, 4 ticks per frame)\n"+string.Join("\n",slime.History));
            COgheSlimeControl.ReservedTop=0;ticksPerFrame=2;
        }
        /// <summary>A visible launch: rest, then a 0.5 s drag (press in the room's middle), release, flight.</summary>
        private IEnumerator ClipLaunch(Vector2 shot,float after=1.2f)
        {
            yield return Settle();yield return Ticks(.35f);yield return Pull(shot.x,shot.y,60);
            yield return Ticks(after,()=>game.Owner.Lost);yield return Settle(3);
        }
        private IEnumerator ClipRetry(){yield return Ticks(.4f);var l=COgheSlimeControl.Layout(new Rect(0,0,width,height));Assert.IsTrue(slime.PressHud(l.Retry.center));yield return Ticks(.8f);}

        [UnityTest,Explicit,Timeout(1800000)] public IEnumerator SlimeClipS1()
        {
            yield return StartClip("S1","S1");
            Caption("Gợi ý lần đầu: ngón tay ma chỉ cách kéo");yield return Ticks(2.2f);
            Caption("Lần 1 (cố tình hụt): kéo vừa → slime dính kính, thấp hơn lỗ");yield return ClipLaunch(S1MissThenFix[0]);
            Caption("Lần 2 (sửa): kéo thẳng xuống → bật thẳng lên lỗ thông gió");yield return ClipLaunch(S1MissThenFix[1]);
            yield return Ticks(1.6f);Assert.IsTrue(game.Owner.Completed,"S1 clip ends with the win");
            Caption("Thoát. 2 cú bật.");yield return Ticks(1.2f);
            EndClip("S1 · Học kéo-thả: hụt rồi sửa được không?");
        }
        [UnityTest,Explicit,Timeout(1800000)] public IEnumerator SlimeClipS2()
        {
            yield return StartClip("S2","S2");
            Caption("Laser bật. Thử ném thẳng từ bệ sang phải...");yield return Ticks(1.2f);
            yield return ClipLaunch(new Vector2(0,.6f),.8f);Assert.IsTrue(game.Owner.Lost,"the clip's first throw hits the laser");
            Caption("Chạm laser = thua, bấm THỬ LẠI");yield return Ticks(1.4f);yield return ClipRetry();
            Caption("Lần này: xuống sàn trước, cạnh thùng");yield return ClipLaunch(S2Solution[0]);
            Caption("Bật thấp, ngang vào hông thùng → đẩy thùng lên nút");yield return ClipLaunch(S2Solution[1]);
            Caption("Thùng đè nút → laser tắt");yield return Ticks(.8f);
            for(int i=2;i<S2Solution.Length;i++){Caption(i==S2Solution.Length-1?"Bật vào lỗ thoát":"Đổi chỗ để có góc bật");yield return ClipLaunch(S2Solution[i]);}
            yield return Ticks(1.4f);Assert.IsTrue(game.Owner.Completed,"S2 clip ends with the win");
            Caption($"Thoát. {slime.Launches} cú bật sau khi thử lại.");yield return Ticks(1.2f);
            EndClip("S2 · Dùng cú bật lên môi trường: đánh vào đâu?");
        }
        [UnityTest,Explicit,Timeout(1800000)] public IEnumerator SlimeClipS3()
        {
            yield return StartClip("S3","S3");
            Caption("Cách A: để gió nâng qua vách");yield return Ticks(1.2f);
            yield return ClipLaunch(S3WayA[0],1.8f);
            Caption("Gió chở qua đỉnh vách, dính kính bên phải");yield return Ticks(.6f);
            for(int i=1;i<S3WayA.Length;i++){Caption("Cú cuối khó: nhảy xuống đúng lỗ");yield return ClipLaunch(S3WayA[i]);}
            yield return Ticks(1.4f);Assert.IsTrue(game.Owner.Completed,"way A wins in the clip");
            Caption($"A xong: {slime.Launches} cú bật. THỬ LẠI để thử cách B");yield return Ticks(1.0f);yield return ClipRetry();
            Caption("Cách B: húc thùng lên nút → quạt tắt, cửa chớp mở");yield return ClipLaunch(S3WayB[0]);yield return Ticks(.8f);
            Caption("Bật thấp qua ống dưới vách");yield return ClipLaunch(S3WayB[1]);
            for(int i=2;i<S3WayB.Length;i++){Caption("Vào lỗ thoát");yield return ClipLaunch(S3WayB[i]);}
            yield return Ticks(1.4f);Assert.IsTrue(game.Owner.Completed,"way B wins in the clip");
            Caption($"B xong: {slime.Launches} cú bật.");yield return Ticks(1.2f);
            EndClip("S3 · Hai cách thoát khác nhau: cưỡi gió / tắt quạt");
        }
    }
}

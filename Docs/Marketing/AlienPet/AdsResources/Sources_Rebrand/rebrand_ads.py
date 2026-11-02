"""Rebrand historical finished ads without regenerating or changing gameplay.
Native edits replace the baked top caption band and full end card. The cinematic
edit uses the preserved picture-lock (no old title), with its original final mix.
"""
from pathlib import Path
import subprocess as sp, json, math
import numpy as np
from PIL import Image,ImageDraw,ImageFont,ImageFilter

R=Path(__file__).resolve().parent;ROOT=R.parents[1]
W=R/'work';W.mkdir(exist_ok=True);OUT=R/'final';OUT.mkdir(exist_ok=True)
FF='/opt/homebrew/bin/ffmpeg';FPS=30;SIZE=(1080,1920)
FONT=ROOT/'Assets/_Game/Venom/Resources/COgheUI'
INK=(39,67,74);TEAL=(47,113,114);PAPER=(250,250,239);MINT=(175,228,208)
N=ROOT/'Artifacts/COgheMarketingReview'
C=ROOT/'Artifacts/COgheCinematicAd-2026-10-22'
PLANS=[
 dict(id='D',name='PUZZLE',duration=15,end=12.7,source=str(N/'COGHE_AD1_PUZZLE_15S_9X16_1080X1920.mp4'),kind='native',
      captions=[(0,'CAN YOU GUIDE IT OUT?'),(2.5,'TAP TO GUIDE IT'),(5.3,'SPLIT. TEAM UP.'),(7.7,'CLEVER PHYSICS PUZZLES'),(11,'A LITTLE VICTORY!')],tag='A tiny friend. A big escape.'),
 dict(id='E',name='PET_PLAY',duration=17,end=14.7,source=str(N/'COGHE_AD2_PET_17S_9X16_1080X1920.mp4'),kind='native',
      captions=[(0,'MEET YOUR ALIEN PET'),(1.5,'A LITTLE LOVE…'),(4,'…A LOT OF FEELINGS.'),(5.5,'EASY ON THE HUGS!'),(7.5,'TIME TO PLAY'),(9.5,'SNACK TIME!'),(11.5,'MAKE IT YOURS')],tag='A little pet. A lot of personality.'),
 dict(id='F',name='GAME_SHOWCASE',duration=30,end=27,source=str(N/'COGHE_AD3_SHOWCASE_30S_9X16_1080X1920.mp4'),kind='native',
      captions=[(0,'MEET YOUR ALIEN PET'),(2.5,'TAP TO GUIDE IT'),(5.3,'SWING ACROSS'),(7.5,'SPLIT. TEAM UP.'),(9.5,'TURN THE GEARS'),(11.5,'MOVE. SOLVE. ESCAPE.'),(13.4,'CELEBRATE TOGETHER'),(15,'A LITTLE LOVE'),(17.5,'SNACK TIME!'),(19.5,'A SPLASH OF COLOR'),(23,'MAKE IT YOURS')],tag='Solve puzzles. Care for your pet.'),
 dict(id='G',name='CINEMATIC_ESCAPE',duration=30,end=24.5,source=str(C/'picture-lock.mp4'),audio=str(C/'final_mix_30s.wav'),kind='cinematic',captions=[],tag='Can you guide it home?'),
 dict(id='H',name='GAMEPLAY_TOUR',duration=30,end=28.3,source='/Users/mrk/Downloads/COghe.mp4',kind='native',
      captions=[(0,'MEET YOUR ALIEN PET'),(4,'PULL. SOLVE. ESCAPE.'),(8,'SPLIT. TEAM UP.'),(11.5,'CLEVER PHYSICS PUZZLES'),(17.5,'MAKE YOURSELF AT HOME'),(19.5,'A LITTLE LOVE'),(21.5,'GET TO KNOW YOUR PET'),(25,'MAKE IT YOURS')],tag='Your little alien friend.'),
]

cache={}
def txt(im,s,fs,y,color=INK,alpha=1,bold=True):
 key=(s,fs,color,bold)
 if key not in cache:
  f=ImageFont.truetype(str(FONT/('ManropeBold.ttf' if bold else 'Manrope.ttf')),fs);b=f.getbbox(s)
  z=Image.new('RGBA',(math.ceil(f.getlength(s))+12,fs+30));ImageDraw.Draw(z).text((6,-b[1]),s,font=f,fill=(*color,255));cache[key]=z
 z=cache[key]
 if alpha<1:z=z.copy();z.putalpha(z.getchannel('A').point(lambda v:int(v*max(0,alpha))))
 im.paste(z,((1080-z.width)//2,int(y)),z)

icon=Image.open(ROOT/'Assets/_Game/Branding/COghe-AppIcon.png').convert('RGB').resize((460,460),Image.Resampling.LANCZOS)
mask=Image.new('L',(460,460));ImageDraw.Draw(mask).rounded_rectangle((0,0,459,459),radius=95,fill=255)
def smooth(x):return 1-(1-max(0,min(1,x)))**3

def endcard(plan,t):
 im=Image.new('RGB',SIZE,PAPER);d=ImageDraw.Draw(im);a=smooth(t/.22)
 txt(im,'A TINY FRIEND. A BIG ESCAPE.',30,230,TEAL,bold=False)
 shadow=Image.new('RGBA',SIZE);sd=ImageDraw.Draw(shadow);sd.rounded_rectangle((310,380,770,840),radius=95,fill=(32,68,63,45));shadow=shadow.filter(ImageFilter.GaussianBlur(19));im.paste(shadow,(0,0),shadow)
 im.paste(icon,(310,355),mask)
 txt(im,'Alien Pet',125,900+(1-a)*20,INK,a)
 txt(im,'Escape Puzzle',52,1055,TEAL,a,bold=False)
 txt(im,plan['tag'],35,1170,INK,a,bold=False)
 width=int(620*(1+.01*math.sin(t*math.pi*2)));x=(1080-width)//2
 d.rounded_rectangle((x,1370,x+width,1500),radius=58,fill=TEAL);txt(im,'PLAY NOW',44,1412,(255,255,245),a)
 txt(im,'ALIEN PET: ESCAPE PUZZLE',22,1640,TEAL,bold=False)
 return im

def native(raw,p,t):
 if t>=p['end']:return endcard(p,t-p['end']).tobytes()
 im=Image.frombytes('RGB',SIZE,raw)
 # All historical baked titles/level counts occupy this caption band. Replace
 # it completely, rather than leaving old letters visible under an alpha fade.
 d=ImageDraw.Draw(im);d.rectangle((0,0,1079,425),fill=PAPER)
 arr=np.zeros((55,1080,4),dtype=np.uint8);arr[:,:,:3]=PAPER;arr[:,:,3]=np.linspace(255,0,55)[:,None].astype(np.uint8)
 gradient=Image.fromarray(arr);im.paste(gradient,(0,426),gradient)
 txt(im,'ALIEN PET  /  ESCAPE PUZZLE',27,150,TEAL,bold=False)
 line='';at=0
 for start,label in p['captions']:
  if t>=start:line=label;at=start
 fs=70 if len(line)<=21 else 59
 txt(im,line,fs,258+15*(1-smooth((t-at)/.18)))
 d.rounded_rectangle((435,369,645,374),2,fill=MINT)
 badge=Image.new('RGBA',(350,58));ImageDraw.Draw(badge).rounded_rectangle((0,0,349,57),radius=29,fill=(250,250,239,225));im.paste(badge,(365,1665),badge)
 txt(im,'ACTUAL GAMEPLAY',22,1683,TEAL,bold=False)
 return im.tobytes()

def cinematic(raw,p,t):
 im=Image.frombytes('RGB',SIZE,raw)
 tag=Image.new('RGBA',(350,57));ImageDraw.Draw(tag).rounded_rectangle((0,0,349,56),radius=28,fill=(12,34,39,175));im.paste(tag,(365,160),tag);txt(im,'AI CINEMATIC',22,177,(243,249,239),bold=False)
 if t>=p['end']:
  a=smooth((t-p['end'])/.45);ar=np.zeros((760,1080,4),dtype=np.uint8);ar[:,:,:3]=(8,26,31);ar[:,:,3]=(np.minimum(1,np.linspace(0,1.8,760))*225*a)[:,None].astype(np.uint8)
  z=Image.fromarray(ar);im.paste(z,(0,1160),z)
  txt(im,'Alien Pet',112,1315,(250,249,238),a)
  txt(im,'Escape Puzzle',44,1460,(204,236,224),a,bold=False)
  d=ImageDraw.Draw(im);d.rounded_rectangle((295,1585,785,1700),radius=53,fill=(157,222,207));txt(im,'PLAY NOW',39,1620,(16,49,54),a)
 return im.tobytes()

def build(p):
 out=OUT/f"AlienPet_{p['id']}_{p['name']}_{p['duration']}s_1080x1920.mp4"
 tmp=W/f"{p['id']}-picture.mp4"
 # A 30 fps output preserves the source timing; the old cinema source is 24 fps.
 dec=sp.Popen([FF,'-v','error','-i',p['source'],'-t',str(p['duration']),'-vf','fps=30,scale=1080:1920:flags=lanczos,setsar=1','-f','rawvideo','-pix_fmt','rgb24','-'],stdout=sp.PIPE)
 enc=sp.Popen([FF,'-y','-v','error','-f','rawvideo','-pix_fmt','rgb24','-video_size','1080x1920','-framerate','30','-i','-','-an','-c:v','libx264','-crf','18','-preset','fast','-pix_fmt','yuv420p',str(tmp)],stdin=sp.PIPE)
 for f in range(p['duration']*30):
  raw=dec.stdout.read(1080*1920*3)
  assert len(raw)==1080*1920*3,(p['id'],f)
  enc.stdin.write((cinematic if p['kind']=='cinematic' else native)(raw,p,f/30))
 dec.stdout.close();dec.wait();enc.stdin.close();assert enc.wait()==0
 sp.run([FF,'-y','-v','error','-i',str(tmp),'-i',p.get('audio',p['source']),'-map','0:v:0','-map','1:a:0','-c:v','copy','-af','loudnorm=I=-16:TP=-1.5:LRA=9','-ar','48000','-ac','2','-c:a','aac','-b:a','192k','-t',str(p['duration']),'-movflags','+faststart',str(out)],check=True)
 print('COMPLETE',out.name,flush=True)

if __name__=='__main__':
 (R/'rebrand-manifest.json').write_text(json.dumps(PLANS,indent=2))
 for p in PLANS:build(p)

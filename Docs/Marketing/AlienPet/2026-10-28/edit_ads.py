"""Reproducible mixed-media ad edit. PIL motion design + ffmpeg; no stock audio."""
from pathlib import Path
import subprocess as sp, json, sys, math, wave
import numpy as np
from PIL import Image,ImageDraw,ImageFont,ImageFilter
ROOT=next(p for p in Path(__file__).resolve().parents if (p/'Assets/_Game').is_dir()); R=ROOT/'Artifacts/AlienPetAds-2026-10-28'; S=R/'sources'; W=R/'edit'; W.mkdir(exist_ok=True); OUT=R/'final';OUT.mkdir(exist_ok=True)
FF='/opt/homebrew/bin/ffmpeg'; SR=48000; FPS=30; size=(1080,1920)
BOLD=str(ROOT/'Assets/_Game/Venom/Resources/COgheUI/ManropeBold.ttf'); REG=str(ROOT/'Assets/_Game/Venom/Resources/COgheUI/Manrope.ttf')
INK=(39,67,74); TEAL=(47,113,114); MINT=(165,225,208); PAPER=(247,248,231)
AI1=next(S.glob('shot_01_*.mp4'));AI3=next(S.glob('shot_03_*.mp4'));AI4=next(S.glob('shot_04_*.mp4'));GAME=S/'gameplay-native.mp4'
def seg(src,start,dur,kind,heading,speed=1):return dict(src=str(src),start=start,duration=dur,kind=kind,heading=heading,speed=speed)
def plans():
 tube=list(S.glob('shot_02R*.mp4'))
 if not tube:tube=[S/'shot_02R_PENDING.mp4']
 return {
 'A_ESCAPE_22s':[
 seg(AI4,2.8,2.4,'cinematic','THIS PET\nHAS A PLAN.'),
 seg(AI1,1,3,'cinematic','ONE LITTLE\nESCAPE ARTIST.'),
 seg(GAME,.9,4,'game','TAP. SOLVE.\nESCAPE.',1.1),
 seg(GAME,16,5,'game',"WHAT’S UNDER\nTHE BOX?",1.4),
 seg(tube[0],1.1,4,'cinematic','SQUEEZE INTO\nADVENTURE.'),
 seg(GAME,31.6,3.6,'end','Alien Pet',.6)],
 'B_BEST_FRIEND_24s':[
 seg(AI3,4.6,2.8,'cinematic','NOT YOUR\nAVERAGE PET.'),
 seg(GAME,26.8,3,'home','A LITTLE LOVE…'),
 seg(AI4,2.8,2.8,'cinematic','…A LOT OF\nPERSONALITY.'),
 seg(GAME,.8,4.4,'game','BIG BRAINS.\nTINY BODY.'),
 seg(GAME,16.4,4.8,'game','HELP IT FIND\nTHE WAY.',1.4),
 seg(AI3,4.8,3,'cinematic','YOUR NEW\nALIEN FRIEND.'),
 seg(GAME,31.6,3.2,'end','Alien Pet',.65)]
 }
def run(cmd):sp.run(cmd,check=True)
def prep(s,p):
 k=s['kind']; speed=s['speed']; vf=f"setpts=(PTS-STARTPTS)/{speed},fps=30,"
 if k=='cinematic':vf+='scale=1080:1920:flags=lanczos,eq=contrast=1.035:saturation=1.04'
 elif k=='game':vf+='crop=1080:1080:0:390,pad=1080:1920:0:490:color=0xe8eee5'
 elif k=='end':vf+='crop=720:650:180:560,scale=900:812:flags=lanczos,pad=1080:1920:90:620:color=0xffffef'
 else:vf+='crop=1080:760:0:470,pad=1080:1920:0:680:color=0xffffef'
 vf+=',setsar=1,format=yuv420p'
 run([FF,'-y','-v','error','-ss',str(s['start']),'-i',s['src'],'-t',str(s['duration']),'-vf',vf,'-an','-c:v','libx264','-preset','fast','-crf','17',str(p)])
 if k=='cinematic':
  run([FF,'-y','-v','error','-ss',str(s['start']),'-i',s['src'],'-t',str(s['duration']),'-vn','-af',f"atempo={speed},afade=t=in:d=0.045,afade=t=out:st={s['duration']-.1}:d=0.1",'-ar',str(SR),'-ac','2',str(p.with_suffix('.wav'))])
def text_layer(text,fs,color,bold=True):
 f=ImageFont.truetype(BOLD if bold else REG,fs);box=f.getbbox(text);im=Image.new('RGBA',(math.ceil(f.getlength(text))+8,fs+25));ImageDraw.Draw(im).text((4,-box[1]),text,font=f,fill=(*color,255));return im
cache={}
def txt(im,text,fs,y,color,alpha=1,x=None,bold=True):
 key=(text,fs,color,bold)
 if key not in cache:cache[key]=text_layer(*key)
 lay=cache[key]; x=int((1080-lay.width)/2 if x is None else x)
 if alpha<1:lay=lay.copy();lay.putalpha(lay.getchannel('A').point(lambda v:int(v*max(0,alpha))))
 im.paste(lay,(x,int(y)),lay)
def smooth(x):return 1-(1-max(0,min(1,x)))**3
# Readable text is held; only arrival and small circuit tracer animate.
def graphic(frame,s,t,idx,total):
 im=Image.frombytes('RGB',size,frame);k=s['kind'];a=smooth(t/.24);shift=int((1-a)*28)
 if k=='cinematic':
  # Top gradient keeps typography clear while leaving scientist and creature unobscured.
  top=Image.new('RGBA',(1080,650));arr=np.zeros((650,1080,4),dtype=np.uint8);arr[:,:,:3]=(9,24,27);arr[:,:,3]=(np.linspace(190,0,650)[:,None]).astype(np.uint8);top=Image.fromarray(arr);im.paste(top,(0,0),top)
 color=(255,255,242) if k=='cinematic' else INK
 d=ImageDraw.Draw(im)
 if k=='end':
  matte=Image.new('L',size,0);md=ImageDraw.Draw(matte);md.rectangle((60,590,450,680),fill=255);md.rectangle((885,590,1010,715),fill=255);md.rectangle((65,1220,160,1450),fill=255);matte=matte.filter(ImageFilter.GaussianBlur(9));im.paste((255,255,239),(0,0,1080,1920),matte)
  veil=Image.new('RGBA',(900,48));va=np.zeros((48,900,4),dtype=np.uint8);va[:,:,:3]=(255,255,239);va[:,:,3]=np.linspace(255,0,48)[:,None].astype(np.uint8);veil=Image.fromarray(va);im.paste(veil,(90,620),veil)
  txt(im,'Alien Pet',128,245+shift,INK,a)
  txt(im,'Escape Puzzle',52,405+shift,TEAL,a, bold=False)
  txt(im,'A tiny friend. A big escape.',34,525,INK,a,bold=False)
  pulse=1+.018*math.sin(2*math.pi*t/.95);ww=int(620*pulse);xx=(1080-ww)//2
  d.rounded_rectangle((xx,1460,xx+ww,1586),radius=55,fill=TEAL)
  txt(im,'PLAY NOW',44,1500,(255,255,244),a)
  txt(im,'ACTUAL GAMEPLAY',21,1645,TEAL,a,bold=False)
 else:
  txt(im,'ALIEN PET  /  ESCAPE PUZZLE',24,160,color,1,bold=False)
  lines=s['heading'].split('\n');fs=78 if max(map(len,lines))<19 else 66
  for i,line in enumerate(lines):txt(im,line,fs,265+i*96+shift,color,a)
  cy=265+len(lines)*96+30
  # Circuit stroke reveal + travelling highlight: authored motion graphic, not fake controls.
  x1,x2=130,950;visible=int((x2-x1)*smooth(t/.8));d.line((x1,cy,x1+visible,cy),fill=MINT if k=='cinematic' else TEAL,width=3)
  for x in (x1,x2):d.ellipse((x-7,cy-7,x+7,cy+7),fill=MINT if k=='cinematic' else TEAL)
  if visible>0:
   x=x1+(t*.22%1)*visible;d.ellipse((x-5,cy-5,x+5,cy+5),fill=(250,210,134))
  label='AI CINEMATIC' if k=='cinematic' else 'ACTUAL GAMEPLAY'
  badge=Image.new('RGBA',(460,66));bd=ImageDraw.Draw(badge);bd.rounded_rectangle((0,0,460,65),32,fill=(13,38,43,220) if k=='cinematic' else (255,255,247,235))
  im.paste(badge,(310,1660),badge)
  txt(im,label,24,1680,(238,248,242) if k=='cinematic' else TEAL,bold=False)
 # Tiny segment bars: quiet motion-design framing, no simulated game UI.
 for j in range(total):
  x=130+j*820/total;d.rounded_rectangle((x,1780,x+820/total-10,1785),2,fill=TEAL if j<=idx else (154,174,167))
 return im.tobytes()
def music(duration,mode):
 n=round(duration*SR);mix=np.zeros((n,2),np.float64);rng=np.random.default_rng(28+mode);beat=60/(144 if mode==0 else 128)
 def add(x,at,g,pan=0):
  b=round(at*SR);e=min(n,b+len(x));
  if b>=n:return
  mix[b:e,0]+=x[:e-b]*g*math.cos((pan+1)*math.pi/4);mix[b:e,1]+=x[:e-b]*g*math.sin((pan+1)*math.pi/4)
 def tone(m,dur,decay=.45):
  t=np.arange(round(dur*SR))/SR;f=440*2**((m-69)/12)
  return (np.sin(2*np.pi*f*t)+.20*np.sin(2*np.pi*2*f*t)+.12*np.sin(2*np.pi*3*f*t))*np.minimum(t/.015,1)*np.exp(-t/decay)*np.minimum((dur-t)/.06,1)
 for i,at in enumerate(np.arange(0,duration-2,beat)):
  t=np.arange(int(.5*SR))/SR;dr=np.sin(2*np.pi*(49*t+70*.022*(1-np.exp(-t/.022))))*np.exp(-t/.13)*np.minimum(t/.002,1)
  add(dr,at,.17 if mode==0 else .11)
  if i%4 in [1,3]:add(rng.normal(0,1,len(t))*np.exp(-t/.025),at,.07)
  root=[50,46,43,45][int(at/4)%4] if mode==0 else [50,54,55,57][int(at/4)%4]
  for j in range(2):add(tone(root+[0,7,12,7,3,7,12,15][(i*2+j)%8],.32),at+j*beat/2,.06 if mode==0 else .048,(-1 if j==0 else 1)*.4)
 for m in [50,57,62,66]:add(tone(m,3,1.6),duration-3,.11)
 # Short airy rises around the montage changes.
 for at in [2.15,5.1,9.1,14.1,duration-3.25]:
  t=np.arange(round(.24*SR))/SR;x=rng.normal(0,1,len(t));x=np.convolve(x,np.ones(25)/25,'same')*(t/.24)**1.2
  add(x,at,.16)
 mix*=np.minimum(np.arange(n)/(.02*SR),1)[:,None]*np.minimum((n-1-np.arange(n))/(.35*SR),1)[:,None]
 return mix

def audio(items,name):
 dur=sum(x['duration'] for x in items);mix=music(dur,0 if name.startswith('A') else 1);at=0
 for i,s in enumerate(items):
  wav=W/f'{name}-{i}.wav'
  if wav.exists():
   with wave.open(str(wav)) as w:x=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(np.float64).reshape(-1,2)/32768
   b=round(at*SR);e=min(len(mix),b+len(x));mix[b:e]+=x[:e-b]*.72
  at+=s['duration']
 # Native game effects timed to visible operation and success.
 sfx=[(5.7,'mech_latch',.55),(8.1,'mech_arrive',.6),(10,'creature_slide',.5),(14,'creature_tada',.45)] if name.startswith('A') else [(3.1,'creature_curious_1',.6),(9.1,'mech_latch',.55),(12.7,'mech_arrive',.45),(16.9,'creature_tada',.45)]
 for at,stem,g in sfx:
  p=ROOT/f'Assets/_Game/Venom/Resources/COgheAudio/{stem}.ogg'
  if not p.exists():continue
  raw=sp.check_output([FF,'-v','error','-i',str(p),'-f','f32le','-ac','2','-ar',str(SR),'-'])
  x=np.frombuffer(raw,dtype='<f4').reshape(-1,2);b=round(at*SR);e=min(len(mix),b+len(x));mix[b:e]+=x[:e-b]*g
 mix=np.tanh(mix);peak=abs(mix).max();mix*=.88/max(.88,peak)
 p=W/f'{name}-mix.wav'
 with wave.open(str(p),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes((mix*32767).astype('<i2').tobytes())
 return p

def build(name,items):
 manifest=[];time=0
 for i,s in enumerate(items):
  p=W/f'{name}-{i}.mp4'
  if not p.exists():prep(s,p)
  manifest.append(dict(**s,timeline_start=round(time,3)));time+=s['duration']
 (W/f'{name}-manifest.json').write_text(json.dumps(manifest,indent=2))
 print('prepared',name,flush=True)
 silent=W/f'{name}-picture.mp4';enc=sp.Popen([FF,'-y','-v','error','-f','rawvideo','-pixel_format','rgb24','-video_size','1080x1920','-framerate','30','-i','-','-an','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p',str(silent)],stdin=sp.PIPE)
 for i,s in enumerate(items):
  dec=sp.Popen([FF,'-v','error','-i',str(W/f'{name}-{i}.mp4'),'-f','rawvideo','-pix_fmt','rgb24','-'],stdout=sp.PIPE);last=None
  for f in range(round(s['duration']*30)):
   raw=dec.stdout.read(1080*1920*3)
   if len(raw)!=1080*1920*3:
    if last is None:raise RuntimeError('No decoded source frame')
    raw=last
   last=raw;enc.stdin.write(graphic(raw,s,f/30,i,len(items)))
  dec.stdout.close();dec.wait();print('rendered',name,i,flush=True)
 enc.stdin.close();assert enc.wait()==0
 wav=audio(items,name);final=OUT/f'AlienPet_{name}_1080x1920.mp4'
 run([FF,'-y','-v','error','-i',str(silent),'-i',str(wav),'-map','0:v:0','-map','1:a:0','-c:v','copy','-af','loudnorm=I=-16:TP=-1.5:LRA=9','-ar','48000','-c:a','aac','-b:a','192k','-movflags','+faststart','-t',str(time),str(final)])
 print('COMPLETE',final,flush=True)
if __name__=='__main__':
 pp=plans()
 for name,items in pp.items():
  if len(sys.argv)>1 and sys.argv[1] not in name:continue
  build(name,items)

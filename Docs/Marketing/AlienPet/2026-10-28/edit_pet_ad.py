"""24-second pet-care creative: Flow footage + captured, unmodified Unity animations.
Run from this folder with Python (Pillow, numpy), ffmpeg and ffprobe installed.
"""
from pathlib import Path
import subprocess as sp, json, math, wave
import numpy as np
from PIL import Image, ImageDraw, ImageFont

R=Path(__file__).resolve().parent
ROOT=R.parents[1]
S=R/'sources'; W=R/'edit'; W.mkdir(exist_ok=True)
OUT=R/'final'; OUT.mkdir(exist_ok=True)
FF='/opt/homebrew/bin/ffmpeg'; SR=48000; FPS=30; SIZE=(1080,1920)
BOLD=str(ROOT/'Assets/_Game/Venom/Resources/COgheUI/ManropeBold.ttf')
REG=str(ROOT/'Assets/_Game/Venom/Resources/COgheUI/Manrope.ttf')
INK=(39,67,74); TEAL=(47,113,114); MINT=(175,228,208); PEACH=(248,195,168)
PAPER=(255,255,245)
GAME=S/'pet-care-native.mp4'

def shot(src,start,duration,kind,heading,tag='',speed=1):
 return dict(src=str(src),start=start,duration=duration,kind=kind,heading=heading,tag=tag,speed=speed)

PLAN=[
 shot(next(S.glob('PET_C01*.mp4')),1.1,3.2,'ai','TINY PET.\nBIG APPETITE.'),
 shot(GAME,7.7,2.6,'home','SNACK TIME!','YUM!'),
 shot(GAME,13.9,2.4,'home','OOPS. SPICY!','A LITTLE TOO HOT…'),
 shot(GAME,19.8,3.4,'style','A SPLASH\nOF YOU.','MINT MAKEOVER'),
 shot(GAME,24.8,1.2,'style','MAKE IT\nYOUR OWN.','FLOWER POWER'),
 shot(GAME,27.8,1,'style','MAKE IT\nYOUR OWN.','PARTY TIME'),
 shot(GAME,29.7,1.2,'style','MAKE IT\nYOUR OWN.','BEE YOURSELF'),
 shot(next(S.glob('PET_C02*.mp4')),2,3.4,'ai','LOVE AT\nFIRST SQUISH.'),
 shot(GAME,3.4,2,'home','SO MUCH\nPERSONALITY.','A LITTLE LOVE GOES A LONG WAY'),
 shot(GAME,29.5,3.6,'end','Alien Pet',speed=.65),
]

def run(args):sp.run(args,check=True)
def prep(s,p):
 vf=f"setpts=(PTS-STARTPTS)/{s['speed']},fps=30,"
 if s['kind']=='ai':vf+='scale=1080:1920:flags=lanczos,eq=saturation=1.05:brightness=0.015'
 elif s['kind']=='home':vf+='crop=1000:810:40:430,scale=1080:874:flags=lanczos,pad=1080:1920:0:500:color=0xfffff5'
 else:vf+='crop=720:480:180:215,scale=1080:720:flags=lanczos,pad=1080:1920:0:620:color=0xfffff5'
 vf+=',setsar=1,format=yuv420p'
 run([FF,'-y','-v','error','-ss',str(s['start']),'-i',s['src'],'-t',str(s['duration']),'-vf',vf,'-an','-c:v','libx264','-preset','fast','-crf','17',str(p)])
 if s['kind']=='ai':
  run([FF,'-y','-v','error','-ss',str(s['start']),'-i',s['src'],'-t',str(s['duration']),'-vn','-af',f"afade=t=in:d=0.04,afade=t=out:st={s['duration']-.1}:d=0.1",'-ar',str(SR),'-ac','2',str(p.with_suffix('.wav'))])

cache={}
def txt(im,text,fs,y,color,alpha=1,bold=True):
 key=(text,fs,color,bold)
 if key not in cache:
  f=ImageFont.truetype(BOLD if bold else REG,fs);b=f.getbbox(text)
  z=Image.new('RGBA',(math.ceil(f.getlength(text))+12,fs+25))
  ImageDraw.Draw(z).text((6,-b[1]),text,font=f,fill=(*color,255));cache[key]=z
 z=cache[key]
 if alpha<1:z=z.copy();z.putalpha(z.getchannel('A').point(lambda v:int(v*max(0,alpha))))
 im.paste(z,((1080-z.width)//2,int(y)),z)

def smooth(x):return 1-(1-max(0,min(1,x)))**3
def star(d,x,y,r,color):
 pts=[]
 for j in range(8):
  a=j*math.pi/4;rr=r if j%2==0 else r*.24;pts.append((x+rr*math.cos(a),y+rr*math.sin(a)))
 d.polygon(pts,fill=color)

def graphic(raw,s,t,idx):
 im=Image.frombytes('RGB',SIZE,raw);kind=s['kind'];a=smooth(t/.22);dy=(1-a)*25
 if kind=='ai':
  ar=np.zeros((650,1080,4),dtype=np.uint8);ar[:,:,:3]=(16,34,36);ar[:,:,3]=np.linspace(195,0,650)[:,None].astype(np.uint8)
  z=Image.fromarray(ar);im.paste(z,(0,0),z)
 col=(255,255,243) if kind=='ai' else INK;d=ImageDraw.Draw(im)
 if kind in ('style','end'):
  # Mask only clipped corner-button shadows, away from the creature and its hat.
  d.rectangle((0,620,220,690),fill=PAPER);d.rectangle((910,620,1079,690),fill=PAPER)
 if kind=='end':
  txt(im,'Alien Pet',128,245+dy,INK,a)
  txt(im,'Escape Puzzle',52,410+dy,TEAL,a,bold=False)
  txt(im,'Your little alien. Your big adventure.',34,530,INK,a,bold=False)
  # Authored motion graphic accents sit outside the real creature.
  for j,(x,y) in enumerate([(170,850),(895,1030),(220,1200)]):
   star(d,x,y+9*math.sin(t*3+j),17+3*math.sin(t*4+j),MINT if j%2==0 else PEACH)
  ww=int(620*(1+.016*math.sin(t*2*math.pi)));x=(1080-ww)//2
  d.rounded_rectangle((x,1460,x+ww,1588),radius=57,fill=TEAL)
  txt(im,'PLAY NOW',44,1502,(255,255,243),a)
  txt(im,'ACTUAL GAMEPLAY',23,1642,TEAL,bold=False)
 else:
  txt(im,'ALIEN PET  /  ESCAPE PUZZLE',24,155,col,bold=False)
  lines=s['heading'].split('\n');fs=80 if max(map(len,lines))<18 else 67
  for j,line in enumerate(lines):txt(im,line,fs,260+j*98+dy,col,a)
  if s['tag']:
   fs=30 if len(s['tag'])>22 else 36
   color=PEACH if idx==2 else MINT
   wid=min(860,max(350,int(len(s['tag'])*fs*.61+90)));x=(1080-wid)//2
   y=1450+int(6*math.sin(t*3.8))
   d.rounded_rectangle((x,y,x+wid,y+86),radius=40,fill=color)
   txt(im,s['tag'],fs,y+24,INK,a)
  badge=Image.new('RGBA',(410,64));bd=ImageDraw.Draw(badge)
  bd.rounded_rectangle((0,0,409,63),radius=32,fill=(24,49,51,215) if kind=='ai' else (225,238,227,255))
  im.paste(badge,(335,1655),badge)
  txt(im,'AI CINEMATIC' if kind=='ai' else 'ACTUAL GAMEPLAY',23,1675,(248,255,247) if kind=='ai' else TEAL,bold=False)
 return im.tobytes()

def music(duration):
 n=round(duration*SR);mix=np.zeros((n,2),dtype=np.float64);beat=60/140;rng=np.random.default_rng(2810)
 def add(x,at,g,pan=0):
  b=round(at*SR);e=min(n,b+len(x))
  if b>=n:return
  mix[b:e,0]+=x[:e-b]*g*math.cos((pan+1)*math.pi/4);mix[b:e,1]+=x[:e-b]*g*math.sin((pan+1)*math.pi/4)
 def note(m,dur,decay=.19):
  t=np.arange(round(dur*SR))/SR;f=440*2**((m-69)/12)
  x=np.sin(2*np.pi*f*t)+.23*np.sin(2*np.pi*f*2*t)*np.exp(-t/.1)+.1*np.sin(2*np.pi*f*3*t)*np.exp(-t/.05)
  return x*np.minimum(t/.003,1)*np.exp(-t/decay)*np.minimum((dur-t)/.04,1)
 # Original, light marimba/pluck phrase: no licensed song or external sample.
 melody=[76,79,83,79,81,79,76,74,72,76,79,83,81,79,76,79]
 for i,at in enumerate(np.arange(0,duration-3,beat/2)):
  add(note(melody[i%len(melody)],.34),at,.095,(-.35 if i%2 else .35))
  if i%4==0:add(note([48,53,55,48][i//8%4],.5,.24),at,.15)
  if i%2:
   t=np.arange(int(.08*SR))/SR;x=rng.normal(0,1,len(t))*np.exp(-t/.014);add(x,at,.022,.1)
  if i%4==0:
   t=np.arange(int(.2*SR))/SR;x=np.sin(2*np.pi*(85*t+40*.025*(1-np.exp(-t/.025))))*np.exp(-t/.045);add(x,at,.085)
 for at in [3.2,5.8,8.2,11.6,12.8,13.8,18.4]:
  t=np.arange(int(.12*SR))/SR;x=np.sin(2*np.pi*(450*t+1200*t*t))*np.exp(-t/.035)*np.minimum(t/.002,1);add(x,at,.13)
 for m in [60,64,67,72,76]:add(note(m,3,.8),20.4,.09)
 return mix

def audio():
 mix=music(24);offset=0
 events=[]
 for line in (R/'native-frames/sounds.txt').read_text().splitlines():
  t,stem,g=line.split();events.append((float(t),stem,float(g)))
 for i,s in enumerate(PLAN):
  if s['kind']=='ai':
   with wave.open(str(W/f'seg-{i}.wav')) as w:x=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(float).reshape(-1,2)/32768
   b=round(offset*SR);e=min(len(mix),b+len(x));mix[b:e]+=x[:e-b]*.5
  else:
   for t,stem,g in events:
    rel=(t-s['start'])/s['speed']
    if not 0<=rel<s['duration']:continue
    p=ROOT/f'Assets/_Game/Venom/Resources/COgheAudio/{stem}.ogg'
    if not p.exists():continue
    raw=sp.check_output([FF,'-v','error','-i',str(p),'-f','f32le','-ac','2','-ar',str(SR),'-'])
    x=np.frombuffer(raw,dtype='<f4').reshape(-1,2)
    b=round((offset+rel)*SR);e=min(len(mix),round((offset+s['duration'])*SR),b+len(x))
    mix[b:e]+=x[:e-b]*g*.8
  offset+=s['duration']
 mix=np.tanh(mix)
 fade=np.minimum(np.arange(len(mix))/(.01*SR),1)*np.minimum((len(mix)-1-np.arange(len(mix)))/(.4*SR),1);mix*=fade[:,None]
 mix*=.85/max(.85,abs(mix).max())
 wav=W/'pet-mix.wav'
 with wave.open(str(wav),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes((mix*32767).astype('<i2').tobytes())
 return wav

def main():
 time=0;manifest=[]
 for i,s in enumerate(PLAN):
  p=W/f'seg-{i}.mp4'
  if not p.exists():prep(s,p)
  manifest.append(dict(**s,timeline_start=round(time,3)));time+=s['duration']
 assert abs(time-24)<.001
 (R/'C-edit-manifest.json').write_text(json.dumps(manifest,indent=2))
 pic=W/'picture.mp4'
 enc=sp.Popen([FF,'-y','-v','error','-f','rawvideo','-pixel_format','rgb24','-video_size','1080x1920','-framerate','30','-i','-','-an','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p',str(pic)],stdin=sp.PIPE)
 for i,s in enumerate(PLAN):
  dec=sp.Popen([FF,'-v','error','-i',str(W/f'seg-{i}.mp4'),'-f','rawvideo','-pix_fmt','rgb24','-'],stdout=sp.PIPE)
  for f in range(round(s['duration']*30)):
   raw=dec.stdout.read(1080*1920*3)
   if len(raw)!=1080*1920*3:raise RuntimeError(f'Missing source frame: {i}/{f}')
   enc.stdin.write(graphic(raw,s,f/30,i))
  dec.stdout.close();dec.wait();print('rendered',i,flush=True)
 enc.stdin.close();assert enc.wait()==0
 final=OUT/'AlienPet_C_CUTE_PET_24s_1080x1920.mp4'
 run([FF,'-y','-v','error','-i',str(pic),'-i',str(audio()),'-map','0:v:0','-map','1:a:0','-c:v','copy','-af','loudnorm=I=-16:TP=-1.5:LRA=9','-ar','48000','-c:a','aac','-b:a','192k','-movflags','+faststart','-t','24',str(final)])
 print('COMPLETE',final,flush=True)

if __name__=='__main__':main()

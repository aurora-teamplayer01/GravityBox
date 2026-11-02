from pathlib import Path
from PIL import Image,ImageDraw
import subprocess as sp,json,re,hashlib
R=Path(__file__).resolve().parent;Q=R/'qa';Q.mkdir(exist_ok=True);results=[]
for p in sorted((R/'final').glob('*.mp4')):
 info=json.loads(sp.check_output(['ffprobe','-v','error','-count_frames','-show_streams','-show_format','-of','json',str(p)]))
 v=next(x for x in info['streams'] if x['codec_type']=='video');a=next(x for x in info['streams'] if x['codec_type']=='audio');dur=float(info['format']['duration'])
 assert v['width']==1080 and v['height']==1920 and v['r_frame_rate']=='30/1'
 assert int(v['nb_read_frames'])==round(dur*30) and a['sample_rate']=='48000' and a['channels']==2
 z=sp.run(['ffmpeg','-hide_banner','-i',str(p),'-vf','blackdetect=d=0.1:pix_th=0.05','-af','astats=metadata=0:reset=0','-f','null','-'],capture_output=True,text=True)
 assert z.returncode==0 and 'black_start:' not in z.stderr
 peaks=[float(x) for x in re.findall(r'Peak level dB: ([\-\d.]+)',z.stderr)];assert peaks and max(peaks)<0
 times=[.8,3.5,dur*.4,dur*.6,dur*.78,dur-.4];sheet=Image.new('RGB',(1320,416),'#243d40');d=ImageDraw.Draw(sheet)
 for i,t in enumerate(times):
  still=Q/f'{p.stem}-{i}.png';sp.run(['ffmpeg','-y','-v','error','-ss',str(t),'-i',str(p),'-frames:v','1',str(still)],check=True)
  im=Image.open(still);im.thumbnail((220,391));sheet.paste(im,(220*i,25));d.text((220*i+5,5),f'{t:.1f}s',fill='white')
 sheet.save(Q/f'{p.stem}-contact.jpg')
 results.append(dict(file=p.name,duration=dur,frames=int(v['nb_read_frames']),format='1080x1920 / 30fps / H.264 / stereo AAC 48kHz',black_intervals=0,peak_dbfs=max(peaks),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
 print(p.name,'PASS',flush=True)
(Q/'validation.json').write_text(json.dumps(results,indent=2))

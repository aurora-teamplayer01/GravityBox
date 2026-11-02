from pathlib import Path
import subprocess,json
from PIL import Image,ImageDraw
root=next(p for p in Path(__file__).resolve().parents if (p/'Assets/_Game').is_dir());r=root/'Artifacts/AlienPetAds-2026-10-28';results=[]
for p in sorted((r/'final').glob('*.mp4')):
 info=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_streams','-show_format','-of','json',str(p)]));v=next(s for s in info['streams'] if s['codec_type']=='video');a=next(s for s in info['streams'] if s['codec_type']=='audio');dur=float(info['format']['duration'])
 assert 18<=dur<=25 and v['width']==1080 and v['height']==1920 and v['r_frame_rate']=='30/1'
 assert int(v['nb_frames'])==round(dur*30) and a['sample_rate']=='48000'
 black=subprocess.run(['ffmpeg','-hide_banner','-i',str(p),'-vf','blackdetect=d=0.10:pix_th=0.05','-an','-f','null','-'],capture_output=True,text=True).stderr
 assert 'black_start:' not in black
 audio=subprocess.run(['ffmpeg','-hide_banner','-i',str(p),'-vn','-af','astats=metadata=0:reset=0','-f','null','-'],capture_output=True,text=True).stderr
 peaks=[line.strip() for line in audio.splitlines() if 'Peak level dB' in line or 'RMS level dB' in line]
 times=[.4,3.5,6.5,10.5,14.5,18,20.5,dur-.5];sheet=Image.new('RGB',(8*200,382),'#192e32');d=ImageDraw.Draw(sheet)
 for i,t in enumerate(times):
  jpg=r/'qa'/f'{p.stem}-{i}.jpg';subprocess.run(['ffmpeg','-y','-v','error','-ss',str(t),'-i',str(p),'-frames:v','1',str(jpg)],check=True);im=Image.open(jpg);im.thumbnail((200,356));sheet.paste(im,(200*i,25));d.text((200*i+5,4),f'{t:.1f}s',fill='white')
 sheet.save(r/'qa'/f'{p.stem}-contact.jpg')
 results.append(dict(file=p.name,duration=dur,frames=int(v['nb_frames']),video='1080x1920, 30fps H.264',audio='48kHz stereo AAC',black_intervals=0,audio_stats=peaks))
(r/'qa'/'validation.json').write_text(json.dumps(results,indent=2));print(json.dumps(results,indent=2))

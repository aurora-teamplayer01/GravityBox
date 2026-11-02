"""Collect finished creative exports with explicit current / archive / reference status."""
from pathlib import Path
import json, hashlib, shutil, subprocess as sp, zipfile, html

ROOT=Path('/Users/mrk/GravityBox')
R=ROOT/'Artifacts/AlienPetPetAd-2026-10-28'
DOC=ROOT/'Docs/Marketing/AlienPet/2026-10-28'
KIT=ROOT/'Artifacts/AlienPet_Ads_ResourceKit_2026-10-28'
ZIP=Path('/Users/mrk/Downloads/AlienPet_Ads_ResourceKit_2026-10-28.zip')
KIT.mkdir(exist_ok=True);(KIT/'Thumbnails').mkdir(exist_ok=True)

def item(id,title,p,group,angle,poster,note=''):
 return dict(id=id,title=title,source=Path(p),group=group,angle=angle,poster_time=poster,note=note)

ITEMS=[
 item('A','Escape Artist',DOC/'videos/AlienPet_A_ESCAPE_22s_1080x1920.mp4','01_CURRENT_ALIEN_PET','Cơ quan, giải đố, tìm lối thoát',.8),
 item('B','Your New Alien Friend',DOC/'videos/AlienPet_B_BEST_FRIEND_24s_1080x1920.mp4','01_CURRENT_ALIEN_PET','Gắn bó với pet + giải đố',1),
 item('C','Tiny Pet, Big Personality',R/'final/AlienPet_C_CUTE_PET_24s_1080x1920.mp4','01_CURRENT_ALIEN_PET','Nuôi pet: cho ăn, phản ứng vui, tiêm màu, thay mũ',12.1),
 item('OLD01','COghe — Puzzle',ROOT/'Artifacts/COgheMarketingReview/COGHE_AD1_PUZZLE_15S_9X16_1080X1920.mp4','02_ARCHIVE_COGHE','Gameplay puzzle',2,'Tên game và một số thông tin cũ; cần cập nhật trước khi chạy ads.'),
 item('OLD02','COghe — Pet',ROOT/'Artifacts/COgheMarketingReview/COGHE_AD2_PET_17S_9X16_1080X1920.mp4','02_ARCHIVE_COGHE','Pet care',2,'Tên game cũ; giữ để tham khảo và dựng lại.'),
 item('OLD03','COghe — Showcase',ROOT/'Artifacts/COgheMarketingReview/COGHE_AD3_SHOWCASE_30S_9X16_1080X1920.mp4','02_ARCHIVE_COGHE','Giới thiệu tính năng',3,'Tên game và số màn trong caption cũ; không đưa thẳng vào campaign mới.'),
 item('OLD04','A Little Alien, A Big Escape',ROOT/'Artifacts/COgheCinematicAd-2026-10-22/COghe_A_Little_Alien_A_Big_Escape_30s.mp4','02_ARCHIVE_COGHE','Điện ảnh AI',3,'End card COghe cũ; không phải gameplay thật.'),
 item('REF01','COghe — alternate showcase',Path('/Users/mrk/Downloads/COghe.mp4'),'03_REFERENCE_EXPORTS','Bản xuất khác tìm thấy trong Downloads',3,'Khác hash với OLD03; tên/claims cũ. Chỉ tham khảo.'),
 item('REF02','Gameplay reel — silent',Path('/Users/mrk/Downloads/Coghe1.mp4'),'03_REFERENCE_EXPORTS','Clip gameplay không có âm thanh',3,'Chưa có âm thanh và end card hoàn chỉnh; không phải quảng cáo sẵn để chạy.'),
]

def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()

manifest=[]
for i in ITEMS:
 src=i['source'];assert src.is_file(),src
 folder=KIT/i['group'];folder.mkdir(exist_ok=True)
 name=src.name if i['id'] in ['A','B','C'] else i['id']+'_'+src.name
 dst=folder/name;shutil.copy2(src,dst)
 info=json.loads(sp.check_output(['ffprobe','-v','error','-show_streams','-show_format','-of','json',str(dst)]))
 v=next(s for s in info['streams'] if s['codec_type']=='video');a=next((s for s in info['streams'] if s['codec_type']=='audio'),None)
 # Decode every exported video fully, including archive exports.
 sp.run(['ffmpeg','-v','error','-i',str(dst),'-f','null','-'],check=True)
 poster=KIT/'Thumbnails'/f"{i['id']}.jpg"
 sp.run(['ffmpeg','-y','-v','error','-ss',str(i['poster_time']),'-i',str(dst),'-frames:v','1','-vf','scale=540:960:force_original_aspect_ratio=decrease','-q:v','2',str(poster)],check=True)
 try:source=str(src.relative_to(ROOT))
 except ValueError:source='Downloads/'+src.name
 row={k:i[k] for k in ['id','title','group','angle','note']}
 row.update(file=str(dst.relative_to(KIT)),thumbnail=str(poster.relative_to(KIT)),source=source,
   status='current_brand_test_export' if i['id'] in ['A','B','C'] else 'archive_rebrand_required' if i['group']=='02_ARCHIVE_COGHE' else 'reference_not_upload_ready',
   duration_seconds=float(info['format']['duration']),width=v['width'],height=v['height'],fps=v['r_frame_rate'],
   video_codec=v['codec_name'],audio_codec=a['codec_name'] if a else None,audio_sample_rate=a.get('sample_rate') if a else None,
   bytes=dst.stat().st_size,sha256=digest(dst),decode_verified=True)
 assert row['sha256']==digest(src)
 manifest.append(row);print('verified',i['id'],flush=True)

DATA=dict(game='Alien Pet: Escape Puzzle',creature='COghe',date='2026-10-28',current_creatives=3,archive_creatives=4,reference_exports=2,assets=manifest)
(KIT/'manifest.json').write_text(json.dumps(DATA,indent=2,ensure_ascii=False))

copy='''ALIEN PET: ESCAPE PUZZLE — ENGLISH AD COPY
2026-10-28 | Creative suggestions, no campaign published

A — ESCAPE ARTIST
Headline: A Tiny Pet. A Big Escape.
Primary text: Can a tiny alien outsmart a lab full of puzzles? Help your new pet find the way out.
CTA: Play Now

B — YOUR NEW ALIEN FRIEND
Headline: Meet Your New Alien Friend
Primary text: Meet your new alien friend. Solve clever escape puzzles, then spend time together at home.
CTA: Play Now

C — TINY PET, BIG PERSONALITY
Headline: Your Pet. Your Personality.
Primary text: Snack time. Silly reactions. A whole new look. Meet your little alien pet and help it escape.
CTA: Play Now

All three current-brand edits are English, vertical 9:16, H.264 / AAC.
AI dramatization and actual Unity gameplay are labelled in each video.
No store URL, pricing claim, fake review, install count or performance guarantee is embedded.
Set the real store destination in the ad platform when the listing is ready.
'''
(KIT/'ad-copy.txt').write_text(copy)

readme='''# Alien Pet: Escape Puzzle — bộ tài nguyên quảng cáo

Ngày xuất: 28/10/2026. Mở **index.html** để xem và tải từng video.

## Dùng để chuẩn bị test quảng cáo

Thư mục **01_CURRENT_ALIEN_PET** có ba video mang nhận diện mới:

| ID | Hướng sáng tạo | Thời lượng |
|---|---|---|
| A | Escape Artist — giải đố, cơ quan, thoát khỏi hộp | 22 giây |
| B | Your New Alien Friend — gắn bó với pet + giải đố | 24 giây |
| C | Tiny Pet, Big Personality — nuôi pet vui nhộn, cho ăn, đổi màu, thay mũ | 24 giây |

Cả ba: tiếng Anh, 1080×1920, 9:16, 30fps, H.264, stereo AAC 48kHz, fast-start.
Đây là các file để test creative, chưa có campaign được đăng. Hiệu quả CPI cần đo thực tế.

## Các bản cũ và nguồn tham khảo

- **02_ARCHIVE_COGHE**: 4 quảng cáo hoàn chỉnh mang tên COghe cũ (15/17/30/30 giây). Cần đổi nhận diện và kiểm tra caption trước khi sử dụng.
- **03_REFERENCE_EXPORTS**: 2 bản xuất khác tìm thấy trong Downloads. `REF01` là showcase tên cũ; `REF02` là gameplay không âm thanh, chưa có end card. Không dùng chúng như bản quảng cáo đã hoàn thiện.
- Các file trùng nội dung do sao chép, intermediate picture-lock và shot nguồn đơn lẻ không được tính là quảng cáo riêng. ActionCut cũ chỉ còn nguồn dựng, không tìm thấy bản final nên không gắn nhãn hoàn thành.

## Tài nguyên kèm theo

- **Thumbnails/**: ảnh xem nhanh từng video.
- **ad-copy.txt**: headline, primary text, CTA tiếng Anh cho A/B/C.
- **manifest.json**: thời lượng, kích thước, frame rate, âm thanh, nguồn, trạng thái và SHA-256.
- **Checks/**: thông số kiểm tra file mới, bảng shot, kết quả capture thật trong Unity.
- **Sources_C/**: hai cảnh Veo, reel gameplay gốc đã nén, mốc thao tác/âm thanh và script dựng clip C để dựng lại. File PNG trung gian không đóng gói.

## Nguồn hình và âm thanh

Các cảnh người thật + sinh vật 3D ở A/B/C được tạo trong Google Flow bằng Veo 3.1 Quality, dựa trên cùng bộ ảnh tham chiếu. C có hai cảnh mới: cho ăn và vuốt ve. Nguồn AI 720×1280 được upscale lên 1080×1920; không gọi đây là AI native 1080p.

Phần ACTUAL GAMEPLAY quay từ Unity hiện tại. C sử dụng thao tác thật trong Home/Style: ăn hai loại snack, phản ứng ăn cay, giữ ống tiêm Mint và thay ba mũ. Fixture mở khóa trong bộ nhớ phục vụ quay hình, không ghi tiến độ hay mua hàng. Các khung hình được crop để tập trung vào pet, không thay animation. Âm thanh game được đặt theo timestamp capture; nhạc nền mới được tổng hợp riêng bằng script, kết hợp foley do Veo tạo. Không dùng bài hát thương mại có sẵn.

Hai cảnh AI có nhãn AI CINEMATIC, cảnh game có nhãn ACTUAL GAMEPLAY. Không gắn logo nền tảng/stores hay đường dẫn chưa được cung cấp.

Toàn bộ 9 video xuất được decode kiểm tra. Ba bản mới được kiểm tra thời lượng, frame count, khung đen và âm lượng; ZIP được đọc lại để kiểm tra checksum. Lịch sử nguồn được giữ nguyên ngoài gói.
'''
(KIT/'README.md').write_text(readme)

def card(x):
 dur=f"{x['duration_seconds']:.2f}".rstrip('0').rstrip('.')
 status='BẢN MỚI' if x['id'] in ['A','B','C'] else 'CẦN ĐỔI NHẬN DIỆN' if x['group']=='02_ARCHIVE_COGHE' else 'CHỈ THAM KHẢO'
 return f'''<article><div class="status">{status}</div><h2>{html.escape(x['id']+' · '+x['title'])}</h2><p class="meta">{dur}s · {x['width']}×{x['height']} · {'Có âm thanh' if x['audio_codec'] else 'Không có âm thanh'}</p><video controls playsinline preload="none" poster="{x['thumbnail']}" src="{x['file']}"></video><p>{html.escape(x['angle'])}</p><p class="note">{html.escape(x['note'])}</p><a download href="{x['file']}">Tải MP4 ↓</a></article>'''

page='''<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Alien Pet — Ad Resource Kit</title><style>
*{box-sizing:border-box}body{margin:0;background:#f5f6ed;color:#29434a;font:16px/1.55 system-ui}main{max-width:1180px;margin:auto;padding:48px 28px}header{max-width:900px;margin-bottom:32px}h1{font-size:clamp(32px,5vw,56px);line-height:1.1;margin:10px 0 18px}header p{font-size:19px;color:#506b68}h2{font-size:22px;line-height:1.25;margin:8px 0}.eyebrow{color:#397b77;letter-spacing:2px;font-size:13px}.grid{display:grid;grid-template-columns:repeat(3,1fr);gap:22px}article{padding:20px;border:1px solid #dbe3d9;border-radius:24px;background:#fffff6}video{width:100%;max-height:560px;background:#203e41;border-radius:14px}.meta,.note{font-size:13px;color:#617573}.note:empty{display:none}.status{font-size:11px;letter-spacing:1px;color:#327168}a{color:#286b69}article a{display:inline-block;background:#dceee3;border-radius:20px;padding:8px 16px;text-decoration:none}details{margin-top:35px;border-top:1px solid #b7c9c0;padding-top:24px}summary{font-size:23px;cursor:pointer;margin-bottom:25px}.info{padding:18px 22px;background:#e2eee5;border-radius:16px;margin:30px 0}footer{font-size:14px;margin-top:35px}@media(max-width:800px){.grid{grid-template-columns:1fr 1fr}}@media(max-width:520px){main{padding:26px 16px}.grid{grid-template-columns:1fr}}
</style><main><header><div class="eyebrow">CREATIVE LIBRARY · 28 OCT 2026</div><h1>Alien Pet:<br>Escape Puzzle</h1><p>Ba hướng quảng cáo với nhận diện mới: giải đố, tình bạn và nuôi pet vui nhộn.</p></header>'''
page+='<div class="grid">'+''.join(card(x) for x in manifest[:3])+'</div>'
page+='<div class="info">3 bản mới tiếng Anh · 9:16 · 1080×1920 · 30fps. Các video cũ được giữ riêng bên dưới để tránh chọn nhầm khi chạy ads.</div>'
page+='<details><summary>4 quảng cáo cũ — cần cập nhật tên game</summary><div class="grid">'+''.join(card(x) for x in manifest[3:7])+'</div></details>'
page+='<details><summary>2 bản xuất tham khảo</summary><div class="grid">'+''.join(card(x) for x in manifest[7:])+'</div></details>'
page+='<footer><a href="ad-copy.txt">Nội dung quảng cáo tiếng Anh</a> · <a href="manifest.json">Thông số và checksum</a> · <a href="README.md">Hướng dẫn sử dụng / nguồn</a><p>AI CINEMATIC và ACTUAL GAMEPLAY được ghi rõ trong các bản mới. Các file được tập hợp để chuẩn bị test; chưa đăng campaign.</p></footer></main></html>'
(KIT/'index.html').write_text(page)

checks=KIT/'Checks';checks.mkdir(exist_ok=True)
for p in [DOC/'qa/validation.json',DOC/'qa/AlienPet_A_ESCAPE_22s_1080x1920-contact.jpg',DOC/'qa/AlienPet_B_BEST_FRIEND_24s_1080x1920-contact.jpg']:
 shutil.copy2(p,checks/p.name)
shutil.copy2(R/'qa/validation.json',checks/'C-validation.json')
shutil.copy2(R/'qa/AlienPet_C_CUTE_PET_24s_1080x1920-contact.jpg',checks/'C-contact.jpg')
shutil.copy2(R/'native-frames/PASS.txt',checks/'C-capture-result.txt')

sources=KIT/'Sources_C';sources.mkdir(exist_ok=True)
for sub in ['sources','native-frames']:(sources/sub).mkdir(exist_ok=True)
for p in (R/'sources').glob('*.mp4'):shutil.copy2(p,sources/'sources'/p.name)
for p in (R/'native-frames').glob('*.txt'):shutil.copy2(p,sources/'native-frames'/p.name)
shutil.copy2(R/'C-edit-manifest.json',sources/'C-edit-manifest.json')
shutil.copy2(R/'edit_pet_ad.py',sources/'edit_pet_ad.py')
(sources/'REBUILD.txt').write_text('Source trims are in C-edit-manifest.json. Script is designed to run at Artifacts/AlienPetPetAd-2026-10-28 inside the game repository; ROOT must point at the repo for Manrope fonts and COgheAudio effects. Capture source is 1135 frames at 30fps. The dev-only capture entrypoint is AlienPetCareReel.cs, flag -alien-pet-care-reel <directory>. No unlock/purchase is written to saves.\n')

with zipfile.ZipFile(ZIP,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in sorted(KIT.rglob('*')):
  if p.is_file():z.write(p,Path(KIT.name)/p.relative_to(KIT))
with zipfile.ZipFile(ZIP) as z:
 assert z.testzip() is None
 for x in manifest:
  assert hashlib.sha256(z.read(KIT.name+'/'+x['file'])).hexdigest()==x['sha256']
 print('ZIP VERIFIED',len(z.namelist()),'files',ZIP.stat().st_size,'bytes',flush=True)
(R/'kit-validation.json').write_text(json.dumps(dict(zip=str(ZIP),zip_sha256=digest(ZIP),video_count=9,current_brand_count=3,all_video_checksums_match=True),indent=2))
print(ZIP,flush=True)

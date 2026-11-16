/* Composition only: no redraw/retouch of runtime images. Portable, offline assets. */
const fs=require('fs'), path=require('path'), crypto=require('crypto');
const {pathToFileURL}=require('url');
const resolveModule=n=>process.env.ALIEN_PET_NODE_MODULES?require(path.join(process.env.ALIEN_PET_NODE_MODULES,n)):require(n);
const sharp=resolveModule('sharp'), {chromium}=resolveModule('playwright');
const root=path.resolve(__dirname,'..'), cfg=JSON.parse(fs.readFileSync(path.join(root,'screenshots.json'),'utf8'));
const final=process.argv.includes('--final');
if(final&&cfg.shots.some(s=>!s.iosVerified||!['iOS device','iOS simulator'].includes(s.provenance)||!s.buildVersion||!s.deviceModel||!s.captureDate))
 throw Error('FINAL BLOCKED: replace every reference with verified iOS captures and record buildVersion/deviceModel/captureDate. See SCREENSHOTS.md.');
const esc=s=>String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const colors={mint:['#f4f7ec','#274e51','#bfe4d6'],coral:['#fff7ee','#344f52','#efcec1'],lavender:['#f7f5f7','#384e59','#dcd8ef']};
function layout(s){
 const [w,h]=cfg.targets[s.device],ipad=s.device==='ipad',p=colors[s.theme];
 const crop=s.crop||{x:0,y:0,width:s.sourceWidth,height:s.sourceHeight};
 const box={x:w*.045,y:ipad?470:550,w:w*.91,h:h-(ipad?650:770)};
 const scale=Math.min(box.w/crop.width,box.h/crop.height),iw=crop.width*scale,ih=crop.height*scale;
 return `<!doctype html><meta charset="utf-8"><style>
 @font-face{font-family:M;src:url('../design-assets/Manrope.ttf')}@font-face{font-family:M;src:url('../design-assets/ManropeBold.ttf');font-weight:700}
 *{box-sizing:border-box}html,body{margin:0;width:${w}px;height:${h}px;overflow:hidden}body{font-family:M,sans-serif;background:${p[0]};color:${p[1]}}
 .wash{position:absolute;inset:0;background:radial-gradient(ellipse at 98% 0%,${p[2]}bc,transparent 57%)}
 header{position:absolute;left:${w*.055}px;right:${w*.055}px;top:65px}.brand{display:flex;justify-content:space-between;align-items:center;font-size:${ipad?32:26}px;font-weight:700;letter-spacing:.12em}.brand b{font-size:${ipad?42:38}px;letter-spacing:-.04em}
 .rule{height:2px;background:${p[1]}26;margin:25px 0}h1{font-size:${ipad?102:112}px;line-height:1.04;letter-spacing:-.055em;margin:30px 0 0;font-weight:700}
 .picture{position:absolute;left:${box.x+(box.w-iw)/2}px;top:${box.y+(box.h-ih)/2}px;width:${iw}px;height:${ih}px;overflow:hidden;border-radius:24px;box-shadow:0 14px 42px ${p[1]}0b}
 .picture img{position:absolute;left:${-crop.x*scale}px;top:${-crop.y*scale}px;width:${s.sourceWidth*scale}px;height:${s.sourceHeight*scale}px;max-width:none}
 footer{position:absolute;left:${w*.055}px;right:${w*.055}px;bottom:78px;font-size:${ipad?33:32}px;line-height:1.3;font-weight:700}.draft{position:absolute;bottom:24px;left:0;width:100%;text-align:center;font-size:${ipad?23:20}px;color:#7c4e3e;letter-spacing:.015em}
 </style><div class="wash"></div><header><div class="brand"><b>Alien Pet</b><span>ESCAPE PUZZLE</span></div><div class="rule"></div><h1>${s.headline.map(esc).join('<br>')}</h1></header><div class="picture"><img src="../${esc(s.source)}"></div><footer>${esc(s.detail)}</footer>${final?'':'<div class="draft">LAYOUT DRAFT · Replace reference with iOS capture before upload</div>'}`;
}
(async()=>{
 const opts={headless:true};if(process.env.ALIEN_PET_CHROMIUM)opts.executablePath=process.env.ALIEN_PET_CHROMIUM;
 const browser=await chromium.launch(opts),audit=[];
 fs.mkdirSync(path.join(root,'layouts'),{recursive:true});
 try{for(const s of cfg.shots){
  const src=path.resolve(root,s.source);if(!src.startsWith(root+path.sep))throw Error('Source outside handoff');
  const m=await sharp(src).metadata();if(m.width!==s.sourceWidth||m.height!==s.sourceHeight)throw Error('Source size mismatch: '+s.id);
  if(s.crop&&(s.crop.x<0||s.crop.y<0||s.crop.width<=0||s.crop.height<=0||s.crop.x+s.crop.width>m.width||s.crop.y+s.crop.height>m.height))throw Error('Invalid crop');
  const [w,h]=cfg.targets[s.device],html=path.join(root,'layouts',s.device+'-'+s.id+'.html');fs.writeFileSync(html,layout(s));
  const pg=await browser.newPage({viewport:{width:w,height:h},deviceScaleFactor:1});
  await pg.goto(pathToFileURL(html).href);await pg.evaluate(()=>document.fonts.ready);
  await pg.waitForFunction(()=>[...document.images].every(i=>i.complete&&i.naturalWidth>0));
  const check=await pg.evaluate(()=>{const r=s=>document.querySelector(s).getBoundingClientRect();return {headerBottom:r('header').bottom,pictureTop:r('.picture').top,pictureBottom:r('.picture').bottom,footerTop:r('footer').top,headingWidth:r('h1').width,headingScroll:document.querySelector('h1').scrollWidth};});
  if(check.headerBottom>=check.pictureTop||check.pictureBottom>=check.footerTop||check.headingScroll>check.headingWidth+1)throw Error('Layout overlap: '+s.id);
  const rel=`screenshots/${final?'upload':'draft'}/${s.device}/${s.id}.png`,out=path.join(root,rel);fs.mkdirSync(path.dirname(out),{recursive:true});
  const png=await pg.screenshot({animations:'disabled'});await sharp(png).removeAlpha().png().toFile(out);await pg.close();
  audit.push({file:rel,width:w,height:h,source:s.source,provenance:s.provenance,iosVerified:s.iosVerified,...check,sha256:crypto.createHash('sha256').update(fs.readFileSync(out)).digest('hex')});
 }}finally{await browser.close();}
 await sharp(path.join(root,'sources/icon-approved-c.png')).resize(1024,1024,{fit:'contain'}).removeAlpha().png().toFile(path.join(root,'assets/icon/AppIcon-1024.png'));
 // Xcode single-size universal iOS icon catalog; retain Unity-generated catalog if it has additional slots.
 const set=path.join(root,'assets/icon/AppIcon.appiconset');fs.mkdirSync(set,{recursive:true});fs.copyFileSync(path.join(root,'assets/icon/AppIcon-1024.png'),path.join(set,'AppIcon-1024.png'));
 fs.writeFileSync(path.join(set,'Contents.json'),JSON.stringify({images:[{filename:'AppIcon-1024.png',idiom:'universal',platform:'ios',size:'1024x1024'}],info:{author:'xcode',version:1}},null,2)+'\n');
 fs.writeFileSync(path.join(root,'qa/render-audit.json'),JSON.stringify(audit,null,2)+'\n');
 const thumbs=await Promise.all(audit.map(a=>sharp(path.join(root,a.file)).resize(240,520,{fit:'contain',background:'#edf1ec'}).png().toBuffer()));
 await sharp({create:{width:1200,height:1040,channels:3,background:'#edf1ec'}}).composite(thumbs.map((input,i)=>({input,left:(i%5)*240,top:Math.floor(i/5)*520}))).png().toFile(path.join(root,'qa/contact-sheet.png'));
 const cards=kind=>audit.filter(a=>a.file.includes('/'+kind+'/')).map(a=>`<a href="${esc(a.file)}"><img src="${esc(a.file)}"><span>${esc(path.basename(a.file))}<br>${a.width} × ${a.height}</span></a>`).join('');
 fs.writeFileSync(path.join(root,'review.html'),`<!doctype html><html lang="vi"><meta charset="utf-8"><title>Alien Pet App Store handoff</title><style>body{font:17px system-ui;background:#f7f8ef;color:#294b50;max-width:1450px;margin:45px auto;padding:24px}h1{font-size:42px}a{color:#245d65}.notice{background:#fff0db;padding:24px;border-radius:14px;line-height:1.6}.grid{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:25px}.grid img{width:100%;border-radius:14px}.grid a{font-size:14px;text-decoration:none}.icon{width:160px;border-radius:30px;float:right}nav{line-height:2.3;margin:30px 0}@media(max-width:750px){.grid{grid-template-columns:repeat(2,1fr)}}</style><img class="icon" src="assets/icon/AppIcon-1024.png"><h1>Alien Pet · App Store</h1><p>Bộ bàn giao ngày 16/11/2026 · iPhone + iPad · en-US</p><div class="notice"><b>${final?'iOS captures composed — verify final build parity before submission':'Bố cục screenshot hoàn chỉnh; chưa dùng để upload'}</b><br>Icon C giữ nguyên thiết kế đã duyệt. ${final?'Ảnh đã được render từ các nguồn được người vận hành đánh dấu iOS verified. Kiểm tra nguồn, build và thiết bị trong audit trước khi upload.':'Ảnh dưới là gameplay thật từ bộ nguồn Android/Unity hiện có, không phải ảnh chụp iOS. Máy phát hành thay bằng capture iPhone/iPad đúng bản build, rồi chạy renderer --final để bỏ nhãn nháp.'}<br>Firebase/AdMob iOS, IAP, signing và kiểm thử TestFlight còn là các cổng kiểm tra trước phát hành. Không có IPA hay chứng chỉ trong gói.</div><nav><a href="START-HERE.md">Bắt đầu từ đây</a> · <a href="RELEASE-PLAN.md">Kế hoạch phát hành</a> · <a href="SCREENSHOTS.md">Capture và xuất ảnh</a> · <a href="technical/IOS-GAPS.md">Các việc kỹ thuật còn thiếu</a> · <a href="metadata/en-US/listing.json">Nội dung store</a> · <a href="QA-CHECKLIST.csv">Checklist</a></nav><h2>iPhone · 1320 × 2868</h2><div class="grid">${cards('iphone')}</div><h2>iPad · 2064 × 2752</h2><div class="grid">${cards('ipad')}</div></html>`);
 console.log(JSON.stringify({rendered:audit.length,mode:final?'ios-composition':'reference-draft',icon:'assets/icon/AppIcon-1024.png'}));
})().catch(e=>{console.error(e);process.exit(1)});

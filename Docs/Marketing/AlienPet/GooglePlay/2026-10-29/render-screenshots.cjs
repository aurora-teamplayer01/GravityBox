/* Reproducible local HTML/CSS composition. Source screenshots are only displayed,
 * crop-framed and uniformly scaled; no retouch, redraw or AI editing is used. */
const fs = require('fs');
const crypto = require('crypto');
const path = require('path');
const { pathToFileURL } = require('url');
const deps = process.env.ALIEN_PET_NODE_MODULES || '/Users/mrk/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules';
const { chromium } = require(path.join(deps, 'playwright'));
const sharp = require(path.join(deps, 'sharp'));
const browserOptions={headless:true};
const cachedBrowser='/Users/mrk/Library/Caches/ms-playwright/chromium_headless_shell-1223/chrome-headless-shell-mac-arm64/chrome-headless-shell';
if(process.env.ALIEN_PET_CHROMIUM)browserOptions.executablePath=process.env.ALIEN_PET_CHROMIUM;
else if(fs.existsSync(cachedBrowser))browserOptions.executablePath=cachedBrowser;
const root = __dirname;
const config = JSON.parse(fs.readFileSync(path.join(root, 'screenshots.config.json'), 'utf8'));
const esc = s => String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const palettes = {
  mint:{bg:'#f6f8ed',ink:'#244d50',accent:'#bfe6d8',sub:'#497473'},
  coral:{bg:'#fff7ed',ink:'#344f52',accent:'#f4ccc1',sub:'#6a7573'},
  lavender:{bg:'#f7f5f7',ink:'#394e59',accent:'#ddd6ef',sub:'#6c7382'}
};
function page(item,kind){
 const tablet=kind!=='phone'; const w=tablet?1440:1080,h=tablet?2560:1920;
 const p=palettes[item.theme]||palettes.mint;
 const crop=item.crop||{x:0,y:0,width:item.sourceWidth,height:item.sourceHeight};
 // Phone: every original pixel in the documented rectangle is included. Tablet: whole source.
 const innerW=tablet?1350:1016, innerH=tablet?2040:1375;
 const src=tablet?{x:0,y:0,width:item.sourceWidth,height:item.sourceHeight}:crop;
 const scale=Math.min(innerW/src.width,innerH/src.height);
 const frameW=src.width*scale,frameH=src.height*scale;
 const frameX=(w-frameW)/2, frameY=tablet?430:400+(1375-frameH)/2;
 return `<!doctype html><html lang="en"><meta charset="utf-8"><title>${esc(item.id)}</title><style>
 @font-face{font-family:Manrope;src:url('design-assets/Manrope.ttf')}@font-face{font-family:Manrope;src:url('design-assets/ManropeBold.ttf');font-weight:700}
 *{box-sizing:border-box}html,body{margin:0;width:${w}px;height:${h}px;overflow:hidden}body{font-family:Manrope,sans-serif;background:${p.bg};color:${p.ink};position:relative}
 .wash{position:absolute;inset:0;background:radial-gradient(ellipse at 96% 7%,${p.accent}91,transparent 46%);pointer-events:none}
 .header{position:absolute;left:${tablet?84:64}px;right:${tablet?84:64}px;top:54px}
 .brand{display:flex;align-items:center;justify-content:space-between;font-size:${tablet?24:18}px;font-weight:700;letter-spacing:.14em;color:${p.sub}}
 .brand .mark{font-size:${tablet?30:24}px;letter-spacing:-.035em;color:${p.ink}}.rule{height:2px;background:${p.ink}22;margin-top:${tablet?26:20}px}
 h1{font-size:${tablet?96:84}px;line-height:1.07;letter-spacing:-.052em;font-weight:700;margin:${tablet?20:27}px 0 0;max-width:100%;white-space:nowrap}
 .eyebrow{font-size:${tablet?18:14}px;letter-spacing:.10em;font-weight:700;color:${p.sub};margin-top:${tablet?16:16}px}
 .picture{position:absolute;left:${frameX}px;top:${frameY}px;width:${frameW}px;height:${frameH}px;overflow:hidden;border-radius:${tablet?20:28}px;background:#f6f8ef}
 .picture img{position:absolute;max-width:none;left:${-src.x*scale}px;top:${-src.y*scale}px;width:${item.sourceWidth*scale}px;height:${item.sourceHeight*scale}px;image-rendering:auto}
 .footer{position:absolute;left:${tablet?70:64}px;right:${tablet?70:64}px;bottom:${tablet?24:34}px;display:flex;gap:20px;align-items:center;justify-content:space-between;color:${p.sub}}
 .detail{font-size:${tablet?20:25}px;line-height:1.3;font-weight:700;letter-spacing:-.025em}.footnote{font-size:${tablet?16:14}px;letter-spacing:.09em;font-weight:700;white-space:nowrap;color:${p.sub}bb}
 </style><div class="wash"></div><header class="header"><div class="brand"><span class="mark">Alien Pet</span><span>ESCAPE PUZZLE</span></div><div class="rule"></div><h1>${item.headline.map(esc).join('<br>')}</h1><div class="eyebrow">${esc(item.eyebrow)}</div></header><div class="picture"><img src="${esc(item.image)}" alt="Unretouched actual game capture"></div><footer class="footer"><div class="detail">${esc(item.detail)}</div><div class="footnote">ACTUAL GAME</div></footer></html>`;
}
async function main(){
 for(const d of ['exports/phone','exports/tablet-7in','exports/tablet-10in','qa'])fs.mkdirSync(path.join(root,d),{recursive:true});
 const browser=await chromium.launch(browserOptions);
 const proof=[];
 try{
  for(const kind of ['phone','tablet-7in','tablet-10in']){
   const rows=config[kind];const w=kind==='phone'?1080:1440,h=kind==='phone'?1920:2560;
   for(const item of rows){
    const sm=await sharp(path.join(root,item.image)).metadata();
    if(sm.width!==item.sourceWidth||sm.height!==item.sourceHeight)throw new Error(`Source dimensions differ: ${item.id}`);
    if(kind!=='phone'&&(sm.width<1200||sm.height<1200))throw new Error(`Tablet source is not a native large-screen capture: ${item.id}`);
    if(kind==='phone'&&item.crop&&(item.crop.x<0||item.crop.y<0||item.crop.x+item.crop.width>sm.width||item.crop.y+item.crop.height>sm.height))throw new Error(`Crop outside source: ${item.id}`);
    const html=`layout-${kind}-${item.id}.html`;fs.writeFileSync(path.join(root,html),page(item,kind));
    const pg=await browser.newPage({viewport:{width:w,height:h},deviceScaleFactor:1});
    await pg.goto(pathToFileURL(path.join(root,html)).href);await pg.evaluate(()=>document.fonts.ready);await pg.waitForFunction(()=>[...document.images].every(i=>i.complete&&i.naturalWidth));
    const dir=kind;const target=path.join(root,'exports',dir,`${item.id}.png`);
    await pg.screenshot({path:target,fullPage:false,animations:'disabled',omitBackground:false});
    const dom=await pg.evaluate(()=>({fontsLoaded:document.fonts.status,heading:{width:document.querySelector('h1').getBoundingClientRect().width,scrollWidth:document.querySelector('h1').scrollWidth},headerBottom:document.querySelector('.header').getBoundingClientRect().bottom,footerTop:document.querySelector('.footer').getBoundingClientRect().top,picture:{x:document.querySelector('.picture').offsetLeft,y:document.querySelector('.picture').offsetTop,width:document.querySelector('.picture').offsetWidth,height:document.querySelector('.picture').offsetHeight}}));
    if(dom.heading.scrollWidth>dom.heading.width||dom.headerBottom>dom.picture.y||dom.picture.y+dom.picture.height>dom.footerTop)throw new Error(`Layout collision: ${item.id}`);
    const meta=await sharp(target).metadata();proof.push({id:item.id,kind,file:path.relative(root,target),width:meta.width,height:meta.height,hasAlpha:meta.hasAlpha,source:item.image,crop:kind!=='phone'?'whole source contain-fit':item.crop,note:item.note||item.provenance,...dom});await pg.close();
   }
  }
 }finally{await browser.close();}
 const icon=path.resolve(root,'../../../../../Assets/_Game/Branding/COghe-AppIcon.png');
 await sharp(icon).resize(512,512,{fit:'fill',kernel:'lanczos3'}).removeAlpha().png({compressionLevel:9}).toFile(path.join(root,'exports','app-icon-512.png'));
 proof.push({id:'approved-icon-c',file:'exports/app-icon-512.png',source:icon,width:512,height:512,transform:'Mechanical whole-image 512x512 Lanczos resize; no crop, retouch, text, border or rounding.'});
 const feature=path.join(root,'sources','feature-art-master.png');
 if(fs.existsSync(feature)){
  await sharp(feature).resize(1024,500,{fit:'fill',kernel:'lanczos3'}).removeAlpha().png({compressionLevel:9}).toFile(path.join(root,'exports','feature-graphic.png'));
  proof.push({id:'illustrative-feature-graphic',file:'exports/feature-graphic.png',source:feature,width:1024,height:500,transform:'Mechanical whole-image resize to Google Play 1024x500; no crop or creative edit. Root-approved illustrative key art, separate from actual gameplay screenshots.'});
 }
 for(const item of proof){
  const output=path.join(root,item.file),source=path.isAbsolute(item.source)?item.source:path.join(root,item.source);
  const m=await sharp(output).metadata();item.hasAlpha=m.hasAlpha;item.bytes=fs.statSync(output).size;item.sha256=crypto.createHash('sha256').update(fs.readFileSync(output)).digest('hex');item.sourceSha256=crypto.createHash('sha256').update(fs.readFileSync(source)).digest('hex');
  if(m.hasAlpha||m.width!==item.width||m.height!==item.height)throw new Error(`Export validation failed: ${item.id}`);
 }
 fs.writeFileSync(path.join(root,'qa','screenshot-render-audit.json'),JSON.stringify(proof,null,2)+'\n');
 const cards=['phone','tablet-7in','tablet-10in'].filter(kind=>proof.some(x=>x.kind===kind)).map(kind=>`<section><h2>${kind==='phone'?'Phone · 6 screenshots':kind==='tablet-7in'?'7-inch tablet · actual native capture':'10-inch tablet · actual native capture'}</h2><div class="shots ${kind==='phone'?'':'tablet'}">${proof.filter(x=>x.kind===kind).map(x=>`<article><a href="${esc(x.file)}"><img src="${esc(x.file)}"></a><p>${esc(x.id)}</p><small>${x.width} × ${x.height}</small></article>`).join('')}</div></section>`).join('');
 const hero=`<section class="hero"><div><img class="icon" src="exports/app-icon-512.png"><p>Approved icon C · 512 × 512</p></div>${fs.existsSync(feature)?'<div><img class="feature" src="exports/feature-graphic.png"><p>Illustrative feature art · 1024 × 500 · Separate from actual gameplay screenshots</p></div>':''}</section>`;
 fs.writeFileSync(path.join(root,'screenshot-review.html'),`<!doctype html><html lang="en"><meta charset="utf-8"><title>Alien Pet — Google Play screenshot review</title><style>@font-face{font-family:M;src:url('design-assets/Manrope.ttf')}body{font-family:M,sans-serif;background:#eef2eb;color:#294c50;margin:0;padding:38px}h1{margin:0 0 8px;font-size:30px}p{font-size:13px}main{margin-top:28px}h2{font-size:22px;margin:30px 0 16px}.shots{display:grid;grid-template-columns:repeat(6,minmax(0,1fr));gap:16px}.shots.tablet{grid-template-columns:repeat(4,minmax(0,1fr))}article{min-width:0}img{width:100%;display:block;border-radius:12px;box-shadow:0 3px 16px #20383a12}small{color:#64807d}.hero{display:flex;gap:34px;align-items:flex-start;margin-top:24px}.hero .icon{width:170px;height:170px}.hero .feature{width:500px;height:auto}@media(max-width:1100px){.shots,.shots.tablet{grid-template-columns:repeat(3,1fr)}}</style><h1>Alien Pet: Escape Puzzle</h1><p>Google Play source-faithful screenshots · 29 October 2026 · Click each image for its full-resolution export.</p>${hero}<main>${cards}</main></html>`);
 const browser2=await chromium.launch(browserOptions);try{const pg=await browser2.newPage({viewport:{width:1800,height:900},deviceScaleFactor:1});await pg.goto(pathToFileURL(path.join(root,'screenshot-review.html')).href);await pg.evaluate(()=>document.fonts.ready);await pg.waitForFunction(()=>[...document.images].every(i=>i.complete&&i.naturalWidth));await pg.screenshot({path:path.join(root,'qa','screenshot-contact-sheet.png'),fullPage:true});}finally{await browser2.close();}
 console.log(`Rendered ${proof.filter(x=>x.kind==='phone').length} phone images, ${proof.filter(x=>x.kind==='tablet-7in').length} native 7in and ${proof.filter(x=>x.kind==='tablet-10in').length} native 10in compositions, approved icon and review contact sheet.`);
}
main().catch(e=>{console.error(e);process.exit(1)});

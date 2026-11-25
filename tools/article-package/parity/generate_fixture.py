#!/usr/bin/env python3
"""Generate the reviewed two-article package fixture deterministically."""
from __future__ import annotations
import hashlib, json, shutil, struct, zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
SOURCE=ROOT/'content/articles'
OUT=Path(__file__).resolve().parent/'fixtures/articles-parity.zip'
PIN=Path(__file__).resolve().parent/'pin.json'
PUBLICATION_ID='fixture-2026.11.24.1'
PROVIDER_COMMIT='c59f549ee2ea93ca84dbd8e17c5330f06edad1e2'

def write_json(path:Path,obj:object)->None:
 path.parent.mkdir(parents=True,exist_ok=True); path.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def sha(data:bytes)->str:return hashlib.sha256(data).hexdigest()
def main()->int:
 work=OUT.parent/'.fixture-stage'; shutil.rmtree(work,ignore_errors=True); work.mkdir(parents=True)
 articles=json.loads((SOURCE/'articles.json').read_text(encoding='utf-8'))
 series=json.loads((SOURCE/'series.json').read_text(encoding='utf-8'))
 selected=sorted((a for a in articles['articles'] if a['status']=='published'),key=lambda a:(a['publishedDate'],a['id']),reverse=True)
 if [a['slug'] for a in selected]!=['sycophancy','bias']: raise SystemExit('unexpected reviewed article set')
 write_json(work/'articles.json',{'schemaVersion':'1.0','articles':selected}); write_json(work/'series.json',series)
 projection=[]
 for a in selected:
  body=(SOURCE/a['bodyPath']).read_bytes(); bp=work/'bodies'/f"{a['slug']}.md"; bp.parent.mkdir(parents=True,exist_ok=True); bp.write_bytes(body)
  image=ROOT/'src/Ulfbou.Site/wwwroot/assets/social/articles'/Path(a['socialImage']).name; data=image.read_bytes()
  if data[:8]!=b'\x89PNG\r\n\x1a\n' or struct.unpack('>II',data[16:24])!=(1200,627): raise SystemExit('invalid social image')
  ip=work/'assets/social/articles'/image.name; ip.parent.mkdir(parents=True,exist_ok=True); ip.write_bytes(data)
  projection.append({'sourceId':a['id'],'sourcePath':f"articles.json#{a['id']}",'bodySourcePath':a['bodyPath'],'status':'published','slug':a['slug'],'canonicalPath':a['canonicalPath'],'outputPath':f"articles/{a['slug']}/index.html",'title':a['title'],'description':a['description'],'publishedDate':a['publishedDate'],'modifiedDate':a['modifiedDate'],'seriesId':a['seriesId'],'seriesPart':a['seriesPart'],'contentSha256':sha(body)})
 write_json(work/'data/generated/articles.json',{'schemaVersion':'1.0','articles':projection})
 files=[]
 for p in sorted(x for x in work.rglob('*') if x.is_file()):
  data=p.read_bytes(); files.append({'path':p.relative_to(work).as_posix(),'bytes':len(data),'sha256':sha(data)})
 write_json(work/'manifest.json',{'schemaVersion':'1.0','product':'articles','files':files})
 OUT.parent.mkdir(parents=True,exist_ok=True)
 with zipfile.ZipFile(OUT,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
  for p in sorted(x for x in work.rglob('*') if x.is_file()):
   info=zipfile.ZipInfo(p.relative_to(work).as_posix(),(1980,1,1,0,0,0)); info.compress_type=zipfile.ZIP_DEFLATED; info.external_attr=0o100644<<16; info.create_system=3
   z.writestr(info,p.read_bytes(),compress_type=zipfile.ZIP_DEFLATED,compresslevel=9)
 digest=sha(OUT.read_bytes())
 pin={'schemaVersion':'1.0','providerRepository':'ulfbou/content','providerCommit':PROVIDER_COMMIT,'releaseTag':f'articles-{PUBLICATION_ID}','publicationId':PUBLICATION_ID,'assetName':f'articles-{PUBLICATION_ID}.zip','archiveSha256':digest,'contractVersion':'1.0','product':'articles','expectedSlugs':['bias','sycophancy'],'fixtureOnly':True}
 write_json(PIN,pin); shutil.rmtree(work)
 print(json.dumps({'archive':str(OUT.relative_to(ROOT)),'sha256':digest,'bytes':OUT.stat().st_size},sort_keys=True)); return 0
if __name__=='__main__':raise SystemExit(main())

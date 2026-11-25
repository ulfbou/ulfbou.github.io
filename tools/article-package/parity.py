#!/usr/bin/env python3
"""Deterministic, non-production article package parity gate."""
from __future__ import annotations
import argparse, hashlib, html.parser, json, re, shutil, stat, struct, subprocess, sys, urllib.parse, zipfile
from pathlib import Path, PurePosixPath
from typing import Any
import xml.etree.ElementTree as ET
HERE=Path(__file__).resolve().parent; sys.path.insert(0,str(HERE/'article-package'))
import consumer
VERDICT_OK='FIXTURE PARITY PROVEN'; VERDICT_BAD='FIXTURE PARITY NOT PROVEN'
PIN_FIELDS={'schemaVersion','providerRepository','providerCommit','releaseTag','publicationId','assetName','archiveSha256','contractVersion','product','expectedSlugs','fixtureOnly'}
class ParityError(ValueError):pass

def digest_file(p:Path)->str:
 h=hashlib.sha256();
 with p.open('rb') as f:
  for b in iter(lambda:f.read(1024*1024),b''):h.update(b)
 return h.hexdigest()
def tree_inventory(p:Path)->list[dict[str,Any]]:
 if not p.exists():return []
 return [{'path':x.relative_to(p).as_posix(),'bytes':x.stat().st_size,'sha256':digest_file(x)} for x in sorted(y for y in p.rglob('*') if y.is_file())]
def inventory_digest(v:list[dict[str,Any]])->str:return hashlib.sha256((json.dumps(v,sort_keys=True,separators=(',',':'))+'\n').encode()).hexdigest()
def load_pin(p:Path)->dict[str,Any]:
 try:v=json.loads(p.read_text(encoding='utf-8'))
 except Exception as e:raise ParityError(f'invalid pin: {e}') from e
 if not isinstance(v,dict) or set(v)!=PIN_FIELDS:raise ParityError('pin is not the closed 1.0 contract')
 if v['schemaVersion']!='1.0' or v['contractVersion']!='1.0':raise ParityError('unsupported pin or package contract')
 if v['providerRepository']!='ulfbou/content':raise ParityError('wrong provider repository')
 if not re.fullmatch(r'[0-9a-f]{40}',v['providerCommit']):raise ParityError('provider commit must be a full lowercase SHA')
 if not isinstance(v['publicationId'],str) or not re.fullmatch(r'fixture-[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[1-9][0-9]*',v['publicationId']):raise ParityError('invalid fixture publication identity')
 if v['releaseTag']!=f"articles-{v['publicationId']}":raise ParityError('release tag mismatch')
 if v['assetName']!=f"articles-{v['publicationId']}.zip":raise ParityError('asset name mismatch')
 if not re.fullmatch(r'[0-9a-f]{64}',v['archiveSha256']):raise ParityError('invalid archive SHA-256')
 if v['product']!='articles' or v['fixtureOnly'] is not True:raise ParityError('wrong product or non-fixture pin')
 if v['expectedSlugs']!=['bias','sycophancy']:raise ParityError('wrong expected slugs')
 return v
def safe_member(info:zipfile.ZipInfo)->str:
 n=info.filename; p=PurePosixPath(n)
 if not n or p.is_absolute() or '..' in p.parts or '.' in p.parts or '\\' in n:raise ParityError(f'unsafe ZIP path: {n!r}')
 mode=(info.external_attr>>16)&0o170000
 if n.endswith('/') or mode not in (0,stat.S_IFREG):raise ParityError(f'unsafe ZIP member type: {n!r}')
 return p.as_posix()
def extract_verified(archive:Path,pin:dict[str,Any],target:Path)->Path:
 if archive.name!='articles-parity.zip':raise ParityError('fixture archive path identity mismatch')
 observed=digest_file(archive)
 if observed!=pin['archiveSha256']:raise ParityError('archive SHA-256 mismatch before extraction')
 target.mkdir(parents=True)
 with zipfile.ZipFile(archive) as z:
  seen=set()
  for i in z.infolist():
   n=safe_member(i)
   if n in seen:raise ParityError(f'duplicate ZIP member: {n}')
   seen.add(n)
  for i in z.infolist():
   n=PurePosixPath(i.filename); out=target.joinpath(*n.parts); out.parent.mkdir(parents=True,exist_ok=True)
   with z.open(i) as src,out.open('wb') as dst:shutil.copyfileobj(src,dst)
 return target

def semantic_package(package:Path,pin:dict[str,Any])->dict[str,Any]:
 verified=consumer.verify(package); slugs=sorted(verified['slugs'])
 if slugs!=pin['expectedSlugs']:raise ParityError('package article set inconsistent with pin')
 articles=json.loads((package/'articles.json').read_text(encoding='utf-8'))['articles']; proj=json.loads((package/'data/generated/articles.json').read_text(encoding='utf-8'))['articles']
 by_slug={a['slug']:a for a in articles}; pp={a['slug']:a for a in proj}
 if set(by_slug)!=set(pp):raise ParityError('article/projection disagreement')
 for s,a in by_slug.items():
  q=pp[s]; body=package/'bodies'/f'{s}.md'; image=package/'assets/social/articles'/Path(a['socialImage']).name
  if not body.is_file():raise ParityError(f'missing body: {s}')
  if not image.is_file():raise ParityError(f'missing social image: {s}')
  expected={'sourceId':a['id'],'status':'published','slug':s,'canonicalPath':a['canonicalPath'],'outputPath':f'articles/{s}/index.html','title':a['title'],'description':a['description'],'publishedDate':a['publishedDate'],'modifiedDate':a['modifiedDate'],'seriesId':a['seriesId'],'seriesPart':a['seriesPart'],'contentSha256':digest_file(body)}
  if any(q.get(k)!=v for k,v in expected.items()):raise ParityError(f'article/projection disagreement: {s}')
 return verified
class DOM(html.parser.HTMLParser):
 def __init__(self):super().__init__(convert_charrefs=True);self.stack=[];self.events=[];self.meta=[];self.links=[];self.jsonld=[];self._script=False;self._buf=[]
 def handle_starttag(self,t,a):
  d=dict(a);self.stack.append(t);self.events.append(('start',t,tuple(sorted((k,v or '') for k,v in a))))
  if t=='meta':self.meta.append((d.get('name') or d.get('property'),d.get('content')))
  if t in ('a','link'):self.links.append((t,d.get('rel'),d.get('href'),d.get('class')))
  if t=='script' and d.get('type')=='application/ld+json':self._script=True;self._buf=[]
 def handle_endtag(self,t):
  self.events.append(('end',t));
  if t=='script' and self._script:self.jsonld.append(json.loads(''.join(self._buf)));self._script=False
  if self.stack:self.stack.pop()
 def handle_data(self,d):
  if self._script:self._buf.append(d)
  elif d.strip():self.events.append(('text',' '.join(d.split())))
def html_semantic(p:Path)->dict[str,Any]:
 q=DOM();q.feed(p.read_text(encoding='utf-8'));return {'events':q.events,'meta':q.meta,'links':q.links,'jsonLd':q.jsonld}
def xml_semantic(p:Path)->Any:
 def n(e):return (e.tag,tuple(sorted(e.attrib.items())),(e.text or '').strip(),tuple(n(c) for c in e))
 return n(ET.parse(p).getroot())
def render(repo:Path,source:Path,site:Path)->None:
 shutil.copytree(repo/'src/Ulfbou.Site/wwwroot',site,dirs_exist_ok=True)
 subprocess.run(['dotnet','run','--project',str(repo/'src/Ulfbou.Articles/Ulfbou.Articles.csproj'),'--',str(source),str(site/'articles'),str(site)],check=True,cwd=repo)
def compare(repo:Path,local:Path,package:Path,local_site:Path,external_site:Path)->tuple[list[dict[str,Any]],list[dict[str,Any]],list[str]]:
 source=external_site.parent/'source'; consumer.prepare(package,source,external_site)
 render(repo,local,local_site); render(repo,source,external_site)
 articles=json.loads((local/'articles.json').read_text(encoding='utf-8'))['articles']; slugs=[a['slug'] for a in sorted((x for x in articles if x['status']=='published'),key=lambda a:(a['publishedDate'],a['id']),reverse=True)]
 checks=[]
 def check(name,method,a,b):checks.append({'name':name,'method':method,'result':'PASS' if a==b else 'FAIL'});return a==b
 check('article-index','parsed semantic HTML',html_semantic(local_site/'articles/index.html'),html_semantic(external_site/'articles/index.html'))
 for s in slugs:check(f'article-page:{s}','parsed semantic HTML and JSON-LD',html_semantic(local_site/f'articles/{s}/index.html'),html_semantic(external_site/f'articles/{s}/index.html'))
 check('generated-projection','normalized JSON',json.loads((local_site/'data/generated/articles.json').read_text(encoding='utf-8')),json.loads((external_site/'data/generated/articles.json').read_text(encoding='utf-8')))
 check('atom-feed','namespace-aware parsed XML',xml_semantic(local_site/'articles/feed.xml'),xml_semantic(external_site/'articles/feed.xml'))
 check('sitemap','parsed scoped XML including non-article entries',xml_semantic(local_site/'sitemap.xml'),xml_semantic(external_site/'sitemap.xml'))
 images=[];canonical=[]
 for a in articles:
  if a['status']!='published':continue
  name=Path(a['socialImage']).name; la=repo/'src/Ulfbou.Site/wwwroot/assets/social/articles'/name; ea=package/'assets/social/articles'/name; data=ea.read_bytes(); dims=struct.unpack('>II',data[16:24]) if data[:8]==b'\x89PNG\r\n\x1a\n' else None
  ok=la.read_bytes()==data and dims==(1200,627);checks.append({'name':f'social-image:{a["slug"]}','method':'exact bytes, PNG signature and dimensions','result':'PASS' if ok else 'FAIL'});images.append({'slug':a['slug'],'sha256':digest_file(ea),'dimensions':list(dims) if dims else None,'publicUrl':'https://ulfbou.github.io'+a['socialImage']})
  absolute='https://ulfbou.github.io'+a['canonicalPath']; sem=html_semantic(external_site/f'articles/{a["slug"]}/index.html'); text=(external_site/f'articles/{a["slug"]}/index.html').read_text(encoding='utf-8'); linked='https://www.linkedin.com/sharing/share-offsite/?url='+urllib.parse.quote(absolute,safe='')
  assertions={'canonical':absolute in text,'openGraph':('og:url',absolute) in sem['meta'],'jsonLd':bool(sem['jsonLd']) and sem['jsonLd'][0].get('mainEntityOfPage')==absolute,'linkedIn':linked.replace('&','&amp;') in text or linked in text,'feed':absolute in (external_site/'articles/feed.xml').read_text(encoding='utf-8'),'sitemap':absolute in (external_site/'sitemap.xml').read_text(encoding='utf-8')}
  canonical.append({'slug':a['slug'],'url':absolute,'assertions':assertions});checks.append({'name':f'canonical-urls:{a["slug"]}','method':'exact cross-output assertions','result':'PASS' if all(assertions.values()) else 'FAIL'})
 return checks,images,canonical

def main(argv:list[str]|None=None)->int:
 ap=argparse.ArgumentParser();ap.add_argument('--local-source',required=True);ap.add_argument('--package',required=True);ap.add_argument('--pin',required=True);ap.add_argument('--output',required=True);ap.add_argument('--repo',default='.');args=ap.parse_args(argv)
 repo=Path(args.repo).resolve(); out=Path(args.output).resolve(); local=Path(args.local_source).resolve(); archive=Path(args.package).resolve(); pin_path=Path(args.pin).resolve()
 protected=[repo/'content/articles',repo/'src/Ulfbou.Site/wwwroot',repo/'.github/workflows/deploy-pages.yml']; before={str(p.relative_to(repo)):inventory_digest(tree_inventory(p)) for p in protected}; stages=[];limitations=['Fixture identity is not a real GitHub Release or production cutover authority.','No private-release retrieval or credential mechanism is exercised.']; final=out.with_name(out.name+'.stage');shutil.rmtree(final,ignore_errors=True)
 report={'reportSchemaVersion':'1.0','websiteRevision':'c2ca2e918201023945982271bc91bd1a96bc0c3d','skippedOrIncompleteStages':[],'limitations':limitations,'verdict':VERDICT_BAD}
 try:
  pin=load_pin(pin_path);stages.append('pin')
  if digest_file(archive)!=pin['archiveSha256']:raise ParityError('archive SHA-256 mismatch before extraction')
  stages.append('archive-digest'); final.mkdir(parents=True); package=extract_verified(archive,pin,final/'workspace/package');stages.append('extraction')
  verification=semantic_package(package,pin);stages.append('package-validation')
  checks,images,canonical=compare(repo,local,package,final/'local/site',final/'external/site');stages.append('render-and-compare')
  after={str(p.relative_to(repo)):inventory_digest(tree_inventory(p)) for p in protected}; protected_report={k:{'before':before[k],'after':after[k],'unchanged':before[k]==after[k]} for k in before};checks.append({'name':'protected-paths','method':'complete file inventory and SHA-256 tree digest','result':'PASS' if all(v['unchanged'] for v in protected_report.values()) else 'FAIL'})
  report.update({'representedProviderRevision':pin['providerCommit'],'fixtureIdentity':{'publicationId':pin['publicationId'],'releaseTag':pin['releaseTag'],'fixtureOnly':True},'archive':{'name':pin['assetName'],'sha256':pin['archiveSha256']},'contractVersion':pin['contractVersion'],'product':pin['product'],'verifiedPackageInventory':json.loads((package/'manifest.json').read_text(encoding='utf-8'))['files'],'orderedArticleSlugs':['sycophancy','bias'],'comparisons':checks,'canonicalUrlAssertions':canonical,'socialImages':images,'protectedPaths':protected_report,'completedStages':stages})
  if all(c['result']=='PASS' for c in checks):report['verdict']=VERDICT_OK
  else:raise ParityError('one or more semantic comparisons failed')
 except Exception as e:
  report['error']=str(e);report['errorType']=type(e).__name__;report['completedStages']=stages;report['skippedOrIncompleteStages']=[x for x in ['pin','archive-digest','extraction','package-validation','render-and-compare','report'] if x not in stages]
  print(f'PARITY ERROR [{type(e).__name__}]: {e}', file=sys.stderr)
  final.mkdir(parents=True,exist_ok=True)
 finally:
  reports=final/'reports';reports.mkdir(parents=True,exist_ok=True); report['protectedPaths']=report.get('protectedPaths',{k:{'before':v,'after':inventory_digest(tree_inventory(repo/k)),'unchanged':v==inventory_digest(tree_inventory(repo/k))} for k,v in before.items()})
  (reports/'parity-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2,sort_keys=True)+'\n',encoding='utf-8',newline='\n')
  md=['# Article-package fixture parity report','',f"**Verdict:** {report['verdict']}",'',f"Archive: `{report.get('archive',{}).get('name',archive.name)}`",'', '## Comparisons','']+[f"- {c['result']}: {c['name']} ({c['method']})" for c in report.get('comparisons',[])]+['','## Limitations','']+[f'- {x}' for x in limitations]
  (reports/'parity-report.md').write_text('\n'.join(md)+'\n',encoding='utf-8',newline='\n')
  shutil.rmtree(final/'workspace',ignore_errors=True);shutil.rmtree(out,ignore_errors=True);final.replace(out)
 print(report['verdict']);return 0 if report['verdict']==VERDICT_OK else 2
if __name__=='__main__':raise SystemExit(main())

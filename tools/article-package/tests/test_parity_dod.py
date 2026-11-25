"""Definition-of-Done negative, differential, and failure-atomicity coverage."""
from __future__ import annotations
import contextlib, copy, hashlib, importlib.util, json, shutil, tempfile, unittest, warnings, zipfile
from pathlib import Path
from unittest import mock
ROOT=Path(__file__).resolve().parents[3]; MODULE=ROOT/'tools/article-package/parity.py'
spec=importlib.util.spec_from_file_location('article_parity_dod',MODULE); parity=importlib.util.module_from_spec(spec);spec.loader.exec_module(parity)
PIN=ROOT/'tools/article-package/parity/pin.json'; ZIP=ROOT/'tools/article-package/parity/fixtures/articles-parity.zip'

class DodTests(unittest.TestCase):
 def setUp(self): self.pin=parity.load_pin(PIN)
 def extract(self,root): return parity.extract_verified(ZIP,self.pin,Path(root)/'package')
 def write_json(self,p,v): p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8',newline='\n')
 def refresh_manifest(self,package,path):
  m=json.loads((package/'manifest.json').read_text(encoding='utf-8')); data=(package/path).read_bytes()
  e=next(x for x in m['files'] if x['path']==path);e['bytes']=len(data);e['sha256']=hashlib.sha256(data).hexdigest();self.write_json(package/'manifest.json',m)
 def repack(self,package,archive):
  with zipfile.ZipFile(archive,'w') as z:
   for p in sorted(x for x in package.rglob('*') if x.is_file()):z.write(p,p.relative_to(package).as_posix())
 def pin_file(self,root,pin): p=Path(root)/'pin.json';self.write_json(p,pin);return p

 def test_all_pin_contract_failures(self):
  base=json.loads(PIN.read_text(encoding='utf-8'))
  cases=[
   ('malformed',None),('schemaVersion','2.0'),('contractVersion','2.0'),('providerRepository','wrong/repo'),
   ('providerCommit','c59f549'),('providerCommit','G'*40),('publicationId','real-release'),
   ('releaseTag','wrong'),('assetName','wrong.zip'),('archiveSha256','ABC'),('product','wrong'),
   ('expectedSlugs',['sycophancy','bias']),('fixtureOnly',False),('unknownField',True)]
  for key,value in cases:
   with self.subTest(key=key,value=value),tempfile.TemporaryDirectory() as d:
    p=Path(d)/'pin.json'
    if key=='malformed':p.write_text('{',encoding='utf-8')
    else:
     v=copy.deepcopy(base);v[key]=value;self.write_json(p,v)
    with self.assertRaises(parity.ParityError):parity.load_pin(p)

 def test_archive_path_type_and_duplicate_failures(self):
  cases=[('/absolute',0o100644,False),('../traversal',0o100644,False),('a/../x',0o100644,False),('link',0o120777,False),('x',0o100644,True)]
  for name,mode,duplicate in cases:
   with self.subTest(name=name),tempfile.TemporaryDirectory() as d:
    archive=Path(d)/'articles-parity.zip'
    with warnings.catch_warnings():
     warnings.simplefilter('ignore')
     with zipfile.ZipFile(archive,'w') as z:
      info=zipfile.ZipInfo(name);info.external_attr=mode<<16;z.writestr(info,b'x')
      if duplicate:z.writestr(info,b'x')
    pin=dict(self.pin);pin['archiveSha256']=hashlib.sha256(archive.read_bytes()).hexdigest()
    with self.assertRaises(parity.ParityError):parity.extract_verified(archive,pin,Path(d)/'out')

 def test_manifest_inventory_size_and_digest_failures(self):
  mutations=['undeclared','missing','bytes','digest','duplicate']
  for mutation in mutations:
   with self.subTest(mutation=mutation),tempfile.TemporaryDirectory() as d:
    package=self.extract(d);manifest=json.loads((package/'manifest.json').read_text(encoding='utf-8'))
    if mutation=='undeclared':(package/'extra.txt').write_text('x',encoding='utf-8')
    elif mutation=='missing':(package/manifest['files'][0]['path']).unlink()
    elif mutation=='bytes':manifest['files'][0]['bytes']+=1
    elif mutation=='digest':manifest['files'][0]['sha256']='0'*64
    else:manifest['files'].append(copy.deepcopy(manifest['files'][0]))
    if mutation in {'bytes','digest','duplicate'}:self.write_json(package/'manifest.json',manifest)
    with self.assertRaises(parity.consumer.PackageError):parity.consumer.verify(package)

 def test_contract_asset_projection_and_article_set_failures(self):
  cases=['unsupported-contract','wrong-product','missing-body','missing-image','projection','article-set']
  for case in cases:
   with self.subTest(case=case),tempfile.TemporaryDirectory() as d:
    package=self.extract(d);pin=dict(self.pin)
    if case=='unsupported-contract':
     a=json.loads((package/'articles.json').read_text(encoding='utf-8'));a['schemaVersion']='2.0';self.write_json(package/'articles.json',a);self.refresh_manifest(package,'articles.json')
    elif case=='wrong-product':
     m=json.loads((package/'manifest.json').read_text(encoding='utf-8'));m['product']='wrong';self.write_json(package/'manifest.json',m)
    elif case=='missing-body':(package/'bodies/bias.md').unlink()
    elif case=='missing-image':(package/'assets/social/articles/bias.png').unlink()
    elif case=='projection':
     p=json.loads((package/'data/generated/articles.json').read_text(encoding='utf-8'));p['articles'][0]['title']='different';self.write_json(package/'data/generated/articles.json',p);self.refresh_manifest(package,'data/generated/articles.json')
    else:pin['expectedSlugs']=['bias']
    with self.assertRaises((parity.ParityError,parity.consumer.PackageError)):parity.semantic_package(package,pin)

 def test_differential_semantics_detect_every_required_mutation(self):
  html='''<!doctype html><html lang="en"><head><title>T</title><meta name="description" content="D"><link rel="canonical" href="https://ulfbou.github.io/articles/bias/"><meta property="og:title" content="T"><meta property="og:url" content="https://ulfbou.github.io/articles/bias/"><script type="application/ld+json">{"@type":"Article","headline":"T"}</script></head><body><article data-article-id="bias"><h1>T</h1><p>Body</p><a href="/x">Link</a><a href="https://www.linkedin.com/sharing/share-offsite/?url=x">Share</a></article></body></html>'''
  mutations={'metadata':('content="D"','content="X"'),'body':('<p>Body</p>','<p>Changed</p>'),'route':('/articles/bias/','/articles/other/'),'canonical':('href="https://ulfbou.github.io/articles/bias/"','href="https://example.test/"'),'jsonld':('"headline":"T"','"headline":"X"'),'open-graph':('property="og:title" content="T"','property="og:title" content="X"'),'linkedin':('sharing/share-offsite','sharing/other')}
  with tempfile.TemporaryDirectory() as d:
   base=Path(d)/'base.html';base.write_text(html,encoding='utf-8');expected=parity.html_semantic(base)
   for name,(old,new) in mutations.items():
    with self.subTest(name=name):
     p=Path(d)/f'{name}.html';p.write_text(html.replace(old,new),encoding='utf-8');self.assertNotEqual(expected,parity.html_semantic(p))
   feed='<feed xmlns="http://www.w3.org/2005/Atom"><entry><id>a</id></entry></feed>';a=Path(d)/'a.xml';b=Path(d)/'b.xml';a.write_text(feed,encoding='utf-8');b.write_text(feed.replace('<id>a','<id>b'),encoding='utf-8');self.assertNotEqual(parity.xml_semantic(a),parity.xml_semantic(b))
   sitemap='<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>/</loc></url><url><loc>/articles/bias/</loc></url></urlset>';a.write_text(sitemap,encoding='utf-8');b.write_text(sitemap.replace('/articles/bias/','/articles/other/'),encoding='utf-8');self.assertNotEqual(parity.xml_semantic(a),parity.xml_semantic(b))
   b.write_text(sitemap.replace('<loc>/</loc>','<loc>/changed/</loc>'),encoding='utf-8');self.assertNotEqual(parity.xml_semantic(a),parity.xml_semantic(b))
   self.assertNotEqual(b'image-a',b'image-b')
   projection={'schemaVersion':'1.0','articles':[{'slug':'bias'},{'slug':'sycophancy'}]}
   for changed in [ {'schemaVersion':'1.0','articles':[{'slug':'bias'}]}, {'schemaVersion':'1.0','articles':[{'slug':'sycophancy'},{'slug':'bias'}]} ]:self.assertNotEqual(projection,changed)

 def run_failure(self,root,archive,pin_path,patches=None):
  out=Path(root)/'artifacts';args=['--local-source',str(ROOT/'content/articles'),'--package',str(archive),'--pin',str(pin_path),'--output',str(out),'--repo',str(ROOT)]
  before={p:parity.inventory_digest(parity.tree_inventory(ROOT/p)) for p in ['content/articles','src/Ulfbou.Site/wwwroot','.github/workflows/deploy-pages.yml']}
  patched=dict(patches or {});patched['tree_inventory']=lambda _p: []
  with mock.patch.multiple(parity,**patched):status=parity.main(args)
  after={p:parity.inventory_digest(parity.tree_inventory(ROOT/p)) for p in before}
  self.assertEqual(status,2);self.assertEqual(before,after);self.assertFalse((out/'workspace').exists());report=json.loads((out/'reports/parity-report.json').read_text(encoding='utf-8'));self.assertEqual(report['verdict'],parity.VERDICT_BAD);self.assertTrue(report['skippedOrIncompleteStages']);return report

 def test_failure_atomicity_archive_manifest_semantic_and_rendering(self):
  with tempfile.TemporaryDirectory() as d:
   bad=Path(d)/'articles-parity.zip';bad.write_bytes(ZIP.read_bytes()+b'x');self.run_failure(d,bad,PIN)
  with tempfile.TemporaryDirectory() as d:
   package=self.extract(d);m=json.loads((package/'manifest.json').read_text(encoding='utf-8'));m['files'][0]['sha256']='0'*64;self.write_json(package/'manifest.json',m);archive=Path(d)/'articles-parity.zip';self.repack(package,archive);pin=dict(self.pin);pin['archiveSha256']=hashlib.sha256(archive.read_bytes()).hexdigest();self.run_failure(d,archive,self.pin_file(d,pin))
  with tempfile.TemporaryDirectory() as d:
   def fail_extract(*_):raise OSError('injected extraction failure')
   self.run_failure(d,ZIP,PIN,{'extract_verified':fail_extract})
  with tempfile.TemporaryDirectory() as d:
   def fail_semantic(*_):raise parity.ParityError('injected semantic failure')
   self.run_failure(d,ZIP,PIN,{'semantic_package':fail_semantic})
  with tempfile.TemporaryDirectory() as d:
   def fail_compare(*_):raise RuntimeError('injected renderer failure')
   self.run_failure(d,ZIP,PIN,{'compare':fail_compare})

 def test_report_generation_failure_never_presents_valid_output(self):
  with tempfile.TemporaryDirectory() as d:
   out=Path(d)/'out';args=['--local-source',str(ROOT/'content/articles'),'--package',str(ZIP),'--pin',str(PIN),'--output',str(out),'--repo',str(ROOT)]
   original=Path.write_text
   def fail_report(path,*a,**k):
    if path.name=='parity-report.json':raise OSError('injected report failure')
    return original(path,*a,**k)
   with mock.patch.object(Path,'write_text',fail_report), mock.patch.object(parity,'compare',side_effect=RuntimeError('stop before report')):
    with self.assertRaises(OSError):parity.main(args)
   self.assertFalse((out/'reports/parity-report.json').exists());self.assertFalse((out/'workspace').exists())

if __name__=='__main__':unittest.main()

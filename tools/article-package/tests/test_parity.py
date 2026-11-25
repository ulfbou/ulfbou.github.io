"""Focused contract, archive, reproducibility, and real-renderer parity tests."""
from __future__ import annotations
import hashlib, importlib.util, json, shutil, subprocess, tempfile, unittest, zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]; MODULE=ROOT/'tools/article-package/parity.py'
spec=importlib.util.spec_from_file_location('article_parity',MODULE); parity=importlib.util.module_from_spec(spec);spec.loader.exec_module(parity)
PIN=ROOT/'tools/article-package/parity/pin.json'; ZIP=ROOT/'tools/article-package/parity/fixtures/articles-parity.zip'
class ParityTests(unittest.TestCase):
 def test_pin_and_archive(self):
  pin=parity.load_pin(PIN);self.assertEqual(parity.digest_file(ZIP),pin['archiveSha256']);self.assertEqual(pin['expectedSlugs'],['bias','sycophancy'])
 def test_fixture_regeneration_is_byte_reproducible(self):
  before=ZIP.read_bytes();subprocess.run(['python',str(ROOT/'tools/article-package/parity/generate_fixture.py')],cwd=ROOT,check=True,capture_output=True);self.assertEqual(before,ZIP.read_bytes())
 def test_closed_pin_failures(self):
  base=json.loads(PIN.read_text(encoding='utf-8'))
  cases=[('extra',True),('providerRepository','other/repo'),('providerCommit','c59f549'),('releaseTag','wrong'),('assetName','wrong.zip'),('expectedSlugs',['bias'])]
  for key,value in cases:
   with self.subTest(key=key),tempfile.TemporaryDirectory() as d:
    v=dict(base);v[key]=value;p=Path(d)/'pin.json';p.write_text(json.dumps(v), encoding='utf-8', newline='\n')
    with self.assertRaises(parity.ParityError):parity.load_pin(p)
 def test_digest_fails_before_extraction(self):
  with tempfile.TemporaryDirectory() as d:
   bad=Path(d)/'articles-parity.zip';bad.write_bytes(ZIP.read_bytes()+b'x');target=Path(d)/'out'
   with self.assertRaisesRegex(parity.ParityError,'before extraction'):parity.extract_verified(bad,parity.load_pin(PIN),target)
   self.assertFalse(target.exists())
 def make_zip(self,name:str,mode:int=0o100644,data:bytes=b'x',duplicate=False):
  d=tempfile.TemporaryDirectory();p=Path(d.name)/'articles-parity.zip'
  with zipfile.ZipFile(p,'w') as z:
   i=zipfile.ZipInfo(name);i.external_attr=mode<<16;z.writestr(i,data)
   if duplicate:z.writestr(i,data)
  pin=parity.load_pin(PIN);pin['archiveSha256']=hashlib.sha256(p.read_bytes()).hexdigest();return d,p,pin
 def test_unsafe_and_duplicate_members(self):
  for name,mode,duplicate in [('../x',0o100644,False),('/x',0o100644,False),('link',0o120777,False),('x',0o100644,True)]:
   with self.subTest(name=name),self.make_zip(name,mode,duplicate=duplicate)[0] as _:pass
   d,p,pin=self.make_zip(name,mode,duplicate=duplicate)
   try:
    with self.assertRaises(parity.ParityError):parity.extract_verified(p,pin,Path(d.name)/'out')
   finally:d.cleanup()
 def test_manifest_and_contract_mutations(self):
  with tempfile.TemporaryDirectory() as d:
   package=parity.extract_verified(ZIP,parity.load_pin(PIN),Path(d)/'package');pin=parity.load_pin(PIN)
   parity.semantic_package(package,pin)
   for path,mutation in [('manifest.json',lambda v:v.update(product='wrong')),('data/generated/articles.json',lambda v:v['articles'][0].update(title='changed'))]:
    original=(package/path).read_text(encoding='utf-8');v=json.loads(original);mutation(v);(package/path).write_text(json.dumps(v), encoding='utf-8', newline='\n')
    with self.assertRaises((parity.ParityError,parity.consumer.PackageError)):parity.semantic_package(package,pin)
    (package/path).write_text(original)
 @unittest.skipUnless(shutil.which('dotnet'),'dotnet is required for the real-renderer integration test')
 def test_real_renderer_gate_is_repeatable(self):
  with tempfile.TemporaryDirectory() as d:
   output=Path(d)/'out';cmd=['python',str(MODULE),'--local-source',str(ROOT/'content/articles'),'--package',str(ZIP),'--pin',str(PIN),'--output',str(output),'--repo',str(ROOT)]
   for run in range(1,3):
    result=subprocess.run(cmd,text=True,encoding='utf-8',errors='replace',capture_output=True)
    report_path=output/'reports/parity-report.json'
    report_text=report_path.read_text(encoding='utf-8') if report_path.is_file() else '<report missing>'
    if result.returncode != 0:
     self.fail(f'parity run {run} failed with exit {result.returncode}\nSTDOUT:\n{result.stdout}\nSTDERR:\n{result.stderr}\nREPORT:\n{report_text}')
    report=json.loads(report_text);self.assertEqual(report['verdict'],parity.VERDICT_OK,report_text)
if __name__=='__main__':unittest.main()

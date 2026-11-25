import importlib.util, json, tempfile, unittest, zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
spec=importlib.util.spec_from_file_location("acquire",ROOT/"tools/article-package/acquire.py"); acquire=importlib.util.module_from_spec(spec); spec.loader.exec_module(acquire)
class AcquireTests(unittest.TestCase):
 def pin(self): return {"state":"configured","providerRepository":"ulfbou/content","providerCommit":"a"*40,"releaseTag":"articles-2026.11.25.1","publicationId":"2026.11.25.1","assetId":1,"assetFilename":"articles-2026.11.25.1.zip","archiveSha256":"b"*64,"contractVersion":"1.0","product":"articles","expectedSlugs":["bias","sycophancy"]}
 def test_rejects_unconfigured_pin(self):
  p=self.pin();p["state"]="unconfigured"
  with self.assertRaisesRegex(acquire.AcquisitionError,"unconfigured"):acquire.validate_pin(p)
 def test_rejects_floating_or_inconsistent_identity(self):
  for key,value in (("providerCommit","master"),("releaseTag","latest"),("assetFilename","articles.zip"),("archiveSha256","0")):
   p=self.pin();p[key]=value
   with self.subTest(key=key),self.assertRaises(acquire.AcquisitionError):acquire.validate_pin(p)
 def test_digest_substitution_is_detected_before_extraction(self):
  with tempfile.TemporaryDirectory() as d:
   f=Path(d)/"a.zip";f.write_bytes(b"not the pinned archive")
   self.assertNotEqual(acquire.digest(f),self.pin()["archiveSha256"])
 def test_rejects_unsafe_duplicate_and_symlink_members(self):
  with tempfile.TemporaryDirectory() as d:
   d=Path(d)
   for name,members in (("traversal.zip",[("../escape",b"x")]),("duplicate.zip",[("a",b"1"),("a",b"2")])):
    z=d/name
    with zipfile.ZipFile(z,"w") as archive:
     for member,data in members:archive.writestr(member,data)
    with self.subTest(name=name),self.assertRaises(acquire.AcquisitionError):acquire.extract_safely(z,d/(name+".out"))
   z=d/"symlink.zip"
   with zipfile.ZipFile(z,"w") as archive:
    info=zipfile.ZipInfo("link");info.external_attr=(0o120777<<16);archive.writestr(info,b"target")
   with self.assertRaises(acquire.AcquisitionError):acquire.extract_safely(z,d/"symlink.out")
if __name__=="__main__":unittest.main()

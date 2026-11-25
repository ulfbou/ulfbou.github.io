#!/usr/bin/env python3
"""Acquire one exactly pinned private article release without mutating website input."""
from __future__ import annotations
import argparse, hashlib, importlib.util, json, os, shutil, stat, sys, tempfile, urllib.error, urllib.request, zipfile
from pathlib import Path, PurePosixPath
from typing import Any

PIN_FIELDS={"state","providerRepository","providerCommit","releaseTag","publicationId","assetId","assetFilename","archiveSha256","contractVersion","product","expectedSlugs"}
HEX=set("0123456789abcdef")
class AcquisitionError(RuntimeError): pass

def fail(message:str)->None: raise AcquisitionError(message)
def load_json(path:Path)->dict[str,Any]:
 try: value=json.loads(path.read_text(encoding="utf-8"))
 except (OSError,UnicodeError,json.JSONDecodeError) as exc: fail(f"invalid JSON in {path}: {exc}")
 if not isinstance(value,dict): fail(f"expected JSON object in {path}")
 return value

def validate_pin(pin:dict[str,Any])->None:
 missing=sorted(PIN_FIELDS-set(pin)); extra=sorted(set(pin)-PIN_FIELDS)
 if missing or extra: fail(f"invalid production pin fields; missing={missing}, extra={extra}")
 if pin["state"]!="configured": fail("production article pin is unconfigured; promote a provider release and replace the reviewed pin")
 strings=("providerRepository","providerCommit","releaseTag","publicationId","assetFilename","archiveSha256","contractVersion","product")
 if any(not isinstance(pin[k],str) or not pin[k] for k in strings): fail("configured production pin contains an empty identity field")
 if pin["providerRepository"]!="ulfbou/content": fail("unexpected provider repository")
 if len(pin["providerCommit"])!=40 or any(c not in HEX for c in pin["providerCommit"]): fail("providerCommit must be a lowercase full commit SHA")
 if pin["releaseTag"]!=f'articles-{pin["publicationId"]}': fail("releaseTag is inconsistent with publicationId")
 if pin["assetFilename"]!=f'articles-{pin["publicationId"]}.zip': fail("assetFilename is inconsistent with publicationId")
 if not isinstance(pin["assetId"],int) or pin["assetId"]<=0: fail("assetId must be a positive GitHub asset ID")
 if len(pin["archiveSha256"])!=64 or any(c not in HEX for c in pin["archiveSha256"]): fail("archiveSha256 must be lowercase SHA-256")
 if pin["contractVersion"]!="1.0" or pin["product"]!="articles": fail("unsupported article package contract")
 slugs=pin["expectedSlugs"]
 if not isinstance(slugs,list) or not slugs or slugs!=sorted(set(slugs)) or any(not isinstance(s,str) or not s for s in slugs): fail("expectedSlugs must be a sorted, unique, non-empty string list")

def request_json(url:str,token:str)->dict[str,Any]:
 req=urllib.request.Request(url,headers={"Accept":"application/vnd.github+json","Authorization":f"Bearer {token}","X-GitHub-Api-Version":"2022-11-28","User-Agent":"ulfbou.github.io-article-consumer"})
 try:
  with urllib.request.urlopen(req) as response: return json.load(response)
 except (urllib.error.URLError,urllib.error.HTTPError,json.JSONDecodeError) as exc: fail(f"GitHub release lookup failed: {exc}")

def download(url:str,token:str,target:Path)->None:
 req=urllib.request.Request(url,headers={"Accept":"application/octet-stream","Authorization":f"Bearer {token}","X-GitHub-Api-Version":"2022-11-28","User-Agent":"ulfbou.github.io-article-consumer"})
 try:
  with urllib.request.urlopen(req) as src,target.open("wb") as dst: shutil.copyfileobj(src,dst)
 except (urllib.error.URLError,urllib.error.HTTPError,OSError) as exc: fail(f"GitHub asset download failed: {exc}")

def digest(path:Path)->str:
 h=hashlib.sha256()
 with path.open("rb") as source:
  for chunk in iter(lambda:source.read(1024*1024),b""): h.update(chunk)
 return h.hexdigest()

def member_name(info:zipfile.ZipInfo)->PurePosixPath:
 raw=info.filename
 path=PurePosixPath(raw)
 mode=info.external_attr>>16
 if not raw or raw.endswith("/") or path.is_absolute() or "." in path.parts or ".." in path.parts or chr(92) in raw: fail(f"unsafe ZIP member: {raw!r}")
 if stat.S_ISLNK(mode): fail(f"symbolic link ZIP member is forbidden: {raw}")
 return path

def extract_safely(archive:Path,target:Path)->None:
 with zipfile.ZipFile(archive) as package:
  seen:set[str]=set()
  members=[]
  for info in package.infolist():
   name=member_name(info); normalized=name.as_posix()
   if normalized in seen: fail(f"duplicate ZIP member: {normalized}")
   seen.add(normalized); members.append((info,name))
  for info,name in members:
   output=target.joinpath(*name.parts); output.parent.mkdir(parents=True,exist_ok=True)
   with package.open(info) as source,output.open("wb") as destination: shutil.copyfileobj(source,destination)

def load_consumer(script:Path):
 spec=importlib.util.spec_from_file_location("article_consumer",script)
 if spec is None or spec.loader is None: fail("cannot load article package consumer")
 module=importlib.util.module_from_spec(spec); spec.loader.exec_module(module); return module

def acquire(pin_path:Path,output:Path,token:str,consumer_path:Path)->dict[str,Any]:
 pin=load_json(pin_path); validate_pin(pin)
 release=request_json(f'https://api.github.com/repos/{pin["providerRepository"]}/releases/tags/{pin["releaseTag"]}',token)
 if release.get("tag_name")!=pin["releaseTag"] or release.get("target_commitish")!=pin["providerCommit"]: fail("GitHub release identity does not match the reviewed pin")
 assets=[a for a in release.get("assets",[]) if a.get("id")==pin["assetId"]]
 if len(assets)!=1 or assets[0].get("name")!=pin["assetFilename"]: fail("GitHub release asset identity does not match the reviewed pin")
 with tempfile.TemporaryDirectory(prefix="article-acquire-") as temporary:
  work=Path(temporary); archive=work/pin["assetFilename"]
  download(assets[0]["url"],token,archive)
  if digest(archive)!=pin["archiveSha256"]: fail("archive SHA-256 mismatch before extraction")
  package=work/"package"; package.mkdir(); extract_safely(archive,package)
  consumer=load_consumer(consumer_path)
  try: verified=consumer.verify(package)
  except Exception as exc: fail(f"article package validation failed: {exc}")
  if verified.get("schemaVersion")!=pin["contractVersion"] or verified.get("slugs")!=pin["expectedSlugs"]: fail("validated package identity does not match the reviewed pin")
  manifest=load_json(package/"manifest.json")
  if manifest.get("releaseId")!=pin["publicationId"] or manifest.get("product")!=pin["product"]: fail("manifest publication identity does not match the reviewed pin")
  staged=output.with_name(output.name+".new")
  if staged.exists(): shutil.rmtree(staged)
  shutil.copytree(package,staged)
  if output.exists(): shutil.rmtree(output)
  staged.replace(output)
 return {"publicationId":pin["publicationId"],"providerCommit":pin["providerCommit"],"archiveSha256":pin["archiveSha256"],"output":str(output),"slugs":pin["expectedSlugs"]}

def main()->int:
 parser=argparse.ArgumentParser(); parser.add_argument("--pin",default="tools/article-package/production-pin.json"); parser.add_argument("--output",required=True); parser.add_argument("--consumer",default="tools/article-package/consumer.py"); parser.add_argument("--token-env",default="ARTICLE_PROVIDER_TOKEN")
 args=parser.parse_args(); token=os.environ.get(args.token_env,"")
 if not token: print(f"ERROR: missing GitHub credential in {args.token_env}",file=sys.stderr); return 2
 try: result=acquire(Path(args.pin),Path(args.output),token,Path(args.consumer))
 except AcquisitionError as exc: print(f"ERROR: {exc}",file=sys.stderr); return 2
 print(json.dumps(result,sort_keys=True)); return 0
if __name__=="__main__": raise SystemExit(main())

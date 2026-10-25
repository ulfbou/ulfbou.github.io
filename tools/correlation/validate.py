#!/usr/bin/env python3
import hashlib,json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]; BASE=ROOT/'docs/aws/correlation'; SCHEMAS=BASE/'schemas'
class Failure(Exception): pass
def fail(c,m): raise Failure(f'{c}: {m}')
def req(v,c,m):
 if not v: fail(c,m)
def load(p,limit=131072):
 b=p.read_bytes(); req(len(b)<=limit,'BYTE_LIMIT',f'{p}: {len(b)}'); req(not b.startswith(b'\xef\xbb\xbf'),'BOM',str(p))
 def pairs(xs):
  d={}
  for k,v in xs:
   req(k not in d,'DUPLICATE_PROPERTY',f'{p}: {k}'); d[k]=v
  return d
 try:return json.loads(b.decode(),object_pairs_hook=pairs,parse_float=lambda x:fail('NON_INTEGER_NUMBER',f'{p}: {x}'),parse_constant=lambda x:fail('NON_FINITE_NUMBER',f'{p}: {x}'))
 except (UnicodeDecodeError,json.JSONDecodeError) as e:fail('JSON',f'{p}: {e}')
def resolve(ref): return load(SCHEMAS/ref)
def validate(v,s,path='$'):
 if '$ref' in s:return validate(v,resolve(s['$ref']),path)
 if 'const' in s:req(v==s['const'],'CONST',path)
 if 'enum' in s:req(v in s['enum'],'ENUM',path)
 t=s.get('type')
 if t=='object':
  req(isinstance(v,dict),'TYPE',path); props=s.get('properties',{}); req(all(k in v for k in s.get('required',[])),'REQUIRED',path)
  if s.get('additionalProperties') is False:req(set(v)<=set(props),'UNKNOWN_PROPERTY',path)
  for k,x in v.items():
   if k in props:validate(x,props[k],path+'.'+k)
 elif t=='array':
  req(isinstance(v,list),'TYPE',path); req(len(v)>=s.get('minItems',0) and len(v)<=s.get('maxItems',10**9),'ITEMS',path)
  for i,x in enumerate(v):validate(x,s.get('items',{}),f'{path}[{i}]')
 elif t=='string':req(isinstance(v,str),'TYPE',path)
 elif t=='integer':req(isinstance(v,int) and not isinstance(v,bool),'TYPE',path)
 elif t=='boolean':req(isinstance(v,bool),'TYPE',path)
 if isinstance(v,str):
  req(len(v)>=s.get('minLength',0) and len(v)<=s.get('maxLength',10**9),'LENGTH',path)
  if 'pattern' in s:req(re.fullmatch(s['pattern'],v) is not None,'PATTERN',path)
 if isinstance(v,int) and not isinstance(v,bool):req(v>=s.get('minimum',-10**99) and v<=s.get('maximum',10**99),'RANGE',path)
 if 'oneOf' in s:
  passed=0
  for branch in s['oneOf']:
   try:validate(v,branch,path);passed+=1
   except Failure:pass
  req(passed==1,'ONE_OF',path)
def canonical(v):return json.dumps(v,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()
def main():
 schemas={p.name.removesuffix('.schema.json'):load(p) for p in SCHEMAS.glob('*.schema.json')}; req(schemas,'SCHEMA_NONE','none')
 ids=set()
 for n,s in schemas.items():
  req(s.get('$schema')=='https://json-schema.org/draft/2020-12/schema','SCHEMA_DRAFT',n);req(s.get('additionalProperties') is False,'SCHEMA_OPEN',n)
  c=s.get('properties',{}).get('schemaVersion',{}).get('const');req(c and c not in ids,'SCHEMA_ID',n);ids.add(c)
 fixtures=BASE/'fixtures/contracts'
 for n,s in schemas.items():
  good=fixtures/f'{n}.valid.json';bad=fixtures/f'{n}.invalid.json'
  if good.exists():validate(load(good),s)
  if bad.exists():
   try:validate(load(bad),s);fail('INVALID_ACCEPTED',n)
   except Failure as e:
    if str(e).startswith('INVALID_ACCEPTED'):raise
 for v in load(BASE/'fixtures/canonical/correlation-state-canonical-vectors-v1.json')['vectors']:
  b=canonical(v['input']);req(b.decode()==v['canonicalUtf8'],'CANON_BYTES',v['id']);req(b.hex()==v['canonicalHex'],'CANON_HEX',v['id']);req('sha256:'+hashlib.sha256(b).hexdigest()==v['sha256'],'CANON_SHA',v['id'])
 t=load(BASE/'registries/state-transitions-v1.json');terminal=set(t['terminalStates']);req(not any(x['from'] in terminal for x in t['transitions']),'TERMINAL_OUTGOING','state machine');req(all(x['revisionDelta']==1 for x in t['transitions']),'REVISION_DELTA','state machine')
 cases=load(BASE/'fixtures/semantic/deletion-authorization-cases-v1.json');expected={'exact':'DeleteConditionally','absent':'AlreadyAbsent','newer-revision':'RevisionConflict','same-revision-different-digest':'DigestConflict','wrong-correlation':'InvalidActiveState','wrong-representation':'InvalidActiveState','malformed':'InvalidActiveState','changed-before-delete':'RevisionConflict'};req({x['id']:x['expected'] for x in cases['cases']}==expected,'DELETE_CASES','incomplete')
 ids=[x['requirementId'] for x in load(BASE/'registries/requirement-traceability-v1.json')['entries']];req(len(ids)==len(set(ids))==67,'TRACEABILITY','expected 67')
 print('PASS: correlation contract package');return 0
if __name__=='__main__':
 try:raise SystemExit(main())
 except Failure as e:print(e,file=sys.stderr);raise SystemExit(1)

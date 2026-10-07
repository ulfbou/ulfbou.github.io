#!/usr/bin/env python3
import argparse, hashlib, json, re, sys
from pathlib import Path, PurePosixPath

class Violation(Exception):
    def __init__(self, code, message): super().__init__(f'{code}: {message}'); self.code=code

def load(path): return json.loads(Path(path).read_text(encoding='utf-8'))
def canonical(value): return json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(',', ':')).encode('utf-8')
def digest(value): return hashlib.sha256(canonical(value)).hexdigest()
def require(ok, code, message):
    if not ok: raise Violation(code, message)

def package_identity(package):
    observations=package.get('observations', [])
    return {
        'repository': package.get('repository'), 'revision': package.get('revision'),
        'collectorContractVersion': package.get('collectorContractVersion'),
        'scope': package.get('scope'), 'redactionPolicy': package.get('redactionPolicy'),
        'exclusionPolicy': package.get('exclusionPolicy'),
        'observationDigests': [digest(x) for x in observations]
    }

def validate(governance, approved, package):
    require(governance.get('inference', {}).get('admittedAdapters') == [], 'GOV-015', 'prototype admits no inference adapters')
    required=['adapterImplementation','zeroChargeVerificationRecord','acceptedDecision','governanceRegistryUpdate']
    require(governance.get('inference', {}).get('adapterSubstitutionRequires') == required, 'GOV-016', 'adapter substitution gates differ')
    require(re.fullmatch(r'[0-9a-f]{40}', package.get('revision','')) is not None, 'GOV-005', 'revision must be a full lowercase commit id')
    expected='evp_'+digest(package_identity(package))
    require(package.get('packageId') == expected, 'GOV-017', 'package identity does not cover canonical scope, policy, and observations')
    observations={}
    for item in package.get('observations',[]):
        oid=item.get('id'); require(oid and oid not in observations, 'GOV-013', 'duplicate or empty observation id')
        path=item.get('path',''); p=PurePosixPath(path)
        require(path and not p.is_absolute() and '..' not in p.parts and '\\' not in path, 'GOV-006', 'evidence path must be normalized and relative')
        observations[oid]=item
    claim_ids=set()
    for project in approved.get('projects',[]):
        for claim in project.get('claims',[]):
            cid=claim.get('id'); require(cid and cid not in claim_ids, 'GOV-013', 'duplicate or empty claim id'); claim_ids.add(cid)
            for ref in claim.get('evidenceRefs',[]):
                require(ref.get('packageId') == package.get('packageId'), 'GOV-004', 'evidence package reference is unresolved')
                require(ref.get('observationId') in observations, 'GOV-004', 'evidence observation reference is unresolved')
                require(ref.get('visibility') == 'Public', 'GOV-008', 'approved reference is not public')
                require(observations[ref['observationId']].get('visibility') == 'Public', 'GOV-008', 'observation is not public')

def compile_index(governance, approved, package):
    validate(governance, approved, package)
    observations={x['id']:x for x in package['observations']}
    projects={}
    for project in approved.get('projects',[]):
        claims=[]
        for claim in project.get('claims',[]):
            if claim.get('visibility') != 'Public': continue
            evidence=[]
            for ref in claim.get('evidenceRefs',[]):
                item=observations[ref['observationId']]
                evidence.append({'kind':item['kind'],'repository':package['repository']['nameWithOwner'],'revision':package['revision'],'path':item['path']})
            claims.append({'id':claim['id'],'text':claim['text'],'limitations':claim.get('limitations',[]),'evidence':evidence})
        if claims: projects[project['id']]={'claims':claims}
    body={'schemaVersion':governance['schemaVersion'],'contractVersion':governance['contractVersion'],'projects':projects}
    body['buildIdentity']={'schemaVersion':governance['schemaVersion'],'contractVersion':governance['contractVersion'],'projectionSha256':digest(body)}
    return body

def write_json(path, value): Path(path).write_bytes(json.dumps(value,ensure_ascii=False,sort_keys=True,indent=2).encode()+b'\n')
def main(argv=None):
    parser=argparse.ArgumentParser(); sub=parser.add_subparsers(dest='command',required=True)
    for name in ('validate','compile','verify'):
        p=sub.add_parser(name); p.add_argument('--governance',required=True); p.add_argument('--approved',required=True); p.add_argument('--evidence',required=True)
        if name=='compile': p.add_argument('--output',required=True)
        if name=='verify': p.add_argument('--projection',required=True)
    a=parser.parse_args(argv)
    try:
        g,approved,evidence=load(a.governance),load(a.approved),load(a.evidence)
        if a.command=='validate': validate(g,approved,evidence)
        elif a.command=='compile': write_json(a.output,compile_index(g,approved,evidence))
        else:
            expected=compile_index(g,approved,evidence); actual=load(a.projection)
            require(canonical(actual)==canonical(expected),'GOV-010','projection is not deterministic from governed inputs')
        print('PASS')
        return 0
    except (Violation, KeyError, TypeError, json.JSONDecodeError) as error:
        print(str(error),file=sys.stderr); return 1
if __name__=='__main__': raise SystemExit(main())

#!/usr/bin/env python3
import argparse, datetime, hashlib, json, pathlib, re, sys
MAX_TOMBSTONE_BYTES=16*1024
MAX_ENTRIES=25
SHA=re.compile(r'^sha256:[0-9a-f]{64}$')
ID=re.compile(r'^[a-z0-9][a-z0-9-]{7,127}$')
def strict_load(path, maximum):
    raw=path.read_bytes()
    if len(raw)>maximum: raise ValueError(f'oversized:{path}')
    def pairs(items):
        value={}
        for key,item in items:
            if key in value: raise ValueError(f'duplicate-property:{path}:{key}')
            value[key]=item
        return value
    return json.loads(raw.decode('utf-8'),object_pairs_hook=pairs)
def canonical(value): return json.dumps(value,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode('utf-8')
def main():
    p=argparse.ArgumentParser(); p.add_argument('--runtime-root',required=True); p.add_argument('--repository',required=True); p.add_argument('--runtime-commit',required=True); p.add_argument('--created-at',required=True); p.add_argument('--output',required=True); a=p.parse_args()
    if not re.fullmatch(r'[0-9a-f]{40}',a.runtime_commit): raise SystemExit('invalid runtime commit')
    root=pathlib.Path(a.runtime_root); now=datetime.datetime.fromisoformat(a.created_at.replace('Z','+00:00'))
    entries=[]
    for path in sorted((root/'correlation-runtime/tombstones/v1').rglob('*.json')) if (root/'correlation-runtime/tombstones/v1').exists() else []:
        value=strict_load(path,MAX_TOMBSTONE_BYTES)
        admitted={'schemaVersion','tombstoneId','correlationId','operationId','capability','providerAlias','terminalState','reasonCode','expectedStateRevision','canonicalRepresentation','expectedStateSha256','terminatedAt','deletionEligibleAt','decisionAuthority','producerEventId','publicLimitations','classification'}
        if set(value)!=admitted: raise SystemExit(f'unknown-or-missing-field:{path}')
        if value['schemaVersion']!='correlation-tombstone-v1' or value['classification']!='PublicPermanently': raise SystemExit(f'invalid-tombstone:{path}')
        if not ID.fullmatch(value['correlationId']) or not SHA.fullmatch(value['expectedStateSha256']): raise SystemExit(f'invalid-identity:{path}')
        eligible=datetime.datetime.fromisoformat(value['deletionEligibleAt'].replace('Z','+00:00'))
        if eligible>now: continue
        relative=path.relative_to(root).as_posix(); digest='sha256:'+hashlib.sha256(path.read_bytes()).hexdigest()
        entries.append({'entryId':'entry-'+hashlib.sha256((relative+'|'+digest).encode()).hexdigest()[:32],'tombstonePath':relative,'tombstoneSha256':digest,'correlationId':value['correlationId'],'expectedStateRevision':value['expectedStateRevision'],'expectedStateSha256':value['expectedStateSha256']})
        if len(entries)==MAX_ENTRIES: break
    if not entries: raise SystemExit(3)
    expires=now+datetime.timedelta(minutes=15)
    body={'schemaVersion':'correlation-deletion-manifest-v1','repository':a.repository,'runtimeBranch':'correlation-runtime','runtimeCommit':a.runtime_commit,'createdAt':now.strftime('%Y-%m-%dT%H:%M:%S.000Z'),'expiresAt':expires.strftime('%Y-%m-%dT%H:%M:%S.000Z'),'policyVersion':'cleanup-policy-v1','entries':entries}
    body['manifestId']='sha256:'+hashlib.sha256(canonical(body)).hexdigest()
    target=pathlib.Path(a.output); target.write_bytes(canonical(body)+b'\n')
    if target.stat().st_size>128*1024: raise SystemExit('manifest exceeds 128 KiB')
    print(body['manifestId'])
if __name__=='__main__': main()

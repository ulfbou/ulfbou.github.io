#!/usr/bin/env python3
import argparse, hashlib, json, pathlib, re
MAX=128*1024
def load(path):
    raw=path.read_bytes()
    if len(raw)>MAX: raise SystemExit(f'oversized:{path}')
    def pairs(items):
        value={}
        for key,item in items:
            if key in value: raise ValueError(f'duplicate:{key}')
            value[key]=item
        return value
    return json.loads(raw.decode(),object_pairs_hook=pairs)
def write(path,value):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(value,sort_keys=True,separators=(',',':'))+'\n',encoding='utf-8')
def main():
    p=argparse.ArgumentParser();p.add_argument('--runtime-root',required=True);p.add_argument('--manifest',required=True);p.add_argument('--receipt',required=True);p.add_argument('--reconciled-at',required=True);a=p.parse_args()
    root=pathlib.Path(a.runtime_root); manifest=load(pathlib.Path(a.manifest)); receipt=load(pathlib.Path(a.receipt))
    if receipt.get('schemaVersion')!='correlation-deletion-receipt-v1' or receipt.get('manifestId')!=manifest.get('manifestId'): raise SystemExit('receipt-manifest mismatch')
    expected={x['entryId']:x for x in manifest['entries']}; observed={x['entryId']:x for x in receipt.get('entries',[])}
    if set(expected)!=set(observed) or len(observed)!=len(receipt.get('entries',[])): raise SystemExit('receipt bijection failure')
    outcomes={'Deleted','AlreadyAbsent','RevisionConflict','DigestConflict','InvalidActiveState','InvalidRequest','TemporaryFailure'}
    if any(x.get('outcome') not in outcomes for x in observed.values()): raise SystemExit('unknown receipt outcome')
    counts={'requested':len(observed),'deleted':sum(x['outcome']=='Deleted' for x in observed.values()),'alreadyAbsent':sum(x['outcome']=='AlreadyAbsent' for x in observed.values()),'conflicts':sum(x['outcome'] in {'RevisionConflict','DigestConflict'} for x in observed.values()),'invalid':sum(x['outcome'] in {'InvalidActiveState','InvalidRequest'} for x in observed.values()),'temporaryFailures':sum(x['outcome']=='TemporaryFailure' for x in observed.values())}
    if receipt.get('summary')!=counts: raise SystemExit('receipt summary mismatch')
    receipt_id=receipt.get('receiptId') or 'sha256:'+hashlib.sha256(pathlib.Path(a.receipt).read_bytes()).hexdigest()
    write(root/f'correlation-runtime/receipts/v1/{receipt_id[7:9]}/{receipt_id[7:]}.json',receipt)
    unresolved=[]; processed=[]
    for entry_id,item in observed.items():
        source=expected[entry_id]; outcome=item['outcome']
        if outcome in {'Deleted','AlreadyAbsent'}:
            tomb=root/source['tombstonePath']
            if tomb.is_file(): tomb.unlink()
            processed.append(entry_id)
        else:
            record={'schemaVersion':'correlation-cleanup-unresolved-v1','unresolvedId':'sha256:'+hashlib.sha256((manifest['manifestId']+'|'+entry_id+'|'+outcome).encode()).hexdigest(),'manifestId':manifest['manifestId'],'receiptId':receipt_id,'entryId':entry_id,'correlationId':source['correlationId'],'failureClass':outcome,'recordedAt':a.reconciled_at,'nextAction':'RetryDelete' if outcome=='TemporaryFailure' else 'HumanReview','publicMessageCode':outcome.upper()}
            write(root/f"correlation-runtime/unresolved/v1/{source['correlationId'][:2]}/{record['unresolvedId'][7:]}.json",record); unresolved.append(entry_id)
    status={'schemaVersion':'correlation-cleanup-status-v1','manifestId':manifest['manifestId'],'receiptId':receipt_id,'runtimeCommit':manifest['runtimeCommit'],'reconciledAt':a.reconciled_at,'outcome':'Reconciled' if not unresolved else 'PartiallyReconciled','processedEntryIds':processed}
    write(root/f"correlation-runtime/status/v1/{manifest['manifestId'][7:9]}/{manifest['manifestId'][7:]}.json",status)
    print(json.dumps({'processed':processed,'unresolved':unresolved},sort_keys=True))
if __name__=='__main__': main()

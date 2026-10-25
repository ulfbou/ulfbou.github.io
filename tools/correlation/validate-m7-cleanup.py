#!/usr/bin/env python3
from pathlib import Path
import sys
root=Path(__file__).resolve().parents[2]; workflow=(root/'.github/workflows/correlation-cleanup.yml').read_text()
def req(v,c):
 if not v: print(c,file=sys.stderr);raise SystemExit(1)
req(workflow.count('contents: write')==1,'M7-WRITE-COUNT')
req(workflow.count('id-token: write')==1,'M7-OIDC-COUNT')
req('contents: write\n      id-token: write' not in workflow,'M7-COMBINED-PRIVILEGE')
req('manifest_sha256' in workflow and 'receipt_sha256' in workflow,'M7-DIGEST-HANDOFF')
req('test "$remote_tip" = "$EXPECTED_TIP"' in workflow,'M7-RACE-GATE')
req('force' not in workflow and 'rebase' not in workflow,'M7-DESTRUCTIVE')
req('actions/' not in workflow and 'uses:' not in workflow,'M7-UNPINNED-ACTION')
req(all((root/p).is_file() for p in ['tools/correlation/compile-cleanup-manifest.py','tools/correlation/reconcile-cleanup.py']),'M7-FILES')
print('PASS: M7 cleanup workflow contract')

#!/usr/bin/env python3
from pathlib import Path
import re,sys
root=Path(__file__).resolve().parents[2];workflow=(root/'.github/workflows/correlation-sandbox-deploy.yml').read_text()
def req(v,c):
 if not v: print(c,file=sys.stderr);raise SystemExit(1)
req('contents: write' not in workflow,'M6-CONTENTS-WRITE')
req(workflow.count('id-token: write')==1,'M6-OIDC-COUNT')
req('environment: correlation-prototype' in workflow,'M6-ENVIRONMENT')
req('--duration-seconds 900' in workflow,'M6-SESSION-BOUND')
req('CapabilityDisabled' in workflow,'M6-DISABLED-FIRST')
req('sam validate --lint' in workflow and 'dotnet test' in workflow,'M6-GATES')
req('cancel-in-progress: false' in workflow,'M6-CONCURRENCY')
req('API_SMOKE_TOKEN' in workflow,'M6-SMOKE-GUARD')
req(all((root/p).is_file() for p in ['scripts/correlation/sandbox-smoke.sh','tools/correlation/capture-sandbox-evidence.py']),'M6-FILES')
print('PASS: M6 sandbox deployment contract')

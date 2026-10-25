#!/usr/bin/env python3
from pathlib import Path
import sys
root=Path(__file__).resolve().parents[2];paths=['src/Ulfbou.Correlation.Runtime/RuntimeContracts.cs','src/Ulfbou.Correlation.Runtime/GeminiProvider.cs','src/Ulfbou.Correlation.Runtime/Handlers.cs','tests/Ulfbou.Correlation.Runtime.Tests/RuntimeBoundaryTests.cs']
def req(x,c):
 if not x:print(c,file=sys.stderr);raise SystemExit(1)
req(all((root/p).is_file() for p in paths),'M5-MISSING');text='\n'.join((root/p).read_text() for p in paths)
for m in ['BrowserRequestBytes=8*1024','CleanupRequestBytes=128*1024','CleanupEntries=25','ProviderResponseBytes=16*1024','ProviderQuotaExhausted','CLEANUP-BIJECTION','CapabilityDisabled','Authoritative']:req(m in text,'M5-MARKER-'+m)
for x in ['visitorPrompt','DynamoDB','StepFunctions','ResultArchive']:req(x not in text,'M5-PROHIBITED-'+x)
print('PASS: M5 runtime integration contract')

#!/usr/bin/env python3
from pathlib import Path
import sys
root=Path(__file__).resolve().parents[2]
files=[root/'src/Ulfbou.Correlation.Aws/S3CorrelationStateStore.cs',root/'src/Ulfbou.Correlation.Aws/ActiveStateDocument.cs',root/'src/Ulfbou.Correlation.Aws/Ulfbou.Correlation.Aws.csproj',root/'src/Ulfbou.Correlation.Core/CorrelationDomain.cs']
def req(ok,code):
    if not ok: print(code,file=sys.stderr); raise SystemExit(1)
req(all(p.is_file() for p in files),'M4-MISSING')
text='\n'.join(p.read_text(encoding='utf-8') for p in files)
for marker in ['IfNoneMatch = "*"','IfMatch = observed.ETag','VersionId = observed.VersionId','MaximumBytes = 16 * 1024','AWSSDK.S3','active/v1/']:
    req(marker in text,'M4-MARKER-'+marker)
for prohibited in ['ListObjects','DeleteObjectsAsync','TransferUtility','BucketName = request']:
    req(prohibited not in text,'M4-PROHIBITED-'+prohibited)
solution=(root/'Ulfbou.Portfolio.slnx').read_text(encoding='utf-8')
req('Ulfbou.Correlation.Aws.csproj' in solution and 'Ulfbou.Correlation.Aws.Tests.csproj' in solution,'M4-SOLUTION')
print('PASS: M4 AWS adapter contract')

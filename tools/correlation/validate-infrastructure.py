#!/usr/bin/env python3
import json, sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
template=json.loads((root/'infrastructure/correlation/template.json').read_text(encoding='utf-8'))
resources=template['Resources']
def req(ok,code):
    if not ok:
        print(code,file=sys.stderr); raise SystemExit(1)
req(template.get('Transform')=='AWS::Serverless-2016-10-31','M3-TRANSFORM')
required={'CorrelationApi','CorrelationBucket','RequestFunction','ResultFunction','CleanupFunction','RequestRole','ResultRole','CleanupRole','CleanupInvocationRole','PrototypeBudget'}
req(required <= set(resources),'M3-RESOURCES')
functions=[resources[x]['Properties'] for x in ['RequestFunction','ResultFunction','CleanupFunction']]
req(all(x['Runtime']=='dotnet10' and x['AutoPublishAlias']=='production' for x in functions),'M3-RUNTIME-ALIAS')
req([x['ReservedConcurrentExecutions'] for x in functions]==[2,2,1],'M3-CONCURRENCY')
roles=[resources[x] for x in ['RequestRole','ResultRole','CleanupRole']]
req(len({x['Properties']['RoleName'] for x in roles})==3,'M3-ROLE-SEPARATION')
cleanup=json.dumps(resources['CleanupRole'])
req('s3:DeleteObject' in cleanup and 'active/v1/*' in cleanup and 'secretsmanager' not in cleanup,'M3-CLEANUP-SCOPE')
invoke=json.dumps(resources['CleanupInvocationRole'])
req('lambda:InvokeFunction' in invoke and 's3:' not in invoke and ':production' in invoke,'M3-INVOKE-SCOPE')
req('token.actions.githubusercontent.com:aud' in invoke and 'token.actions.githubusercontent.com:sub' in invoke,'M3-OIDC-CONDITIONS')
api=resources['CorrelationApi']['Properties']
req(api['DefaultRouteSettings']=={'ThrottlingBurstLimit':2,'ThrottlingRateLimit':1},'M3-THROTTLE')
req(resources['CorrelationBucket']['DeletionPolicy']=='Retain','M3-BUCKET-RETENTION')
for logical in ['RequestFunction','ResultFunction','CleanupFunction']:
    req(resources[logical+'LogGroup']['Properties']['RetentionInDays']==7,'M3-LOG-RETENTION')
source=' '.join((root/p).read_text(encoding='utf-8') for p in ['src/Ulfbou.Correlation.RequestFunction/Function.cs','src/Ulfbou.Correlation.ResultFunction/Function.cs','src/Ulfbou.Correlation.CleanupFunction/Function.cs'])
req('CapabilityDisabled' in source,'M3-DISABLED-BY-DEFAULT')
print('PASS: M3 infrastructure contract')

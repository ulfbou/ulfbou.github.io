#!/usr/bin/env python3
import argparse,datetime,hashlib,json,pathlib,subprocess
def aws(*args):
    result=subprocess.run(["aws",*args,"--output","json"],check=True,capture_output=True,text=True)
    return json.loads(result.stdout)
p=argparse.ArgumentParser();p.add_argument("--stack",required=True);p.add_argument("--region",required=True);p.add_argument("--candidate",required=True);p.add_argument("--output",required=True);a=p.parse_args()
stack=aws("cloudformation","describe-stacks","--stack-name",a.stack,"--region",a.region)["Stacks"][0]
resources=aws("cloudformation","describe-stack-resources","--stack-name",a.stack,"--region",a.region)["StackResources"]
allowed={"RequestFunction","ResultFunction","CleanupFunction","CorrelationApi","CorrelationBucket"}
evidence={"schemaVersion":"correlation-sandbox-evidence-v1","capturedAt":datetime.datetime.now(datetime.timezone.utc).isoformat(timespec="seconds"),"candidateSha":a.candidate,"stackId":stack["StackId"],"stackStatus":stack["StackStatus"],"outputs":sorted(stack.get("Outputs",[]),key=lambda x:x["OutputKey"]),"resources":[{"logicalId":x["LogicalResourceId"],"physicalId":x.get("PhysicalResourceId"),"status":x["ResourceStatus"]} for x in resources if x["LogicalResourceId"] in allowed]}
raw=json.dumps(evidence,sort_keys=True,separators=(",",":"))+"\n";path=pathlib.Path(a.output);path.write_text(raw,encoding="utf-8");print(hashlib.sha256(raw.encode()).hexdigest())

#!/usr/bin/env bash
set -euo pipefail
endpoint=${1:?endpoint required}
token=${2:?smoke token required}
payload='{"schemaVersion":"correlation-create-request-v1","scenarioId":"git-backed-correlation-cleanup","scenarioSha256":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","clientOperationId":"smoke-reviewed"}'
response=$(curl --fail-with-body --silent --show-error --max-time 20 --proto '=https' --tlsv1.2 -H 'content-type: application/json' -H "x-correlation-smoke-token: $token" --data "$payload" "$endpoint/correlations")
python - "$response" <<'PY'
import json,sys
value=json.loads(sys.argv[1])
assert value["schemaVersion"]=="correlation-create-response-v1"
assert value["state"]=="Completed"
assert value["result"]["classification"]=="CandidateAnalysis"
assert value["result"]["authoritative"] is False
print("PASS: reviewed sandbox smoke")
PY

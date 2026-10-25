#!/usr/bin/env python3
from html.parser import HTMLParser
from pathlib import Path
import json,sys
root=Path(__file__).resolve().parents[2]
class Parser(HTMLParser):
    def __init__(self): super().__init__(); self.ids=set(); self.tags=[]
    def handle_starttag(self,tag,attrs): self.tags.append(tag); value=dict(attrs).get('id'); self.ids.add(value) if value else None
def req(ok,code):
    if not ok: print(code,file=sys.stderr);raise SystemExit(1)
html=(root/'correlation/index.html').read_text(encoding='utf-8');css=(root/'correlation/styles.css').read_text(encoding='utf-8');js=(root/'correlation/app.js').read_text(encoding='utf-8')
p=Parser();p.feed(html)
req({'main','scenario-panel','availability','analyze','result','replay','limitations'}<=p.ids,'M8-HTML-IDS')
req('noscript' in p.tags and 'skip-link' in html,'M8-ACCESSIBILITY')
for marker in ['Candidate analysis','Static mode','AWS','GitHub','revision-bound']: req(marker in html,'M8-CONTENT-'+marker)
for marker in ['credentials: \'omit\'','Response exceeds 16 KiB','authoritative!==false','setStatic(','runtime-config.json','textContent']: req(marker in js,'M8-JS-'+marker)
for prohibited in ['innerHTML=value','localStorage','sessionStorage','document.cookie','Authorization','apiKey']: req(prohibited not in js,'M8-PROHIBITED-'+prohibited)
req('@media (prefers-reduced-motion:reduce)' in css,'M8-REDUCED-MOTION')
scenario=json.loads((root/'correlation/demo/architecture-scenario-v1.json').read_text()); replay=json.loads((root/'correlation/demo/correlation-public-replay-v1.json').read_text())
req(scenario['classification']=='ReviewedPublicStaticFixture' and replay['classification']=='PublicTransformed','M8-CLASSIFICATION')
serialized=json.dumps([scenario,replay]).lower()
for prohibited in ['aws_access_key_id','private key','authorization header','email address']: req(prohibited not in serialized,'M8-PUBLIC-DATA-'+prohibited)
req(not (root/'correlation/demo/runtime-config.json').exists(),'M8-LIVE-CONFIG-COMMITTED')
print('PASS: M8 degraded-safe portfolio demonstration')

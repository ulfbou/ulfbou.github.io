const scenarioUrl = './demo/architecture-scenario-v1.json';
const replayUrl = './demo/correlation-public-replay-v1.json';
const configurationUrl = './demo/runtime-config.json';
const admittedApiOrigin = /^https:\/\/[a-z0-9]+\.execute-api\.eu-north-1\.amazonaws\.com$/;

const escapeText = value => String(value);
const scenarioPanel = document.querySelector('#scenario-panel');
const replayList = document.querySelector('#replay');
const limitations = document.querySelector('#limitations');
const availability = document.querySelector('#availability');
const analyze = document.querySelector('#analyze');
const result = document.querySelector('#result');
const mode = document.querySelector('#mode-status');
let scenario;
let runtime = null;

async function boundedJson(url, maximumBytes) {
  const response = await fetch(url, { cache: 'no-store', credentials: 'omit' });
  if (!response.ok) throw new Error(`HTTP ${response.status}`);
  const bytes = new Uint8Array(await response.arrayBuffer());
  if (bytes.byteLength > maximumBytes) throw new Error('Response exceeds its public byte limit');
  return JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(bytes));
}

function renderScenario(value) {
  scenarioPanel.replaceChildren();
  const title = document.createElement('h3'); title.textContent = escapeText(value.title);
  const factsHeading = document.createElement('h4'); factsHeading.textContent = 'Established facts';
  const facts = document.createElement('ul'); facts.className = 'facts';
  value.facts.forEach(item => { const li=document.createElement('li'); li.textContent=escapeText(item); facts.append(li); });
  const questionsHeading = document.createElement('h4'); questionsHeading.textContent = 'Questions for candidate analysis';
  const questions = document.createElement('ol'); questions.className = 'questions';
  value.questions.forEach(item => { const li=document.createElement('li'); li.textContent=escapeText(item); questions.append(li); });
  scenarioPanel.append(title,factsHeading,facts,questionsHeading,questions);
}

function renderReplay(value) {
  replayList.replaceChildren(); limitations.replaceChildren();
  value.steps.forEach(step => { const li=document.createElement('li'); const content=document.createElement('div'); const strong=document.createElement('strong'); strong.textContent=escapeText(step.event); const time=document.createElement('time'); time.dateTime=step.occurredAt; time.textContent=new Date(step.occurredAt).toLocaleString(); content.append(strong,time); li.append(content); replayList.append(li); });
  value.limitations.forEach(item => { const li=document.createElement('li'); li.textContent=escapeText(item); limitations.append(li); });
}

function setStatic(reason) {
  runtime=null; analyze.disabled=true; mode.textContent='Static mode'; mode.className='status static';
  availability.textContent=`Optional dynamic analysis is unavailable. ${reason} The reviewed static architecture remains available.`;
}

async function loadRuntime() {
  try {
    const value=await boundedJson(configurationUrl,2048);
    const endpoint=new URL(value.apiEndpoint);
    if (value.schemaVersion!=='correlation-runtime-config-v1' || endpoint.protocol!=='https:' || !admittedApiOrigin.test(endpoint.origin) || endpoint.pathname!=='/production') throw new Error('Runtime configuration is not admitted');
    runtime=value; analyze.disabled=false; mode.textContent='Sandbox available'; mode.className='status live'; availability.textContent='The optional bounded sandbox is configured. Provider output will remain candidate analysis.';
  } catch { setStatic('No admitted runtime configuration was published.'); }
}

analyze.addEventListener('click', async () => {
  if (!runtime || !scenario) return;
  analyze.disabled=true; result.replaceChildren();
  const heading=document.createElement('h3'); heading.textContent='Candidate analysis';
  const waiting=document.createElement('p'); waiting.textContent='Waiting for the bounded sandboxâ€¦'; result.append(heading,waiting);
  const controller=new AbortController(); const timeout=setTimeout(()=>controller.abort(),20000);
  try {
    const body={schemaVersion:'correlation-create-request-v1',scenarioId:scenario.scenarioId,scenarioSha256:runtime.scenarioSha256,clientOperationId:`browser-${crypto.randomUUID().replaceAll('-','')}`};
    const response=await fetch(`${runtime.apiEndpoint}/correlations`,{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify(body),credentials:'omit',referrerPolicy:'no-referrer',signal:controller.signal});
    const bytes=new Uint8Array(await response.arrayBuffer()); if(bytes.byteLength>16384)throw new Error('Response exceeds 16 KiB');
    const value=JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(bytes));
    if(!response.ok)throw new Error(value.code||`HTTP ${response.status}`);
    if(value.schemaVersion!=='correlation-create-response-v1'||value.result?.classification!=='CandidateAnalysis'||value.result?.authoritative!==false)throw new Error('Response classification is not admitted');
    result.replaceChildren(heading); const label=document.createElement('p');label.className='classification';label.textContent='Provider-generated candidate analysis â€” not accepted architectural truth';result.append(label);
    value.result.observations.forEach(observation=>{const section=document.createElement('section');const q=document.createElement('h4');q.textContent=escapeText(observation.question);const assessment=document.createElement('p');assessment.textContent=`Assessment: ${escapeText(observation.assessment)}`;const rationale=document.createElement('p');rationale.textContent=escapeText(observation.rationale);section.append(q,assessment,rationale);result.append(section);});
  } catch (error) {
    result.replaceChildren(heading); const p=document.createElement('p');p.textContent=`Dynamic analysis is unavailable (${escapeText(error.name==='AbortError'?'timeout':error.message)}). The static architecture remains complete.`;result.append(p);
  } finally { clearTimeout(timeout); analyze.disabled=!runtime; }
});

Promise.all([boundedJson(scenarioUrl,8192),boundedJson(replayUrl,16384)])
  .then(([scenarioValue,replayValue])=>{scenario=scenarioValue;renderScenario(scenarioValue);renderReplay(replayValue);})
  .catch(()=>{scenarioPanel.textContent='The static demonstration fixtures could not be loaded.';replayList.replaceChildren(Object.assign(document.createElement('li'),{textContent:'Static replay unavailable.'}));});
loadRuntime();

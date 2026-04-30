window.saos = (function(){
  const l={};
  return {
    on:(e,c)=>{(l[e]??=[]).push(c)},
    emit:(e,d)=>{(l[e]||[]).forEach(f=>f(d))},
    register:m=>window.saos.emit('saos.app.register',m),
    navigate:p=>window.saos.emit('saos.navigate',{path:p})
  };
})();
window.saosDiagInit = (dotnet)=>{
  window.saos.on('saos.app.register', m=>dotnet.invokeMethodAsync('Push','reg',`${m.id} v${m.version||'?'} registered`));
  window.saos.on('saos.navigate', e=>dotnet.invokeMethodAsync('Push','nav',`→ ${e.path}`));
  dotnet.invokeMethodAsync('Push','rdy','kernel handshake OK');
};
window.saosPersist = {
  save: v => localStorage.setItem('saos.view', v),
  load: () => localStorage.getItem('saos.view') || 'overview'
};
document.addEventListener('keydown', e=>{
  if(e.altKey) return;
  const map = {'1':'overview','2':'tree','3':'phases','4':'blazor','5':'architecture','6':'template','7':'msbuild','8':'diagnostics','9':'progress'};
  if(map[e.key]) window.location.search = '?view='+map[e.key];
});

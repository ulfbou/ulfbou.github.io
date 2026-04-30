window.renderSaosTree = function(id, data){
  const el=document.getElementById(id); if(!el) return;
  el.innerHTML = renderTree(data);
};
function renderTree(nodes,prefix=''){
  let h=''; nodes.forEach((n,i)=>{const last=i===nodes.length-1;const c=last?'└── ':'├── ';const p=prefix+(last?' ':'│ ');
    h+=`<div class="tree-item"><div class="branch" onclick="this.parentElement.classList.toggle('collapsed')"><span class="tree-connector">${prefix}${c}</span><span class="tree-icon">${n.icon}</span><span class="tree-name ${n.cls}">${n.name}</span>${n.comment?`<span class="tree-comment"> ${n.comment}</span>`:''}</div>`;
    if(n.children) h+=`<div class="tree-children">${renderTree(n.children,p)}</div>`; h+=`</div>`;}); return h;
}
const treeData = [{"name":"ulfbou.github.io/","cls":"dir","icon":"📁","children":[{"name":"kernel/","cls":"dir","icon":"📁","comment":"← never edited by hand","children":[{"name":"src/","cls":"dir","icon":"📁","children":[{"name":"ipc/","cls":"dir","icon":"📁","children":[{"name":"bus.ts","cls":"file","icon":"📄","comment":"IPC event bus"},{"name":"types.ts","cls":"config","icon":"📄","comment":"SaosEvent, AppManifest"}]},{"name":"router/","cls":"dir","icon":"📁","children":[{"name":"kernelRouter.ts","cls":"file","icon":"📄"}]},{"name":"shell/","cls":"dir","icon":"📁","children":[{"name":"index.html","cls":"special","icon":"🌐","comment":"OS entry point"},{"name":"kernel.ts","cls":"file","icon":"📄"}]}]},{"name":"apps.json","cls":"special","icon":"📋","comment":"← auto-generated only"}]},{"name":"automation/","cls":"dir","icon":"📁","comment":"Automation Plane","children":[{"name":".github/workflows/","cls":"dir","icon":"📁","children":[{"name":"on-app-register.yml","cls":"config","icon":"⚙️","comment":"saos.app.register handler"},{"name":"deploy-kernel.yml","cls":"config","icon":"⚙️"}]},{"name":"scripts/","cls":"dir","icon":"📁","children":[{"name":"update-registry.ts","cls":"file","icon":"📄","comment":"writes apps.json"}]}]},{"name":"packages/","cls":"dir","icon":"📁","comment":"NuGet / npm","children":[{"name":"Saos.Interop/","cls":"dir","icon":"📁","comment":"NuGet → all Blazor apps","children":[{"name":"SaosKernelExtensions.cs","cls":"file","icon":"📄","comment":"AddSaosKernel()"},{"name":"SaosRouterAdapter.cs","cls":"file","icon":"📄"},{"name":"SaosDiagnostics.razor","cls":"file","icon":"📄"},{"name":"ISaosContext.cs","cls":"special","icon":"📄"}]},{"name":"Saos.Templates/","cls":"dir","icon":"📁","comment":"dotnet new saos-blazor","children":[{"name":"template.json","cls":"config","icon":"⚙️"},{"name":"MyApp.csproj","cls":"config","icon":"⚙️","comment":"MSBuild target embedded"},{"name":"Program.cs","cls":"file","icon":"📄"}]}]}]}];
// copy buttons
document.addEventListener('click', e=>{
  if(e.target.classList.contains('copy-btn')){
    const code = e.target.closest('.code-block')?.querySelector('code')?.innerText || '';
    navigator.clipboard.writeText(code);
    e.target.textContent='copied'; setTimeout(()=>e.target.textContent='copy',1200);
  }
});
document.addEventListener('DOMContentLoaded',()=>{ renderSaosTree('repoTree',treeData); renderSaosTree('templateTree',treeData[0].children[2].children[1].children); });

// card navigation via data-navigate
document.addEventListener('click', e=>{
  const card = e.target.closest('[data-navigate]');
  if(card){
    const view = card.getAttribute('data-navigate');
    // use Blazor's navigate by updating query string
    window.location.search = '?view=' + view;
  }
});

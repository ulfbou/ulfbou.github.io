%%DX v1.3 author=tool
%%FILE path="Saos.Roadmap/App.razor" readonly="true"
    <Layout.MainLayout />
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Components/CodeBlock.razor" readonly="true"
    @inject IJSRuntime JS
    <div class="code-block">
    <div class="code-header">
    <span>@Language</span>
    <button @onclick="Copy">@buttonText</button>
    </div>
    <pre>@Code</pre>
    </div>
    
    @code{
    [Parameter] public string Language {get;set;}="text";
    [Parameter] public string Code {get;set;}="";
    string buttonText="copy";
    async Task Copy(){
     await JS.InvokeVoidAsync("navigator.clipboard.writeText", Code);
     buttonText="copied!";
     await Task.Delay(1500);
     buttonText="copy";
     StateHasChanged();
    }
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Components/ProgressBar.razor" readonly="true"
    <div class="progress-item">
    <div>@Label</div>
    <div class="progress-bar"><div class="progress-fill" style="width:@($"{Value}%")"></div></div>
    </div>
    @code{
    [Parameter] public string Label {get;set;}="";
    [Parameter] public int Value {get;set;}
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Components/TreeView.razor" readonly="true"
    @using Saos.Roadmap.Models
    <div class="tree">
    @foreach(var node in Nodes.Select((n,i)=>(n,i)))
    {
        <TreeNodeView Node="node.n" Prefix="" IsLast="@(node.i == Nodes.Count-1)" />
    }
    </div>
    
    @code{
    [Parameter] public List<TreeNode> Nodes {get;set;}=new();
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Layout/MainLayout.razor" readonly="true"
    @using Saos.Roadmap.Views
    
    <div class="titlebar">
      <div>🔴 🟡 🟢</div>
      <div style="flex:1;text-align:center">SAOS / saos-roadmap</div>
      <div>@DateTime.Now.ToString("HH:mm:ss")</div>
    </div>
    
    <div class="shell">
      <div class="sidebar">
        @foreach (var item in items)
        {
          <div class="sidebar-item @(active == item ? "active" : "")" @onclick="() => active = item">@item</div>
        }
      </div>
      <main class="main">
        @switch(active)
        {
          case "overview": <Overview /> break;
          case "architecture": <Architecture /> break;
          case "registry": <Registry /> break;
          case "automation": <Automation /> break;
          case "sdk": <Sdk /> break;
          case "templates": <Templates /> break;
          case "roadmap": <Roadmap /> break;
          case "diagnostics": <Diagnostics /> break;
        }
      </main>
    </div>
    
    @code{
     string active="overview";
     string[] items=["overview","architecture","registry","automation","sdk","templates","roadmap","diagnostics"];
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Models/TreeNode.cs" readonly="true"
    namespace Saos.Roadmap.Models;
    public record TreeNode(string Name,string Cls,string Icon,string? Comment=null,List<TreeNode>? Children=null)
    {
        public bool Collapsed {get;set;}
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Program.cs" readonly="true"
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Saos.Roadmap.Services;
    
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.Services.AddSingleton<ClockService>();
    builder.Services.AddSingleton<DiagnosticsService>();
    await builder.Build().RunAsync();
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Saos.Roadmap.csproj" readonly="true"
    <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.0-*" />
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.0-*" PrivateAssets="all" />
      </ItemGroup>
    </Project>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Services/ClockService.cs" readonly="true"
    namespace Saos.Roadmap.Services;
    public class ClockService
    {
        public string Now => DateTime.Now.ToString("HH:mm:ss");
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Services/DiagnosticsService.cs" readonly="true"
    namespace Saos.Roadmap.Services;
    public class DiagnosticsService
    {
        public List<string> Events {get;} = new();
    }
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Architecture.razor" readonly="true"
    <div class='view-title'>Architecture</div><div class='card'>System architecture visualization.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Automation.razor" readonly="true"
    <div class='view-title'>Automation</div><div class='card'>GitHub actions automation plane.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Diagnostics.razor" readonly="true"
    <div class='view-title'>Diagnostics</div><div class='card'>Live diagnostics stream.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Overview.razor" readonly="true"
    <div class="view-header">
    <div class="view-title">SAOS <span style="color:var(--green)">Infrastructure</span> Roadmap</div>
    </div>
    <div class="card">
    The kernel never contains app code. Apps dispatch a JSON registration event.
    </div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Registry.razor" readonly="true"
    <div class='view-title'>Registry</div><div class='card'>apps.json registry.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Roadmap.razor" readonly="true"
    <div class='view-title'>Roadmap</div><div class='card'>Phase roadmap and progress.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Sdk.razor" readonly="true"
    <div class='view-title'>SDK</div><div class='card'>Blazor + JS SDK information.</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/Views/Templates.razor" readonly="true"
    <div class='view-title'>Templates</div><div class='card'>dotnet new saos-blazor</div>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/wwwroot/css/app.css" readonly="true"
    :root{--bg:#080c10;--surface:#0d1117;--surface2:#161b22;--border:#21262d;--text:#e6edf3;--text-muted:#7d8590;--green:#3fb950;--green-glow:rgba(63,185,80,0.15);--font-mono:'JetBrains Mono', monospace;--font-display:'Syne', sans-serif;}
    body{background:var(--bg);color:var(--text);font-family:var(--font-mono);margin:0}
    body::before{content:'';position:fixed;inset:0;background:repeating-linear-gradient(0deg,transparent,transparent 2px,rgba(0,0,0,.03) 2px,rgba(0,0,0,.03) 4px);pointer-events:none;z-index:9999}
    .titlebar{height:40px;background:var(--surface);display:flex;align-items:center;padding:0 16px;border-bottom:1px solid var(--border)}
    .shell{display:flex;height:calc(100vh - 40px)}
    .sidebar{width:220px;background:var(--surface);border-right:1px solid var(--border);padding:16px 0}
    .sidebar-item{padding:8px 12px;color:var(--text-muted);cursor:pointer}
    .sidebar-item.active{background:var(--green-glow);color:var(--green)}
    .main{flex:1;padding:32px;overflow:auto}
    .view-title{font-family:var(--font-display);font-size:28px;font-weight:800}
    .card{background:var(--surface);border:1px solid var(--border);padding:20px;border-radius:10px;margin-bottom:16px}
    .tree .branch{display:flex;gap:6px;padding:2px 8px;cursor:pointer}
    .progress-bar{height:4px;background:var(--surface2)}
    .progress-fill{height:100%;background:var(--green);transition:width 1s;width:0}
    .code-block{background:var(--surface2);border:1px solid var(--border);border-radius:8px}
    .code-header{display:flex;justify-content:space-between;padding:10px}
    pre{padding:16px;overflow:auto}
%%ENDBLOCK

%%FILE path="Saos.Roadmap/wwwroot/index.html" readonly="true"
    <!DOCTYPE html>
    <html>
    <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SAOS — Static Application Operating System</title>
    <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@300;400;500;700&family=Syne:wght@400;600;700;800&display=swap" rel="stylesheet">
    <link href="css/app.css" rel="stylesheet" />
    </head>
    <body>
    <div id="boot"></div>
    <div id="app">Loading...</div>
    <script src="_framework/blazor.webassembly.js"></script>
    <script src="js/interop.js"></script>
    </body>
    </html>
%%ENDBLOCK

%%FILE path="Saos.Roadmap/wwwroot/js/interop.js" readonly="true"
    window.saosBoot = {
     start: function(dotnet){
       const msgs=['Initializing kernel...','Loading apps.json...','Connecting IPC bus...','Mounting shell...','Ready'];
       let i=0,p=0;
       const t=setInterval(()=>{
          p+=20;
          dotnet.invokeMethodAsync('UpdateBoot', p, msgs[Math.min(i, msgs.length-1)]);
          i++;
          if(p>=100){ clearInterval(t); dotnet.invokeMethodAsync('CompleteBoot');}
       },180);
     }
    };
    window.saosNavigate = (view)=>console.log('navigate',view);
%%ENDBLOCK

%%END

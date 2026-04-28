

window.saosBoot = {
  start: function(dotnetRef) {
    let p = 0;
    const msgs = ["Initializing kernel...","Loading apps.json...","Connecting IPC bus...","Mounting shell...","Ready"];
    const interval = setInterval(() => {
      p += 20;
      const msg = msgs[Math.min(Math.floor(p/20), msgs.length-1)];
      dotnetRef.invokeMethodAsync('UpdateBoot', p, msg);
      const bar = document.getElementById('bootBar');
      if(bar) bar.style.width = p + '%';
      if (p >= 100) {
        clearInterval(interval);
        setTimeout(() => dotnetRef.invokeMethodAsync('BootComplete'), 300);
      }
    }, 180);
  },
  injectOriginal: function() { }
};

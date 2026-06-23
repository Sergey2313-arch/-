(() => {
  const startHash = window.__RM_START_HASH || location.hash || '#home';
  const $ = (s) => document.querySelector(s);
  const $$ = (s) => [...document.querySelectorAll(s)];

  function showPage(page) {
    const id = String(page || 'home').replace('#', '') || 'home';
    const target = $(`#page-${id}`);
    if (!target) return false;
    $$('.page').forEach((item) => item.classList.add('hidden'));
    target.classList.remove('hidden');
    $$('[data-page]').forEach((link) => link.classList.toggle('active', link.dataset.page === id));
    $('#nav')?.classList.remove('open');
    if (location.hash !== `#${id}`) history.replaceState(null, '', `#${id}`);
    return true;
  }

  function bindSafeNavigation() {
    $$('[data-page]').forEach((link) => {
      if (link.dataset.bugfixBound === '1') return;
      link.dataset.bugfixBound = '1';
      link.addEventListener('click', () => {
        setTimeout(() => {
          const id = link.dataset.page;
          if (id && $(`#page-${id}`)) showPage(id);
        }, 0);
      });
    });
  }

  function fixMobileMenu() {
    const menu = $('#menuBtn');
    const nav = $('#nav');
    if (!menu || !nav || menu.dataset.mobileFixed === '1') return;
    menu.dataset.mobileFixed = '1';
    document.addEventListener('click', (event) => {
      if (!nav.classList.contains('open')) return;
      if (nav.contains(event.target) || menu.contains(event.target)) return;
      nav.classList.remove('open');
    });
  }

  function addResetDealsToProfile() {
    const profileActions = $('#profileActions');
    if (!profileActions || $('#resetDealsBtn')) return;
    const btn = document.createElement('button');
    btn.className = 'btn btn-soft';
    btn.id = 'resetDealsBtn';
    btn.type = 'button';
    btn.textContent = 'Сбросить сделки';
    btn.onclick = () => {
      localStorage.removeItem('rm_deals_v1');
      sessionStorage.removeItem('rm_active_deal');
      alert('Demo-сделки сброшены');
    };
    profileActions.appendChild(btn);
  }

  async function boot() {
    try { await import('./deals.js'); } catch (error) { console.warn('Deals module failed', error); }
    bindSafeNavigation();
    fixMobileMenu();
    setTimeout(() => {
      const id = startHash.replace('#', '') || 'home';
      document.querySelector(`[data-page="${id}"]`)?.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
      if ($(`#page-${id}`)) showPage(id);
    }, 120);
    setInterval(() => {
      bindSafeNavigation();
      addResetDealsToProfile();
    }, 1000);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

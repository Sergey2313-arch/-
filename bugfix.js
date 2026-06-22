(() => {
  const START_HASH = window.__RM_START_HASH || location.hash || '#home';

  const $ = (selector) => document.querySelector(selector);
  const $$ = (selector) => [...document.querySelectorAll(selector)];

  function toast(text) {
    const el = $('#toast');
    if (!el) return;
    el.textContent = String(text).slice(0, 140);
    el.classList.remove('hidden');
    clearTimeout(toast.timer);
    toast.timer = setTimeout(() => el.classList.add('hidden'), 2200);
  }

  function safeJson(key, fallback) {
    try {
      return JSON.parse(localStorage.getItem(key) || 'null') || fallback;
    } catch {
      localStorage.removeItem(key);
      return fallback;
    }
  }

  function showPage(page) {
    const id = page.replace('#', '') || 'home';
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

  function normalizeDemoData() {
    const privacy = localStorage.getItem('rm_privacy');
    if (privacy !== 'accepted') return;

    const required = {
      rm_orders: [],
      rm_freelance: [],
      rm_products: [],
      rm_chats: [],
      rm_support: { ticket: 'SUP-1001', messages: [] }
    };

    Object.entries(required).forEach(([key, fallback]) => safeJson(key, fallback));
  }

  function fixForms() {
    ['chatInput', 'supportInput', 'itemTitle', 'itemDesc', 'nameInput', 'emailInput'].forEach((id) => {
      const input = document.getElementById(id);
      if (!input || input.dataset.fixed === '1') return;
      input.dataset.fixed = '1';
      input.addEventListener('input', () => {
        input.value = input.value.replace(/[<>`]/g, '').slice(0, Number(input.maxLength) > 0 ? Number(input.maxLength) : 300);
      });
    });
  }

  function fixSupportTemplates() {
    $$('[data-support-template]').forEach((btn) => {
      if (btn.dataset.templateFixed === '1') return;
      btn.dataset.templateFixed = '1';
      btn.addEventListener('click', () => {
        const input = $('#supportInput');
        if (!input) return;
        input.value = btn.dataset.supportTemplate || '';
        input.focus();
      });
    });
  }

  function addRecoveryActions() {
    const profileActions = $('#profileActions');
    if (!profileActions || $('#resetDemoBtn')) return;

    const button = document.createElement('button');
    button.className = 'btn btn-soft';
    button.id = 'resetDemoBtn';
    button.type = 'button';
    button.textContent = 'Сбросить demo';
    button.addEventListener('click', () => {
      ['rm_orders', 'rm_freelance', 'rm_products', 'rm_chats', 'rm_support'].forEach((key) => localStorage.removeItem(key));
      toast('Demo-данные сброшены. Обнови страницу.');
    });

    profileActions.appendChild(button);
  }

  function handleStartHash() {
    const hash = START_HASH.replace('#', '') || 'home';
    if (hash === 'stats') {
      setTimeout(() => document.querySelector('[data-page="stats"]')?.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true })), 80);
      return;
    }
    if (hash === 'support') {
      setTimeout(() => document.querySelector('[data-page="support"]')?.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true })), 80);
      return;
    }
    if ($(`#page-${hash}`)) setTimeout(() => showPage(hash), 80);
  }

  function polishMobile() {
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

  function boot() {
    normalizeDemoData();
    bindSafeNavigation();
    fixForms();
    fixSupportTemplates();
    polishMobile();
    handleStartHash();

    setInterval(() => {
      bindSafeNavigation();
      fixForms();
      fixSupportTemplates();
      addRecoveryActions();
    }, 1000);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

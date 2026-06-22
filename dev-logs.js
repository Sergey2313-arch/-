(() => {
  const STORAGE_KEY = 'rm_dev_logs';
  const MAX_LOGS = 120;

  const readLogs = () => {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); }
    catch { localStorage.removeItem(STORAGE_KEY); return []; }
  };

  const writeLogs = (logs) => {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(logs.slice(-MAX_LOGS))); }
    catch { /* localStorage may be disabled */ }
  };

  const safe = (value) => String(value ?? '').replace(/[<>`]/g, '').slice(0, 300);

  function log(type, message, data = {}) {
    const item = {
      time: new Date().toLocaleString('ru-RU'),
      type: safe(type),
      message: safe(message),
      data
    };

    const logs = readLogs();
    logs.push(item);
    writeLogs(logs);

    const consoleMethod = type === 'error' ? 'error' : type === 'warn' ? 'warn' : 'log';
    console[consoleMethod](`[ReviMarket:${item.type}] ${item.message}`, data);
    window.dispatchEvent(new CustomEvent('rm:dev-log', { detail: item }));
  }

  window.ReviMarketDev = {
    log,
    getLogs: readLogs,
    clearLogs: () => { localStorage.removeItem(STORAGE_KEY); renderPanel(); log('system', 'Логи очищены'); },
    exportLogs: () => {
      const blob = new Blob([JSON.stringify(readLogs(), null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `revimarket-dev-logs-${Date.now()}.json`;
      a.click();
      URL.revokeObjectURL(url);
      log('system', 'Экспорт dev-логов');
    }
  };

  function css() {
    const style = document.createElement('style');
    style.textContent = `
      .dev-toggle{position:fixed;left:18px;bottom:18px;z-index:9998;border:1px solid rgba(53,255,182,.28);border-radius:999px;padding:10px 13px;background:rgba(4,8,12,.82);color:#bfffe9;box-shadow:0 0 22px rgba(53,255,182,.18);font-size:12px;font-weight:900;backdrop-filter:blur(12px)}
      .dev-panel{position:fixed;left:18px;bottom:70px;z-index:9998;width:min(620px,calc(100vw - 36px));max-height:min(620px,70vh);display:grid;grid-template-rows:auto 1fr auto;border:1px solid rgba(53,255,182,.26);border-radius:22px;background:rgba(5,8,13,.96);box-shadow:0 0 44px rgba(53,255,182,.16),0 0 90px rgba(139,44,255,.12);overflow:hidden;backdrop-filter:blur(16px)}
      .dev-panel.hidden{display:none!important}.dev-head{display:flex;justify-content:space-between;align-items:center;gap:12px;padding:14px 16px;border-bottom:1px solid rgba(255,255,255,.08)}.dev-head b{color:#fff}.dev-head span{color:#a995bd;font-size:12px}
      .dev-list{overflow:auto;padding:12px;display:grid;gap:9px}.dev-log{padding:10px 12px;border:1px solid rgba(255,255,255,.08);border-radius:14px;background:rgba(255,255,255,.045);font-family:Consolas,monospace;font-size:12px;color:#e9ddff}.dev-log[data-type="error"]{border-color:rgba(255,72,113,.36);color:#ffd0d8}.dev-log[data-type="warn"]{border-color:rgba(255,209,102,.36);color:#ffe9b5}.dev-log[data-type="security"]{border-color:rgba(53,255,182,.36);color:#caffef}
      .dev-log small{display:block;margin-bottom:4px;color:#8fb2a5}.dev-actions{display:flex;flex-wrap:wrap;gap:8px;padding:12px;border-top:1px solid rgba(255,255,255,.08)}.dev-actions button{border:1px solid rgba(255,255,255,.10);border-radius:12px;background:rgba(255,255,255,.06);color:#fff;padding:9px 11px;font-weight:900}.dev-actions button:hover{border-color:rgba(53,255,182,.4)}
      @media(max-width:620px){.dev-toggle{left:10px;bottom:10px}.dev-panel{left:10px;bottom:58px;width:calc(100vw - 20px);border-radius:18px}.dev-head{align-items:flex-start;flex-direction:column}}
    `;
    document.head.appendChild(style);
  }

  function buildPanel() {
    if (document.querySelector('#devPanel')) return;

    const toggle = document.createElement('button');
    toggle.className = 'dev-toggle';
    toggle.id = 'devToggle';
    toggle.type = 'button';
    toggle.textContent = 'Dev logs';

    const panel = document.createElement('section');
    panel.className = 'dev-panel hidden';
    panel.id = 'devPanel';
    panel.innerHTML = `
      <div class="dev-head">
        <div><b>ReviMarket Developer Logs</b><br><span>Ctrl + ` + '`' + ` — открыть / закрыть</span></div>
        <span id="devCount">0 events</span>
      </div>
      <div class="dev-list" id="devList"></div>
      <div class="dev-actions">
        <button type="button" id="devRefresh">Обновить</button>
        <button type="button" id="devExport">Экспорт JSON</button>
        <button type="button" id="devClear">Очистить</button>
        <button type="button" id="devClose">Закрыть</button>
      </div>
    `;

    document.body.append(toggle, panel);

    toggle.onclick = () => togglePanel();
    panel.querySelector('#devClose').onclick = () => panel.classList.add('hidden');
    panel.querySelector('#devRefresh').onclick = renderPanel;
    panel.querySelector('#devExport').onclick = () => window.ReviMarketDev.exportLogs();
    panel.querySelector('#devClear').onclick = () => window.ReviMarketDev.clearLogs();
  }

  function renderPanel() {
    const list = document.querySelector('#devList');
    const count = document.querySelector('#devCount');
    if (!list || !count) return;

    const logs = readLogs();
    count.textContent = `${logs.length} events`;
    list.innerHTML = logs.slice().reverse().map((item) => `
      <article class="dev-log" data-type="${safe(item.type)}">
        <small>${safe(item.time)} • ${safe(item.type)}</small>
        <div>${safe(item.message)}</div>
      </article>
    `).join('') || '<article class="dev-log"><small>empty</small><div>Логов пока нет</div></article>';
  }

  function togglePanel() {
    buildPanel();
    const panel = document.querySelector('#devPanel');
    panel.classList.toggle('hidden');
    renderPanel();
  }

  function collectState() {
    const keys = ['rm_privacy','rm_user','rm_orders','rm_freelance','rm_products','rm_chats','rm_support'];
    const state = {};
    keys.forEach((key) => state[key] = localStorage.getItem(key) ? 'exists' : 'empty');
    return state;
  }

  function bindObservers() {
    window.addEventListener('hashchange', () => log('navigation', `Переход: ${location.hash || '#home'}`));
    window.addEventListener('error', (event) => log('error', event.message, { file: event.filename, line: event.lineno }));
    window.addEventListener('unhandledrejection', (event) => log('error', 'Unhandled promise rejection', { reason: String(event.reason) }));
    window.addEventListener('rm:dev-log', renderPanel);

    document.addEventListener('submit', (event) => {
      const id = event.target?.id || 'unknown-form';
      log('form', `Submit: ${id}`);
    }, true);

    document.addEventListener('click', (event) => {
      const page = event.target?.closest?.('[data-page]')?.dataset?.page;
      if (page) log('ui', `Клик по разделу: ${page}`);

      const create = event.target?.closest?.('[data-create]')?.dataset?.create;
      if (create) log('ui', `Открытие формы создания: ${create}`);
    }, true);
  }

  function boot() {
    css();
    buildPanel();
    bindObservers();
    log('system', 'Dev logs запущены', collectState());
    log('security', `Privacy status: ${localStorage.getItem('rm_privacy') || 'not selected'}`);

    document.addEventListener('keydown', (event) => {
      if (event.ctrlKey && event.key === '`') {
        event.preventDefault();
        togglePanel();
      }
    });

    if (location.search.includes('debug=1') || location.hash === '#dev') {
      togglePanel();
    }
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

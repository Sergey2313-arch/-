const state = {
  moderation: [
    { id: 1, type: 'Заказ', title: '6 карточек для Ozon — наушники', reason: 'Новый заказ, нужна проверка описания', status: 'На проверке' },
    { id: 2, type: 'Товар', title: 'Шаблон инфографики WB', reason: 'Проверить авторские права', status: 'Жалоба' },
    { id: 3, type: 'Профиль', title: 'DesignPro_777', reason: 'Подозрительный аватар и одинаковые отзывы', status: 'Риск' },
    { id: 4, type: 'Заказ', title: 'Срочно сделать 20 карточек', reason: 'Слишком низкий бюджет', status: 'На проверке' },
    { id: 5, type: 'Сообщение', title: 'Чат сделки #1024', reason: 'Жалоба на грубость', status: 'Жалоба' },
    { id: 6, type: 'Файл', title: 'archive_result.zip', reason: 'Файл требует проверки перед скачиванием', status: 'Риск' }
  ],
  tickets: [
    { id: 101, subject: 'Не пришёл результат по заказу', user: 'customer_42', agent: 'SupportAgent', priority: 'High', status: 'New' },
    { id: 102, subject: 'Как вывести деньги?', user: 'designer_17', agent: 'FinanceSupport', priority: 'Medium', status: 'InProgress' },
    { id: 103, subject: 'Хочу удалить demo-данные', user: 'revive_user', agent: 'SupportAgent', priority: 'Low', status: 'WaitingUser' },
    { id: 104, subject: 'Заказчик просит работу вне биржи', user: 'designer_21', agent: 'Moderator', priority: 'High', status: 'New' }
  ],
  disputes: [
    { id: 201, subject: 'Заказчик не принимает карточки', amount: '3 000 ₽', status: 'Open', decision: 'Нужна проверка файлов и переписки' },
    { id: 202, subject: 'Исполнитель пропал после оплаты', amount: '5 500 ₽', status: 'Critical', decision: 'Проверить срок, чат и заморозку платежа' }
  ],
  roles: [
    { name: 'Owner', desc: 'Полный доступ ко всему проекту, настройкам, ролям, платежам и безопасности.' },
    { name: 'Admin', desc: 'Управление пользователями, заказами, спорами, поддержкой и модерацией.' },
    { name: 'Moderator', desc: 'Проверка заказов, товаров, профилей, жалоб и нарушений.' },
    { name: 'SupportAgent', desc: 'Ответы пользователям, тикеты, помощь по заказам и аккаунтам.' },
    { name: 'FinanceManager', desc: 'Проверка платежей, комиссий, возвратов и выплат исполнителям.' },
    { name: 'ReadOnly', desc: 'Только просмотр dashboard, статистики и audit log без изменения данных.' }
  ]
};

const STORAGE_KEY = 'revimarket_admin_audit_logs';
const $ = (selector) => document.querySelector(selector);
const $$ = (selector) => [...document.querySelectorAll(selector)];

function safeText(value) {
  return String(value ?? '').replace(/[<>&"']/g, (char) => ({
    '<': '&lt;',
    '>': '&gt;',
    '&': '&amp;',
    '"': '&quot;',
    "'": '&#039;'
  }[char]));
}

function getLogs() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) || [];
  } catch {
    return [];
  }
}

function saveLogs(logs) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(logs.slice(0, 80)));
}

function addLog(action, target, role = 'Owner') {
  const logs = getLogs();
  logs.unshift({
    time: new Date().toLocaleString('ru-RU'),
    action,
    target,
    role
  });
  saveLogs(logs);
  renderAudit();
}

function toast(message) {
  const toastEl = $('#toast');
  toastEl.textContent = message;
  toastEl.classList.add('show');
  clearTimeout(window.__adminToastTimer);
  window.__adminToastTimer = setTimeout(() => toastEl.classList.remove('show'), 2400);
}

function setTab(tabName) {
  $$('.nav-link').forEach((btn) => btn.classList.toggle('active', btn.dataset.tab === tabName));
  $$('.tab').forEach((tab) => tab.classList.toggle('active', tab.id === `tab-${tabName}`));
  location.hash = tabName;
  addLog('AdminTabOpened', tabName);
}

function statusBadge(status) {
  const lower = status.toLowerCase();
  let cls = '';
  if (lower.includes('жалоба') || lower.includes('critical') || lower.includes('риск')) cls = 'bad';
  else if (lower.includes('new') || lower.includes('проверке') || lower.includes('open')) cls = 'warn';
  else if (lower.includes('resolved') || lower.includes('approved')) cls = 'good';
  return `<span class="badge ${cls}">${safeText(status)}</span>`;
}

function renderModeration() {
  const query = ($('#moderationSearch')?.value || '').toLowerCase();
  const items = state.moderation.filter((item) => {
    const text = `${item.type} ${item.title} ${item.reason} ${item.status}`.toLowerCase();
    return text.includes(query);
  });

  $('#moderationTable').innerHTML = items.map((item) => `
    <tr>
      <td>${safeText(item.type)}</td>
      <td><b>${safeText(item.title)}</b></td>
      <td>${safeText(item.reason)}</td>
      <td>${statusBadge(item.status)}</td>
      <td>
        <div class="actions">
          <button class="btn good" data-approve="${item.id}">Одобрить</button>
          <button class="btn" data-fix="${item.id}">На правку</button>
          <button class="btn bad" data-block="${item.id}">Скрыть</button>
        </div>
      </td>
    </tr>
  `).join('');
}

function renderTickets() {
  $('#ticketsGrid').innerHTML = state.tickets.map((ticket) => `
    <article class="ticket">
      <div class="ticket-head">
        <div>
          <p class="eyebrow">ticket #${ticket.id}</p>
          <h3>${safeText(ticket.subject)}</h3>
        </div>
        ${statusBadge(ticket.priority)}
      </div>
      <p>Пользователь: <b>${safeText(ticket.user)}</b></p>
      <p>Агент: ${safeText(ticket.agent)} • Статус: ${statusBadge(ticket.status)}</p>
      <div class="ticket-actions">
        <button class="btn primary" data-take-ticket="${ticket.id}">Взять</button>
        <button class="btn good" data-resolve-ticket="${ticket.id}">Закрыть</button>
        <button class="btn" data-wait-ticket="${ticket.id}">Ждём пользователя</button>
      </div>
    </article>
  `).join('');
}

function renderDisputes() {
  $('#disputesGrid').innerHTML = state.disputes.map((dispute) => `
    <article class="ticket">
      <div class="ticket-head">
        <div>
          <p class="eyebrow">dispute #${dispute.id}</p>
          <h3>${safeText(dispute.subject)}</h3>
        </div>
        ${statusBadge(dispute.status)}
      </div>
      <p>Сумма сделки: <b>${safeText(dispute.amount)}</b></p>
      <p>${safeText(dispute.decision)}</p>
      <div class="ticket-actions">
        <button class="btn primary" data-dispute="designer:${dispute.id}">Деньги исполнителю</button>
        <button class="btn" data-dispute="refund:${dispute.id}">Возврат заказчику</button>
        <button class="btn good" data-dispute="partial:${dispute.id}">Частичное решение</button>
      </div>
    </article>
  `).join('');
}

function renderRoles() {
  $('#rolesGrid').innerHTML = state.roles.map((role) => `
    <article class="role-card">
      <div class="role-head">
        <h3>${safeText(role.name)}</h3>
        <span class="badge">role</span>
      </div>
      <p>${safeText(role.desc)}</p>
    </article>
  `).join('');
}

function renderAudit() {
  const logs = getLogs();
  $('#auditList').innerHTML = logs.length ? logs.map((log) => `
    <article class="log-item">
      <span class="log-time">${safeText(log.time)}</span>
      <span><b class="log-action">${safeText(log.action)}</b> → ${safeText(log.target)}</span>
      <span class="badge">${safeText(log.role)}</span>
    </article>
  `).join('') : '<article class="log-item"><span>Пока нет demo-логов.</span></article>';
}

function recalcMetrics() {
  $('#metricModeration').textContent = state.moderation.length;
  $('#metricTickets').textContent = state.tickets.filter((ticket) => ticket.status !== 'Resolved').length;
  $('#metricDisputes').textContent = state.disputes.length;
}

function bindEvents() {
  $$('.nav-link').forEach((btn) => btn.addEventListener('click', () => setTab(btn.dataset.tab)));
  $$('[data-tab-jump]').forEach((btn) => btn.addEventListener('click', () => setTab(btn.dataset.tabJump)));

  $('#moderationSearch')?.addEventListener('input', renderModeration);

  document.addEventListener('click', (event) => {
    const approveId = event.target.dataset.approve;
    const fixId = event.target.dataset.fix;
    const blockId = event.target.dataset.block;
    const takeTicket = event.target.dataset.takeTicket;
    const resolveTicket = event.target.dataset.resolveTicket;
    const waitTicket = event.target.dataset.waitTicket;
    const dispute = event.target.dataset.dispute;

    if (approveId) {
      const item = state.moderation.find((x) => x.id === Number(approveId));
      if (item) item.status = 'Approved';
      addLog('ModeratorApprovedItem', `moderation:${approveId}`, 'Moderator');
      toast('Объект одобрен');
      renderModeration();
    }

    if (fixId) {
      const item = state.moderation.find((x) => x.id === Number(fixId));
      if (item) item.status = 'NeedsFix';
      addLog('ModeratorSentToFix', `moderation:${fixId}`, 'Moderator');
      toast('Отправлено на правку');
      renderModeration();
    }

    if (blockId) {
      const item = state.moderation.find((x) => x.id === Number(blockId));
      if (item) item.status = 'Hidden';
      addLog('ModeratorHiddenItem', `moderation:${blockId}`, 'Moderator');
      toast('Объект скрыт');
      renderModeration();
    }

    if (takeTicket) {
      const ticket = state.tickets.find((x) => x.id === Number(takeTicket));
      if (ticket) ticket.status = 'InProgress';
      addLog('SupportAgentTookTicket', `ticket:${takeTicket}`, 'SupportAgent');
      toast('Тикет взят в работу');
      renderTickets();
      recalcMetrics();
    }

    if (resolveTicket) {
      const ticket = state.tickets.find((x) => x.id === Number(resolveTicket));
      if (ticket) ticket.status = 'Resolved';
      addLog('SupportAgentResolvedTicket', `ticket:${resolveTicket}`, 'SupportAgent');
      toast('Тикет закрыт');
      renderTickets();
      recalcMetrics();
    }

    if (waitTicket) {
      const ticket = state.tickets.find((x) => x.id === Number(waitTicket));
      if (ticket) ticket.status = 'WaitingUser';
      addLog('SupportAgentWaitsUser', `ticket:${waitTicket}`, 'SupportAgent');
      toast('Статус: ждём пользователя');
      renderTickets();
    }

    if (dispute) {
      addLog('AdminResolvedDisputeDemo', dispute, 'Admin');
      toast('Demo-решение по спору записано в audit log');
    }
  });

  $('#newTicketBtn')?.addEventListener('click', () => {
    const nextId = Math.max(...state.tickets.map((ticket) => ticket.id)) + 1;
    state.tickets.unshift({
      id: nextId,
      subject: 'Новый demo-вопрос пользователя',
      user: `user_${nextId}`,
      agent: 'Unassigned',
      priority: 'Medium',
      status: 'New'
    });
    addLog('SupportTicketCreatedDemo', `ticket:${nextId}`, 'SupportAgent');
    toast('Demo-тикет создан');
    renderTickets();
    recalcMetrics();
  });

  $('#clearAuditBtn')?.addEventListener('click', () => {
    saveLogs([]);
    renderAudit();
    toast('Demo-логи очищены');
  });
}

function init() {
  renderModeration();
  renderTickets();
  renderDisputes();
  renderRoles();
  renderAudit();
  recalcMetrics();
  bindEvents();

  const startTab = location.hash.replace('#', '');
  if (startTab && $(`#tab-${startTab}`)) setTab(startTab);
  else addLog('AdminPanelOpened', 'dashboard');
}

init();

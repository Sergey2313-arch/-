(() => {
  const $ = (selector) => document.querySelector(selector);
  const $$ = (selector) => [...document.querySelectorAll(selector)];

  const safe = (value) => String(value ?? '').replace(/[&<>'"]/g, (char) => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    "'": '&#39;',
    '"': '&quot;'
  }[char]));

  const plain = (value, limit = 160) => String(value ?? '').replace(/[<>'"`]/g, '').trim().slice(0, limit);
  const rub = (value) => new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(value) || 0, 10000000))) + ' ₽';

  function appState() {
    try { return typeof state !== 'undefined' ? state : null; }
    catch { return null; }
  }

  function privacyStatus() {
    const value = localStorage.getItem('rm_privacy');
    if (value === 'accepted') return 'OK';
    if (value === 'declined') return 'OFF';
    return 'Не выбрано';
  }

  function go(page) {
    if (typeof route === 'function') route(page);
    else location.hash = page;
  }

  function create(type) {
    if (typeof openCreate === 'function') openCreate(type);
  }

  function userLabel(user) {
    if (!user) return 'Гость';
    return user.role === 'customer' ? 'Заказчик' : 'Исполнитель';
  }

  function currentData() {
    const app = appState();
    const user = app?.user || null;
    const userName = user ? plain(user.name, 40) : '';
    const orders = app ? [...app.orders, ...app.freelance] : [];
    const products = app ? app.products : [];
    const chats = app ? app.chats : [];
    const myOrders = user ? orders.filter((item) => plain(item.meta, 40) === userName) : [];
    const myProducts = user ? products.filter((item) => plain(item.meta, 40) === userName) : [];
    return { app, user, userName, orders, products, chats, myOrders, myProducts };
  }

  function itemRows(items, type, emptyText) {
    if (!items.length) return `<div class="rm-profile-empty">${safe(emptyText)}</div>`;
    return items.map((item) => `
      <article class="rm-profile-row">
        <div>
          <h3>${safe(item.title)}</h3>
          <p>${safe(item.desc)}</p>
          <div class="rm-profile-meta">
            <span>${type === 'product' ? 'Товар' : 'Заказ'}</span>
            <span>${rub(item.price)}</span>
            <span>${safe(item.cat)}</span>
          </div>
        </div>
        <button class="btn btn-soft" data-rm-open="${type === 'product' ? 'shop' : 'freelance'}">Открыть</button>
      </article>
    `).join('');
  }

  function chatRows(chats, mode) {
    if (!chats.length) return '<div class="rm-profile-empty">Пока нет активных диалогов. После отклика или покупки здесь появится чат.</div>';
    return chats.map((chat) => `
      <article class="rm-profile-row">
        <div>
          <h3>${safe(chat.title)}</h3>
          <p>${mode === 'seller' ? 'Чат с продавцом / исполнителем' : 'Чат с заказчиком'}: ${safe(chat.customer)} ↔ ${safe(chat.creator)}</p>
          <div class="rm-profile-meta">
            <span>${safe(chat.messages?.length || 0)} сообщений</span>
            <span>${mode === 'seller' ? 'продавец' : 'заказчик'}</span>
          </div>
        </div>
        <button class="btn btn-primary" data-rm-chat="${safe(chat.id)}">Открыть чат</button>
      </article>
    `).join('');
  }

  function statusPanel(data) {
    const role = userLabel(data.user);
    const loginText = data.user ? 'Профиль активен в beta-режиме. Дальше сюда добавим рейтинг, отзывы, баланс, сделки и уведомления.' : 'Ты сейчас в режиме гостя. Войди, чтобы создавать заказы, товары и видеть свои чаты.';
    return `
      <div class="rm-profile-row">
        <div>
          <h3>Статус профиля: ${safe(data.user ? 'Активен' : 'Гость')}</h3>
          <p>${safe(loginText)}</p>
          <div class="rm-profile-meta">
            <span>${safe(role)}</span>
            <span>Конфиденциальность: ${safe(privacyStatus())}</span>
            <span>Beta</span>
          </div>
        </div>
        ${data.user ? '<button class="btn btn-soft" data-rm-open="shop">К товарам</button>' : '<button class="btn btn-primary" id="rmProfileLogin">Войти</button>'}
      </div>
    `;
  }

  function renderProfileFixed(active = sessionStorage.getItem('rm_profile_tab') || 'orders') {
    const page = $('#page-profile .profile');
    if (!page) return;

    $('#rmAccountUpgrade')?.remove();
    const old = $('#rmProfileFixed');
    if (old) old.remove();

    const data = currentData();
    const role = userLabel(data.user);
    const displayName = data.user ? data.userName : 'Гость';
    const avatar = data.user ? plain(data.user.name, 1).toUpperCase() : 'RM';
    const tabs = [
      ['orders', 'Мои заказы'],
      ['products', 'Мои товары'],
      ['seller-chat', 'Чат с продавцом'],
      ['customer-chat', 'Чат с заказчиком'],
      ['status', 'Статус профиля']
    ];

    const content = {
      orders: `
        <div class="rm-profile-actions">
          <button class="btn btn-primary" data-rm-create="order">Создать дизайн-заказ</button>
          <button class="btn btn-soft" data-rm-create="freelance">Создать фриланс-заказ</button>
        </div>
        ${itemRows(data.myOrders, 'order', data.user ? 'Ты пока не создавал заказы. Создай первый заказ или фриланс-задачу.' : 'Войди, чтобы видеть свои заказы.')}
      `,
      products: `
        <div class="rm-profile-actions">
          <button class="btn btn-primary" data-rm-create="product">Добавить товар</button>
          <button class="btn btn-soft" data-rm-open="shop">Открыть товары</button>
        </div>
        ${itemRows(data.myProducts, 'product', data.user ? 'Ты пока не добавлял товары. Можно добавить шаблон, аудит или цифровую услугу.' : 'Войди, чтобы видеть свои товары.')}
      `,
      'seller-chat': chatRows(data.chats, 'seller'),
      'customer-chat': chatRows(data.chats, 'customer'),
      status: statusPanel(data)
    };

    const wrapper = document.createElement('div');
    wrapper.id = 'rmProfileFixed';
    wrapper.className = 'rm-profile-fixed';
    wrapper.innerHTML = `
      <section class="rm-profile-top">
        <div class="rm-profile-avatar">${safe(avatar)}</div>
        <div>
          <p class="eyebrow">personal cabinet</p>
          <h2>${safe(displayName)}</h2>
          <p>Личный кабинет ReviMarket: заказы, товары, переписки и статус профиля в одном месте.</p>
        </div>
        <div class="rm-profile-actions">
          ${data.user ? '<button class="btn btn-soft" id="rmProfileLogout">Выйти</button>' : '<button class="btn btn-primary" id="rmProfileLoginTop">Войти</button>'}
        </div>
      </section>

      <section class="rm-profile-stats">
        <div class="rm-profile-stat"><b>${safe(role)}</b><span>роль аккаунта</span></div>
        <div class="rm-profile-stat"><b>${data.myOrders.length}</b><span>моих заказов</span></div>
        <div class="rm-profile-stat"><b>${data.myProducts.length}</b><span>моих товаров</span></div>
        <div class="rm-profile-stat"><b>${safe(privacyStatus())}</b><span>конфиденциальность</span></div>
      </section>

      <nav class="rm-profile-tabs">
        ${tabs.map(([id, title]) => `<button type="button" class="rm-profile-tab ${active === id ? 'active' : ''}" data-rm-tab="${id}">${title}</button>`).join('')}
      </nav>

      <section class="rm-profile-panel">
        ${content[active] || content.orders}
      </section>
    `;

    page.appendChild(wrapper);
    bindProfileFixed();
  }

  function bindProfileFixed() {
    $$('[data-rm-tab]').forEach((button) => {
      button.onclick = () => {
        sessionStorage.setItem('rm_profile_tab', button.dataset.rmTab);
        renderProfileFixed(button.dataset.rmTab);
      };
    });

    $$('[data-rm-create]').forEach((button) => {
      button.onclick = () => create(button.dataset.rmCreate);
    });

    $$('[data-rm-open]').forEach((button) => {
      button.onclick = () => go(button.dataset.rmOpen);
    });

    $$('[data-rm-chat]').forEach((button) => {
      button.onclick = () => {
        const app = appState();
        if (app) app.selectedChatId = button.dataset.rmChat;
        go('chat');
      };
    });

    $('#rmProfileLogin')?.addEventListener('click', () => typeof openAuth === 'function' && openAuth());
    $('#rmProfileLoginTop')?.addEventListener('click', () => typeof openAuth === 'function' && openAuth());
    $('#rmProfileLogout')?.addEventListener('click', () => $('#logoutBtn')?.click());
  }

  function hardOverride() {
    try { renderProfile = () => renderProfileFixed(); } catch {}
  }

  function boot() {
    hardOverride();
    renderProfileFixed();

    setInterval(() => {
      hardOverride();
      $('#rmAccountUpgrade')?.remove();
      if (!$('#page-profile')?.classList.contains('hidden')) {
        if (!$('#rmProfileFixed')) renderProfileFixed();
      }
    }, 350);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

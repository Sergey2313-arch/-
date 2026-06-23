(() => {
  const $ = (selector) => document.querySelector(selector);
  const $$ = (selector) => [...document.querySelectorAll(selector)];

  const navItems = [
    ['shop', 'Каталог'],
    ['freelance', 'Заказы'],
    ['creators', 'Исполнители'],
    ['chat', 'Сообщения'],
    ['profile', 'Профиль']
  ];

  const esc = (value) => String(value ?? '').replace(/[&<>'"]/g, (char) => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    "'": '&#39;',
    '"': '&quot;'
  }[char]));

  function money(value) {
    return new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(value) || 0, 10000000))) + ' ₽';
  }

  function go(page) {
    if (typeof window.route === 'function') window.route(page);
    else if (typeof route === 'function') route(page);
    else location.hash = page;
  }

  function fixNav() {
    const nav = $('#nav');
    if (!nav) return;

    nav.innerHTML = navItems.map(([page, title]) => (
      `<a href="#${page}" data-page="${page}">${title}</a>`
    )).join('');

    nav.querySelectorAll('a[data-page]').forEach((link) => {
      link.addEventListener('click', (event) => {
        event.preventDefault();
        go(link.dataset.page);
      });
    });
  }

  function fixLanding() {
    const eyebrow = $('#page-home .eyebrow');
    const title = $('#page-home h1');
    const text = $('#page-home .hero-text');
    const buttons = $$('#page-home .hero-actions .btn');

    if (eyebrow) eyebrow.textContent = 'российская beta-биржа для работы и цифровых товаров';
    if (title) title.textContent = 'ReviMarket — площадка для заказчиков, исполнителей и продавцов';
    if (text) {
      text.textContent = 'Российская beta-биржа, где можно размещать заказы, находить исполнителей, продавать информационные товары и вести переписку внутри платформы. Сейчас это демо-версия без реальных платежей.';
    }
    if (buttons[0]) {
      buttons[0].textContent = 'Открыть заказы';
      buttons[0].dataset.go = 'freelance';
    }
    if (buttons[1]) {
      buttons[1].textContent = 'Открыть каталог';
      buttons[1].dataset.go = 'shop';
    }
  }

  function fixSections() {
    const shopTitle = $('#page-shop .section-head h2');
    const shopText = $('#page-shop .section-head p:last-child');
    const freelanceTitle = $('#page-freelance .section-head h2');
    const freelanceText = $('#page-freelance .section-head p:last-child');
    const chatTitle = $('#page-chat .section-head h2');

    if (shopTitle) shopTitle.textContent = 'Каталог услуг и товаров';
    if (shopText) shopText.textContent = 'Услуги, шаблоны, цифровые товары, аудиты, макеты и готовые предложения.';
    if (freelanceTitle) freelanceTitle.textContent = 'Биржа заказов';
    if (freelanceText) freelanceText.textContent = 'Заказы, которые нужно выполнить: дизайн, сайты, карточки, тексты, монтаж и простые IT-задачи.';
    if (chatTitle) chatTitle.textContent = 'Сообщения';
  }

  function renderStableProfile() {
    const box = $('#page-profile .profile');
    if (!box) return;

    const user = window.state?.user || state?.user || null;
    const orders = window.state ? [...window.state.orders, ...window.state.freelance] : [];
    const products = window.state?.products || [];
    const chats = window.state?.chats || [];
    const role = user ? (user.role === 'customer' ? 'Заказчик' : 'Исполнитель') : 'Гость';
    const name = user ? user.name : 'Гость';
    const initials = user ? user.name.slice(0, 1).toUpperCase() : 'RM';

    box.innerHTML = `
      <div class="rm-profile-fixed">
        <section class="rm-profile-top">
          <div class="rm-profile-avatar">${esc(initials)}</div>
          <div>
            <p class="eyebrow">personal cabinet</p>
            <h2>${esc(name)}</h2>
            <p>Личный кабинет ReviMarket: заказы, товары, сообщения и статус профиля.</p>
          </div>
          <div class="rm-profile-actions">
            ${user ? '<button class="btn btn-soft" id="stableLogout">Выйти</button>' : '<button class="btn btn-primary" id="stableLogin">Войти</button>'}
          </div>
        </section>

        <section class="rm-profile-stats">
          <div class="rm-profile-stat"><b>${esc(role)}</b><span>роль</span></div>
          <div class="rm-profile-stat"><b>${orders.length}</b><span>доступных заказов</span></div>
          <div class="rm-profile-stat"><b>${products.length}</b><span>товаров в каталоге</span></div>
          <div class="rm-profile-stat"><b>Beta</b><span>статус</span></div>
        </section>

        <nav class="rm-profile-tabs">
          <button class="rm-profile-tab active" data-stable-tab="orders">Мои заказы</button>
          <button class="rm-profile-tab" data-stable-tab="products">Мои товары</button>
          <button class="rm-profile-tab" data-stable-tab="seller">Чат с продавцом</button>
          <button class="rm-profile-tab" data-stable-tab="customer">Чат с заказчиком</button>
          <button class="rm-profile-tab" data-stable-tab="status">Статус профиля</button>
        </nav>

        <section class="rm-profile-panel" id="stableProfilePanel"></section>
      </div>
    `;

    const showTab = (tab) => {
      $$('.rm-profile-tab').forEach((btn) => btn.classList.toggle('active', btn.dataset.stableTab === tab));
      const panel = $('#stableProfilePanel');
      if (!panel) return;

      if (tab === 'orders') {
        panel.innerHTML = `
          <div class="rm-profile-actions"><button class="btn btn-primary" data-stable-create="freelance">Создать заказ</button></div>
          ${orders.slice(0, 4).map((item) => `
            <article class="rm-profile-row"><div><h3>${esc(item.title)}</h3><p>${esc(item.desc)}</p><div class="rm-profile-meta"><span>${money(item.price)}</span><span>${esc(item.cat)}</span></div></div><button class="btn btn-soft" data-stable-go="freelance">Открыть</button></article>
          `).join('') || '<div class="rm-profile-empty">Пока нет заказов.</div>'}
        `;
      }

      if (tab === 'products') {
        panel.innerHTML = `
          <div class="rm-profile-actions"><button class="btn btn-primary" data-stable-create="product">Добавить товар / услугу</button></div>
          ${products.slice(0, 4).map((item) => `
            <article class="rm-profile-row"><div><h3>${esc(item.title)}</h3><p>${esc(item.desc)}</p><div class="rm-profile-meta"><span>${money(item.price)}</span><span>${esc(item.cat)}</span></div></div><button class="btn btn-soft" data-stable-go="shop">Открыть</button></article>
          `).join('') || '<div class="rm-profile-empty">Пока нет товаров.</div>'}
        `;
      }

      if (tab === 'seller' || tab === 'customer') {
        panel.innerHTML = chats.slice(0, 4).map((chat) => `
          <article class="rm-profile-row"><div><h3>${esc(chat.title)}</h3><p>${esc(chat.customer)} ↔ ${esc(chat.creator)}</p><div class="rm-profile-meta"><span>${esc(chat.messages?.length || 0)} сообщений</span></div></div><button class="btn btn-primary" data-stable-go="chat">Открыть чат</button></article>
        `).join('') || '<div class="rm-profile-empty">Пока нет сообщений.</div>';
      }

      if (tab === 'status') {
        panel.innerHTML = `
          <article class="rm-profile-row"><div><h3>Статус: ${user ? 'активен' : 'гость'}</h3><p>Сейчас это beta-режим. Реальные роли, баланс, сделки и отзывы появятся после backend.</p><div class="rm-profile-meta"><span>${esc(role)}</span><span>Beta</span></div></div>${user ? '<button class="btn btn-soft" id="stableLogout2">Выйти</button>' : '<button class="btn btn-primary" id="stableLogin2">Войти</button>'}</article>
        `;
      }

      bindPanelActions();
    };

    $$('.rm-profile-tab').forEach((button) => {
      button.addEventListener('click', () => showTab(button.dataset.stableTab));
    });

    $('#stableLogin')?.addEventListener('click', () => window.openAuth ? window.openAuth() : openAuth());
    $('#stableLogout')?.addEventListener('click', () => $('#logoutBtn')?.click());
    showTab('orders');
  }

  function bindPanelActions() {
    $$('[data-stable-go]').forEach((button) => button.onclick = () => go(button.dataset.stableGo));
    $$('[data-stable-create]').forEach((button) => button.onclick = () => {
      if (typeof window.openCreate === 'function') window.openCreate(button.dataset.stableCreate);
      else if (typeof openCreate === 'function') openCreate(button.dataset.stableCreate);
    });
    $('#stableLogin2')?.addEventListener('click', () => window.openAuth ? window.openAuth() : openAuth());
    $('#stableLogout2')?.addEventListener('click', () => $('#logoutBtn')?.click());
  }

  function hookRoute() {
    const originalRoute = window.route || (typeof route === 'function' ? route : null);
    if (!originalRoute || window.__stableRouteHooked) return;
    window.__stableRouteHooked = true;
    window.route = function(page) {
      originalRoute(page);
      if (page === 'profile') setTimeout(renderStableProfile, 0);
      setTimeout(() => {
        fixNav();
        fixSections();
      }, 0);
    };
  }

  function boot() {
    fixNav();
    fixLanding();
    fixSections();
    hookRoute();
    if (location.hash === '#profile') renderStableProfile();
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

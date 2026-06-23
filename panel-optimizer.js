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

  function getState() {
    try {
      return typeof state !== 'undefined' ? state : null;
    } catch {
      return null;
    }
  }

  function hasPrivacy() {
    return localStorage.getItem('rm_privacy') === 'accepted';
  }

  function runRoute(page) {
    if (typeof route === 'function') route(page);
    else location.hash = page;
  }

  function optimizeNavigation() {
    const nav = $('#nav');
    if (!nav) return;

    const desired = [
      ['shop', 'Каталог'],
      ['freelance', 'Заказы'],
      ['creators', 'Исполнители'],
      ['chat', 'Сообщения'],
      ['profile', 'Профиль']
    ];
    const allowed = desired.map(([page]) => page);

    nav.querySelectorAll('a[data-page]').forEach((link) => {
      if (!allowed.includes(link.dataset.page)) link.remove();
    });

    desired.forEach(([page, text]) => {
      let link = nav.querySelector(`a[data-page="${page}"]`);
      if (!link) {
        link = document.createElement('a');
        link.href = `#${page}`;
        link.dataset.page = page;
      }
      link.textContent = text;
      link.classList.remove('auth-only', 'hidden');
      link.onclick = (event) => {
        event.preventDefault();
        runRoute(page);
      };
      nav.appendChild(link);
    });
  }

  function renameMainSections() {
    const shopTitle = $('#page-shop .section-head h2');
    const shopText = $('#page-shop .section-head p:last-child');
    const shopSearch = $('#productsSearch');
    const shopButton = $('#page-shop [data-create="product"]');

    if (shopTitle) shopTitle.textContent = 'Каталог услуг и товаров';
    if (shopText) shopText.textContent = 'Услуги, цифровые товары, шаблоны, аудиты, макеты и готовые предложения.';
    if (shopSearch) shopSearch.placeholder = 'Поиск по каталогу: шаблон, аудит, услуга...';
    if (shopButton) shopButton.textContent = '+ Товар / услуга';

    const workTitle = $('#page-freelance .section-head h2');
    const workText = $('#page-freelance .section-head p:last-child');
    const workSearch = $('#freelanceSearch');
    const workButton = $('#page-freelance [data-create="freelance"]');

    if (workTitle) workTitle.textContent = 'Биржа заказов';
    if (workText) workText.textContent = 'Заказы, которые нужно выполнить: дизайн, сайты, карточки, тексты, монтаж и простые IT-задачи.';
    if (workSearch) workSearch.placeholder = 'Поиск заказа: сайт, карточки, текст, монтаж...';
    if (workButton) workButton.textContent = '+ Заказ';

    const heroActions = $$('#page-home .hero-actions .btn');
    if (heroActions[0]) {
      heroActions[0].textContent = 'Открыть заказы';
      heroActions[0].dataset.go = 'freelance';
    }
    if (heroActions[1]) {
      heroActions[1].textContent = 'Открыть каталог';
      heroActions[1].dataset.go = 'shop';
    }
  }

  function itemHtml(item, type, emptyText) {
    if (!item) return `<div class="profile-empty">${safe(emptyText)}</div>`;
    const label = type === 'product' ? 'Товар' : 'Заказ';
    return `
      <article class="profile-item">
        <div>
          <h3>${safe(item.title)}</h3>
          <p>${safe(item.desc)}</p>
          <div class="profile-item-meta">
            <span>${label}</span>
            <span>${rub(item.price)}</span>
            <span>${safe(item.cat)}</span>
          </div>
        </div>
        <button class="btn btn-soft" data-profile-open="${type}">Открыть</button>
      </article>
    `;
  }

  function listHtml(items, type, emptyText) {
    if (!items.length) return `<div class="profile-empty">${safe(emptyText)}</div>`;
    return `<div class="profile-list">${items.map((item) => itemHtml(item, type, emptyText)).join('')}</div>`;
  }

  function chatListHtml(chats, mode) {
    if (!chats.length) return '<div class="profile-empty">Пока нет активных диалогов.</div>';
    return `<div class="profile-list">${chats.map((chat) => `
      <article class="profile-item">
        <div>
          <h3>${safe(chat.title)}</h3>
          <p>${mode === 'seller' ? 'Диалог с продавцом / исполнителем' : 'Диалог с заказчиком'}: ${safe(chat.customer)} ↔ ${safe(chat.creator)}</p>
          <div class="profile-item-meta">
            <span>${safe(chat.messages?.length || 0)} сообщений</span>
            <span>${mode === 'seller' ? 'Продавец' : 'Заказчик'}</span>
          </div>
        </div>
        <button class="btn btn-primary" data-profile-chat="${safe(chat.id)}">Открыть чат</button>
      </article>
    `).join('')}</div>`;
  }

  function renderProfileTabs(activeTab = 'orders') {
    const app = getState();
    const actions = $('#profileActions');
    if (!app || !actions) return;

    const user = app.user;
    const userName = user ? plain(user.name, 40) : '';
    const myOrders = user ? [...app.orders, ...app.freelance].filter((item) => plain(item.meta, 40) === userName) : [];
    const myProducts = user ? app.products.filter((item) => plain(item.meta, 40) === userName) : [];
    const role = user ? (user.role === 'customer' ? 'Заказчик' : 'Исполнитель') : 'Гость';
    const profileStatus = user ? 'Активен' : 'Гость';
    const privacyStatus = hasPrivacy() ? 'Принята' : 'Не принята';
    const publishCount = myOrders.length + myProducts.length;

    if (!user) {
      $('#avatar').textContent = 'RM';
      $('#profileName').textContent = 'Гость';
      $('#profileInfo').textContent = hasPrivacy()
        ? 'Войди в beta-профиль, чтобы создавать заказы, товары и видеть свои чаты.'
        : 'Сначала прими конфиденциальность, потом можно войти в demo-профиль.';
    } else {
      $('#avatar').textContent = plain(user.name, 1).toUpperCase() || 'U';
      $('#profileName').textContent = userName;
      $('#profileInfo').textContent = `${role} • профиль ReviMarket beta`;
    }

    const tabs = [
      ['orders', 'Мои заказы'],
      ['products', 'Мои товары'],
      ['seller-chat', 'Чат с продавцом'],
      ['customer-chat', 'Чат с заказчиком'],
      ['status', 'Статус профиля']
    ];

    actions.innerHTML = `
      <div class="profile-tabs-card">
        <div class="profile-tabs">
          ${tabs.map(([id, title]) => `<button type="button" class="profile-tab-btn ${id === activeTab ? 'active' : ''}" data-profile-tab="${id}">${title}</button>`).join('')}
        </div>

        <section class="profile-tab-content ${activeTab === 'orders' ? 'active' : ''}" data-profile-pane="orders">
          <div class="profile-actions-row">
            <button class="btn btn-primary" data-create="order">Создать дизайн-заказ</button>
            <button class="btn btn-soft" data-create="freelance">Создать фриланс-заказ</button>
          </div>
          ${listHtml(myOrders, 'order', user ? 'Ты пока не создавал заказы. Нажми кнопку выше, чтобы добавить первый.' : 'Войди, чтобы видеть свои заказы.')}
        </section>

        <section class="profile-tab-content ${activeTab === 'products' ? 'active' : ''}" data-profile-pane="products">
          <div class="profile-actions-row">
            <button class="btn btn-primary" data-create="product">Добавить товар / услугу</button>
            <button class="btn btn-soft" data-go="shop">Открыть каталог</button>
          </div>
          ${listHtml(myProducts, 'product', user ? 'Ты пока не добавлял товары. Можно добавить шаблон, аудит или цифровую услугу.' : 'Войди, чтобы видеть свои товары.')}
        </section>

        <section class="profile-tab-content ${activeTab === 'seller-chat' ? 'active' : ''}" data-profile-pane="seller-chat">
          ${chatListHtml(app.chats, 'seller')}
        </section>

        <section class="profile-tab-content ${activeTab === 'customer-chat' ? 'active' : ''}" data-profile-pane="customer-chat">
          ${chatListHtml(app.chats, 'customer')}
        </section>

        <section class="profile-tab-content ${activeTab === 'status' ? 'active' : ''}" data-profile-pane="status">
          <div class="profile-status-grid">
            <div class="profile-status-card"><b>${safe(profileStatus)}</b><span>статус</span></div>
            <div class="profile-status-card"><b>${safe(role)}</b><span>роль</span></div>
            <div class="profile-status-card"><b>${publishCount}</b><span>моих публикаций</span></div>
            <div class="profile-status-card"><b>${safe(privacyStatus)}</b><span>конфиденциальность</span></div>
          </div>
          <div class="profile-actions-row">
            ${user ? '<button class="btn btn-soft" data-go="support">Поддержка</button>' : '<button class="btn btn-primary" id="profileLoginOptimized">Войти</button>'}
            <button class="btn btn-soft" id="profilePrivacyOptimized">Конфиденциальность</button>
          </div>
        </section>
      </div>
    `;

    bindProfileTabs();
  }

  function bindProfileTabs() {
    $$('[data-profile-tab]').forEach((button) => {
      button.onclick = () => renderProfileTabs(button.dataset.profileTab);
    });

    $$('[data-create]').forEach((button) => {
      button.onclick = () => {
        if (typeof openCreate === 'function') openCreate(button.dataset.create);
      };
    });

    $$('[data-go]').forEach((button) => {
      button.onclick = () => runRoute(button.dataset.go);
    });

    $$('[data-profile-open]').forEach((button) => {
      button.onclick = () => runRoute(button.dataset.profileOpen === 'product' ? 'shop' : 'freelance');
    });

    $$('[data-profile-chat]').forEach((button) => {
      button.onclick = () => {
        const app = getState();
        if (app) app.selectedChatId = button.dataset.profileChat;
        runRoute('chat');
      };
    });

    const login = $('#profileLoginOptimized');
    if (login) login.onclick = () => {
      if (typeof openAuth === 'function') openAuth();
    };

    const privacy = $('#profilePrivacyOptimized');
    if (privacy) privacy.onclick = () => {
      if (typeof openPrivacy === 'function') openPrivacy();
    };
  }

  function overrideProfileRenderer() {
    try {
      renderProfile = () => renderProfileTabs('orders');
    } catch (error) {
      console.warn('Profile renderer override failed', error);
    }
  }

  function boot() {
    optimizeNavigation();
    renameMainSections();
    overrideProfileRenderer();

    if (location.hash === '#profile' || !$('#profileActions')?.innerHTML.trim()) {
      renderProfileTabs('orders');
    }

    let retries = 0;
    const timer = setInterval(() => {
      optimizeNavigation();
      renameMainSections();
      retries += 1;
      if (retries > 10) clearInterval(timer);
    }, 350);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

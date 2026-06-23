(() => {
  const $ = (s) => document.querySelector(s);
  const $$ = (s) => [...document.querySelectorAll(s)];

  const categories = [
    {id:'market-design', icon:'🛒', title:'Дизайн маркетплейсов', desc:'Карточки WB/Ozon, инфографика, баннеры, упаковка товара.', tags:['WB','Ozon','Figma'], target:'orders', query:'WB Ozon карточки'},
    {id:'it', icon:'💻', title:'Разработка и IT', desc:'Сайты, лендинги, боты, багфиксы, ASP.NET, JavaScript.', tags:['HTML','JS','ASP.NET'], target:'freelance', query:'сайт лендинг'},
    {id:'ai', icon:'🤖', title:'Нейросети и AI', desc:'AI-аватары, генерация картинок, промпты, автоматизация.', tags:['AI','Prompt','Bot'], target:'freelance', query:'AI бот'},
    {id:'content', icon:'✍️', title:'Тексты и контент', desc:'Описания товаров, статьи, посты, переводы, продающие тексты.', tags:['Text','SEO','Cards'], target:'freelance', query:'тексты'},
    {id:'seo', icon:'📈', title:'SEO и трафик', desc:'Ключевые слова, аудит, продвижение, карточки и сайты.', tags:['SEO','Traffic','Audit'], target:'shop', query:'аудит'},
    {id:'social', icon:'📱', title:'Соцсети и маркетинг', desc:'VK, Telegram, сторис, посты, оформление групп и каналов.', tags:['VK','TG','SMM'], target:'orders', query:'соцсети'},
    {id:'video', icon:'🎬', title:'Видео и монтаж', desc:'Shorts, Reels, клипы, субтитры, обложки и промо-ролики.', tags:['Video','Shorts','Edit'], target:'freelance', query:'монтаж'},
    {id:'business', icon:'📄', title:'Бизнес и документы', desc:'Презентации, резюме, заявки, таблицы, коммерческие тексты.', tags:['Docs','PDF','CV'], target:'freelance', query:'документы'},
    {id:'digital', icon:'📦', title:'Цифровые товары', desc:'Шаблоны, иконки, макеты, наборы, готовые дизайн-паки.', tags:['Templates','SVG','Figma'], target:'shop', query:'шаблон'},
    {id:'urgent', icon:'⚡', title:'Срочные заказы', desc:'Задачи на сегодня: быстро, понятно, с повышенной комиссией.', tags:['Fast','Today','12%'], target:'proposals', query:''}
  ];

  const defaultProposals = [
    {title:'Сделаю 8 карточек товара для WB/Ozon', category:'Дизайн маркетплейсов', price:3500, days:2, owner:'Nika Visual', text:'Подготовлю стильные карточки, инфографику и первый экран товара.', rating:'4.9'},
    {title:'Соберу лендинг под услугу или товар', category:'Разработка и IT', price:12000, days:5, owner:'Dmitry UI', text:'Главный экран, блоки преимуществ, адаптив и базовая форма заявки.', rating:'4.8'},
    {title:'Сделаю AI-аватар, обложку или промо-арт', category:'Нейросети и AI', price:2500, days:1, owner:'Max Neon', text:'Генерация визуала в неоновом стиле, подготовка под соцсети.', rating:'4.7'},
    {title:'Напишу продающее описание карточки товара', category:'Тексты и контент', price:1500, days:1, owner:'Alina Cards', text:'Описание, преимущества, оффер и рекомендации по первому экрану.', rating:'4.8'}
  ];

  function html(value) {
    return String(value || '').replace(/[&<>"']/g, (c) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  }

  function cut(value, limit = 160) {
    return String(value || '').replace(/[<>"'`]/g, '').trim().slice(0, limit);
  }

  function rub(value) {
    return new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(value) || 0, 10000000))) + ' ₽';
  }

  function acceptedPrivacy() {
    return localStorage.getItem('rm_privacy') === 'accepted';
  }

  function readJson(key, fallback) {
    try {
      const data = JSON.parse(localStorage.getItem(key) || 'null');
      return Array.isArray(data) ? data : fallback;
    } catch {
      localStorage.removeItem(key);
      return fallback;
    }
  }

  function saveJson(key, value) {
    if (!acceptedPrivacy()) return false;
    localStorage.setItem(key, JSON.stringify(value));
    return true;
  }

  function user() {
    try { if (typeof state !== 'undefined' && state.user) return state.user; } catch {}
    try { return JSON.parse(localStorage.getItem('rm_user') || 'null'); } catch { return null; }
  }

  function notify(text) {
    try { if (typeof toast === 'function') return toast(text); } catch {}
    alert(text);
  }

  function go(page) {
    if (page === 'categories' || page === 'proposals') return showExtraPage(page);
    try { if (typeof route === 'function') return route(page); } catch {}
    showExtraPage(page);
  }

  function showExtraPage(page) {
    const id = String(page || 'home').replace('#', '') || 'home';
    const target = $('#page-' + id);
    if (!target) return false;
    $$('.page').forEach((item) => item.classList.add('hidden'));
    target.classList.remove('hidden');
    $$('[data-page]').forEach((link) => link.classList.toggle('active', link.dataset.page === id));
    $('#nav')?.classList.remove('open');
    if (location.hash !== '#' + id) history.replaceState(null, '', '#' + id);
    if (id === 'categories') renderCategories();
    if (id === 'proposals') renderProposals();
    if (id === 'profile') patchProfile();
    return true;
  }

  function addNavLink(page, title, beforeSelector) {
    const nav = $('#nav');
    if (!nav || nav.querySelector('[data-page="' + page + '"]')) return;
    const link = document.createElement('a');
    link.href = '#' + page;
    link.dataset.page = page;
    link.textContent = title;
    const before = beforeSelector ? nav.querySelector(beforeSelector) : null;
    nav.insertBefore(link, before || nav.querySelector('[data-page="profile"]'));
    link.addEventListener('click', (event) => {
      event.preventDefault();
      showExtraPage(page);
    });
  }

  function ensurePages() {
    const main = $('main');
    if (!main) return;

    addNavLink('categories', 'Категории', '[data-page="orders"]');
    addNavLink('proposals', 'Предложения', '[data-page="chat"]');

    if (!$('#page-categories')) {
      const section = document.createElement('section');
      section.className = 'page hidden';
      section.id = 'page-categories';
      section.innerHTML = `
        <div class="section-head">
          <div>
            <p class="eyebrow">marketplace categories</p>
            <h2>Категории работ</h2>
            <p>Как на больших биржах, только под ReviMarket: IT, дизайн маркетплейсов, AI, тексты, видео, документы и цифровые товары.</p>
          </div>
          <button class="btn btn-primary" data-extra-go="proposals">Смотреть предложения</button>
        </div>
        <div class="rm-category-grid" id="rmCategoriesGrid"></div>
      `;
      main.appendChild(section);
    }

    if (!$('#page-proposals')) {
      const section = document.createElement('section');
      section.className = 'page hidden';
      section.id = 'page-proposals';
      section.innerHTML = `
        <div class="section-head">
          <div>
            <p class="eyebrow">creator offers</p>
            <h2>Предложения исполнителей</h2>
            <p>Исполнитель может выставить готовую услугу: цена, срок, категория и быстрый переход в чат.</p>
          </div>
          <button class="btn btn-primary auth-only hidden" id="rmProposalFocus">+ Предложение</button>
        </div>
        <form class="rm-upgrade-form neon-card auth-only hidden" id="rmProposalForm">
          <div class="form-row">
            <input id="rmProposalTitle" placeholder="Название предложения" required maxlength="80" />
            <input id="rmProposalPrice" type="number" min="100" step="100" value="3000" required />
            <input id="rmProposalDays" type="number" min="1" max="60" value="2" required />
          </div>
          <select id="rmProposalCategory"></select>
          <textarea id="rmProposalText" placeholder="Что входит в услугу, что получит заказчик..." required maxlength="260"></textarea>
          <button class="btn btn-primary" type="submit">Опубликовать предложение</button>
        </form>
        <div class="rm-proposals-grid" id="rmProposalsGrid"></div>
      `;
      main.appendChild(section);
    }
  }

  function categoryCard(category, small = false) {
    const cls = small ? 'rm-mini-category' : 'rm-cat-card';
    const iconCls = small ? 'rm-mini-icon' : 'rm-cat-icon';
    return `
      <article class="${cls}">
        <div class="${iconCls}">${category.icon}</div>
        <h3>${html(category.title)}</h3>
        <p>${html(category.desc)}</p>
        <div class="rm-cat-meta">${category.tags.map((tag) => `<span>${html(tag)}</span>`).join('')}</div>
        <button class="btn btn-soft" data-cat-target="${html(category.target)}" data-cat-query="${html(category.query)}">Открыть</button>
      </article>
    `;
  }

  function renderCategories() {
    const grid = $('#rmCategoriesGrid');
    if (!grid) return;
    grid.innerHTML = categories.map((cat) => categoryCard(cat)).join('');
    bindCategoryButtons();
  }

  function injectHomeCategories() {
    const hero = $('#page-home .hero-info');
    if (!hero || $('#rmHomeCats')) return;
    const block = document.createElement('div');
    block.id = 'rmHomeCats';
    block.innerHTML = `
      <div class="rm-home-cats">
        ${categories.slice(0, 5).map((cat) => categoryCard(cat, true)).join('')}
      </div>
    `;
    hero.appendChild(block);
    bindCategoryButtons();
  }

  function bindCategoryButtons() {
    $$('[data-cat-target]').forEach((button) => {
      if (button.dataset.catBound === '1') return;
      button.dataset.catBound = '1';
      button.addEventListener('click', () => {
        const target = button.dataset.catTarget || 'freelance';
        const query = button.dataset.catQuery || '';
        go(target);
        setTimeout(() => {
          const searchMap = {
            orders: '#ordersSearch',
            freelance: '#freelanceSearch',
            shop: '#productsSearch'
          };
          const input = $(searchMap[target]);
          if (input && query) {
            input.value = query;
            input.dispatchEvent(new Event('input', {bubbles: true}));
          }
        }, 80);
      });
    });
  }

  function proposalData() {
    return readJson('rm_proposals', defaultProposals);
  }

  function renderProposals() {
    const form = $('#rmProposalForm');
    const select = $('#rmProposalCategory');
    const grid = $('#rmProposalsGrid');
    if (!grid) return;

    if (select && !select.dataset.ready) {
      select.innerHTML = categories.map((cat) => `<option value="${html(cat.title)}">${html(cat.title)}</option>`).join('');
      select.dataset.ready = '1';
    }

    const list = proposalData();
    grid.innerHTML = list.length ? list.map((item, index) => `
      <article class="rm-proposal-card">
        <div class="card-top"><span class="tag">${html(item.category)}</span><span class="price">${rub(item.price)}</span></div>
        <h3>${html(item.title)}</h3>
        <p>${html(item.text)}</p>
        <div class="rm-proposal-meta">
          <span>⏱ ${html(item.days)} дн.</span>
          <span>★ ${html(item.rating || 'new')}</span>
          <span>${html(item.owner || 'Исполнитель')}</span>
        </div>
        <div class="card-bottom">
          <button class="btn btn-soft" data-extra-chat="${index}">Написать</button>
          <button class="btn btn-primary" data-extra-order="${index}">Заказать</button>
        </div>
      </article>
    `).join('') : '<div class="card">Предложений пока нет</div>';

    if (form && !form.dataset.bound) {
      form.dataset.bound = '1';
      form.addEventListener('submit', (event) => {
        event.preventDefault();
        const currentUser = user();
        if (!acceptedPrivacy()) {
          notify('Сначала прими конфиденциальность');
          return;
        }
        if (!currentUser) {
          try { if (typeof openAuth === 'function') openAuth(); } catch {}
          notify('Сначала войди, чтобы публиковать предложения');
          return;
        }
        const next = proposalData();
        next.unshift({
          title: cut($('#rmProposalTitle')?.value, 80),
          category: cut($('#rmProposalCategory')?.value, 60),
          price: Math.max(100, Math.min(Number($('#rmProposalPrice')?.value) || 0, 10000000)),
          days: Math.max(1, Math.min(Number($('#rmProposalDays')?.value) || 1, 60)),
          owner: cut(currentUser.name, 40),
          ownerEmail: cut(currentUser.email, 80),
          text: cut($('#rmProposalText')?.value, 260),
          rating: 'new'
        });
        saveJson('rm_proposals', next);
        form.reset();
        renderProposals();
        patchProfile();
        notify('Предложение опубликовано');
      });
    }

    $('#rmProposalFocus')?.addEventListener('click', () => $('#rmProposalTitle')?.focus(), {once: true});

    $$('[data-extra-chat], [data-extra-order]').forEach((button) => {
      if (button.dataset.extraBound === '1') return;
      button.dataset.extraBound = '1';
      button.addEventListener('click', () => {
        if (!user()) {
          try { if (typeof openAuth === 'function') openAuth(); } catch {}
          notify('Сначала войди');
          return;
        }
        notify(button.dataset.extraOrder ? 'Демо-заказ создан. В полной версии тут будет безопасная сделка.' : 'Открываю чат с исполнителем');
        go('chat');
      });
    });
  }

  function patchAuthVisibility() {
    const logged = !!user();
    $$('.auth-only').forEach((el) => el.classList.toggle('hidden', !logged));
    $$('.guest-only').forEach((el) => el.classList.toggle('hidden', logged));
  }

  function patchProfile() {
    const actions = $('#profileActions');
    if (!actions) return;
    const old = $('#rmAccountUpgrade');
    if (old) old.remove();

    const currentUser = user();
    const proposals = proposalData();
    const myProposals = currentUser ? proposals.filter((p) => p.ownerEmail === currentUser.email || p.owner === currentUser.name) : [];
    const platformOrders = (() => {
      try { return (state.orders?.length || 0) + (state.freelance?.length || 0); } catch { return 0; }
    })();
    const platformProducts = (() => {
      try { return state.products?.length || 0; } catch { return 0; }
    })();
    const roleName = currentUser ? (currentUser.role === 'customer' ? 'Заказчик' : 'Исполнитель') : 'Гость';
    const demoBalance = myProposals.reduce((sum, item) => sum + Number(item.price || 0), 0);

    const block = document.createElement('div');
    block.className = 'rm-account-upgrade neon-card';
    block.id = 'rmAccountUpgrade';
    block.innerHTML = `
      <div class="rm-account-head">
        <div>
          <p class="eyebrow">personal cabinet</p>
          <h2>Личный кабинет ReviMarket</h2>
          <p>${currentUser ? 'Управление профилем, предложениями, заказами и demo-активностью.' : 'Войди в beta-режим, чтобы увидеть кабинет исполнителя или заказчика.'}</p>
        </div>
        <button class="btn btn-primary" data-extra-go="proposals">Предложения</button>
      </div>
      <div class="rm-dashboard-grid">
        <div class="rm-dashboard-card"><b>${html(roleName)}</b><span>роль аккаунта</span></div>
        <div class="rm-dashboard-card"><b>${myProposals.length}</b><span>моих предложений</span></div>
        <div class="rm-dashboard-card"><b>${rub(demoBalance)}</b><span>demo-стоимость услуг</span></div>
        <div class="rm-dashboard-card"><b>${acceptedPrivacy() ? 'OK' : 'OFF'}</b><span>конфиденциальность</span></div>
      </div>
      <div class="rm-account-grid" style="margin-top:16px">
        <div class="rm-account-card"><h3>Мои действия</h3><p>Создать заказ, услугу, товар или быстро открыть чат.</p><div class="rm-account-actions"><button class="btn btn-soft" data-extra-create="order">Заказ</button><button class="btn btn-soft" data-extra-create="freelance">Фриланс</button><button class="btn btn-soft" data-extra-create="product">Товар</button></div></div>
        <div class="rm-account-card"><h3>Предложения</h3><p>Готовые услуги исполнителей, как мини-kwork, но в стиле ReviMarket.</p><div class="rm-account-actions"><button class="btn btn-soft" data-extra-go="proposals">Открыть</button></div></div>
        <div class="rm-account-card"><h3>Платформа</h3><p>${platformOrders} заказов и ${platformProducts} товаров в текущей beta.</p><div class="rm-account-actions"><button class="btn btn-soft" data-extra-go="categories">Категории</button></div></div>
        <div class="rm-account-card"><h3>Backend-план</h3><p>Дальше сюда войдут баланс, сделки, уведомления, отзывы и арбитраж.</p><div class="rm-account-actions"><button class="btn btn-soft" data-extra-go="offer">Оферта</button></div></div>
      </div>
      <div class="rm-cabinet-list">
        <div><b>Что улучшили:</b> категории работ, витрина предложений, кабинет заказчика/исполнителя.</div>
        <div><b>Следующий уровень:</b> регистрация, база данных, отклики на заказы, статусы сделок, реальные отзывы.</div>
      </div>
    `;
    actions.appendChild(block);
    bindExtraButtons(block);
  }

  function bindExtraButtons(root = document) {
    root.querySelectorAll('[data-extra-go]').forEach((button) => {
      if (button.dataset.extraGoBound === '1') return;
      button.dataset.extraGoBound = '1';
      button.addEventListener('click', () => go(button.dataset.extraGo));
    });
    root.querySelectorAll('[data-extra-create]').forEach((button) => {
      if (button.dataset.extraCreateBound === '1') return;
      button.dataset.extraCreateBound = '1';
      button.addEventListener('click', () => {
        try { if (typeof openCreate === 'function') return openCreate(button.dataset.extraCreate); } catch {}
        notify('Создание доступно после входа');
      });
    });
  }

  function bindNav() {
    $$('[data-page="categories"], [data-page="proposals"]').forEach((link) => {
      if (link.dataset.upgradeBound === '1') return;
      link.dataset.upgradeBound = '1';
      link.addEventListener('click', (event) => {
        event.preventDefault();
        showExtraPage(link.dataset.page);
      });
    });
  }

  function boot() {
    ensurePages();
    injectHomeCategories();
    renderCategories();
    renderProposals();
    patchAuthVisibility();
    patchProfile();
    bindNav();
    bindExtraButtons();

    setInterval(() => {
      bindNav();
      patchAuthVisibility();
      if (!$('#page-profile')?.classList.contains('hidden')) patchProfile();
    }, 1200);

    const start = (window.__RM_START_HASH || location.hash || '').replace('#', '');
    if (start === 'categories' || start === 'proposals') {
      setTimeout(() => showExtraPage(start), 180);
    }
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

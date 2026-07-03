(() => {
  const $ = (s, root = document) => root.querySelector(s);
  const $$ = (s, root = document) => [...root.querySelectorAll(s)];

  const KEYS = {
    privacy: 'rm_privacy',
    user: 'rm_user',
    orders: 'rm_orders',
    freelance: 'rm_freelance',
    products: 'rm_products',
    chats: 'rm_chats',
    support: 'rm_support',
    proposals: 'rm_proposals'
  };

  const defaults = {
    orders: [
      {title:'Карточки товара для WB/Ozon', desc:'8 карточек в неоновом стиле: преимущества, инфографика, чистая подача.', price:7500, cat:'cards', tags:['WB','Ozon','Figma'], meta:'12 откликов'},
      {title:'Баннер для акции магазина', desc:'Яркий баннер для сайта и VK. Нужен стильный визуал без перегруза.', price:4500, cat:'banner', tags:['Banner','VK','PSD'], meta:'7 откликов'},
      {title:'Оформление Telegram-постов', desc:'6 шаблонов для новостей, акций, отзывов и подборок.', price:6000, cat:'social', tags:['Telegram','SMM','Canva'], meta:'9 откликов'}
    ],
    freelance: [
      {title:'Сделать лендинг услуги', desc:'Главный экран, преимущества, цены, отзывы и форма заявки.', price:15000, cat:'web', tags:['HTML','UI','Landing'], meta:'5 дней'},
      {title:'Логотип для малого бренда', desc:'3 варианта логотипа, цвета и базовая упаковка.', price:6000, cat:'design', tags:['Logo','Brand','Figma'], meta:'3 дня'},
      {title:'Монтаж короткого ролика', desc:'30 секунд для VK/Shorts: динамика, субтитры, музыка.', price:5000, cat:'video', tags:['Video','Shorts','Edit'], meta:'2 дня'},
      {title:'Тексты для карточек товара', desc:'Продающие описания и преимущества для товаров маркетплейса.', price:4000, cat:'content', tags:['Text','WB','Ozon'], meta:'4 дня'}
    ],
    products: [
      {title:'Figma-шаблоны карточек', desc:'Готовые шаблоны для карточек WB/Ozon.', price:1990, cat:'template', tags:['Figma','Templates','WB'], meta:'Nika Visual'},
      {title:'Пак иконок для товаров', desc:'120 иконок для преимуществ и инфографики.', price:990, cat:'digital', tags:['SVG','PNG','Icons'], meta:'Max Neon'},
      {title:'Аудит карточки товара', desc:'Разбор ошибок, визуала и оффера с рекомендациями.', price:2500, cat:'service', tags:['Audit','WB','Ozon'], meta:'Alina Cards'}
    ],
    proposals: [
      {title:'Сделаю 8 карточек товара для WB/Ozon', category:'Дизайн маркетплейсов', price:3500, days:2, owner:'Nika Visual', text:'Подготовлю стильные карточки, инфографику и первый экран товара.', rating:'4.9'},
      {title:'Соберу лендинг под услугу или товар', category:'Разработка и IT', price:12000, days:5, owner:'Dmitry UI', text:'Главный экран, блоки преимуществ, адаптив и базовая форма заявки.', rating:'4.8'},
      {title:'Сделаю AI-аватар, обложку или промо-арт', category:'Нейросети и AI', price:2500, days:1, owner:'Max Neon', text:'Генерация визуала в неоновом стиле, подготовка под соцсети.', rating:'4.7'}
    ],
    chats: [
      {id:'cards-wb', title:'Карточки товара для WB/Ozon', customer:'Заказчик Сергей', creator:'Nika Visual', messages:[
        {from:'customer', text:'Здравствуйте, нужно 8 карточек для товара. Стиль чёрно-фиолетовый, как у ReviMarket.', time:'10:20'},
        {from:'creator', text:'Приняла. Нужны фото товара, характеристики и примеры, которые нравятся.', time:'10:22'}
      ]},
      {id:'landing', title:'Лендинг услуги', customer:'Заказчик Марк', creator:'Dmitry UI', messages:[
        {from:'customer', text:'Нужен лендинг: главный экран, преимущества, цены и форма заявки.', time:'12:05'},
        {from:'creator', text:'Могу сделать адаптив под телефон. Начну с прототипа.', time:'12:11'}
      ]}
    ],
    support: {ticket:'SUP-1001', messages:[
      {from:'support', text:'Здравствуйте! Это поддержка ReviMarket. Опишите проблему: заказ, исполнитель, товар или аккаунт.', time:'09:00'}
    ]},
    creators: [
      {name:'Nika Visual', role:'Дизайнер карточек WB/Ozon', rating:'4.98', orders:142, tags:['WB','Ozon','Figma']},
      {name:'Max Neon', role:'Баннеры и молодежный визуал', rating:'4.91', orders:88, tags:['VK','PSD','Promo']},
      {name:'Alina Cards', role:'Карточки и аудит товаров', rating:'4.87', orders:117, tags:['Audit','Cards','Market']},
      {name:'Dmitry UI', role:'Сайты и интерфейсы', rating:'4.83', orders:64, tags:['HTML','CSS','UI']}
    ]
  };

  const categories = [
    {id:'market-design', icon:'🛒', title:'Дизайн маркетплейсов', desc:'Карточки WB/Ozon, инфографика, баннеры, упаковка товара.', tags:['WB','Ozon','Figma'], target:'orders', query:'WB'},
    {id:'it', icon:'💻', title:'Разработка и IT', desc:'Сайты, лендинги, боты, багфиксы, ASP.NET, JavaScript.', tags:['HTML','JS','ASP.NET'], target:'freelance', query:'сайт'},
    {id:'ai', icon:'🤖', title:'Нейросети и AI', desc:'AI-аватары, генерация картинок, промпты, автоматизация.', tags:['AI','Prompt','Bot'], target:'proposals'},
    {id:'content', icon:'✍️', title:'Тексты и контент', desc:'Описания товаров, статьи, посты, переводы, продающие тексты.', tags:['Text','SEO','Cards'], target:'freelance', query:'текст'},
    {id:'seo', icon:'📈', title:'SEO и трафик', desc:'Ключевые слова, аудит, продвижение, карточки и сайты.', tags:['SEO','Traffic','Audit'], target:'shop', query:'аудит'},
    {id:'social', icon:'📱', title:'Соцсети и маркетинг', desc:'VK, Telegram, сторис, посты, оформление групп и каналов.', tags:['VK','TG','SMM'], target:'orders', query:'Telegram'},
    {id:'video', icon:'🎬', title:'Видео и монтаж', desc:'Shorts, Reels, клипы, субтитры, обложки и промо-ролики.', tags:['Video','Shorts','Edit'], target:'freelance', query:'монтаж'},
    {id:'business', icon:'📄', title:'Бизнес и документы', desc:'Презентации, резюме, заявки, таблицы, коммерческие тексты.', tags:['Docs','PDF','CV'], target:'freelance'},
    {id:'digital', icon:'📦', title:'Цифровые товары', desc:'Шаблоны, иконки, макеты, наборы, готовые дизайн-паки.', tags:['Templates','SVG','Figma'], target:'shop', query:'шаблон'},
    {id:'urgent', icon:'⚡', title:'Срочные заказы', desc:'Задачи на сегодня: быстро, понятно, с повышенной комиссией.', tags:['Fast','Today','12%'], target:'proposals'}
  ];

  function safeGet(key) { try { return localStorage.getItem(key); } catch { return null; } }
  function safeSet(key, value) { try { localStorage.setItem(key, value); return true; } catch { return false; } }
  function safeRemove(key) { try { localStorage.removeItem(key); } catch {} }
  function acceptedPrivacy() { return safeGet(KEYS.privacy) === 'accepted'; }
  function declinedPrivacy() { return safeGet(KEYS.privacy) === 'declined'; }
  function readJson(key, fallback) {
    if (!acceptedPrivacy()) return clone(fallback);
    try { return JSON.parse(safeGet(key) || 'null') || clone(fallback); }
    catch { safeRemove(key); return clone(fallback); }
  }
  function clone(value) { return JSON.parse(JSON.stringify(value)); }
  function clean(value, limit = 180) { return String(value || '').replace(/[<>"'`]/g, '').trim().slice(0, limit); }
  function esc(value) { return String(value || '').replace(/[&<>'"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c])); }
  function money(value) { return new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(value) || 0, 10000000))) + ' ₽'; }
  function nowTime() { return new Date().toLocaleTimeString('ru-RU', {hour:'2-digit', minute:'2-digit'}); }

  const state = {
    user: readJson(KEYS.user, null),
    createType: 'order',
    selectedChatId: 'cards-wb',
    pendingAuth: false,
    orders: readJson(KEYS.orders, defaults.orders),
    freelance: readJson(KEYS.freelance, defaults.freelance),
    products: readJson(KEYS.products, defaults.products),
    proposals: readJson(KEYS.proposals, defaults.proposals),
    chats: readJson(KEYS.chats, defaults.chats),
    support: readJson(KEYS.support, defaults.support),
    creators: defaults.creators
  };

  window.ReviMarket = { state, route, openAuth, openCreate, save, acceptedPrivacy };

  function save() {
    if (!acceptedPrivacy()) return false;
    safeSet(KEYS.orders, JSON.stringify(state.orders));
    safeSet(KEYS.freelance, JSON.stringify(state.freelance));
    safeSet(KEYS.products, JSON.stringify(state.products));
    safeSet(KEYS.proposals, JSON.stringify(state.proposals));
    safeSet(KEYS.chats, JSON.stringify(state.chats));
    safeSet(KEYS.support, JSON.stringify(state.support));
    if (state.user) safeSet(KEYS.user, JSON.stringify(state.user));
    else safeRemove(KEYS.user);
    return true;
  }

  function clearUserData() {
    [KEYS.user, KEYS.orders, KEYS.freelance, KEYS.products, KEYS.proposals, KEYS.chats, KEYS.support].forEach(safeRemove);
  }

  function toast(text) {
    const el = $('#toast');
    if (!el) return;
    el.textContent = clean(text, 140);
    el.classList.remove('hidden');
    clearTimeout(toast.timer);
    toast.timer = setTimeout(() => el.classList.add('hidden'), 2400);
  }
  window.toast = toast;

  function openPrivacy() { $('#privacyModal')?.classList.remove('hidden'); }
  function closePrivacy() { $('#privacyModal')?.classList.add('hidden'); }
  function requirePrivacy(forAuth = false) {
    if (acceptedPrivacy()) return true;
    state.pendingAuth = !!forAuth;
    openPrivacy();
    toast(declinedPrivacy() ? 'Нажми Принять, чтобы включить вход.' : 'Сначала подтверди конфиденциальность.');
    return false;
  }

  function acceptPrivacy() {
    safeSet(KEYS.privacy, 'accepted');
    closePrivacy();
    save();
    authUi();
    renderAll();
    toast('Конфиденциальность принята');
    if (state.pendingAuth) {
      state.pendingAuth = false;
      setTimeout(openAuth, 120);
    }
  }

  function declinePrivacy() {
    safeSet(KEYS.privacy, 'declined');
    clearUserData();
    state.user = null;
    closePrivacy();
    authUi();
    renderProfile();
    toast('Сохранение отключено. Для входа нажми Принять.');
  }

  function openAuth() {
    state.pendingAuth = true;
    if (!requirePrivacy(true)) return;
    state.pendingAuth = false;
    $('#authModal')?.classList.remove('hidden');
    setTimeout(() => $('#nameInput')?.focus(), 80);
  }
  window.openAuth = openAuth;

  function closeModals() { $$('.modal').forEach(m => m.classList.add('hidden')); }
  window.closeModals = closeModals;

  function authUi() {
    const logged = !!state.user;
    $$('.auth-only').forEach(el => el.classList.toggle('hidden', !logged));
    $$('.guest-only').forEach(el => el.classList.toggle('hidden', logged));
    const ordersCount = $('#countOrders');
    const productsCount = $('#countProducts');
    if (ordersCount) ordersCount.textContent = state.orders.length + state.freelance.length;
    if (productsCount) productsCount.textContent = state.products.length;
  }

  function ensurePage(id, html) {
    if ($('#page-' + id)) return;
    const section = document.createElement('section');
    section.className = 'page hidden';
    section.id = 'page-' + id;
    section.innerHTML = html;
    $('main')?.appendChild(section);
  }

  function ensureNav(page, title, before = '[data-page="profile"]') {
    const nav = $('#nav');
    if (!nav || nav.querySelector(`[data-page="${page}"]`)) return;
    const link = document.createElement('a');
    link.href = '#' + page;
    link.dataset.page = page;
    link.textContent = title;
    nav.insertBefore(link, nav.querySelector(before));
  }

  function ensureDynamicPages() {
    ensureNav('categories', 'Категории', '[data-page="orders"]');
    ensureNav('proposals', 'Предложения', '[data-page="chat"]');
    ensureNav('support', 'Поддержка', '[data-page="profile"]');
    ensureNav('stats', 'Статистика', '[data-page="profile"]');
    ensureNav('offer', 'Оферта', '[data-page="profile"]');

    ensurePage('categories', `<div class="section-head"><div><p class="eyebrow">marketplace categories</p><h2>Категории работ</h2><p>Основные направления ReviMarket Global.</p></div></div><div class="rm-category-grid" id="rmCategoriesGrid"></div>`);
    ensurePage('proposals', `<div class="section-head"><div><p class="eyebrow">creator offers</p><h2>Предложения исполнителей</h2><p>Готовые услуги, цена, срок и быстрый переход в чат.</p></div><button class="btn btn-primary auth-only hidden" id="proposalFocusBtn">+ Предложение</button></div><form class="rm-upgrade-form neon-card auth-only hidden" id="proposalForm"><div class="form-row"><input id="proposalTitle" placeholder="Название предложения" required maxlength="80"><input id="proposalPrice" type="number" min="100" step="100" value="3000" required><input id="proposalDays" type="number" min="1" max="60" value="2" required></div><select id="proposalCategory"></select><textarea id="proposalText" placeholder="Что входит в услугу..." required maxlength="260"></textarea><button class="btn btn-primary" type="submit">Опубликовать предложение</button></form><div class="rm-proposals-grid" id="proposalsGrid"></div>`);
    ensurePage('support', `<div class="section-head"><div><p class="eyebrow">support center</p><h2>Поддержка</h2><p>Обращения по заказам, аккаунту и услугам.</p></div></div><div class="support-layout"><aside class="support-panel neon-card"><h3>ReviMarket Support</h3><p>Выбери тему или напиши сообщение.</p><div class="support-tags"><button type="button" data-support-template="Проблема с заказом">Проблема с заказом</button><button type="button" data-support-template="Вопрос по оплате">Оплата</button><button type="button" data-support-template="Вопрос по аккаунту">Аккаунт</button></div></aside><section class="support-window neon-card"><div class="chat-head"><h3>Поддержка ReviMarket</h3><p>Пользователь ↔ Support</p></div><div class="chat-messages" id="supportMessages"></div><form class="chat-form" id="supportForm"><input id="supportInput" placeholder="Напиши в поддержку..."><button class="btn btn-primary" type="submit">Отправить</button></form></section></div>`);
    ensurePage('stats', `<div class="section-head"><div><p class="eyebrow">platform stats</p><h2>Статистика</h2><p>Активность ReviMarket Global.</p></div></div><div class="stats-grid" id="platformStats"></div>`);
    ensurePage('offer', `<div class="section-head"><div><p class="eyebrow">rules & commission</p><h2>Оферта и правила</h2><p>Frontend-заготовка правил платформы и комиссии.</p></div></div><div class="neon-card" style="padding:20px"><h3>Комиссии платформы</h3><div class="stats-grid"><div class="stat-card"><b>10%</b><span>заказ / фриланс</span></div><div class="stat-card"><b>7%</b><span>цифровой товар</span></div><div class="stat-card"><b>12%</b><span>срочная сделка</span></div><div class="stat-card"><b>0%</b><span>вывод средств</span></div></div><p style="color:var(--muted)">Перед реальным запуском нужны backend, платежный провайдер и юридическая проверка оферты.</p></div>`);
  }

  function route(page) {
    const id = String(page || 'home').replace('#', '') || 'home';
    const target = $('#page-' + id) || $('#page-home');
    $$('.page').forEach(p => p.classList.add('hidden'));
    target.classList.remove('hidden');
    $$('[data-page]').forEach(a => a.classList.toggle('active', a.dataset.page === target.id.replace('page-', '')));
    $('#nav')?.classList.remove('open');
    if (location.hash !== '#' + target.id.replace('page-', '')) history.replaceState(null, '', '#' + target.id.replace('page-', ''));
    renderByPage(target.id.replace('page-', ''));
  }
  window.route = route;

  function card(item, type) {
    return `<article class="card"><div class="card-top"><span class="tag">${esc(item.cat || item.category || type)}</span><span class="price">${money(item.price)}</span></div><h3>${esc(item.title)}</h3><p>${esc(item.desc || item.text)}</p><div class="chips">${(item.tags || []).map(t => `<span>${esc(t)}</span>`).join('')}</div><div class="card-bottom"><span>${esc(item.meta || '')}</span><button class="btn btn-soft" data-open-chat>Открыть</button></div></article>`;
  }

  function renderCards(type) {
    const map = {
      orders: ['#ordersGrid', '#ordersSearch', '#ordersFilter', state.orders],
      freelance: ['#freelanceGrid', '#freelanceSearch', '#freelanceFilter', state.freelance],
      products: ['#productsGrid', '#productsSearch', '#productsFilter', state.products]
    };
    const cfg = map[type];
    if (!cfg) return;
    const [gridId, searchId, filterId, list] = cfg;
    const grid = $(gridId);
    if (!grid) return;
    const q = clean($(searchId)?.value || '', 80).toLowerCase();
    const f = $(filterId)?.value || 'all';
    let items = [...list];
    if (f !== 'all') items = items.filter(i => i.cat === f);
    if (q) items = items.filter(i => `${i.title} ${i.desc} ${(i.tags || []).join(' ')}`.toLowerCase().includes(q));
    grid.innerHTML = items.length ? items.map(i => card(i, type)).join('') : '<article class="card">Ничего не найдено</article>';
    $$('[data-open-chat]', grid).forEach(btn => btn.onclick = () => {
      if (!state.user) { openAuth(); toast('Сначала войди'); return; }
      toast('Открываю чат по сделке');
      route('chat');
    });
  }

  function renderCreators() {
    const grid = $('#creatorsGrid');
    if (!grid) return;
    grid.innerHTML = state.creators.map(c => `<article class="creator"><div class="creator-avatar">${esc(c.name[0])}</div><div><h3>${esc(c.name)}</h3><p>${esc(c.role)}</p><div class="chips">${c.tags.map(t => `<span>${esc(t)}</span>`).join('')}</div></div><div><div class="rating">★ ${esc(c.rating)}</div><p>${esc(c.orders)} заказов</p></div></article>`).join('');
  }

  function renderChat() {
    const threads = $('#chatThreads');
    const messages = $('#chatMessages');
    if (!threads || !messages) return;
    if (!state.chats.length) state.chats = clone(defaults.chats);
    const active = state.chats.find(c => c.id === state.selectedChatId) || state.chats[0];
    state.selectedChatId = active.id;
    threads.innerHTML = state.chats.map(chat => `<button class="chat-thread ${chat.id === active.id ? 'active' : ''}" data-chat-id="${esc(chat.id)}"><b>${esc(chat.title)}</b><span>${esc(chat.customer)} ↔ ${esc(chat.creator)}</span><span>${esc(chat.messages.at(-1)?.text || '')}</span></button>`).join('');
    $('#chatTitle').textContent = active.title;
    $('#chatSubtitle').textContent = `${active.customer} ↔ ${active.creator}`;
    messages.innerHTML = active.messages.map(msg => {
      const isMe = state.user && ((state.user.role === 'customer' && msg.from === 'customer') || (state.user.role === 'creator' && msg.from === 'creator'));
      return `<div class="message ${isMe ? 'me' : 'other'}"><b>${msg.from === 'customer' ? 'Заказчик' : 'Исполнитель'}</b><p>${esc(msg.text)}</p><small>${esc(msg.time)}</small></div>`;
    }).join('');
    messages.scrollTop = messages.scrollHeight;
    $$('[data-chat-id]').forEach(btn => btn.onclick = () => { state.selectedChatId = btn.dataset.chatId; renderChat(); });
  }

  function sendChat(text) {
    if (!requirePrivacy()) return;
    if (!state.user) { openAuth(); return; }
    const active = state.chats.find(c => c.id === state.selectedChatId) || state.chats[0];
    active.messages.push({from: state.user.role === 'creator' ? 'creator' : 'customer', text: clean(text, 240), time: nowTime()});
    save();
    renderChat();
  }

  function renderSupport() {
    const box = $('#supportMessages');
    if (!box) return;
    box.innerHTML = state.support.messages.map(msg => `<div class="message ${msg.from === 'user' ? 'me' : 'other'}"><b>${msg.from === 'user' ? esc(state.user?.name || 'Пользователь') : 'Support'}</b><p>${esc(msg.text)}</p><small>${esc(msg.time)}</small></div>`).join('');
    box.scrollTop = box.scrollHeight;
  }

  function sendSupport(text) {
    if (!requirePrivacy()) return;
    if (!state.user) { openAuth(); return; }
    const time = nowTime();
    state.support.messages.push({from:'user', text:clean(text, 240), time});
    state.support.messages.push({from:'support', text:'Спасибо, обращение принято. Поддержка обработает заявку.', time});
    save();
    renderSupport();
  }

  function renderCategories() {
    const grid = $('#rmCategoriesGrid');
    if (!grid) return;
    grid.innerHTML = categories.map(cat => `<article class="rm-cat-card"><div class="rm-cat-icon">${cat.icon}</div><h3>${esc(cat.title)}</h3><p>${esc(cat.desc)}</p><div class="rm-cat-meta">${cat.tags.map(t => `<span>${esc(t)}</span>`).join('')}</div><button class="btn btn-soft" data-cat-target="${cat.target}" data-cat-query="${esc(cat.query || '')}">Открыть</button></article>`).join('');
    $$('[data-cat-target]').forEach(btn => btn.onclick = () => {
      route(btn.dataset.catTarget);
      setTimeout(() => {
        const map = {orders:'#ordersSearch', freelance:'#freelanceSearch', shop:'#productsSearch'};
        const input = $(map[btn.dataset.catTarget]);
        if (input && btn.dataset.catQuery) { input.value = btn.dataset.catQuery; input.dispatchEvent(new Event('input', {bubbles:true})); }
      }, 80);
    });
  }

  function renderHomeCategories() {
    const hero = $('#page-home .hero-info');
    if (!hero || $('#rmHomeCats')) return;
    const block = document.createElement('div');
    block.id = 'rmHomeCats';
    block.className = 'rm-home-cats';
    block.innerHTML = categories.slice(0, 5).map(cat => `<article class="rm-mini-category"><div class="rm-mini-icon">${cat.icon}</div><h3>${esc(cat.title)}</h3><p>${esc(cat.desc)}</p><button class="btn btn-soft" data-home-cat="${cat.target}">Открыть</button></article>`).join('');
    hero.appendChild(block);
    $$('[data-home-cat]', block).forEach(btn => btn.onclick = () => route(btn.dataset.homeCat));
  }

  function renderProposals() {
    const select = $('#proposalCategory');
    if (select && !select.dataset.ready) {
      select.innerHTML = categories.map(c => `<option value="${esc(c.title)}">${esc(c.title)}</option>`).join('');
      select.dataset.ready = '1';
    }
    const grid = $('#proposalsGrid');
    if (!grid) return;
    grid.innerHTML = state.proposals.map(p => `<article class="rm-proposal-card"><div class="card-top"><span class="tag">${esc(p.category)}</span><span class="price">${money(p.price)}</span></div><h3>${esc(p.title)}</h3><p>${esc(p.text)}</p><div class="rm-proposal-meta"><span>⏱ ${esc(p.days)} дн.</span><span>★ ${esc(p.rating || 'new')}</span><span>${esc(p.owner || 'Исполнитель')}</span></div><div class="card-bottom"><button class="btn btn-soft" data-open-chat>Написать</button><button class="btn btn-primary" data-open-chat>Заказать</button></div></article>`).join('');
    $$('[data-open-chat]', grid).forEach(btn => btn.onclick = () => { if (!state.user) openAuth(); else route('chat'); });
  }

  function userStatsHtml() {
    const myName = state.user?.name || '';
    const myProposals = state.proposals.filter(p => p.owner === myName || p.ownerEmail === state.user?.email).length;
    return `<div class="stats-grid"><div class="stat-card"><b>${state.user ? (state.user.role === 'creator' ? 'Исполнитель' : 'Заказчик') : 'Гость'}</b><span>роль</span></div><div class="stat-card"><b>${myProposals}</b><span>моих предложений</span></div><div class="stat-card"><b>${state.orders.length + state.freelance.length}</b><span>заказов</span></div><div class="stat-card"><b>${state.products.length}</b><span>товаров</span></div></div>`;
  }

  function renderProfile() {
    if (!$('#profileActions')) return;
    if (!state.user) {
      $('#avatar').textContent = 'RM';
      $('#profileName').textContent = 'Гость';
      $('#profileInfo').textContent = acceptedPrivacy() ? 'Войди в ReviMarket Global, чтобы создавать заказы и товары.' : 'Сначала прими конфиденциальность для входа.';
      $('#profileActions').innerHTML = `${userStatsHtml()}<button class="btn btn-primary" id="profileLogin">Войти / зарегистрироваться</button><button class="btn btn-soft" id="profilePrivacy">Конфиденциальность</button>`;
      $('#profileLogin').onclick = openAuth;
      $('#profilePrivacy').onclick = openPrivacy;
      return;
    }
    $('#avatar').textContent = clean(state.user.name, 1).toUpperCase() || 'U';
    $('#profileName').textContent = clean(state.user.name, 40);
    $('#profileInfo').textContent = `${state.user.role === 'creator' ? 'Исполнитель' : 'Заказчик'} • ${clean(state.user.email, 80)}`;
    $('#profileActions').innerHTML = `${userStatsHtml()}<button class="btn btn-primary" data-create="order">Создать заказ</button><button class="btn btn-soft" data-create="freelance">Фриланс-заказ</button><button class="btn btn-soft" data-create="product">Добавить товар</button><button class="btn btn-soft" data-go="proposals">Предложения</button><button class="btn btn-soft" data-go="chat">Чат</button><button class="btn btn-soft" data-go="support">Поддержка</button>`;
    bindActions();
  }

  function renderStats() {
    const box = $('#platformStats');
    if (!box) return;
    const messages = state.chats.reduce((sum, c) => sum + c.messages.length, 0);
    box.innerHTML = `<div class="stat-card"><b>${state.orders.length + state.freelance.length}</b><span>заказов</span></div><div class="stat-card"><b>${state.products.length}</b><span>товаров</span></div><div class="stat-card"><b>${state.proposals.length}</b><span>предложений</span></div><div class="stat-card"><b>${messages}</b><span>сообщений</span></div><div class="stat-card"><b>${state.creators.length}</b><span>исполнителей</span></div><div class="stat-card"><b>Global</b><span>версия</span></div>`;
  }

  function renderByPage(id) {
    if (id === 'orders') renderCards('orders');
    if (id === 'freelance') renderCards('freelance');
    if (id === 'shop') renderCards('products');
    if (id === 'creators') renderCreators();
    if (id === 'chat') renderChat();
    if (id === 'support') renderSupport();
    if (id === 'categories') renderCategories();
    if (id === 'proposals') renderProposals();
    if (id === 'profile') renderProfile();
    if (id === 'stats') renderStats();
  }

  function renderAll() {
    authUi();
    renderCards('orders');
    renderCards('freelance');
    renderCards('products');
    renderCreators();
    renderChat();
    renderSupport();
    renderCategories();
    renderHomeCategories();
    renderProposals();
    renderProfile();
    renderStats();
  }

  function openCreate(type) {
    if (!requirePrivacy()) return;
    if (!state.user) { openAuth(); toast('Сначала войди'); return; }
    state.createType = type;
    const cfg = {
      order: ['Создать заказ', [['cards','Карточки'], ['banner','Баннеры'], ['social','Соцсети']]],
      freelance: ['Создать фриланс-заказ', [['web','Сайты'], ['design','Дизайн'], ['content','Контент'], ['video','Видео']]],
      product: ['Добавить товар', [['template','Шаблоны'], ['digital','Цифровое'], ['service','Услуги']]]
    }[type] || null;
    if (!cfg) return;
    $('#createTitle').textContent = cfg[0];
    $('#createType').textContent = type;
    $('#itemCategory').innerHTML = cfg[1].map(([v,t]) => `<option value="${v}">${t}</option>`).join('');
    $('#createModal')?.classList.remove('hidden');
  }
  window.openCreate = openCreate;

  function validateAuth(name, email) {
    if (name.length < 2) return 'Имя должно быть минимум 2 символа';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Введи нормальную почту';
    return '';
  }

  function handleAuthSubmit(event) {
    event.preventDefault();
    if (!requirePrivacy(true)) return;
    const name = clean($('#nameInput')?.value, 40);
    const email = clean($('#emailInput')?.value, 80).toLowerCase();
    const role = $('#roleInput')?.value === 'creator' ? 'creator' : 'customer';
    const error = validateAuth(name, email);
    if (error) { toast(error); return; }
    state.user = {name, email, role, createdAt: new Date().toISOString()};
    save();
    authUi();
    closeModals();
    renderProfile();
    toast('Готово, аккаунт создан');
    route('profile');
  }

  function handleCreateSubmit(event) {
    event.preventDefault();
    if (!requirePrivacy()) return;
    if (!state.user) { openAuth(); return; }
    const item = {title:clean($('#itemTitle')?.value, 80), desc:clean($('#itemDesc')?.value, 300), price:Math.max(100, Math.min(Number($('#itemPrice')?.value) || 0, 10000000)), cat:$('#itemCategory')?.value || 'new', tags:['New','Global'], meta:clean(state.user.name, 40)};
    if (!item.title || !item.desc) { toast('Заполни название и описание'); return; }
    if (state.createType === 'order') { state.orders.unshift(item); route('orders'); }
    if (state.createType === 'freelance') { state.freelance.unshift(item); route('freelance'); }
    if (state.createType === 'product') { state.products.unshift(item); route('shop'); }
    save();
    authUi();
    closeModals();
    $('#createForm')?.reset();
    toast('Опубликовано');
  }

  function bindActions() {
    $$('[data-page]').forEach(link => {
      if (link.dataset.bound === '1') return;
      link.dataset.bound = '1';
      link.onclick = (e) => { e.preventDefault(); route(link.dataset.page); };
    });
    $$('[data-go]').forEach(btn => btn.onclick = () => route(btn.dataset.go));
    $$('[data-create]').forEach(btn => btn.onclick = () => openCreate(btn.dataset.create));
  }

  function bindForms() {
    $('#menuBtn')?.addEventListener('click', () => $('#nav')?.classList.toggle('open'));
    $('#loginBtn')?.addEventListener('click', openAuth);
    $('#logoutBtn')?.addEventListener('click', () => { state.user = null; save(); authUi(); renderProfile(); toast('Ты вышел'); route('home'); });
    $('#acceptPrivacyBtn')?.addEventListener('click', acceptPrivacy);
    $('#declinePrivacyBtn')?.addEventListener('click', declinePrivacy);
    $$('[data-close]').forEach(el => el.addEventListener('click', closeModals));
    $('#authForm')?.addEventListener('submit', handleAuthSubmit);
    $('#createForm')?.addEventListener('submit', handleCreateSubmit);
    $('#chatForm')?.addEventListener('submit', e => { e.preventDefault(); const input = $('#chatInput'); const text = clean(input?.value, 240); if (!text) return toast('Сообщение пустое'); sendChat(text); input.value = ''; });
    document.addEventListener('submit', e => {
      if (e.target?.id === 'supportForm') { e.preventDefault(); const input = $('#supportInput'); const text = clean(input?.value, 240); if (!text) return toast('Сообщение пустое'); sendSupport(text); input.value = ''; }
      if (e.target?.id === 'proposalForm') { e.preventDefault(); handleProposalSubmit(); }
    });
    document.addEventListener('click', e => {
      const tpl = e.target.closest('[data-support-template]');
      if (tpl) { $('#supportInput').value = tpl.dataset.supportTemplate; $('#supportInput').focus(); }
      if (e.target.closest('#proposalFocusBtn')) $('#proposalTitle')?.focus();
    });
    ['orders','freelance','products'].forEach(type => {
      const map = {orders:['#ordersSearch','#ordersFilter'], freelance:['#freelanceSearch','#freelanceFilter'], products:['#productsSearch','#productsFilter']}[type];
      map.forEach(sel => $(sel)?.addEventListener('input', () => renderCards(type)));
    });
  }

  function handleProposalSubmit() {
    if (!requirePrivacy()) return;
    if (!state.user) { openAuth(); return; }
    const title = clean($('#proposalTitle')?.value, 80);
    const text = clean($('#proposalText')?.value, 260);
    if (!title || !text) return toast('Заполни предложение');
    state.proposals.unshift({title, category:clean($('#proposalCategory')?.value, 60), price:Math.max(100, Math.min(Number($('#proposalPrice')?.value) || 0, 10000000)), days:Math.max(1, Math.min(Number($('#proposalDays')?.value) || 1, 60)), owner:clean(state.user.name, 40), ownerEmail:clean(state.user.email, 80), text, rating:'new'});
    save();
    $('#proposalForm')?.reset();
    renderProposals();
    renderProfile();
    toast('Предложение опубликовано');
  }

  function init() {
    ensureDynamicPages();
    bindActions();
    bindForms();
    renderAll();
    const start = (window.__RM_START_HASH || location.hash || '#home').replace('#', '') || 'home';
    route(start);
    if (!safeGet(KEYS.privacy)) openPrivacy();
  }

  init();
})();

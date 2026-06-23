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

  const clean = (value, limit = 160) => String(value ?? '').replace(/[<>'"`]/g, '').trim().slice(0, limit);
  const money = (value) => new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(value) || 0, 10000000))) + ' ₽';

  function appState() {
    try { return typeof state !== 'undefined' ? state : null; }
    catch { return null; }
  }

  function showOnly(pageId) {
    $$('.page').forEach((page) => page.classList.add('hidden'));
    const page = $(`#${pageId}`);
    if (page) page.classList.remove('hidden');
    $$('[data-page]').forEach((link) => link.classList.remove('active'));
    $('#nav')?.classList.remove('open');
    history.replaceState(null, '', `#${pageId.replace('page-', '')}`);
  }

  function runRoute(page) {
    if (typeof route === 'function') route(page);
    else location.hash = page;
  }

  function ensureDetailPages() {
    const main = $('main');
    if (!main) return;

    if (!$('#page-product-detail')) {
      const product = document.createElement('section');
      product.id = 'page-product-detail';
      product.className = 'page hidden detail-page';
      product.innerHTML = '<div id="productDetailRoot"></div>';
      main.appendChild(product);
    }

    if (!$('#page-order-detail')) {
      const order = document.createElement('section');
      order.id = 'page-order-detail';
      order.className = 'page hidden detail-page';
      order.innerHTML = '<div id="orderDetailRoot"></div>';
      main.appendChild(order);
    }
  }

  function detailTags(tags = []) {
    return `<div class="detail-tags">${tags.map((tag) => `<span>${safe(tag)}</span>`).join('')}</div>`;
  }

  function openProductDetail(index) {
    const app = appState();
    if (!app) return;
    const item = app.products[Number(index)] || app.products[0];
    if (!item) return;

    $('#productDetailRoot').innerHTML = `
      <button class="btn btn-soft detail-back" data-back-to="shop">← Назад к товарам</button>
      <div class="detail-layout">
        <article class="detail-main">
          <p class="eyebrow">product detail</p>
          <h2>${safe(item.title)}</h2>
          <p class="detail-description">${safe(item.desc)}</p>
          ${detailTags(item.tags)}
          <h3 class="detail-section-title">Что входит</h3>
          <div class="detail-card-body">
            Информационный товар или цифровая услуга ReviMarket: шаблон, аудит, макет, пакет материалов или готовый набор для работы с маркетплейсами.
          </div>
        </article>
        <aside class="detail-side">
          <span class="detail-badge">Товар</span>
          <div class="detail-price">${money(item.price)}</div>
          <h3>Информация</h3>
          <div class="detail-info-grid">
            <div class="detail-info-card"><b>${safe(item.meta || 'Продавец')}</b><span>продавец</span></div>
            <div class="detail-info-card"><b>${safe(item.cat)}</b><span>категория</span></div>
            <div class="detail-info-card"><b>Beta</b><span>статус публикации</span></div>
            <div class="detail-info-card"><b>Digital</b><span>формат товара</span></div>
          </div>
          <div class="detail-actions">
            <button class="btn btn-primary" data-write-seller>Написать продавцу</button>
            <button class="btn btn-soft" data-back-to="shop">Открыть список товаров</button>
          </div>
        </aside>
      </div>
    `;

    bindDetailButtons();
    showOnly('page-product-detail');
  }

  function openOrderDetail(source, index) {
    const app = appState();
    if (!app) return;
    const list = source === 'orders' ? app.orders : app.freelance;
    const item = list[Number(index)] || list[0];
    if (!item) return;

    const sourceTitle = source === 'orders' ? 'Дизайн-заказ' : 'Фриланс-заказ';

    $('#orderDetailRoot').innerHTML = `
      <button class="btn btn-soft detail-back" data-back-to="freelance">← Назад к работе</button>
      <div class="detail-layout">
        <article class="detail-main">
          <p class="eyebrow">order detail</p>
          <h2>${safe(item.title)}</h2>
          <p class="detail-description">${safe(item.desc)}</p>
          ${detailTags(item.tags)}
          <h3 class="detail-section-title">Техническое задание</h3>
          <div class="detail-card-body">
            Нужно выполнить задачу по описанию заказчика. В полной версии здесь будут файлы ТЗ, сроки, этапы сделки, отклики исполнителей и безопасная оплата.
          </div>
        </article>
        <aside class="detail-side">
          <span class="detail-badge">${sourceTitle}</span>
          <div class="detail-price">${money(item.price)}</div>
          <h3>Информация</h3>
          <div class="detail-info-grid">
            <div class="detail-info-card"><b>${safe(item.meta || 'Заказчик')}</b><span>заказчик / срок</span></div>
            <div class="detail-info-card"><b>${safe(item.cat)}</b><span>категория</span></div>
            <div class="detail-info-card"><b>Открыт</b><span>статус заказа</span></div>
            <div class="detail-info-card"><b>Beta</b><span>режим сделки</span></div>
          </div>
          <div class="detail-actions">
            <button class="btn btn-primary" data-write-customer>Откликнуться / написать заказчику</button>
            <button class="btn btn-soft" data-back-to="freelance">Открыть список работы</button>
          </div>
        </aside>
      </div>
    `;

    bindDetailButtons();
    showOnly('page-order-detail');
  }

  function cardHtml(item, source, index) {
    const isProduct = source === 'products';
    const buttonText = isProduct ? 'Подробнее о товаре' : 'Подробности заказа';
    const type = isProduct ? 'product' : 'order';
    return `
      <article class="card">
        <div class="card-top">
          <span class="tag">${safe(item.cat)}</span>
          <span class="price">${money(item.price)}</span>
        </div>
        <h3>${safe(item.title)}</h3>
        <p>${safe(item.desc)}</p>
        <div class="chips">${(item.tags || []).map((tag) => `<span>${safe(tag)}</span>`).join('')}</div>
        <div class="card-bottom">
          <span>${safe(item.meta)}</span>
          <button class="btn btn-soft" data-detail-open="${type}" data-detail-source="${safe(source)}" data-detail-index="${index}">${buttonText}</button>
        </div>
      </article>
    `;
  }

  function getWorkItems(app) {
    return [
      ...app.orders.map((item, index) => ({ item, source: 'orders', index })),
      ...app.freelance.map((item, index) => ({ item, source: 'freelance', index }))
    ];
  }

  function renderOptimizedCards(type) {
    const app = appState();
    if (!app) return;

    const map = {
      orders: ['#ordersGrid', '#ordersSearch', '#ordersFilter'],
      freelance: ['#freelanceGrid', '#freelanceSearch', '#freelanceFilter'],
      products: ['#productsGrid', '#productsSearch', '#productsFilter']
    };

    const cfg = map[type];
    if (!cfg) return;

    const [gridId, searchId, filterId] = cfg;
    const grid = $(gridId);
    const search = $(searchId);
    const filter = $(filterId);
    if (!grid || !search || !filter) return;

    const q = clean(search.value, 80).toLowerCase();
    const f = filter.value;
    let items = [];

    if (type === 'products') {
      items = app.products.map((item, index) => ({ item, source: 'products', index }));
    } else if (type === 'freelance') {
      items = getWorkItems(app);
    } else {
      items = app.orders.map((item, index) => ({ item, source: 'orders', index }));
    }

    if (f !== 'all') items = items.filter((entry) => entry.item.cat === f || entry.source === f);
    if (q) items = items.filter((entry) => (entry.item.title + entry.item.desc + (entry.item.tags || []).join(' ')).toLowerCase().includes(q));

    grid.innerHTML = items.length
      ? items.map((entry) => cardHtml(entry.item, entry.source, entry.index)).join('')
      : '<div class="card">Ничего не найдено</div>';

    bindDetailButtons();
  }

  function updateFilters() {
    const workFilter = $('#freelanceFilter');
    if (workFilter && workFilter.dataset.detailFilterReady !== '1') {
      workFilter.dataset.detailFilterReady = '1';
      workFilter.innerHTML = `
        <option value="all">Все заказы</option>
        <option value="orders">Дизайн-заказы</option>
        <option value="freelance">Фриланс-заказы</option>
        <option value="cards">Карточки</option>
        <option value="web">Сайты</option>
        <option value="design">Дизайн</option>
        <option value="content">Контент</option>
        <option value="video">Видео</option>
      `;
      workFilter.oninput = () => renderOptimizedCards('freelance');
    }

    const productFilter = $('#productsFilter');
    if (productFilter && productFilter.dataset.detailFilterReady !== '1') {
      productFilter.dataset.detailFilterReady = '1';
      productFilter.innerHTML = `
        <option value="all">Все товары</option>
        <option value="template">Шаблоны</option>
        <option value="digital">Цифровое</option>
        <option value="service">Услуги</option>
      `;
      productFilter.oninput = () => renderOptimizedCards('products');
    }
  }

  function bindDetailButtons() {
    $$('[data-detail-open]').forEach((button) => {
      button.onclick = () => {
        const kind = button.dataset.detailOpen;
        const source = button.dataset.detailSource;
        const index = button.dataset.detailIndex;
        if (kind === 'product') openProductDetail(index);
        else openOrderDetail(source, index);
      };
    });

    $$('[data-back-to]').forEach((button) => {
      button.onclick = () => runRoute(button.dataset.backTo);
    });

    $$('[data-write-seller]').forEach((button) => {
      button.onclick = () => {
        try {
          const app = appState();
          if (app && app.chats?.[2]) app.selectedChatId = app.chats[2].id;
        } catch {}
        runRoute('chat');
      };
    });

    $$('[data-write-customer]').forEach((button) => {
      button.onclick = () => {
        try {
          const app = appState();
          if (app && app.chats?.[0]) app.selectedChatId = app.chats[0].id;
        } catch {}
        runRoute('chat');
      };
    });
  }

  function overrideRenderCards() {
    try {
      renderCards = renderOptimizedCards;
    } catch (error) {
      console.warn('Render cards override failed', error);
    }
  }

  function boot() {
    ensureDetailPages();
    updateFilters();
    overrideRenderCards();
    renderOptimizedCards('products');
    renderOptimizedCards('freelance');
    renderOptimizedCards('orders');

    const productSearch = $('#productsSearch');
    const workSearch = $('#freelanceSearch');
    if (productSearch) productSearch.oninput = () => renderOptimizedCards('products');
    if (workSearch) workSearch.oninput = () => renderOptimizedCards('freelance');

    setInterval(() => {
      updateFilters();
      bindDetailButtons();
    }, 800);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

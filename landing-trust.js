(() => {
  const $ = (selector) => document.querySelector(selector);
  const $$ = (selector) => [...document.querySelectorAll(selector)];

  const allowedItems = [
    ['shop', 'Каталог'],
    ['freelance', 'Заказы'],
    ['creators', 'Исполнители'],
    ['chat', 'Сообщения'],
    ['profile', 'Профиль']
  ];
  const allowedPages = new Set(allowedItems.map(([page]) => page));

  function routeTo(page) {
    if (typeof route === 'function') route(page);
    else location.hash = page;
  }

  function cleanHeaderNav() {
    const nav = $('#nav');
    if (!nav) return;

    nav.querySelectorAll('a[data-page]').forEach((link) => {
      if (!allowedPages.has(link.dataset.page)) {
        link.remove();
      }
    });

    allowedItems.forEach(([page, title]) => {
      let link = nav.querySelector(`a[data-page="${page}"]`);
      if (!link) {
        link = document.createElement('a');
        link.href = `#${page}`;
        link.dataset.page = page;
        nav.appendChild(link);
      }
      link.textContent = title;
      link.style.display = '';
      link.classList.remove('hidden', 'auth-only');
      link.onclick = (event) => {
        event.preventDefault();
        routeTo(page);
      };
    });
  }

  function lockHeaderNav() {
    const nav = $('#nav');
    if (!nav || nav.dataset.trustLocked === '1') return;
    nav.dataset.trustLocked = '1';

    const observer = new MutationObserver(() => cleanHeaderNav());
    observer.observe(nav, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'style'] });
  }

  function updateLandingText() {
    const hero = $('#page-home .hero-info');
    if (!hero) return;

    const eyebrow = hero.querySelector('.eyebrow');
    const title = hero.querySelector('h1');
    const text = hero.querySelector('.hero-text');

    if (eyebrow) eyebrow.textContent = 'российская beta-биржа для работы и цифровых товаров';
    if (title) title.textContent = 'ReviMarket — безопасная площадка для заказчиков, исполнителей и продавцов';
    if (text) {
      text.textContent = 'Мы создаём российскую онлайн-биржу, где можно размещать заказы, находить исполнителей, продавать информационные товары и вести сделку внутри платформы. В beta-версии уже есть каталог услуг, биржа заказов, профиль, чаты и демо-логика безопасной сделки.';
    }

    if (!$('#heroTrustBadges')) {
      const badges = document.createElement('div');
      badges.id = 'heroTrustBadges';
      badges.className = 'hero-trust-badges';
      badges.innerHTML = `
        <span>✓ Российский проект</span>
        <span>✓ Каталог услуг</span>
        <span>✓ Биржа заказов</span>
        <span>✓ Demo-безопасная сделка</span>
      `;
      hero.insertBefore(badges, hero.querySelector('.eyebrow'));
    }

    if (!$('#heroDescriptionCard')) {
      const card = document.createElement('div');
      card.id = 'heroDescriptionCard';
      card.className = 'hero-description-card';
      card.innerHTML = `
        <p>
          ReviMarket помогает заказчику быстро найти специалиста, а исполнителю — показать услуги и получить заказ без лишних переписок вне площадки. Мы делаем понятную систему: каталог → заказ → подробная страница → чат → статус сделки → поддержка.
        </p>
        <div class="hero-trust-list">
          <div><b>Каталог</b><span>Услуги, цифровые товары, шаблоны, аудиты и готовые предложения.</span></div>
          <div><b>Заказы</b><span>Биржа задач для дизайнеров, разработчиков, авторов и исполнителей.</span></div>
          <div><b>Профиль</b><span>Кабинет пользователя: заказы, товары, чаты и статус аккаунта.</span></div>
        </div>
      `;
      const actions = hero.querySelector('.hero-actions');
      if (actions) hero.insertBefore(card, actions);
      else hero.appendChild(card);
    }

    const buttons = $$('#page-home .hero-actions .btn');
    if (buttons[0]) buttons[0].textContent = 'Открыть заказы';
    if (buttons[1]) buttons[1].textContent = 'Открыть каталог';
  }

  function boot() {
    cleanHeaderNav();
    lockHeaderNav();
    updateLandingText();

    setInterval(() => {
      cleanHeaderNav();
      updateLandingText();
    }, 1000);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

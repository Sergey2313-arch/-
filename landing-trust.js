(() => {
  const $ = (selector) => document.querySelector(selector);
  const $$ = (selector) => [...document.querySelectorAll(selector)];

  function cleanHeaderNav() {
    const nav = $('#nav');
    if (!nav) return;

    const items = [
      ['shop', 'Товары'],
      ['freelance', 'Работа'],
      ['profile', 'Профиль']
    ];
    const allowed = new Set(items.map(([page]) => page));

    nav.querySelectorAll('a[data-page]').forEach((link) => {
      if (!allowed.has(link.dataset.page)) link.remove();
    });

    items.forEach(([page, title]) => {
      let link = nav.querySelector(`a[data-page="${page}"]`);
      if (!link) {
        link = document.createElement('a');
        link.href = `#${page}`;
        link.dataset.page = page;
        nav.appendChild(link);
      }
      link.textContent = title;
      link.classList.remove('hidden', 'auth-only');
      link.onclick = (event) => {
        event.preventDefault();
        if (typeof route === 'function') route(page);
        else location.hash = page;
      };
    });
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
      text.textContent = 'Мы создаём российскую онлайн-биржу, где можно размещать заказы, находить исполнителей, продавать информационные товары и вести сделку внутри платформы. В beta-версии уже есть витрина товаров, список работ, профиль, чаты и демо-логика безопасной сделки.';
    }

    if (!$('#heroTrustBadges')) {
      const badges = document.createElement('div');
      badges.id = 'heroTrustBadges';
      badges.className = 'hero-trust-badges';
      badges.innerHTML = `
        <span>✓ Российский проект</span>
        <span>✓ Заказы и цифровые товары</span>
        <span>✓ Чаты внутри сделки</span>
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
          ReviMarket помогает заказчику быстро найти специалиста, а исполнителю — показать услуги и получить заказ без лишних переписок вне площадки. Мы делаем понятную систему: товар или заказ → подробная страница → чат → статус сделки → поддержка.
        </p>
        <div class="hero-trust-list">
          <div><b>Прозрачность</b><span>Описание, цена, статус и детали заказа видны до начала работы.</span></div>
          <div><b>Поддержка</b><span>Для споров и вопросов заложены тикеты, модерация и агенты поддержки.</span></div>
          <div><b>Развитие</b><span>Сейчас beta без реальных платежей, дальше — backend, роли и безопасные выплаты.</span></div>
        </div>
      `;
      const actions = hero.querySelector('.hero-actions');
      if (actions) hero.insertBefore(card, actions);
      else hero.appendChild(card);
    }

    const buttons = $$('#page-home .hero-actions .btn');
    if (buttons[0]) buttons[0].textContent = 'Открыть работу';
    if (buttons[1]) buttons[1].textContent = 'Открыть товары';
  }

  function boot() {
    cleanHeaderNav();
    updateLandingText();

    let count = 0;
    const timer = setInterval(() => {
      cleanHeaderNav();
      updateLandingText();
      count += 1;
      if (count > 20) clearInterval(timer);
    }, 250);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

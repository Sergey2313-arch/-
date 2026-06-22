(() => {
  const COMMISSIONS = {
    deal: 10,
    digital: 7,
    urgent: 12,
    withdrawal: 0
  };

  const rules = [
    'Пользователь обязан указывать достоверное описание заказа, услуги или товара.',
    'Исполнитель обязан соблюдать сроки, качество и условия, согласованные в чате сделки.',
    'Запрещены мошенничество, спам, обман, публикация чужих работ без прав и обход комиссии платформы.',
    'Запрещены товары и услуги, нарушающие закон, права третьих лиц или правила площадки.',
    'Споры решаются через поддержку ReviMarket. В beta-версии это демонстрационный сценарий.',
    'Реальные платежи, возвраты и удержания в текущей beta-версии не выполняются.'
  ];

  function money(n){ return new Intl.NumberFormat('ru-RU').format(n) + ' ₽'; }

  function calcCommission(amount, type){
    const rate = COMMISSIONS[type] ?? COMMISSIONS.deal;
    const commission = Math.round((Number(amount) || 0) * rate / 100);
    return { rate, commission, creator: Math.max(0, (Number(amount) || 0) - commission) };
  }

  function ensureStyles(){
    if(document.querySelector('#offerStyles')) return;
    const style = document.createElement('style');
    style.id = 'offerStyles';
    style.textContent = `
      .offer-grid{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-top:18px}.offer-card{padding:22px;border:1px solid rgba(255,43,214,.22);border-radius:26px;background:linear-gradient(145deg,rgba(35,13,63,.7),rgba(255,255,255,.035));box-shadow:0 0 34px rgba(194,38,255,.14)}.offer-card h3{margin:0 0 12px;font-size:26px}.offer-card p,.offer-card li{color:#c9b7d8;line-height:1.65}.offer-list{display:grid;gap:10px;padding:0;margin:14px 0;list-style:none}.offer-list li{padding:12px 14px;border:1px solid rgba(255,255,255,.08);border-radius:16px;background:rgba(255,255,255,.045)}.commission-table{width:100%;border-collapse:collapse;margin-top:12px;overflow:hidden;border-radius:16px}.commission-table th,.commission-table td{padding:12px;border-bottom:1px solid rgba(255,255,255,.08);text-align:left}.commission-table th{color:#fff;background:rgba(255,43,214,.09)}.commission-table td{color:#c9b7d8}.offer-calc{display:grid;grid-template-columns:1fr 1fr auto;gap:10px;margin-top:12px}.offer-result{margin-top:12px;padding:14px;border:1px solid rgba(53,255,182,.22);border-radius:16px;background:rgba(53,255,182,.06);color:#d9fff2;font-weight:900}.offer-note{margin-top:16px;padding:14px;border:1px solid rgba(255,209,102,.26);border-radius:16px;background:rgba(255,209,102,.06);color:#ffe9b5}.offer-actions{display:flex;flex-wrap:wrap;gap:10px;margin-top:16px}@media(max-width:920px){.offer-grid{grid-template-columns:1fr}.offer-calc{grid-template-columns:1fr}}@media(max-width:620px){.offer-card{border-radius:20px}.commission-table th,.commission-table td{padding:10px;font-size:13px}}
    `;
    document.head.appendChild(style);
  }

  function ensureOfferPage(){
    ensureStyles();

    const nav = document.querySelector('#nav');
    if(nav && !nav.querySelector('[data-page="offer"]')){
      const link = document.createElement('a');
      link.href = '#offer';
      link.dataset.page = 'offer';
      link.textContent = 'Оферта';
      const profile = nav.querySelector('[data-page="profile"]');
      nav.insertBefore(link, profile || null);
    }

    if(!document.querySelector('#page-offer')){
      const page = document.createElement('section');
      page.className = 'page hidden';
      page.id = 'page-offer';
      document.querySelector('main')?.appendChild(page);
    }
  }

  function renderOffer(){
    ensureOfferPage();
    const page = document.querySelector('#page-offer');
    if(!page) return;

    page.innerHTML = `
      <div class="section-head">
        <div>
          <p class="eyebrow">terms / public offer</p>
          <h2>Оферта и правила ReviMarket</h2>
          <p>Демо-раздел для beta-версии: правила площадки, комиссии и порядок работы сделок.</p>
        </div>
      </div>

      <div class="offer-grid">
        <article class="offer-card">
          <h3>Комиссия платформы</h3>
          <p>Комиссия удерживается только в полной версии при реальной безопасной сделке. В текущей beta реальные платежи отключены.</p>
          <table class="commission-table">
            <thead><tr><th>Тип операции</th><th>Комиссия</th><th>Пример</th></tr></thead>
            <tbody>
              <tr><td>Заказ / фриланс-сделка</td><td>${COMMISSIONS.deal}%</td><td>с 10 000 ₽ комиссия 1 000 ₽</td></tr>
              <tr><td>Цифровой товар / шаблон</td><td>${COMMISSIONS.digital}%</td><td>с 2 000 ₽ комиссия 140 ₽</td></tr>
              <tr><td>Срочная сделка / продвижение</td><td>${COMMISSIONS.urgent}%</td><td>дополнительный сервис платформы</td></tr>
              <tr><td>Вывод средств</td><td>${COMMISSIONS.withdrawal}%</td><td>комиссия банка может быть отдельно</td></tr>
            </tbody>
          </table>

          <div class="offer-calc">
            <input id="offerAmount" type="number" min="0" step="100" value="10000" placeholder="Сумма сделки" />
            <select id="offerType">
              <option value="deal">Заказ / фриланс</option>
              <option value="digital">Цифровой товар</option>
              <option value="urgent">Срочная сделка</option>
            </select>
            <button class="btn btn-primary" id="offerCalcBtn">Посчитать</button>
          </div>
          <div class="offer-result" id="offerResult">Комиссия: 1 000 ₽, исполнителю: 9 000 ₽</div>
        </article>

        <article class="offer-card">
          <h3>Правила сайта</h3>
          <ul class="offer-list">
            ${rules.map(rule => `<li>${rule}</li>`).join('')}
          </ul>
        </article>

        <article class="offer-card">
          <h3>Как проходит сделка</h3>
          <ul class="offer-list">
            <li>Заказчик создаёт заказ и описывает задачу.</li>
            <li>Исполнитель откликается и обсуждает детали в чате.</li>
            <li>Стороны согласуют цену, срок и результат.</li>
            <li>В полной версии деньги резервируются до завершения работы.</li>
            <li>После принятия результата исполнитель получает выплату за вычетом комиссии.</li>
          </ul>
        </article>

        <article class="offer-card">
          <h3>Споры и поддержка</h3>
          <p>Если заказчик и исполнитель не могут договориться, они обращаются в поддержку. В полной версии поддержка проверяет переписку, описание заказа и приложенные материалы.</p>
          <div class="offer-note">Этот текст — демо-заготовка для проекта. Перед реальными платежами оферту, правила, возвраты и обработку данных нужно оформлять с юристом.</div>
          <div class="offer-actions">
            <button class="btn btn-primary" data-offer-go="support">Написать в поддержку</button>
            <button class="btn btn-soft" data-offer-go="chat">Открыть чат сделки</button>
          </div>
        </article>
      </div>
    `;

    const update = () => {
      const amount = Number(document.querySelector('#offerAmount')?.value || 0);
      const type = document.querySelector('#offerType')?.value || 'deal';
      const result = calcCommission(amount, type);
      const el = document.querySelector('#offerResult');
      if(el) el.textContent = `Комиссия: ${money(result.commission)} (${result.rate}%), исполнителю: ${money(result.creator)}`;
      window.ReviMarketDev?.log?.('offer', 'Расчёт комиссии', { amount, type, rate: result.rate, commission: result.commission });
    };

    document.querySelector('#offerCalcBtn')?.addEventListener('click', update);
    document.querySelector('#offerAmount')?.addEventListener('input', update);
    document.querySelector('#offerType')?.addEventListener('change', update);
    document.querySelectorAll('[data-offer-go]').forEach(btn => btn.onclick = () => document.querySelector(`[data-page="${btn.dataset.offerGo}"]`)?.dispatchEvent(new MouseEvent('click', { bubbles:true, cancelable:true })));
  }

  function showOffer(){
    ensureOfferPage();
    document.querySelectorAll('.page').forEach(p => p.classList.add('hidden'));
    document.querySelector('#page-offer')?.classList.remove('hidden');
    document.querySelectorAll('[data-page]').forEach(a => a.classList.toggle('active', a.dataset.page === 'offer'));
    document.querySelector('#nav')?.classList.remove('open');
    location.hash = 'offer';
    renderOffer();
    window.ReviMarketDev?.log?.('navigation', 'Открыт раздел оферты');
  }

  function boot(){
    ensureOfferPage();
    document.querySelector('[data-page="offer"]')?.addEventListener('click', e => {
      e.preventDefault();
      showOffer();
    });
    if(location.hash === '#offer') showOffer();
  }

  if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

(() => {
  const KEY = 'rm_deals_v1';
  const PRIVACY_KEY = 'rm_privacy';
  const $ = (s) => document.querySelector(s);
  const $$ = (s) => [...document.querySelectorAll(s)];
  const esc = (v) => String(v ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const clean = (v, n = 160) => String(v ?? '').replace(/[<>`]/g, '').trim().slice(0, n);
  const money = (n) => new Intl.NumberFormat('ru-RU').format(Number(n) || 0) + ' ₽';
  const clone = (x) => JSON.parse(JSON.stringify(x));

  const status = {
    open: ['Открыт', 'Ждём отклики'],
    picked: ['Исполнитель выбран', 'Можно запускать demo-гарантию'],
    hold: ['Demo-гарантия', 'Условия сделки зафиксированы'],
    work: ['В работе', 'Исполнитель выполняет задачу'],
    check: ['Проверка', 'Заказчик смотрит результат'],
    done: ['Завершён', 'Работа принята'],
    dispute: ['Разбор', 'Нужна поддержка']
  };

  const demoDeals = [
    {
      id: 'wb-cards', title: 'Карточки товара для WB/Ozon', type: 'Дизайн', price: 7500, term: '3 дня',
      desc: '8 карточек в неоновом стиле: преимущества, инфографика и чистая подача.',
      customer: 'Заказчик Сергей', creator: '', step: 'open', guarantee: false,
      offers: [
        {name:'Nika Visual', price:7200, term:'2 дня', text:'Сделаю в Figma, отдам PNG и исходник.'},
        {name:'Alina Cards', price:7600, term:'3 дня', text:'Добавлю аудит первого экрана и инфографики.'}
      ],
      log: ['Заказ опубликован', 'Появились первые отклики']
    },
    {
      id: 'landing', title: 'Лендинг услуги', type: 'Фриланс', price: 15000, term: '5 дней',
      desc: 'Главный экран, преимущества, цены, отзывы и форма заявки.',
      customer: 'Заказчик Марк', creator: 'Dmitry UI', step: 'hold', guarantee: true,
      offers: [{name:'Dmitry UI', price:15000, term:'5 дней', text:'Сделаю адаптивный HTML/CSS макет.'}],
      log: ['Заказ опубликован', 'Исполнитель выбран', 'Demo-гарантия включена']
    },
    {
      id: 'audit', title: 'Аудит карточки товара', type: 'Услуга', price: 2500, term: '1 день',
      desc: 'Разбор визуала, оффера и ошибок карточки с рекомендациями.',
      customer: 'Заказчик Анна', creator: 'Alina Cards', step: 'check', guarantee: true,
      offers: [{name:'Alina Cards', price:2500, term:'1 день', text:'Проверю визуал, оффер и конкурентов.'}],
      log: ['Заказ опубликован', 'Работа отправлена на проверку']
    }
  ];

  function read(key, fallback){
    try { return JSON.parse(localStorage.getItem(key) || 'null') || clone(fallback); }
    catch { localStorage.removeItem(key); return clone(fallback); }
  }

  function user(){ return read('rm_user', null); }
  function save(items){ if(localStorage.getItem(PRIVACY_KEY) === 'accepted') localStorage.setItem(KEY, JSON.stringify(items)); }
  function deals(){ return read(KEY, demoDeals); }

  function toast(text){
    const el = $('#toast');
    if(!el) return;
    el.textContent = clean(text, 140);
    el.classList.remove('hidden');
    clearTimeout(toast.t);
    toast.t = setTimeout(() => el.classList.add('hidden'), 2300);
  }

  function needUser(){
    const u = user();
    if(u) return u;
    $('#loginBtn')?.click();
    toast('Сначала войди в demo-аккаунт');
    return null;
  }

  function ensureCss(){
    if($('#deals-css')) return;
    const s = document.createElement('style');
    s.id = 'deals-css';
    s.textContent = `
      .deal-metrics{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:16px 0 22px}.deal-metric{padding:14px;border:1px solid rgba(255,255,255,.1);border-radius:18px;background:rgba(255,255,255,.045)}.deal-metric b{display:block;font-size:22px}.deal-metric span{color:#a995bd;font-size:12px;font-weight:900;text-transform:uppercase}.deal-wrap{display:grid;grid-template-columns:340px 1fr;gap:18px;align-items:start}.deal-list{display:grid;gap:12px}.deal-item{width:100%;text-align:left;padding:15px;border:1px solid rgba(204,65,255,.24);border-radius:20px;background:rgba(255,255,255,.05);color:white}.deal-item.active{background:linear-gradient(135deg,rgba(75,22,255,.36),rgba(255,43,214,.17));box-shadow:0 0 24px rgba(255,43,214,.18)}.deal-item b,.deal-state b{display:block}.deal-item span,.deal-state span{display:block;color:#a995bd;font-size:13px}.deal-box{padding:24px;border-radius:28px}.deal-top{display:flex;flex-wrap:wrap;justify-content:space-between;gap:12px;margin-bottom:14px}.deal-state{padding:11px 14px;border:1px solid rgba(53,255,182,.25);border-radius:18px;background:rgba(53,255,182,.07)}.deal-state b{color:#35ffb6}.offer-list,.deal-log{display:grid;gap:10px;margin:12px 0 18px}.offer-row,.log-row{padding:13px;border:1px solid rgba(255,255,255,.09);border-radius:16px;background:rgba(255,255,255,.045)}.offer-row{display:grid;grid-template-columns:1fr auto;gap:10px}.offer-row p,.log-row{color:#c9b7d8;line-height:1.55}.deal-actions{display:flex;flex-wrap:wrap;gap:10px}.deal-note{margin:12px 0;padding:13px;border:1px solid rgba(255,209,102,.28);border-radius:16px;background:rgba(255,209,102,.06);color:#ffe9b5}@media(max-width:920px){.deal-wrap{grid-template-columns:1fr}.deal-metrics{grid-template-columns:1fr 1fr}}@media(max-width:620px){.deal-metrics,.offer-row{grid-template-columns:1fr}.deal-box{padding:18px;border-radius:22px}}
    `;
    document.head.appendChild(s);
  }

  function ensurePage(){
    ensureCss();
    const nav = $('#nav');
    if(nav && !nav.querySelector('[data-page="deals"]')){
      const a = document.createElement('a');
      a.href = '#deals';
      a.dataset.page = 'deals';
      a.textContent = 'Сделки';
      const after = nav.querySelector('[data-page="creators"]');
      if(after) after.insertAdjacentElement('afterend', a);
      else nav.appendChild(a);
    }
    if(!$('#page-deals')){
      const page = document.createElement('section');
      page.className = 'page hidden';
      page.id = 'page-deals';
      $('main')?.appendChild(page);
    }
  }

  const selected = () => sessionStorage.getItem('rm_active_deal') || deals()[0]?.id || '';
  const setSelected = (id) => sessionStorage.setItem('rm_active_deal', id);

  function activeDealHtml(d){
    const fee = Math.round((Number(d.price) || 0) * 0.1);
    const st = status[d.step] || status.open;
    return `
      <article class="deal-box neon-card">
        <div class="deal-top">
          <div class="deal-state"><b>${esc(st[0])}</b><span>${esc(st[1])}</span></div>
          <div class="deal-state"><b>${d.guarantee ? 'Demo-гарантия включена' : 'Без demo-гарантии'}</b><span>${d.creator ? esc(d.creator) : 'исполнитель не выбран'}</span></div>
        </div>
        <h3>${esc(d.title)}</h3>
        <p class="hero-text">${esc(d.desc)}</p>
        <div class="deal-metrics">
          <div class="deal-metric"><b>${money(d.price)}</b><span>бюджет</span></div>
          <div class="deal-metric"><b>${esc(d.term)}</b><span>срок</span></div>
          <div class="deal-metric"><b>${money(fee)}</b><span>комиссия 10%</span></div>
          <div class="deal-metric"><b>${money((Number(d.price)||0)-fee)}</b><span>исполнителю</span></div>
        </div>
        <div class="deal-note">Это frontend demo для GitHub Pages: показывает механику сделки без сервера и настоящей оплаты.</div>
        <h3>Отклики</h3>
        <div class="offer-list">
          ${(d.offers || []).map((o, i) => `<div class="offer-row"><div><b>${esc(o.name)}</b><p>${esc(o.text)}</p></div><div><b>${money(o.price)}</b><p>${esc(o.term)}</p>${!d.creator ? `<button class="btn btn-soft" data-pick="${i}">Выбрать</button>` : ''}</div></div>`).join('') || '<div class="log-row">Откликов пока нет</div>'}
        </div>
        <h3>История</h3>
        <div class="deal-log">${(d.log || []).map(x => `<div class="log-row">${esc(x)}</div>`).join('')}</div>
        <div class="deal-actions">
          <button class="btn btn-primary" data-action-deal="offer">Откликнуться</button>
          <button class="btn btn-soft" data-action-deal="guarantee">Demo-гарантия</button>
          <button class="btn btn-soft" data-action-deal="work">В работу</button>
          <button class="btn btn-soft" data-action-deal="send">Отправить</button>
          <button class="btn btn-primary" data-action-deal="accept">Принять</button>
          <button class="btn btn-soft" data-action-deal="dispute">Разбор</button>
          <button class="btn btn-soft" data-open-chat>Чат</button>
        </div>
      </article>`;
  }

  function render(){
    ensurePage();
    const items = deals();
    const current = items.find(x => x.id === selected()) || items[0];
    if(current) setSelected(current.id);
    const sum = items.reduce((a,b)=>a+(Number(b.price)||0),0);
    const offers = items.reduce((a,b)=>a+(b.offers?.length || 0),0);
    const active = items.filter(x => !['done','dispute'].includes(x.step)).length;
    $('#page-deals').innerHTML = `
      <div class="section-head"><div><p class="eyebrow">deal workflow</p><h2>Сделки и отклики</h2><p>То, чего не хватало бирже: отклики, выбор исполнителя, demo-гарантия, статусы, проверка и разбор.</p></div><button class="btn btn-primary" data-reset-deals>Сброс demo</button></div>
      <div class="deal-metrics"><div class="deal-metric"><b>${items.length}</b><span>сделок</span></div><div class="deal-metric"><b>${active}</b><span>активных</span></div><div class="deal-metric"><b>${offers}</b><span>откликов</span></div><div class="deal-metric"><b>${money(sum)}</b><span>сумма</span></div></div>
      <div class="deal-wrap"><aside class="deal-list">${items.map(d => `<button class="deal-item ${current && d.id===current.id ? 'active' : ''}" data-deal-id="${esc(d.id)}"><b>${esc(d.title)}</b><span>${esc(status[d.step]?.[0] || 'Открыт')} • ${money(d.price)}</span><span>${esc(d.creator || 'исполнитель не выбран')}</span></button>`).join('')}</aside>${current ? activeDealHtml(current) : '<div class="deal-box neon-card">Сделок пока нет</div>'}</div>`;
    bindActions();
  }

  function mutate(fn){
    const u = needUser();
    if(!u) return;
    const items = deals();
    const d = items.find(x => x.id === selected());
    if(!d) return;
    fn(d, u);
    save(items);
    render();
  }

  function log(d, text){ d.log = d.log || []; d.log.push(text); }

  function bindActions(){
    $$('[data-deal-id]').forEach(b => b.onclick = () => { setSelected(b.dataset.dealId); render(); });
    $$('[data-pick]').forEach(b => b.onclick = () => mutate((d,u) => {
      if(u.role !== 'customer') return toast('Выбирать исполнителя может заказчик');
      const o = d.offers[Number(b.dataset.pick)];
      if(!o) return;
      d.creator = o.name; d.price = o.price; d.term = o.term; d.step = 'picked';
      log(d, `Выбран исполнитель: ${o.name}`); toast('Исполнитель выбран');
    }));
    $$('[data-action-deal]').forEach(b => b.onclick = () => mutate((d,u) => {
      const a = b.dataset.actionDeal;
      if(a === 'offer'){
        if(u.role !== 'creator') return toast('Откликаться может исполнитель');
        if((d.offers || []).some(o => o.name === u.name)) return toast('Ты уже откликнулся');
        d.offers = d.offers || [];
        d.offers.push({name: clean(u.name,40), price: Number(d.price)||0, term:'3 дня', text:'Готов выполнить задачу, детали обсудим в чате.'});
        log(d, `${clean(u.name,40)} оставил отклик`); toast('Отклик добавлен');
      }
      if(a === 'guarantee'){
        if(u.role !== 'customer') return toast('Demo-гарантию включает заказчик');
        if(!d.creator) return toast('Сначала выбери исполнителя');
        d.guarantee = true; d.step = 'hold'; log(d, 'Demo-гарантия включена'); toast('Demo-гарантия включена');
      }
      if(a === 'work'){
        if(!d.guarantee) return toast('Сначала включи demo-гарантию');
        d.step = 'work'; log(d, 'Сделка переведена в работу'); toast('Сделка в работе');
      }
      if(a === 'send'){
        if(u.role !== 'creator') return toast('Отправляет работу исполнитель');
        if(!['hold','work'].includes(d.step)) return toast('Сделка ещё не готова к сдаче');
        d.step = 'check'; log(d, 'Работа отправлена на проверку'); toast('Работа на проверке');
      }
      if(a === 'accept'){
        if(u.role !== 'customer') return toast('Принимает работу заказчик');
        if(d.step !== 'check') return toast('Сначала нужна отправка работы');
        d.step = 'done'; log(d, 'Работа принята, сделка завершена'); toast('Сделка завершена');
      }
      if(a === 'dispute'){
        d.step = 'dispute'; log(d, 'Открыт разбор через поддержку'); toast('Открыт разбор');
      }
    }));
    $('[data-open-chat]')?.addEventListener('click', () => document.querySelector('[data-page="chat"]')?.dispatchEvent(new MouseEvent('click', {bubbles:true, cancelable:true})));
    $('[data-reset-deals]')?.addEventListener('click', () => { localStorage.removeItem(KEY); sessionStorage.removeItem('rm_active_deal'); render(); toast('Demo-сделки сброшены'); });
  }

  function show(){
    ensurePage();
    $$('.page').forEach(p => p.classList.add('hidden'));
    $('#page-deals')?.classList.remove('hidden');
    $$('[data-page]').forEach(a => a.classList.toggle('active', a.dataset.page === 'deals'));
    $('#nav')?.classList.remove('open');
    if(location.hash !== '#deals') history.replaceState(null, '', '#deals');
    render();
  }

  function boot(){
    ensurePage();
    document.querySelector('[data-page="deals"]')?.addEventListener('click', e => { e.preventDefault(); show(); });
    if(location.hash === '#deals') show();
  }

  if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

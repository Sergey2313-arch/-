(() => {
  const style = document.createElement('style');
  style.textContent = `
    .site-stats-grid{display:grid;grid-template-columns:repeat(4,minmax(140px,1fr));gap:14px;margin:18px 0 28px}
    .site-stat-card{padding:18px;border:1px solid rgba(255,43,214,.24);border-radius:22px;background:rgba(255,255,255,.055);box-shadow:0 0 24px rgba(139,44,255,.10)}
    .site-stat-card b{display:block;font-size:30px;margin-bottom:6px;color:#fff}
    .site-stat-card span{color:#a995bd;font-size:12px;font-weight:900;text-transform:uppercase;letter-spacing:.7px}
    .stats-panel{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-top:18px}
    .stats-box{padding:22px;border-radius:26px;border:1px solid rgba(204,65,255,.24);background:linear-gradient(145deg,rgba(36,13,63,.7),rgba(255,255,255,.035));box-shadow:0 0 38px rgba(194,38,255,.18)}
    .stats-box h3{margin:0 0 12px;font-size:26px}.stats-box p{color:#a995bd;line-height:1.65}.stats-row{display:flex;justify-content:space-between;gap:12px;padding:12px 0;border-bottom:1px solid rgba(255,255,255,.08)}
    .stats-row:last-child{border-bottom:0}.stats-row span{color:#a995bd}.stats-row b{color:#fff}.profile-score{font-size:54px;font-weight:1000;line-height:1;color:#35ffb6;text-shadow:0 0 22px rgba(53,255,182,.35)}
    @media(max-width:920px){.site-stats-grid,.stats-panel{grid-template-columns:1fr 1fr}}@media(max-width:620px){.site-stats-grid,.stats-panel{grid-template-columns:1fr}.site-stat-card,.stats-box{border-radius:20px}}
  `;
  document.head.appendChild(style);

  const esc = (value) => String(value ?? '').replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
  const read = (key, fallback) => {
    try { return JSON.parse(localStorage.getItem(key) || 'null') || fallback; }
    catch { return fallback; }
  };

  function ensureStatsPage(){
    const nav = document.querySelector('#nav');
    if(nav && !nav.querySelector('[data-page="stats"]')){
      const link = document.createElement('a');
      link.href = '#stats';
      link.dataset.page = 'stats';
      link.textContent = 'Статистика';
      const profile = nav.querySelector('[data-page="profile"]');
      nav.insertBefore(link, profile || null);
    }

    if(!document.querySelector('#page-stats')){
      const page = document.createElement('section');
      page.className = 'page hidden';
      page.id = 'page-stats';
      document.querySelector('main')?.appendChild(page);
    }
  }

  function getData(){
    const user = read('rm_user', null);
    const orders = read('rm_orders', []);
    const freelance = read('rm_freelance', []);
    const products = read('rm_products', []);
    const chats = read('rm_chats', []);
    const support = read('rm_support', {messages: []});
    const privacy = localStorage.getItem('rm_privacy') === 'accepted' ? 'Принято' : localStorage.getItem('rm_privacy') === 'declined' ? 'Отклонено' : 'Не выбрано';
    return {user, orders, freelance, products, chats, support, privacy};
  }

  function countMessages(chats){
    return chats.reduce((sum, chat) => sum + (chat.messages?.length || 0), 0);
  }

  function renderStats(){
    ensureStatsPage();
    const page = document.querySelector('#page-stats');
    if(!page) return;
    const data = getData();
    const allOrders = [...data.orders, ...data.freelance];
    const allPublications = [...data.orders, ...data.freelance, ...data.products];
    const userName = data.user?.name || 'Гость';
    const role = data.user ? (data.user.role === 'customer' ? 'Заказчик' : 'Исполнитель') : 'Гость';
    const createdByUser = data.user ? allPublications.filter(item => item.meta === data.user.name).length : 0;
    const dealMessages = countMessages(data.chats);
    const userDealMessages = data.user ? data.chats.reduce((sum, chat) => sum + (chat.messages || []).filter(m => m.from === (data.user.role === 'customer' ? 'customer' : 'creator')).length, 0) : 0;
    const supportUserMessages = (data.support.messages || []).filter(m => m.from === 'user').length;
    const supportTotal = (data.support.messages || []).length;
    const activityScore = Math.min(100, createdByUser * 18 + userDealMessages * 6 + supportUserMessages * 10 + (data.user ? 20 : 0));
    const totalMoney = allOrders.reduce((sum, item) => sum + (Number(item.price) || 0), 0);

    page.innerHTML = `
      <div class="section-head">
        <div>
          <p class="eyebrow">platform analytics</p>
          <h2>Статистика ReviMarket</h2>
          <p>Общая статистика всей beta-платформы и конкретного профиля пользователя.</p>
        </div>
      </div>

      <div class="site-stats-grid">
        <div class="site-stat-card"><b>${allOrders.length}</b><span>всего заказов</span></div>
        <div class="site-stat-card"><b>${data.products.length}</b><span>товаров и услуг</span></div>
        <div class="site-stat-card"><b>${data.chats.length}</b><span>диалогов сделок</span></div>
        <div class="site-stat-card"><b>${dealMessages}</b><span>сообщений в сделках</span></div>
        <div class="site-stat-card"><b>${supportTotal}</b><span>сообщений поддержки</span></div>
        <div class="site-stat-card"><b>${data.privacy}</b><span>конфиденциальность</span></div>
        <div class="site-stat-card"><b>${new Intl.NumberFormat('ru-RU').format(totalMoney)} ₽</b><span>сумма заказов</span></div>
        <div class="site-stat-card"><b>Beta</b><span>статус проекта</span></div>
      </div>

      <div class="stats-panel">
        <div class="stats-box">
          <h3>Профиль: ${esc(userName)}</h3>
          <div class="stats-row"><span>Роль</span><b>${esc(role)}</b></div>
          <div class="stats-row"><span>Мои публикации</span><b>${createdByUser}</b></div>
          <div class="stats-row"><span>Мои сообщения в сделках</span><b>${userDealMessages}</b></div>
          <div class="stats-row"><span>Мои обращения в поддержку</span><b>${supportUserMessages}</b></div>
          <div class="stats-row"><span>Статус данных</span><b>${esc(data.privacy)}</b></div>
        </div>
        <div class="stats-box">
          <h3>Активность профиля</h3>
          <div class="profile-score">${activityScore}%</div>
          <p>Оценка считается по демо-активности: вход, публикации, сообщения в сделках и обращения в поддержку.</p>
          <div class="hero-actions">
            <button class="btn btn-primary" data-stats-go="profile">Открыть профиль</button>
            <button class="btn btn-soft" data-stats-go="chat">Открыть чат</button>
            <button class="btn btn-soft" data-stats-go="support">Поддержка</button>
          </div>
        </div>
      </div>
    `;

    page.querySelectorAll('[data-stats-go]').forEach(btn => btn.onclick = () => {
      const target = btn.dataset.statsGo;
      document.querySelector(`[data-page="${target}"]`)?.dispatchEvent(new MouseEvent('click', {bubbles:true, cancelable:true}));
    });
  }

  function showStats(){
    ensureStatsPage();
    document.querySelectorAll('.page').forEach(p => p.classList.add('hidden'));
    document.querySelector('#page-stats')?.classList.remove('hidden');
    document.querySelectorAll('[data-page]').forEach(a => a.classList.toggle('active', a.dataset.page === 'stats'));
    document.querySelector('#nav')?.classList.remove('open');
    location.hash = 'stats';
    renderStats();
  }

  function boot(){
    ensureStatsPage();
    document.querySelector('[data-page="stats"]')?.addEventListener('click', e => {
      e.preventDefault();
      showStats();
    });
    if(location.hash === '#stats') showStats();
  }

  if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

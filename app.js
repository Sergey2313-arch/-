const $ = (s) => document.querySelector(s);
const $$ = (s) => [...document.querySelectorAll(s)];

const PRIVACY_KEY = 'rm_privacy';
const hasAcceptedPrivacy = () => localStorage.getItem(PRIVACY_KEY) === 'accepted';
const hasDeclinedPrivacy = () => localStorage.getItem(PRIVACY_KEY) === 'declined';

const defaults = {
  orders: [
    {title:'Карточки товара для WB/Ozon',desc:'8 карточек в неоновом стиле: преимущества, инфографика, чистая подача.',price:7500,cat:'cards',tags:['WB','Ozon','Figma'],meta:'12 откликов'},
    {title:'Баннер для акции магазина',desc:'Яркий баннер для сайта и VK. Нужен стильный визуал без перегруза.',price:4500,cat:'banner',tags:['Banner','VK','PSD'],meta:'7 откликов'},
    {title:'Оформление Telegram-постов',desc:'6 шаблонов для новостей, акций, отзывов и подборок.',price:6000,cat:'social',tags:['Telegram','SMM','Canva'],meta:'9 откликов'}
  ],
  freelance: [
    {title:'Сделать лендинг услуги',desc:'Главный экран, преимущества, цены, отзывы и форма заявки.',price:15000,cat:'web',tags:['HTML','UI','Landing'],meta:'5 дней'},
    {title:'Логотип для малого бренда',desc:'3 варианта логотипа, цвета и базовая упаковка.',price:6000,cat:'design',tags:['Logo','Brand','Figma'],meta:'3 дня'},
    {title:'Монтаж короткого ролика',desc:'30 секунд для VK/Shorts: динамика, субтитры, музыка.',price:5000,cat:'video',tags:['Video','Shorts','Edit'],meta:'2 дня'},
    {title:'Тексты для карточек товара',desc:'Продающие описания и преимущества для товаров маркетплейса.',price:4000,cat:'content',tags:['Text','WB','Ozon'],meta:'4 дня'}
  ],
  products: [
    {title:'Figma-шаблоны карточек',desc:'Готовые шаблоны для карточек WB/Ozon.',price:1990,cat:'template',tags:['Figma','Templates','WB'],meta:'Nika Visual'},
    {title:'Пак иконок для товаров',desc:'120 иконок для преимуществ и инфографики.',price:990,cat:'digital',tags:['SVG','PNG','Icons'],meta:'Max Neon'},
    {title:'Аудит карточки товара',desc:'Разбор ошибок, визуала и оффера с рекомендациями.',price:2500,cat:'service',tags:['Audit','WB','Ozon'],meta:'Alina Cards'}
  ],
  chats: [
    {
      id:'cards-wb',
      title:'Карточки товара для WB/Ozon',
      customer:'Заказчик Сергей',
      creator:'Nika Visual',
      messages:[
        {from:'customer', text:'Здравствуйте, нужно 8 карточек для товара. Стиль чёрно-фиолетовый, как у ReviMarket.', time:'10:20'},
        {from:'creator', text:'Приняла. Нужны фото товара, характеристики и примеры, которые нравятся.', time:'10:22'},
        {from:'customer', text:'Ок, подготовлю материалы. Срок 3 дня подойдёт?', time:'10:25'}
      ]
    },
    {
      id:'landing',
      title:'Лендинг услуги',
      customer:'Заказчик Марк',
      creator:'Dmitry UI',
      messages:[
        {from:'customer', text:'Нужен лендинг: главный экран, преимущества, цены и форма заявки.', time:'12:05'},
        {from:'creator', text:'Могу сделать beta-макет и адаптив под телефон. Начну с прототипа.', time:'12:11'}
      ]
    },
    {
      id:'audit',
      title:'Аудит карточки товара',
      customer:'Заказчик Анна',
      creator:'Alina Cards',
      messages:[
        {from:'customer', text:'Хочу понять, почему карточка плохо продаёт.', time:'14:40'},
        {from:'creator', text:'Посмотрю визуал, оффер, первый экран и преимущества. Потом дам список правок.', time:'14:44'}
      ]
    }
  ]
};

function readData(key, fallback){
  if (!hasAcceptedPrivacy()) return fallback;
  try { return JSON.parse(localStorage.getItem(key) || 'null') || fallback; }
  catch { localStorage.removeItem(key); return fallback; }
}

function clean(value, limit = 160){
  return String(value || '').replace(/[<>"'`]/g, '').trim().slice(0, limit);
}

function esc(value){
  return String(value || '').replace(/[&<>'"]/g, (char) => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[char]));
}

const state = {
  user: readData('rm_user', null),
  createType: 'order',
  selectedChatId: 'cards-wb',
  orders: readData('rm_orders', defaults.orders),
  freelance: readData('rm_freelance', defaults.freelance),
  products: readData('rm_products', defaults.products),
  chats: readData('rm_chats', defaults.chats),
  creators: [
    {name:'Nika Visual',role:'Дизайнер карточек WB/Ozon',rating:'4.98',orders:142,tags:['WB','Ozon','Figma']},
    {name:'Max Neon',role:'Баннеры и молодежный визуал',rating:'4.91',orders:88,tags:['VK','PSD','Promo']},
    {name:'Alina Cards',role:'Карточки и аудит товаров',rating:'4.87',orders:117,tags:['Audit','Cards','Market']},
    {name:'Dmitry UI',role:'Сайты и интерфейсы',rating:'4.83',orders:64,tags:['HTML','CSS','UI']}
  ]
};

function clearDemoStorage(){
  ['rm_user','rm_orders','rm_freelance','rm_products','rm_chats'].forEach(k => localStorage.removeItem(k));
}

function save(){
  if (!hasAcceptedPrivacy()) return;
  localStorage.setItem('rm_orders', JSON.stringify(state.orders));
  localStorage.setItem('rm_freelance', JSON.stringify(state.freelance));
  localStorage.setItem('rm_products', JSON.stringify(state.products));
  localStorage.setItem('rm_chats', JSON.stringify(state.chats));
  if(state.user) localStorage.setItem('rm_user', JSON.stringify({name:state.user.name,email:state.user.email,role:state.user.role}));
  else localStorage.removeItem('rm_user');
}

function money(n){return new Intl.NumberFormat('ru-RU').format(Math.max(0, Math.min(Number(n) || 0, 10000000)))+' ₽'}
function toast(t){const el=$('#toast');el.textContent=clean(t,120);el.classList.remove('hidden');clearTimeout(toast.t);toast.t=setTimeout(()=>el.classList.add('hidden'),2400)}

function openPrivacy(){ $('#privacyModal')?.classList.remove('hidden'); }
function closePrivacy(){ $('#privacyModal')?.classList.add('hidden'); }
function requirePrivacy(){
  if (hasAcceptedPrivacy()) return true;
  openPrivacy();
  toast(hasDeclinedPrivacy() ? 'Ты отклонил сохранение данных. Нажми Принять, чтобы пользоваться входом.' : 'Сначала выбери настройки конфиденциальности.');
  return false;
}

function acceptPrivacy(){
  localStorage.setItem(PRIVACY_KEY, 'accepted');
  closePrivacy();
  save();
  authUi();
  toast('Конфиденциальность принята');
}

function declinePrivacy(){
  localStorage.setItem(PRIVACY_KEY, 'declined');
  clearDemoStorage();
  state.user = null;
  closePrivacy();
  authUi();
  renderProfile();
  toast('Сохранение демо-данных отключено');
}

function route(page){
  const id = ['home','orders','freelance','shop','creators','chat','profile'].includes(page)?page:'home';
  $$('.page').forEach(p=>p.classList.add('hidden'));
  $('#page-'+id).classList.remove('hidden');
  $$('[data-page]').forEach(a=>a.classList.toggle('active',a.dataset.page===id));
  location.hash=id;
  $('#nav').classList.remove('open');
  if(id==='orders') renderCards('orders');
  if(id==='freelance') renderCards('freelance');
  if(id==='shop') renderCards('products');
  if(id==='creators') renderCreators();
  if(id==='chat') renderChat();
  if(id==='profile') renderProfile();
}

function authUi(){
  const ok=!!state.user;
  $$('.auth-only').forEach(e=>e.classList.toggle('hidden',!ok));
  $$('.guest-only').forEach(e=>e.classList.toggle('hidden',ok));
  $('#countOrders').textContent=state.orders.length+state.freelance.length;
  $('#countProducts').textContent=state.products.length;
}

function card(item,type){
  return `<article class="card"><div class="card-top"><span class="tag">${esc(item.cat)}</span><span class="price">${money(item.price)}</span></div><h3>${esc(item.title)}</h3><p>${esc(item.desc)}</p><div class="chips">${(item.tags||[]).map(t=>`<span>${esc(t)}</span>`).join('')}</div><div class="card-bottom"><span>${esc(item.meta)}</span><button class="btn btn-soft" data-action="${esc(type)}">Открыть</button></div></article>`
}

function renderCards(type){
  const map={orders:['#ordersGrid','#ordersSearch','#ordersFilter'],freelance:['#freelanceGrid','#freelanceSearch','#freelanceFilter'],products:['#productsGrid','#productsSearch','#productsFilter']};
  const [gridId,searchId,filterId]=map[type];
  const q=clean($(searchId).value,80).toLowerCase();
  const f=$(filterId).value;
  let items=[...state[type]];
  if(f!=='all') items=items.filter(i=>i.cat===f);
  if(q) items=items.filter(i=>(i.title+i.desc+(i.tags||[]).join(' ')).toLowerCase().includes(q));
  $(gridId).innerHTML=items.length?items.map(i=>card(i,type)).join(''):'<div class="card">Ничего не найдено</div>';
  $$('[data-action]').forEach(b=>b.onclick=()=>{ if(!state.user){openAuth();toast('Сначала войди')} else {toast('Открываю чат по сделке'); route('chat')} });
}

function renderCreators(){
  $('#creatorsGrid').innerHTML=state.creators.map(c=>`<article class="creator neon-card"><div class="creator-avatar">${esc(c.name[0])}</div><div><h3>${esc(c.name)}</h3><p>${esc(c.role)}</p><div class="chips">${c.tags.map(t=>`<span>${esc(t)}</span>`).join('')}</div></div><div><div class="rating">★ ${esc(c.rating)}</div><p>${esc(c.orders)} заказов</p></div></article>`).join('');
}

function renderChat(){
  const threads = $('#chatThreads');
  const messages = $('#chatMessages');
  if(!threads || !messages) return;
  if(!state.chats.length) state.chats = defaults.chats;
  const active = state.chats.find(c => c.id === state.selectedChatId) || state.chats[0];
  state.selectedChatId = active.id;

  threads.innerHTML = state.chats.map(chat => `
    <button class="chat-thread ${chat.id === active.id ? 'active' : ''}" data-chat-id="${esc(chat.id)}">
      <b>${esc(chat.title)}</b>
      <span>${esc(chat.customer)} ↔ ${esc(chat.creator)}</span>
      <span>${esc(chat.messages.at(-1)?.text || 'Нет сообщений')}</span>
    </button>
  `).join('');

  $('#chatTitle').textContent = active.title;
  $('#chatSubtitle').textContent = `${active.customer} ↔ ${active.creator}`;
  messages.innerHTML = active.messages.map(msg => {
    const isMe = state.user && ((state.user.role === 'customer' && msg.from === 'customer') || (state.user.role === 'creator' && msg.from === 'creator'));
    const name = msg.from === 'customer' ? 'Заказчик' : 'Исполнитель';
    return `<div class="message ${isMe ? 'me' : 'other'}"><b>${name}</b><p>${esc(msg.text)}</p><small>${esc(msg.time)}</small></div>`;
  }).join('');
  messages.scrollTop = messages.scrollHeight;

  $$('[data-chat-id]').forEach(btn => btn.onclick = () => {
    state.selectedChatId = btn.dataset.chatId;
    renderChat();
  });
}

function sendChatMessage(text){
  if(!requirePrivacy()) return;
  if(!state.user){openAuth();toast('Сначала войди, чтобы писать в чат');return;}
  const active = state.chats.find(c => c.id === state.selectedChatId) || state.chats[0];
  active.messages.push({
    from: state.user.role === 'customer' ? 'customer' : 'creator',
    text: clean(text,240),
    time: new Date().toLocaleTimeString('ru-RU', {hour:'2-digit', minute:'2-digit'})
  });
  save();
  renderChat();
}

function renderProfile(){
  if(!state.user){$('#avatar').textContent='RM';$('#profileName').textContent='Гость';$('#profileInfo').textContent=hasAcceptedPrivacy()?'Войди в beta-режим, чтобы создавать заказы и товары.':'Сначала прими конфиденциальность для демо-входа.';$('#profileActions').innerHTML='<button class="btn btn-primary" id="profileLogin">Войти</button><button class="btn btn-soft" id="profilePrivacy">Конфиденциальность</button>';$('#profileLogin').onclick=openAuth;$('#profilePrivacy').onclick=openPrivacy;return}
  $('#avatar').textContent=clean(state.user.name,1).toUpperCase() || 'U';
  $('#profileName').textContent=clean(state.user.name,40);
  $('#profileInfo').textContent=(state.user.role==='customer'?'Заказчик':'Исполнитель')+' • данные скрыты в beta';
  $('#profileActions').innerHTML='<button class="btn btn-primary" data-create="order">Создать заказ</button><button class="btn btn-soft" data-create="freelance">Фриланс-заказ</button><button class="btn btn-soft" data-create="product">Добавить товар</button><button class="btn btn-soft" data-go="chat">Открыть чат</button>';
  bindCreate();
  $$('[data-go]').forEach(b=>b.onclick=()=>route(b.dataset.go));
}

function openAuth(){ if(!requirePrivacy()) return; $('#authModal').classList.remove('hidden') }
function closeModals(){ $$('.modal').forEach(m=>m.classList.add('hidden')) }
function openCreate(type){
  if(!requirePrivacy()) return;
  if(!state.user){openAuth();toast('Сначала авторизуйся');return}
  state.createType=type;
  const cfg={
    order:['design order','Создать дизайн-заказ',[['cards','Карточки'],['banner','Баннеры'],['social','Соцсети']]],
    freelance:['freelance task','Создать фриланс-заказ',[['web','Сайты'],['design','Дизайн'],['content','Контент'],['video','Видео']]],
    product:['product','Добавить товар',[['template','Шаблоны'],['digital','Цифровое'],['service','Услуги']]]
  }[type];
  $('#createType').textContent=cfg[0];$('#createTitle').textContent=cfg[1];
  $('#itemCategory').innerHTML=cfg[2].map(x=>`<option value="${x[0]}">${x[1]}</option>`).join('');
  $('#createModal').classList.remove('hidden');
}
function bindCreate(){ $$('[data-create]').forEach(b=>b.onclick=()=>openCreate(b.dataset.create)) }

function init(){
  authUi();renderCards('orders');renderCards('freelance');renderCards('products');renderCreators();renderProfile();renderChat();
  $$('[data-page]').forEach(a=>a.onclick=e=>{e.preventDefault();route(a.dataset.page)});
  $$('[data-go]').forEach(b=>b.onclick=()=>route(b.dataset.go));
  $('#menuBtn').onclick=()=>$('#nav').classList.toggle('open');
  $('#loginBtn').onclick=openAuth;
  $('#logoutBtn').onclick=()=>{state.user=null;save();authUi();renderProfile();toast('Ты вышел');route('home')};
  $$('[data-close]').forEach(e=>e.onclick=closeModals);
  $('#acceptPrivacyBtn').onclick=acceptPrivacy;
  $('#declinePrivacyBtn').onclick=declinePrivacy;
  $('#chatForm').onsubmit=e=>{e.preventDefault();const input=$('#chatInput');const text=clean(input.value,240);if(!text){toast('Сообщение пустое');return;}sendChatMessage(text);input.value='';};
  $('#authForm').onsubmit=e=>{e.preventDefault();if(!requirePrivacy()) return;state.user={name:clean($('#nameInput').value,40),email:clean($('#emailInput').value,80),role:$('#roleInput').value};save();authUi();closeModals();renderProfile();toast('Добро пожаловать, '+state.user.name);route('profile')};
  $('#createForm').onsubmit=e=>{e.preventDefault();if(!requirePrivacy()) return;const item={title:clean($('#itemTitle').value,80),desc:clean($('#itemDesc').value,300),price:Math.max(0, Math.min(Number($('#itemPrice').value)||0, 10000000)),cat:$('#itemCategory').value,tags:['New','Beta'],meta:clean(state.user.name,40)};if(state.createType==='order'){state.orders.unshift(item);route('orders')}if(state.createType==='freelance'){state.freelance.unshift(item);route('freelance')}if(state.createType==='product'){state.products.unshift(item);route('shop')}save();authUi();closeModals();$('#createForm').reset();toast('Опубликовано')};
  ['orders','freelance','products'].forEach(type=>{const map={orders:['#ordersSearch','#ordersFilter'],freelance:['#freelanceSearch','#freelanceFilter'],products:['#productsSearch','#productsFilter']}[type];map.forEach(id=>$(id).oninput=()=>renderCards(type));});
  route(location.hash.replace('#','')||'home');bindCreate();
  if(!localStorage.getItem(PRIVACY_KEY)) openPrivacy();
}
init();

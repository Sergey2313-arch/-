const $ = (s) => document.querySelector(s);
const $$ = (s) => [...document.querySelectorAll(s)];

const state = {
  user: JSON.parse(localStorage.getItem('rm_user') || 'null'),
  createType: 'order',
  orders: JSON.parse(localStorage.getItem('rm_orders') || 'null') || [
    {title:'Карточки товара для WB/Ozon',desc:'8 карточек в неоновом стиле: преимущества, инфографика, чистая подача.',price:7500,cat:'cards',tags:['WB','Ozon','Figma'],meta:'12 откликов'},
    {title:'Баннер для акции магазина',desc:'Яркий баннер для сайта и VK. Нужен стильный визуал без перегруза.',price:4500,cat:'banner',tags:['Banner','VK','PSD'],meta:'7 откликов'},
    {title:'Оформление Telegram-постов',desc:'6 шаблонов для новостей, акций, отзывов и подборок.',price:6000,cat:'social',tags:['Telegram','SMM','Canva'],meta:'9 откликов'}
  ],
  freelance: JSON.parse(localStorage.getItem('rm_freelance') || 'null') || [
    {title:'Сделать лендинг услуги',desc:'Главный экран, преимущества, цены, отзывы и форма заявки.',price:15000,cat:'web',tags:['HTML','UI','Landing'],meta:'5 дней'},
    {title:'Логотип для малого бренда',desc:'3 варианта логотипа, цвета и базовая упаковка.',price:6000,cat:'design',tags:['Logo','Brand','Figma'],meta:'3 дня'},
    {title:'Монтаж короткого ролика',desc:'30 секунд для VK/Shorts: динамика, субтитры, музыка.',price:5000,cat:'video',tags:['Video','Shorts','Edit'],meta:'2 дня'},
    {title:'Тексты для карточек товара',desc:'Продающие описания и преимущества для товаров маркетплейса.',price:4000,cat:'content',tags:['Text','WB','Ozon'],meta:'4 дня'}
  ],
  products: JSON.parse(localStorage.getItem('rm_products') || 'null') || [
    {title:'Figma-шаблоны карточек',desc:'Готовые шаблоны для карточек WB/Ozon.',price:1990,cat:'template',tags:['Figma','Templates','WB'],meta:'Nika Visual'},
    {title:'Пак иконок для товаров',desc:'120 иконок для преимуществ и инфографики.',price:990,cat:'digital',tags:['SVG','PNG','Icons'],meta:'Max Neon'},
    {title:'Аудит карточки товара',desc:'Разбор ошибок, визуала и оффера с рекомендациями.',price:2500,cat:'service',tags:['Audit','WB','Ozon'],meta:'Alina Cards'}
  ],
  creators: [
    {name:'Nika Visual',role:'Дизайнер карточек WB/Ozon',rating:'4.98',orders:142,tags:['WB','Ozon','Figma']},
    {name:'Max Neon',role:'Баннеры и молодежный визуал',rating:'4.91',orders:88,tags:['VK','PSD','Promo']},
    {name:'Alina Cards',role:'Карточки и аудит товаров',rating:'4.87',orders:117,tags:['Audit','Cards','Market']},
    {name:'Dmitry UI',role:'Сайты и интерфейсы',rating:'4.83',orders:64,tags:['HTML','CSS','UI']}
  ]
};

function money(n){return new Intl.NumberFormat('ru-RU').format(n)+' ₽'}
function save(){localStorage.setItem('rm_orders',JSON.stringify(state.orders));localStorage.setItem('rm_freelance',JSON.stringify(state.freelance));localStorage.setItem('rm_products',JSON.stringify(state.products));if(state.user)localStorage.setItem('rm_user',JSON.stringify(state.user));else localStorage.removeItem('rm_user')}
function toast(t){const el=$('#toast');el.textContent=t;el.classList.remove('hidden');clearTimeout(toast.t);toast.t=setTimeout(()=>el.classList.add('hidden'),2400)}

function route(page){
  const id = ['home','orders','freelance','shop','creators','profile'].includes(page)?page:'home';
  $$('.page').forEach(p=>p.classList.add('hidden'));
  $('#page-'+id).classList.remove('hidden');
  $$('[data-page]').forEach(a=>a.classList.toggle('active',a.dataset.page===id));
  location.hash=id;
  $('#nav').classList.remove('open');
  if(id==='orders') renderCards('orders');
  if(id==='freelance') renderCards('freelance');
  if(id==='shop') renderCards('products');
  if(id==='creators') renderCreators();
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
  return `<article class="card"><div class="card-top"><span class="tag">${item.cat}</span><span class="price">${money(item.price)}</span></div><h3>${item.title}</h3><p>${item.desc}</p><div class="chips">${item.tags.map(t=>`<span>${t}</span>`).join('')}</div><div class="card-bottom"><span>${item.meta}</span><button class="btn btn-soft" data-action="${type}">Открыть</button></div></article>`
}

function renderCards(type){
  const map={orders:['#ordersGrid','#ordersSearch','#ordersFilter'],freelance:['#freelanceGrid','#freelanceSearch','#freelanceFilter'],products:['#productsGrid','#productsSearch','#productsFilter']};
  const [gridId,searchId,filterId]=map[type];
  const q=$(searchId).value.toLowerCase().trim();
  const f=$(filterId).value;
  let items=[...state[type]];
  if(f!=='all') items=items.filter(i=>i.cat===f);
  if(q) items=items.filter(i=>(i.title+i.desc+i.tags.join(' ')).toLowerCase().includes(q));
  $(gridId).innerHTML=items.length?items.map(i=>card(i,type)).join(''):'<div class="card">Ничего не найдено</div>';
  $$('[data-action]').forEach(b=>b.onclick=()=>{ if(!state.user){openAuth();toast('Сначала войди')} else toast('Демо-действие выполнено') });
}

function renderCreators(){
  $('#creatorsGrid').innerHTML=state.creators.map(c=>`<article class="creator neon-card"><div class="creator-avatar">${c.name[0]}</div><div><h3>${c.name}</h3><p>${c.role}</p><div class="chips">${c.tags.map(t=>`<span>${t}</span>`).join('')}</div></div><div><div class="rating">★ ${c.rating}</div><p>${c.orders} заказов</p></div></article>`).join('');
}

function renderProfile(){
  if(!state.user){$('#avatar').textContent='RM';$('#profileName').textContent='Гость';$('#profileInfo').textContent='Войди в beta-режим, чтобы создавать заказы и товары.';$('#profileActions').innerHTML='<button class="btn btn-primary" id="profileLogin">Войти</button>';$('#profileLogin').onclick=openAuth;return}
  $('#avatar').textContent=state.user.name.slice(0,1).toUpperCase();
  $('#profileName').textContent=state.user.name;
  $('#profileInfo').textContent=(state.user.role==='customer'?'Заказчик':'Исполнитель')+' • '+state.user.email;
  $('#profileActions').innerHTML='<button class="btn btn-primary" data-create="order">Создать заказ</button><button class="btn btn-soft" data-create="freelance">Фриланс-заказ</button><button class="btn btn-soft" data-create="product">Добавить товар</button>';
  bindCreate();
}

function openAuth(){ $('#authModal').classList.remove('hidden') }
function closeModals(){ $$('.modal').forEach(m=>m.classList.add('hidden')) }
function openCreate(type){
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
  authUi();renderCards('orders');renderCards('freelance');renderCards('products');renderCreators();renderProfile();
  $$('[data-page]').forEach(a=>a.onclick=e=>{e.preventDefault();route(a.dataset.page)});
  $$('[data-go]').forEach(b=>b.onclick=()=>route(b.dataset.go));
  $('#menuBtn').onclick=()=>$('#nav').classList.toggle('open');
  $('#loginBtn').onclick=openAuth;$('#logoutBtn').onclick=()=>{state.user=null;save();authUi();renderProfile();toast('Ты вышел');route('home')};
  $$('[data-close]').forEach(e=>e.onclick=closeModals);
  $('#authForm').onsubmit=e=>{e.preventDefault();state.user={name:$('#nameInput').value.trim(),email:$('#emailInput').value.trim(),role:$('#roleInput').value};save();authUi();closeModals();renderProfile();toast('Добро пожаловать, '+state.user.name);route('profile')};
  $('#createForm').onsubmit=e=>{e.preventDefault();const item={title:$('#itemTitle').value.trim(),desc:$('#itemDesc').value.trim(),price:+$('#itemPrice').value,cat:$('#itemCategory').value,tags:['New','Beta'],meta:state.user.name};if(state.createType==='order'){state.orders.unshift(item);route('orders')}if(state.createType==='freelance'){state.freelance.unshift(item);route('freelance')}if(state.createType==='product'){state.products.unshift(item);route('shop')}save();authUi();closeModals();$('#createForm').reset();toast('Опубликовано')};
  ['orders','freelance','products'].forEach(type=>{const map={orders:['#ordersSearch','#ordersFilter'],freelance:['#freelanceSearch','#freelanceFilter'],products:['#productsSearch','#productsFilter']}[type];map.forEach(id=>$(id).oninput=()=>renderCards(type));});
  route(location.hash.replace('#','')||'home');bindCreate();
}
init();

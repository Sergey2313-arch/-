(() => {
  const replacements = [
    ['black / purple neon beta', 'black / purple neon global'],
    ['Beta', 'Global'],
    ['beta', 'global'],
    ['demo login', 'global login'],
    ['demo-чат', 'чат'],
    ['demo-данные', 'локальные данные'],
    ['Демо-данные', 'Локальные данные'],
    ['demo-заказ', 'заказ'],
    ['Демо-заказ', 'Заказ'],
    ['demo-предложение', 'предложение'],
    ['demo-предложения', 'предложения'],
    ['demo-стоимость', 'стоимость'],
    ['Demo-сделки', 'Сделки'],
    ['demo-сделки', 'сделки'],
    ['demo', 'global'],
    ['Demo', 'Global'],
    ['beta-режим', 'режим платформы'],
    ['beta-версия', 'frontend-версия'],
    ['beta версии', 'frontend версии'],
    ['Вход в ReviMarket', 'Вход в ReviMarket Global'],
    ['Войди в Global-режим', 'Войди в ReviMarket Global'],
    ['Войди в global-режим', 'Войди в ReviMarket Global'],
    ['Войди в режим платформы', 'Войди в ReviMarket Global']
  ];

  function replaceString(value) {
    let next = String(value || '');
    replacements.forEach(([from, to]) => {
      next = next.split(from).join(to);
    });
    return next;
  }

  function replaceText(node) {
    if (!node || node.nodeType !== Node.TEXT_NODE) return;
    const next = replaceString(node.nodeValue);
    if (next !== node.nodeValue) node.nodeValue = next;
  }

  function walk(root = document.body) {
    if (!root) return;
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach(replaceText);
  }

  function polishGlobal() {
    walk();
    document.title = 'ReviMarket Global — freelance, design, goods';
    const meta = document.querySelector('meta[name="description"]');
    if (meta) meta.content = 'ReviMarket Global — фриланс-платформа, заказы для дизайнеров и маркетплейс цифровых услуг.';

    document.querySelectorAll('[placeholder]').forEach((item) => {
      item.setAttribute('placeholder', replaceString(item.getAttribute('placeholder')));
    });

    document.querySelectorAll('.eyebrow').forEach((item) => {
      item.textContent = replaceString(item.textContent);
    });

    const statusMetric = [...document.querySelectorAll('.metrics b')].find((item) => ['global', 'beta'].includes(item.textContent.trim().toLowerCase()));
    if (statusMetric) statusMetric.textContent = 'Global';
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', polishGlobal);
  } else {
    polishGlobal();
  }

  setTimeout(polishGlobal, 250);
  setTimeout(polishGlobal, 1000);
  setInterval(polishGlobal, 1600);
})();

(() => {
  const replacements = [
    ['black / purple neon beta', 'black / purple neon global'],
    ['Beta', 'Global'],
    ['beta', 'global'],
    ['demo login', 'global login'],
    ['beta-режим', 'режим платформы'],
    ['beta-версия', 'frontend-версия'],
    ['beta версии', 'frontend версии'],
    ['beta', 'global']
  ];

  function replaceText(node) {
    if (!node || node.nodeType !== Node.TEXT_NODE) return;
    let text = node.nodeValue;
    let next = text;
    replacements.forEach(([from, to]) => {
      next = next.split(from).join(to);
    });
    if (next !== text) node.nodeValue = next;
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
    if (meta) meta.content = 'ReviMarket Global — фриланс-биржа, заказы для дизайнеров и маркет товаров.';

    document.querySelectorAll('.eyebrow').forEach((item) => {
      item.textContent = item.textContent.replace('beta', 'global').replace('Beta', 'Global');
    });

    document.querySelectorAll('#countOrders, #countProducts').forEach((item) => {
      if (!item.textContent.trim()) item.textContent = '0';
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', polishGlobal);
  } else {
    polishGlobal();
  }

  setInterval(polishGlobal, 900);
})();
